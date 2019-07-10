using System;
using System.Collections.Generic;
using System.Text;
using MessageServer;
using MessageServer.Enums;
using WebSocket_Server.Message.Enums;

namespace WebSocket_Server.Message
{
    static class Message
    {
        private static readonly MessageProcessor _messageProcessor;
        public static WebSocketFrame CreateTextMessage(string message)
        {
            PayLoadData pay = new PayLoadData(message);
            return new WebSocketFrame(Fin.Final, Opcode.Text, pay, false);
        }

        public static WebSocketFrame CreateClosingMessage(CloseCode code)
        {
            PayLoadData pay = new PayLoadData(code);
            return new WebSocketFrame(Fin.Final,Opcode.Close,pay,false);
        }

        public static void CreateFileMessage()
        {

        }


    }
}
