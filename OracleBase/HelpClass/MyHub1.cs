using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MainBLL.Bulk;
using MainBLL.SysModel;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using NFine.Code;

namespace OracleBase.HelpClass
{
    [HubName("myhub")] //这个HubName很重要
    public class MyHub1 : Hub
    {

        public  List<UserInfo> OnlineUsers = new List<UserInfo>(); // 在线用户列表

        public  void SendMessage(string fatherid, string name,string userid, string message)
        {
            Tb_Lt_Son son=new Tb_Lt_Son()
            {
                FatherID = fatherid.ToInt(),
                UserName = name,
                Addtime = DateTime.Now,
                Info = message,
                UserId = userid
            };

          Clients.All.receiveMessage(name, message, userid, son.ID);
        }

      

       
        /// <summary>
        /// 登录连线
        /// </summary>
        /// <param name="userId">用户Id</param>
        /// <param name="userName">用户名</param>
        public void Register(string userName)
        {
     
           var connnectId = Context.ConnectionId;
         
            if (!OnlineUsers.Any(x => x.ConnectionId == connnectId))
            {
                //添加在线人员
                OnlineUsers.Add(new UserInfo
                {
                    ConnectionId = connnectId,
                    UserName = userName,
                    LastLoginTime = DateTime.Now
                });
            }
            // 所有客户端同步在线用户
            Clients.All.onConnected(connnectId, userName, OnlineUsers);
        }

        public override Task OnConnected()
        {
            Console.WriteLine("客户端连接，连接ID是:{0},当前在线人数为{1}", Context.ConnectionId, OnlineUsers.Count + 1);
            return base.OnConnected();
        }

        /// <summary>
        /// 断线时调用
        /// </summary>
        /// <param name="stopCalled"></param>
        /// <returns></returns>
        public override Task OnDisconnected(bool stopCalled)
        {
      
            var user = OnlineUsers.FirstOrDefault(u => u.ConnectionId == Context.ConnectionId);

            // 判断用户是否存在,存在则删除
            if (user == null)
            {
                return base.OnDisconnected(stopCalled);
            }
            // 删除用户
            OnlineUsers.Remove(user);
            Clients.All.onUserDisconnected(OnlineUsers);   //调用客户端用户离线通知
      

            return base.OnDisconnected(stopCalled);
        }

        /// <summary>
        /// 重新连接时调用
        /// </summary>
        /// <returns></returns>
        public override Task OnReconnected()
        {
            return base.OnReconnected();
        }

    }
}