// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Engine.Events;
using SiliconStudio.Xenko.Input;
using BallsOfSteel.Core;

namespace BallsOfSteel.Player
{
    public class PlayerInputControl : SyncScript
    {
        public CameraComponent Camera { get; set; }

        // Physical controller, keyboard or a networking device
        public IControlInput ContolInput { get; set; }

        public override void Update()
        {
            // Left stick: movement
            var moveDirection = ContolInput.WalkDirection;

            // Broadcast the movement vector as a world-space Vector3 to allow characters to be controlled
            var worldSpeed = (Camera != null)
                ? Utils.LogicDirectionToWorldDirection(moveDirection, Camera, Vector3.UnitY)
                : new Vector3(moveDirection.X, 0, moveDirection.Y);


        }
    }
}
