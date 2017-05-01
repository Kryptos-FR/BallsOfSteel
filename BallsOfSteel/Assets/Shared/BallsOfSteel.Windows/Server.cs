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

namespace BallsOfSteel.Windows
{
    public class Server : AsyncScript
    {
        // Declared public member fields and properties will show in the game studio

        public override async Task Execute()
        {
            while(Game.IsRunning)
            {
                // Do stuff every new frame
                await Script.NextFrame();
            }
        }
    }
}
