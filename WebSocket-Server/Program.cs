using System;
using System.Threading;
using System.Threading.Tasks;
using WebSocket_Server.Server;
using WebSocket_Server.Session;

namespace WebSocket_Server
{
    class Program
    {
        private static SessionManager sessionManager = new SessionManager();
        static void Main(string[] args)
        {
            Task task = start();
            task.Wait();

        }

        static async Task start()
        {
                                 
            sessionManager.OnMessage += Show_message;
            var server = new WebSocketServer(80, sessionManager);
            await server.Start();
            
        }

        private static void Show_message(Guid guid, string message)
        {
            Console.WriteLine($"{guid}" + message);
            sessionManager.BroadCastMessage(guid,"Hi,user");
        }
    }
}
