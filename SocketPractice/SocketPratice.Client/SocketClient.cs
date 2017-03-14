using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


/*
* Author              :    yjq
* Email               :    425527169@qq.com
* Create Time         :    2017/3/10 16:29:09
* Class Version       :    v1.0.0.0
* Class Description   :    
* Copyright @yjq 2017 . All rights reserved.
*/

namespace SocketPratice.Client
{
    public sealed class SocketClient
    {
        private static int _totalReceivedCount = 0;
        private int _bufferSize = 60000;
        private const int MessageHeaderSize = 4;

        private Socket _clientSocket;
        private bool _isConnected = false;
        private IPEndPoint _hostEndPoint;

        private SocketAsyncEventArgs _receiveEventArgs;


        public SocketClient(IPEndPoint hostEndPoint)
        {
            _hostEndPoint = hostEndPoint;
            _clientSocket = new Socket(hostEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            _receiveEventArgs = new SocketAsyncEventArgs();
            _receiveEventArgs.UserToken = new AsyncUserToken(_clientSocket);
            _receiveEventArgs.RemoteEndPoint = _hostEndPoint;
            _receiveEventArgs.SetBuffer(new byte[_bufferSize], 0, _bufferSize);
            _receiveEventArgs.Completed += OnReceive;
        }


        public void Connect()
        {
            SocketAsyncEventArgs connectArgs = new SocketAsyncEventArgs();
            connectArgs.UserToken = this._clientSocket;
            connectArgs.RemoteEndPoint = this._hostEndPoint;
            connectArgs.Completed += OnConneted;
            if (!_clientSocket.ConnectAsync(connectArgs))
            {
                Console.WriteLine("同步连接");
                ProcessOnConnected(connectArgs);
            }

            if (!_clientSocket.ReceiveAsync(_receiveEventArgs))
            {
                ProcessReceive(_receiveEventArgs);
            }
        }


        private void OnConneted(object sender, SocketAsyncEventArgs e)
        {
            ProcessOnConnected(e);

        }

        private void ProcessOnConnected(SocketAsyncEventArgs e)
        {
            _isConnected = e.SocketError == SocketError.Success;
            if (_isConnected)
            {
                Console.WriteLine("连接成功");
            }
            else
            {
                Console.WriteLine("连接失败");
                throw new Exception("连接失败");
            }
        }
        public void OnReceive(object sender, SocketAsyncEventArgs e)
        {
            ProcessReceive(e);
        }

        private void ProcessReceive(SocketAsyncEventArgs e)
        {
            if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
            {
                AsyncUserToken userToken = e.UserToken as AsyncUserToken;
                ProcessReceiveData(userToken.DataStartOffset, userToken.NextReceiveOffset - userToken.DataStartOffset + e.BytesTransferred, 0, userToken, e);

                userToken.NextReceiveOffset += e.BytesTransferred;

                //如果达到缓冲区的结尾，则将NextReceiveOffset复位到缓冲区起始位置，并迁移可能需要迁移的未处理的数据
                if (userToken.NextReceiveOffset == e.Buffer.Length)
                {
                    //将NextReceiveOffset复位到缓冲区起始位置
                    userToken.NextReceiveOffset = 0;

                    //如果还有未处理的数据，则把这些数据迁移到数据缓冲区的起始位置
                    if (userToken.DataStartOffset < e.Buffer.Length)
                    {
                        var notYesProcessDataSize = e.Buffer.Length - userToken.DataStartOffset;
                        Buffer.BlockCopy(e.Buffer, userToken.DataStartOffset, e.Buffer, 0, notYesProcessDataSize);

                        //数据迁移到缓冲区起始位置后，需要再次更新NextReceiveOffset
                        userToken.NextReceiveOffset = notYesProcessDataSize;
                    }

                    userToken.DataStartOffset = 0;
                }

                //更新接收数据的缓冲区下次接收数据的起始位置和最大可接收数据的长度
                e.SetBuffer(userToken.NextReceiveOffset, e.Buffer.Length - userToken.NextReceiveOffset);

                if (!userToken.ClientSocket.ReceiveAsync(e))
                {
                    ProcessReceive(e);
                }
            }
        }

        private void ProcessReceiveData(int dataStartOffset, int totalReceivedSize, int alreadyProcessDataSize, AsyncUserToken userToken, SocketAsyncEventArgs e)
        {
            if (alreadyProcessDataSize >= totalReceivedSize)
            {
                return;
            }
            if (userToken.MessageSize == null)
            {
                if (totalReceivedSize > MessageHeaderSize)
                {
                    var headerData = new byte[MessageHeaderSize];
                    Buffer.BlockCopy(e.Buffer, dataStartOffset, headerData, 0, MessageHeaderSize);
                    var messageSize = BitConverter.ToInt32(headerData, 0);
                    userToken.MessageSize = messageSize;
                    userToken.DataStartOffset = dataStartOffset + MessageHeaderSize;
                    ProcessReceiveData(userToken.DataStartOffset, totalReceivedSize, alreadyProcessDataSize + MessageHeaderSize, userToken, e);
                }
            }
            else
            {
                var messageSize = userToken.MessageSize.Value;
                if (totalReceivedSize - alreadyProcessDataSize >= messageSize)
                {
                    var messageData = new byte[messageSize];
                    Buffer.BlockCopy(e.Buffer, dataStartOffset, messageData, 0, messageSize);
                    ProcessMessage(messageData);

                    userToken.MessageSize = null;
                    userToken.DataStartOffset = dataStartOffset + messageSize;
                    ProcessReceiveData(userToken.DataStartOffset, totalReceivedSize, alreadyProcessDataSize + messageSize, userToken, e);
                }
            }
        }

        private void ProcessMessage(byte[] messageData)
        {
            Task.Factory.StartNew(() =>
            {

                int current = Interlocked.Increment(ref _totalReceivedCount);
                if (messageData != null)
                {
                    Console.WriteLine($"接收时间{DateTime.Now.ToString("yyyyMMddHHmmsss")},当前是第{current.ToString()}条信息,信息内容{Encoding.UTF8.GetString(messageData)}");
                }
            });
        }

    }
}
