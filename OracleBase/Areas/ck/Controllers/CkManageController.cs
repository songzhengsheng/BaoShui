using Main.HelpClass;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using NFine.Code;
using OracleBase.HelpClass;
using OracleBase.Models;

namespace OracleBase.Areas.ck.Controllers
{


    [SignLoginAuthorize]
    public class CkManageController : Controller
    {
        private Entities db = new Entities();
        // GET: ck/CkManage

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult AddUser()
        {
            return View();
        }
        public ActionResult AddUser_Vue()
        {
            return View();
        }
        public ActionResult GOODSAGENTList()
        {
            return View();
        }

        public ActionResult AddC_GOODSAGENT(int id)
        {
            C_GOODSAGENT model = db.C_GOODSAGENT.Find(id) ?? new C_GOODSAGENT();
            return View(model);
        }
      


    }
}
