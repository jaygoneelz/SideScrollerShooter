using Microsoft.Xna.Framework.Graphics;
namespace SideScrollerShooter.Game.States;
/// <summary>Transitions requested during update commit after the active state's update returns.</summary>
public static class StateManager
{
    private static IGameState _current,_pending;
    public static string CurrentName=>_current?.GetType().Name??"None";
    public static void ChangeState(IGameState next)=>_pending=next;
    public static void Update(float dt)
    {
        Commit(); _current?.Update(dt); Commit();
    }
    private static void Commit()
    {
        if(_pending==null) return;
        var next=_pending; _pending=null; _current?.Exit(); _current=next; _current.Enter();
    }
    public static void Draw(SpriteBatch sb)=>_current?.Draw(sb);
    public static void Shutdown() { _pending=null; _current?.Exit(); _current=null; }
}
