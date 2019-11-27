using System;
using System.Collections.Generic;
using System.Text;

namespace UPnP.Exceptions
{
    public class NoNatDeviceException : Exception
    {
        public NoNatDeviceException()
        {
        }

        public NoNatDeviceException(string message) : base(message)
        {
        }

        public NoNatDeviceException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
