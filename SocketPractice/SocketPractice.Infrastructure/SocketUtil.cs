using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;


/*
* Author              :    yjq
* Email               :    425527169@qq.com
* Create Time         :    2017/3/8 14:43:46
* Class Version       :    v1.0.0.0
* Class Description   :    
* Copyright @yjq 2017 . All rights reserved.
*/

namespace SocketPractice.Infrastructure
{
    public sealed class SocketUtil
    {
        public static IPAddress GetLocalIPV4()
        {
            return Dns.GetHostEntry(Dns.GetHostName()).AddressList.First(m => m.AddressFamily == AddressFamily.InterNetwork);
        }

        public static Socket CreateSocket(int senderBufferSize, int receiveBufferSize)
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.NoDelay = true;
            socket.Blocking = false;
            socket.SendBufferSize = senderBufferSize;
            socket.ReceiveBufferSize = receiveBufferSize;
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);
            return socket;
        }

        public static void ShutDown(Socket socket)
        {
            if (socket == null) return;

            ExUtil.IgnoreException(() =>
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close(10000);
            });

        }

        public static void Close(Socket socket)
        {
            if (socket == null) return;

            ExUtil.IgnoreException(() =>
            {
                socket.Close(10000);
            });

        }
    }
}
