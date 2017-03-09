using SocketPractice.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SocketPractice
{
    class Program
    {
        static void Main(string[] args)
        {

            SocketManager m_socket = new SocketManager(200, 1024);
            m_socket.Init();
            m_socket.Start(new IPEndPoint(IPAddress.Any, 13909));

            Console.ReadLine();
        }
    }
}
