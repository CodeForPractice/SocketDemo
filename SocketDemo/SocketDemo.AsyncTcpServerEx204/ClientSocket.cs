using System;
using System.Net.Sockets;

namespace SocketDemo.AsyncTcpServerEx204
{
    /// <summary>
    /// 客户端socket信息
    /// </summary>
    public class ClientSocket
    {
        private Socket _clientSocket;
        private byte[] _rcvBuffer;
        private Guid _id;

        public ClientSocket(Socket clientSocket)
        {
            _clientSocket = clientSocket;
            _id = Guid.NewGuid();
        }

        public Socket Client
        {
            get { return _clientSocket; }
        }

        public byte[] RcvBuffer
        {
            get
            {
                return _rcvBuffer;
            }
        }

        public Guid Id
        {
            get { return _id; }
        }

        public void ClearBuffer()
        {
            _rcvBuffer = new byte[1024];
        }

        public string GetAddrInfo()
        {
            if (_clientSocket != null)
            {
                return $"{_clientSocket.RemoteEndPoint.ToString()}";
            }
            return string.Empty;
        }

        public void Dispose()
        {
            try
            {
                _clientSocket.Shutdown(SocketShutdown.Both);
                _clientSocket.Close();
            }
            finally
            {
                _clientSocket = null;
                _rcvBuffer = null;
            }
        }
    }
}