using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SideScrollerShooter.Engine;
using SideScrollerShooter.Game.Entities;
using SideScrollerShooter.Game.Managers;
using SideScrollerShooter.Game.States;
namespace SideScrollerShooter.Diagnostics;

/// <summary>Opt-in deterministic integration run. Synthetic input, isolated saves, actual GPU screenshots.</summary>
public sealed class SmokeRunner
{
    private int _frame;
    private readonly string _output=Path.Combine(AppContext.BaseDirectory,"smoke-output");
    private readonly StreamWriter _log;
    private string _lastState="";
    public SmokeRunner()
    {
        Directory.CreateDirectory(_output);
        Environment.SetEnvironmentVariable("SIDESCROLLERSHOOTER_DATA",Path.Combine(_output,"smoke-data"));
        _log=new StreamWriter(Path.Combine(_output,"smoke-log.txt")) { AutoFlush=true };
    }
    public void CheckGameplay()
    {
        void Require(bool condition,string reason) { if(!condition) throw new InvalidOperationException(reason); }
        int before=EventManager.SubscriptionCount;
        var world=Game1.World;
        using(var spawns=new SpawnManager(world))
        using(var player=new Player(ResourceManager.LoadTexture("Textures/playerShip1_blue"),world))
        using(var score=new ScoreManager())
        {
            spawns.BeginLevel(world.Levels[0]);
            foreach(var enemy in spawns.Enemies) enemy.Position=player.Position;
            EventManager.Publish(Events.SpawnBullet,player.Position);
            EventManager.Publish(Events.SpawnBullet,player.Position);
            var contacts=new CollisionManager().Detect(spawns,player);
            Require(spawns.Enemies[0].IsAlive && score.Score==0,"Detection changed game state");
            CollisionResponse.Apply(contacts); CollisionResponse.Apply(contacts);
            Require(score.Score==200 && score.Kills==2,"Contact response awarded duplicates");
            Require(player.Lives==world.Lives,"Destroyed enemies damaged the player");
            spawns.RemoveInactive();
            Require(spawns.Enemies.Count==0 && spawns.Bullets.Count==0,"Inactive objects were not removed");
            player.Hit(); player.Hit(); Require(player.Lives==world.Lives-1,"Hit cooldown failed");
            player.Update(world.InvulnerabilitySeconds+0.01f);
            for(int kind=0;kind<3;kind++)
            {
                spawns.Update(world.PowerUpInterval,player.Position);
                var pickup=spawns.PowerUps[0]; Require((int)pickup.Kind==kind,"Pickup cycle failed");
                player.Position=pickup.Position;
                CollisionResponse.Apply(new CollisionManager().Detect(spawns,player));
                spawns.RemoveInactive();
                if(kind==0) Require(player.RapidTime>0,"Rapid-fire event not delivered");
                if(kind==1) { int lives=player.Lives; player.Hit(); Require(player.ShieldTime>0 && player.Lives==lives,"Shield did not protect player"); }
                if(kind==2) Require(player.Lives==world.Lives,"Repair did not restore life");
            }
            _log.WriteLine("PASS: real entity collision detection/response/removal, duplicate contacts, invulnerability, and all three pickup effects.");
        }
        Require(EventManager.SubscriptionCount==before,"Integration checks leaked listeners");
        foreach(int hz in new[]{30,60,120})
        {
            using var player=new Player(ResourceManager.LoadTexture("Textures/playerShip1_blue"),world);
            for(int i=0;i<hz;i++) { EventManager.Publish(Events.Move,new Vector2(1,0)); player.Update(1f/hz); }
            Require(Math.Abs(player.Position.X-(world.PlayerX+world.PlayerSpeed))<0.1f,"Actual player movement depends on frame rate");
            player.Position=new Vector2(-1000,-1000); player.Update(0);
            Require(player.Bounds.Left>=0 && player.Bounds.Top>=100,"Actual player escapes top/left bounds");
            player.Position=new Vector2(10000,10000); player.Update(0);
            Require(player.Bounds.Right<=world.Width && player.Bounds.Bottom<=world.Height,"Actual player escapes bottom/right bounds");
        }
        _log.WriteLine("PASS: actual player displacement at 30/60/120 Hz and clamping at all four boundaries.");
    }
    public KeyboardState ReadKeyboard()
    {
        return _frame switch
        {
            2 or 320 or 6700 => new KeyboardState(Keys.Enter),
            120 => new KeyboardState(Keys.F3),
            180 or 240 => new KeyboardState(Keys.P),
            280 or 6800 => new KeyboardState(Keys.Escape),
            6710 => new KeyboardState(Keys.H),
            _ => StateManager.CurrentName=="PlayState" ? new KeyboardState(Keys.Space) : new KeyboardState()
        };
    }
    public void BeforeUpdate()
    {
        _frame++;
        if(StateManager.CurrentName=="PlayState") EventManager.Publish(Events.PowerUpCollected,PowerUpKind.Shield);
        if(_frame==350) EventManager.Publish(Events.PowerUpCollected,PowerUpKind.RapidFire);
        if(_frame==360) EventManager.Publish(Events.PowerUpCollected,PowerUpKind.Repair);
    }
    public void AfterDraw(GraphicsDevice device)
    {
        string state=StateManager.CurrentName;
        if(state!=_lastState) { _log.WriteLine($"frame {_frame}: {state}; subscriptions={EventManager.SubscriptionCount}"); _lastState=state; }
        string capture=_frame switch
        {
            1=>"01-menu", 150=>"02-gameplay-bounds",200=>"03-paused",300=>"04-menu-after-exit",
            500=>"05-sector-one",2300=>"06-sector-two",4500=>"07-sector-three",6650=>"08-mission-complete",6740=>"09-high-scores",_=>null
        };
        if(capture!=null)
        {
            var pixels=new Color[device.PresentationParameters.BackBufferWidth*device.PresentationParameters.BackBufferHeight];
            device.GetBackBufferData(pixels);
            using var texture=new Texture2D(device,device.PresentationParameters.BackBufferWidth,device.PresentationParameters.BackBufferHeight);
            texture.SetData(pixels);
            using var stream=File.Create(Path.Combine(_output,capture+".png"));
            texture.SaveAsPng(stream,texture.Width,texture.Height);
            _log.WriteLine($"capture {capture}: {state}");
        }
        if(_frame==300 && EventManager.SubscriptionCount!=0) throw new InvalidOperationException("State exit leaked subscriptions");
        if(_frame==6650 && state!="GameOverState") throw new InvalidOperationException("Campaign did not complete");
        if(_frame==6740 && state!="HighScoresState") throw new InvalidOperationException("Score save/table navigation failed");
        if(_frame>=6900)
        {
            if(EventManager.SubscriptionCount!=0) throw new InvalidOperationException("Campaign leaked subscriptions");
            _log.WriteLine("PASS: menu, play, pause, exit/retry, all sectors, victory, score save, table, listener cleanup. Synthetic shield kept the test pilot alive.");
            _log.Dispose(); Game1.Instance.Exit();
        }
    }
}
