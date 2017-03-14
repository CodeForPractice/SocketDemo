using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketDemo.Sniffer
{
    public class PacketArrivedEventArgs : EventArgs
    {
        private string protocol;//协议
        private string destinationPort;//目标端口
        private string originationPort;//源端口
        private string destinationAddress;//目标地址
        private string originationAddress;//源地址
        private string version;//IP版本号
        private uint totalLenrth;//数据包长度
        private uint messageLength;//数据包中消息长度
        private uint headerLength;//数据包中头长度
        private byte[] receiveBuffer = null;//数据包中数据字节流
        private byte[] headerBuffer = null;//数据包头部数据字节流
        private byte[] messageBuffer = null;//数据包消息数据字节流
        private DateTime date = DateTime.Now;//捕获事件

        public PacketArrivedEventArgs(int receiveLength)
        {
            this.protocol = string.Empty;
            this.destinationPort = string.Empty;
            this.originationPort = string.Empty;
            this.destinationAddress = string.Empty;
            this.originationAddress = string.Empty;
            this.version = string.Empty;

            this.totalLenrth = 0;
            this.messageLength = 0;
            this.headerLength = 0;

            this.receiveBuffer = new byte[receiveLength];
            this.headerBuffer = new byte[receiveLength];
            this.messageBuffer = new byte[receiveLength];
            this.date = DateTime.Now;
        }

        public string Protocol
        {
            get
            { return protocol; }
            set { protocol = value; }
        }

        public string DestinationPort
        {
            get
            {
                return destinationPort;
            }
            set
            {
                destinationPort = value;
            }
        }

        public string OriginationPort
        {
            get
            {
                return originationPort;
            }
            set
            {
                originationPort = value;
            }
        }

        public string DestinationAddress
        {
            get
            {
                return destinationAddress;
            }
            set
            {
                destinationAddress = value;
            }
        }
    }
}
