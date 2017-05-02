// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using SiliconStudio.Core.Collections;

namespace BallsOfSteel
{
    public class Client : AsyncScript
    {
        private string hostIp = "127.0.0.1";
        private bool isConnected = false;
        private FastList<byte[]> packetsToSend = new FastList<byte[]>();

        public XboxInput input;

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

                packetsToSend.Add(stream.ToArray());
            }
        }

        private bool previousAttack;
        private bool previousJump;
        private bool previousShoot;
        private Vector2 previousFaceDirection;
        private Vector2 previousWalkDirection;

        private bool IsInputUpdateNeeded(IControlInput newInputState)
        {
            return previousAttack != newInputState.Attack 
                || previousJump != newInputState.Jump
                || previousShoot != newInputState.Shoot
                || previousFaceDirection != newInputState.FaceDirection
                || previousWalkDirection != newInputState.WalkDirection;
        }

        private bool IsSinglePushUpdateNeeded(IControlInput newInputState)
        {
            return previousAttack != newInputState.Attack
                   || previousJump != newInputState.Jump
                   || previousShoot != newInputState.Shoot;
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
            
            Stopwatch netUpdate = new Stopwatch();
            netUpdate.Start();

            while (Game.IsRunning)
            {
                if (Input.IsKeyPressed(Keys.Escape))
                {
                    break;
                }

                for (var index = 0; index < packetsToSend.Count; index++)
                {
                    sender.Client.Send(packetsToSend[index]);
                }

                packetsToSend.Clear();

                if ((IsInputUpdateNeeded(input) && netUpdate.ElapsedMilliseconds >= 50) || IsSinglePushUpdateNeeded(input))
                {
                    PushInputUpdate(input);

                    netUpdate.Restart();

                    previousAttack = input.Attack;
                    previousJump = input.Jump;
                    previousShoot = input.Shoot;
                    previousFaceDirection = input.FaceDirection;
                    previousWalkDirection = input.WalkDirection;
                }

                // Do stuff every new frame
                await Script.NextFrame();
            }

            sender.Client.Send(new[] { (byte)PacketMagic.Disconnect });
        }
    }
}