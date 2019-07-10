
namespace WebSocket_Server.Message.Enums
{
    //https://tools.ietf.org/html/rfc6455#section-7.4
    internal enum CloseCode : ushort
    {
        Normal = 1000,
        Away = 1001,
        ProtoError = 1002,
        NotAcceptedData = 1003,
        WrongType = 1008,
        TooBig = 1009,
        UnxCondition = 1011
    }
}
