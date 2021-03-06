﻿using SocketPractice.Infrastructure;
using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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

        private SocketAsyncEventArgs _sendArgs;//发送时操作
        private int _sending = 0;
        private ConcurrentQueue<byte[]> _sendMessageQueue = new ConcurrentQueue<byte[]>();

        private ConcurrentStack<SocketAsyncEventArgs> _sendEventArgsList = new ConcurrentStack<SocketAsyncEventArgs>();

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

            _sendArgs = new SocketAsyncEventArgs();
            _sendArgs.UserToken = _clientSocket;
            _sendArgs.RemoteEndPoint = _hostPoint;
            _sendArgs.Completed += SendArgs_Completed;

            for (int i = 0; i < 50; i++)
            {
                SocketAsyncEventArgs sendArgs = new SocketAsyncEventArgs();
                sendArgs.UserToken = _clientSocket;
                sendArgs.RemoteEndPoint = _hostPoint;
                sendArgs.Completed += SendArgs_Completed;
                _sendEventArgsList.Push(sendArgs);
            }
        }

        private void SendArgs_Completed(object sender, SocketAsyncEventArgs e)
        {
            ProcessSend(e);
        }

        private void ProcessSend(SocketAsyncEventArgs e)
        {
            long currentSendCount = Interlocked.Increment(ref _SendedCount);
            LogUtil.Debug($"当前发送了{currentSendCount.ToString()}条信息");
            if (e == _sendArgs)
            {
                ExistSend();
            }
            else
            {
                _sendEventArgsList.Push(e);
            }
            SendMessage();
        }
        public void SendMessage(byte[] messageData)
        {
            if (messageData == null || messageData.Length == 0) return;
            _sendMessageQueue.Enqueue(messageData);
            SendMessage();
        }

        private void SendMessage()
        {

            if (!_sendMessageQueue.IsEmpty)
            {
                SocketAsyncEventArgs sendEventArg = null;
                if (_sendEventArgsList.TryPop(out sendEventArg))
                {
                    LogUtil.Warn("采用缓冲发送对象发送");
                    SendMessage(sendEventArg);
                }
                else if (EnteringSend())
                {
                    SendMessage(_sendArgs);
                }
            }
        }
        private void SendMessage(SocketAsyncEventArgs e)
        {
            byte[] messageBytes = null;
            if (_sendMessageQueue.TryDequeue(out messageBytes))
            {
                if (messageBytes != null)
                {
                    e.SetBuffer(messageBytes, 0, messageBytes.Length);
                    if (!_clientSocket.SendAsync(e))
                    {
                        ProcessSend(_sendArgs);
                    }
                }
            }
        }


        private bool EnteringSend()
        {
            bool isEntered = Interlocked.CompareExchange(ref _sending, 1, 0) == 0;
            if (isEntered)
            {
                LogUtil.Debug("进入发送信息成功!");
            }
            else
            {
                LogUtil.Warn("进入发送信息失败!");
            }
            return isEntered;
        }

        private void ExistSend()
        {
            Interlocked.Exchange(ref _sending, 0);
        }

        #region 接收处理

        private void ReceivedArgs_Completed(object sender, SocketAsyncEventArgs e)
        {
            ProcessReceived(e);
        }

        private void ProcessReceived(SocketAsyncEventArgs e)
        {
            if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
            {
                AsyncUserToken userToken = e.UserToken as AsyncUserToken;
                DealReceivedData(e, userToken, userToken.DataStartOffset, 0, userToken.NextReceiveOffset - userToken.DataStartOffset + e.BytesTransferred);
                userToken.NextReceiveOffset += e.BytesTransferred;
                if (userToken.NextReceiveOffset == e.Buffer.Length)//如果到达缓冲区尾部
                {
                    int notDealSize = e.Buffer.Length - userToken.DataStartOffset;//判断是否还有未处理的流
                    if (notDealSize > 0)
                    {
                        Buffer.BlockCopy(e.Buffer, userToken.DataStartOffset, e.Buffer, 0, notDealSize);
                    }
                    userToken.DataStartOffset = 0;
                    userToken.NextReceiveOffset = notDealSize;
                }

                e.SetBuffer(userToken.NextReceiveOffset, e.Buffer.Length - userToken.NextReceiveOffset);

                if (!userToken.ClientSocket.ReceiveAsync(e))
                {
                    ProcessReceived(e);
                }
            }
            else
            {
                ShutDown();
            }
        }

        private void DealReceivedData(SocketAsyncEventArgs e, AsyncUserToken userToken, int dataStartOffset, int dealedSize, int totalReceivedSize)
        {
            if (dealedSize >= totalReceivedSize)
            {
                return;
            }
            if (userToken.MessageSize == null)
            {
                if (totalReceivedSize > _HeaderLength)
                {
                    var messageData = new byte[_HeaderLength];
                    Buffer.BlockCopy(e.Buffer, dataStartOffset, messageData, 0, _HeaderLength);
                    userToken.MessageSize = BitConverter.ToInt32(messageData, 0);
                    userToken.DataStartOffset = dataStartOffset + _HeaderLength;
                    DealReceivedData(e, userToken, userToken.DataStartOffset, dealedSize + _HeaderLength, totalReceivedSize);
                }
            }
            else
            {
                int bodyMessageSize = userToken.MessageSize.Value;
                if (totalReceivedSize >= dealedSize + bodyMessageSize)
                {
                    var messageData = new byte[bodyMessageSize];
                    Buffer.BlockCopy(e.Buffer, dataStartOffset, messageData, 0, bodyMessageSize);
                    ProcessMessage(messageData);
                    userToken.DataStartOffset = dataStartOffset + bodyMessageSize;
                    userToken.MessageSize = null;
                    DealReceivedData(e, userToken, userToken.DataStartOffset, dealedSize + bodyMessageSize, totalReceivedSize);
                }
            }
        }

        private void ProcessMessage(byte[] data)
        {
            if (data == null || data.Length <= 0) return;
            long currentCount = Interlocked.Increment(ref _receivedCount);
            LogUtil.Info($"当前接收到第{currentCount.ToString()}条信息,信息内容：{Encoding.UTF8.GetString(data)}");
        }

        #endregion 接收处理

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