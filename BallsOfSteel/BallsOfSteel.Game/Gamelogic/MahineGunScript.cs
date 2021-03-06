﻿// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Audio;
using SiliconStudio.Xenko.Input;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Particles.Components;
using SiliconStudio.Xenko.Physics;

namespace Gamelogic
{
    public class MahineGunScript : SyncScript
    {        
        [Display("Gun", category: "Sound")]
        public Sound GunSound { get; set; }
        private SoundInstance gunSfxInstance;
        
        // Declared public member fields and properties will show in the game studio
        public Prefab BulletPrefab { get; set; }

        public bool IsShooting { get; set; } = true;

        public ParticleSystemComponent MuzzleFlash { get; set; }

        public float Cooldown { get; set; } = 0.2f;

        public float ReloadCooldown { get; set; } = 1.2f;

        public int MagazineCapacity { get; set; } = 40;

        public float BulletInitialSpeed { get; set; } = 20f;

        private float cooldownRemaining = 0f;

        private int bulletsRemaining = 0;

        private Random rand = new Random();

        public override void Start()
        {
            bulletsRemaining = MagazineCapacity;
            gunSfxInstance = GunSound?.CreateInstance();
            if (gunSfxInstance != null)
            {
                // boost sfx volume
                gunSfxInstance.Volume = 10;
                gunSfxInstance.Stop();
            }
        }

        public override void Update()
        {
            if (MuzzleFlash != null)
            {
                if (IsShooting && cooldownRemaining <= 0f)
                {
                    MuzzleFlash.ParticleSystem.Play();
                }
                else
                {
                    MuzzleFlash.ParticleSystem.StopEmitters();
                }
            }

            if (bulletsRemaining <= 0)
            {
                bulletsRemaining = MagazineCapacity;
                cooldownRemaining = ReloadCooldown;
                
                // Stop shoot sound effect
                gunSfxInstance?.Stop();
                // TODO Play reload sound effect
                return;
            }

            var dt = (float)Game.UpdateTime.Elapsed.TotalSeconds;
            if (cooldownRemaining > 0)
                cooldownRemaining -= dt;

            if (!IsShooting || cooldownRemaining > 0f)
            {
                // Stop shoot sound effect
                gunSfxInstance?.Stop();
                return; // Won't or can't shoot
            }

            bulletsRemaining--;
            ShootBullets(BulletPrefab, 2f, Entity.Transform.WorldMatrix);

            // Play shoot sound effect
            if (gunSfxInstance?.PlayState == SoundPlayState.Stopped)
                gunSfxInstance?.Play();

            // Do stuff every new frame
        }

        protected void ShootBullets(Prefab source, float timeout, Matrix localMatrix)
        {
            if (source == null)
                return;

            var forwardVector = -localMatrix.Backward;
            var forwardForce = (float) ((0.75f + rand.NextDouble() * 0.25f) * BulletInitialSpeed);

            Func<Task> spawnTask = async () =>
            {
                // Clone
                var spawnedEntities = source.Instantiate();

                // Add
                foreach (var prefabEntity in spawnedEntities)
                {
                    prefabEntity.Transform.UpdateLocalMatrix();
                    var entityMatrix = prefabEntity.Transform.LocalMatrix * localMatrix;
                    entityMatrix.Decompose(out prefabEntity.Transform.Scale, out prefabEntity.Transform.Rotation, out prefabEntity.Transform.Position);

                    SceneSystem.SceneInstance.RootScene.Entities.Add(prefabEntity);

                    // Add initial speed
                    var rigidBody = prefabEntity.Get<RigidbodyComponent>();

                    if (rigidBody != null)
                    {
                        rigidBody.Activate();
                        rigidBody.ApplyImpulse(forwardVector * forwardForce);
                    }

                }

                // Countdown
                var secondsCountdown = timeout;
                while (secondsCountdown > 0f)
                {
                    await Script.NextFrame();
                    secondsCountdown -= (float)Game.UpdateTime.Elapsed.TotalSeconds;
                }

                // Remove
                foreach (var clonedEntity in spawnedEntities)
                {
                    SceneSystem.SceneInstance.RootScene.Entities.Remove(clonedEntity);
                }

                // Cleanup
                spawnedEntities.Clear();
            };

            Script.AddTask(spawnTask);
        }
    }
}
