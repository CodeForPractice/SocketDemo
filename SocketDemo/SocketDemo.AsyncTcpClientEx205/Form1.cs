using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SocketDemo.AsyncTcpClientEx205
{
    public partial class Form1 : Form
    {
        Socket client = null;
        byte[] rcvBuffer;
        string sendStr;
        delegate void AppendDelegeate(string msg);
        AppendDelegeate appendDel;
        public Form1()
        {
            InitializeComponent();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            appendDel = new AppendDelegeate(AppendMethod);
        }

        private void AppendMethod(string msg)
        {
            lstBoxMsg.Items.Add(msg);
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                client.EndConnect(ar);
                lstBoxMsg.Invoke(appendDel, $"已经成功链接到服务器{client.RemoteEndPoint.ToString()}!");
                lstBoxMsg.Invoke(appendDel, $"本地连接端点为{client.LocalEndPoint.ToString()}!");
                SetRcvCallback();
            }
            catch
            {
                lstBoxMsg.Invoke(appendDel, "无法与远程服务器建立连接");
            }
        }

        private void SetRcvCallback()
        {
            rcvBuffer = new byte[client.SendBufferSize];
            AsyncCallback rcvCallback = new AsyncCallback(ReceiveCallback);
            client.BeginReceive(rcvBuffer, 0, rcvBuffer.Length, SocketFlags.None, rcvCallback, client);
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                int i = client.EndReceive(ar);
                string data = $"收：{Encoding.UTF8.GetString(rcvBuffer, 0, i)}";
                lstBoxMsg.Invoke(appendDel, data);
                SetRcvCallback();
            }
            catch (Exception ex)
            {
                ShutDown(ex.Message);

            }
        }

        private void ShutDown(string msg)
        {
            if (client != null)
            {
                client.Shutdown(SocketShutdown.Both);
                client.Close(50);
                client = null;
            }
            if (!string.IsNullOrWhiteSpace(msg))
            {
                lstBoxMsg.Invoke(appendDel, msg);
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            if (client == null) return;
            if (!client.Connected)
                return;
            ShutDown("断开了与服务器的连接");
        }

        private void SendData()
        {
            try
            {
                byte[] msg = Encoding.UTF8.GetBytes(sendStr);
                AsyncCallback sendCallback = new AsyncCallback(SendCallback);
                client.BeginSend(msg, 0, msg.Length, SocketFlags.None, sendCallback, client);

            }
            catch(Exception ex)
            {
                ShutDown(ex.Message);
            }
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                client.EndSend(ar);
                lstBoxMsg.Invoke(appendDel, $"发：{sendStr}");
            }
            catch(Exception ex)
            {
                ShutDown(ex.Message);
            }
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            if (txtSendMsg.Text.Trim() == "")
            {
                MessageBox.Show("发送内容不能为空");
                return;
            }
            if (client == null)
            {
                MessageBox.Show("请先连接服务器");
                btnConnect.Focus();
                return;
            }
            if (!client.Connected)
            {
                MessageBox.Show("请先连接服务器");
                btnConnect.Focus();
                return;
            }
            sendStr = txtSendMsg.Text.Trim();
            txtSendMsg.Text = "";
            SendData();
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            if (client == null)
            {
                client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            }
            if (!client.Connected)
            {
                IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9009);
                AsyncCallback connectCallback = new AsyncCallback(ConnectCallback);
                client.BeginConnect(remoteEP, connectCallback, client);
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            lstBoxMsg.Items.Clear();
        }
    }
}
