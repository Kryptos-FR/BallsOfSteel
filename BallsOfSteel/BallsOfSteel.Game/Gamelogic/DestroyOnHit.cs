// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Input;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Physics;

namespace Gamelogic
{
    public class DestroyOnHit : AsyncScript
    {
        // Declared public member fields and properties will show in the game studio

        public override async Task Execute()
        {
            var trigger = Entity.Get<RigidbodyComponent>();
            trigger.ProcessCollisions = true;

            while (Game.IsRunning)
            {
                // Wait for the next collision event
                var firstCollision = await trigger.NewCollision();

                // Collisions with characters will be processed on the character side
                if (firstCollision.ColliderA.CollisionGroup == CollisionFilterGroups.CharacterFilter ||
                    firstCollision.ColliderB.CollisionGroup == CollisionFilterGroups.CharacterFilter)
                    continue;

                // This was remove before - didn't work due to ordering problem?
                Entity.Get<RigidbodyComponent>().Enabled = false;
                return;
            }
        }
    }
}
