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

namespace SocketDemo.TcpServerEx202
{
    public partial class Form1 : Form
    {
        private Socket serverSocket;
        private Socket clientSocket;
        public Form1()
        {
            InitializeComponent();
            Initialize();
        }

        private void Initialize()
        {
            this.serviceState.Items.Clear();
            this.acceptMess.Text = string.Empty;
            this.sendMess.Text = string.Empty;
            this.txt_serviceIp.Text = "127.0.0.1";
            this.txt_servicePort.Text = "8088";
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            this.btnStart.Enabled = false;
            SetTextEnable(false);
            IPAddress ip = IPAddress.Parse(this.txt_serviceIp.Text);
            IPEndPoint serverPoint = new IPEndPoint(ip, int.Parse(this.txt_servicePort.Text));
            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            serverSocket.Bind(serverPoint);
            serverSocket.Listen(10);
            clientSocket = serverSocket.Accept();
            this.serviceState.Items.Add($"与客户{clientSocket.RemoteEndPoint.ToString()}建立连接");
        }

        private void SetTextEnable(bool isEnabled)
        {
            this.txt_serviceIp.Enabled = isEnabled;
            this.txt_servicePort.Enabled = isEnabled;
        }
    }
}
