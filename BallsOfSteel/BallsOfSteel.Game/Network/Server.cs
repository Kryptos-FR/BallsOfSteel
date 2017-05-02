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
using Gamelogic;

namespace BallsOfSteel
{
    public class Server : AsyncScript
    {
        public struct RemoteClient
        {
            public bool isUsed;
            public IPEndPoint clientIp;
            public Stack<NetworkPlayerInputData> incomingInput;
        }

        public const int MaxPlayerCount = 8;

        private readonly RemoteClient[] clients = new RemoteClient[MaxPlayerCount];
        
        public MainGameLogic GameLogic { get; set; }

        private void Reply(UdpClient client, IPEndPoint distantClient, PacketMagic magic)
        {
            client.SendAsync(new[] { (byte)magic }, 1, distantClient);
        }

        private RemoteClient FindClientByIp(IPEndPoint clientIp)
        {
            for (var i = 0; i < clients.Length; i++)
            {
                if (clients[i].clientIp.Address.Equals(clientIp.Address))
                {
                    return clients[i];
                }
            }
            
            throw new ArgumentOutOfRangeException("Unknown client");
        }

        private bool AllocateClientSlot(IPEndPoint clientInfos)
        {
            for (var i = 0; i < clients.Length; i++)
            {
                if (clients[i].clientIp.Address.Equals(clientInfos.Address))
                {
                    return false;
                }
            }

            for (var i = 0; i < clients.Length; i++)
            {
                if (!clients[i].isUsed)
                {
                    clients[i].clientIp = clientInfos;
                    clients[i].isUsed = true;
                    clients[i].incomingInput = new Stack<NetworkPlayerInputData>();
                    GameLogic.ClientsToRegister.Push(clients[i]);

                    Console.WriteLine(">> BoS Server : Accepted connection request from client {1}", i, clientInfos);

                    return true;
                }
            }

            return false;
        }

        private bool FreeClientSlot(IPEndPoint clientInfos)
        {
            for (var i = 0; i < clients.Length; i++)
            {
                if (clients[i].clientIp.Equals(clientInfos))
                {
                    clients[i].clientIp = clientInfos;
                    clients[i].isUsed = true;
                    GameLogic.ClientsToRemove.Push(clients[i]);

                    Console.WriteLine(">> BoS Server : Received disconnection request from client {1}", i, clientInfos);

                    return true;
                }
            }

            return false;
        }

        public override async Task Execute()
        {
            for (var i = 0; i < MaxPlayerCount; i++)
            {
                clients[i] = new RemoteClient() { clientIp = new IPEndPoint(IPAddress.None, 0) };
            }

            UdpClient listener = new UdpClient(SharedNetwork.ListenPort);
            Console.WriteLine(">> BoS Server : Listening on port {0}", SharedNetwork.ListenPort);

            while (Game.IsRunning)
            {
                var incomingPacket = await listener.ReceiveAsync();

                if (incomingPacket.Buffer.Length != 0)
                {
                    var cmd = (PacketMagic)incomingPacket.Buffer[0];

                    var operationSucceeded = false;
                    switch (cmd)
                    {
                        case PacketMagic.Connect:
                            operationSucceeded = AllocateClientSlot(incomingPacket.RemoteEndPoint);
                            break;

                        case PacketMagic.Disconnect:
                            operationSucceeded = FreeClientSlot(incomingPacket.RemoteEndPoint);
                            break;

                        case PacketMagic.InputMove:
                            using (MemoryStream ms = new MemoryStream(incomingPacket.Buffer))
                            {
                                using (BinaryReader reader = new BinaryReader(ms))
                                {
                                    // Skip Packet Magic Byte
                                    reader.ReadByte();

                                    RemoteClient client = FindClientByIp(incomingPacket.RemoteEndPoint);

                                    NetworkPlayerInputData input = new NetworkPlayerInputData();
                                    input.WalkDirection = new Vector2(reader.ReadSingle(), reader.ReadSingle());
                                    input.FaceDirection = new Vector2(reader.ReadSingle(), reader.ReadSingle());
                                    input.Jump = reader.ReadBoolean();
                                    input.Attack = reader.ReadBoolean();
                                    input.Shoot = reader.ReadBoolean();

                                    client.incomingInput.Push(input);
                                }
                            }
                            break;

                        default:
                            break;
                    }

                    Reply(listener, incomingPacket.RemoteEndPoint, operationSucceeded ? PacketMagic.Success : PacketMagic.Failed);
                }

                // Do stuff every new frame
                await Script.NextFrame();
            }
        }
    }
}