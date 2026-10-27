using System;
using Microsoft.Xna.Framework;
namespace SideScrollerShooter.Engine;
public static class Geometry
{
    // AABB of a sprite rotated around its centre. Drawing uses exactly the same transform.
    public static Vector2 Extents(float width,float height,float rotation,float scale=1)
    {
        float c=MathF.Abs(MathF.Cos(rotation)),s=MathF.Abs(MathF.Sin(rotation));
        return new Vector2((width*c+height*s)*scale/2,(width*s+height*c)*scale/2);
    }
    public static Rectangle Bounds(Vector2 centre,Vector2 half)
    {
        int left=(int)MathF.Floor(centre.X-half.X), top=(int)MathF.Floor(centre.Y-half.Y);
        return new Rectangle(left,top,(int)MathF.Ceiling(centre.X+half.X)-left,(int)MathF.Ceiling(centre.Y+half.Y)-top);
    }
    public static Vector2 Clamp(Vector2 centre,Vector2 half,Rectangle area) => new(
        Math.Clamp(centre.X,area.Left+half.X,area.Right-half.X),
        Math.Clamp(centre.Y,area.Top+half.Y,area.Bottom-half.Y));
}
/// <summary>Elapsed-time sprite-sheet animation; catches up if several frames elapse.</summary>
public sealed class SpriteAnimator
{
    public int Frame { get; private set; }
    private readonly int _frames;
    private readonly float _frameSeconds;
    private double _time;
    public SpriteAnimator(int frames,float fps) { if(frames<1 || fps<=0) throw new ArgumentOutOfRangeException(); _frames=frames; _frameSeconds=1/fps; }
    public void Update(float dt) { _time+=dt; while(_time+1e-8>=_frameSeconds) { _time-=_frameSeconds; Frame=(Frame+1)%_frames; } }
}
