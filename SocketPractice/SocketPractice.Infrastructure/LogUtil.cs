using NLog;
using System;

/*
* Author              :    yjq
* Email               :    425527169@qq.com
* Create Time         :    2017/3/8 15:43:09
* Class Version       :    v1.0.0.0
* Class Description   :
* Copyright @yjq 2017 . All rights reserved.
*/

namespace SocketPractice.Infrastructure
{
    public sealed class LogUtil
    {
        private static Logger GetLogger(string loggerName = null)
        {
            if (string.IsNullOrWhiteSpace(loggerName))
            {
                loggerName = "SocketPractice";
            }
            return LogManager.GetLogger(loggerName);
        }

        /// <summary>
        /// 输出调试日志信息
        /// </summary>
        /// <param name="msg">日志内容</param>
        public static void Debug(string msg, string loggerName = null)
        {
            GetLogger(loggerName: loggerName).Debug(msg);
        }

        /// <summary>
        /// 输出普通日志信息
        /// </summary>
        /// <param name="msg">日志内容</param>
        public static void Info(string msg, string loggerName = null)
        {
            GetLogger(loggerName: loggerName).Info(msg);
        }

        /// <summary>
        /// 输出警告日志
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="loggerName"></param>
        public static void Warn(string msg, string loggerName = null)
        {
            GetLogger(loggerName: loggerName).Warn(msg);
        }

        /// <summary>
        /// 输出警告日志信息
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="memberName"></param>
        /// <param name="loggerName"></param>
        public static void Warn(Exception ex, string memberName = null, string loggerName = null)
        {
            GetLogger(loggerName: loggerName).Warn(ex);
        }

        /// <summary>
        /// 输出错误日志信息
        /// </summary>
        /// <param name="msg">日志内容</param>
        public static void Error(string msg, string loggerName = null)
        {
            GetLogger(loggerName: loggerName).Error(msg);
        }

        /// <summary>
        /// 输出错误日志信息
        /// </summary>
        /// <param name="ex">异常信息</param>
        public static void Error(Exception ex, string memberName = null, string loggerName = null)
        {
            GetLogger(loggerName: loggerName).Error(ex);
        }

        public static void Error(string msg,Exception ex, string memberName = null, string loggerName = null)
        {
            GetLogger(loggerName: loggerName).Error(ex, msg);
        }
    }
}