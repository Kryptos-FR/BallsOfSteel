using BallsOfSteel.Player;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;

namespace BallsOfSteel
{
    public struct ClientCommand
    {
        public PacketMagic Magic;
        public object Data;
    }

    public class NetworkPlayerInputData
    {
        public int ControllerID { get; set; }

        public Vector2 WalkDirection;
        public Vector2 FaceDirection;

        public bool Jump;
        public bool Attack;
        public bool Shoot;
    }

    public static class SharedNetwork
    {
        public const int ListenPort = 8886;
    }
}
