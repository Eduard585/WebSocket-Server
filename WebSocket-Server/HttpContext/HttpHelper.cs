using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using WebSocket_Server.Exceptions;

namespace WebSocket_Server.HttpContext
{
    public static class HttpHelper
    {
        private const string HTTP_GET_HEADER_REGEX = @"^GET(.*)HTTP\/1\.1";

        private static readonly ILog _log = LogManager.GetLogger(typeof(HttpHelper));

        public static async Task<string> ReadHttpHeaderAsync(Stream stream)
        {
            int length = 1024 * 16;
            byte[] buffer = new byte[length];
            int offset = 0;
            int bytesRead = 0;

            do
            {
                if (offset >= length)
                {
                    throw new EntityTooLargeException("Http header message too large ro fit in buffer (16KB)");
                }

                bytesRead = await stream.ReadAsync(buffer, offset, length - offset);
                offset += bytesRead;
                string header = Encoding.UTF8.GetString(buffer, 0, offset);

                if (header.Contains("\r\n\r\n"))
                {
                    return header;
                }
            } while (bytesRead > 0);

            return string.Empty;
        }

        public static bool IsWebSocketRequestUpgrade(string header)
        {
            Regex getRegex = new Regex(HTTP_GET_HEADER_REGEX,RegexOptions.IgnoreCase);
            Match getRegexMatch = getRegex.Match(header);

            if (getRegexMatch.Success)
            {
                Regex webSocketUpgradeRegex = new Regex("Upgrade: websocket", RegexOptions.IgnoreCase);
                Match webSocketUpgradeRegexMatch = webSocketUpgradeRegex.Match(header);
                return webSocketUpgradeRegexMatch.Success;
            }

            return false;
        }

        public static IList<string> GetSubProtocols(string header)
        {
            Regex regex = new Regex(@"Sec-WebSocket-Protocol:(?<protocols>.+)", RegexOptions.IgnoreCase);
            Match match = regex.Match(header);

            if (match.Success)
            {
                const int MAX_LEN = 2048;
                if (match.Length > MAX_LEN)
                {
                    throw new EntityTooLargeException($"Sec-WebSocket-Protocol exceeded the maximum of length of {MAX_LEN}");
                }

                // extract a csv list of sub protocols (in order of highest preference first)
                string csv = match.Groups["protocols"].Value.Trim();
                return csv.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.Trim())
                    .ToList();
            }

            return new List<string>();
        }

        public static string ComputeSocketAcceptString(string secWebSocketKey)
        {
            const string webSocketGuid = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
            string concatenated = secWebSocketKey + webSocketGuid;
            byte[] concatenatedAsBytes = Encoding.UTF8.GetBytes(concatenated);

            // note an instance of SHA1 is not threadsafe so we have to create a new one every time here
            byte[] sha1Hash = SHA1.Create().ComputeHash(concatenatedAsBytes);
            string secWebSocketAccept = Convert.ToBase64String(sha1Hash);
            return secWebSocketAccept;
        }
    }
}
