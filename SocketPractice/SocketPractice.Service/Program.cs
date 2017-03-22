using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SocketPractice.Service
{
    class Program
    {
        static void Main(string[] args)
        {
            SocketListener listener = new SocketListener(6000, 60000);

            listener.Start(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9900));

            Console.ReadLine();
            listener.Stop();
        }
    }
}
