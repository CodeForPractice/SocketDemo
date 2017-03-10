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
* Create Time         :    2017/3/10 16:29:09
* Class Version       :    v1.0.0.0
* Class Description   :    
* Copyright @yjq 2017 . All rights reserved.
*/

namespace SocketPratice.Client
{
    public sealed class SocketClient
    {
        private int _bufferSize = 60000;
        private const int MessageHeaderSize = 4;

        private Socket _clientSocket;
        private bool _isConnected = false;
        private IPEndPoint _hostEndPoint;


        public SocketClient(IPEndPoint hostEndPoint)
        {
            _hostEndPoint = hostEndPoint;
            _clientSocket = new Socket(hostEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
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

        }


        private void OnConneted(object sender,SocketAsyncEventArgs e)
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
            }
        }
        public void OnReceive(object sender,SocketAsyncEventArgs e)
        {
            ProcessReceive(e);
        }

        private void ProcessReceive(SocketAsyncEventArgs receiveArgs)
        {

        }
    }
}
