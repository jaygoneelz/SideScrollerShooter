using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SideScrollerShooter.Engine;
namespace SideScrollerShooter.Game.Entities;
public enum PowerUpKind { RapidFire, Shield, Repair }
public sealed class Player : BaseEntity, IDisposable
{
    private readonly WorldConfig _config;
    private readonly IDisposable _moveSubscription,_shootSubscription,_powerSubscription;
    private Vector2 _movement;
    private bool _shooting;
    private float _shootTimer,_rapidTime,_shieldTime,_hurtTime;
    private readonly SpriteAnimator _thruster=new(4,12);
    public int Lives { get; private set; }
    public float RapidTime=>_rapidTime;
    public float ShieldTime=>_shieldTime;
    public Player(Texture2D texture,WorldConfig config):base(texture,new Vector2(config.PlayerX,config.PlayerY))
    {
        _config=config; Lives=config.Lives; Rotation=MathHelper.PiOver2; Scale=0.65f;
        _moveSubscription=EventManager.Subscribe(Events.Move,p=>_movement=(Vector2)p);
        _shootSubscription=EventManager.Subscribe(Events.Shoot,_=>_shooting=true);
        _powerSubscription=EventManager.Subscribe(Events.PowerUpCollected,p=>ApplyPowerUp((PowerUpKind)p));
    }
    public override void Update(float dt)
    {
        _rapidTime=Math.Max(0,_rapidTime-dt); _shieldTime=Math.Max(0,_shieldTime-dt); _hurtTime=Math.Max(0,_hurtTime-dt);
        Position=Geometry.Clamp(Position+_movement*_config.PlayerSpeed*dt,HalfSize,_config.PlayArea);
        _shootTimer-=dt;
        if(_shooting && _shootTimer<=0)
        {
            EventManager.Publish(Events.SpawnBullet,Position+new Vector2(HalfSize.X+8,0));
            AudioManager.PlayShoot(); _shootTimer=_rapidTime>0?0.1f:0.28f;
        }
        _shooting=false; _movement=Vector2.Zero;
        _thruster.Update(dt);
        Tint=_hurtTime>0 && ((int)(_hurtTime*12)%2==0)?Color.White*0.3f:Color.White;
    }
    public void ApplyPowerUp(PowerUpKind kind)
    {
        if(kind==PowerUpKind.RapidFire) _rapidTime=_config.PowerUpDuration;
        if(kind==PowerUpKind.Shield) _shieldTime=_config.PowerUpDuration;
        if(kind==PowerUpKind.Repair) Lives=Math.Min(_config.Lives,Lives+1);
    }
    public void Hit()
    {
        if(_hurtTime>0 || _shieldTime>0 || Lives<=0) return;
        Lives--; _hurtTime=_config.InvulnerabilitySeconds; AudioManager.PlayHit();
    }
    public override void Draw(SpriteBatch sb)
    {
        Ui.DrawThruster(sb,Position-new Vector2(HalfSize.X+10,0),_thruster.Frame);
        base.Draw(sb);
        if(_shieldTime>0) Ui.Outline(sb,Bounds,Color.Cyan,2);
    }
    public void Dispose() { _moveSubscription.Dispose(); _shootSubscription.Dispose(); _powerSubscription.Dispose(); }
}
