using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.IO;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using Main.HelpClass;
using MainBLL.Big;
using Newtonsoft.Json;
using NFine.Code;
using NFine.Code.Select;
using OracleBase.API;
using OracleBase.HelpClass;
using OracleBase.HelpClass.Sys;
using OracleBase.Models;

namespace OracleBase.Areas.ck.Controllers
{
    [SignLoginAuthorize]
    public class EntrustController : Controller
    {
        private Entities db = new Entities();
        // GET: ck/Entrust
        public ActionResult EntrustIndex()
        {
            return View();
        }
        public ActionResult EntrustIndex_dsh()
        {
            return View();
        }
        public ActionResult EntrustIndex_djlsh()
        {
            return View();
        }
        public ActionResult EntrustIndex_dbsx()
        {
            OperatorModel loginModel = OperatorProvider.Provider.GetCurrent();
            int Ht = db.Set<C_TB_HC_CONTRACT>().Where(n => n.State == "待经理审核" && n.YuanQuID == loginModel.YuanquID).Count();
            int Ph = db.Set<C_TB_HC_GOODSBILL>().Where(n => n.State == "待经理审核" && n.YuanQuID == loginModel.YuanquID).Count();
            int Wt = db.Set<C_TB_HC_CONSIGN>().Where(n => n.State == "待经理审核" && n.YuanQuID == loginModel.YuanquID).Count();
            ViewBag.Ht = Ht;
            ViewBag.Ph = Ph;
            ViewBag.Wt = Wt;
            return View();
        }
        [OperationLog(OperationLogAttribute.Operatetype.审核, OperationLogAttribute.ImportantLevel.一般操作, "委托通过")]
        [HttpPost]
        public JsonResult Ajaxtg(int id)
        {
            C_TB_HC_CONSIGN infoModel = db.C_TB_HC_CONSIGN.Find(id);
            infoModel.State = "待提交审核";
            db.Set<C_TB_HC_CONSIGN>().AddOrUpdate(infoModel);
            int c = db.SaveChanges();
            if (c > 0)
            {
                return Json("成功");
            }
            return Json("失败");
        }
        [OperationLog(OperationLogAttribute.Operatetype.审核, OperationLogAttribute.ImportantLevel.一般操作, "委托驳回")]
        [HttpPost]
        public JsonResult Ajaxbh(int id, string Shyj)
        {
            C_TB_HC_CONSIGN infoModel = db.C_TB_HC_CONSIGN.Find(id);
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
            C_TB_HC_CONSIGN model = db.C_TB_HC_CONSIGN.Find(id);
            return View(model);
        }
        [System.Web.Http.HttpGet]
        public object GetCONSIGNList(int limit, int offset, string CGNO, string GoodsBill_Num, string WeiTuoTime, string WeiTuoTime1, string WeiTuoRen, string PAPERYNO, string ShipName, string VGNO)
        {
            OperatorModel loginModel = OperatorProvider.Provider.GetCurrent();
            var wherelambda = ExtLinq.True<C_TB_HC_CONSIGN>();
            wherelambda = wherelambda.And(t => t.State != "待审核");
            wherelambda = wherelambda.And(t => t.State != "被驳回");
            wherelambda = wherelambda.And(t => t.YuanQuID == loginModel.YuanquID);
            if (!string.IsNullOrEmpty(CGNO))
            {
                wherelambda = wherelambda.And(t => t.CGNO.Contains(CGNO));
            }
            if (!string.IsNullOrEmpty(GoodsBill_Num))
            {
                wherelambda = wherelambda.And(t => t.GoodsBill_Num.Contains(GoodsBill_Num));
            }
            if (!string.IsNullOrEmpty(WeiTuoTime))
            {
                DateTime WeiTuoTime_date = Convert.ToDateTime(WeiTuoTime + " 00:00:00");
                wherelambda = wherelambda.And(n => n.WeiTuoTime >= WeiTuoTime_date);
                // list = list.Where(n => n.publish_time >= Convert.ToDateTime(fbsj) && n.publish_time <= Convert.ToDateTime(fbsj1)).ToList();
            }
            if (!string.IsNullOrEmpty(WeiTuoTime1))
            {
                DateTime WeiTuoTime1_date = Convert.ToDateTime(WeiTuoTime1 + " 23:59:59");
                wherelambda = wherelambda.And(n => n.WeiTuoTime <= WeiTuoTime1_date);
                // list = list.Where(n => n.publish_time >= Convert.ToDateTime(fbsj) && n.publish_time <= Convert.ToDateTime(fbsj1)).ToList();
            }
            if (!string.IsNullOrEmpty(WeiTuoRen))
            {
                wherelambda = wherelambda.And(t => t.BLNO.Contains(WeiTuoRen));
            }
            if (!string.IsNullOrEmpty(ShipName))
            {
                wherelambda = wherelambda.And(t => t.ShipName.Contains(ShipName));
            }
            if (!string.IsNullOrEmpty(VGNO))
            {
                wherelambda = wherelambda.And(t => t.VGNO.Contains(VGNO));
            }
            if (!string.IsNullOrEmpty(PAPERYNO))
            {
                wherelambda = wherelambda.And(t => t.PAPERYNO.Contains(PAPERYNO));
            }
            var list = db.Set<C_TB_HC_CONSIGN>().Where(wherelambda).OrderByDescending(n => n.CREATETIME).AsQueryable();
            foreach (var items_Consign in list)
            {
                decimal? FactNum = 0;
                decimal? FactNumW = 0;
                List<C_TB_HS_TALLYBILL> list_TALLYBILL = db.C_TB_HS_TALLYBILL.Where(n => n.CONSIGN_ID == items_Consign.ID).OrderBy(n => n.ID).ToList();
                foreach (var items_TALLYBILL in list_TALLYBILL)
                {
                    if (items_TALLYBILL.CODE_OPSTYPE == "进库")
                    {
                        FactNum += items_TALLYBILL.AMOUNT;
                        FactNumW += items_TALLYBILL.WEIGHT;
                    }
                    if (items_TALLYBILL.CODE_OPSTYPE == "出库")
                    {
                        FactNum += items_TALLYBILL.AMOUNT;
                        FactNumW += items_TALLYBILL.WEIGHT;
                    }
                }
                items_Consign.FactNum = FactNum;
                items_Consign.FactNumW = FactNumW;
            }
            int total = list.Count();
            object rows = list.Skip(offset).Take(limit).AsQueryable();
            return Json(new { total = total, rows = rows }, JsonRequestBehavior.AllowGet);

        }
        [System.Web.Http.HttpGet]
        public object GetCONSIGNList_dsh(int limit, int offset, string CGNO, string GoodsBill_Num, string WeiTuoTime, string WeiTuoTime1, string WeiTuoRen, string PAPERYNO, string ShipName, string VGNO)
        {
            OperatorModel loginModel = OperatorProvider.Provider.GetCurrent();
            var wherelambda = ExtLinq.True<C_TB_HC_CONSIGN>();
            wherelambda = wherelambda.And(t => t.State == "待审核");
            wherelambda = wherelambda.And(t => t.YuanQuID == loginModel.YuanquID);
            if (!string.IsNullOrEmpty(CGNO))
            {
                wherelambda = wherelambda.And(t => t.CGNO.Contains(CGNO));
            }
            if (!string.IsNullOrEmpty(GoodsBill_Num))
            {
                wherelambda = wherelambda.And(t => t.GoodsBill_Num.Contains(GoodsBill_Num));
            }
            if (!string.IsNullOrEmpty(WeiTuoTime))
            {
                DateTime WeiTuoTime_date = Convert.ToDateTime(WeiTuoTime + " 00:00:00");
                wherelambda = wherelambda.And(n => n.WeiTuoTime >= WeiTuoTime_date);
                // list = list.Where(n => n.publish_time >= Convert.ToDateTime(fbsj) && n.publish_time <= Convert.ToDateTime(fbsj1)).ToList();
            }
            if (!string.IsNullOrEmpty(WeiTuoTime1))
            {
                DateTime WeiTuoTime1_date = Convert.ToDateTime(WeiTuoTime1 + " 23:59:59");
                wherelambda = wherelambda.And(n => n.WeiTuoTime <= WeiTuoTime1_date);
                // list = list.Where(n => n.publish_time >= Convert.ToDateTime(fbsj) && n.publish_time <= Convert.ToDateTime(fbsj1)).ToList();
            }
            if (!string.IsNullOrEmpty(WeiTuoRen))
            {
                wherelambda = wherelambda.And(t => t.BLNO.Contains(WeiTuoRen));
            }
            if (!string.IsNullOrEmpty(ShipName))
            {
                wherelambda = wherelambda.And(t => t.ShipName.Contains(ShipName));
            }
            if (!string.IsNullOrEmpty(VGNO))
            {
                wherelambda = wherelambda.And(t => t.VGNO.Contains(VGNO));
            }
            if (!string.IsNullOrEmpty(PAPERYNO))
            {
                wherelambda = wherelambda.And(t => t.PAPERYNO.Contains(PAPERYNO));
            }
            var list = db.Set<C_TB_HC_CONSIGN>().Where(wherelambda).OrderByDescending(n => n.CREATETIME).AsQueryable();
            foreach (var items_Consign in list)
            {
                decimal? FactNum = 0;
                decimal? FactNumW = 0;
                List<C_TB_HS_TALLYBILL> list_TALLYBILL = db.C_TB_HS_TALLYBILL.Where(n => n.CONSIGN_ID == items_Consign.ID).OrderBy(n => n.ID).ToList();
                foreach (var items_TALLYBILL in list_TALLYBILL)
                {
                    if (items_TALLYBILL.CODE_OPSTYPE == "进库")
                    {
                        FactNum += items_TALLYBILL.AMOUNT;
                        FactNumW += items_TALLYBILL.WEIGHT;
                    }
                    if (items_TALLYBILL.CODE_OPSTYPE == "出库")
                    {
                        FactNum += items_TALLYBILL.AMOUNT;
                        FactNumW += items_TALLYBILL.WEIGHT;
                    }
                }
                items_Consign.FactNum = FactNum;
                items_Consign.FactNumW = FactNumW;
            }
            int total = list.Count();
            object rows = list.Skip(offset).Take(limit).AsQueryable();
            return Json(new { total = total, rows = rows }, JsonRequestBehavior.AllowGet);

        }
        [System.Web.Http.HttpGet]
        public object GetCONSIGNList_djlsh(int limit, int offset, string CGNO, string GoodsBill_Num, string WeiTuoTime, string WeiTuoTime1, string WeiTuoRen, string PAPERYNO, string ShipName, string VGNO)
        {
            OperatorModel loginModel = OperatorProvider.Provider.GetCurrent();
            var wherelambda = ExtLinq.True<C_TB_HC_CONSIGN>();
            wherelambda = wherelambda.And(t => t.State == "待经理审核");
            wherelambda = wherelambda.And(t => t.YuanQuID == loginModel.YuanquID);
            if (!string.IsNullOrEmpty(CGNO))
            {
                wherelambda = wherelambda.And(t => t.CGNO.Contains(CGNO));
            }
            if (!string.IsNullOrEmpty(GoodsBill_Num))
            {
                wherelambda = wherelambda.And(t => t.GoodsBill_Num.Contains(GoodsBill_Num));
            }
            if (!string.IsNullOrEmpty(WeiTuoTime))
            {
                DateTime WeiTuoTime_date = Convert.ToDateTime(WeiTuoTime + " 00:00:00");
                wherelambda = wherelambda.And(n => n.WeiTuoTime >= WeiTuoTime_date);
                // list = list.Where(n => n.publish_time >= Convert.ToDateTime(fbsj) && n.publish_time <= Convert.ToDateTime(fbsj1)).ToList();
            }
            if (!string.IsNullOrEmpty(WeiTuoTime1))
            {
                DateTime WeiTuoTime1_date = Convert.ToDateTime(WeiTuoTime1 + " 23:59:59");
                wherelambda = wherelambda.And(n => n.WeiTuoTime <= WeiTuoTime1_date);
                // list = list.Where(n => n.publish_time >= Convert.ToDateTime(fbsj) && n.publish_time <= Convert.ToDateTime(fbsj1)).ToList();
            }
            if (!string.IsNullOrEmpty(WeiTuoRen))
            {
                wherelambda = wherelambda.And(t => t.BLNO.Contains(WeiTuoRen));
            }
            if (!string.IsNullOrEmpty(ShipName))
            {
                wherelambda = wherelambda.And(t => t.ShipName.Contains(ShipName));
            }
            if (!string.IsNullOrEmpty(VGNO))
            {
                wherelambda = wherelambda.And(t => t.VGNO.Contains(VGNO));
            }
            if (!string.IsNullOrEmpty(PAPERYNO))
            {
                wherelambda = wherelambda.And(t => t.PAPERYNO.Contains(PAPERYNO));
            }
            var list = db.Set<C_TB_HC_CONSIGN>().Where(wherelambda).OrderByDescending(n => n.CREATETIME).AsQueryable();
            foreach (var items_Consign in list)
            {
                decimal? FactNum = 0;
                decimal? FactNumW = 0;
                List<C_TB_HS_TALLYBILL> list_TALLYBILL = db.C_TB_HS_TALLYBILL.Where(n => n.CONSIGN_ID == items_Consign.ID).OrderBy(n => n.ID).ToList();
                foreach (var items_TALLYBILL in list_TALLYBILL)
                {
                    if (items_TALLYBILL.CODE_OPSTYPE == "进库")
                    {
                        FactNum += items_TALLYBILL.AMOUNT;
                        FactNumW += items_TALLYBILL.WEIGHT;
                    }
                    if (items_TALLYBILL.CODE_OPSTYPE == "出库")
                    {
                        FactNum += items_TALLYBILL.AMOUNT;
                        FactNumW += items_TALLYBILL.WEIGHT;
                    }
                }
                items_Consign.FactNum = FactNum;
                items_Consign.FactNumW = FactNumW;
            }
            int total = list.Count();
            object rows = list.Skip(offset).Take(limit).AsQueryable();
            return Json(new { total = total, rows = rows }, JsonRequestBehavior.AllowGet);

        }
        [OperationLog(OperationLogAttribute.Operatetype.删除, OperationLogAttribute.ImportantLevel.危险操作, "删除委托")]
        [HttpPost]
        public JsonResult DelEntrustById(int id)
        {
            EFHelpler<C_TB_HC_CONSIGN> ef = new EFHelpler<C_TB_HC_CONSIGN>();
            try
            {
                C_TB_HC_CONSIGN model_CONSIGN = db.C_TB_HC_CONSIGN.Find(id) ?? new C_TB_HC_CONSIGN();
                C_TB_HC_CONSIGN u = new C_TB_HC_CONSIGN() { ID = id };
                ef.delete(u);

                C_TB_HC_GOODSBILL model_GoodsBill = db.C_TB_HC_GOODSBILL.Find(model_CONSIGN.GOODSBILL_ID) ?? new C_TB_HC_GOODSBILL();
                List<C_TB_HC_CONSIGN> list_CONSIGN = db.C_TB_HC_CONSIGN.Where(n => n.GOODSBILL_ID == model_CONSIGN.GOODSBILL_ID).OrderBy(n => n.ID).ToList();
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
        [OperationLog(OperationLogAttribute.Operatetype.删除, OperationLogAttribute.ImportantLevel.危险操作, "批量删除委托")]
        [HttpPost]
        public JsonResult DelEntrustList(string datalist)
        {
            List<C_TB_HC_CONSIGN> list = JsonConvert.DeserializeObject<List<C_TB_HC_CONSIGN>>(datalist);
            EFHelpler<C_TB_HC_CONSIGN> efse = new EFHelpler<C_TB_HC_CONSIGN>();

            try
            {

                efse.delete(list.ToArray());
                foreach (var items in list)
                {
                    C_TB_HC_GOODSBILL model_GoodsBill = db.C_TB_HC_GOODSBILL.Find(items.GOODSBILL_ID) ?? new C_TB_HC_GOODSBILL();
                    List<C_TB_HC_CONSIGN> list_CONSIGN = db.C_TB_HC_CONSIGN.Where(n => n.GOODSBILL_ID == items.GOODSBILL_ID).OrderBy(n => n.ID).ToList();
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
                }
                return Json(new AjaxResult { state = ResultType.success.ToString(), message = "成功！" }.ToJson());

            }
            catch (Exception e)
            {
                return Json(new AjaxResult { state = ResultType.error.ToString(), message = e.Message }.ToJson());
            }

        }



        public ActionResult Add_C_CONSIGN(int id_GoodsBill, int id)
        {
            C_TB_HC_CONSIGN model = db.C_TB_HC_CONSIGN.Find(id) ?? new C_TB_HC_CONSIGN();
            C_TB_HC_GOODSBILL model_GoodsBill = db.C_TB_HC_GOODSBILL.Find(id_GoodsBill) ?? new C_TB_HC_GOODSBILL();
            List<C_GOODSTYPE> list_GOODSTYPE = db.C_GOODSTYPE.OrderBy(n => n.ID).ToList();//货物类型
            List<SelectListItem> emplist_C_GOODSTYPE = SelectHelp.CreateSelect<C_GOODSTYPE>(list_GOODSTYPE, "GoodsName", "GoodsName", null);
            ViewData["GOODSTYPE_List"] = new SelectList(emplist_C_GOODSTYPE, "Value", "Text", "是");

            List<C_GOODS> list_GOODS = db.C_GOODS.OrderBy(n => n.ID).ToList();//货物
            List<SelectListItem> emplist_C_GOODS = SelectHelp.CreateSelect<C_GOODS>(list_GOODS, "GoodsName", "GoodsName", null);
            ViewData["GOODS_List"] = new SelectList(emplist_C_GOODS, "Value", "Text", "是");

            if (id != 0)
            {
                ViewBag.BLNO = model.BLNO;
                ViewBag.PLANAMOUNT = model.PLANAMOUNT;
                ViewBag.PLANWEIGHT = model.PLANWEIGHT;
                ViewBag.GoodsName = model.GoodsName;
                ViewBag.WeiTuoRen = model.WeiTuoRen;
                ViewBag.GoodsType = model.GoodsType;
                ViewBag.VGNO = model.VGNO;
                ViewBag.ShipName = model.ShipName;
            }
            else
            {
                ViewBag.BLNO = model_GoodsBill.BLNO;//提单
                ViewBag.PLANAMOUNT = model_GoodsBill.PLANAMOUNT;//计划件数
                ViewBag.PLANWEIGHT = model_GoodsBill.PLANWEIGHT;//计划重量
                ViewBag.GoodsName = model_GoodsBill.C_GOODS;//货物名称
                ViewBag.WeiTuoRen = model_GoodsBill.C_GOODSAGENT_NAME;//委托人（船代）
                ViewBag.GoodsType = model_GoodsBill.GoodsType;//货物类型
                ViewBag.VGNO = model_GoodsBill.VGNO;//航次
                ViewBag.ShipName = model_GoodsBill.ShipName;//船名
            }
            ViewBag.id_GoodsBill = id_GoodsBill;
            C_TB_HC_GOODSBILL model_GoodsBill_Num = db.C_TB_HC_GOODSBILL.Find(id_GoodsBill) ?? new C_TB_HC_GOODSBILL();
            ViewBag.GoodsBill_Num = model_GoodsBill_Num.GBNO;
            return View(model);
        }
        [OperationLog(OperationLogAttribute.Operatetype.添加或编辑, OperationLogAttribute.ImportantLevel.一般操作, "添加或编辑委托")]
        [HttpPost]
        public JsonResult Add_C_CONSIGN(C_TB_HC_CONSIGN model)
        {
            OperatorModel loginModel = OperatorProvider.Provider.GetCurrent();
            if (string.IsNullOrEmpty(model.CGNO))           //新建时生成流水号
            {
                string TodayTime = DateTime.Today.ToString("yyyyMMdd");
                C_TB_HC_CONSIGN Num = db.C_TB_HC_CONSIGN.OrderByDescending(n => n.CREATETIME).ToList().FirstOrDefault();
                if (Num != null)
                {
                    if (!string.IsNullOrEmpty(Num.CGNO))
                    {
                        if (Num.CGNO.Substring(2, 8) == TodayTime)
                        {
                            model.CGNO = "WT" + TodayTime + (Convert.ToInt32(Num.CGNO.Replace("WT" + TodayTime, "")) + 1).ToString("0000");
                        }
                        else
                        {
                            model.CGNO = "WT" + TodayTime + "0001";
                        }
                    }
                }

                else
                {
                    model.CGNO = "WT" + TodayTime + "0001";
                }

            }
            int stateid = 0;
            if (model.ID == 0)
            {
                model.CREATORNAME = loginModel.UserName;
                model.CREATETIME = DateTime.Now;
                stateid = 1;
                model.State = MainBLL.GoodBill.WtStateEnum.待提交审核.ToString();
                model.YuanQuID = loginModel.YuanquID;
                model.LaiYuan = "业务员";

            }

            db.Set<C_TB_HC_CONSIGN>().AddOrUpdate(model);

            try
            {
                if (model.CONTAINERTYPE== "场-汽")
                {
                    GoodsBillController Gb = new GoodsBillController();
                    C_TB_HC_GOODSBILL model_GoodsBill_gt = Gb.GetKuCun(model.GOODSBILL_ID.ToInt());//获取库存
                    if (model.PLANAMOUNT > model_GoodsBill_gt.KunCun.ToInt() - model_GoodsBill_gt.SuoHuoKunCun)//计划重量大于库存-锁库
                    {
                        return Json(new AjaxResult { state = ResultType.error.ToString(), message = "失败！最大可出库委托数量为"+ (model_GoodsBill_gt.KunCun.ToInt() - model_GoodsBill_gt.SuoHuoKunCun).ToString() + "件" });
                    }
                    if (model.PLANWEIGHT > model_GoodsBill_gt.KunCunW.ToInt() - model_GoodsBill_gt.SuoHuoKunCunW)//计划重量大于库存-锁库
                    {
                        return Json(new AjaxResult { state = ResultType.error.ToString(), message = "失败！最大可出库委托数量为" + (model_GoodsBill_gt.KunCunW.ToInt() - model_GoodsBill_gt.SuoHuoKunCunW).ToString() + "吨" });
                    }
                }
               
                db.SaveChanges();
                C_TB_HC_GOODSBILL model_GoodsBill = db.C_TB_HC_GOODSBILL.Find(model.GOODSBILL_ID) ?? new C_TB_HC_GOODSBILL();
                List<C_TB_HC_CONSIGN> list_CONSIGN = db.C_TB_HC_CONSIGN.Where(n => n.GOODSBILL_ID == model.GOODSBILL_ID).OrderBy(n => n.ID).ToList();
                if (list_CONSIGN.Count != 0)
                {
                    model_GoodsBill.State = "已生成" + list_CONSIGN.Count + "条委托";
                }
                else
                {
                    model_GoodsBill.State = "进行中";
                }
                db.Set<C_TB_HC_GOODSBILL>().AddOrUpdate(model_GoodsBill);
                int c = db.SaveChanges();
                if (c > 0)
                {
                    return Json(new AjaxResult { state = ResultType.success.ToString(), message = "成功！", data = model.ID });
                }
                else
                {
                    if (c == 0)
                    {
                        return Json(new AjaxResult { state = ResultType.success.ToString(), message = "成功！", data = model.ID });
                    }
                    return Json(new AjaxResult { state = ResultType.error.ToString(), message = "失败！" });
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
                return Json(msg);
            }
            catch (Exception ex)
            {
                Log4NetHelper log = new Log4NetHelper();
                log.Error(ex.Message, ex);
                return Json(ex.InnerException.InnerException.Message);
            }

        }

        [OperationLog(OperationLogAttribute.Operatetype.编辑, OperationLogAttribute.ImportantLevel.危险操作, "审核委托")]
        [HttpPost]
        public JsonResult shenheCONSIGN(int id)
        {

            try
            {
                using (var scope = new TransactionScope(TransactionScopeOption.Required))
                {
                    C_TB_HC_CONSIGN model_CONSIGN = db.C_TB_HC_CONSIGN.Find(id) ?? new C_TB_HC_CONSIGN();
                    model_CONSIGN.State = "已完成";
                    db.Set<C_TB_HC_CONSIGN>().AddOrUpdate(model_CONSIGN);
                    db.SaveChanges();
                    List<C_TB_HS_TALLYBILL> list_TALLYBILL = db.C_TB_HS_TALLYBILL
                        .Where(n => n.CONSIGN_ID == model_CONSIGN.ID).OrderBy(n => n.ID).ToList();
                    foreach (var items_TALLYBILL in list_TALLYBILL)
                    {
                        C_TB_HS_TALLYBILL model_TALLYBILL =
                            db.C_TB_HS_TALLYBILL.Find(items_TALLYBILL.ID) ?? new C_TB_HS_TALLYBILL();
                        model_TALLYBILL.State = "已完成";
                        db.Set<C_TB_HS_TALLYBILL>().AddOrUpdate(model_TALLYBILL);
                        db.SaveChanges();
                    }

                    if (!string.IsNullOrEmpty(model_CONSIGN.Ckdh))
                    {
                        if (list_TALLYBILL.Sum(n=>n.WEIGHT)<=0)
                        {
                            return Json(new AjaxResult { state = ResultType.error.ToString(), message = "实提数量小于或者等于0" }.ToJson());
                        }
                        BigController big = new BigController();
                        bool a = big.PostEditCkSt(id);
                        if (a == true)
                        {
                            scope.Complete();
                            return Json(new AjaxResult
                            { state = ResultType.success.ToString(), message = "成功" }.ToJson());
                        }
                        else
                        {
                            return Json(new AjaxResult { state = ResultType.error.ToString(), message = "失败" }.ToJson());
                        }
                    }
                    else
                    {
                        scope.Complete();
                    }
                    return Json(new AjaxResult { state = ResultType.success.ToString(), message = "成功" }.ToJson());
                }
            }
            catch (Exception e)
            {
                return Json(new AjaxResult { state = ResultType.error.ToString(), message = e.Message }.ToJson());
            }
        }
        public JsonResult qx_shenheCONSIGN(int id)
        {

            try
            {
                C_TB_HC_CONSIGN model_CONSIGN = db.C_TB_HC_CONSIGN.Find(id) ?? new C_TB_HC_CONSIGN();
                List<C_TB_HS_TALLYBILL> list_TALLYBILL = db.C_TB_HS_TALLYBILL.Where(n => n.CONSIGN_ID == id).OrderBy(n => n.ID).ToList();
                if (list_TALLYBILL.Count != 0)
                {
                    model_CONSIGN.State = "已生成" + list_TALLYBILL.Count + "条委托";
                }
                else
                {
                    model_CONSIGN.State = "进行中";
                }
                db.Set<C_TB_HC_CONSIGN>().AddOrUpdate(model_CONSIGN);
                db.SaveChanges();
                return Json(new AjaxResult { state = ResultType.success.ToString(), message = "成功" }.ToJson());
            }
            catch (Exception e)
            {
                return Json(new AjaxResult { state = ResultType.error.ToString(), message = e.Message });
            }
        }
        public ActionResult CONSIGN_ck(int id_GoodsBill, int id)
        {

            C_TB_HC_CONSIGN model = db.C_TB_HC_CONSIGN.Find(id) ?? new C_TB_HC_CONSIGN();
            C_TB_HC_GOODSBILL model_GoodsBill = db.C_TB_HC_GOODSBILL.Find(id_GoodsBill) ?? new C_TB_HC_GOODSBILL();
            if (id != 0)
            {
                ViewBag.BLNO = model.BLNO;
                ViewBag.PLANAMOUNT = model.PLANAMOUNT;
                ViewBag.PLANWEIGHT = model.PLANWEIGHT;
            }
            else
            {
                ViewBag.BLNO = model_GoodsBill.BLNO;
                ViewBag.PLANAMOUNT = model_GoodsBill.PLANAMOUNT;
                ViewBag.PLANWEIGHT = model_GoodsBill.PLANWEIGHT;
            }
            ViewBag.id_GoodsBill = id_GoodsBill;
            C_TB_HC_GOODSBILL model_GoodsBill_Num = db.C_TB_HC_GOODSBILL.Find(id_GoodsBill) ?? new C_TB_HC_GOODSBILL();
            ViewBag.GoodsBill_Num = model_GoodsBill_Num.GBNO;
            return View(model);
        }

        /// <summary>
        /// 添加委托的附件
        /// </summary>
        /// <param name="CONSING_ID"></param>
        /// <returns></returns>
        public ActionResult AddConsignFile(string CONSING_ID = null)
        {
            ViewBag.CONSING_ID = CONSING_ID;
            List<C_TB_HC_CONSIGN_FILES> list = db.C_TB_HC_CONSIGN_FILES.Where(n => n.CONSING_ID == CONSING_ID).ToList();
            ViewBag.list = list;
            return View();
        }
        [OperationLog(OperationLogAttribute.Operatetype.添加, OperationLogAttribute.ImportantLevel.一般操作, "添加委托附件")]
        public ActionResult WebuploaderUpLoadProcess(string id, string name, string type, string lastModifiedDate, int size, HttpPostedFileBase file, string CONSING_ID)
        {
            //string CONTRACT_Guid = null;

            //保存到临时文件夹
            string urlPath = "Upload/" + CONSING_ID;
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
            C_TB_HC_CONSIGN_FILES f = new C_TB_HC_CONSIGN_FILES()
            {
                CONSING_ID = CONSING_ID,
                path = urlPath + "/" + filePathName,
                FileName = name,
                Guid = Guid.NewGuid().ToString(),
                State = "进行中"

            };
            db.C_TB_HC_CONSIGN_FILES.Add(f);
            db.SaveChanges();
            return Json(new
            {
                jsonrpc = "2.0",
                id = id,
                filePath = urlPath + "/" + filePathName//返回一个视图界面可直接使用的url
            });

        }
        [OperationLog(OperationLogAttribute.Operatetype.删除, OperationLogAttribute.ImportantLevel.危险操作, "删除委托附件")]
        [HttpPost]
        public ActionResult DelConsignFile(string guid)
        {
            C_TB_HC_CONSIGN_FILES u = new C_TB_HC_CONSIGN_FILES() { Guid = guid };
            db.C_TB_HC_CONSIGN_FILES.Attach(u);
            db.C_TB_HC_CONSIGN_FILES.Remove(u);
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

        [OperationLog(OperationLogAttribute.Operatetype.编辑, OperationLogAttribute.ImportantLevel.危险操作, "审核委托")]
        [HttpPost]
        public JsonResult WtUpdateState(int id, string state)
        {
            try
            {
                C_TB_HC_CONSIGN model_CONSIGN = db.C_TB_HC_CONSIGN.Find(id) ?? new C_TB_HC_CONSIGN();
                model_CONSIGN.State = state;
                db.Set<C_TB_HC_CONSIGN>().AddOrUpdate(model_CONSIGN);
                db.SaveChanges();

                return Json(new AjaxResult { state = ResultType.success.ToString(), message = "成功" });
            }
            catch (Exception e)
            {
                return Json(new AjaxResult { state = ResultType.error.ToString(), message = e.Message });
            }
        }
    }
}