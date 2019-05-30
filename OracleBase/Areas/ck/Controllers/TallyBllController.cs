using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Migrations;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Main.HelpClass;
using MainBLL.TallyBLL;
using NFine.Code;
using NFine.Code.Select;
using NPOI.HSSF.UserModel;
using NPOI.HSSF.Util;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using OracleBase.HelpClass;
using OracleBase.HelpClass.Sys;
using OracleBase.Models;



namespace OracleBase.Areas.ck.Controllers
{
    /// <summary>
    /// TallyBll  理货单
    /// </summary>
    [SignLoginAuthorize]
    public class TallyBllController : Controller
    {
        private Entities db = new Entities();

        // GET: ck/TallyBll
        public ActionResult TallyBllIndex()
        {
            return View();
        }

        [System.Web.Http.HttpGet]
        public object GetTallyBllList(int limit, int offset, string TBNO, string CGNO, string BEGINTIME, string ENDTIME,
            string huodai, string banci, string chuanming, string hangci)
        {
            OperatorModel loginModel = OperatorProvider.Provider.GetCurrent();
            var yuanquid = loginModel.YuanquID ?? throw new ArgumentNullException("loginModel.YuanquID");

            var wherelambda = ExtLinq.True<C_TB_HS_TALLYBILL>();
            wherelambda = wherelambda.And(t => t.Type != "清场" && t.YuanQuID == yuanquid);
            if (!string.IsNullOrEmpty(TBNO))
            {
                wherelambda = wherelambda.And(t => t.TBNO.Contains(TBNO));
            }

            if (!string.IsNullOrEmpty(CGNO))
            {
                wherelambda = wherelambda.And(t => t.CGNO.Contains(CGNO));
            }

            if (!string.IsNullOrEmpty(BEGINTIME))
            {
                DateTime BEGINTIME_date = Convert.ToDateTime(BEGINTIME + " 00:00:00");
                wherelambda = wherelambda.And(n => n.SIGNDATE >= BEGINTIME_date);

            }

            if (!string.IsNullOrEmpty(ENDTIME))
            {
                DateTime ENDTIME_date = Convert.ToDateTime(ENDTIME + " 23:59:59");
                wherelambda = wherelambda.And(n => n.SIGNDATE <= ENDTIME_date);

            }

            if (!string.IsNullOrEmpty(huodai))
            {
                wherelambda = wherelambda.And(t => t.HuoDai.Contains(huodai));
            }

            if (!string.IsNullOrEmpty(banci))
            {
                wherelambda = wherelambda.And(t => t.BanCi == banci);
            }

            if (!string.IsNullOrEmpty(chuanming))
            {
                wherelambda = wherelambda.And(t => t.ChuanMing.Contains(chuanming));
            }

            if (!string.IsNullOrEmpty(hangci))
            {
                wherelambda = wherelambda.And(t => t.HangCi.Contains(hangci));
            }

            var list = db.Set<C_TB_HS_TALLYBILL>().Where(wherelambda).OrderByDescending(n => n.ID).AsQueryable();
            decimal CODE_SECTION = 0;
            foreach (var items in list)
            {
                CODE_SECTION = Convert.ToDecimal(items.CODE_SECTION);
                items.CODE_SECTION = db.C_TB_CODE_BOOTH.FirstOrDefault(n => n.ID == CODE_SECTION).BOOTH;
            }

            int total = list.Count();
            object rows = list.Skip(offset).Take(limit).AsQueryable();
            return Json(new {total = total, rows = rows}, JsonRequestBehavior.AllowGet);

        }

        /// <summary>
        /// 添加理货单
        /// </summary>
        /// <returns></returns>
        public ActionResult AddTallyBll(decimal CONSIGN_ID, int id,string type)
        {
            OperatorModel loginModel = OperatorProvider.Provider.GetCurrent();
            C_TB_HS_TALLYBILL model = db.C_TB_HS_TALLYBILL.Find(id) ?? new C_TB_HS_TALLYBILL();
            model.TALLYMAN = loginModel.UserName;
            C_TB_HC_CONSIGN model_CONSIGN = db.C_TB_HC_CONSIGN.Find(CONSIGN_ID) ?? new C_TB_HC_CONSIGN();
            model.ChuanMing = model_CONSIGN.ShipName;
            model.HangCi = model_CONSIGN.VGNO;
            model.HuoDai = model_CONSIGN.WeiTuoRen;
            model.GoodsName = model_CONSIGN.GoodsName;
            model.CAOZUO = model_CONSIGN.CODE_OPERATION; //作业过程


            List<SelectListItem> STORAGEList = GetChang();
            ViewData["STORAGEList"] = new SelectList(STORAGEList, "Value", "Text");
            ViewBag.CONSIGN_ID = CONSIGN_ID;
            ViewBag.CGNO = model_CONSIGN.CGNO;
            ViewBag.RoleId = loginModel.RoleId;
            ViewBag.type = type;
            return View(model);
        }
        [OperationLog(OperationLogAttribute.Operatetype.添加或编辑, OperationLogAttribute.ImportantLevel.一般操作, "添加或编辑理货单")]
        [HttpPost]
        public ActionResult AddTallyBll(C_TB_HS_TALLYBILL model)
        {
            decimal? KuCun = 0;
            decimal? KuCunW = 0;
            decimal? KuCUnX = 0;
            OperatorModel loginModel = OperatorProvider.Provider.GetCurrent();
            C_TB_HC_CONSIGN model_CONSIGN_sh = db.C_TB_HC_CONSIGN.FirstOrDefault(n=>n.ID==model.CONSIGN_ID) ?? new C_TB_HC_CONSIGN();
            C_TB_HC_GOODSBILL model_GOODSBILL = db.C_TB_HC_GOODSBILL.FirstOrDefault(n => n.ID == model_CONSIGN_sh.GOODSBILL_ID) ?? new C_TB_HC_GOODSBILL();
            List<C_TB_HC_CONSIGN> list_CONSIGN = db.C_TB_HC_CONSIGN.Where(n => n.GOODSBILL_ID == model_GOODSBILL.ID).OrderBy(n => n.ID).ToList();
            foreach (var items_Consign in list_CONSIGN)
            {
                List<C_TB_HS_TALLYBILL> list_TALLYBILL = db.C_TB_HS_TALLYBILL.Where(n => n.CONSIGN_ID == items_Consign.ID).OrderBy(n => n.ID).ToList();
                foreach (var items_TALLYBILL in list_TALLYBILL)
                {
                    if (items_TALLYBILL.CODE_OPSTYPE == "进库")
                    {
                        KuCun += items_TALLYBILL.AMOUNT;
                        KuCunW += items_TALLYBILL.WEIGHT;
                        KuCUnX += items_TALLYBILL.XIANGSHU;
                    }
                    if (items_TALLYBILL.CODE_OPSTYPE == "出库")
                    {
                        KuCun -= items_TALLYBILL.AMOUNT;
                        KuCunW -= items_TALLYBILL.WEIGHT;
                        KuCUnX -= items_TALLYBILL.XIANGSHU;
                    }
                }
            }
         
            
            var yuanquid = loginModel.YuanquID ?? throw new ArgumentNullException("loginModel.YuanquID");
            model.YuanQuID = yuanquid;
            decimal AorU = model.ID;

            if (model.TBNO == null)
            {
                string TodayTime = DateTime.Today.ToString("yyyyMMdd");
                C_TB_HS_TALLYBILL Num = db.C_TB_HS_TALLYBILL.OrderByDescending(n => n.TBNO)
                    .FirstOrDefault();
                if (Num != null)
                {
                    if (!string.IsNullOrEmpty(Num.TBNO))
                    {
                        if (Num.TBNO.Substring(2, 8) == TodayTime)
                        {
                            model.TBNO = "LH" + TodayTime +
                                         (Convert.ToInt32(Num.TBNO.Replace("LH" + TodayTime, "")) + 1).ToString("0000");
                        }
                        else
                        {
                            model.TBNO = "LH" + TodayTime + "0001";
                        }
                    }
                }

                else
                {
                    model.TBNO = "LH" + TodayTime + "0001";
                }
            }

            if (model.CAOZUO == "其他") //借磅的情况（件数和重量都=0，这样库存就不相加了）
            {
                model.AMOUNT = 0; //件数
                model.WEIGHT = 0; //重量
                model.PIECEWEIGHT = 0; //件重
                model.XIANGSHU = 0; //箱数

            }

            var cgno = model.CGNO;
            if (model.Type=="清场")
            {
                model.CreatTime = DateTime.Now;
            }
            C_TB_HC_CONSIGN con = db.C_TB_HC_CONSIGN.FirstOrDefault(n => n.CGNO == cgno);
            if (con != null) model.CONSIGN_ID = con.ID;
            if (model.ID == 0)
            {
                model.State = "进行中";
            }
            try
            {
                List<C_TB_HC_CONSIGN> list_wth =
                    db.C_TB_HC_CONSIGN.Where(n => n.CGNO == model.CGNO).OrderBy(n => n.ID).ToList();
                if (list_wth.Count == 0)
                {
                    return Json("找不到对应的委托号");
                }

                List<C_TB_HC_CONSIGN> list_wth1 = db.C_TB_HC_CONSIGN
                    .Where(n => n.CGNO == model.CGNO && n.State != "已完成").OrderBy(n => n.ID).ToList();
                if (list_wth1.Count == 0)
                {
                    return Json("该委托号流程已结束");
                }

                db.C_TB_HS_TALLYBILL.AddOrUpdate(model);
                db.SaveChanges();
                bool b = EidSTOCKDORMANT(model, AorU);
                C_TB_HC_CONSIGN model_CONSIGN = db.C_TB_HC_CONSIGN.Find(model.CONSIGN_ID) ?? new C_TB_HC_CONSIGN();
                List<C_TB_HS_TALLYBILL> list_TALLYBILL = db.C_TB_HS_TALLYBILL
                    .Where(n => n.CONSIGN_ID == model.CONSIGN_ID).OrderBy(n => n.ID).ToList();
                if (list_TALLYBILL.Count != 0)
                {
                    model_CONSIGN.State = "已生成" + list_TALLYBILL.Count + "条理货单";
                }
                else
                {
                    model_CONSIGN.State = "进行中";
                }

                db.Set<C_TB_HC_CONSIGN>().AddOrUpdate(model_CONSIGN);
                db.SaveChanges();
                if (b)
                {
                    return Json("成功");
                }
                else
                {
                    db.C_TB_HS_TALLYBILL.Remove(model);
                    db.SaveChanges();
                    return Json("更新库存失败");
                }
            }

            catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            {
                var msg = string.Empty;
                var errors = (from u in ex.EntityValidationErrors select u.ValidationErrors).ToList();
                foreach (var item in errors)
                    msg += item.FirstOrDefault().ErrorMessage;

                return Json(new AjaxResult {state = ResultType.error.ToString(), message = msg});
            }
            catch (Exception ex)
            {
                return Json(new AjaxResult { state = ResultType.error.ToString(), message = ex.ToString() });
            }



        }

        /// <summary>
        ///编辑库存
        /// </summary>
        /// <returns></returns>
        public bool EidSTOCKDORMANT(C_TB_HS_TALLYBILL model, decimal AorU)
        {
            string Id = model.ID.ToString();
            C_TB_HS_STOCKDORMANT stocModel = db.C_TB_HS_STOCKDORMANT.FirstOrDefault(n => n.TALLYBILL_ID == Id) ??
                                             new C_TB_HS_STOCKDORMANT(); //理货单号
            C_TB_HC_CONSIGN Model_CONSIGN = db.C_TB_HC_CONSIGN.FirstOrDefault(n => n.ID == model.CONSIGN_ID); //委托号
            C_TB_HC_GOODSBILL Model_GoodsBill =
                db.C_TB_HC_GOODSBILL.FirstOrDefault(n => n.ID == Model_CONSIGN.GOODSBILL_ID); //票货编号
            stocModel.ID = stocModel.ID;
            stocModel.TALLYBILL_ID = model.ID.ToString();
            stocModel.CODE_STORAGE = model.STORAG; //场
            stocModel.CODE_BOOTH = model.CODE_SECTION; //位
            stocModel.GBNO = model.CGNO; //委托
            if (model.CODE_OPSTYPE == "进库")
            {
                stocModel.AMOUNT = model.AMOUNT; //件数
                stocModel.WEIGHT = model.WEIGHT; //重量
                stocModel.VOLUME = model.VOLUME; //体积
            }
            else
            {
                stocModel.AMOUNT = -model.AMOUNT; //件数
                stocModel.WEIGHT = -model.WEIGHT; //重量
                stocModel.VOLUME = -model.VOLUME; //体积
            }

            stocModel.FIRST_INDATE = DateTime.Now;
            stocModel.BOOTH_INDATE = DateTime.Now;
            stocModel.REMARK = model.CODE_OPSTYPE;
            stocModel.LastEidTime = DateTime.Now;
            stocModel.HuoDai = Model_GoodsBill.C_GOODSAGENT_NAME ?? null;
            stocModel.GoodsName = Model_GoodsBill.C_GOODS ?? null;

            stocModel.YuanQuID = model.YuanQuID;
            db.C_TB_HS_STOCKDORMANT.AddOrUpdate(stocModel);
            try
            {
                db.SaveChanges();
                return true;
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            {
                var msg = string.Empty;
                var errors = (from u in ex.EntityValidationErrors select u.ValidationErrors).ToList();
                foreach (var item in errors)
                    msg += item.FirstOrDefault().ErrorMessage;

                return false;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        /// <summary>
        /// 在新建理货单的时候查找委托
        /// </summary>
        /// <returns></returns>     
        [ValidateInput(false)]
        public string GetEntrustListByCode(string Keyword)
        {
            List<C_TB_HC_CONSIGN> list = db.C_TB_HC_CONSIGN.Where(n => n.CGNO.Contains(Keyword) && n.State != "已完成")
                .ToList();
            List<SuggesTallyBLL> stb = list.Select(x => new SuggesTallyBLL
                {name = x.CGNO, ename = x.WeiTuoRen, jobtitle = x.ShouHuoRen, hengzhong = x.HengZhong}).ToList();
            JsonHelper jshelp = new JsonHelper();
            return jshelp.List2JSON(stb);
        }

        /// <summary>
        /// 根据登录人获取所有场  的列表
        /// </summary>
        /// <returns></returns>
        public List<SelectListItem> GetChang()
        {
            OperatorModel loginModel = OperatorProvider.Provider.GetCurrent();
            var yuanquid = loginModel.YuanquID ?? throw new ArgumentNullException("loginModel.YuanquID");
            List<C_TB_CODE_SECTION> sections = db.C_TB_CODE_SECTION.Where(n => n.CODE_COMPANY == yuanquid).ToList();
            decimal[] id = new decimal[sections.Count];
            for (int j = 0; j < sections.Count; j++)
            {
                id[j] = (int) sections[j].ID;
            }

            List<C_TB_CODE_STORAGE> storages = db.C_TB_CODE_STORAGE.Where(n => id.Contains(n.SECTION_ID)).ToList();
            List<SelectListItem> emplist =
                SelectHelp.CreateSelect<C_TB_CODE_STORAGE>(storages, "STORAGEName", "ID", null);
            return emplist;
        }

        [HttpGet]
        public string GetBoothByChangId(int id)
        {

            List<C_TB_CODE_BOOTH> sections = db.C_TB_CODE_BOOTH.Where(n => n.Storage_ID == id).ToList();
            List<Booth> stb = sections.Select(x => new Booth {ID = x.ID, BoothName = x.BOOTH}).ToList();
            JsonHelper jshelp = new JsonHelper();
            return jshelp.List2JSON(stb);

        }
        [OperationLog(OperationLogAttribute.Operatetype.删除, OperationLogAttribute.ImportantLevel.危险操作, "删除理货单")]
        [HttpPost]
        public JsonResult DelTALLYBILLById(int id)
        {
            string id1 = id.ToString();
            EFHelpler<C_TB_HS_TALLYBILL> ef = new EFHelpler<C_TB_HS_TALLYBILL>();
            EFHelpler<C_TB_HS_STOCKDORMANT> ef1 = new EFHelpler<C_TB_HS_STOCKDORMANT>();
            EFHelpler<BS_ZYLB_TBLL> ef2 = new EFHelpler<BS_ZYLB_TBLL>();
            try
            {
                List<BS_ZYLB_TBLL> list_ZYLB_TBLL = db.BS_ZYLB_TBLL.Where(n => n.TallBllId == id).ToList();
                if (list_ZYLB_TBLL.Count>0)
                {
                    ef2.delete(list_ZYLB_TBLL.ToArray());
                }
                C_TB_HS_TALLYBILL model_TALLYBILL = db.C_TB_HS_TALLYBILL.Find(id) ?? new C_TB_HS_TALLYBILL();
                C_TB_HS_STOCKDORMANT
                    stocModel = db.C_TB_HS_STOCKDORMANT.FirstOrDefault(n => n.TALLYBILL_ID == id1); //委托号
                ef.delete(model_TALLYBILL);
                ef1.delete(stocModel);
                C_TB_HC_CONSIGN model_CONSIGN =
                    db.C_TB_HC_CONSIGN.Find(model_TALLYBILL.CONSIGN_ID) ?? new C_TB_HC_CONSIGN();
                List<C_TB_HS_TALLYBILL> list_ALLYBILL = db.C_TB_HS_TALLYBILL
                    .Where(n => n.CONSIGN_ID == model_TALLYBILL.CONSIGN_ID).OrderBy(n => n.ID).ToList();
                if (list_ALLYBILL.Count != 0)
                {
                    model_CONSIGN.State = "已生成" + list_ALLYBILL.Count + "条理货单";
                }
                else
                {
                    model_CONSIGN.State = "进行中";
                }

                db.Set<C_TB_HC_CONSIGN>().AddOrUpdate(model_CONSIGN);
                db.SaveChanges();
                return Json(new AjaxResult {state = ResultType.success.ToString(), message = "成功"}.ToJson());
            }
            catch (Exception e)
            {
                return Json(new AjaxResult {state = ResultType.error.ToString(), message = e.Message}.ToJson());
            }
        }
        [OperationLog(OperationLogAttribute.Operatetype.编辑, OperationLogAttribute.ImportantLevel.一般操作, "审核理货单")]
        [HttpPost]
        public JsonResult shenheTALLYBILL(int id)
        {

            try
            {
                OperatorModel loginModel = OperatorProvider.Provider.GetCurrent();

                C_TB_HS_TALLYBILL model_TALLYBILL = db.C_TB_HS_TALLYBILL.Find(id) ?? new C_TB_HS_TALLYBILL();
                model_TALLYBILL.State = "已完成";
                model_TALLYBILL.Shr = loginModel.UserName;
                db.Set<C_TB_HS_TALLYBILL>().AddOrUpdate(model_TALLYBILL);
                db.SaveChanges();
                return Json(new AjaxResult {state = ResultType.success.ToString(), message = "成功"}.ToJson());
            }
            catch (Exception e)
            {
                return Json(new AjaxResult {state = ResultType.error.ToString(), message = e.Message}.ToJson());
            }
        }
        [OperationLog(OperationLogAttribute.Operatetype.编辑, OperationLogAttribute.ImportantLevel.一般操作, "审核理货单")]
        [HttpPost]
        public JsonResult qx_shenheTALLYBILL(int id)
        {

            try
            {
                C_TB_HS_TALLYBILL model_TALLYBILL = db.C_TB_HS_TALLYBILL.Find(id) ?? new C_TB_HS_TALLYBILL();
                model_TALLYBILL.State = "进行中";
                db.Set<C_TB_HS_TALLYBILL>().AddOrUpdate(model_TALLYBILL);
                db.SaveChanges();
                return Json(new AjaxResult {state = ResultType.success.ToString(), message = "成功"}.ToJson());
            }
            catch (Exception e)
            {
                return Json(new AjaxResult {state = ResultType.error.ToString(), message = e.Message}.ToJson());
            }
        }

        public ActionResult TallyBll_ck(int id)
        {
            C_TB_HS_TALLYBILL model = db.C_TB_HS_TALLYBILL.Find(id) ?? new C_TB_HS_TALLYBILL();
            int STORAG = Convert.ToInt32(model.STORAG);
            int CODE_SECTION = Convert.ToInt32(model.CODE_SECTION);
            C_TB_CODE_STORAGE model_c = db.C_TB_CODE_STORAGE.Find(STORAG) ?? new C_TB_CODE_STORAGE();
            C_TB_CODE_BOOTH model_w = db.C_TB_CODE_BOOTH.Find(CODE_SECTION) ?? new C_TB_CODE_BOOTH();
            ViewBag.STORAG = model_c.STORAGEName;
            ViewBag.CODE_SECTION = model_w.BOOTH;
            return View(model);
        }


        /// <summary>
        /// 根据委托号，获取委托的信息
        /// </summary>
        /// <param name="cgno">委托号</param>
        /// <returns></returns>
        [HttpGet]
        public JsonResult GetInfoByWtHao(string cgno, string hz)
        {
            try
            {
                C_TB_HC_CONSIGN wt = db.C_TB_HC_CONSIGN.FirstOrDefault(n => n.CGNO == cgno && n.HengZhong == hz);

                C_TB_HC_GOODSBILL goodsbill = db.C_TB_HC_GOODSBILL.Find(wt.GOODSBILL_ID);
                if (goodsbill != null)
                {
                    GoodBillAndConsign Gc = new GoodBillAndConsign()
                    {
                        GoodBLNO = goodsbill.BLNO,
                        CODE_INOUT = goodsbill.CODE_INOUT,
                        NWM = goodsbill.NWM,
                        VGNO = goodsbill.VGNO,
                        C_GOODSAGENT_ID = goodsbill.C_GOODSAGENT_ID,
                        C_GOODS = goodsbill.C_GOODS,
                        CODE_PACK_NAME = goodsbill.CODE_PACK_NAME,
                        GoodPIECEWEIGHT = goodsbill.PIECEWEIGHT,
                        GoodPLANAMOUNT = goodsbill.PLANAMOUNT,
                        GoodPLANWEIGHT = goodsbill.PLANWEIGHT,
                        MARK_GOOGSBILLTYPE = goodsbill.MARK_GOOGSBILLTYPE,
                        MARK = goodsbill.MARK,

                        CODE_OPERATION = wt.CODE_OPERATION,
                        WeiTuoRen = wt.WeiTuoRen,
                        ShouHuoRen = wt.ShouHuoRen,
                        CODE_TRANS = wt.CODE_TRANS,
                        WeiTuoTime = wt.WeiTuoTime,
                        BLNO = wt.BLNO,
                        PLANAMOUNT = wt.PLANAMOUNT,
                        PLANWEIGHT = wt.PLANWEIGHT,
                        PLANVOLUME = wt.PLANVOLUME,
                        CONTAINERTYPE = wt.CONTAINERTYPE,
                        CONTAINERNUM = wt.CONTAINERNUM,
                        PAPERYNO = wt.PAPERYNO,
                        BoolQuanLuYun = wt.BoolQuanLuYun,
                        SPONSOR = wt.SPONSOR,
                        Phone = wt.Phone,
                        BeiZhu = wt.BeiZhu,
                        HengZhong = wt.HengZhong,

                    };
                    return Json(Gc, JsonRequestBehavior.AllowGet);
                }

                return Json(null, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new AjaxResult {state = ResultType.error.ToString(), message = e.Message}.ToJson());
            }
        }
        #region 库存查询
        public ActionResult KuCunIndex()
        {
            return View();
        }

        [System.Web.Http.HttpGet]
        public object GetKuCunBllList(int limit, int offset, string YuanQU_Name, string SECTION_Name)
        {
            OperatorModel loginModel = OperatorProvider.Provider.GetCurrent();
            List<KuCun> kclist = new List<KuCun>();
            List<C_TB_CODE_BOOTH> list_Booth = db.C_TB_CODE_BOOTH.Where(n => n.CompanyId == loginModel.YuanquID)
                .OrderBy(n => n.ID).ToList();
            string CODE_SECTION = "";
            var storages = db.C_TB_CODE_STORAGE.AsQueryable();
            var SECTIONs = db.C_TB_CODE_SECTION.AsQueryable();
            var yuanqus = db.C_Dic_YuanQu.AsQueryable();
            foreach (var items_Booth in list_Booth)
            {
                KuCun kc = new KuCun();
                C_TB_CODE_STORAGE model_STORAGE = storages.FirstOrDefault(n => n.ID == items_Booth.Storage_ID);
                kc.STORAG_Name = model_STORAGE.STORAGEName; //获取场名称

                C_TB_CODE_SECTION model_SECTION = SECTIONs.FirstOrDefault(n => n.ID == model_STORAGE.SECTION_ID);
                kc.SECTION_Name = model_SECTION.SECTION; //获取库名称

                C_Dic_YuanQu model_YuanQu = yuanqus.FirstOrDefault(n => n.ID == model_SECTION.CODE_COMPANY);
                kc.YuanQU_Name = model_YuanQu.YuanQuName; //获取公司名称

                CODE_SECTION = items_Booth.ID.ToString();
                List<C_TB_HS_STOCKDORMANT> list_STOCKDORMANT = db.C_TB_HS_STOCKDORMANT
                    .Where(n => n.CODE_BOOTH == CODE_SECTION).OrderBy(n => n.ID).ToList();
                kc.CODE_SECTION_Name = items_Booth.BOOTH; //获取位信息
                kc.STORAG = items_Booth.ID; //获取位信息
                foreach (var items_STOCKDORMANT in list_STOCKDORMANT)
                {

                    kc.AMOUNT += Convert.ToDecimal(items_STOCKDORMANT.AMOUNT);
                    kc.WEIGHT += Convert.ToDecimal(items_STOCKDORMANT.WEIGHT);
                }

                if (string.IsNullOrEmpty(YuanQU_Name) && string.IsNullOrEmpty(SECTION_Name))
                {
                    kclist.Add(kc);

                }
                else if (!string.IsNullOrEmpty(SECTION_Name) || !string.IsNullOrEmpty(YuanQU_Name))
                {
                    if (kc.SECTION_Name.Contains(SECTION_Name) && kc.YuanQU_Name.Contains(YuanQU_Name))
                    {
                        kclist.Add(kc);
                    }
                }

            }

            int total = kclist.Count();
            object rows = kclist.Skip(offset).Take(limit).AsQueryable();
            return Json(new {total = total, rows = rows}, JsonRequestBehavior.AllowGet);

        }

        public ActionResult KuCun_Xq(int id)
        {
            ViewBag.Booth_id = id;
            return View();
        }

        [System.Web.Http.HttpGet]
        public object GetKuCunList_xq(int limit, int offset, string Booth_id)
        {
            List<KuCun_XQ> kclist = new List<KuCun_XQ>();
            List<C_TB_HS_STOCKDORMANT> list_Booth = db.C_TB_HS_STOCKDORMANT.Where(n => n.CODE_BOOTH == Booth_id)
                .OrderBy(n => n.ID).ToList();
            Array gbnos = list_Booth.Select(n => n.GBNO).Distinct().ToArray();
            foreach (var gbno in gbnos)
            {
                string gbnoString = gbno.ToString();
                List<C_TB_HS_STOCKDORMANT> listBooth = list_Booth.Where(n => n.GBNO == gbnoString).ToList();
                int i = 0;
                string BGON = null, REMARK = null, huodai = null, huowu = null;
                decimal AMOUNT = 0, WEIGHT = 0;
                foreach (var items in listBooth)
                {
                    if (i == 0)
                    {
                        BGON = items.GBNO;
                        REMARK = items.REMARK;
                        huodai = items.HuoDai;
                        huowu = items.GoodsName;
                    }

                    decimal A = (decimal)items.AMOUNT;
                    AMOUNT += System.Math.Abs(A);
                    decimal W = (decimal)items.WEIGHT;
                    WEIGHT += System.Math.Abs(W);
                  
                    i++;
                }

                KuCun_XQ kc = new KuCun_XQ()
                {
                    BGON = BGON,
                    REMARK = REMARK,
                    huodai = huodai,
                    huowu = huowu,
                    AMOUNT = AMOUNT,
                    WEIGHT = WEIGHT

                };
                kclist.Add(kc);
            }
            int total = kclist.Count();
            object rows = kclist.Skip(offset).Take(limit).AsQueryable();
            return Json(new {total = total, rows = rows}, JsonRequestBehavior.AllowGet);

        }

        public ActionResult ChakanLiHuoDanByWeiTuo(string BGON)
        {
            ViewBag.BGON = BGON;
            return View();
        }
        [System.Web.Http.HttpGet]
        public object ChakanLiHuoDanByWeiTuoList(int limit, int offset,  string CGNO)
        {
            OperatorModel loginModel = OperatorProvider.Provider.GetCurrent();
            var yuanquid = loginModel.YuanquID ?? throw new ArgumentNullException("loginModel.YuanquID");

            var wherelambda = ExtLinq.True<C_TB_HS_TALLYBILL>();
            wherelambda = wherelambda.And(t => t.Type != "清场" && t.YuanQuID == yuanquid);

            if (!string.IsNullOrEmpty(CGNO))
            {
                wherelambda = wherelambda.And(t => t.CGNO.Contains(CGNO));
            }

            var list = db.Set<C_TB_HS_TALLYBILL>().Where(wherelambda).OrderByDescending(n => n.ID).AsQueryable();
       

            int total = list.Count();
            object rows = list.Skip(offset).Take(limit).AsQueryable();
            return Json(new { total = total, rows = rows }, JsonRequestBehavior.AllowGet);

        }
      #endregion
        public ActionResult ClearIndex()
        {
   
            return View();
        }
        public ActionResult AddClear(int id)
        {
            C_TB_HS_TALLYBILL model = db.C_TB_HS_TALLYBILL.Find(id) ?? new C_TB_HS_TALLYBILL();

            List<C_TB_CODE_CAOZUO> listcaozuo = db.C_TB_CODE_CAOZUO.OrderBy(n => n.ID).ToList();//操作
            List<SelectListItem> emplist = SelectHelp.CreateSelect<C_TB_CODE_CAOZUO>(listcaozuo, "NAME", "ID", null);
            ViewData["CAOZUOList"] = new SelectList(emplist, "Value", "Text");

            List<SelectListItem> STORAGEList = GetChang();
            ViewData["STORAGEList"] = new SelectList(STORAGEList, "Value", "Text");

            List<C_GOODS> list_GOODS = db.C_GOODS.OrderBy(n => n.ID).ToList();//货物
            List<SelectListItem> emplist_GOODS = SelectHelp.CreateSelect<C_GOODS>(list_GOODS, "GoodsName", "GoodsName", null);
            ViewData["GOODS_List"] = new SelectList(emplist_GOODS, "Value", "Text", "是");
            return View(model);
        }
        [OperationLog(OperationLogAttribute.Operatetype.添加或编辑, OperationLogAttribute.ImportantLevel.一般操作, "添加清场")]
        [HttpPost]
        public ActionResult AddClear(C_TB_HS_TALLYBILL model)
        {
            OperatorModel loginModel = OperatorProvider.Provider.GetCurrent();
            var yuanquid = loginModel.YuanquID ?? throw new ArgumentNullException("loginModel.YuanquID");
            model.YuanQuID = yuanquid;
            decimal AorU = model.ID;
          //  C_TB_HS_TALLYBILL model_TALLYBILL = db.C_TB_HS_TALLYBILL.Find(AorU) ?? new C_TB_HS_TALLYBILL();
            if (model.CleraNum == null)
            {
                string TodayTime = DateTime.Today.ToString("yyyyMMdd");
                C_TB_HS_TALLYBILL Num = db.C_TB_HS_TALLYBILL.Where(n=>n.Type=="清场").OrderByDescending(n => n.ID).FirstOrDefault();
                if (Num != null)
                {
                    if (!string.IsNullOrEmpty(Num.CleraNum))
                    {
                        if (Num.CleraNum.Substring(0, 8) == TodayTime)
                        {
                            model.CleraNum = TodayTime + (Convert.ToInt32(Num.CleraNum.Replace(TodayTime, "")) + 1).ToString("0000");
                        }
                        else
                        {
                            model.CleraNum = TodayTime + "0001";
                        }
                    }
                }

                else
                {
                    model.CleraNum = TodayTime + "0001";
                }
            }

            if (model.ID == 0)
            {
                model.State = "进行中";
                model.Type = "清场";
            }
            model.TBNO = "清场";
            model.CONSIGN_ID = 0;
            model.CGNO = "清场";
            try
            {

                db.C_TB_HS_TALLYBILL.AddOrUpdate(model);
                db.SaveChanges();
                bool b = EidSTOCKDORMANT_clear(model, AorU);
                if (b)
                {
                    return Json(new AjaxResult { state = ResultType.success.ToString(), message = "成功！" }.ToJson());
                }
                else
                {
                    db.C_TB_HS_TALLYBILL.Remove(model);
                    db.SaveChanges();
                    return Json(new AjaxResult { state = ResultType.error.ToString(), message = "库存更新失败！" }.ToJson());
                }
            }

            catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            {
                var msg = string.Empty;
                var errors = (from u in ex.EntityValidationErrors select u.ValidationErrors).ToList();
                foreach (var item in errors)
                    msg += item.FirstOrDefault().ErrorMessage;

                return Json(new AjaxResult { state = ResultType.error.ToString(), message = msg }.ToJson());
            }
            catch (Exception e)
            {
                return Json(new AjaxResult { state = ResultType.error.ToString(), message = e.Message }.ToJson());
            }


        }
        public bool EidSTOCKDORMANT_clear(C_TB_HS_TALLYBILL model, decimal AorU)
        {
            string Id = model.ID.ToString();
            C_TB_HS_STOCKDORMANT stocModel = db.C_TB_HS_STOCKDORMANT.FirstOrDefault(n => n.TALLYBILL_ID == Id) ?? new C_TB_HS_STOCKDORMANT();//理货单号
            stocModel.ID = stocModel.ID;
            stocModel.TALLYBILL_ID = model.ID.ToString();
            stocModel.CODE_STORAGE = model.STORAG;//场
            stocModel.CODE_BOOTH = model.CODE_SECTION;//位
                stocModel.AMOUNT = -model.AMOUNT;//件数
                stocModel.WEIGHT = -model.WEIGHT;//重量
                stocModel.VOLUME = -model.VOLUME;//体积

            stocModel.FIRST_INDATE = DateTime.Now;
            stocModel.BOOTH_INDATE = DateTime.Now;
            stocModel.REMARK = "出库";
            stocModel.LastEidTime = DateTime.Now;
            stocModel.GoodsName = model.GoodsName;
            stocModel.YuanQuID = model.YuanQuID;
            db.C_TB_HS_STOCKDORMANT.AddOrUpdate(stocModel);
            try
            {
                db.SaveChanges();
                return true;
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            {
                var msg = string.Empty;
                var errors = (from u in ex.EntityValidationErrors select u.ValidationErrors).ToList();
                foreach (var item in errors)
                    msg += item.FirstOrDefault().ErrorMessage;

                return false;
            }
            catch (Exception e)
            {
                return false;
            }
        }
        public object GetTallyBllList_clear(int limit, int offset, string BEGINTIME, string ENDTIME)
        {
            OperatorModel loginModel = OperatorProvider.Provider.GetCurrent();
            var yuanquid = loginModel.YuanquID ?? throw new ArgumentNullException("loginModel.YuanquID");
            var wherelambda = ExtLinq.True<C_TB_HS_TALLYBILL>();
            wherelambda = wherelambda.And(t => t.Type == "清场"&&t.YuanQuID==yuanquid);

            if (!string.IsNullOrEmpty(BEGINTIME))
            {
                DateTime BEGINTIME_date = Convert.ToDateTime(BEGINTIME + " 00:00:00");
                wherelambda = wherelambda.And(n => n.SIGNDATE >= BEGINTIME_date);
                // list = list.Where(n => n.publish_time >= Convert.ToDateTime(fbsj) && n.publish_time <= Convert.ToDateTime(fbsj1)).ToList();
            }
            if (!string.IsNullOrEmpty(ENDTIME))
            {
                DateTime ENDTIME_date = Convert.ToDateTime(ENDTIME + " 23:59:59");
                wherelambda = wherelambda.And(n => n.SIGNDATE <= ENDTIME_date);
                // list = list.Where(n => n.publish_time >= Convert.ToDateTime(fbsj) && n.publish_time <= Convert.ToDateTime(fbsj1)).ToList();
            }
            var list = db.Set<C_TB_HS_TALLYBILL>().Where(wherelambda).OrderByDescending(n => n.ID).AsQueryable();
            decimal STORAG = 0;
            foreach (var items in list)
            {
                STORAG = Convert.ToDecimal(items.STORAG);
                items.STORAG = db.C_TB_CODE_BOOTH.FirstOrDefault(n => n.ID == STORAG).BOOTH;
            }
            int total = list.Count();
            object rows = list.Skip(offset).Take(limit).AsQueryable();
            return Json(new { total = total, rows = rows }, JsonRequestBehavior.AllowGet);

        }
        public JsonResult GetTALLYBILLList(decimal id)
        {
            List<C_TB_HS_TALLYBILL> list = db.C_TB_HS_TALLYBILL.Where(n => n.CONSIGN_ID == id&&n.Type=="清场").OrderByDescending(n => n.ID).ToList();
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
    }
}