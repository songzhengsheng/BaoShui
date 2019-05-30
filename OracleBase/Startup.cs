using Microsoft.Owin;
using OracleBase;
using Owin;

[assembly: OwinStartup(typeof(OracleBase.Startup))]
namespace OracleBase
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR();//在线聊天
        }
    }
}
