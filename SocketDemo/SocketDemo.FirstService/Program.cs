using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SocketDemo.FirstService
{
    class Program
    {
        static void Main(string[] args)
        {
            IPEndPoint ipPoint = new IPEndPoint(IPAddress.Any, 0209);
            Socket serviceSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            serviceSocket.Bind(ipPoint);
            serviceSocket.Listen(58);
            Console.WriteLine("wating for a client...");

            Socket client = serviceSocket.Accept();
            IPEndPoint clientPoint = (IPEndPoint)client.RemoteEndPoint;
            Console.WriteLine(clientPoint.Address + "已连接");
            string defaultContent = "Hello";
            byte[] data = new byte[1024];
            data = Encoding.UTF8.GetBytes(defaultContent);
            client.Send(data, data.Length, SocketFlags.None);
            int receivedLength = 0;
            while (true)
            {
                var receiveData = new byte[1024];
                receivedLength = client.Receive(receiveData);
                if (receivedLength == 0)
                {
                    break;
                }
                var receivedContent = Encoding.UTF8.GetString(receiveData);
                Console.WriteLine(clientPoint.Address + "接收内容:" + receivedContent);
            }
            Console.WriteLine(clientPoint.Address + "已断开");
            client.Shutdown(SocketShutdown.Both);
            client.Close();
        }
    }
}
