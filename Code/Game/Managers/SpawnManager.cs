using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SideScrollerShooter.Engine;
using SideScrollerShooter.Game.Entities;
namespace SideScrollerShooter.Game.Managers;
public sealed class SpawnManager : IDisposable
{
    private readonly List<Enemy> _enemies=new();
    private readonly List<Bullet> _bullets=new();
    private readonly List<PowerUp> _powerUps=new();
    private readonly Texture2D _enemyTex,_bulletTex,_powerTex;
    private readonly WorldConfig _config;
    private readonly Random _random;
    private readonly IDisposable _bulletSubscription;
    private float _spawnTime,_powerTime;
    private int _patternIndex,_pickupIndex;
    public IReadOnlyList<Enemy> Enemies=>_enemies;
    public IReadOnlyList<Bullet> Bullets=>_bullets;
    public IReadOnlyList<PowerUp> PowerUps=>_powerUps;
    public LevelConfig Level { get; private set; }
    public SpawnManager(WorldConfig config)
    {
        _config=config; _random=new Random(config.Seed);
        _enemyTex=ResourceManager.LoadTexture("Textures/ufoRed");
        _bulletTex=ResourceManager.LoadTexture("Textures/laserBlue01");
        _powerTex=ResourceManager.LoadTexture("Textures/powerupBlue_bolt");
        _bulletSubscription=EventManager.Subscribe(Events.SpawnBullet,p=>_bullets.Add(new Bullet(_bulletTex,(Vector2)p,_config.BulletSpeed)));
    }
    public void BeginLevel(LevelConfig level)
    {
        Level=level; _spawnTime=_powerTime=0; _patternIndex=0;
        _enemies.Clear(); _bullets.Clear(); _powerUps.Clear();
        foreach(var initial in level.InitialEnemies) Spawn(initial.Type,new Vector2(initial.X,initial.Y));
    }
    private void Spawn(string kind,Vector2 pos)=>_enemies.Add(new Enemy(_enemyTex,pos,Level.EnemySpeed,kind));
    public void Update(float dt,Vector2 player)
    {
        _spawnTime+=dt; _powerTime+=dt;
        while(_spawnTime>=Level.SpawnInterval)
        {
            _spawnTime-=Level.SpawnInterval;
            if(_enemies.Count<40) Spawn(Level.Pattern[_patternIndex++%Level.Pattern.Count],new Vector2(_config.Width+60,_random.Next(165,_config.Height-65)));
        }
        while(_powerTime>=_config.PowerUpInterval)
        {
            _powerTime-=_config.PowerUpInterval;
            _powerUps.Add(new PowerUp(_powerTex,new Vector2(_config.Width+30,_random.Next(170,_config.Height-70)),(PowerUpKind)(_pickupIndex++%3)));
        }
        foreach(var enemy in _enemies) { enemy.Target=player; enemy.Update(dt); }
        foreach(var bullet in _bullets) bullet.Update(dt);
        foreach(var pickup in _powerUps) pickup.Update(dt);
    }
    public void RemoveInactive()
    { _enemies.RemoveAll(e=>!e.IsAlive); _bullets.RemoveAll(e=>!e.IsAlive); _powerUps.RemoveAll(e=>!e.IsAlive); }
    public void Draw(SpriteBatch sb,bool debug)
    {
        foreach(var enemy in _enemies) { enemy.Draw(sb); if(debug) { Ui.Outline(sb,enemy.Bounds,Color.Red); sb.DrawString(Ui.Font,enemy.Behaviour,enemy.Position+new Vector2(-30,-50),Color.White,0,Vector2.Zero,0.65f,SpriteEffects.None,0); } }
        foreach(var bullet in _bullets) { bullet.Draw(sb); if(debug) Ui.Outline(sb,bullet.Bounds,Color.Yellow); }
        foreach(var pickup in _powerUps) { pickup.Draw(sb); if(debug) Ui.Outline(sb,pickup.Bounds,Color.Lime); }
    }
    public void Dispose()=>_bulletSubscription.Dispose();
}
