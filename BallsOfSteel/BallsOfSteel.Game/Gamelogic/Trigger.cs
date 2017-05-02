using System;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Physics;
using System.Threading.Tasks;
using BallsOfSteel.Player;
using Gamelogic;
using SiliconStudio.Core;
using SiliconStudio.Xenko.Engine.Events;

namespace BallsOfSteel.Gamelogic
{
    public class Trigger : AsyncScript
    {
        public override async Task Execute()
        {
            var trigger = Entity.Get<RigidbodyComponent>();
            trigger.ProcessCollisions = true;

            while (Game.IsRunning)
            {
                // Wait for the next collision event
                var firstCollision = await trigger.NewCollision();

                // Filter collisions based on collision groups
                var filterAhitB = ((int)firstCollision.ColliderA.CanCollideWith) & ((int)firstCollision.ColliderB.CollisionGroup);
                var filterBhitA = ((int)firstCollision.ColliderB.CanCollideWith) & ((int)firstCollision.ColliderA.CollisionGroup);
                if (filterAhitB == 0 || filterBhitA == 0)
                    continue;

                var character = (firstCollision.ColliderA.Entity.Get<PlayerInputControl>()) ??
                                (firstCollision.ColliderB.Entity.Get<PlayerInputControl>());

                var damage = (firstCollision.ColliderA.Entity?.Get<DamagingScript>()) ??
                                (firstCollision.ColliderB.Entity?.Get<DamagingScript>());

                if (character == null || damage == null)
                    return; // I guess I'm impervious to that damage

                var damageOwner = damage.Entity;
                while (damageOwner.GetParent() != null)
                    damageOwner = damageOwner.GetParent();

                if (damageOwner == character.Entity)
                    continue; // No self inflicted wounds

                if (damage.DamagePerHit <= 0)
                    continue;

                character.TakeDamage(damage.DamagePerHit);
                damage.RemoveDamage();

                damage.Entity.Get<RigidbodyComponent>().Enabled = false;

                // TODO: Spawn on-hit effects
                // TODO: Staggerf

            }
        }
    }
}
