using SocketPractice.Infrastructure;
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
            ClientSocket clientSocket = new ClientSocket(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9900));
            clientSocket.Connect();

            Parallel.For(0, 2, i =>
            {
                string message = $"你好！{i.ToString()}";
                var dataByte = Encoding.UTF8.GetBytes(message);
                var headerByte = BitConverter.GetBytes(dataByte.Length);
                var messageByte = new byte[headerByte.Length + dataByte.Length];
                headerByte.CopyTo(messageByte, 0);
                dataByte.CopyTo(messageByte, headerByte.Length);
                clientSocket.SendMessage(messageByte);
            });
            LogUtil.Info("发送完成");
            for (int i = 0; i < 100; i++)
            {
                string message = $"你好！{i.ToString()}";
                var dataByte = Encoding.UTF8.GetBytes(message);
                var headerByte = BitConverter.GetBytes(dataByte.Length);
                var messageByte = new byte[headerByte.Length + dataByte.Length];
                headerByte.CopyTo(messageByte, 0);
                dataByte.CopyTo(messageByte, headerByte.Length);
                clientSocket.SendMessage(messageByte);
            }

            Console.ReadLine();
        }
    }
}
