using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SocketDemo.FirstCustomer
{
    class Program
    {
        static void Main(string[] args)
        {
            Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Console.WriteLine("请输入服务端Ip:");
            string serviceIp = Console.ReadLine();
            Console.WriteLine("请输入服务端端口:");
            int servicePort = Convert.ToInt32(Console.ReadLine());
            IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse(serviceIp), servicePort);
            try
            {
                client.Connect(ipPoint);
            }
            catch(SocketException ex)
            {
                Console.WriteLine(ex.ToString());
            }
            byte[] receiveBytes = new byte[1024];
            int receivedLength = client.Receive(receiveBytes);
            string receivedContent = Encoding.UTF8.GetString(receiveBytes, 0, receivedLength);
            Console.WriteLine(receivedContent);
            while (true)
            {
                Console.WriteLine("请输入要发送的内容：");
                string input = Console.ReadLine();
                if (string.Equals(input, "exists", StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }
                client.Send(Encoding.UTF8.GetBytes(input));
                receivedLength = client.Receive(receiveBytes);
                receivedContent = Encoding.ASCII.GetString(receiveBytes, 0, receivedLength);
                Console.WriteLine(receivedContent);
            }
            Console.WriteLine("disconnect from sercer");
            client.Shutdown(SocketShutdown.Both);
            client.Close();
        }
    }
}
