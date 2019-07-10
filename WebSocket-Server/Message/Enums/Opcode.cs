

namespace MessageServer.Enums
{
    internal enum Opcode : byte
    {
        Cont = 0x0,//Continuation frame
        Text = 0x1,//Text frame
        Binary = 0x2,//Binary frame
        Close = 0x8,//Close frame(opcode 1000 in binary)
        Ping = 0x9,
        Pong = 0xa
    }
}
