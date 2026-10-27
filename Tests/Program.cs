using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SideScrollerShooter.Engine;
using SideScrollerShooter.Game;
using SideScrollerShooter.Game.Entities;
using SideScrollerShooter.Game.Managers;
using SideScrollerShooter.Game.States;

int passed=0,failed=0;
void Check(bool condition,string message) { if(!condition) throw new Exception(message); }
void Near(float a,float b,float tolerance=0.01f)=>Check(Math.Abs(a-b)<tolerance,$"{a} != {b}");
void Test(string name,Action run)
{
    try { run(); Console.WriteLine("PASS "+name); passed++; }
    catch(Exception e) { Console.WriteLine("FAIL "+name+": "+e.Message); failed++; }
}
var worldPath=Path.Combine(AppContext.BaseDirectory,"Config","world.json");
var testRoot=Path.Combine(AppContext.BaseDirectory,"test-output","run-"+Guid.NewGuid());
Directory.CreateDirectory(testRoot);
Test("rotated non-square sprite bounds share centred drawing coordinates",()=>
{
    var half=Geometry.Extents(100,40,MathHelper.PiOver2);
    Near(half.X,20); Near(half.Y,50);
    var rect=Geometry.Bounds(new Vector2(200,300),half);
    Check(rect.Contains(200,300),"centre outside bounds");
    Check(rect.Left>=179 && rect.Right<=221 && rect.Top>=249 && rect.Bottom<=351,"rotation not reflected in AABB");
});
Test("player clamps entire rotated sprite at all four playfield edges",()=>
{
    var half=Geometry.Extents(100,40,MathHelper.PiOver2,0.65f);
    var area=new Rectangle(0,100,1120,620);
    foreach(var p in new[]{new Vector2(-999,-999),new Vector2(9999,9999)})
    {
        var bounds=Geometry.Bounds(Geometry.Clamp(p,half,area),half);
        Check(bounds.Left>=area.Left && bounds.Top>=area.Top && bounds.Right<=area.Right && bounds.Bottom<=area.Bottom,"clipping at boundary");
    }
});
Test("same velocity displacement at 30, 60 and 120 updates per second",()=>
{
    foreach(int hz in new[]{30,60,120})
    {
        var p=Vector2.Zero; for(int i=0;i<hz*2;i++) p+=new Vector2(300,0)*(1f/hz);
        Near(p.X,600,0.05f);
    }
});
Test("animation advances across multiple frames during one long update",()=>
{
    var a=new SpriteAnimator(4,10); a.Update(0.35f); Check(a.Frame==3,"animation skipped catchup");
    a.Update(0.1f); Check(a.Frame==0,"animation failed to loop");
});
Test("animation has same phase at different frame rates",()=>
{
    var frames=new List<int>();
    foreach(int hz in new[]{30,60,120}) { var a=new SpriteAnimator(4,10); for(int i=0;i<hz;i++) a.Update(1f/hz); frames.Add(a.Frame); }
    Check(frames.All(f=>f==frames[0]),"different animation phase");
});
Test("disposed subscriptions are idempotent and do not fire",()=>
{
    int before=EventManager.SubscriptionCount,calls=0;
    var sub=EventManager.Subscribe("test",_=>calls++);
    EventManager.Publish("test"); sub.Dispose(); sub.Dispose(); EventManager.Publish("test");
    Check(calls==1 && EventManager.SubscriptionCount==before,"subscription leak");
});
Test("unsubscribe during event dispatch is safe",()=>
{
    IDisposable sub=null; int calls=0;
    sub=EventManager.Subscribe("self",_=>{ calls++; sub.Dispose(); });
    EventManager.Publish("self"); EventManager.Publish("self"); Check(calls==1,"reentrant unsubscribe failed");
});
Test("score listens to destruction events and deduplicates entity identities",()=>
{
    using var score=new ScoreManager(); var id=Guid.NewGuid();
    EventManager.Publish(Events.EnemyDestroyed,new EnemyDestroyed(id,100));
    EventManager.Publish(Events.EnemyDestroyed,new EnemyDestroyed(id,100));
    EventManager.Publish(Events.EnemyDestroyed,new EnemyDestroyed(Guid.NewGuid(),200));
    Check(score.Score==300 && score.Kills==2,"duplicate or missing award");
});
Test("100 session score managers leave no listeners behind",()=>
{
    int before=EventManager.SubscriptionCount;
    for(int i=0;i<100;i++) { using var score=new ScoreManager(); EventManager.Publish(Events.EnemyDestroyed,new EnemyDestroyed(Guid.NewGuid(),100)); Check(score.Score==100,"old session handled score"); }
    Check(EventManager.SubscriptionCount==before,"session listener leak");
});
Test("configuration loads positions, enemy patterns and all three sectors",()=>
{
    var config=WorldConfig.Load(worldPath);
    Check(config.Levels.Count==3 && config.Levels.All(l=>l.InitialEnemies.Count>0),"missing data-driven levels");
    Near(config.PlayerX,130); Check(config.Levels.SelectMany(l=>l.Pattern).Distinct().Count()==3,"missing enemy variety");
});
Test("configuration rejects invalid spawn rates and unknown enemy types",()=>
{
    var config=WorldConfig.Load(worldPath); config.Levels[0].SpawnInterval=0;
    bool rejected=false; try { config.Validate(); } catch(InvalidDataException) { rejected=true; } Check(rejected,"zero interval accepted");
    config=WorldConfig.Load(worldPath); config.Levels[0].Pattern.Add("typo"); rejected=false;
    try { config.Validate(); } catch(InvalidDataException) { rejected=true; } Check(rejected,"unknown type accepted");
});
Test("enemy state objects transition Enter / Patrol / Charge / Retreat",()=>
{
    var brain=new EnemyBrain("hunter",100,new Vector2(1000,300)); var target=new Vector2(100,500);
    Check(brain.StateName=="Enter","wrong initial state");
    brain.Update(0.51f,target); Check(brain.StateName=="Patrol","no patrol transition");
    brain.Update(1.21f,target); Check(brain.StateName=="Charge","no charge transition");
    Check(brain.Velocity.X<0 && brain.Velocity.Y>0,"charge does not aim toward target");
    brain.Update(1.01f,target); Check(brain.StateName=="Retreat","no retreat transition");
});
Test("weaver patrol has vertical movement and scout charge stays straight",()=>
{
    var weaver=new EnemyBrain("weaver",100,new Vector2(1000,300));
    weaver.Update(0.51f,Vector2.Zero); weaver.Update(0.1f,Vector2.Zero); Check(weaver.Velocity.Y!=0,"weaver does not weave");
    var scout=new EnemyBrain("scout",100,new Vector2(1000,300));
    scout.Update(0.51f,Vector2.Zero); scout.Update(2.6f,Vector2.Zero); Near(scout.Velocity.Y,0);
});
Test("table serializes a named top ten, orders scores and reloads",()=>
{
    string path=Path.Combine(testRoot,"table.json"); var table=new HighScoreManager(path);
    for(int i=0;i<15;i++) Check(table.Add("pilot"+i,i*100,1),"save failed");
    var loaded=new HighScoreManager(path);
    Check(loaded.Entries.Count==10 && loaded.Entries[0].Score==1400 && loaded.Entries[9].Score==500,"wrong table order");
    Check(loaded.Entries[0].Name=="PILOT14" && loaded.Entries[0].RecordedAt!=default,"missing serialization fields");
    Check(!File.Exists(path+".tmp"),"temporary file was not replaced");
});
Test("legacy integer score imports and corrupt save is preserved",()=>
{
    string path=Path.Combine(testRoot,"legacy.json"); File.WriteAllText(path,"1234");
    Check(new HighScoreManager(path).Entries[0].Score==1234,"legacy score not imported");
    File.WriteAllText(path,"not valid JSON"); var table=new HighScoreManager(path);
    Check(table.Entries.Count==0 && table.Status.Length>0 && File.ReadAllText(path)=="not valid JSON","corruption handling failed");
});
Test("failed save reports failure without changing in-memory leaderboard",()=>
{
    string folder=Path.Combine(testRoot,"cannot-replace-directory"); Directory.CreateDirectory(folder);
    var table=new HighScoreManager(folder);
    Check(!table.Add("pilot",100,1) && table.Entries.Count==0 && table.Status.Contains("could not"),"save failure hidden");
});
Test("state transition exits old state only after its update completes",()=>
{
    var trace=new List<string>(); var next=new FakeState("next",trace);
    var first=new FakeState("first",trace) { DuringUpdate=()=>StateManager.ChangeState(next) };
    StateManager.ChangeState(first); StateManager.Update(1f/60);
    Check(string.Join(",",trace)=="first.enter,first.update.begin,first.update.end,first.exit,next.enter","unsafe transition order");
    StateManager.Shutdown(); Check(trace[^1]=="next.exit","shutdown did not exit");
});
Check(EventManager.SubscriptionCount==0,"final subscription leak");
Console.WriteLine($"RESULT: {passed} passed; {failed} failed");
Environment.ExitCode=failed==0?0:1;

// Reproducible algorithm experiment: same inputs and hit count, with/without per-bullet ToList copies.
if(args.Contains("--benchmark"))
{
    var random=new Random(379);
    var bullets=Enumerable.Range(0,80).Select(_=>new Rectangle(random.Next(1120),random.Next(720),20,6)).ToList();
    var enemies=Enumerable.Range(0,40).Select(_=>new Rectangle(random.Next(1120),random.Next(720),60,60)).ToList();
    long Run(bool copies)
    {
        long hits=0;
        foreach(var b in copies?bullets.ToList():bullets)
            foreach(var e in copies?enemies.ToList():enemies) if(b.Intersects(e)) hits++;
        return hits;
    }
    for(int i=0;i<1000;i++) { Run(true); Run(false); }
    foreach(bool copies in new[]{true,false})
    {
        long alloc=GC.GetAllocatedBytesForCurrentThread(),hits=0; var timer=Stopwatch.StartNew();
        for(int i=0;i<20000;i++) hits+=Run(copies);
        timer.Stop(); alloc=GC.GetAllocatedBytesForCurrentThread()-alloc;
        Console.WriteLine($"BENCH {(copies?"snapshot-lists":"direct-lists")}: {timer.Elapsed.TotalMilliseconds:F2} ms, {alloc} allocated bytes, {hits} hits; 20,000 frames x 80 bullets x 40 enemies");
    }
}
sealed class FakeState : IGameState
{
    readonly string _name; readonly List<string> _trace;
    public Action DuringUpdate;
    public FakeState(string name,List<string> trace) { _name=name; _trace=trace; }
    public void Enter()=>_trace.Add(_name+".enter");
    public void Exit()=>_trace.Add(_name+".exit");
    public void Update(float dt) { _trace.Add(_name+".update.begin"); DuringUpdate?.Invoke(); _trace.Add(_name+".update.end"); }
    public void Draw(SpriteBatch sb) { }
}
