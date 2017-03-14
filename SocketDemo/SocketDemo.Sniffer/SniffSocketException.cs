using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketDemo.Sniffer
{
    public class SniffSocketException : Exception
    {
        public SniffSocketException() : base() { }

        public SniffSocketException(string message) : base(message) { }

        public SniffSocketException(string message, Exception innerException) : base(message, innerException) { }
    }
}
