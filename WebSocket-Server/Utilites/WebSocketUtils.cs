using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using WebSocket_Server.HttpContext;
using log4net;

namespace WebSocket_Server.Server.Utilites
{
    class WebSocketUtils
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(WebSocketUtils));
        public async Task<WebSocketHttpContext> ReadHttpHeaderFromStreamAsync(Stream stream)
        {
            string header = await HttpHelper.ReadHttpHeaderAsync(stream);
            bool isWebsocketRequest = HttpHelper.IsWebSocketRequestUpgrade(header);
            IList<string> subProtocols = HttpHelper.GetSubProtocols(header);
            return new WebSocketHttpContext(isWebsocketRequest,subProtocols,header);
        }

        public async Task AcceptHandShake(string header, Stream stream)
        {
            try
            {
                Regex webSocketKeyRegex = new Regex("Sec-WebSocket-Key: (.*)", RegexOptions.IgnoreCase);

                Match match = webSocketKeyRegex.Match(header);
                if (match.Success)
                {
                    const string eol = "\r\n";
                    string secWebSocketKey = match.Groups[1].Value.Trim();
                    string secWebSocketAccept = HttpHelper.ComputeSocketAcceptString(secWebSocketKey);
                    Byte[] response = Encoding.UTF8.GetBytes("HTTP/1.1 101 Switching Protocols" + eol
                                      + "Connection: Upgrade" + eol
                                      + "Upgrade: websocket" + eol
                                      + $"Sec-WebSocket-Accept: {secWebSocketAccept}" + eol
                                      + eol);

                    stream.Write(response, 0, response.Length);                   
                }                
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot accept handshake");             
            }
            finally
            {
                Console.WriteLine("HandShake Accepted");
            }
        }
    }
}
