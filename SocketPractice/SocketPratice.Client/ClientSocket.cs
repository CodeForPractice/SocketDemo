using SocketPractice.Infrastructure;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace SocketPratice.Client
{
    public class ClientSocket
    {
        private static long _receivedCount = 0;//已接受次数
        private static long _SendedCount = 0;//已发送次数
        private const int _HeaderLength = 4;//头部长度
        private readonly int _bufferSize = 1024 * 60;//接受流的长度

        private ManualResetEvent _connectedEvent;

        private Socket _clientSocket;
        private IPEndPoint _hostPoint;//服务端地址
        private SocketAsyncEventArgs _receivedArgs;//接收时操作

        public ClientSocket(IPEndPoint hostPoint)
        {
            _hostPoint = hostPoint;
            _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _connectedEvent = new ManualResetEvent(false);

            _receivedArgs = new SocketAsyncEventArgs();
            _receivedArgs.UserToken = new AsyncUserToken(_clientSocket);
            _receivedArgs.RemoteEndPoint = _hostPoint;
            _receivedArgs.SetBuffer(new byte[_bufferSize], 0, _bufferSize);
            _receivedArgs.Completed += ReceivedArgs_Completed;

        }

        private void ReceivedArgs_Completed(object sender, SocketAsyncEventArgs e)
        {
            ProcessReceived(e);
        }

        private void ProcessReceived(SocketAsyncEventArgs e)
        {
            if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
            {

                if (e.BytesTransferred > _HeaderLength)
                {

                }
            }
            else
            {
                ShutDown();
            }
        }

        private void ProcessMessage(byte[] data)
        {
            if (data == null || data.Length <= 0) return;
            long currentCount = Interlocked.Increment(ref _receivedCount);
            LogUtil.Info($"当前接收到第{currentCount.ToString()}条信息,信息内容：{Encoding.UTF8.GetString(data)}");
        }

        #region 与服务端连接以及连接处理

        public ClientSocket Connect()
        {
            SocketAsyncEventArgs connectArgs = new SocketAsyncEventArgs();
            connectArgs.UserToken = _clientSocket;
            connectArgs.RemoteEndPoint = _hostPoint;
            connectArgs.Completed += ConnectArgs_Completed;

            if (!_clientSocket.ConnectAsync(connectArgs))
            {
                ProcessConnected(connectArgs);
            }

            _connectedEvent.WaitOne();

            if (!_clientSocket.ReceiveAsync(_receivedArgs))
            {
                ProcessReceived(_receivedArgs);
            }
            return this;
        }

        private void ConnectArgs_Completed(object sender, SocketAsyncEventArgs e)
        {
            ProcessConnected(e);
        }

        private void ProcessConnected(SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                LogUtil.Info($"与服务端{e.RemoteEndPoint}连接成功！");
            }
            else
            {
                LogUtil.Error($"与服务端{e.RemoteEndPoint}连接失败！");
                throw new Exception($"与服务端{e.RemoteEndPoint}连接失败！");
            }
            _connectedEvent.Set();
        }

        #endregion 与服务端连接以及连接处理


        private void ShutDown()
        {
            LogUtil.Warn($"开始断开连接与{_hostPoint}的连接");
            _clientSocket.Shutdown(SocketShutdown.Both);
            _clientSocket.Close(100);
            LogUtil.Warn($"与{_hostPoint}的连接断开完毕");
        }
    }
}