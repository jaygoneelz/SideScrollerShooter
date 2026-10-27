using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SideScrollerShooter.Engine;
using SideScrollerShooter.Game.Entities;
using SideScrollerShooter.Game.Managers;
namespace SideScrollerShooter.Game.States;
public sealed class PlayState : IGameState
{
    private SpawnManager _spawns;
    private Player _player;
    private ScoreManager _score;
    private readonly CollisionManager _collisions=new();
    private WorldConfig _world;
    private int _level;
    private float _levelTime,_bannerTime;
    private bool _paused;
    private double _updateMs,_collisionMs;
    public void Enter()
    {
        _world=Game1.World; _score=new ScoreManager();
        _spawns=new SpawnManager(_world); _player=new Player(ResourceManager.LoadTexture("Textures/playerShip1_blue"),_world);
        BeginLevel();
    }
    private void BeginLevel() { _levelTime=0; _bannerTime=3; _spawns.BeginLevel(_world.Levels[_level]); }
    public void Exit() { _player.Dispose(); _spawns.Dispose(); _score.Dispose(); }
    public void Update(float dt)
    {
        if(InputManager.Pressed(Keys.Escape)) { StateManager.ChangeState(new MenuState()); return; }
        if(InputManager.Pressed(Keys.P)) _paused=!_paused;
        if(_paused) return;
        long start=Stopwatch.GetTimestamp();
        _levelTime+=dt; _bannerTime=Math.Max(0,_bannerTime-dt);
        InputManager.PublishGameplay(); _player.Update(dt); _spawns.Update(dt,_player.Position);
        long collisionStart=Stopwatch.GetTimestamp();
        CollisionResponse.Apply(_collisions.Detect(_spawns,_player));
        _collisionMs=Stopwatch.GetElapsedTime(collisionStart).TotalMilliseconds;
        _spawns.RemoveInactive();
        if(_player.Lives<=0) StateManager.ChangeState(new GameOverState(_score.Score,_level+1,false));
        else if(_levelTime>=_world.Levels[_level].Duration)
        {
            if(_level+1==_world.Levels.Count) StateManager.ChangeState(new GameOverState(_score.Score,_level+1,true));
            else { _level++; BeginLevel(); }
        }
        _updateMs=Stopwatch.GetElapsedTime(start).TotalMilliseconds;
    }
    public void Draw(SpriteBatch sb)
    {
        sb.Begin(); Ui.Background(sb);
        _spawns.Draw(sb,Game1.DebugView); _player.Draw(sb);
        if(Game1.DebugView) Ui.Outline(sb,_player.Bounds,Color.Cyan);
        Ui.Panel(sb,new Rectangle(0,0,Game1.ScreenWidth,99));
        Ui.Text(sb,_world.Levels[_level].Name,22,12,Ui.Accent);
        Ui.Text(sb,$"SCORE {_score.Score:N0}    LIVES {_player.Lives}    LEFT {Math.Max(0,_world.Levels[_level].Duration-_levelTime):00.0}s",22,48);
        Ui.Text(sb,$"R {_player.RapidTime:0.0}s   S {_player.ShieldTime:0.0}s",700,12,Color.LightGray,0.85f);
        Ui.Text(sb,"P pause   ESC menu   M mute",700,48,Color.SlateGray,0.8f);
        if(_bannerTime>0) Ui.Centre(sb,_world.Levels[_level].Name,130,Ui.Accent,1.5f);
        if(Game1.DebugView)
        {
            Ui.Panel(sb,new Rectangle(12,Game1.ScreenHeight-90,Game1.ScreenWidth-24,78));
            Ui.Text(sb,$"Update {_updateMs:F3} ms | collisions {_collisionMs:F3} ms | checks {_collisions.Checks} | listeners {EventManager.SubscriptionCount}",25,Game1.ScreenHeight-80,Color.Lime,0.8f);
            Ui.Text(sb,$"Enemies {_spawns.Enemies.Count} | bullets {_spawns.Bullets.Count} | pickups {_spawns.PowerUps.Count} | fixed step 60 Hz",25,Game1.ScreenHeight-48,Color.LightGray,0.8f);
        }
        if(_paused) { Ui.Panel(sb,new Rectangle(Game1.ScreenWidth/2-220,280,440,130)); Ui.Centre(sb,"PAUSED",300,Ui.Accent,1.7f); Ui.Centre(sb,"P / RESUME",360); }
        sb.End();
    }
}
