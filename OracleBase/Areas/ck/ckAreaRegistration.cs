using System.Web.Mvc;

namespace OracleBase.Areas.ck
{
    public class ckAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "ck";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "ck_default",
                "ck/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}