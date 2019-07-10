using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using log4net;
using MessageServer.Enums;
using WebSocket_Server.Message.Frame;

namespace MessageServer
{
    internal class WebSocketFrame
    {
        #region AutoProperites
        public byte[] ExtendedPayloadLength { get; set; }

        public Fin Fin { get; set; }

        public Mask Mask { get; set; }

        public byte[] MaskingKey { get; set; }

        public Opcode Opcode { get; set; }

        public PayLoadData PayloadData { get; set; }

        public byte PayloadLength { get; set; }

        public Rsv Rsv2 { get; set; }

        public Rsv Rsv3 { get; set; }

        public Rsv Rsv4 { get; set; }

        public static ILog Log => _log;

#endregion
        

        private static readonly ILog _log = LogManager.GetLogger(typeof(WebSocketFrame));
        

        public WebSocketFrame() { }

        //internal WebSocketFrame(string textMessage)
        //    :this()
        public WebSocketFrame(Fin fin, Opcode opcode, PayLoadData payLoadData, bool mask = false)
        {
            Fin = fin;
            Rsv2 = Rsv.Off;
            Rsv3 = Rsv.Off;
            Rsv4 = Rsv.Off;
            Opcode = opcode;

            var len = payLoadData.Length;
            if (len < 126)
            {
                PayloadLength = (byte) len;
                ExtendedPayloadLength = new byte[0];
            }
            else if (len < 65536)
            {
                PayloadLength = 126;
                ExtendedPayloadLength = ((ushort) len).InternalToByteArray(ByteOrder.Big);
            }
            else
            {
                PayloadLength = 127;
                ExtendedPayloadLength = len.InternalToByteArray(ByteOrder.Big);
            }

            if (mask)
            {
                Mask = Mask.On;
            }
            else
            {
                Mask = Mask.Off;
                MaskingKey = new byte[0];
            }

            PayloadData = payLoadData;


        }

        public static WebSocketFrame ReadFrame(Stream stream, bool unmask)
        {
            Console.WriteLine("Reading stream...");         
            var frame = WebSocketFrameReader.processHeader(stream.ReadBytes(2));
            WebSocketFrameReader.readExtPayLoadLength(stream, frame);
            WebSocketFrameReader.readMaskingKey(stream, frame);
            WebSocketFrameReader.readPayloadData(stream, frame);

            if (unmask)
                frame.Unmask();
            if(frame.Opcode == Opcode.Close)
                frame.PayloadData.SetClosingCode();

            return frame;
        }
       
        internal int ExtendedPayloadLengthCount
        {
            get
            {
                return PayloadLength < 126 ? 0 : (PayloadLength == 126 ? 2 : 8);
            }
        }

        internal void Unmask()
        {
            if (Mask == Mask.Off)
                return;

            Mask = Mask.Off;
            PayloadData.Mask(MaskingKey);
            MaskingKey = new byte[0];
        }
       
        private bool IsMasked
        {
            get
            {
                return Mask == Mask.On;
            }
        }

        internal ulong FullPayloadLength
        {
            get
            {
                return PayloadLength < 126
                    ? PayloadLength
                    : PayloadLength == 126
                        ? BitConverter.ToUInt16(ExtendedPayloadLength)
                        : ExtendedPayloadLength.ToUInt64(ByteOrder.Big);
            }
        }

        internal static WebSocketFrame CreateCloseFrame(PayLoadData payLoadData, bool mask)
        {
            return new WebSocketFrame(Fin.Final, Opcode.Close, payLoadData, false);
        }
        
        public bool WriteToStream(NetworkStream stream)
        {
            try
            {
                var binaryWriter = new BinaryWriter(stream);
                var header = (int) Fin;
                header = (header << 1) + (int) Rsv2;
                header = (header << 1) + (int) Rsv3;
                header = (header << 1) + (int) Rsv4;
                header = (header << 4) + (int) Opcode;
                header = (header << 1) + (int) Mask;
                header = (header << 7) + (int) PayloadLength;
                binaryWriter.Write(((ushort) header).InternalToByteArray(ByteOrder.Big));
               
                if (PayloadLength > 125)
                    binaryWriter.Write(ExtendedPayloadLength, 0, PayloadLength == 126 ? 2 : 8);


                if (Mask == Mask.On)
                    binaryWriter.Write(MaskingKey, 0, 4);

                if (PayloadLength > 0)
                {
                    if (PayloadLength < 127)
                        binaryWriter.Write(PayloadData.ToArray(), 0, PayloadLength);
                    else
                        binaryWriter.Write(PayloadData.ToArray(), 0, 1024);
                }
               
                return true;
            }
            catch (Exception ex)
            {
                _log.Error("Unable to write to stream");
                return false;
            }


        }
        
        public byte[] ToArray()
        {
            using (var buff = new MemoryStream())
            {
                var header = (int) Fin;
                header = (header << 1) + (int) Rsv2;
                header = (header << 1) + (int) Rsv3;
                header = (header << 1) + (int) Rsv4;
                header = (header << 4) + (int) Opcode;
                header = (header << 1) + (int) Mask;
                header = (header << 7) + (int) PayloadLength;
                buff.Write(((ushort) header).InternalToByteArray(ByteOrder.Big), 0, 2);

                if (PayloadLength > 125)
                    buff.Write(ExtendedPayloadLength, 0, PayloadLength == 126 ? 2 : 8);

                if (Mask == Mask.On)
                    buff.Write(MaskingKey, 0, 4);

                if (PayloadLength > 0)
                {
                    var bytes = PayloadData.ToArray();
                    if (PayloadLength < 127)
                        buff.Write(bytes, 0, bytes.Length);
                    else
                        buff.WriteBytes(bytes, 1024);
                }

                buff.Close();
                var a = buff.ToArray();
                return a;


            }
        }

        public string TextToString()
        {
            byte[] bytes = PayloadData.ToArray();
            return Encoding.UTF8.GetString(bytes);
        }
    }
}