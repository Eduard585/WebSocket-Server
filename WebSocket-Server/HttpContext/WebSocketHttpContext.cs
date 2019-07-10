using System;
using System.Collections.Generic;
using System.Text;

namespace WebSocket_Server.HttpContext
{
    class WebSocketHttpContext
    {
        public bool IsWebSocketRequest { get; private set; }
        public IList<string> WebSocketRequestedProtocols { get; private set; }
        public string HttpHeader { get; private set; }

        public WebSocketHttpContext(bool isWebSocketRequest, IList<string> webSocketRequestedProtocols,
            string httpHeader)
        {
            IsWebSocketRequest = isWebSocketRequest;
            WebSocketRequestedProtocols = webSocketRequestedProtocols;
            HttpHeader = httpHeader;
        }
    }
}
