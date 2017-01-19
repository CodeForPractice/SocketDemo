using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace SocketDemo.BasicOfSocket
{
    class Program
    {
        static void Main(string[] args)
        {
            // IPAddress ip= IPAddress.Parse("192.168.0.1");

            #region 获取指定主机的IP
            IPAddress[] ips = Dns.GetHostAddresses("www.baidu.com");
            foreach (var item in ips)
            {
                Console.WriteLine(item.ToString());
            }
            #endregion

            #region 获取本地主机名
            //string hostName = Dns.GetHostName();
            //Console.WriteLine(hostName); 
            #endregion

            #region IPHostEntry
            IPHostEntry ipEntry = Dns.GetHostEntry("www.baidu.com");
            for (int i = 0, length = ipEntry.AddressList.Length; i < length; i++)
            {
                Console.WriteLine(ipEntry.AddressList[i]);
            }
            Console.WriteLine(ipEntry.HostName); 
            #endregion
            Console.Read();
        }
    }
}
