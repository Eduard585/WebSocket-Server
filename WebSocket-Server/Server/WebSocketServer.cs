using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebSocket_Server.Session;
using log4net;
using log4net.Repository.Hierarchy;
using WebSocket_Server.Message;

namespace WebSocket_Server.Server
{
    public class WebSocketServer : IDisposable
    {
        private static TcpListener tcpListener;
        private readonly SessionManager _sessionManager;
        private readonly MessageProcessor _messageProcessor;      
        
        private int _port = 80;
        private bool _isDisposed = false;
        private static readonly ILog _log = LogManager.GetLogger(typeof(WebSocketServer));
        public WebSocketServer(int port, SessionManager sessionManager)
        {
            Console.WriteLine("Initializing WebSocket-Server");
            _port = port;
            _sessionManager = sessionManager;
        }

        public async Task Start()
        {
            try
            {
                IPAddress ipAdd = IPAddress.Any;
                tcpListener = new TcpListener(ipAdd, 80);
                tcpListener.Start();
                Console.WriteLine("Server has started\nWaiting for a connection");
                await Listen();
                Console.WriteLine("Start listening");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Server is shouting down");
                Dispose();
            }
        }

        private async Task Listen()
        {
            try
            {
                while (true)
                {
                    TcpClient tcpClient = await tcpListener.AcceptTcpClientAsync();
                    _sessionManager.ProcessTcpClient(tcpClient);
                }
            }
            catch (Exception ex)
            {
                _log.Error("Error occured while listening");
            }
            finally
            {
                Console.WriteLine("Server stopped listening");
            }
        }
             
        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
