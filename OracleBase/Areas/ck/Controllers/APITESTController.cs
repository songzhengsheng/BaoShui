using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MainBLL.Big;
using NFine.Code;
using OracleBase.API;

namespace OracleBase.Areas.ck.Controllers
{
    public class APITESTController : Controller
    {
        // GET: ck/APITEST/PostAddSure
        public ActionResult PostAddSure()
        {
            return View();
        }
        [HttpPost]
        public ActionResult PostAddSure(string a)
        {
            SureEntity model = new SureEntity()
            {
                BLNO = "BLNO",
                GoodsName = "GoodsName",
                HUODAI = "HUODAI"

            };
            string aa=  model.ToJson();
           // var r = HttpMethods.HttpPost("http://58.241.235.76:8099/big/AddSure2", model);
            BigController b=new BigController();
            var r = b.PostAddSure2(model);
            return View();
        }
    }
}