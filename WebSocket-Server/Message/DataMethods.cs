using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using MessageServer.Enums;

namespace MessageServer
{
    public static class DataMethods
    {

        internal static byte[] Append(this ushort code, string reason)
        {
            var ret = code.InternalToByteArray(ByteOrder.Big);
            if (reason != null && reason.Length > 0)
            {
                var buff = new List<byte>(ret);
                buff.AddRange(Encoding.UTF8.GetBytes(reason));
                ret = buff.ToArray();
            }

            return ret;
        }

        internal static byte[] InternalToByteArray(this ushort value, ByteOrder order)
        {
            var bytes = BitConverter.GetBytes(value);
            if(!order.IsHostOrder())
                Array.Reverse(bytes);

            return bytes;
        }

        internal static byte[] InternalToByteArray(this ulong value, ByteOrder order)
        {
            var bytes = BitConverter.GetBytes(value);
            if (!order.IsHostOrder())
                Array.Reverse(bytes);

            return bytes;
        }

        internal static void WriteBytes(this Stream stream, byte[] bytes, int bufferLength)
        {
            using (var input = new MemoryStream(bytes))
                input.CopyTo(stream, bufferLength);
        }

        public static bool IsHostOrder(this ByteOrder order)
        {           
            return !(BitConverter.IsLittleEndian ^ (order == ByteOrder.Little));
        }

        public static byte[] ReadBytes(this Stream stream, int length)
        {
            var buff = new byte[length];
            var offset = 0;
            try
            {
                var nread = 0;
                while (length > 0)
                {
                    nread = stream.Read(buff, offset, length);
                    if (nread == 0) break;

                    offset += nread;
                    length -= nread;
                }
            }
            catch
            {

            }

            return buff;
        }

        public static byte[] ReadBytes(this Stream stream, long length, int bufferLength)
        {
            using (var dest = new MemoryStream())
            {
                try
                {
                    var buff = new byte[bufferLength];
                    var nread = 0;
                    while (length > 0)
                    {
                        if (length < bufferLength)
                            bufferLength = (int)length;

                        nread = stream.Read(buff, 0, bufferLength);
                        if (nread == 0)
                            break;

                        dest.Write(buff, 0, nread);
                        length -= nread;
                        Console.WriteLine(length);
                    }
                }
                catch
                {
                    Console.WriteLine("Cant readbytes");
                }

                dest.Close();
                return dest.ToArray();
            }
        }

        public static ulong ToUInt64(this byte[] source, ByteOrder sourceOrder)
        {
            Array.Reverse(source,0,8);//TODO check why have to reverse
            return BitConverter.ToUInt64(source, 0);
        }         
    }
}
