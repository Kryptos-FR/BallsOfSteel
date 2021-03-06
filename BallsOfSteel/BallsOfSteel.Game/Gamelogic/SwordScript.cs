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
using SiliconStudio.Xenko.Physics;
using SiliconStudio.Xenko.Particles.Components;

namespace Gamelogic
{
    public class SwordScript : SyncScript
    {
        [Display("Sword", category: "Sound")]
        public Sound SwordSound { get; set; }
        private SoundInstance swordSfxInstance;

        private Vector3[] animPos = new Vector3[4];
        private Vector3[] animRot = new Vector3[4];

        public float SwingDuration { get; set; } = 0.4f;
        private float swingCooldown = 0;
        public float SwingCooldown => swingCooldown;

        public override void Start()
        {
            animPos[0] = new Vector3(-0.585f, 0f, 0f);
            animPos[1] = new Vector3(0f, 0f, -0.75f);
            animPos[2] = new Vector3(0.5f, 0f, -0.5f);
            animPos[3] = new Vector3(0.8f, 0f, -0.325f);

            animRot[0] = new Vector3(2.0944f, 0f, 0f);
            animRot[1] = new Vector3(0f, 0f, 1.5708f);
            animRot[2] = new Vector3(0f, -0.523599f, 1.5708f);
            animRot[3] = new Vector3(0f, -1.3962634f, 1.5708f);

            // Initialization of the script.
            swordSfxInstance = SwordSound?.CreateInstance();
            swordSfxInstance?.Stop();
        }

        public override void Update()
        {
            if (swingCooldown <= 0)
                return;

            var dt = (float)Game.UpdateTime.Elapsed.TotalSeconds;
            swingCooldown = Math.Max(0, swingCooldown - dt);

            if (swingCooldown < SwingDuration * 0.5f)
            {
                DisableDamage();
            }

            var lerp = 1f - Math.Abs(2 * swingCooldown - SwingDuration) / SwingDuration;
            lerp = Math.Max(0, Math.Min(lerp, 0.999f));

            var index = (int)(lerp * (animPos.Length - 1));
            lerp = (lerp * (animPos.Length - 1) - index);

            Entity.Transform.Position = Vector3.Lerp(animPos[index], animPos[index + 1], lerp);
            Entity.Transform.RotationEulerXYZ = Vector3.Lerp(animRot[index], animRot[index + 1], lerp);

        }

        public void Slash()
        {
            if (swingCooldown > 0.1f) // Allow to start a new swing before the previous one finishes
                return;

            EnableDamage();
            swingCooldown = SwingDuration;
        }

        private void EnableDamage()
        {
            Entity.Get<RigidbodyComponent>().Enabled = true;
            Entity.Get<ParticleSystemComponent>().Enabled = true;
            Entity.Get<ParticleSystemComponent>().ParticleSystem.Play();
            swordSfxInstance?.Play();
        }

        private void DisableDamage()
        {
            Entity.Get<RigidbodyComponent>().Enabled = false;
            Entity.Get<ParticleSystemComponent>().ParticleSystem.StopEmitters();
            swordSfxInstance?.Stop();
        }

    }
}
