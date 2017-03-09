using System;
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
        public List<byte> Buffer { get; set; }

        public AsyncUserToken()
        {
            this.Buffer = new List<byte>();
        }
    }
}