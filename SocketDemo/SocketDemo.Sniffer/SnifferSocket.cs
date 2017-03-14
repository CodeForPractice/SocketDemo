using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SocketDemo.Sniffer
{
    public class SnifferSocket
    {
        private bool keepRunning;//是否继续捕获标志
        private static int receiveBufferLength;//捕获到的数据流长度
        private byte[] receiveBufferBytes;//收到的字节流
        private Socket socket = null;//套接字对象

        public SnifferSocket()
        {
            receiveBufferLength = 4096;
            receiveBufferBytes = new byte[receiveBufferLength];
        }
        /// <summary>
        /// 是否继续捕获标志
        /// </summary>
        public bool KeepRunning
        {
            get { return keepRunning; }
            set { keepRunning = value; }
        }

        public void CreateAndBindSocket(string ip)
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.IP);
            socket.Blocking = false;//是否处于阻止模式
            socket.Bind(new IPEndPoint(IPAddress.Parse(ip), 0));//使socket与一个本地终结点关联
            SetSocketOption();//设置socket选项
        }

        private void SetSocketOption()
        {
            SniffSocketException ex;
            try
            {
                //将指定的socket选项设置为指定的boolean值
                socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.HeaderIncluded, 1);
                byte[] inValue = new byte[4] { 1,0,0,0};//操作需要的输入数据
                byte[] outValue = new byte[4];//操作返回的输出数据
                //指定执行的操作的控制代码
                int ioControlCode = unchecked((int)0x98000001);
                int returnCode = socket.IOControl(ioControlCode, inValue, outValue);
                returnCode = outValue[0] + outValue[1] + outValue[2] + outValue[3];
                if (returnCode != 0)
                {
                    ex = new SniffSocketException("command execute error");
                    throw ex;
                }
            }
            catch(Exception e)
            {
                ex = new SniffSocketException("socket error", e);
                throw ex;
            }
        }

        /// <summary>
        /// 禁止socket的发送和接受
        /// </summary>
        public void ShutDown()
        {
            if (socket != null)
            {
                socket.Shutdown(SocketShutdown.Both);
            }
        }

        public void Run()
        {
            //socket开始异步监听
            IAsyncResult ar = socket.BeginReceive(receiveBufferBytes, 0, receiveBufferLength, SocketFlags.None, new AsyncCallback(CallReceive), this);
        }

        public void CallReceive(IAsyncResult ar)
        {
            int receiveBytes;
            receiveBytes = socket.EndReceive(ar);//结束挂起的异步读取
            //解析接受的数据包，并引发PacketArrival事件
            Receive(receiveBufferBytes, receiveBytes);
            if (KeepRunning)//是否继续监听
            {
                Run();
            }
        }

        unsafe private void Receive(byte[] buf,int len)
        {
            byte protocol = 0;
            uint version = 0;
            uint ipSourceAddress = 0;
            uint ipDestinationAddress = 0;
            short sourcePort = 0;
            short destinationPort = 0;
            IPAddress ip;
            
        }
    }
}
