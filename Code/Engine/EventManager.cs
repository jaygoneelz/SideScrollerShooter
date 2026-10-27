using System;
using System.Collections.Generic;
namespace SideScrollerShooter.Engine;

/// <summary>Single-threaded observer bus. Owners dispose subscription tokens on state exit.</summary>
public static class EventManager
{
    private static readonly Dictionary<string, List<Action<object>>> Handlers = new();
    public static int SubscriptionCount { get { int n=0; foreach(var list in Handlers.Values) n+=list.Count; return n; } }
    public static IDisposable Subscribe(string name, Action<object> handler)
    {
        if (!Handlers.TryGetValue(name,out var list)) Handlers[name]=list=new();
        list.Add(handler);
        return new Subscription(name,handler);
    }
    public static void Publish(string name, object value=null)
    {
        // Snapshot permits a handler to unsubscribe safely during dispatch.
        if(Handlers.TryGetValue(name,out var list)) foreach(var handler in list.ToArray()) handler(value);
    }
    private sealed class Subscription : IDisposable
    {
        private readonly string _name;
        private Action<object> _handler;
        public Subscription(string name,Action<object> handler) { _name=name; _handler=handler; }
        public void Dispose()
        {
            if(_handler==null) return;
            if(Handlers.TryGetValue(_name,out var list)) { list.Remove(_handler); if(list.Count==0) Handlers.Remove(_name); }
            _handler=null;
        }
    }
}
public static class Events
{
    public const string Move="Input_Move", Shoot="Input_Shoot", SpawnBullet="Spawn_Bullet";
    public const string EnemyDestroyed="Enemy_Destroyed", PowerUpCollected="PowerUp_Collected";
}
