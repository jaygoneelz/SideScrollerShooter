using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SideScrollerShooter.Engine;
namespace SideScrollerShooter.Game.Entities;
public sealed class PowerUp : BaseEntity
{
    public PowerUpKind Kind { get; }
    private float _time;
    public PowerUp(Texture2D texture,Vector2 position,PowerUpKind kind):base(texture,position)
    { Kind=kind; Tint=kind==PowerUpKind.RapidFire?Color.Cyan:kind==PowerUpKind.Shield?Color.MediumPurple:Color.Lime; }
    public override void Update(float dt)
    {
        _time+=dt; Position.X-=110*dt; Scale=0.8f+0.08f*MathF.Sin(_time*6);
        if(Bounds.Right<0) Destroy();
    }
    public override void Draw(SpriteBatch sb)
    {
        base.Draw(sb);
        string text=Kind==PowerUpKind.RapidFire?"R":Kind==PowerUpKind.Shield?"S":"+";
        sb.DrawString(Ui.Font,text,Position+new Vector2(-5,20),Tint);
    }
}
