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
        private int _isReceiving = 0;

        private MemoryStream receivedStream = new MemoryStream();

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
            TryReceived();
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

        private void TryReceived()
        {
            if (EnteringReceive())
            {
                Console.WriteLine("=========");
                _receivedEventArgs.SetBuffer(new byte[BUFFER_SIZE], 0, BUFFER_SIZE);
                if (!_receivedEventArgs.AcceptSocket.ReceiveAsync(_receivedEventArgs))
                {
                    ProcessReceived(_receivedEventArgs);
                }
            }
        }

        private bool EnteringReceive()
        {
            return Interlocked.CompareExchange(ref _isReceiving, 1, 0) == 0;
        }

        private void ExistReceive()
        {
            Interlocked.Exchange(ref _isReceiving, 0);
        }

        private void ProcessReceived(SocketAsyncEventArgs e)
        {
            if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
            {
                AsyncUserToken userToken = e.UserToken as AsyncUserToken;
                var messageData = new byte[e.BytesTransferred];
                Buffer.BlockCopy(e.Buffer, 0, messageData, 0, e.BytesTransferred);
                userToken.AddBuffer(messageData);
                TryParsing(userToken);
                ExistReceive();
                TryReceived();
            }
            else
            {
                Close();
            }
        }
        private static int _HeaderSize = 4;
        private int _parsedHeaderSize = 0;
        private int _parsedBodySize = 0;
        private int _parsedIndex = 0;
        private byte[] _package;

        private void TryParsing(AsyncUserToken userToken)
        {
            if (_isClosed == 1) return;
            if (EnterParsing())
            {
                Console.WriteLine("进入解析");
                byte[] messageData = null;
                while (!userToken.Buffer.IsEmpty)
                {
                    if (userToken.Buffer.TryDequeue(out messageData))
                    {
                        receivedStream.Write(messageData, 0, messageData.Length);
                    }
                }
                if (receivedStream.Length == 0)
                {
                    ExistParsing();
                    TryParsing(userToken);
                }
                var messageByte = receivedStream.ToArray();
                for (int i = 0; i < messageByte.Length; i++)
                {
                    if (_parsedHeaderSize < _HeaderSize)
                    {
                        _parsedBodySize |= (messageByte[i] << (_parsedHeaderSize * 8));// little-endian order
                        _parsedHeaderSize++;
                        if (_parsedHeaderSize == _HeaderSize)
                        {
                            _package = new byte[_parsedBodySize];
                        }
                    }
                    else
                    {
                        //获取需要拷贝的长度
                        int needCopyLength = Math.Min(_parsedBodySize - _parsedIndex, messageByte.Length - i);
                        Buffer.BlockCopy(messageByte, i, _package, _parsedIndex, needCopyLength);
                        _parsedIndex += needCopyLength;
                        i += needCopyLength - 1;
                        if (_parsedIndex == _parsedBodySize)
                        {
                            Console.WriteLine("接收到:" + Encoding.UTF8.GetString(_package));
                            SendMessage(BuilderData(_package));
                            _parsedHeaderSize = 0;
                            _parsedBodySize = 0;
                            _parsedIndex = 0;
                            _package = null;
                        }
                    }
                }
                ExistParsing();
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
                    var data = _sendStream.ToArray();
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

        private byte[] BuilderData(byte[] data)
        {
            if (data == null || data.Length <= 0) return null;
            int dataLength = data.Length;
            byte[] messageData = new byte[dataLength + _HeaderSize];
            var header = BitConverter.GetBytes(dataLength);
            header.CopyTo(messageData, 0);
            data.CopyTo(messageData, _HeaderSize);
            return messageData;
        }
    }
}
