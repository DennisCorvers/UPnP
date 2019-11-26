using System;
using System.Collections.Generic;
using System.Text;

namespace UPnP.Exceptions
{
    public class AlreadyWorkingException : Exception
    {
        public AlreadyWorkingException()
        {
        }

        public AlreadyWorkingException(string message) : base(message)
        {
        }

        public AlreadyWorkingException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
