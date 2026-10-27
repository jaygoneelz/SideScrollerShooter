using System.Collections.Generic;
using SideScrollerShooter.Game.Entities;
namespace SideScrollerShooter.Game.Managers;
public enum ContactKind { BulletEnemy, EnemyPlayer, PickupPlayer }
public readonly record struct Contact(ContactKind Kind,BaseEntity First,BaseEntity Second);
/// <summary>Detection only. Brute-force rectangle overlap, no scoring or destruction.</summary>
public sealed class CollisionManager
{
    private readonly List<Contact> _contacts=new();
    public int Checks { get; private set; }
    public IReadOnlyList<Contact> Detect(SpawnManager spawns,Player player)
    {
        _contacts.Clear(); Checks=0;
        foreach(var bullet in spawns.Bullets)
            foreach(var enemy in spawns.Enemies)
                if(bullet.IsAlive && enemy.IsAlive) { Checks++; if(bullet.Bounds.Intersects(enemy.Bounds)) _contacts.Add(new(ContactKind.BulletEnemy,bullet,enemy)); }
        foreach(var enemy in spawns.Enemies)
            if(enemy.IsAlive) { Checks++; if(enemy.Bounds.Intersects(player.Bounds)) _contacts.Add(new(ContactKind.EnemyPlayer,enemy,player)); }
        foreach(var pickup in spawns.PowerUps)
            if(pickup.IsAlive) { Checks++; if(pickup.Bounds.Intersects(player.Bounds)) _contacts.Add(new(ContactKind.PickupPlayer,pickup,player)); }
        return _contacts;
    }
}
