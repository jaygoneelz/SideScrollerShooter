using System;
using Microsoft.Xna.Framework;
namespace SideScrollerShooter.Game.Entities;
public interface IEnemyState
{
    string Name { get; }
    void Enter(EnemyBrain brain);
    void Update(EnemyBrain brain,float dt,Vector2 target);
    void Exit(EnemyBrain brain);
}
/// <summary>Behaviour FSM, independent of rendering. Lifetime belongs only to BaseEntity.</summary>
public sealed class EnemyBrain
{
    public Vector2 Position,Velocity;
    public float StateTime { get; private set; }
    public readonly string Kind;
    public readonly float Speed;
    public string StateName=>_state.Name;
    private IEnemyState _state;
    public EnemyBrain(string kind,float speed,Vector2 position)
    { Kind=kind; Speed=speed; Position=position; Change(new EnterState()); }
    public void Change(IEnemyState next) { _state?.Exit(this); _state=next; StateTime=0; _state.Enter(this); }
    public void Update(float dt,Vector2 target) { StateTime+=dt; _state.Update(this,dt,target); Position+=Velocity*dt; }
    private sealed class EnterState : IEnemyState
    {
        public string Name=>"Enter";
        public void Enter(EnemyBrain b) { b.Velocity=new Vector2(-b.Speed*0.5f,0); }
        public void Update(EnemyBrain b,float dt,Vector2 target) { if(b.StateTime>=0.5f) b.Change(new PatrolState()); }
        public void Exit(EnemyBrain b) { }
    }
    private sealed class PatrolState : IEnemyState
    {
        public string Name=>"Patrol";
        public void Enter(EnemyBrain b) { b.Velocity=new Vector2(-b.Speed,0); }
        public void Update(EnemyBrain b,float dt,Vector2 target)
        {
            b.Velocity=new Vector2(-b.Speed,b.Kind=="weaver"?MathF.Cos(b.StateTime*3)*100:0);
            if(b.StateTime>=(b.Kind=="hunter"?1.2f:2.5f)) b.Change(new ChargeState(target));
        }
        public void Exit(EnemyBrain b) { }
    }
    private sealed class ChargeState : IEnemyState
    {
        private readonly Vector2 _target;
        public string Name=>"Charge";
        public ChargeState(Vector2 target) { _target=target; }
        public void Enter(EnemyBrain b)
        {
            var direction=b.Kind=="scout"?new Vector2(-1,0):_target-b.Position;
            if(direction.LengthSquared()<0.01f) direction=new Vector2(-1,0);
            direction.Normalize(); b.Velocity=direction*b.Speed*1.6f;
        }
        public void Update(EnemyBrain b,float dt,Vector2 target) { if(b.StateTime>=1) b.Change(new RetreatState()); }
        public void Exit(EnemyBrain b) { }
    }
    private sealed class RetreatState : IEnemyState
    {
        public string Name=>"Retreat";
        public void Enter(EnemyBrain b) { b.Velocity=new Vector2(-b.Speed*1.2f,0); }
        public void Update(EnemyBrain b,float dt,Vector2 target) { }
        public void Exit(EnemyBrain b) { }
    }
}
