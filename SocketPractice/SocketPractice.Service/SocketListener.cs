using SocketPractice.Infrastructure;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;


/*
* Author              :    yjq
* Email               :    425527169@qq.com
* Create Time         :    2017/3/22 11:51:59
* Class Version       :    v1.0.0.0
* Class Description   :    
* Copyright @yjq 2017 . All rights reserved.
*/

namespace SocketPractice.Service
{
    public sealed class SocketListener
    {
        private readonly int _maxCount;//监听数
        private const int MESSAGE_HEADER_SIZE = 4;//头部长度
        private readonly int _bufferSize;//缓存流长度

        private Socket _listenerSocket;
        private ConcurrentQueue<MessageData> _messageDataQueue = new ConcurrentQueue<MessageData>();

        public SocketListener(int maxCount, int bufferSize)
        {
            _maxCount = maxCount;
            _bufferSize = bufferSize;


        }

        public void Start(IPEndPoint localEndPoint)
        {
            _listenerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _listenerSocket.ReceiveBufferSize = _bufferSize;
            _listenerSocket.SendBufferSize = _bufferSize;
            _listenerSocket.Bind(localEndPoint);
            _listenerSocket.Listen(_maxCount);

            StartAccept(null);
            Task.Factory.StartNew(() => ProcessSendMessage());
        }

        private void StartAccept(SocketAsyncEventArgs acceptEventArgs)
        {
            if (acceptEventArgs == null)
            {
                acceptEventArgs = new SocketAsyncEventArgs();
                acceptEventArgs.Completed += OnIOCompleted;
            }
            else
            {
                acceptEventArgs.AcceptSocket = null;
            }
            if (!_listenerSocket.AcceptAsync(acceptEventArgs))
            {
                ProcessAccepted(acceptEventArgs);
            }
            LogUtil.Debug("开启监听");
        }

        private void ProcessAccepted(SocketAsyncEventArgs e)
        {
            try
            {
                if (e.SocketError == SocketError.Success)
                {
                    LogUtil.Debug($"{e.AcceptSocket.RemoteEndPoint}已连接");
                    TcpConnection tcpConnection = new TcpConnection(e.AcceptSocket);
                    //SocketAsyncEventArgs receiveEventArgs = new SocketAsyncEventArgs();
                    //receiveEventArgs.UserToken = new AsyncUserToken(e.AcceptSocket);
                    //receiveEventArgs.Completed += OnIOCompleted;
                    //receiveEventArgs.SetBuffer(new byte[_bufferSize], 0, _bufferSize);
                    //if (!e.AcceptSocket.ReceiveAsync(receiveEventArgs))
                    //{
                    //    ProcessReceived(receiveEventArgs);
                    //}
                }
                else
                {
                    CloseSocket(e);
                }
            }
            catch (Exception ex)
            {
                LogUtil.Error(ex);
            }
            StartAccept(e);
        }

        private void ProcessReceived(SocketAsyncEventArgs e)
        {
            if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
            {
                AsyncUserToken userToken = e.UserToken as AsyncUserToken;
                DealReceivedData(e, userToken, 0, userToken.DataStartOffset, userToken.NextReceiveOffset - userToken.DataStartOffset + e.BytesTransferred);
                userToken.NextReceiveOffset += e.BytesTransferred;
                if (userToken.NextReceiveOffset == e.Buffer.Length)
                {
                    int notDealSize = e.Buffer.Length - userToken.DataStartOffset;
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
                CloseSocket(e);
            }
        }

        private void DealReceivedData(SocketAsyncEventArgs e, AsyncUserToken userToken, int dealedSize, int dataOffSet, int totalReceivedSize)
        {
            if (dealedSize >= totalReceivedSize)
            {
                return;
            }
            if (userToken.MessageSize == null && totalReceivedSize > MESSAGE_HEADER_SIZE)
            {
                var messageHeader = new byte[MESSAGE_HEADER_SIZE];
                Buffer.BlockCopy(e.Buffer, dataOffSet, messageHeader, 0, MESSAGE_HEADER_SIZE);
                userToken.MessageSize = BitConverter.ToInt32(messageHeader, 0);
                userToken.DataStartOffset = dataOffSet + MESSAGE_HEADER_SIZE;
                DealReceivedData(e, userToken, dealedSize + MESSAGE_HEADER_SIZE, userToken.DataStartOffset, totalReceivedSize);
            }
            else if (userToken.MessageSize != null)
            {
                int bodyMessageSize = userToken.MessageSize.Value;
                if (totalReceivedSize >= bodyMessageSize + dealedSize)
                {
                    var messageData = new byte[bodyMessageSize];
                    Buffer.BlockCopy(e.Buffer, dataOffSet, messageData, 0, bodyMessageSize);
                    ProcessMessage(messageData, userToken);
                    userToken.DataStartOffset = bodyMessageSize + dataOffSet;
                    userToken.MessageSize = null;
                    DealReceivedData(e, userToken, dealedSize + bodyMessageSize, userToken.DataStartOffset, totalReceivedSize);
                }
            }
        }

        private void ProcessMessage(byte[] data, AsyncUserToken userToken)
        {
            if (data == null || data.Length <= 0) return;
            LogUtil.Info($"接收到来自{userToken.ClientSocket.RemoteEndPoint}信息,信息内容：{Encoding.UTF8.GetString(data)}");

            _messageDataQueue.Enqueue(new Service.MessageData()
            {
                Data = data,
                UserToken = userToken
            });
        }

        private void ProcessSendMessage()
        {
            MessageData messageData = null;
            while (true)
            {
                if (!_messageDataQueue.IsEmpty)
                {
                    if (_messageDataQueue.TryDequeue(out messageData))
                    {
                        var messageByte = BuilderData(messageData.Data);
                        SocketAsyncEventArgs sendEventArgs = new SocketAsyncEventArgs();
                        sendEventArgs.UserToken = messageData.UserToken;
                        sendEventArgs.Completed += OnIOCompleted;
                        sendEventArgs.SetBuffer(messageByte, 0, messageByte.Length);
                        messageData.UserToken.ClientSocket.SendAsync(sendEventArgs);
                    }
                }
                else
                {
                    System.Threading.Thread.Sleep(1);
                }

            }
        }

        private byte[] BuilderData(byte[] data)
        {
            if (data == null || data.Length <= 0) return null;
            int dataLength = data.Length;
            byte[] messageData = new byte[dataLength+MESSAGE_HEADER_SIZE];
            var header = BitConverter.GetBytes(dataLength);
            header.CopyTo(messageData, 0);
            data.CopyTo(messageData, MESSAGE_HEADER_SIZE);
            return messageData;
        }

        private void ProcessSendedMessage(SocketAsyncEventArgs e)
        {
            LogUtil.Debug($"发送一条信息成功");
        }

        private void OnIOCompleted(object sender, SocketAsyncEventArgs e)
        {
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Accept:
                    ProcessAccepted(e);
                    break;
                case SocketAsyncOperation.Receive:
                    ProcessReceived(e);
                    break;
                case SocketAsyncOperation.Send:
                    ProcessSendedMessage(e);
                    break;
                default:
                    break;
            }
        }

        private void CloseSocket(SocketAsyncEventArgs e)
        {
            var userToken = e.UserToken as AsyncUserToken;
            if (userToken != null)
            {
                EndPoint clientEndPoint = userToken.Remote;
                LogUtil.Warn($"开始断开连接与{clientEndPoint}的连接");
                userToken.Dispose();
                LogUtil.Warn($"与{clientEndPoint}的连接断开完毕");
            }
        }

        public void Stop()
        {
            try
            {
                _listenerSocket?.Shutdown(SocketShutdown.Both);
                _listenerSocket.Close();
            }
            catch { }
        }
    }
}
