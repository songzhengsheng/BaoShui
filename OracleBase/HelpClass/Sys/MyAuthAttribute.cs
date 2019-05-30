using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using OracleBase.Models;

namespace Main.HelpClass
{
    public class MyAuthAttribute:AuthorizeAttribute
    {
        private Entities db = new Entities();
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (httpContext.Session != null)
            {
                Sys_User model=(Sys_User)httpContext.Session["LoginUser"];
                var userName = model.userName;
                string[] strRoles = Roles.Split(',');
                if (string.IsNullOrWhiteSpace(Roles))
                {
                    return false;
                }
                if (strRoles.Length > 0 && JudgeAuthorize(userName, strRoles))
                {
                    return true;
                }
                return false;
            }
            return false;
           
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            base.HandleUnauthorizedRequest(filterContext);
            if (filterContext == null)
            {
                throw new ArgumentNullException("filterContext");
            }
            else
            {
                filterContext.HttpContext.Response.Redirect("/home/NoAuthorize");
            }
            base.HandleUnauthorizedRequest(filterContext);
        }

        private bool JudgeAuthorize(string userName,string[] strRoles)
        {
            return strRoles.Contains(Roid(userName), StringComparer.OrdinalIgnoreCase);
        }

        private string Roid(string userName)
        {

              Sys_User user = db.Sys_User.FirstOrDefault(n => n.userName == userName);
                if (user != null)
                {
                    return user.roleId.ToString();
                }
                return null;
            
        }

    }
}