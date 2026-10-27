using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SideScrollerShooter.Engine;
namespace SideScrollerShooter.Game.Entities;
public sealed class Enemy : BaseEntity
{
    private readonly EnemyBrain _brain;
    private readonly SpriteAnimator _beacon=new(4,8);
    public Vector2 Target;
    public int Points { get; }
    public string Behaviour=>_brain.StateName;
    public Enemy(Texture2D texture,Vector2 position,float speed,string kind):base(texture,position)
    {
        _brain=new EnemyBrain(kind,speed,position); Rotation=-MathHelper.PiOver2;
        Tint=kind=="hunter"?Color.MediumPurple:kind=="weaver"?Color.Orange:Color.White;
        Points=kind=="hunter"?200:kind=="weaver"?150:100;
    }
    public override void Update(float dt)
    {
        _brain.Update(dt,Target); Position=_brain.Position; _beacon.Update(dt);
        if(Bounds.Right<0 || Bounds.Bottom<95 || Bounds.Top>Game1.ScreenHeight) Destroy();
    }
    public override void Draw(SpriteBatch sb)
    {
        base.Draw(sb);
        sb.Draw(Ui.Pixel,new Rectangle((int)Position.X-3,(int)Position.Y-3,6,6),_beacon.Frame%2==0?Color.White:Color.Red);
    }
}
