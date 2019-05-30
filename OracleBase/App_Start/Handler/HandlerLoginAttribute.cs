using System.Web;
using System.Web.Mvc;
using NFine.Code;

namespace Main.Handler
{
    public class HandlerLoginAttribute : AuthorizeAttribute
    {
        public bool Ignore = true;
        public HandlerLoginAttribute(bool ignore = true)
        {
            Ignore = ignore;
        }
        //public override void OnAuthorization(AuthorizationContext filterContext)
        //{
        //    if (Ignore == false)
        //    {
        //        return;
        //    }
        //    if (OperatorProvider.Provider.GetCurrent() == null)
        //    {
        //        WebHelper.WriteCookie("nfine_login_error", "overdue");
        //        filterContext.HttpContext.Response.Write("<script>location.href = '/Home/Login';</script>");
             
        //        return;
        //    }
        //}
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (OperatorProvider.Provider.GetCurrent() == null)
            {
                WebHelper.WriteCookie("nfine_login_error", "overdue");
                httpContext.Response.Write("<script>location.href = '/Home/Login';</script>");
                return false;
            }
            return true;
        }
     
    }
}