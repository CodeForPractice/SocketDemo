using System;

/*
* Author              :    yjq
* Email               :    425527169@qq.com
* Create Time         :    2017/3/8 15:31:48
* Class Version       :    v1.0.0.0
* Class Description   :
* Copyright @yjq 2017 . All rights reserved.
*/

namespace SocketPractice.Infrastructure
{
    public sealed class ExUtil
    {
        public static void IgnoreException(Action action)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                LogUtil.Error(ex);
            }
        }

        public static T IgnoreException<T>(Func<T> action, T defaultValue = default(T))
        {
            try
            {
                return action();
            }
            catch (Exception ex)
            {
                LogUtil.Error(ex);
                return defaultValue;
            }
        }
    }
}