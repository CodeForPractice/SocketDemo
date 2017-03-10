using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SocketPratice.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            SocketClient clientSocket = new SocketClient(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9900));
            clientSocket.Connect();


            Console.ReadLine();
        }
    }
}
