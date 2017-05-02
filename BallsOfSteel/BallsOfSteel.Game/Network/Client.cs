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
using BallsOfSteel.Player;

namespace BallsOfSteel
{
    public class Client : AsyncScript
    {
        private string hostIp = "127.0.0.1";
        private bool isConnected = false;
        private Stack<byte[]> packetsToSend = new Stack<byte[]>();

        public void PushInputUpdate(IControlInput input)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    writer.Write((byte)PacketMagic.InputMove);

                    writer.Write(input.WalkDirection.X);
                    writer.Write(input.WalkDirection.Y);

                    writer.Write(input.FaceDirection.X);
                    writer.Write(input.FaceDirection.Y);

                    writer.Write(input.Jump);
                    writer.Write(input.Attack);
                    writer.Write(input.Shoot);
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

            sender.Client.Send(new[] { (byte)PacketMagic.Connect });

            Console.WriteLine(">> BoS Client : Awaiting reply from {0}", hostIp);

            byte[] buffer = new byte[1024];
            SocketAsyncEventArgs e = new SocketAsyncEventArgs();
            e.SetBuffer(buffer, 0, buffer.Length);

            e.Completed += (o, args) =>
            {
                if (e.Buffer.Length != 0)
                {
                    var cmd = (PacketMagic)e.Buffer[0];

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

            int fakeStuff = 0;
            while (Game.IsRunning)
            {
                while (packetsToSend.Count != 0)
                {
                    var packet = packetsToSend.Pop();
                    sender.Client.Send(packet);
                }

                fakeStuff++;

                if (fakeStuff % 120 == 0)
                {                 
                    using (MemoryStream stream = new MemoryStream())
                    {
                        using (BinaryWriter writer = new BinaryWriter(stream))
                        {
                            writer.Write((byte)PacketMagic.InputMove);

                            writer.Write(0.1f);
                            writer.Write(0.0f);

                            writer.Write(1.0f);
                            writer.Write(0.0f);

                            writer.Write(true);
                            writer.Write(false);
                            writer.Write(true);
                        }

                        packetsToSend.Push(stream.ToArray());
                    }
                }

                // Do stuff every new frame
                await Script.NextFrame();
            }

            sender.Client.Send(new[] { (byte)PacketMagic.Disconnect });
        }
    }
}