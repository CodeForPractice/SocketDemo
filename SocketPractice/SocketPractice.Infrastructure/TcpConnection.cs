using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


/*
* Author              :    yjq
* Email               :    425527169@qq.com
* Create Time         :    2017/3/23 13:41:00
* Class Version       :    v1.0.0.0
* Class Description   :    
* Copyright @yjq 2017 . All rights reserved.
*/

namespace SocketPractice.Infrastructure
{
    /// <summary>
    /// 通信信息
    /// </summary>
    public sealed class TcpConnection
    {
        private Socket _socket;
        private const int BUFFER_SIZE = 60000;

        private EndPoint _localPoint;//本地地址
        private EndPoint _remoteEndPoint;//远端地址

        private SocketAsyncEventArgs _receivedEventArgs;
        private SocketAsyncEventArgs _sendEventArgs;
        private ConcurrentQueue<byte[]> _sendingQueue = new ConcurrentQueue<byte[]>();//待发送列表
        private MemoryStream _sendStream = new MemoryStream();

        private int _isSending = 0;
        private int _isClosed = 0;
        private int isParsing = 0;

        public TcpConnection(Socket socket)
        {
            EnsureUtil.NotNull(socket, "socket");
            _socket = socket;
            _localPoint = _socket.LocalEndPoint;
            _remoteEndPoint = _socket.RemoteEndPoint;

            _receivedEventArgs = new SocketAsyncEventArgs();
            _receivedEventArgs.UserToken = new AsyncUserToken();
            _receivedEventArgs.AcceptSocket = _socket;
            _receivedEventArgs.Completed += OnIOCompleted;

            _sendEventArgs = new SocketAsyncEventArgs();
            _sendEventArgs.AcceptSocket = _socket;
            _sendEventArgs.Completed += OnIOCompleted;
        }

        public Socket Socket { get { return _socket; } }

        public EndPoint LocalEndPoint { get { return _localPoint; } }

        public EndPoint RemoteEndPoint { get { return _remoteEndPoint; } }

        private void OnIOCompleted(object sender, SocketAsyncEventArgs e)
        {
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Receive:
                    ProcessReceived(e);
                    break;
                case SocketAsyncOperation.Send:
                    ProcessSend(e);
                    break;
                default:
                    throw new Exception("找不到对应的LastOperation");

            }
        }

        public void SendMessage(byte[] data)
        {
            _sendingQueue.Enqueue(data);
            TrySend();
        }

        private void ProcessReceived(SocketAsyncEventArgs e)
        {
            if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
            {

            }
            else
            {
                Close();
            }
        }
        private void TryParsing()
        {
            if (_isClosed == 1) return;
            if (EnterParsing())
            {

            }
        }

        private void ProcessSend(SocketAsyncEventArgs e)
        {
            if (e.Buffer != null)
            {
                e.SetBuffer(null, 0, 0);
            }
            ExistSending();
            if (_isClosed == 1) return;
            if (e.SocketError == SocketError.Success)
            {
                TrySend();
            }
            else
            {
                Close();
            }
        }

        private void TrySend()
        {
            if (_isClosed == 1) return;
            if (EnterSending())
            {
                _sendStream.SetLength(0);
                while (!_sendingQueue.IsEmpty)
                {
                    if (_sendStream.Length >= BUFFER_SIZE)
                    {
                        break;
                    }
                    byte[] messageByte = null;
                    if (_sendingQueue.TryDequeue(out messageByte))
                    {
                        _sendStream.Write(messageByte, 0, messageByte.Length);
                    }
                }
                if (_sendStream.Length == 0)
                {
                    ExistSending();
                    if (!_sendingQueue.IsEmpty)
                    {
                        TrySend();
                    }
                    return;
                }
                try
                {
                    var data = _sendStream.GetBuffer();
                    _sendEventArgs.SetBuffer(data, 0, data.Length);
                    if (!_sendEventArgs.AcceptSocket.SendAsync(_sendEventArgs))
                    {
                        ProcessSend(_sendEventArgs);
                    }
                }
                catch (Exception ex)
                {
                    LogUtil.Error(ex);
                    Close();
                }
            }
        }

        private bool EnterSending()
        {
            return Interlocked.CompareExchange(ref _isSending, 1, 0) == 0;
        }

        private void ExistSending()
        {
            Interlocked.Exchange(ref _isSending, 0);
        }

        private bool EnterParsing()
        {
            return Interlocked.CompareExchange(ref isParsing, 1, 0) == 0;
        }

        private void ExistParsing()
        {
            Interlocked.Exchange(ref isParsing, 0);
        }

        private void Close()
        {
            if (Interlocked.CompareExchange(ref _isClosed, 1, 0) == 0)
            {
                if (_sendEventArgs != null)
                {
                    _sendEventArgs.AcceptSocket = null;
                    _sendEventArgs.Dispose();
                }
                if (_receivedEventArgs != null)
                {
                    _receivedEventArgs.AcceptSocket = null;
                    _receivedEventArgs.Dispose();
                }
                LogUtil.Debug($"开始关闭{_localPoint}与{_remoteEndPoint}的连接");
                SocketUtil.ShutDown(_socket);
                LogUtil.Debug($"完成关闭{_localPoint}与{_remoteEndPoint}的连接");
            }
        }
    }
}
