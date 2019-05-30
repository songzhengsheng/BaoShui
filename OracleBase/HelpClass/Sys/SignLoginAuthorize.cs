using System.Collections;
using System.Web;
using System.Web.Mvc;
using NFine.Code;

namespace Main.HelpClass
{
    public class SignLoginAuthorize : AuthorizeAttribute
    {
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            OperatorModel operatorModel = OperatorProvider.Provider.GetCurrent();
            if (operatorModel == null)
            {
                return false;
            }
                var sessionId = operatorModel.UserCode;
                Hashtable userOnline = (Hashtable)(httpContext.Application["Online"]);
                if (userOnline != null)
                {
                    IDictionaryEnumerator idE = userOnline.GetEnumerator();           
                    if (userOnline.Count > 0)
                    {
                        while (idE.MoveNext())
                        {
                            //判断是否登录时保存的session是否与当前页面的sesion相同
                            if (userOnline.Contains(sessionId))
                            {
                                if (idE.Key != null && idE.Key.ToString().Equals(sessionId))
                                {
                                    //判断当前session保存的值是否为被注销值
                                    if (idE.Value != null && "XXXXXX".Equals(idE.Value.ToString()))
                                    {
                                        //验证被注销则清空session
                                        userOnline.Remove(sessionId);
                                        httpContext.Application.Lock();
                                        httpContext.Application["online"] = userOnline;
                                       
                                        httpContext.Response.Write("<script>alert('你的帐号在别处登录，你被强迫下线！');location.href='/Home/Login';</script>");
                                        return false;
                                    }
                                }
                            }
                            else
                            {
                                return false;
                            }
                        }
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
           
            return false;
        }
    }
}