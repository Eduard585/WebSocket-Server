using System;
using System.Collections.Generic;
using System.Text;

namespace WebSocket_Server.Server
{
    public class WebSocketServerOptions
    {
        public TimeSpan KeepAliveInterval { get; set; }

        public string SubProtocol { get; set; }

        public WebSocketServerOptions()
        {
            KeepAliveInterval = TimeSpan.FromSeconds(60);
            SubProtocol = null;
        }
    }
}
