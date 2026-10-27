using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SideScrollerShooter.Engine;
namespace SideScrollerShooter.Game.Entities;
/// <summary>Shared identity, centred transform, bounds, lifetime and drawing.</summary>
public abstract class BaseEntity
{
    public Guid Id { get; }=Guid.NewGuid();
    protected readonly Texture2D Texture;
    public Vector2 Position;
    public bool IsAlive { get; private set; }=true;
    public float Rotation { get; protected set; }
    public float Scale { get; protected set; }=0.7f;
    public Color Tint { get; protected set; }=Color.White;
    public Vector2 HalfSize=>Geometry.Extents(Texture.Width,Texture.Height,Rotation,Scale);
    public Rectangle Bounds=>Geometry.Bounds(Position,HalfSize);
    protected BaseEntity(Texture2D texture,Vector2 position) { Texture=texture; Position=position; }
    public void Destroy()=>IsAlive=false;
    public abstract void Update(float dt);
    public virtual void Draw(SpriteBatch sb) => sb.Draw(Texture,Position,null,Tint,Rotation,new Vector2(Texture.Width,Texture.Height)/2,Scale,SpriteEffects.None,0);
}
