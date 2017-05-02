namespace BallsOfSteel
{
    public enum PacketMagic : byte
    {
        Unassigned = 0x0,

        // client states
        Connect = 0x1,
        Disconnect = 0x2,

        Success = 0x3,
        Failed = 0x4,

        Ack = 0x5,

        // input states
        InputMove = 0x10,
    }
}