using System;
using System.Threading;
using System.Threading.Tasks;
using WebSocket_Server;
using WebSocket_Server.Server;
using WebSocket_Server.Session;

namespace DemoServer
{
    class Program
    {
        private static SessionManager _sessionManager = new SessionManager();
        static void Main(string[] args)
        {
            Task task = startServer();
            task.Wait();
        }

        static async Task startServer()
        {
            _sessionManager.OnMessage += ShowMessage;
            var server = new WebSocketServer(80, _sessionManager);
            await server.Start();
        }

        private static void ShowMessage(Guid guid, string message)
        {
            Console.WriteLine($"{guid} "+message);
            _sessionManager.BroadCastMessage(guid,"Hello from server!");
        }
    }
}
