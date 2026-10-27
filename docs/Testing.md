# Validation record

## Console regression suite

The dependency-light console runner in `Tests` compiles the same production sources for geometry, animation, event handling, configuration, enemy behaviour, scoring, persistence and screen transitions. Run it with `dotnet run --project Tests/SideScrollerShooter.Tests.csproj -c Release`. Its exit code is nonzero if a test fails.

Seventeen regression tests passed on this Windows host:

1. Centred AABB of a quarter-turned non-square sprite.
2. Whole-sprite clamping on all four playfield edges.
3. Equivalent velocity displacement at 30/60/120 Hz.
4. Multi-frame animation catch-up and loop.
5. Equal animation phase at different update rates.
6. Idempotent unsubscribe and no later callback.
7. Safe unsubscribe during dispatch.
8. Score listener with identity-based duplicate protection.
9. One hundred disposable score sessions with no leaked listeners.
10. Real configuration positions, sectors and enemy types.
11. Rejection of zero spawn interval and unknown enemy type.
12. Enemy Enter/Patrol/Charge/Retreat transitions and aimed charge.
13. Weaver vertical motion and straight scout charge.
14. Named top-ten JSON sorting, truncation and reload.
15. Legacy integer import and nondestructive corrupt-file loading.
16. Explicit failure on a blocked save destination.
17. Deferred screen transition ordering and shutdown cleanup.

The first restricted test run could not save to its temporary directory. Repeating with filesystem access passed the persistence test. This was an execution permission issue; the failure was not concealed by changing the expected result. The test also deliberately uses a directory as a save destination to prove errors are surfaced.

## Graphics integration test

`--smoke-test` initializes real MonoGame graphics and original content. Before playing the scripted campaign, it asserts that actual entities produce these outcomes:

- Detection leaves score and lifetime untouched.
- Two bullets overlapping two enemies yield exactly two kills and 200 points, even when the same contact batch is applied twice.
- Enemies already destroyed by bullets do not damage the player.
- Inactive entities are removed after responses.
- Repeated immediate hits respect invulnerability.
- Rapid-fire, shield and repair pickups apply through collision events.
- Actual Player.Update displacement agrees at 30/60/120 Hz.
- The player's actual rotated bounds remain inside every playfield edge.
- All integration-test subscriptions are disposed.

The scripted run then exercises menu, gameplay, F3 overlay, pause/resume, abandonment and restart, all three sectors, victory, save and dedicated high-score table. It checks zero gameplay listeners after exit and completion. It uses a continuously refreshed shield and synthetic keyboard events, with a fixed simulated delta and uncapped drawing to finish quickly. It cannot establish normal difficulty, keyboard ergonomics or human completion rates.

Screenshots are actual GPU captures. Selected images are copied into `docs/screenshots` after visual inspection for clipping, orientation, legible text and bounds alignment. The smoke test saves its own table beneath its executable directory and does not use the normal pilot records.

## Build and outstanding manual checks

Release builds succeeded with no C# errors. NuGet reported NU1900 because its vulnerability service could not be reached in this environment; the warning is retained rather than disabled. Restore with a working connection before distribution.

Human campaign balance, audio listening/mute verification on speakers, different Windows hardware, extended sessions and other operating systems are not fully evaluated. None is represented as a passed test. Extremely fast objects still need swept collision if configuration ranges are expanded.
