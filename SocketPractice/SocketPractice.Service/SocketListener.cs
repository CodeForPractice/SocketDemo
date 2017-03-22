using SocketPractice.Infrastructure;
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
        }

        private void ProcessAccepted(SocketAsyncEventArgs e)
        {
            try
            {
                if (e.SocketError == SocketError.Success)
                {
                    LogUtil.Debug($"{e.AcceptSocket.RemoteEndPoint}已连接");
                    SocketAsyncEventArgs receiveEventArgs = new SocketAsyncEventArgs();
                    receiveEventArgs.UserToken = new AsyncUserToken(e.AcceptSocket);
                    receiveEventArgs.Completed += OnIOCompleted;
                    receiveEventArgs.SetBuffer(new byte[_bufferSize], 0, _bufferSize);
                    if (!e.AcceptSocket.ReceiveAsync(receiveEventArgs))
                    {
                        ProcessReceived(receiveEventArgs);
                    }
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

            }
            else
            {
                CloseSocket(e);
            }
        }

        private void DealReceivedData(SocketAsyncEventArgs e, AsyncUserToken userToken,int dealedSize,int dataOffSet,int totalReceivedSize)
        {
            if (dealedSize >= totalReceivedSize)
            {
                return;
            }
            if (userToken.MessageSize == null&& totalReceivedSize>MESSAGE_HEADER_SIZE)
            {
                
            }
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
    }
}
