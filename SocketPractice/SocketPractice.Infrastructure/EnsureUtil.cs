using System;

/*
* Author              :    yjq
* Email               :    425527169@qq.com
* Create Time         :    2017/3/23 13:44:10
* Class Version       :    v1.0.0.0
* Class Description   :
* Copyright @yjq 2017 . All rights reserved.
*/

namespace SocketPractice.Infrastructure
{
    public sealed class EnsureUtil
    {
        private EnsureUtil()
        {
        }

        public static void NotNull(object obj, string argenment)
        {
            if (obj == null)
            {
                throw new ArgumentNullException($"{argenment}不能为空");
            }
        }
    }
}