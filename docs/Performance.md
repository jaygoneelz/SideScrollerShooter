# Performance and hardware study

## Collision-loop allocation

Copying the enemy list for every bullet with `ToList()` allocates B enemy-reference arrays in addition to the outer list snapshot, while still performing B x E overlap checks. The benchmark compares that allocation-heavy approach with direct traversal.

The game detector iterates read-only lists directly. It records contacts, applies responses separately, then removes inactive entities. That ordering makes snapshots unnecessary: no detection loop mutates the collection it is traversing. A reusable contact list retains its capacity between frames. Dispatch still snapshots event handlers for safe unsubscription; this is a deliberate smaller allocation that has not been optimized away.

## Reproducible experiment

Run:

```powershell
dotnet run --project Tests/SideScrollerShooter.Tests.csproj -c Release -- --benchmark
```

The benchmark creates 80 bullet rectangles and 40 enemy rectangles using seed 379, warms each algorithm for 1,000 calls, then runs 20,000 simulated frames of each. Both algorithms perform the same pairwise overlap test and return the same 380,000 accumulated hits. The only intentional difference is the list-copy pattern. Timing uses `Stopwatch`; allocation uses `GC.GetAllocatedBytesForCurrentThread`.

| Version | Total CPU elapsed | Allocated on benchmark thread | Hits |
| --- | ---: | ---: | ---: |
| Snapshot list traversal | 286.12 ms | 1,140,320,040 bytes | 380,000 |
| Direct list traversal | 88.74 ms | 40 bytes | 380,000 |

Observed on this Windows x64 host on 27 October 2026, Release build, .NET 8 target running with the installed .NET runtime. These are one experiment's results, not an average over multiple machines. Stopwatch creation accounts for the small direct-loop allocation. This benchmark isolates rectangle traversal and copies; it does not run full entity behaviour, GPU drawing, contact buffering or audio. Therefore its ratio must not be reported as whole-game acceleration or an FPS guarantee. Subsequent measurements can vary with JIT, background activity and hardware.

## Why this relates to hardware

The snapshot version allocates and writes many temporary arrays of object references. That consumes memory bandwidth and later creates work for the garbage collector. Removing those copies avoids unnecessary memory traffic and allocation work while preserving exactly the same brute-force algorithm. A C# list provides a contiguous array of references; the entities themselves still occupy separate managed objects. This change does not magically convert them into a cache-local structure-of-arrays layout.

An axis-aligned rectangle test is inexpensive; at the intended modest enemy count, avoiding repeated collection copies is a proportionate first optimization. The game caps active enemies at 40. A 60-Hz target permits about 16.67 ms per simulation interval, but recent CPU timings alone do not establish the full CPU/GPU frame budget on another machine.

## In-game profiling

Press F3 for update milliseconds, collision milliseconds, pair count, entity counts, listener count and collision bounds. `PlayState` timestamps the update and collision subsection using `Stopwatch.GetTimestamp` / `GetElapsedTime`. These are recent CPU timings that include detector and response work, not GPU timers or percentile statistics. The overlay makes it possible to reproduce a dense scene before choosing a further optimization.

For a fuller experiment, profile a Release build on the same hardware and world configuration; capture multiple representative runs, report median and 95th-percentile frame time, CPU utilization, allocated bytes and garbage collections. Compare identical seeds and verify identical gameplay outcomes. Record the profiler overhead and whether the graphics test or normal interactive mode was used.

## Further work, explicitly not implemented

| Option | When it could help | Cost / correctness condition |
| --- | --- | --- |
| Uniform spatial grid | Pair count becomes the measured bottleneck | Assign an entity to every touched cell; deduplicate pairs; compare all detected hits with brute force |
| Bullet pool | Allocation traces show short-lived bullets dominating GC | Reset position, lifetime and identity correctly; never reuse an active bullet |
| Structure-of-arrays storage | Large entity populations make object access expensive | More complex ownership and rendering integration; profile cache behaviour before claiming gains |
| Texture atlas / batching study | GPU captures show texture state changes limiting throughput | Preserve transparency/layer order; measure actual draw calls and GPU time |
| Streaming soundtrack | Music duration makes resident PCM too large | Maintain loop boundaries and platform codec availability |

Collision detection and movement should not simply run concurrently over the same mutable positions. That would introduce inconsistent contacts and races. Any future parallel version would need a stable simulation snapshot or partitioned ownership and a synchronization point before applying responses. No unmeasured percentage speedups, cache-miss reductions or nearly linear multicore gains are asserted.

Networking is not implemented. A future score service would require server-side validation and a privacy decision; local leaderboard files are editable and are not an anti-cheat system.

## References

- [Microsoft Stopwatch reference](https://learn.microsoft.com/en-us/dotnet/api/system.diagnostics.stopwatch): elapsed-time measurements.
- [MonoGame timestep guide](https://docs.monogame.net/articles/getting_to_know/howto/HowTo_VariableStepGameLoop.html): simulation timing.
- The experiment source is `Tests/Program.cs`; the actual game detector is `Code/Game/Managers/CollisionManager.cs`. Proposed optimizations above are separated from measured and implemented changes.
