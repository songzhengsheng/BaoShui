using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;
using Main.HelpClass;
using MainBLL.SysModel;
using MainBLL.TallyBLL;
using Newtonsoft.Json;
using NFine.Code;
using NFine.Code.Select;

using OracleBase.HelpClass;
using OracleBase.HelpClass.Sys;
using OracleBase.Models;

namespace OracleBase.Areas.ck.Controllers
{
    [SignLoginAuthorize]
    public class GoodsBillController : Controller
    {
        private Entities db = new Entities();
        // GET: ck/GoodsBill

        public ActionResult GOODSBILLList()
        {
            OperatorModel loginModel = OperatorProvider.Provider.GetCurrent();
            ViewBag.roleId = loginModel.RoleId;
            return View();
        }

        public ActionResult GOODSBILLList_wancheng()
        {
            OperatorModel loginModel = OperatorProvider.Provider.GetCurrent();
            ViewBag.roleId = loginModel.RoleId;
            return View();
        }
        public ActionResult GOODSBILLList_Dsh()
        {
            OperatorModel loginModel = OperatorProvider.Provider.GetCurrent();
            ViewBag.roleId = loginModel.RoleId;
            return View();
        }
        [HttpPost]
        public JsonResult Ajaxtg(int id)
        {
            C_TB_HC_GOODSBILL infoModel = db.C_TB_HC_GOODSBILL.Find(id);
            infoModel.State = "进行中";
            db.Set<C_TB_HC_GOODSBILL>().AddOrUpdate(infoModel);
            int c = db.SaveChanges();
            if (c > 0)
            {
                return Json("成功");
            }
            return Json("失败");
        }
        [OperationLog(OperationLogAttribute.Operatetype.审核, OperationLogAttribute.ImportantLevel.一般操作, "票货驳回")]
        [HttpPost]
        public JsonResult Ajaxbh(int id, string Shyj)
        {
            C_TB_HC_GOODSBILL infoModel = db.C_TB_HC_GOODSBILL.Find(id);
            infoModel.RejectReason = Shyj;
            infoModel.State = "被驳回";
            int c = db.SaveChanges();
            if (c > 0)
            {
                return Json("成功");
            }
            return Json("失败");
        }
        public ActionResult SureBao_Newshenhe(int id)
        {
            C_TB_HC_GOODSBILL model = db.C_TB_HC_GOODSBILL.Find(id);
            return View(model);
        }
        [HttpGet]
        public object GetGOODSBILLList(int limit, int offset, string GBNO, string C_GOODSAGENT_NAME, string CreatTime, string CreatTime1, string BLNO, string C_GOODS, string HuoZhu, string ShipName, string VGNO)
        {
            var wherelambda = ExtLinq.True<C_TB_HC_GOODSBILL>();
            OperatorModel loginModel = OperatorProvider.Provider.GetCurrent();
            wherelambda = wherelambda.And(t => t.YuanQuID == loginModel.YuanquID);
            wherelambda = wherelambda.And(t => t.State != "已完成");
            wherelambda = wherelambda.And(t => t.State != "待审核");
            wherelambda = wherelambda.And(t => t.State != "被驳回");
            //if (loginModel.RoleId=="3")
            //{
            //    var userid = loginModel.UserId.ToInt();
            //    wherelambda = wherelambda.And(t => t.SysUserID == userid);//业务员员只能看到自己录入的数据  
            //}

            if (!string.IsNullOrEmpty(GBNO))
            {
                wherelambda = wherelambda.And(t => t.GBNO.Contains(GBNO));
            }
            if (!string.IsNullOrEmpty(C_GOODSAGENT_NAME))
            {
                wherelambda = wherelambda.And(t => t.C_GOODSAGENT_NAME.Contains(C_GOODSAGENT_NAME));
            }
            if (!string.IsNullOrEmpty(CreatTime))
            {
                DateTime CreatTime_date = Convert.ToDateTime(CreatTime + " 00:00:00");
                wherelambda = wherelambda.And(n => n.CreatTime >= CreatTime_date);

            }
            if (!string.IsNullOrEmpty(CreatTime1))
            {
                DateTime CreatTime1_date = Convert.ToDateTime(CreatTime1 + " 23:59:59");
                wherelambda = wherelambda.And(n => n.CreatTime <= CreatTime1_date);

            }
            if (!string.IsNullOrEmpty(BLNO))
            {
                wherelambda = wherelambda.And(t => t.BLNO.Contains(BLNO));
            }
            if (!string.IsNullOrEmpty(C_GOODS))
            {
                wherelambda = wherelambda.And(t => t.C_GOODS.Contains(C_GOODS));
            }
            if (!string.IsNullOrEmpty(HuoZhu))
            {
                wherelambda = wherelambda.And(t => t.HuoZhu.Contains(HuoZhu));
            }
            if (!string.IsNullOrEmpty(ShipName))
            {
                wherelambda = wherelambda.And(t => t.ShipName.Contains(ShipName));
            }
            if (!string.IsNullOrEmpty(VGNO))
            {
                wherelambda = wherelambda.And(t => t.VGNO.Contains(VGNO));
            }
            var list = db.Set<C_TB_HC_GOODSBILL>().Where(wherelambda).OrderByDescending(n => n.CreatTime).AsQueryable();
            foreach (var itsms_GoodsBill in list)//计算库存
            {
                decimal? KuCun = 0;
                decimal? KuCunW = 0;
                List<C_TB_HC_CONSIGN> list_CONSIGN = db.C_TB_HC_CONSIGN.Where(n => n.GOODSBILL_ID == itsms_GoodsBill.ID).OrderBy(n => n.ID).ToList();
                foreach (var items_Consign in list_CONSIGN)
                {
                    List<C_TB_HS_TALLYBILL> list_TALLYBILL = db.C_TB_HS_TALLYBILL.Where(n => n.CONSIGN_ID == items_Consign.ID).OrderBy(n => n.ID).ToList();
                    foreach (var items_TALLYBILL in list_TALLYBILL)
                    {
                        if (items_TALLYBILL.CODE_OPSTYPE == "进库")
                        {
                            KuCun += items_TALLYBILL.AMOUNT;
                            KuCunW += items_TALLYBILL.WEIGHT;
                        }
                        if (items_TALLYBILL.CODE_OPSTYPE == "出库")
                        {
                            KuCun -= items_TALLYBILL.AMOUNT;
                            KuCunW -= items_TALLYBILL.WEIGHT;
                        }
                    }
                }
                itsms_GoodsBill.KunCun = KuCun.ToString();
                itsms_GoodsBill.KunCunW = KuCunW.ToDecimal(3).ToString();


            }
            int total = list.Count();
            object rows = list.Skip(offset).Take(limit).AsQueryable();
            return Json(new { total = total, rows = rows }, JsonRequestBehavior.AllowGet);

        }
        [HttpGet]
        public object GetGOODSBILLList_wancheng(int limit, int offset, string GBNO, string C_GOODSAGENT_NAME, string CreatTime, string CreatTime1, string BLNO, string C_GOODS, string HuoZhu, string ShipName, string VGNO)
        {
            var wherelambda = ExtLinq.True<C_TB_HC_GOODSBILL>();
            OperatorModel loginModel = OperatorProvider.Provider.GetCurrent();
            wherelambda = wherelambda.And(t => t.YuanQuID == loginModel.YuanquID);
            wherelambda = wherelambda.And(t => t.State == "已完成");
            if (!string.IsNullOrEmpty(GBNO))
            {
                wherelambda = wherelambda.And(t => t.GBNO.Contains(GBNO));
            }
            if (!string.IsNullOrEmpty(C_GOODSAGENT_NAME))
            {
                wherelambda = wherelambda.And(t => t.C_GOODSAGENT_NAME.Contains(C_GOODSAGENT_NAME));
            }
            if (!string.IsNullOrEmpty(CreatTime))
            {
                DateTime CreatTime_date = Convert.ToDateTime(CreatTime + " 00:00:00");
                wherelambda = wherelambda.And(n => n.CreatTime >= CreatTime_date);

            }
            if (!string.IsNullOrEmpty(CreatTime1))
            {
                DateTime CreatTime1_date = Convert.ToDateTime(CreatTime1 + " 23:59:59");
                wherelambda = wherelambda.And(n => n.CreatTime <= CreatTime1_date);

            }
            if (!string.IsNullOrEmpty(BLNO))
            {
                wherelambda = wherelambda.And(t => t.BLNO.Contains(BLNO));
            }
            if (!string.IsNullOrEmpty(C_GOODS))
            {
                wherelambda = wherelambda.And(t => t.C_GOODS.Contains(C_GOODS));
            }
            if (!string.IsNullOrEmpty(HuoZhu))
            {
                wherelambda = wherelambda.And(t => t.HuoZhu.Contains(HuoZhu));
            }
            if (!string.IsNullOrEmpty(ShipName))
            {
                wherelambda = wherelambda.And(t => t.ShipName.Contains(ShipName));
            }
            if (!string.IsNullOrEmpty(VGNO))
            {
                wherelambda = wherelambda.And(t => t.VGNO.Contains(VGNO));
            }
            var list = db.Set<C_TB_HC_GOODSBILL>().Where(wherelambda).OrderByDescending(n => n.CreatTime).AsQueryable();
            foreach (var itsms_GoodsBill in list)//计算库存
            {
                decimal? KuCun = 0;
                decimal? KuCunW = 0;
                List<C_TB_HC_CONSIGN> list_CONSIGN = db.C_TB_HC_CONSIGN.Where(n => n.GOODSBILL_ID == itsms_GoodsBill.ID).OrderBy(n => n.ID).ToList();
                foreach (var items_Consign in list_CONSIGN)
                {
                    List<C_TB_HS_TALLYBILL> list_TALLYBILL = db.C_TB_HS_TALLYBILL.Where(n => n.CONSIGN_ID == items_Consign.ID).OrderBy(n => n.ID).ToList();
                    foreach (var items_TALLYBILL in list_TALLYBILL)
                    {
                        if (items_TALLYBILL.CODE_OPSTYPE == "进库")
                        {
                            KuCun += items_TALLYBILL.AMOUNT;
                            KuCunW += items_TALLYBILL.WEIGHT;
                        }
                        if (items_TALLYBILL.CODE_OPSTYPE == "出库")
                        {
                            KuCun -= items_TALLYBILL.AMOUNT;
                            KuCunW -= items_TALLYBILL.WEIGHT;
                        }
                    }
                }
                itsms_GoodsBill.KunCun = KuCun.ToString();
                itsms_GoodsBill.KunCunW = KuCunW.ToDecimal(3).ToString();


            }
            int total = list.Count();
            object rows = list.Skip(offset).Take(limit).AsQueryable();
            return Json(new { total = total, rows = rows }, JsonRequestBehavior.AllowGet);

        }
        [HttpGet]
        public object GetGOODSBILLList_weishenghe(int limit, int offset, string GBNO, string C_GOODSAGENT_NAME, string CreatTime, string CreatTime1, string BLNO, string C_GOODS, string HuoZhu, string ShipName, string VGNO)
        {
            var wherelambda = ExtLinq.True<C_TB_HC_GOODSBILL>();
            OperatorModel loginModel = OperatorProvider.Provider.GetCurrent();
            wherelambda = wherelambda.And(t => t.YuanQuID == loginModel.YuanquID);
            wherelambda = wherelambda.And(t => t.State == "待审核");
            if (!string.IsNullOrEmpty(GBNO))
            {
                wherelambda = wherelambda.And(t => t.GBNO.Contains(GBNO));
            }
            if (!string.IsNullOrEmpty(C_GOODSAGENT_NAME))
            {
                wherelambda = wherelambda.And(t => t.C_GOODSAGENT_NAME.Contains(C_GOODSAGENT_NAME));
            }
            if (!string.IsNullOrEmpty(CreatTime))
            {
                DateTime CreatTime_date = Convert.ToDateTime(CreatTime + " 00:00:00");
                wherelambda = wherelambda.And(n => n.CreatTime >= CreatTime_date);

            }
            if (!string.IsNullOrEmpty(CreatTime1))
            {
                DateTime CreatTime1_date = Convert.ToDateTime(CreatTime1 + " 23:59:59");
                wherelambda = wherelambda.And(n => n.CreatTime <= CreatTime1_date);

            }
            if (!string.IsNullOrEmpty(BLNO))
            {
                wherelambda = wherelambda.And(t => t.BLNO.Contains(BLNO));
            }
            if (!string.IsNullOrEmpty(C_GOODS))
            {
                wherelambda = wherelambda.And(t => t.C_GOODS.Contains(C_GOODS));
            }
            if (!string.IsNullOrEmpty(HuoZhu))
            {
                wherelambda = wherelambda.And(t => t.HuoZhu.Contains(HuoZhu));
            }
            if (!string.IsNullOrEmpty(ShipName))
            {
                wherelambda = wherelambda.And(t => t.ShipName.Contains(ShipName));
            }
            if (!string.IsNullOrEmpty(VGNO))
            {
                wherelambda = wherelambda.And(t => t.VGNO.Contains(VGNO));
            }
            var list = db.Set<C_TB_HC_GOODSBILL>().Where(wherelambda).OrderByDescending(n => n.CreatTime).AsQueryable();
            foreach (var itsms_GoodsBill in list)//计算库存
            {
                decimal? KuCun = 0;
                decimal? KuCunW = 0;
                List<C_TB_HC_CONSIGN> list_CONSIGN = db.C_TB_HC_CONSIGN.Where(n => n.GOODSBILL_ID == itsms_GoodsBill.ID).OrderBy(n => n.ID).ToList();
                foreach (var items_Consign in list_CONSIGN)
                {
                    List<C_TB_HS_TALLYBILL> list_TALLYBILL = db.C_TB_HS_TALLYBILL.Where(n => n.CONSIGN_ID == items_Consign.ID).OrderBy(n => n.ID).ToList();
                    foreach (var items_TALLYBILL in list_TALLYBILL)
                    {
                        if (items_TALLYBILL.CODE_OPSTYPE == "进库")
                        {
                            KuCun += items_TALLYBILL.AMOUNT;
                            KuCunW += items_TALLYBILL.WEIGHT;
                        }
                        if (items_TALLYBILL.CODE_OPSTYPE == "出库")
                        {
                            KuCun -= items_TALLYBILL.AMOUNT;
                            KuCunW -= items_TALLYBILL.WEIGHT;
                        }
                    }
                }
                itsms_GoodsBill.KunCun = KuCun.ToString();
                itsms_GoodsBill.KunCunW = KuCunW.ToDecimal(3).ToString();


            }
            int total = list.Count();
            object rows = list.Skip(offset).Take(limit).AsQueryable();
            return Json(new { total = total, rows = rows }, JsonRequestBehavior.AllowGet);

        }
        [OperationLog(OperationLogAttribute.Operatetype.删除, OperationLogAttribute.ImportantLevel.一般操作, "删除票货操作")]
        [HttpPost]
        public JsonResult Del_Dic_GOODSBILL(string datalist)
        {
            List<C_TB_HC_GOODSBILL> list = JsonConvert.DeserializeObject<List<C_TB_HC_GOODSBILL>>(datalist);

            EFHelpler<C_TB_HC_GOODSBILL> ef = new EFHelpler<C_TB_HC_GOODSBILL>();
            ef.delete(list.ToArray());
            return Json(new AjaxResult { state = ResultType.success.ToString(), message = "成功！" }.ToJson());

        }
        public ActionResult Add_C_GOODSBILL(int id)
        {
            C_TB_HC_GOODSBILL model = db.C_TB_HC_GOODSBILL.Find(id) ?? new C_TB_HC_GOODSBILL();
            List<C_TB_CODE_INOUT> list_InOut = db.C_TB_CODE_INOUT.OrderBy(n => n.ID).ToList();//进出口
            List<SelectListItem> emplist_InOut = SelectHelp.CreateSelect<C_TB_CODE_INOUT>(list_InOut, "Name", "Name", null);
            ViewData["InOut_List"] = new SelectList(emplist_InOut, "Value", "Text", "是");

            List<C_TB_CODE_TRADE> list_TRADE = db.C_TB_CODE_TRADE.OrderBy(n => n.ID).ToList();//内外贸
            List<SelectListItem> emplist_TRADE = SelectHelp.CreateSelect<C_TB_CODE_TRADE>(list_TRADE, "Name", "Name", null);
            ViewData["TRADE_List"] = new SelectList(emplist_TRADE, "Value", "Text", "是");

            List<C_GOODSAGENT> list_GOODSAGENT = db.C_GOODSAGENT.OrderBy(n => n.Name).ToList();//货代
            List<SelectListItem> emplist_GOODSAGENT = SelectHelp.CreateSelect<C_GOODSAGENT>(list_GOODSAGENT, "Name", "ID", null);
            ViewData["GOODSAGENT_List"] = new SelectList(emplist_GOODSAGENT, "Value", "Text", "是");

            List<C_GOODS> list_GOODS = db.C_GOODS.OrderBy(n => n.GoodsName).ToList();//货物
            List<SelectListItem> emplist_GOODS = SelectHelp.CreateSelect<C_GOODS>(list_GOODS, "GoodsName", "GoodsName", null);
            ViewData["GOODS_List"] = new SelectList(emplist_GOODS, "Value", "Text", "是");

            List<C_TB_CODE_PACK> list_PACK = db.C_TB_CODE_PACK.OrderBy(n => n.ID).ToList();//包装
            List<SelectListItem> emplist_PACK = SelectHelp.CreateSelect<C_TB_CODE_PACK>(list_PACK, "Name", "Name", null);
            ViewData["PACK_List"] = new SelectList(emplist_PACK, "Value", "Text", "是");
            List<C_GOODSTYPE> list_GOODSType = db.C_GOODSTYPE.OrderByDescending(n => n.ID).ToList();//货物类型
            List<SelectListItem> emplist_list_GOODSType = SelectHelp.CreateSelect<C_GOODSTYPE>(list_GOODSType, "GoodsName", "GoodsName", null);
            ViewData["GOODSType_List"] = new SelectList(emplist_list_GOODSType, "Value", "Text", "是");
            return View(model);
        }
        [ValidateInput(false)]
        public string GetEntrustListByCode(string Keyword)
        {
            List<string> Statelist = new List<string>()
          {
              "通过",
              "已完成"
          };
            OperatorModel loginModel = OperatorProvider.Provider.GetCurrent();
            int yuanquId = loginModel.YuanquID.ToInt();
            List<C_TB_HC_CONTRACT> list = db.C_TB_HC_CONTRACT.Where(n => n.YuanQuID == yuanquId && n.ContoractNumber.Contains(Keyword) && Statelist.Contains(n.State)).ToList();
            List<HeTongTallyBLL> stb = list.Select(x => new HeTongTallyBLL { ContoractNumber = x.ContoractNumber, Guid = x.Guid }).ToList();
            JsonHelper jshelp = new JsonHelper();
            return jshelp.List2JSON(stb);
        }
        [OperationLog(OperationLogAttribute.Operatetype.添加或编辑, OperationLogAttribute.ImportantLevel.一般操作, "添加票货")]
        [HttpPost]
        public JsonResult Add_C_GOODSBILL(C_TB_HC_GOODSBILL model)
        {
            OperatorModel loginModel = OperatorProvider.Provider.GetCurrent();
            if (string.IsNullOrEmpty(model.GBNO))           //新建时生成流水号
            {
                string TodayTime = DateTime.Today.ToString("yyyyMMdd");
                C_TB_HC_GOODSBILL Num = db.C_TB_HC_GOODSBILL.OrderByDescending(n => n.CreatTime).ToList().FirstOrDefault();
                if (Num != null)
                {
                    if (!string.IsNullOrEmpty(Num.GBNO))
                    {
                        if (Num.GBNO.Substring(2, 8) == TodayTime)
                        {
                            model.GBNO = "PH" + TodayTime + (Convert.ToInt32(Num.GBNO.Replace("PH" + TodayTime, "")) + 1).ToString("0000");
                        }
                        else
                        {
                            model.GBNO = "PH" + TodayTime + "0001";
                        }
                    }
                }

                else
                {
                    model.GBNO = "PH" + TodayTime + "0001";
                }

            }
            var yuanquid = loginModel.YuanquID ?? throw new ArgumentNullException("loginModel.YuanquID");
            model.YuanQuID = yuanquid;
            model.CreatPeople = loginModel.UserName;
            model.SysUserID = loginModel.UserId.ToInt();
            model.C_GOODSAGENT_NAME = db.C_GOODSAGENT.FirstOrDefault(n => n.ID == model.C_GOODSAGENT_ID).Name.ToString();
            if (model.ID == 0)
            {
                model.CreatTime = DateTime.Now;
                model.State = MainBLL.GoodBill.GoodBillStateEnum.待提交审核.ToString();
                model.KunCun = "0";
                model.KunCunW = "0";
            }
            if (!string.IsNullOrEmpty(model.ContoractNumber))
            {
                C_TB_HC_CONTRACT model_ht = db.C_TB_HC_CONTRACT.FirstOrDefault(n => n.ContoractNumber == model.ContoractNumber);
                if (model_ht == null)
                {
                    return Json("找不到对应的合同号");
                }
                else
                {
                    model.CONTRACT_Guid = model_ht.Guid;
                }
            }
            db.Set<C_TB_HC_GOODSBILL>().AddOrUpdate(model);
            try
            {


                db.SaveChanges();
                return Json("成功");
            }
            catch (Exception ex)
            {
                return Json(ex.InnerException.InnerException.Message);
            }

        }
        [OperationLog(OperationLogAttribute.Operatetype.审核, OperationLogAttribute.ImportantLevel.一般操作, "审核通过")]
        [HttpPost]
        public JsonResult shenheGoodsBill(int id)
        {

            try
            {
                C_TB_HC_GOODSBILL model_GoodsBill = db.C_TB_HC_GOODSBILL.Find(id) ?? new C_TB_HC_GOODSBILL();
                model_GoodsBill.State = "已完成";
                db.Set<C_TB_HC_GOODSBILL>().AddOrUpdate(model_GoodsBill);
                db.SaveChanges();
                List<C_TB_HC_CONSIGN> list_CONSIGN = db.C_TB_HC_CONSIGN.Where(n => n.GOODSBILL_ID == model_GoodsBill.ID).OrderBy(n => n.ID).ToList();
                foreach (var items_CONSIGN in list_CONSIGN)
                {
                    C_TB_HC_CONSIGN model_CONSIGN = db.C_TB_HC_CONSIGN.Find(items_CONSIGN.ID) ?? new C_TB_HC_CONSIGN();
                    model_CONSIGN.State = "已完成";
                    db.Set<C_TB_HC_CONSIGN>().AddOrUpdate(model_CONSIGN);
                    db.SaveChanges();
                    List<C_TB_HS_TALLYBILL> list_TALLYBILL = db.C_TB_HS_TALLYBILL.Where(n => n.CONSIGN_ID == model_CONSIGN.ID).OrderBy(n => n.ID).ToList();
                    foreach (var items_TALLYBILL in list_TALLYBILL)
                    {
                        C_TB_HS_TALLYBILL model_TALLYBILL = db.C_TB_HS_TALLYBILL.Find(items_TALLYBILL.ID) ?? new C_TB_HS_TALLYBILL();
                        model_TALLYBILL.State = "已完成";
                        db.Set<C_TB_HS_TALLYBILL>().AddOrUpdate(model_TALLYBILL);
                        db.SaveChanges();
                    }
                }

                return Json(new AjaxResult { state = ResultType.success.ToString(), message = "成功" }.ToJson());
            }
            catch (Exception e)
            {
                return Json(new AjaxResult { state = ResultType.error.ToString(), message = e.Message }.ToJson());
            }
        }
        public JsonResult qx_shenheGoodsBill(int id)
        {

            try
            {
                C_TB_HC_GOODSBILL model_GoodsBill = db.C_TB_HC_GOODSBILL.Find(id) ?? new C_TB_HC_GOODSBILL();
                List<C_TB_HC_CONSIGN> list_CONSIGN = db.C_TB_HC_CONSIGN.Where(n => n.GOODSBILL_ID == id).OrderBy(n => n.ID).ToList();
                if (list_CONSIGN.Count != 0)
                {
                    model_GoodsBill.State = "已生成" + list_CONSIGN.Count + "条委托";
                }
                else
                {
                    model_GoodsBill.State = "进行中";
                }
                db.Set<C_TB_HC_GOODSBILL>().AddOrUpdate(model_GoodsBill);
                db.SaveChanges();

                return Json(new AjaxResult { state = ResultType.success.ToString(), message = "成功" }.ToJson());
            }
            catch (Exception e)
            {
                return Json(new AjaxResult { state = ResultType.error.ToString(), message = e.Message }.ToJson());
            }
        }
        public ActionResult GOODSBILL_xq(int id)
        {
            C_TB_HC_GOODSBILL model = db.C_TB_HC_GOODSBILL.Find(id) ?? new C_TB_HC_GOODSBILL();
            return View(model);
        }
        public ActionResult HeTong_xq(string CONTRACT_Guid)
        {
            C_TB_HC_CONTRACT model = db.C_TB_HC_CONTRACT.Find(CONTRACT_Guid) ?? new C_TB_HC_CONTRACT();
            List<C_TB_HC_CONTRACT_DETAILED> List_DETAILED = db.C_TB_HC_CONTRACT_DETAILED.Where(n => n.CONTRACT_Guid == CONTRACT_Guid).ToList();
            ViewBag.list = List_DETAILED;

            List<C_TB_HC_CONTRACT_FILES> filelist = db.C_TB_HC_CONTRACT_FILES.Where(n => n.CONTRACT_Guid == CONTRACT_Guid)
                .ToList();
            ViewBag.filelist = filelist;
            return View(model);
        }
        public JsonResult GetCONSIGNList(decimal id)
        {
            List<C_TB_HC_CONSIGN> list = db.C_TB_HC_CONSIGN.Where(n => n.GOODSBILL_ID == id && n.State != "待审核" && n.State != "被驳回").OrderByDescending(n => n.ID).ToList();
            foreach (var items_Consign in list)
            {
                decimal? FactNum = 0;
                decimal? FactNumW = 0;
                List<C_TB_HS_TALLYBILL> list_TALLYBILL = db.C_TB_HS_TALLYBILL.Where(n => n.CONSIGN_ID == items_Consign.ID && n.Type != "清场").OrderBy(n => n.ID).ToList();
                foreach (var items_TALLYBILL in list_TALLYBILL)
                {
                    //if (items_TALLYBILL.CODE_OPSTYPE == "进库")
                    //{
                    //    FactNum += items_TALLYBILL.AMOUNT;
                    //    FactNumW += items_TALLYBILL.WEIGHT;
                    //}
                    //if (items_TALLYBILL.CODE_OPSTYPE == "出库")
                    //{
                    //    FactNum -= items_TALLYBILL.AMOUNT;
                    //    FactNumW -= items_TALLYBILL.WEIGHT;
                    //}
                    FactNum += items_TALLYBILL.AMOUNT;
                    FactNumW += items_TALLYBILL.WEIGHT;
                }
                items_Consign.FactNum = FactNum;
                items_Consign.FactNumW = FactNumW;
            }

            var total = list.Count;
            var rows = list.ToList();
            return Json(new { total = total, rows = rows }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetTALLYBILLList(decimal id)
        {
            List<C_TB_HS_TALLYBILL> list = db.C_TB_HS_TALLYBILL.Where(n => n.CONSIGN_ID == id).OrderByDescending(n => n.ID).ToList();
            var total = list.Count;
            decimal CODE_SECTION = 0;
            foreach (var items in list)
            {
                CODE_SECTION = Convert.ToDecimal(items.CODE_SECTION);
                items.CODE_SECTION = db.C_TB_CODE_BOOTH.FirstOrDefault(n => n.ID == CODE_SECTION).BOOTH;
            }
            var rows = list.ToList();

            return Json(new { total = total, rows = rows }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        ///票货查询
        /// </summary>
        /// <returns></returns>
        public ActionResult SearGOODSBILLList()
        {
            OperatorModel loginModel = OperatorProvider.Provider.GetCurrent();
            ViewBag.roleId = loginModel.RoleId;
            return View();
        }
        /// <summary>
        ///票货查询
        /// </summary>
        /// <returns></returns>
        public ActionResult MoneyGOODSBILLList()
        {
            OperatorModel loginModel = OperatorProvider.Provider.GetCurrent();
            ViewBag.roleId = loginModel.RoleId;
            return View();
        }

        /// <summary>
        ///票货状态修改的方法
        /// </summary>
        /// <param name="id"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GoodsBillShenHe(int id, string state)
        {
            try
            {
                C_TB_HC_GOODSBILL model_GoodsBill = db.C_TB_HC_GOODSBILL.Find(id) ?? new C_TB_HC_GOODSBILL();
                model_GoodsBill.State = state;
                db.Set<C_TB_HC_GOODSBILL>().AddOrUpdate(model_GoodsBill);
                db.SaveChanges();
                return Json(new AjaxResult { state = ResultType.success.ToString(), message = "成功" });
            }
            catch (Exception e)
            {
                return Json(new AjaxResult { state = ResultType.error.ToString(), message = e.Message });
            }
        }
        public C_TB_HC_GOODSBILL GetKuCun(int id)
        {
            C_TB_HC_GOODSBILL model_GoodsBill = db.C_TB_HC_GOODSBILL.Find(id) ?? new C_TB_HC_GOODSBILL();
            decimal? KuCun = 0;
            decimal? KuCunW = 0;
            List<C_TB_HC_CONSIGN> list_CONSIGN = db.C_TB_HC_CONSIGN.Where(n => n.GOODSBILL_ID == model_GoodsBill.ID).OrderBy(n => n.ID).ToList();
            foreach (var items_Consign in list_CONSIGN)
            {
                List<C_TB_HS_TALLYBILL> list_TALLYBILL = db.C_TB_HS_TALLYBILL.Where(n => n.CONSIGN_ID == items_Consign.ID).OrderBy(n => n.ID).ToList();
                foreach (var items_TALLYBILL in list_TALLYBILL)
                {
                    if (items_TALLYBILL.CODE_OPSTYPE == "进库")
                    {
                        KuCun += items_TALLYBILL.AMOUNT;
                        KuCunW += items_TALLYBILL.WEIGHT;
                    }
                    if (items_TALLYBILL.CODE_OPSTYPE == "出库")
                    {
                        KuCun -= items_TALLYBILL.AMOUNT;
                        KuCunW -= items_TALLYBILL.WEIGHT;
                    }
                }
            }
            model_GoodsBill.KunCun = KuCun.ToString();
            model_GoodsBill.KunCunW = KuCunW.ToDecimal(3).ToString();
            return model_GoodsBill;
        }
    }
}