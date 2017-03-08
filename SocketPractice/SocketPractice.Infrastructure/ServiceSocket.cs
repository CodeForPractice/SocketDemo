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
* Create Time         :    2017/3/8 16:38:18
* Class Version       :    v1.0.0.0
* Class Description   :    
* Copyright @yjq 2017 . All rights reserved.
*/

namespace SocketPractice.Infrastructure
{
    public class ServiceSocket
    {
        private Socket _socket;
        private IPEndPoint _listeningEndPoint;
        private SocketAsyncEventArgs _acceptAsyncEventArgs;

        public ServiceSocket(IPEndPoint listeningEndPoint)
        {
            _listeningEndPoint = listeningEndPoint;
            _socket = SocketUtil.CreateSocket(1024, 1024);
            _acceptAsyncEventArgs = new SocketAsyncEventArgs();
            _acceptAsyncEventArgs.Completed += AcceptCompleted;

        }

        public void Start()
        {
            LogUtil.Debug($"开始监听,端口地址:【{_listeningEndPoint}】");

            try
            {
                _socket.Bind(_listeningEndPoint);
                _socket.Listen(5000);
            }
            catch (Exception ex)
            {
                LogUtil.Error($"启动监听失败,端口地址:【{_listeningEndPoint}】", ex);
                ShutDown();
            }

            StartAccepting();
        }

        private void StartAccepting()
        {
            try
            {
                var isAcceptAsync = _socket.AcceptAsync(_acceptAsyncEventArgs);
                if (!isAcceptAsync)//如果是同步处理
                {
                    ProccessAccept(_acceptAsyncEventArgs);
                }
            }
            catch (Exception ex)
            {
                if (!(ex is ObjectDisposedException))
                {
                    LogUtil.Error("socket接收时出现异常，一秒后重新开始接收", ex);
                }
                System.Threading.Thread.Sleep(1000);
                StartAccepting();
            }
        }

        private void AcceptCompleted(object sender, SocketAsyncEventArgs e)
        {
            ProccessAccept(e);
        }

        private void ProccessAccept(SocketAsyncEventArgs e)
        {
            try
            {
                if (e.SocketError == SocketError.Success)
                {
                    var acceptSocket = e.AcceptSocket;
                    e.AcceptSocket = null;////释放上次绑定的Socket，等待下一个Socket连接 
                    OnSocketAccepted(acceptSocket);
                }
                else
                {
                    SocketUtil.ShutDown(e.AcceptSocket);
                    e.AcceptSocket = null;////释放上次绑定的Socket，等待下一个Socket连接 
                }
            }
            catch (ObjectDisposedException) { }
            catch (Exception ex)
            {
                LogUtil.Error("处理时出现异常", ex);
            }
            finally
            {
                StartAccepting();
            }
        }

        private void OnSocketAccepted(Socket acceptSocket)
        {

        }


        public void ShutDown()
        {
            SocketUtil.ShutDown(_socket);
            LogUtil.Error($"关闭监听,端口地址:【{_listeningEndPoint}】");
        }
    }
}
