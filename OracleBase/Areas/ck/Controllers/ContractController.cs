using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Migrations;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Main.HelpClass;
using NFine.Code;
using NFine.Code.Select;
using OracleBase.HelpClass.Sys;
using OracleBase.Models;


namespace OracleBase.Areas.ck.Controllers
{
    /// <summary>
    /// 合同管理 Contract
    /// </summary>
    [SignLoginAuthorize]
    public class ContractController : Controller
    {
        private Entities db = new Entities();
        // GET: ck/Contract
        public ActionResult Index()
        {

            return View();
        }
        [System.Web.Http.HttpGet]
        public object GetContractList(int limit, int offset, string ContoractNumber, string EntrustPeople)
        {
            OperatorModel loginModel = OperatorProvider.Provider.GetCurrent();
            var yuanquid = loginModel.YuanquID ?? throw new ArgumentNullException("loginModel.YuanquID");

            var wherelambda = ExtLinq.True<C_TB_HC_CONTRACT>();
            wherelambda = wherelambda.And(t => t.YuanQuID == yuanquid);
            //if (loginModel.RoleId == "3")
            //{
            //    var userid = loginModel.UserId.ToInt();
            //    wherelambda = wherelambda.And(t => t.CrearSysUserId == userid);//业务员员只能看到自己录入的数据  
            //}
            if (!string.IsNullOrEmpty(ContoractNumber))
            {
                wherelambda = wherelambda.And(t => t.ContoractNumber.Contains(ContoractNumber));
            }
            if (!string.IsNullOrEmpty(EntrustPeople))
            {
                wherelambda = wherelambda.And(t => t.EntrustPeople.Contains(EntrustPeople));
            }
        
            var list = db.Set<C_TB_HC_CONTRACT>().Where(wherelambda).OrderByDescending(n => n.LastEdiTime).AsQueryable();
            int total = list.Count();
            object rows = list.Skip(offset).Take(limit).AsQueryable();
            return Json(new { total = total, rows = rows }, JsonRequestBehavior.AllowGet);

        }
        public JsonResult GetContractDetailedList(string contractGuid)
        {
            List<C_TB_HC_CONTRACT_DETAILED> list = db.C_TB_HC_CONTRACT_DETAILED.Where(n => n.CONTRACT_Guid == contractGuid).OrderByDescending(n => n.LastEdiTime).ToList();
            var total = list.Count;
            var rows = list.ToList();
            return Json(new { total = total, rows = rows }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetContractFilesList(string contractGuid)
        {
            List<C_TB_HC_CONTRACT_FILES> list = db.C_TB_HC_CONTRACT_FILES.Where(n => n.CONTRACT_Guid == contractGuid).ToList();
            var total = list.Count;
            var rows = list.ToList();
            return Json(new { total = total, rows = rows }, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 添加、编辑合同页面
        /// </summary>
        /// <returns></returns>      
        public ActionResult AddContract(string guid)
        {
            var loginModel = OperatorProvider.Provider.GetCurrent();
           var yuanquid = loginModel.YuanquID;
            List<C_TB_CODE_CUSTOMER> list_CUSTOMER = db.C_TB_CODE_CUSTOMER.Where(n=>n.YuanQuID== yuanquid).OrderBy(n => n.ID).ToList();//委托人,收货人
            List<SelectListItem> emplist_CUSTOMER = SelectHelp.CreateSelect<C_TB_CODE_CUSTOMER>(list_CUSTOMER, "Name", "Code", null);
            ViewData["CUSTOMER_List"] = new SelectList(emplist_CUSTOMER, "Value", "Text", "是");

            C_TB_HC_CONTRACT model = db.C_TB_HC_CONTRACT.Find(guid)??new C_TB_HC_CONTRACT();
            return View(model);
        }
        [OperationLog(OperationLogAttribute.Operatetype.添加或编辑, OperationLogAttribute.ImportantLevel.一般操作, "添加合同")]
        [HttpPost]
        public ActionResult AddContract(C_TB_HC_CONTRACT model)
        {
            OperatorModel loginModel = OperatorProvider.Provider.GetCurrent();
            var yuanquid = loginModel.YuanquID ?? throw new ArgumentNullException("loginModel.YuanquID");
            model.YuanQuID = yuanquid;
            model.CrearSysUserId = loginModel.UserId.ToInt();
            model.CreatUserName = loginModel.UserName;

            string entrustId = model.EntrustID.ToString();
            C_TB_CODE_CUSTOMER customerEntry = db.C_TB_CODE_CUSTOMER.FirstOrDefault(n => n.Code == entrustId);
            if (customerEntry != null) model.EntrustPeople = customerEntry.Name;

            if (model.Guid=="0")
            {
                model.Guid = Guid.NewGuid().ToString();
                model.State = "待提交审核";
                model.CreatTime=DateTime.Now;
                model.LastEdiTime = DateTime.Now;
                db.C_TB_HC_CONTRACT.Add(model);
            }
            else
            {
                model.LastEdiTime = DateTime.Now;
                db.Entry(model).State = EntityState.Modified;
            }
           int c=  db.SaveChanges();
            if (c>0)
            {
                return Json(new AjaxResult { state = ResultType.success.ToString(), message = "成功！",data = model.Guid}.ToJson());
            }
           else
            {
                if (c == 0)
                {
                    return Json(new AjaxResult { state = ResultType.warning.ToString(), message = "无更新" }.ToJson());
                }
                return Json(new AjaxResult { state = ResultType.error.ToString(), message = "失败！" }.ToJson());
            }
        }
        /// <summary>
        /// 锁定合同
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        [OperationLog(OperationLogAttribute.Operatetype.编辑, OperationLogAttribute.ImportantLevel.一般操作, "审核合同")]
        [HttpPost]
        public ActionResult SuoDing(string guid)
        {
            C_TB_HC_CONTRACT u =db.C_TB_HC_CONTRACT.Find(guid);
            u.State = "已完成";

            string delsql = "update  C_TB_HC_CONTRACT_DETAILED set C_TB_HC_CONTRACT_DETAILED.\"State\"='已完成' where C_TB_HC_CONTRACT_DETAILED.\"CONTRACT_Guid\"='" + guid + "'";
            db.Database.ExecuteSqlCommand(delsql);


             delsql = "update  C_TB_HC_CONTRACT_FILES set C_TB_HC_CONTRACT_FILES.\"State\"='已完成' where C_TB_HC_CONTRACT_FILES.\"CONTRACT_Guid\"='" + guid + "'";
            db.Database.ExecuteSqlCommand(delsql);
            int c = db.SaveChanges();
            if (c > 0)
            {
                return Json(new AjaxResult { state = ResultType.success.ToString(), message = "成功！" }.ToJson());
            }
            else
            {
                if (c == 0)
                {
                    return Json(new AjaxResult { state = ResultType.warning.ToString(), message = "无更新" }.ToJson());
                }
                return Json(new AjaxResult { state = ResultType.error.ToString(), message = "失败！" }.ToJson());
            }
        }
        /// <summary>
        /// 把合同提交审核
        /// </summary>
        /// <param name="guid">唯一编号</param>
        /// <param name="zhuangtai">状态</param>
        /// <returns></returns>
        [OperationLog(OperationLogAttribute.Operatetype.编辑, OperationLogAttribute.ImportantLevel.一般操作, "提交合同到经理审核")]
        [HttpPost]
        public ActionResult Tjsh(string guid,string zhuangtai)
        {
            C_TB_HC_CONTRACT u = db.C_TB_HC_CONTRACT.Find(guid);
            u.State = zhuangtai;
            int c = db.SaveChanges();
            if (c > 0)
            {
                return Json(new AjaxResult { state = ResultType.success.ToString(), message = "成功！" });
            }
            else
            {
                if (c == 0)
                {
                    return Json(new AjaxResult { state = ResultType.warning.ToString(), message = "无更新" });
                }
                return Json(new AjaxResult { state = ResultType.error.ToString(), message = "失败！" });
            }
        }
        [OperationLog(OperationLogAttribute.Operatetype.删除, OperationLogAttribute.ImportantLevel.危险操作, "删除合同")]
        [HttpPost]
        public ActionResult DelContract(string guid)
        {
            C_TB_HC_CONTRACT u = new C_TB_HC_CONTRACT() { Guid = guid };
            db.C_TB_HC_CONTRACT.Attach(u);
            db.C_TB_HC_CONTRACT.Remove(u);
            string delsql = "delete from C_TB_HC_CONTRACT_DETAILED where 'CONTRACT_Guid'='" + guid + "'";
            db.Database.ExecuteSqlCommand(delsql);
            delsql = "delete from C_TB_HC_CONTRACT_FILES where 'CONTRACT_Guid'='" + guid + "'";
            db.Database.ExecuteSqlCommand(delsql);
            int c = db.SaveChanges();
            if (c > 0)
            {
                return Json(new AjaxResult { state = ResultType.success.ToString(), message = "成功！"}.ToJson());
            }
            else
            {
                if (c == 0)
                {
                    return Json(new AjaxResult { state = ResultType.warning.ToString(), message = "无更新" }.ToJson());
                }
                return Json(new AjaxResult { state = ResultType.error.ToString(), message = "失败！" }.ToJson());
            }
        }
        [OperationLog(OperationLogAttribute.Operatetype.删除, OperationLogAttribute.ImportantLevel.危险操作, "删除合同详细")]
        [HttpPost]
        public ActionResult DelContractSon(string guid)
        {
            C_TB_HC_CONTRACT_DETAILED u = new C_TB_HC_CONTRACT_DETAILED() { Guid = guid };
            db.C_TB_HC_CONTRACT_DETAILED.Attach(u);
            db.C_TB_HC_CONTRACT_DETAILED.Remove(u);
            int c = db.SaveChanges();
            if (c > 0)
            {
                return Json(new AjaxResult { state = ResultType.success.ToString(), message = "成功！" }.ToJson());
            }
            else
            {
                if (c == 0)
                {
                    return Json(new AjaxResult { state = ResultType.warning.ToString(), message = "无更新" }.ToJson());
                }
                return Json(new AjaxResult { state = ResultType.error.ToString(), message = "失败！" }.ToJson());
            }
        }
        /// <summary>
        /// 编辑合同的详细
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
       
        public ActionResult AddContractDetailed(string CONTRACT_Guid=null, string guid=null)
        {
            List<C_GOODS> list_GOODS = db.C_GOODS.OrderBy(n => n.ID).ToList();//货物
            List<SelectListItem> emplist_GOODS = SelectHelp.CreateSelect<C_GOODS>(list_GOODS, "GoodsName", "Code", null);
            ViewData["GOODS_List"] = new SelectList(emplist_GOODS, "Value", "Text", "是");

            List<C_Charge> listC_Charge = db.C_Charge.OrderBy(n => n.creatTime).ToList();//费目
            List<SelectListItem> emplistlistC_Charge = SelectHelp.CreateSelect<C_Charge>(listC_Charge, "Name", "CODE", null);
            ViewData["emplistlistC_Charge"] = new SelectList(emplistlistC_Charge, "Value", "Text", "是");

            C_TB_HC_CONTRACT_DETAILED model =db.C_TB_HC_CONTRACT_DETAILED.Find(guid)??  new C_TB_HC_CONTRACT_DETAILED()
            {
                CONTRACT_Guid = CONTRACT_Guid
            };
            return View(model);
        }
        [OperationLog(OperationLogAttribute.Operatetype.添加或编辑, OperationLogAttribute.ImportantLevel.一般操作, "添加或编辑合同详细")]
        [HttpPost]
        public ActionResult AddContractDetailed(C_TB_HC_CONTRACT_DETAILED model)
        {
            var feiMuZhongLeiCode = model.FeiMuZhongLeiCode;
            C_Charge chargeEntry = db.C_Charge.FirstOrDefault(n => n.CODE == feiMuZhongLeiCode);
            if (chargeEntry != null) model.FeiMuZhongLei = chargeEntry.Name;
            var huoMingCode = model.HuoMingCode;
            C_GOODS cGoodsEntry = db.C_GOODS.FirstOrDefault(n => n.Code == huoMingCode);
            if (cGoodsEntry != null) model.HuoMing = cGoodsEntry.GoodsName;
            if (model.Guid == null)
            {
                model.Guid = Guid.NewGuid().ToString();
                model.State = "进行中";
                model.CreatTime = DateTime.Now;
                model.LastEdiTime = DateTime.Now;
                db.C_TB_HC_CONTRACT_DETAILED.Add(model);
            }
            else
            {
                model.LastEdiTime = DateTime.Now;
                db.Entry(model).State = EntityState.Modified;
            }
            try
            {
          
                int c = db.SaveChanges();
                if (c > 0)
                {
                    return Json(new AjaxResult
                        {state = ResultType.success.ToString(), message = "成功！", data = model.CONTRACT_Guid}.ToJson());
                }
                else
                {
                    if (c == 0)
                    {
                        return Json(new AjaxResult {state = ResultType.warning.ToString(), message = "无更新"}.ToJson());
                    }

                    return Json(new AjaxResult {state = ResultType.error.ToString(), message = "失败！"}.ToJson());
                }

            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            {
                var msg = string.Empty;
                var errors = (from u in ex.EntityValidationErrors select u.ValidationErrors).ToList();
                foreach (var item in errors)
                    msg += item.FirstOrDefault().ErrorMessage;
                Log4NetHelper log = new Log4NetHelper();
                log.Error(msg, ex);
                return null;
            }
            catch (Exception e)
            {
                Log4NetHelper log = new Log4NetHelper();
                log.Error(e.Message, e);
                throw;
            }
        }

        public ActionResult AddContractFile(string CONTRACT_Guid = null)
        {
            ViewBag.CONTRACT_Guid = CONTRACT_Guid;
            List<C_TB_HC_CONTRACT_FILES> list = db.C_TB_HC_CONTRACT_FILES.Where(n => n.CONTRACT_Guid == CONTRACT_Guid)
                .ToList();
            ViewBag.list = list;
            return View();
        }
        public ActionResult WebuploaderUpLoadProcess(string id, string name, string type, string lastModifiedDate, int size, HttpPostedFileBase file, string CONTRACT_Guid)
        {
            //string CONTRACT_Guid = null;

            //保存到临时文件夹
            string urlPath = "Upload/" + CONTRACT_Guid;
            string filePathName = string.Empty;

            string localPath = Path.Combine(HttpRuntime.AppDomainAppPath, urlPath);
            if (Request.Files.Count == 0)
            {
                return Json(new { jsonrpc = 2.0, error = new { code = 102, message = "保存失败" }, id = "id" });
            }
            filePathName = name;
            if (!System.IO.Directory.Exists(localPath))
            {
                System.IO.Directory.CreateDirectory(localPath);
            }
            file.SaveAs(Path.Combine(localPath, filePathName));
            C_TB_HC_CONTRACT_FILES f = new C_TB_HC_CONTRACT_FILES()
            {
                CONTRACT_Guid = CONTRACT_Guid,
                path = urlPath + "/" + filePathName,
                FileName = name,
                Guid = Guid.NewGuid().ToString(),
                State = "进行中"

            };
            db.C_TB_HC_CONTRACT_FILES.Add(f);
            db.SaveChanges();
            return Json(new
            {
                jsonrpc = "2.0",
                id = id,
                filePath = urlPath + "/" + filePathName//返回一个视图界面可直接使用的url
            });

        }
        [OperationLog(OperationLogAttribute.Operatetype.删除, OperationLogAttribute.ImportantLevel.危险操作, "删除合同附件")]
        [HttpPost]
        public ActionResult DelContractFile(string guid)
        {
            C_TB_HC_CONTRACT_FILES u = new C_TB_HC_CONTRACT_FILES() { Guid = guid };
            db.C_TB_HC_CONTRACT_FILES.Attach(u);
            db.C_TB_HC_CONTRACT_FILES.Remove(u);
            int c = db.SaveChanges();
            if (c > 0)
            {
                return Json(new AjaxResult { state = ResultType.success.ToString(), message = "成功！" }.ToJson());
            }
            else
            {
                if (c == 0)
                {
                    return Json(new AjaxResult { state = ResultType.warning.ToString(), message = "无更新" }.ToJson());
                }
                return Json(new AjaxResult { state = ResultType.error.ToString(), message = "失败！" }.ToJson());
            }
        }
    }
}