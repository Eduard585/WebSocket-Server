using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using MessageServer.Enums;
using WebSocket_Server.Message.Enums;

namespace MessageServer
{
    class PayLoadData
    {
        private byte[] _data;
        private long _length;
        private CloseCode _code;
        private string _reason;

        public static readonly PayLoadData Empty;
        public static readonly ulong MaxLength;
        static PayLoadData()
        {
            Empty = new PayLoadData();
            MaxLength = Int64.MaxValue;
        }

        internal PayLoadData()
        {                        
            _data = new byte[0];
        }

        internal PayLoadData(string message)
            : this(Encoding.UTF8.GetBytes(message))
        {

        }
        internal PayLoadData(byte[] data)
            : this(data, data.LongLength)
        {

        }
        internal PayLoadData(byte[] data, long length)
        {
            _data = data;
            _length = length;
        }

        //Using for creating closing frames
        internal PayLoadData(CloseCode closeCode)
        {
            var code = (ushort)closeCode;
            _code = (CloseCode) code;          
            _data = code.InternalToByteArray(ByteOrder.Big);
        }
            
        internal void Mask(byte[] key)
        {
            for (long i = 0; i < _length; i++)
                _data[i] = (byte)(_data[i] ^ key[i % 4]);
        }

        public ulong Length
        {
            get { return (ulong) _length; }
        }

        public byte[] ToArray()
        {
            return _data;
        }

        public CloseCode GetCloseCode()
        {
            return _code;
        }

        internal void SetClosingCode()
        {
            var closeCode = _data.Take(2).Reverse().ToArray();//TODO why reverse
            if(closeCode.Length > 0)
                _code = (CloseCode) BitConverter.ToUInt16(closeCode);
        }
    }
}
