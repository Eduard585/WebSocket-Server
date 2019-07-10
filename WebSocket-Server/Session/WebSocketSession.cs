using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using MessageServer.Enums;
using WebSocket_Server.HttpContext;
using WebSocket_Server.Message;
using WebSocket_Server.Message.Enums;
using WebSocket_Server.Server;
using WebSocket_Server.Server.Utilites;
using WebSocket_Server.Session;

namespace WebSocket_Server.Transport
{
    internal class WebSocketSession
    {
        private readonly TcpClient _client;
        private readonly WebSocketUtils _utils = new WebSocketUtils();
        internal readonly Guid _guid;
        private readonly MessageProcessor _messageProcessor;
        private  bool _closeMessageSent = false;
        private SessionManager.FrameHandler _onMessage;
        
        protected internal NetworkStream Stream { get; private set; }

        private static readonly ILog _log = LogManager.GetLogger(typeof(WebSocketSession));
        private bool _isDisposed = false;
        public WebSocketSession(TcpClient tcpClient,ref SessionManager.FrameHandler onMessage,Guid guid)
        {
            _client = tcpClient;
            Stream = tcpClient.GetStream();           
            _messageProcessor = new MessageProcessor();
            _onMessage = onMessage;
            _guid = guid;
        }

        public async Task Process()
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            CancellationToken token = cts.Token;

            await ListenToWebSocketRequestAsync(token);
            cts.Dispose();
        }

        internal void SendMessage(string message)
        {
            _messageProcessor.SendTextMessage(Stream, message);
        }

        private void ListenToWebSocketRequest()
        {

        }

        private async Task ListenToWebSocketRequestAsync(CancellationToken token)
        {
            Console.WriteLine($"Client {_guid} connected");
            CancellationTokenSource ctSrc = new CancellationTokenSource();
            CancellationToken ctToken = ctSrc.Token;           
                while (true)
                {
                    try
                    {
                        if (Stream.DataAvailable)
                        {
                            var frame = _messageProcessor.ReadMessage(Stream);
                            MessageRecieved(frame.TextToString());
                            if (frame.Opcode == Opcode.Close && _closeMessageSent == false)
                            {                                                                                                                                                               
                                 _closeMessageSent = true;                                
                                 _messageProcessor.SendClosingFrame(Stream, CloseCode.Normal);                               
                                 CloseSession();
                                 ctSrc.Dispose();
                                 break;                                                                   
                            }                           
                        }
                        else
                        {
                            await Task.Delay(1, token);
                        }
                    }
                    catch
                    {
                        Console.WriteLine("Error occured while listening session");
                    }                    
                }
        }

        private void CloseSession()
        {           
            Stream.Close();
            _client.Close();
            _isDisposed = true;
        }

        private void MessageRecieved(string message)
        {
            try
            {
                _onMessage(_guid, message);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            
        }
    }
}

