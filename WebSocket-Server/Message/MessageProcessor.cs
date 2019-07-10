using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using MessageServer;
using MessageServer.Enums;
using WebSocket_Server.Message.Enums;
using WebSocket_Server.Message.Frame;
using WebSocket_Server.Session;

namespace WebSocket_Server.Message
{
    class MessageProcessor
    {
        private bool a2 = true;
        

        private const int MAX_FRAME_SIZE = 1024 * 64;//Max size of frame - 512kb. Separate data by frames if more 
       
        public WebSocketFrame ReadMessage(NetworkStream stream)//TODO check if frame is too big and separate
        {
            var a = WebSocketFrame.ReadFrame(stream, true);                      
            return a;
        }
        
        //TODO try to send file

        public bool SendTextMessage(NetworkStream stream, string message)
        {
            Message.CreateTextMessage(message).WriteToStream(stream);
            return true;
        }
        

        public void SendClosingFrame(NetworkStream stream,CloseCode code)
        {           
            try
            {
                Console.WriteLine("Sending close message");
                Message.CreateClosingMessage(code).WriteToStream(stream);   
                Console.WriteLine("Close frame has been sent");
            }
            catch
            {
                Console.WriteLine("Could not send close frame");            
            }            
        }

        

        

       
    }
}
