// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Engine.Events;
using SiliconStudio.Xenko.Input;
using BallsOfSteel.Core;
using Gamelogic;
using SiliconStudio.Core;
using SiliconStudio.Xenko.Physics;

namespace BallsOfSteel.Player
{
    public class PlayerInputControl : SyncScript
    {
        public float MaximumHitpoints { get; set; } = 100;
        private float currentHitpoints = 0;

        public CameraComponent Camera { get; set; }

        // Physical controller, keyboard or a networking device
        public XboxInput ContolInput { get; set; }

        // TEMPORARY: FOR TEST
        public PlayerInputData RemoteContolInput { get; set; }

        public CharacterComponent Character { get; set; }

        public Client GameClient { get; set; }

        public Entity ModelChildEntity { get; set; }

        public MahineGunScript MachineGun { get; set; }

        [Display("Run Speed")]
        public float MaxRunSpeed { get; set; } = 10;

        private float yawOrientation;

        public override void Start()
        {
            currentHitpoints = MaximumHitpoints;
        }

        public void TakeDamage(float amount)
        {
            currentHitpoints -= amount;

            // TODO: Die

        }

        public override void Update()
        {
            if (Character == null)
                return;

            // Jump
            if (RemoteContolInput.Jump)
            {
                if (Character.IsGrounded) Character.Jump();
            }

            // Left stick: movement
            var moveDirection = RemoteContolInput.WalkDirection;

            // Broadcast the movement vector as a world-space Vector3 to allow characters to be controlled
            var worldSpeed = (Camera != null)
                ? Utils.LogicDirectionToWorldDirection(moveDirection, Camera, Vector3.UnitY)
                : new Vector3(moveDirection.X, 0, moveDirection.Y);

            Character.SetVelocity(worldSpeed * MaxRunSpeed);

            // Facing direction
            if (ModelChildEntity == null)
                return;

            // Character orientation
            if (moveDirection.Length() > 0.001)
            {
                yawOrientation = MathUtil.RadiansToDegrees((float)Math.Atan2(worldSpeed.Z, -worldSpeed.X) + MathUtil.PiOverTwo);
            }
            
            // Left stick: movement
            var faceDirection = RemoteContolInput.FaceDirection;
            if (faceDirection.Length() > 0.001)
            {
                // Broadcast the movement vector as a world-space Vector3 to allow characters to be controlled
                var worldFacing = (Camera != null)
                    ? Utils.LogicDirectionToWorldDirection(faceDirection, Camera, Vector3.UnitY)
                    : new Vector3(faceDirection.X, 0, faceDirection.Y);

                if (faceDirection.Length() > 0.001)
                {
                    yawOrientation =
                        MathUtil.RadiansToDegrees((float) Math.Atan2(worldFacing.Z, -worldFacing.X) + MathUtil.PiOverTwo);
                }
            }

            if (MachineGun != null)
            {
                MachineGun.IsShooting = RemoteContolInput.Shoot;
            }

            ModelChildEntity.Transform.Rotation = Quaternion.RotationYawPitchRoll(MathUtil.DegreesToRadians(yawOrientation), 0, 0);

            GameClient.PushInputUpdate(ContolInput);
        }
    }
}
