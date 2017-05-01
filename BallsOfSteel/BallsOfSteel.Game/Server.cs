// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Input;
using SiliconStudio.Xenko.Engine;

using System.Net.Sockets;
using System.Threading;

namespace BallsOfSteel
{
    public class Server : AsyncScript
    {
        struct Client
        {
            public long clientHash; // some kind of GUID or stuff?
            public bool isAlive;
            public string remoteIp;
        }

        public const int MaxPlayerCount = 64;
        private Client[] clients = new Client[MaxPlayerCount];

        // Declared public member fields and properties will show in the game studio
        private UdpClient serverClient = new UdpClient();

        public override async Task Execute()
        {
            try
            {
                serverClient.Client.Connect("127.0.0.1", 3748);
            }
            catch (Exception e)
            {
                throw e;
            }

            byte[] received = new byte[512];
            while (true)
            {
                serverClient.Client.Receive(received);

                if (received[0] == 0xFF)
                {
                    throw new AbandonedMutexException();
                }
            }
        }
    }
}
