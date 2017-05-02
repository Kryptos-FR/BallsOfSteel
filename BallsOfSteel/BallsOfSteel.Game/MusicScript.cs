// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
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

namespace BallsOfSteel
{
    /// <summary>
    /// The main script in charge of the sound.
    /// </summary>
    public class MusicScript : StartupScript
    {
        [Display("Background Music")]
        public Sound SoundMusic { get; set; }

        private SoundInstance bgmInstance;

        public override void Start()
        {
            bgmInstance = SoundMusic?.CreateInstance();

            if (bgmInstance != null)
            {
                // start ambient music
                bgmInstance.IsLooping = true;
                bgmInstance.Play();
            }
        }

        public override void Cancel()
        {
            bgmInstance?.Stop();
        }
    }
}
