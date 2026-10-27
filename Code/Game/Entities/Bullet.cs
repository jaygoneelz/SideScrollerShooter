using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
namespace SideScrollerShooter.Game.Entities;
public sealed class Bullet : BaseEntity
{
    private readonly float _speed;
    public Bullet(Texture2D texture,Vector2 position,float speed):base(texture,position) { _speed=speed; Rotation=MathHelper.PiOver2; Scale=0.85f; }
    public override void Update(float dt) { Position.X+=_speed*dt; if(Bounds.Left>Game1.ScreenWidth) Destroy(); }
}
