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

        // Non-normalized vector with which indicates the facing direction.
        public Vector2 FaceDirection
        {
            get
            {
                var face = Input.GetRightThumbAny(DeadZone);

                var moveLength = face.Length();
                var isDeadZoneLeft = moveLength < DeadZone;
                if (isDeadZoneLeft)
                {
                    face = Vector2.Zero;
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

                    face *= moveLength;
                }

                return face;
            }
        }

        // Indicates if the players wants to initiate jump this frame, regardless of if they can
        public bool Jump { get { return false; } }

        public Vector2 WalkDirection
        {
            get
            {
                var walk = Input.GetLeftThumbAny(DeadZone);

                var moveLength = walk.Length();
                var isDeadZoneLeft = moveLength < DeadZone;
                if (isDeadZoneLeft)
                {
                    walk = Vector2.Zero;
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

                    walk *= moveLength;
                }

                return walk;
            }
        }

        public override void Update()
        {

        }
    }
}
