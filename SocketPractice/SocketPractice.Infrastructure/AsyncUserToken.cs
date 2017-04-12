using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

/*
* Author              :    yjq
* Email               :    425527169@qq.com
* Create Time         :    2017/3/9 17:07:23
* Class Version       :    v1.0.0.0
* Class Description   :
* Copyright @yjq 2017 . All rights reserved.
*/

namespace SocketPractice.Infrastructure
{
    public sealed class AsyncUserToken
    {
        private ConcurrentQueue<byte[]> _buffer;
        /// <summary>
        /// 客户端IP地址
        /// </summary>
        public IPAddress IPAddress { get; set; }

        /// <summary>
        /// 远程地址
        /// </summary>
        public EndPoint Remote { get; set; }

        /// <summary>
        /// 通信SOKET
        /// </summary>
        public Socket Socket { get; set; }

        /// <summary>
        /// 连接时间
        /// </summary>
        public DateTime ConnectTime { get; set; }

        /// <summary>
        /// 所属用户信息
        /// </summary>
        //public UserInfoModel UserInfo { get; set; }

        /// <summary>
        /// 数据缓存区
        /// </summary>
        public ConcurrentQueue<byte[]> Buffer { get { return _buffer; } }

        public AsyncUserToken()
        {
            _buffer = new ConcurrentQueue<byte[]>();
        }

        public void AddBuffer(byte[] data)
        {
            if (data == null) return;
            _buffer.Enqueue(data);
        }
    }
}