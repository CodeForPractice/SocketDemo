namespace SocketDemo.TcpServerEx202
{
    partial class Form1
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.txt_serviceIp = new System.Windows.Forms.TextBox();
            this.txt_servicePort = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.serviceState = new System.Windows.Forms.ListBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.acceptMess = new System.Windows.Forms.RichTextBox();
            this.sendMess = new System.Windows.Forms.RichTextBox();
            this.btnStart = new System.Windows.Forms.Button();
            this.btnSend = new System.Windows.Forms.Button();
            this.btnStop = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(37, 49);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "服务器Ip：";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(37, 73);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 12);
            this.label2.TabIndex = 1;
            this.label2.Text = "监听端口：";
            // 
            // txt_serviceIp
            // 
            this.txt_serviceIp.Location = new System.Drawing.Point(108, 46);
            this.txt_serviceIp.Name = "txt_serviceIp";
            this.txt_serviceIp.Size = new System.Drawing.Size(124, 21);
            this.txt_serviceIp.TabIndex = 2;
            // 
            // txt_servicePort
            // 
            this.txt_servicePort.Location = new System.Drawing.Point(108, 73);
            this.txt_servicePort.Name = "txt_servicePort";
            this.txt_servicePort.Size = new System.Drawing.Size(124, 21);
            this.txt_servicePort.TabIndex = 3;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.serviceState);
            this.groupBox1.Location = new System.Drawing.Point(262, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(265, 122);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "服务器状态";
            // 
            // serviceState
            // 
            this.serviceState.FormattingEnabled = true;
            this.serviceState.ItemHeight = 12;
            this.serviceState.Location = new System.Drawing.Point(6, 18);
            this.serviceState.Name = "serviceState";
            this.serviceState.Size = new System.Drawing.Size(241, 88);
            this.serviceState.TabIndex = 0;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(37, 193);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(65, 12);
            this.label3.TabIndex = 5;
            this.label3.Text = "接收信息：";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(37, 278);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(65, 12);
            this.label4.TabIndex = 6;
            this.label4.Text = "发送信息：";
            // 
            // acceptMess
            // 
            this.acceptMess.Location = new System.Drawing.Point(108, 140);
            this.acceptMess.Name = "acceptMess";
            this.acceptMess.Size = new System.Drawing.Size(401, 96);
            this.acceptMess.TabIndex = 7;
            this.acceptMess.Text = "";
            // 
            // sendMess
            // 
            this.sendMess.Location = new System.Drawing.Point(108, 254);
            this.sendMess.Name = "sendMess";
            this.sendMess.Size = new System.Drawing.Size(401, 96);
            this.sendMess.TabIndex = 8;
            this.sendMess.Text = "";
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(39, 396);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(75, 23);
            this.btnStart.TabIndex = 9;
            this.btnStart.Text = "开始监听";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // btnSend
            // 
            this.btnSend.Location = new System.Drawing.Point(209, 396);
            this.btnSend.Name = "btnSend";
            this.btnSend.Size = new System.Drawing.Size(75, 23);
            this.btnSend.TabIndex = 10;
            this.btnSend.Text = "发送消息";
            this.btnSend.UseVisualStyleBackColor = true;
            // 
            // btnStop
            // 
            this.btnStop.Location = new System.Drawing.Point(434, 396);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(75, 23);
            this.btnStop.TabIndex = 11;
            this.btnStop.Text = "停止监听";
            this.btnStop.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(561, 451);
            this.Controls.Add(this.btnStop);
            this.Controls.Add(this.btnSend);
            this.Controls.Add(this.btnStart);
            this.Controls.Add(this.sendMess);
            this.Controls.Add(this.acceptMess);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.txt_servicePort);
            this.Controls.Add(this.txt_serviceIp);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Name = "Form1";
            this.Text = "Tcp服务器";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txt_serviceIp;
        private System.Windows.Forms.TextBox txt_servicePort;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ListBox serviceState;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.RichTextBox acceptMess;
        private System.Windows.Forms.RichTextBox sendMess;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Button btnSend;
        private System.Windows.Forms.Button btnStop;
    }
}

