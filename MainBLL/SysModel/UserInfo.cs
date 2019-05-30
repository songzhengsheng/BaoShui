using System;

namespace MainBLL.SysModel
{
    /// <summary>
    ///在线聊天时候使用
    /// </summary>
    public class UserInfo
    {
        public string ConnectionId { get; set; }
        public string UserName { get; set; }
        public DateTime LastLoginTime { get; set; }
 
    }
}