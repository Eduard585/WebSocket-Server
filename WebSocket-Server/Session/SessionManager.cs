using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using log4net;
using MessageServer;
using WebSocket_Server.Transport;
using WebSocket_Server.HttpContext;
using WebSocket_Server.Server.Utilites;

namespace WebSocket_Server.Session
{
    public class SessionManager
    {
        public delegate void FrameHandler(Guid guid, string message);

        public event FrameHandler OnMessage;

        private Dictionary<Guid,WebSocketSession> _sessions = new Dictionary<Guid, WebSocketSession>();
        private FrameHandler _handler;
        private readonly WebSocketUtils _utils = new WebSocketUtils();
        private static readonly ILog _log = LogManager.GetLogger(typeof(SessionManager));

        public void BroadCastMessage(List<Guid> guidList,string message)
        {
            for (int i = 0; i < guidList.Count; i++)
            {
                _sessions[guidList[i]].SendMessage(message);
            }
        }

        public void BroadCastMessage(Guid guid, string message)
        {
            _sessions[guid].SendMessage(message);
        }


        private async Task ProcessTcpClientAsync(TcpClient tcpClient)
        {
            try
            {
                Console.WriteLine("Connection opened. Creating new session");
                if (await AcceptWebSocketSession(tcpClient))
                {
                    Guid guid = Guid.NewGuid();
                    WebSocketSession webSocketSession = new WebSocketSession(tcpClient, ref OnMessage, guid);
                    _sessions.Add(guid,webSocketSession);
                    Console.WriteLine("Sessions : " + _sessions.Count);

                    await webSocketSession.Process();//Infinite loop

                    _sessions.Remove(guid);//Remove session from list and closing stream and tcpClient
                    CloseSession(webSocketSession);
                }
                else
                {
                    Console.WriteLine("Accepting failed. Closing connection");
                    tcpClient.Close();
                }
                
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error occured while creating new connection");
            }
            
        }

        internal void ProcessTcpClient(TcpClient tcpClient)
        {
            Task.Run(() => ProcessTcpClientAsync(tcpClient));           
        }

        private void CloseSession(WebSocketSession session)
        {
            session.Stream.Close();
            
        }

        private async Task<bool> AcceptWebSocketSession(TcpClient tcpClient)
        {
            try
            {
                NetworkStream stream = tcpClient.GetStream();
                Console.WriteLine("Reading Http headers from stream");
                WebSocketHttpContext context = await _utils.ReadHttpHeaderFromStreamAsync(stream);
                if (context.IsWebSocketRequest)
                {
                    Console.WriteLine("Requested an upgrade to Websocket protocol.");
                    await _utils.AcceptHandShake(context.HttpHeader, stream);
                    Console.WriteLine("Handshake Accepted. Start listening");
                    return true;
                }

                return false;
            }
            catch
            {
                Console.WriteLine("Error occured while acception WebSocket session");
                return false;
            }
        }
    }
}
