using System;
using System.Net;
using System.Net.Sockets;

/*
* Author              :    yjq
* Email               :    425527169@qq.com
* Create Time         :    2017/3/10 16:11:53
* Class Version       :    v1.0.0.0
* Class Description   :
* Copyright @yjq 2017 . All rights reserved.
*/

namespace SocketPratice.Client
{
    public sealed class AsyncUserToken : IDisposable
    {
        public Socket ClientSocket { get; set; }

        public EndPoint Remote { get; set; }

        public int? MessageSize { get; set; }

        public int DataStartOffset { get; set; }

        public int NextReceiveOffset { get; set; }

        public AsyncUserToken(Socket serviceSocket)
        {
            ClientSocket = serviceSocket;
            Remote = serviceSocket.RemoteEndPoint;
        }

        public void Dispose()
        {
            try
            {
                this.ClientSocket.Shutdown(SocketShutdown.Send);
                this.ClientSocket.Close();
            }
            catch
            {
            }
        }
    }
}