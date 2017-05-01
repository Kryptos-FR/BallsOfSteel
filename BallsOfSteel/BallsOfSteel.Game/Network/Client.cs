// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Collections.Generic;
using System.IO;
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
    public class Client : AsyncScript
    {
        private string hostIp = "127.0.0.1";
        private bool isConnected = false;
        private Stack<byte[]> packetsToSend = new Stack<byte[]>();

        public void PushInputUpdate(Vector2 moveDirection, Vector3 worldSpeed, bool isJumping)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    writer.Write((byte)PacketMagic.InputMove);
                    writer.Write(moveDirection.X);
                    writer.Write(moveDirection.Y);
                    writer.Write(worldSpeed.X);
                    writer.Write(worldSpeed.Y);
                    writer.Write(worldSpeed.Z);
                    writer.Write(isJumping);
                }

                packetsToSend.Push(stream.ToArray());
            }
        }

        public override async Task Execute()
        {
            IPEndPoint ep = new IPEndPoint(IPAddress.Parse(hostIp), SharedNetwork.ListenPort);

            var sender = new UdpClient();
            sender.Client.Connect(ep);
            
            if (!sender.Client.Connected)
            {
                throw new Exception(">> BoS Client : Failed to connect to " + hostIp + ":" + SharedNetwork.ListenPort);
            }

            sender.Client.Send(new[]{ (byte)PacketMagic.Connect });

            byte[] buffer = new byte[1024];
            SocketAsyncEventArgs e = new SocketAsyncEventArgs();
            e.SetBuffer(buffer, 0, buffer.Length);

            e.Completed += (o, args) =>
            {
                if (e.Buffer.Length != 0)
                {
                    var cmd = (PacketMagic) e.Buffer[0];

                    if (cmd == PacketMagic.Failed)
                    {
                        return;
                        throw new Exception(">> BoS Client : Server " + hostIp +
                                            " is full or you are already connected");
                    }
                    else if (cmd == PacketMagic.Success)
                    {
                        isConnected = true;
                        Console.WriteLine(">> BoS Client : Connected to {0}", hostIp);
                    }
                }
                else
                {
                    throw new Exception(">> BoS Client : Server " + hostIp + " : no reply received");
                }
            };

            var incomingPacket = sender.Client.ReceiveAsync(e);

            while (!isConnected)
            {
                await Script.NextFrame();
            }

            while (sender.Client.Connected && Game.IsRunning)
            {
                while (packetsToSend.Count != 0)
                {
                    var packet = packetsToSend.Pop();
                    sender.Client.Send(packet);
                }

                // Do stuff every new frame
                await Script.NextFrame();
            }

            sender.Client.Send(new[] { (byte)PacketMagic.Disconnect });
        }
    }
}
