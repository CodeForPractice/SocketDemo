using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace SocketDemo.AsyncTcpServerEx204
{
    public partial class Form1 : Form
    {
        private ConcurrentBag<ClientSocket> clientList = new ConcurrentBag<ClientSocket>();//客户端链接列表
        private bool isStart = false;//是否启动监听
        private TcpListener listener;

        private delegate void AppendMsgDelegate(string msg);//添加消息内容的委托

        private AppendMsgDelegate appendMsg;

        private delegate void AddClientDelegate(ClientSocket client);

        private AddClientDelegate clientAdded;

        private delegate void RemoveClientDelegate(ClientSocket client);

        private RemoveClientDelegate removeClient;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            appendMsg = new AppendMsgDelegate(AppendMsg);
            clientAdded = new AddClientDelegate(ClientAdd);
            removeClient = new RemoveClientDelegate(ClientShutDown);
        }

        private void ClientAdd(ClientSocket client)
        {
            if (client != null)
            {
                clientList.Add(client);
                comboBoxClient.Items.Add(client.GetAddrInfo());
            }
        }

        private void ClientShutDown(ClientSocket client)
        {
            if (client != null)
            {
                clientList.TryTake(out client);
                if (client != null)
                {
                    comboBoxClient.Items.Remove(client.GetAddrInfo());
                    client.Dispose();
                }
            }
        }

        private void AppendMsg(string msg)
        {
            lstBoxStatu.Items.Add(msg);
            lstBoxStatu.SelectedIndex = lstBoxStatu.Items.Count - 1;
            lstBoxStatu.ClearSelected();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (isStart) return;
            IPEndPoint localEP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9009);
            listener = new TcpListener(localEP);
            listener.Start(20);
            StartListening();
        }

        #region 开始监听

        /// <summary>
        /// 开始监听
        /// </summary>
        private void StartListening()
        {
            isStart = true;
            btnStart.Enabled = false;
            lstBoxStatu.Invoke(appendMsg, $"服务器已启动监听,监听端口为：{listener.LocalEndpoint.ToString()}");
            SetAcceptBack();
        }

        #endregion 开始监听

        private void SetAcceptBack()
        {
            AsyncCallback acceptCallBack = new AsyncCallback(AcceptCallback);
            listener.BeginAcceptSocket(acceptCallBack, listener);
        }

        private void AcceptCallback(IAsyncResult ar)
        {
            try
            {
                Socket handle = listener.EndAcceptSocket(ar);
                ClientSocket client = new ClientSocket(handle);
                comboBoxClient.Invoke(clientAdded, client);
                if (isStart)
                {
                    SetAcceptBack();
                }
                client.ClearBuffer();
                SetBeginRcv(client);
            }
            catch
            {
                isStart = false;
            }
        }

        private void SetBeginRcv(ClientSocket client)
        {
            AsyncCallback rcvCallBack = new AsyncCallback(ReceiveCallback);
            client.Client.BeginReceive(client.RcvBuffer, 0, client.RcvBuffer.Length, SocketFlags.None, rcvCallBack, client);
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            ClientSocket client = (ClientSocket)ar.AsyncState;
            try
            {
                int i = client.Client.EndReceive(ar);
                if (i == 0)
                {
                    comboBoxClient.Invoke(removeClient, client);
                    return;
                }
                else
                {
                    string data = Encoding.UTF8.GetString(client.RcvBuffer, 0, i);
                    data = $"From{client.GetAddrInfo()}:{data}";
                    lstBoxStatu.Invoke(appendMsg, data);
                    client.ClearBuffer();
                    SetBeginRcv(client);
                }
            }
            catch
            {
                comboBoxClient.Invoke(removeClient, client);
            }
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            if (!isStart) return;
            listener.Stop();
            isStart = false;
            lstBoxStatu.Invoke(appendMsg, "已经结束服务器的监听");
            btnStart.Enabled = true;
        }

        private void SendData(ClientSocket client, string data)
        {
            if (client == null) return;
            try
            {
                byte[] msg = Encoding.UTF8.GetBytes(data);
                AsyncCallback sendCallback = new AsyncCallback(SendCallback);
                client.Client.BeginSend(msg, 0, msg.Length, SocketFlags.None, SendCallback, client);
                lstBoxStatu.Invoke(appendMsg, data);
            }
            catch
            {
                comboBoxClient.Invoke(removeClient, client);
            }
        }

        private void SendCallback(IAsyncResult ar)
        {
            ClientSocket client = (ClientSocket)ar.AsyncState;
            try
            {
                client.Client.EndSend(ar);
            }
            catch
            {
                comboBoxClient.Invoke(removeClient, client);
            }
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            if (!isStart)
            {
                MessageBox.Show("服务器未启动监听");
                return;
            }
            if (txtSendMsg.Text.Trim() == "")
            {
                MessageBox.Show("发送的内容不能为空");
                return;
            }
            if (comboBoxClient.SelectedIndex < 0)
            {
                MessageBox.Show("请选择发送客户");
                return;
            }
            SendData(clientList.ElementAt(comboBoxClient.SelectedIndex), txtSendMsg.Text);
            txtSendMsg.Text = "";
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            this.lstBoxStatu.Items.Clear();
        }
    }
}