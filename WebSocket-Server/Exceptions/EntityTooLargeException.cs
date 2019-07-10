using System;
using System.Collections.Generic;
using System.Text;

namespace WebSocket_Server.Exceptions
{
    public class EntityTooLargeException : Exception
    {
        public EntityTooLargeException() : base()
        {

        }
        public EntityTooLargeException(string message) : base(message)
        {

        }
    }
}
