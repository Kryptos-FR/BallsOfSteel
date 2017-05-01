// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Engine.Events;
using SiliconStudio.Xenko.Input;
using BallsOfSteel.Core;

namespace BallsOfSteel.Player
{
    public class XboxInput : SyncScript, IControlInput
    {
        private bool jumpButtonDown = false;

        public float DeadZone { get; set; } = 0.25f;

        public int ControllerID { get; set; } = 0;

        // Indicates if the players wants to initiate jump this frame, regardless of if they can
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

        public override void Update()
        {
            // Jump
            var isJumpDown = Input.IsGamePadButtonDown(GamePadButton.A, ControllerID) || (Input.GetLeftTrigger(ControllerID) > DeadZone);
            didJump = isJumpDown && !wasJumpDown;
            wasJumpDown = isJumpDown;

            // Jump
            var isAttackDown = Input.IsGamePadButtonDown(GamePadButton.B, ControllerID) || (Input.GetRightTrigger(ControllerID) > DeadZone);
            didAttack = isAttackDown && !wasAttackDown;
            wasAttackDown = isAttackDown;

            // Walking direction
            walkDirection = Input.GetLeftThumb(ControllerID);

            var moveLength = walkDirection.Length();
            var isDeadZoneLeft = moveLength < DeadZone;
            if (isDeadZoneLeft)
            {
                walkDirection = Vector2.Zero;
            }
            else
            {
                if (moveLength > 1)
                {
                    moveLength = 1;
                }
                else
                {
                    moveLength = (moveLength - DeadZone) / (1f - DeadZone);
                }

                walkDirection *= moveLength;
            }

            // Facing direction
            faceDirection = Input.GetRightThumb(ControllerID);

            var faceLength = faceDirection.Length();
            var isDeadZoneRight = faceLength < DeadZone;
            if (isDeadZoneRight)
            {
                faceDirection = Vector2.Zero;
            }
            else
            {
                if (faceLength > 1)
                {
                    faceLength = 1;
                }
                else
                {
                    faceLength = (faceLength - DeadZone) / (1f - DeadZone);
                }

                faceDirection *= faceLength;
            }
        }
    }
}
