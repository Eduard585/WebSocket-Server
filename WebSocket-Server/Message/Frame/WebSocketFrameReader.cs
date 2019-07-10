using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using log4net;
using MessageServer;
using MessageServer.Enums;
using WebSocket_Server.Message.Enums;

namespace WebSocket_Server.Message.Frame
{
    internal static class WebSocketFrameReader
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(WebSocketFrameReader));
        private static readonly byte[] _emptyBytes = new byte[0];
       
        public static WebSocketFrame processHeader(byte[] header)
        {
            if (header.Length != 2)
                throw new Exception("The header of a frame cannot be read from the stream.");


            var fin = (header[0] & 0x80) == 0x80 ? Fin.Final : Fin.More;
            var rsv1 = (header[0] & 0x40) == 0x40 ? Rsv.On : Rsv.Off;
            var rsv2 = (header[0] & 0x20) == 0x20 ? Rsv.On : Rsv.Off;
            var rsv3 = (header[0] & 0x10) == 0x10 ? Rsv.On : Rsv.Off;
            var opcode = (Opcode)(byte)(header[0] & 0x0f);
            var mask = (header[1] & 0x80) == 0x80 ? Mask.On : Mask.Off;
            var payloadLen = (byte)(header[1] & 0x7f);
          
            var frame = new WebSocketFrame
            {
                Fin = fin,
                Rsv2 = rsv1,
                Rsv3 = rsv2,
                Rsv4 = rsv3,
                Opcode = opcode,
                Mask = mask,
                PayloadLength = payloadLen
            };
            
            return frame;
        }   
        public static WebSocketFrame readExtPayLoadLength(Stream stream,WebSocketFrame frame)
        {
            var len = frame.PayloadLength < 126 ? 0 : (frame.PayloadLength == 126 ? 2 : 8);
            if (len == 0)
            {
                frame.ExtendedPayloadLength = _emptyBytes;
                return frame;
            }

            var bytes = stream.ReadBytes(len);
             
            if (bytes.Length != len)
                throw new Exception(
                    "The extended payload length of a frame cannot be read from the stream.");

            var a = Encoding.UTF8.GetString(bytes);
            frame.ExtendedPayloadLength = bytes;
            return frame;
        }

        public static WebSocketFrame readMaskingKey(Stream stream, WebSocketFrame frame)
        {
            var len = frame.Mask == Mask.On ? 4 : 0;
            if (len == 0)
            {
                frame.MaskingKey = _emptyBytes;
                return frame; 
            }

            var bytes = stream.ReadBytes(len);
            if (bytes.Length != len)
                throw new Exception("The masking key of a frame cannot be read from the stream.");

            frame.MaskingKey = bytes;
            return frame;
        }
      
        public static WebSocketFrame readPayloadData (Stream stream, WebSocketFrame frame)
        {
            try
            {
                var len = frame.FullPayloadLength;
                if (len == 0)
                {
                    frame.PayloadData = PayLoadData.Empty;
                    return frame;
                }

                if (len > PayLoadData.MaxLength)
                    throw new Exception("A frame has a long payload length.");

                var llen = (long) len;
                var bytes = frame.PayloadLength < 127
                    ? stream.ReadBytes((int)len)
                    : stream.ReadBytes(llen, 1024);
                if (bytes.LongLength != llen)
                    throw new Exception(
                        "The payload data of a frame cannot be read from the stream.");
                //if (frame.Opcode != Opcode.Close)
                //{                    
                    frame.PayloadData = new PayLoadData(bytes, llen);
                //}
                //else
                //{
                //    var closeCode = BitConverter.ToUInt16(bytes.Take(2).Reverse().ToArray());//TODO why reverse
                //    frame.PayloadData = new PayLoadData((CloseCode)closeCode,"");
                //}                              
                return frame;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error occured while reading PayloadData");
                throw ex;
            }
          
        }

        
        
    }

   
}
