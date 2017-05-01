// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Audio;
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
        
        [Display("Jump", category: "Sound")]
        public Sound JumpSound { get; set; }
        private SoundInstance jumpSfxInstance;
        
        [Display("Spawn", category: "Sound")]
        public Sound SpawnSound { get; set; }
        private SoundInstance spawnSfxInstance;

        public CameraComponent Camera { get; set; }

        // Physical controller, keyboard or a networking device
        [DataMemberIgnore]
        public IControlInput ControlInput { get; set; }

        public CharacterComponent Character { get; set; }

        public Entity ModelChildEntity { get; set; }

        public MahineGunScript MachineGun { get; set; }

        public SwordScript Sword { get; set; }


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
            
            jumpSfxInstance = JumpSound?.CreateInstance();
            jumpSfxInstance?.Stop();
            spawnSfxInstance = SpawnSound?.CreateInstance();
            spawnSfxInstance?.Play();
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
            spawnSfxInstance?.Play();
        }

        protected void WaitingToRespawn()
        {
            if (ControlInput.Jump)
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
            if (ControlInput.Jump)
            {
                if (Character.IsGrounded)
                {
                    jumpSfxInstance?.Stop();
                    Character.Jump();
                    jumpSfxInstance?.Play();
                }
            }

            // Left stick: movement
            var moveDirection = ControlInput.WalkDirection;

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
            var faceDirection = ControlInput.FaceDirection;
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
                MachineGun.IsShooting = ControlInput.Shoot;
            }

            if (Sword != null && ControlInput.Attack)
            {
                Sword.Slash();
            }

            ModelChildEntity.Transform.Rotation = Quaternion.RotationYawPitchRoll(MathUtil.DegreesToRadians(yawOrientation), 0, 0);
        }
    }
}
