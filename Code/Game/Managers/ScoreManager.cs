using System;
using System.Collections.Generic;
using SideScrollerShooter.Engine;
namespace SideScrollerShooter.Game.Managers;
public sealed record EnemyDestroyed(Guid EntityId,int Points);
public sealed class ScoreManager : IDisposable
{
    private readonly IDisposable _subscription;
    private readonly HashSet<Guid> _awarded=new();
    public int Score { get; private set; }
    public int Kills=>_awarded.Count;
    public ScoreManager()
    {
        _subscription=EventManager.Subscribe(Events.EnemyDestroyed,p=>
        {
            if(p is EnemyDestroyed destroyed && destroyed.Points>0 && _awarded.Add(destroyed.EntityId)) Score+=destroyed.Points;
        });
    }
    public void Dispose()=>_subscription.Dispose();
}
