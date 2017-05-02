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
using BallsOfSteel.Player;

namespace Player
{
    public class NetworkInput : AsyncScript, IControlInput
    {
        // TODO add [DataMemberIgnore] property for the network connection. Should be set externally
        [DataMemberIgnore]
        public Server.RemoteClient client;
        
        public bool Jump => didJump;

        private bool wasJumpDown = false;
        private bool didJump = false;

        public bool Attack => didAttack;

        private bool wasAttackDown = false;
        private bool didAttack = false;

        public Vector2 WalkDirection => walkDirection;

        public Vector2 FaceDirection => faceDirection;

        private Vector2 walkDirection = Vector2.Zero;

        private Vector2 faceDirection = Vector2.Zero;

        public bool Shoot => (faceDirection.LengthSquared() > 0);

        public override async Task Execute()
        {
            while(Game.IsRunning)
            {
                // wait incomingInput
                // update events
                
                // TODO Await client input, cache it in 
                // walkDirection
                // faceDirection
                // didAttack
                // didJump

                // Do stuff every new frame
                await Script.NextFrame();
            }
        }
    }
}
