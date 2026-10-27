using System.Collections.Generic;
using SideScrollerShooter.Engine;
using SideScrollerShooter.Game.Entities;
namespace SideScrollerShooter.Game.Managers;
/// <summary>Responses consume contacts. Physical removal happens later in SpawnManager.</summary>
public static class CollisionResponse
{
    public static void Apply(IReadOnlyList<Contact> contacts)
    {
        foreach(var contact in contacts)
        {
            if(!contact.First.IsAlive || !contact.Second.IsAlive) continue;
            switch(contact.Kind)
            {
                case ContactKind.BulletEnemy:
                    var enemy=(Enemy)contact.Second;
                    contact.First.Destroy(); enemy.Destroy();
                    EventManager.Publish(Events.EnemyDestroyed,new EnemyDestroyed(enemy.Id,enemy.Points));
                    AudioManager.PlayHit(); break;
                case ContactKind.EnemyPlayer:
                    contact.First.Destroy(); ((Player)contact.Second).Hit(); break;
                case ContactKind.PickupPlayer:
                    contact.First.Destroy();
                    EventManager.Publish(Events.PowerUpCollected,((PowerUp)contact.First).Kind); break;
            }
        }
    }
}
