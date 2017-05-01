using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Engine.Events;
using SiliconStudio.Xenko.Input;
using BallsOfSteel.Core;

namespace BallsOfSteel.Player
{
    public interface IControlInput
    {
        // Non-normalized vector with which indicates the walking direction.
        Vector2 WalkDirection { get; }

        // Non-normalized vector with which indicates the facing direction.
        Vector2 FaceDirection { get; }

        // Indicates if the players wants to initiate jump this frame, regardless of if they can
        bool Jump { get; }

        bool Shoot { get; }
    }
}
