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
        [DataMemberIgnore]
        public IControlInput ContolInput { get; set; }

        public CharacterComponent Character { get; set; }

        public Entity ModelChildEntity { get; set; }

        public MahineGunScript MachineGun { get; set; }

        private bool isAlive = false;

        [Display("Run Speed")]
        public float MaxRunSpeed { get; set; } = 10;

        [Display("Invulnerability")]
        public float Invulnerability { get; set; } = 0.2f;

        private float invulnerabilityCooldown = 0f;

        private float yawOrientation;

        public override void Start()
        {
            currentHitpoints = MaximumHitpoints;
            invulnerabilityCooldown = 1;
        }

        public void TakeDamage(float amount)
        {
            if (invulnerabilityCooldown > 0)
                return;

            currentHitpoints -= amount;
            invulnerabilityCooldown = Invulnerability;
            ModelChildEntity.Get<ModelComponent>().Enabled = false;

            if (currentHitpoints <= 0)
                Die();
        }

        public void Die()
        {
            // TODO Particle effect - death

            Entity.Get<CharacterComponent>().Enabled = false;
            Entity.Get<RigidbodyComponent>().Enabled = false;

            Entity.Transform.Position.Y = 200;

            currentHitpoints = 0;
            isAlive = false;
        }

        public void Respawn(Vector3 respawnPosition)
        {
            Entity.Transform.Position = respawnPosition;

            Entity.Get<CharacterComponent>().Enabled = true;
            Entity.Get<RigidbodyComponent>().Enabled = true;

            isAlive = true;
            invulnerabilityCooldown = 1;
            currentHitpoints = MaximumHitpoints;
        }

        protected void WaitingToRespawn()
        {
            if (ContolInput.Jump)
            {
                Respawn(new Vector3(0, 2, 0));
            }
        }

        public override void Update()
        {
            if (Character == null)
                return;

            if (!isAlive)
            {
                WaitingToRespawn();
                return;
            }

            var dt = (float)Game.UpdateTime.Elapsed.TotalSeconds;
            if (invulnerabilityCooldown > 0)
            {
                invulnerabilityCooldown -= dt;
                if (invulnerabilityCooldown <= 0)
                {
                    ModelChildEntity.Get<ModelComponent>().Enabled = true;
                }
                else
                {
                    ModelChildEntity.Get<ModelComponent>().Enabled = !ModelChildEntity.Get<ModelComponent>().Enabled;
                }
            }

            // Jump
            if (ContolInput.Jump)
            {
                if (Character.IsGrounded) Character.Jump();
            }

            // Left stick: movement
            var moveDirection = ContolInput.WalkDirection;

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
            var faceDirection = ContolInput.FaceDirection;
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
                MachineGun.IsShooting = ContolInput.Shoot;
            }

            ModelChildEntity.Transform.Rotation = Quaternion.RotationYawPitchRoll(MathUtil.DegreesToRadians(yawOrientation), 0, 0);
        }
    }
}
