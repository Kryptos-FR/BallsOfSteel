using SiliconStudio.Core.Mathematics;

namespace BallsOfSteel
{
    public struct ClientCommand
    {
        public PacketMagic Magic;
        public object Data;
    }

    public struct PlayerInputData
    {
        public PacketMagic Magic;
        public Vector2 WalkDirection;
        public Vector2 FaceDirection;
        public bool Jump;
    }

    public static class SharedNetwork
    {
        public const int ListenPort = 8886;
    }
}