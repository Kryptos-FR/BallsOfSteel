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

using System.Net.Sockets;

namespace BallsOfSteel
{
    public class Client : AsyncScript
    {
        // Declared public member fields and properties will show in the game studio
        private UdpClient client = new UdpClient();

        public string hostIp = "127.0.0.1";
        public int hostPort = 3748;
        
        public override async Task Execute()
        {
            try
            {
                client.Client.Connect(hostIp, hostPort);

                client.Client.Send(new byte[] {0xFF});
            }
            catch (Exception e)
            {
                throw e;
            }

            while (Game.IsRunning)
            {
                // Do stuff every new frame
                await Script.NextFrame();
            }
        }
    }
}
