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
    public class BsYeWuWeiTuoController : Controller
    {
        private Entities db = new Entities();
        // GET: ck/BsYeWuWeiTuo

        public ActionResult YeWuWeiTuoList()
        {
            OperatorModel loginModel = OperatorProvider.Provider.GetCurrent();
            ViewBag.roleId = loginModel.RoleId;
            return View();
        }
        public JsonResult GetTALLYBILLList(decimal id)
        {
            C_TB_HC_CONSIGN mode_Consign = db.C_TB_HC_CONSIGN.Find(id);
            C_TB_HC_GOODSBILL mode_goodsBill = db.C_TB_HC_GOODSBILL.FirstOrDefault(n=>n.ID==mode_Consign.GOODSBILL_ID);
            List<C_TB_HC_CONSIGN> list_Consign = db.C_TB_HC_CONSIGN.Where(n => n.GOODSBILL_ID == mode_goodsBill.ID).OrderByDescending(n => n.ID).ToList();
            List<C_TB_HS_TALLYBILL> list = new List<C_TB_HS_TALLYBILL>();
            foreach (var items in list_Consign)
            {
                List<C_TB_HS_TALLYBILL> list_TallBll = db.C_TB_HS_TALLYBILL.Where(n => n.CONSIGN_ID == items.ID).OrderByDescending(n => n.ID).ToList();
                foreach (var items_Tallbll in list_TallBll)
                {
                    decimal? CODE_SECTION = Convert.ToDecimal(items_Tallbll.CODE_SECTION);
                    items_Tallbll.CODE_SECTION = db.C_TB_CODE_BOOTH.FirstOrDefault(n => n.ID == CODE_SECTION).BOOTH;
                    list.Add(items_Tallbll);
                    
                }
            }
            list = list.OrderByDescending(n=>n.CreatTime).ToList();
            var total = list.Count;
            //decimal CODE_SECTION = 0;

            //foreach (var items in list)
            //{
            //    string xiangxi = "";
            //    if (!string.IsNullOrEmpty(items.ZuoYeLeiXIng))
            //    {
            //        string[] Guid = items.ZuoYeLeiXIng.Split(',');
            //        foreach (var tiems_g in Guid)
            //        {
            //            BS_LAOWUZUOYELEIBIE Model_LAOWUZUOYELEIBIE = db.BS_LAOWUZUOYELEIBIE.FirstOrDefault(n => n.Guid == tiems_g);//查找合同
            //            xiangxi += items.GoodsName + Model_LAOWUZUOYELEIBIE.ZuoYeLeiBieMingCheng + Model_LAOWUZUOYELEIBIE.DanJia + "*" + items.WEIGHT + "=" + Model_LAOWUZUOYELEIBIE.DanJia.ToDecimal() * items.WEIGHT + ";";
            //        }
            //        items.ZuoYeLeiXIng = xiangxi;
            //    }
            //    CODE_SECTION = Convert.ToDecimal(items.CODE_SECTION);
            //    items.CODE_SECTION = db.C_TB_CODE_BOOTH.FirstOrDefault(n => n.ID == CODE_SECTION).BOOTH;
            //}
            var rows = list.ToList();

            return Json(new { total = total, rows = rows }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetTALLYBILLList_zylb(decimal id)
        {
            List<BS_ZYLB_TBLL> list = db.BS_ZYLB_TBLL.Where(n => n.ConSignId == id).OrderByDescending(n => n.CreatTime).ToList();
            var total = list.Count;
            //decimal CODE_SECTION = 0;

            //foreach (var items in list)
            //{
            //    string xiangxi = "";
            //    if (!string.IsNullOrEmpty(items.ZuoYeLeiXIng))
            //    {
            //        string[] Guid = items.ZuoYeLeiXIng.Split(',');
            //        foreach (var tiems_g in Guid)
            //        {
            //            BS_LAOWUZUOYELEIBIE Model_LAOWUZUOYELEIBIE = db.BS_LAOWUZUOYELEIBIE.FirstOrDefault(n => n.Guid == tiems_g);//查找合同
            //            xiangxi += items.GoodsName + Model_LAOWUZUOYELEIBIE.ZuoYeLeiBieMingCheng + Model_LAOWUZUOYELEIBIE.DanJia + "*" + items.WEIGHT + "=" + Model_LAOWUZUOYELEIBIE.DanJia.ToDecimal() * items.WEIGHT + ";";
            //        }
            //        items.ZuoYeLeiXIng = xiangxi;
            //    }
            //    CODE_SECTION = Convert.ToDecimal(items.CODE_SECTION);
            //    items.CODE_SECTION = db.C_TB_CODE_BOOTH.FirstOrDefault(n => n.ID == CODE_SECTION).BOOTH;
            //}
            var rows = list.ToList();

            return Json(new { total = total, rows = rows }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult Getzylb(decimal id)
        {
            C_TB_HC_CONSIGN model = db.C_TB_HC_CONSIGN.Find(id);
            OperatorModel loginModel = OperatorProvider.Provider.GetCurrent();
            string ZhiLingXiangMu = model.ZhiLingXiangMu.Replace("入库","进库");
            List<BS_LAOWUZUOYELEIBIE> list = db.BS_LAOWUZUOYELEIBIE.Where(n => n.JinChuKu == ZhiLingXiangMu && n.YuanQuId == loginModel.YuanquID).OrderByDescending(n => n.CreatTime).ToList();
            var total = list.Count;
            var rows = list.ToList();
            return Json(new { total = total, rows = rows }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public object GetYeWuWeiTuoList(int limit, int offset, string GBNO, string C_GOODSAGENT_NAME, string CreatTime, string CreatTime1, string BLNO, string C_GOODS, string HuoZhu, string ShipName, string VGNO)
        {

            var wherelambda = ExtLinq.True<C_TB_HC_GOODSBILL>();
            OperatorModel loginModel = OperatorProvider.Provider.GetCurrent();
            wherelambda = wherelambda.And(t => t.YuanQuID == loginModel.YuanquID);
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
            int count = db.Set<C_TB_HC_GOODSBILL>().Where(wherelambda).OrderByDescending(n => n.CreatTime).Count();
            var list = db.Set<C_TB_HC_GOODSBILL>().Where(wherelambda).OrderByDescending(n => n.CreatTime).Skip(offset).Take(limit).AsQueryable();

            foreach (var itsms_GoodsBill in list)//计算库存
            {
                C_TB_HC_CONTRACT Model_CONTRACT = db.C_TB_HC_CONTRACT.FirstOrDefault(n => n.Guid == itsms_GoodsBill.CONTRACT_Guid);//查找合同
                decimal? KuCun = 0;
                decimal? KuCunW = 0;
                List<C_TB_HC_CONSIGN> list_CONSIGN = db.C_TB_HC_CONSIGN.Where(n => n.GOODSBILL_ID == itsms_GoodsBill.ID).OrderBy(n => n.ID).ToList();
                foreach (var items_Consign in list_CONSIGN)
                {
                    List<C_TB_HS_TALLYBILL> list_TALLYBILL = db.C_TB_HS_TALLYBILL.Where(n => n.CONSIGN_ID == items_Consign.ID&&n.Type!="还原亏吨").OrderBy(n => n.ID).ToList();
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
                decimal? ShiJiJInKu = 0;
                decimal? ShiJiChuKu = 0;
                itsms_GoodsBill.KunCun = KuCun.ToString();
                itsms_GoodsBill.KunCunW = KuCunW.ToDecimal(3).ToString();
                List<C_TB_JIESUAN> list_JieSuan = db.C_TB_JIESUAN.Where(n => n.GoodsBill_id == itsms_GoodsBill.ID).OrderBy(n => n.ID).ToList();
                itsms_GoodsBill.Lkjehj = list_JieSuan.Sum(n => n.LaiKuanJinE).ToDecimal(2);
                List<C_TB_CHENGBEN> list_ChengBen = db.C_TB_CHENGBEN.Where(n => n.GoodsBill_id == itsms_GoodsBill.ID).OrderBy(n => n.ID).ToList();
                itsms_GoodsBill.ChengBenJinE = list_ChengBen.Sum(n => n.ChengBenJinE).ToDecimal(2);
                if (list_JieSuan.Sum(n => n.KaiPiaoJinE) == 0)
                {
                    itsms_GoodsBill.HuiShouLv = 0;
                }
                else
                {
                    itsms_GoodsBill.HuiShouLv = (list_JieSuan.Sum(n => n.LaiKuanJinE) / list_JieSuan.Sum(n => n.KaiPiaoJinE)).ToDecimal(2);

                }
                List<C_TB_HC_CONSIGN> list_KuCunCONSIGN = db.C_TB_HC_CONSIGN.Where(n => n.GOODSBILL_ID == itsms_GoodsBill.ID).OrderBy(n => n.ID).ToList();
                foreach (var items_KuCunCONSIGN in list_KuCunCONSIGN)
                {
                    List<C_TB_HS_TALLYBILL> list_KuCunTALLYBILL = db.C_TB_HS_TALLYBILL.Where(n => n.CONSIGN_ID == items_KuCunCONSIGN.ID).OrderBy(n => n.ID).ToList();
                    ShiJiJInKu += list_KuCunTALLYBILL.Where(n => n.CODE_OPSTYPE == "进库" && n.Type != "还原亏吨").Sum(n => n.WEIGHT);
                    ShiJiChuKu += list_KuCunTALLYBILL.Where(n => n.CODE_OPSTYPE == "出库" && n.Type != "还原亏吨").Sum(n => n.WEIGHT);
                }
                itsms_GoodsBill.ShiJiJInKu = ShiJiJInKu;
                itsms_GoodsBill.ShiJiChuKu = ShiJiChuKu;
            }
            int total = count;
            object rows = list;
            return Json(new { total = total, rows = rows }, JsonRequestBehavior.AllowGet);

        }
        /// <summary>
        /// 新增业务委托
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult Add_C_BsYeWuWeiTuo(int id)
        {
            C_TB_HC_GOODSBILL model = db.C_TB_HC_GOODSBILL.Find(id) ?? new C_TB_HC_GOODSBILL();
            OperatorModel loginModel = OperatorProvider.Provider.GetCurrent();
            List<C_GOODS> list_GOODS = db.C_GOODS.OrderBy(n => n.GoodsName).ToList();//货物
            List<SelectListItem> emplist_GOODS = SelectHelp.CreateSelect<C_GOODS>(list_GOODS, "GoodsName", "GoodsName", null);
            ViewData["GOODS_List"] = new SelectList(emplist_GOODS, "Value", "Text", "是");
            ViewBag.YuanQuId = loginModel.YuanquID;
            return View(model);
        }
        [OperationLog(OperationLogAttribute.Operatetype.添加或编辑, OperationLogAttribute.ImportantLevel.一般操作, "添加业务委托")]
        [HttpPost]
        public JsonResult Add_C_BsYeWuWeiTuo(C_TB_HC_GOODSBILL model)
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
            model.DanJia = (model.HuoZhi.ToDecimal() / model.GanZhong.ToDecimal()).ToString();
            if (model.ID == 0)
            {
                model.CreatTime = DateTime.Now;
                model.State = "进行中";
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

        #region 报关

        /// <summary>
        /// 新增报关
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult Add_C_BaoGuan(int id, string guid)
        {
            OperatorModel loginModel = OperatorProvider.Provider.GetCurrent();
            BS_BAOGUAN model = db.BS_BAOGUAN.Find(guid) ?? new BS_BAOGUAN()
            {
                GoodsBillId = id,
                Guid = Guid.NewGuid().ToString(),
                State = "未放行"
            };
            C_TB_HC_GOODSBILL model_goodsbill = db.C_TB_HC_GOODSBILL.Find(id) ?? db.C_TB_HC_GOODSBILL.Find(model.GoodsBillId);
            List<C_GOODS> list_GOODS = db.C_GOODS.OrderBy(n => n.GoodsName).ToList();//货物
            List<SelectListItem> emplist_GOODS = SelectHelp.CreateSelect<C_GOODS>(list_GOODS, "GoodsName", "GoodsName", null);
            ViewData["GOODS_List"] = new SelectList(emplist_GOODS, "Value", "Text", "是");
            List<C_TB_CODE_CUSTOMER> list_CUSTOMER = db.C_TB_CODE_CUSTOMER.Where(n => n.YuanQuID == loginModel.YuanquID).OrderBy(n => n.ID).ToList();//委托人,收货人
            List<SelectListItem> emplist_CUSTOMER = SelectHelp.CreateSelect<C_TB_CODE_CUSTOMER>(list_CUSTOMER, "Name", "Name", null);
            ViewData["CUSTOMER_List"] = new SelectList(emplist_CUSTOMER, "Value", "Text", "是");
            ViewBag.GoodsBillId = id;
            ViewBag.yuanquid = loginModel.YuanquID;
            ViewBag.MaoZhong = model_goodsbill.PLANWEIGHT;
            ViewBag.JingZhong = model_goodsbill.jccj;
            ViewBag.HuoZhi = model_goodsbill.HuoZhi;
            ViewBag.DanJia = model_goodsbill.DanJia;
            ViewBag.GanZhong = model_goodsbill.GanZhong;
            C_TB_HC_CONTRACT Model_CONTRACT = db.C_TB_HC_CONTRACT.FirstOrDefault(n => n.Guid == model_goodsbill.CONTRACT_Guid);//查找合同
            decimal? KuCun = 0;
            decimal? KuCunW = 0;
            List<C_TB_HC_CONSIGN> list_CONSIGN = db.C_TB_HC_CONSIGN.Where(n => n.GOODSBILL_ID == model_goodsbill.ID).OrderBy(n => n.ID).ToList();
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
            decimal? ShiJiJInKu = 0;
            decimal? ShiJiChuKu = 0;
            model_goodsbill.KunCun = KuCun.ToString();
            model_goodsbill.KunCunW = KuCunW.ToDecimal(3).ToString();
            List<C_TB_JIESUAN> list_JieSuan = db.C_TB_JIESUAN.Where(n => n.GoodsBill_id == model_goodsbill.ID).OrderBy(n => n.ID).ToList();
            model_goodsbill.Lkjehj = list_JieSuan.Sum(n => n.LaiKuanJinE).ToDecimal(2);
            List<C_TB_CHENGBEN> list_ChengBen = db.C_TB_CHENGBEN.Where(n => n.GoodsBill_id == model_goodsbill.ID).OrderBy(n => n.ID).ToList();
            model_goodsbill.ChengBenJinE = list_ChengBen.Sum(n => n.ChengBenJinE).ToDecimal(2);
            if (list_JieSuan.Sum(n => n.KaiPiaoJinE) == 0)
            {
                model_goodsbill.HuiShouLv = 0;
            }
            else
            {
                model_goodsbill.HuiShouLv = (list_JieSuan.Sum(n => n.LaiKuanJinE) / list_JieSuan.Sum(n => n.KaiPiaoJinE)).ToDecimal(2);

            }
            List<C_TB_HC_CONSIGN> list_KuCunCONSIGN = db.C_TB_HC_CONSIGN.Where(n => n.GOODSBILL_ID == model_goodsbill.ID).OrderBy(n => n.ID).ToList();
            foreach (var items_KuCunCONSIGN in list_KuCunCONSIGN)
            {
                List<C_TB_HS_TALLYBILL> list_KuCunTALLYBILL = db.C_TB_HS_TALLYBILL.Where(n => n.CONSIGN_ID == items_KuCunCONSIGN.ID).OrderBy(n => n.ID).ToList();
                ShiJiJInKu += list_KuCunTALLYBILL.Where(n => n.CODE_OPSTYPE == "进库" && n.Type != "清场").Sum(n => n.WEIGHT);
                ShiJiChuKu += list_KuCunTALLYBILL.Where(n => n.CODE_OPSTYPE == "出库" && n.Type != "清场").Sum(n => n.WEIGHT);
            }
            model_goodsbill.ShiJiJInKu = ShiJiJInKu;
            model_goodsbill.ShiJiChuKu = ShiJiChuKu;
            ViewBag.KuCunW = KuCunW;
            return View(model);
        }
        public ActionResult Add_C_BaoGuanFangXing(int id, string guid)
        {
            OperatorModel loginModel = OperatorProvider.Provider.GetCurrent();
            BS_BAOGUAN model = db.BS_BAOGUAN.Find(guid) ?? new BS_BAOGUAN()
            {
                GoodsBillId = id,
                Guid = Guid.NewGuid().ToString(),
                State = "未放行"
            };

            List<C_GOODS> list_GOODS = db.C_GOODS.OrderBy(n => n.GoodsName).ToList();//货物
            List<SelectListItem> emplist_GOODS = SelectHelp.CreateSelect<C_GOODS>(list_GOODS, "GoodsName", "GoodsName", null);
            ViewData["GOODS_List"] = new SelectList(emplist_GOODS, "Value", "Text", "是");
            List<C_TB_CODE_CUSTOMER> list_CUSTOMER = db.C_TB_CODE_CUSTOMER.Where(n => n.YuanQuID == loginModel.YuanquID).OrderBy(n => n.ID).ToList();//委托人,收货人
            List<SelectListItem> emplist_CUSTOMER = SelectHelp.CreateSelect<C_TB_CODE_CUSTOMER>(list_CUSTOMER, "Name", "Name", null);
            ViewData["CUSTOMER_List"] = new SelectList(emplist_CUSTOMER, "Value", "Text", "是");
            ViewBag.GoodsBillId = id;
            return View(model);
        }
        [HttpPost]
        public JsonResult Add_C_BaoGuan(BS_BAOGUAN model)
        {
            OperatorModel loginModel = OperatorProvider.Provider.GetCurrent();
            var yuanquid = loginModel.YuanquID ?? throw new ArgumentNullException("loginModel.YuanquID");

            model.CreatPeople = loginModel.UserName;
            model.CreatTime = DateTime.Now;
            db.Set<BS_BAOGUAN>().AddOrUpdate(model);
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
        [HttpPost]
        public JsonResult Add_C_BaoGuanFangXing(BS_BAOGUAN model)
        {
            OperatorModel loginModel = OperatorProvider.Provider.GetCurrent();
            var yuanquid = loginModel.YuanquID ?? throw new ArgumentNullException("loginModel.YuanquID");

            model.CreatPeople = loginModel.UserName;
            model.CreatTime = DateTime.Now;
            model.State = "结束";
            db.Set<BS_BAOGUAN>().AddOrUpdate(model);
            BS_FangHuoNeiBuShenPi model_sh = db.BS_FangHuoNeiBuShenPi.FirstOrDefault(n => n.GoodsBillId == model.GoodsBillId && n.State_BaoGuan != "结束");
            if (model_sh != null && model.BaoGuanLeiBie == "出库报关")
            {
                model_sh.State_BaoGuan = "结束";
                db.Set<BS_FangHuoNeiBuShenPi>().AddOrUpdate(model_sh);
            }
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

        public ActionResult BaoGuanList(int GoodsBillId)
        {
            OperatorModel loginModel = OperatorProvider.Provider.GetCurrent();
            ViewBag.GoodsBillId = GoodsBillId;
            ViewBag.RoleId = loginModel.RoleId;
            return View();
        }

        [System.Web.Http.HttpGet]
        public object GetBaoGuanList(int limit, int offset, int GoodsBillId)
        {
            var wherelambda = ExtLinq.True<BS_BAOGUAN>();
            wherelambda = wherelambda.And(t => t.GoodsBillId == GoodsBillId);
            var list = db.Set<BS_BAOGUAN>().Where(wherelambda).OrderByDescending(n => n.CreatTime).AsQueryable();
            int total = list.Count();
            object rows = list.Skip(offset).Take(limit).AsQueryable();
            return Json(new { total = total, rows = rows }, JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public JsonResult DelBaoGuanById(string guid)
        {
            try
            {
                EFHelpler<BS_BAOGUAN> ef = new EFHelpler<BS_BAOGUAN>();
                BS_BAOGUAN model = db.BS_BAOGUAN.Find(guid);
                ef.delete(model);
                db.SaveChanges();
                return Json(new AjaxResult { state = ResultType.success.ToString(), message = "成功" });
            }
            catch (Exception e)
            {
                return Json(new AjaxResult { state = ResultType.error.ToString(), message = e.Message });
            }
        }

        #endregion

        #region 报检




        /// <summary>
        /// 新增报检
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult Add_C_BaoJian(int id, string guid)
        {
            OperatorModel loginModel = OperatorProvider.Provider.GetCurrent();
            BS_BAOJIAN model = db.BS_BAOJIAN.Find(guid) ?? new BS_BAOJIAN()
            {
                GoodsBillId = id,
                Guid = Guid.NewGuid().ToString()
            };
            List<C_TB_CODE_CUSTOMER> list_CUSTOMER = db.C_TB_CODE_CUSTOMER.Where(n => n.YuanQuID == loginModel.YuanquID).OrderBy(n => n.ID).ToList();//委托人,收货人
            List<SelectListItem> emplist_CUSTOMER = SelectHelp.CreateSelect<C_TB_CODE_CUSTOMER>(list_CUSTOMER, "Name", "Name", null);
            ViewData["CUSTOMER_List"] = new SelectList(emplist_CUSTOMER, "Value", "Text", "是");
            // ViewBag.GoodsBillId = id;
            return View(model);
        }
        [HttpPost]
        public JsonResult Add_C_BaoJian(BS_BAOJIAN model)
        {
            OperatorModel loginModel = OperatorProvider.Provider.GetCurrent();
            var yuanquid = loginModel.YuanquID ?? throw new ArgumentNullException("loginModel.YuanquID");

            model.CreatPeople = loginModel.UserName;
            model.CreatTime = DateTime.Now;
            db.Set<BS_BAOJIAN>().AddOrUpdate(model);
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
        public ActionResult BaoJianList(int GoodsBillId)
        {
            ViewBag.GoodsBillId = GoodsBillId;
            return View();
        }

        [System.Web.Http.HttpGet]
        public object GetBaoJianList(int limit, int offset, int GoodsBillId)
        {
            var wherelambda = ExtLinq.True<BS_BAOJIAN>();
            wherelambda = wherelambda.And(t => t.GoodsBillId == GoodsBillId);
            var list = db.Set<BS_BAOJIAN>().Where(wherelambda).OrderByDescending(n => n.CreatTime).AsQueryable();
            int total = list.Count();
            object rows = list.Skip(offset).Take(limit).AsQueryable();
            return Json(new { total = total, rows = rows }, JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public JsonResult DelBaoJianById(string guid)
        {
            try
            {
                EFHelpler<BS_BAOJIAN> ef = new EFHelpler<BS_BAOJIAN>();
                BS_BAOJIAN model = db.BS_BAOJIAN.Find(guid);
                ef.delete(model);
                db.SaveChanges();
                return Json(new AjaxResult { state = ResultType.success.ToString(), message = "成功" });
            }
            catch (Exception e)
            {
                return Json(new AjaxResult { state = ResultType.error.ToString(), message = e.Message });
            }
        }
        #endregion

        #region 仓单
        /// <summary>
        /// 新增仓单
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult Add_C_CangDan(int id, string guid)
        {
            OperatorModel loginModel = OperatorProvider.Provider.GetCurrent();
            C_TB_HC_GOODSBILL model_goodsbill = db.C_TB_HC_GOODSBILL.Find(id) ?? new C_TB_HC_GOODSBILL();
            BS_CANGDAN model = db.BS_CANGDAN.Find(guid) ?? new BS_CANGDAN()
            {
                GoodsBillId = id,
                Guid = Guid.NewGuid().ToString()
            };
            List<BS_C_PINGMING> list_GOODS = db.BS_C_PINGMING.OrderBy(n => n.GoodsName).ToList();//货物
            List<SelectListItem> emplist_GOODS = SelectHelp.CreateSelect<BS_C_PINGMING>(list_GOODS, "GoodsName", "GoodsName", null);
            ViewData["GOODS_List"] = new SelectList(emplist_GOODS, "Value", "Text", "是");
            List<BS_C_TAITOUREN> list_CUSTOMER = db.BS_C_TAITOUREN.OrderBy(n => n.Guid).ToList();//抬头人
            List<SelectListItem> emplist_CUSTOMER = SelectHelp.CreateSelect<BS_C_TAITOUREN>(list_CUSTOMER, "GoodsName", "GoodsName", null);
            ViewData["CUSTOMER_List"] = new SelectList(emplist_CUSTOMER, "Value", "Text", "是");
            ViewBag.GoodsBillId = id;
            ViewBag.YingWenChuanMing = model_goodsbill.YingWenChuanMing;
            ViewBag.VGNO = model_goodsbill.VGNO;

            return View(model);
        }
        [HttpPost]
        public JsonResult Add_C_CangDan(BS_CANGDAN model)
        {
            OperatorModel loginModel = OperatorProvider.Provider.GetCurrent();
            try
            {
                if (string.IsNullOrEmpty(model.CangDanHao))           //新建时生成流水号
                {
                    string TodayTime = DateTime.Today.ToString("yyyyMMdd");
                    BS_CANGDAN Num = db.BS_CANGDAN.OrderByDescending(n => n.CreatTime).ToList().FirstOrDefault();
                    if (Num != null)
                    {
                        if (!string.IsNullOrEmpty(Num.CangDanHao))
                        {
                            if (Num.CangDanHao.Substring(4, 8) == TodayTime)
                            {
                                model.CangDanHao = "YGBS" + TodayTime + (Convert.ToInt32(Num.CangDanHao.Replace("YGBS" + TodayTime, "")) + 1).ToString("0000");
                            }
                            else
                            {
                                model.CangDanHao = "YGBS" + TodayTime + "0001";
                            }
                        }
                    }

                    else
                    {
                        model.CangDanHao = "YGBS" + TodayTime + "0001";
                    }

                }
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            {
                var msg = string.Empty;
                var errors = (from u in ex.EntityValidationErrors select u.ValidationErrors).ToList();
                foreach (var item in errors)
                    msg += item.FirstOrDefault().ErrorMessage;
                return Json(msg);
            }
            catch (Exception e)
            {
                return Json(e.Message);
            }

            var yuanquid = loginModel.YuanquID ?? throw new ArgumentNullException("loginModel.YuanquID");
            if (model.Guid != null)
            {
                model.CreatPeople = loginModel.UserName;
                model.CreatTime = DateTime.Now;
                model.BoolHuiShou = "否";
            }

            db.Set<BS_CANGDAN>().AddOrUpdate(model);
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
        public ActionResult CangDanList(int GoodsBillId)
        {
            ViewBag.GoodsBillId = GoodsBillId;
            return View();
        }

        [System.Web.Http.HttpGet]
        public object GetCangDanList(int limit, int offset, int GoodsBillId)
        {
            var wherelambda = ExtLinq.True<BS_CANGDAN>();
            wherelambda = wherelambda.And(t => t.GoodsBillId == GoodsBillId);
            var list = db.Set<BS_CANGDAN>().Where(wherelambda).OrderByDescending(n => n.CreatTime).AsQueryable();
            int total = list.Count();
            object rows = list.Skip(offset).Take(limit).AsQueryable();
            return Json(new { total = total, rows = rows }, JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public JsonResult DelCangDanById(string guid)
        {
            try
            {
                EFHelpler<BS_CANGDAN> ef = new EFHelpler<BS_CANGDAN>();
                BS_CANGDAN model = db.BS_CANGDAN.Find(guid);
                ef.delete(model);
                db.SaveChanges();
                return Json(new AjaxResult { state = ResultType.success.ToString(), message = "成功" });
            }
            catch (Exception e)
            {
                return Json(new AjaxResult { state = ResultType.error.ToString(), message = e.Message });
            }
        }
        #endregion

        #region 委托

        /// <summary>
        /// 新增委托
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult Add_C_WeiTuo(int id, int weiTuoID)
        {
            OperatorModel loginModel = OperatorProvider.Provider.GetCurrent();

            C_TB_HC_CONSIGN model = db.C_TB_HC_CONSIGN.Find(weiTuoID);
            if (model == null)
            {
                model = new C_TB_HC_CONSIGN();
                model.GOODSBILL_ID = id;
            }
            C_TB_HC_GOODSBILL model_goodsbill = db.C_TB_HC_GOODSBILL.Find(id) ?? db.C_TB_HC_GOODSBILL.Find(model.GOODSBILL_ID);
            List<C_TB_CODE_CUSTOMER> list_CUSTOMER = db.C_TB_CODE_CUSTOMER.Where(n => n.YuanQuID == loginModel.YuanquID).OrderBy(n => n.ID).ToList();//委托人,收货人
            List<SelectListItem> emplist_CUSTOMER = SelectHelp.CreateSelect<C_TB_CODE_CUSTOMER>(list_CUSTOMER, "Name", "Name", null);
            ViewData["CUSTOMER_List"] = new SelectList(emplist_CUSTOMER, "Value", "Text", "是");
            ViewBag.FengPiaoChuanMingHangCi = model_goodsbill.ShipName + "_" + model_goodsbill.VGNO;
            List<BS_BAOGUAN> list_ZuiYeDanHao = db.BS_BAOGUAN.Where(n => n.GoodsBillId == id && n.ZhongXinZuoYeDanHao != null).OrderBy(n => n.CreatTime).ToList();//作业单号
            List<SelectListItem> emplist_ZuiYeDanHao = SelectHelp.CreateSelect<BS_BAOGUAN>(list_ZuiYeDanHao, "ZhongXinZuoYeDanHao", "ZhongXinZuoYeDanHao", null);
            ViewData["ZuiYeDanHao_List"] = new SelectList(emplist_ZuiYeDanHao, "Value", "Text", "是");
            ViewBag.PLANWEIGHT = model_goodsbill.PLANWEIGHT;


            ViewBag.ShipName = model_goodsbill.ShipName;
            ViewBag.VGNO = model_goodsbill.VGNO;
            ViewBag.BLNO = model_goodsbill.BLNO;
            ViewBag.C_GOODS = model_goodsbill.C_GOODS;
            ViewBag.JiHuaDaoGangRiQi = model_goodsbill.JiHuaDaoGangRiQi;
            ViewBag.HuoZhu = model_goodsbill.HuoZhu;
            ViewBag.BaoShuiLeiXing = model_goodsbill.BaoShuiLeiXing;
            ViewBag.XiangShu = model_goodsbill.XiangShu;
            ViewBag.State = model_goodsbill.State;
            ViewBag.CreatPeople = model_goodsbill.CreatPeople;
            ViewBag.CreatTime = model_goodsbill.CreatTime;
            ViewBag.JieDanRiQi = model_goodsbill.JieDanRiQi;
            ViewBag.ContoractNumber = model_goodsbill.ContoractNumber;
            ViewBag.BoolDanZheng = model_goodsbill.BoolDanZheng;
            ViewBag.MARK = model_goodsbill.MARK;


            return View(model);
        }
        [HttpPost]
        public JsonResult Add_C_WeiTuo(C_TB_HC_CONSIGN model)
        {
            OperatorModel loginModel = OperatorProvider.Provider.GetCurrent();
            var yuanquid = loginModel.YuanquID ?? throw new ArgumentNullException("loginModel.YuanquID");
            C_TB_HC_GOODSBILL model_goodsBill = db.C_TB_HC_GOODSBILL.Find(model.GOODSBILL_ID);
            model.YuanQuID = yuanquid;
            model.CREATORNAME = loginModel.UserName;
            model.CREATETIME = DateTime.Now;
            model.State = "进行中";
            if (model.ZhiLingXiangMu == "入库" && model_goodsBill.State == "接单审核完")
            {
                return Json("改业务委托已接单审核完,不可新增入库委托");
            }
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
            db.Set<C_TB_HC_CONSIGN>().AddOrUpdate(model);
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
        public ActionResult WeiTuoList(int GoodsBillId)
        {
            ViewBag.GoodsBillId = GoodsBillId;
            return View();
        }

        [System.Web.Http.HttpGet]
        public object GetWeiTuoList(int limit, int offset, int GoodsBillId)
        {
            var wherelambda = ExtLinq.True<C_TB_HC_CONSIGN>();
            wherelambda = wherelambda.And(t => t.GOODSBILL_ID == GoodsBillId);
            var list = db.Set<C_TB_HC_CONSIGN>().Where(wherelambda).OrderByDescending(n => n.CREATETIME).AsQueryable();
            int total = list.Count();
            object rows = list.Skip(offset).Take(limit).AsQueryable();
            return Json(new { total = total, rows = rows }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult DelWeiTuoById(int id)
        {
            try
            {
                EFHelpler<C_TB_HC_CONSIGN> ef = new EFHelpler<C_TB_HC_CONSIGN>();
                C_TB_HC_CONSIGN model = db.C_TB_HC_CONSIGN.Find(id);
                ef.delete(model);
                db.SaveChanges();
                return Json(new AjaxResult { state = ResultType.success.ToString(), message = "成功" });
            }
            catch (Exception e)
            {
                return Json(new AjaxResult { state = ResultType.error.ToString(), message = e.Message });
            }
        }
        #endregion

        /// <summary>
        /// 业务详细
        /// </summary>
        /// <returns></returns>
        public ActionResult YwWeiTuo_Deilated(int id)
        {
            C_TB_HC_GOODSBILL model_goodsbill = db.C_TB_HC_GOODSBILL.Find(id) ?? new C_TB_HC_GOODSBILL();

            List<BS_BAOGUAN> bgList = db.BS_BAOGUAN.Where(n => n.GoodsBillId == id).OrderBy(n => n.CreatTime).ToList();
            List<BS_BAOJIAN> bjList = db.BS_BAOJIAN.Where(n => n.GoodsBillId == id).OrderBy(n => n.CreatTime).ToList();
            List<BS_CANGDAN> cdList = db.BS_CANGDAN.Where(n => n.GoodsBillId == id).OrderBy(n => n.CreatTime).ToList();

            ViewBag.model_goodsbill = model_goodsbill;
            ViewBag.bgList = bgList;
            ViewBag.bjList = bjList;
            ViewBag.cdList = cdList;
            return View();
        }
        public JsonResult GetCONSIGNList(decimal id)
        {
            List<C_TB_HC_CONSIGN> list = db.C_TB_HC_CONSIGN.Where(n => n.GOODSBILL_ID == id).OrderByDescending(n => n.ID).ToList();
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

        /// <summary>
        /// 新增放货指令
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult Add_C_FangHuoZhiLing(int id, string guid)
        {
            OperatorModel loginModel = OperatorProvider.Provider.GetCurrent();
            BS_FANGHUOZHILING model = db.BS_FANGHUOZHILING.Find(guid);
            if (model == null)
            {
                model = new BS_FANGHUOZHILING();
                model.Guid = Guid.NewGuid().ToString();
                model.GoodsBillId = id;
                model.State = "待领导审核";
            }
            if (model == null || model.State == "被驳回")
            {
                model.State = "待领导审核";

            }

            List<C_TB_CODE_CUSTOMER> list_CUSTOMER = db.C_TB_CODE_CUSTOMER.Where(n => n.YuanQuID == loginModel.YuanquID).OrderBy(n => n.ID).ToList();//委托人,收货人
            List<SelectListItem> emplist_CUSTOMER = SelectHelp.CreateSelect<C_TB_CODE_CUSTOMER>(list_CUSTOMER, "Name", "Name", null);
            ViewData["CUSTOMER_List"] = new SelectList(emplist_CUSTOMER, "Value", "Text", "是");
            ViewBag.GoodsBillId = id;
            return View(model);
        }
        [HttpPost]
        public JsonResult Add_C_FangHuoZhiLing(BS_FANGHUOZHILING model)
        {
            OperatorModel loginModel = OperatorProvider.Provider.GetCurrent();
            var yuanquid = loginModel.YuanquID ?? throw new ArgumentNullException("loginModel.YuanquID");
            List<BS_FangHuoNeiBuShenPi> List_sh = db.BS_FangHuoNeiBuShenPi.Where(n => n.GoodsBillId == model.GoodsBillId && n.State_BaoGuan == "结束" && n.State_CangDan == "仓单提货已确认" && n.State_JiFei == "计费已完成").ToList();
            List<BS_FangHuoNeiBuShenPi> List_sh_btg = db.BS_FangHuoNeiBuShenPi.Where(n => n.GoodsBillId == model.GoodsBillId && (n.State_BaoGuan != "结束" || n.State_CangDan != "仓单提货已确认" || n.State_JiFei != "计费已完成")).ToList();
            if (List_sh.Count() > 0 && List_sh_btg.Count() == 0)
            {
                model.State = "审核通过";
            }
            model.CreatPeople = loginModel.UserName;
            model.CreatTime = DateTime.Now;
            db.Set<BS_FANGHUOZHILING>().AddOrUpdate(model);
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
        public ActionResult FangHuoZhiLingList(int GoodsBillId)
        {
            ViewBag.GoodsBillId = GoodsBillId;
            return View();
        }
        public ActionResult FangHuoZhiLingList_sh()
        {
            return View();
        }
        [System.Web.Http.HttpGet]
        public object GetFangHuoZhiLingList(int limit, int offset, int GoodsBillId)
        {
            var wherelambda = ExtLinq.True<BS_FANGHUOZHILING>();
            wherelambda = wherelambda.And(t => t.GoodsBillId == GoodsBillId);
            var list = db.Set<BS_FANGHUOZHILING>().Where(wherelambda).OrderByDescending(n => n.CreatTime).AsQueryable();
            int total = list.Count();
            object rows = list.Skip(offset).Take(limit).AsQueryable();
            return Json(new { total = total, rows = rows }, JsonRequestBehavior.AllowGet);
        }
        [System.Web.Http.HttpGet]
        public object GetFangHuoZhiLingList_sh(int limit, int offset)
        {
            OperatorModel loginModel = OperatorProvider.Provider.GetCurrent();
            var wherelambda = ExtLinq.True<BS_FANGHUOZHILING>();
            wherelambda = wherelambda.And(t => t.State == "待领导审核");
            List<BS_FANGHUOZHILING> BS_FANGHUOZHILING = new List<BS_FANGHUOZHILING>();
            List<C_TB_HC_GOODSBILL> list_GOODSBILL = db.C_TB_HC_GOODSBILL.Where(n => n.YuanQuID == loginModel.YuanquID).OrderBy(n => n.ID).ToList();
            foreach (var items in list_GOODSBILL)
            {
                List<BS_FANGHUOZHILING> list_th = db.BS_FANGHUOZHILING.Where(n => n.GoodsBillId == items.ID).ToList();
                foreach (var items_th in list_th)
                {
                    BS_FANGHUOZHILING.Add(items_th);
                }
            }

            var list = db.BS_FANGHUOZHILING.Where(wherelambda).OrderByDescending(n => n.CreatTime).AsQueryable();
            int total = list.Count();
            object rows = list.Skip(offset).Take(limit).AsQueryable();
            return Json(new { total = total, rows = rows }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult TiHuoList(int GoodsBillId)
        {
            ViewBag.GoodsBillId = GoodsBillId;
            return View();
        }

        [System.Web.Http.HttpGet]
        public object GetTiHuoList(int limit, int offset, int GoodsBillId)
        {
            var wherelambda = ExtLinq.True<BS_FangHuoNeiBuShenPi>();
            wherelambda = wherelambda.And(t => t.GoodsBillId == GoodsBillId);
            var list = db.Set<BS_FangHuoNeiBuShenPi>().Where(wherelambda).OrderByDescending(n => n.CreatTime).AsQueryable();
            int total = list.Count();
            object rows = list.Skip(offset).Take(limit).AsQueryable();
            return Json(new { total = total, rows = rows }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult DelFangHuoZhiLingById(string guid)
        {
            try
            {
                EFHelpler<BS_FANGHUOZHILING> ef = new EFHelpler<BS_FANGHUOZHILING>();
                BS_FANGHUOZHILING model = db.BS_FANGHUOZHILING.Find(guid);
                ef.delete(model);
                db.SaveChanges();
                return Json(new AjaxResult { state = ResultType.success.ToString(), message = "成功" });
            }
            catch (Exception e)
            {
                return Json(new AjaxResult { state = ResultType.error.ToString(), message = e.Message });
            }
        }
        [HttpPost]
        public JsonResult DelTiHuoById(string guid)
        {
            try
            {
                EFHelpler<BS_FangHuoNeiBuShenPi> ef = new EFHelpler<BS_FangHuoNeiBuShenPi>();
                BS_FangHuoNeiBuShenPi model = db.BS_FangHuoNeiBuShenPi.Find(guid);
                ef.delete(model);
                db.SaveChanges();
                return Json(new AjaxResult { state = ResultType.success.ToString(), message = "成功" });
            }
            catch (Exception e)
            {
                return Json(new AjaxResult { state = ResultType.error.ToString(), message = e.Message });
            }
        }

        public ActionResult AddTallyBll(decimal CONSIGN_ID, int id, string type)
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

            TallyBllController tl = new TallyBllController();
            List<SelectListItem> STORAGEList = tl.GetChang();
            ViewData["STORAGEList"] = new SelectList(STORAGEList, "Value", "Text");
            ViewBag.CONSIGN_ID = CONSIGN_ID;
            ViewBag.CGNO = model_CONSIGN.CGNO;
            ViewBag.RoleId = loginModel.RoleId;
            ViewBag.type = type;
            return View(model);
        }
        public ActionResult AddTallyBll_pj(decimal CONSIGN_ID, int id, string type)
        {
            decimal? allin = 0;//累计进库
            decimal? allout = 0;//累计出库
            int sum = 0;
            C_TB_HC_CONSIGN model_consign = db.C_TB_HC_CONSIGN.Find(CONSIGN_ID) ?? new C_TB_HC_CONSIGN();
            C_TB_HC_GOODSBILL model_goodsbill = db.C_TB_HC_GOODSBILL.Find(model_consign.GOODSBILL_ID) ?? new C_TB_HC_GOODSBILL();
            List<C_TB_HC_CONSIGN> list_consign = db.C_TB_HC_CONSIGN.Where(n => n.GOODSBILL_ID == model_goodsbill.ID).OrderByDescending(n => n.ID).ToList();

            foreach (var items_consign in list_consign)
            {
                List<C_TB_HS_TALLYBILL> list = db.C_TB_HS_TALLYBILL.Where(n => n.CONSIGN_ID == items_consign.ID && n.Type != "清空场存").OrderByDescending(n => n.ID).ToList();
                List<C_TB_HS_TALLYBILL> list_qc = db.C_TB_HS_TALLYBILL.Where(n => n.CONSIGN_ID == items_consign.ID && n.Type == "清空场存").OrderByDescending(n => n.ID).ToList();
                sum += list_qc.Count();
                foreach (var items in list)
                {
                    if (items.CODE_OPSTYPE == "进库")
                    {
                        allin += items.WEIGHT;
                    }
                    if (items.CODE_OPSTYPE == "出库")
                    {
                        allout += items.WEIGHT;
                    }
                }
            }
           
            OperatorModel loginModel = OperatorProvider.Provider.GetCurrent();
            C_TB_HS_TALLYBILL model = db.C_TB_HS_TALLYBILL.Find(id) ?? new C_TB_HS_TALLYBILL();
            model.TALLYMAN = loginModel.UserName;
            C_TB_HC_CONSIGN model_CONSIGN = db.C_TB_HC_CONSIGN.Find(CONSIGN_ID) ?? new C_TB_HC_CONSIGN();
            model.ChuanMing = model_CONSIGN.ShipName;
            model.HangCi = model_CONSIGN.VGNO;
            model.HuoDai = model_CONSIGN.WeiTuoRen;
            model.GoodsName = model_CONSIGN.GoodsName;
            model.CAOZUO = model_CONSIGN.CODE_OPERATION; //作业过程
            ViewBag.allin = allin;
            ViewBag.allout = allout;
            if (sum != 0)
            {
                ViewBag.sunyi = allout - allin;
            }
            else
            {
                ViewBag.sunyi = 0;
            }
            

            if (model_CONSIGN.ZhiLingXiangMu == "入库")
            {
                ViewBag.ZhiLingXiangMu = "进库";
            }
            if (model_CONSIGN.ZhiLingXiangMu == "出库")
            {
                ViewBag.ZhiLingXiangMu = "出库";
            }
            TallyBllController tl = new TallyBllController();
            List<SelectListItem> STORAGEList = tl.GetChang();
            ViewData["STORAGEList"] = new SelectList(STORAGEList, "Value", "Text");
            List<BS_LAOWUZUOYELEIBIE> list_zuoye = db.BS_LAOWUZUOYELEIBIE.Where(n => n.JinChuKu == model_CONSIGN.ZhiLingXiangMu && n.YuanQuId == loginModel.YuanquID).ToList();
            ViewData["ZuoYe"] = list_zuoye;
            ViewBag.CONSIGN_ID = CONSIGN_ID;
            ViewBag.CGNO = model_CONSIGN.CGNO;
            ViewBag.RoleId = loginModel.RoleId;
            ViewBag.type = type;
            ViewBag.CONSIGN_ID = CONSIGN_ID;
            ViewBag.WeiTuoBianHao = model_CONSIGN.WeiTuoBianHao;
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
            C_TB_HC_CONSIGN model_CONSIGN_sh = db.C_TB_HC_CONSIGN.FirstOrDefault(n => n.ID == model.CONSIGN_ID) ?? new C_TB_HC_CONSIGN();
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
            if (model.Type == "清场")
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
                //if (model_GOODSBILL.State == "接单审核完" && model.CODE_OPSTYPE == "进库")
                //{
                //    return Json("改业务委托已接单审核完，不可添加进库理货单");
                //}
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
                TallyBllController tl = new TallyBllController();
                bool b = tl.EidSTOCKDORMANT(model, AorU);
                C_TB_HC_CONSIGN model_CONSIGN = db.C_TB_HC_CONSIGN.Find(model.CONSIGN_ID) ?? new C_TB_HC_CONSIGN();
                List<C_TB_HS_TALLYBILL> list_TALLYBILL = db.C_TB_HS_TALLYBILL
                    .Where(n => n.CONSIGN_ID == model.CONSIGN_ID).OrderBy(n => n.ID).ToList();
                if (list_TALLYBILL.Count != 0)
                {
                    model_CONSIGN.State = "已生成" + list_TALLYBILL.Count + "条信息";
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

                return Json(new AjaxResult { state = ResultType.error.ToString(), message = msg });
            }
            catch (Exception ex)
            {
                return Json(new AjaxResult { state = ResultType.error.ToString(), message = ex.ToString() });
            }



        }
        [HttpPost]
        public ActionResult AddTallyBll_pj(C_TB_HS_TALLYBILL model)
        {
            decimal? KuCun = 0;
            decimal? KuCunW = 0;
            decimal? KuCUnX = 0;
            OperatorModel loginModel = OperatorProvider.Provider.GetCurrent();
            C_TB_HC_CONSIGN model_CONSIGN_sh = db.C_TB_HC_CONSIGN.FirstOrDefault(n => n.ID == model.CONSIGN_ID) ?? new C_TB_HC_CONSIGN();
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
            model.TALLYMAN = loginModel.UserName;
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
            if (model.Type == "清场")
            {
                model.CreatTime = DateTime.Now;
            }
            C_TB_HC_CONSIGN con = db.C_TB_HC_CONSIGN.FirstOrDefault(n => n.CGNO == cgno);
            if (con != null) model.CONSIGN_ID = con.ID;
           

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
                TallyBllController tl = new TallyBllController();
                bool b = tl.EidSTOCKDORMANT(model, AorU);
                C_TB_HC_CONSIGN model_CONSIGN = db.C_TB_HC_CONSIGN.Find(model.CONSIGN_ID) ?? new C_TB_HC_CONSIGN();
                List<C_TB_HS_TALLYBILL> list_TALLYBILL = db.C_TB_HS_TALLYBILL
                    .Where(n => n.CONSIGN_ID == model.CONSIGN_ID).OrderBy(n => n.ID).ToList();
                if (list_TALLYBILL.Count != 0)
                {
                    model_CONSIGN.State = "已生成" + list_TALLYBILL.Count + "条信息";
                }
                else
                {
                    model_CONSIGN.State = "进行中";
                }
                if (model.State == "1")//1为进库状态
                {
                    model.State = "进行中";
                }
                if (model.State == "2")//2为完工状态
                {

                    model_CONSIGN.State = "已完成";
                    foreach(var items in list_TALLYBILL)
                        {
                        items.State = "已完成";
                        db.Set<C_TB_HS_TALLYBILL>().AddOrUpdate(items);
                    }
                }
               
                db.Set<C_TB_HC_CONSIGN>().AddOrUpdate(model_CONSIGN);
                db.SaveChanges();
                string datalist = model.ZuoYeLeiXIng;
                List<BS_LAOWUZUOYELEIBIE> list_TdZylb = JsonConvert.DeserializeObject<List<BS_LAOWUZUOYELEIBIE>>(datalist);
                foreach (var items_TdZylb in list_TdZylb)
                {
                    BS_ZYLB_TBLL Model_BS_ZYLB_TBLL = new BS_ZYLB_TBLL();
                    Model_BS_ZYLB_TBLL.TallBllId = model.ID;
                    Model_BS_ZYLB_TBLL.ConSignId = model_CONSIGN.ID;
                    Model_BS_ZYLB_TBLL.Guid = Guid.NewGuid().ToString();
                    Model_BS_ZYLB_TBLL.ZuoYeLeiBieMingCheng = items_TdZylb.ZuoYeLeiBieMingCheng;
                    Model_BS_ZYLB_TBLL.GongSiMingCheng = items_TdZylb.GongSiMingCheng;
                    Model_BS_ZYLB_TBLL.JinChuKu = items_TdZylb.JinChuKu;
                    Model_BS_ZYLB_TBLL.DanJia = items_TdZylb.DanJia;
                    Model_BS_ZYLB_TBLL.BeiZhu = items_TdZylb.BeiZhu;
                    Model_BS_ZYLB_TBLL.CreatPeople = loginModel.UserName;
                    Model_BS_ZYLB_TBLL.CreatTime = DateTime.Now;
                    Model_BS_ZYLB_TBLL.YuanQuId = loginModel.YuanquID;
                    Model_BS_ZYLB_TBLL.ShuLiang = items_TdZylb.ShuLiang;
                    Model_BS_ZYLB_TBLL.JinE = items_TdZylb.ShuLiang* items_TdZylb.DanJia.ToDecimal();
                    db.Set<BS_ZYLB_TBLL>().AddOrUpdate(Model_BS_ZYLB_TBLL);
                    db.SaveChanges();
                }
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

                return Json(new AjaxResult { state = ResultType.error.ToString(), message = msg });
            }
            catch (Exception ex)
            {
                return Json(new AjaxResult { state = ResultType.error.ToString(), message = ex.ToString() });
            }



        }

        [OperationLog(OperationLogAttribute.Operatetype.审核, OperationLogAttribute.ImportantLevel.一般操作, "审核通过")]
        [HttpPost]
        public JsonResult shenheGoodsBill(int id)
        {

            try
            {
                C_TB_HC_GOODSBILL model_GoodsBill = db.C_TB_HC_GOODSBILL.Find(id) ?? new C_TB_HC_GOODSBILL();
                model_GoodsBill.State = "接单审核完";
                db.Set<C_TB_HC_GOODSBILL>().AddOrUpdate(model_GoodsBill);
                db.SaveChanges();
                List<C_TB_HC_CONSIGN> list_CONSIGN = db.C_TB_HC_CONSIGN.Where(n => n.GOODSBILL_ID == model_GoodsBill.ID && n.ZhiLingXiangMu == "入库").OrderBy(n => n.ID).ToList();
                foreach (var items_CONSIGN in list_CONSIGN)
                {
                    C_TB_HC_CONSIGN model_CONSIGN = db.C_TB_HC_CONSIGN.Find(items_CONSIGN.ID) ?? new C_TB_HC_CONSIGN();
                    model_CONSIGN.State = "已完成";
                    db.Set<C_TB_HC_CONSIGN>().AddOrUpdate(model_CONSIGN);
                    db.SaveChanges();
                    List<C_TB_HS_TALLYBILL> list_TALLYBILL = db.C_TB_HS_TALLYBILL.Where(n => n.CONSIGN_ID == model_CONSIGN.ID && n.CODE_OPSTYPE == "进库").OrderBy(n => n.ID).ToList();
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
        [OperationLog(OperationLogAttribute.Operatetype.审核, OperationLogAttribute.ImportantLevel.一般操作, "仓单提货审核通过")]
        [HttpPost]
        public JsonResult shenheCdth(string id)
        {
            BS_CANGDAN model_FangHuoNeiBuShenPi = db.BS_CANGDAN.Find(id) ?? new BS_CANGDAN();
            model_FangHuoNeiBuShenPi.State = "仓单提货已确认";
            db.Set<BS_CANGDAN>().AddOrUpdate(model_FangHuoNeiBuShenPi);
            BS_FangHuoNeiBuShenPi model_sh = db.BS_FangHuoNeiBuShenPi.FirstOrDefault(n => n.GoodsBillId == model_FangHuoNeiBuShenPi.GoodsBillId && n.State_CangDan != "仓单提货已确认");
            if (model_sh != null)
            {
                model_sh.State_CangDan = "仓单提货已确认";
                db.Set<BS_FangHuoNeiBuShenPi>().AddOrUpdate(model_sh);
            }
            try
            {

                db.SaveChanges();
                return Json(new AjaxResult { state = ResultType.success.ToString(), message = "成功" }.ToJson());
            }
            catch (Exception e)
            {
                return Json(new AjaxResult { state = ResultType.error.ToString(), message = e.Message }.ToJson());
            }
        }
        [HttpPost]
        public JsonResult shenhe_fh(string id)
        {

            try
            {
                BS_FANGHUOZHILING model_FangHuoNeiBuShenPi = db.BS_FANGHUOZHILING.Find(id) ?? new BS_FANGHUOZHILING();
                model_FangHuoNeiBuShenPi.State = "领导审核通过";
                db.Set<BS_FANGHUOZHILING>().AddOrUpdate(model_FangHuoNeiBuShenPi);
                db.SaveChanges();
                return Json(new AjaxResult { state = ResultType.success.ToString(), message = "成功" }.ToJson());
            }
            catch (Exception e)
            {
                return Json(new AjaxResult { state = ResultType.error.ToString(), message = e.Message }.ToJson());
            }
        }
        [HttpPost]
        public JsonResult cdhs(string id)
        {
            OperatorModel loginModel = OperatorProvider.Provider.GetCurrent();
            try
            {
                BS_CANGDAN model_CANGDAN = db.BS_CANGDAN.Find(id) ?? new BS_CANGDAN();
                model_CANGDAN.BoolHuiShou = "是";
                model_CANGDAN.HuiShouRen = loginModel.UserName;
                model_CANGDAN.HuiShouRiQi = DateTime.Now;
                db.Set<BS_CANGDAN>().AddOrUpdate(model_CANGDAN);
                db.SaveChanges();
                return Json(new AjaxResult { state = ResultType.success.ToString(), message = "成功" }.ToJson());
            }
            catch (Exception e)
            {
                return Json(new AjaxResult { state = ResultType.error.ToString(), message = e.Message }.ToJson());
            }
        }
        [HttpPost]
        public JsonResult shenhe_fh_bh(string id)
        {

            try
            {
                BS_FANGHUOZHILING model_FangHuoNeiBuShenPi = db.BS_FANGHUOZHILING.Find(id) ?? new BS_FANGHUOZHILING();
                model_FangHuoNeiBuShenPi.State = "被驳回";
                db.Set<BS_FANGHUOZHILING>().AddOrUpdate(model_FangHuoNeiBuShenPi);
                db.SaveChanges();
                return Json(new AjaxResult { state = ResultType.success.ToString(), message = "成功" }.ToJson());
            }
            catch (Exception e)
            {
                return Json(new AjaxResult { state = ResultType.error.ToString(), message = e.Message }.ToJson());
            }
        }
        /// <summary>
        /// 新增提货审批
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult Add_C_TiHuoShengPi(int id, string guid)
        {
            OperatorModel loginModel = OperatorProvider.Provider.GetCurrent();
            BS_FangHuoNeiBuShenPi model = db.BS_FangHuoNeiBuShenPi.Find(guid);
            C_TB_HC_GOODSBILL model_goodsbill = db.C_TB_HC_GOODSBILL.Find(id) ?? new C_TB_HC_GOODSBILL();
            if (model == null)
            {
                model = new BS_FangHuoNeiBuShenPi();
               // model.Guid = Guid.NewGuid().ToString();
                model.GoodsBillId = id;
                ViewBag.ChuanMing = model_goodsbill.ShipName;
                ViewBag.TiDanHao = model_goodsbill.BLNO;
                ViewBag.HuoMing = model_goodsbill.C_GOODS;
                ViewBag.TiDanShu = model_goodsbill.PLANWEIGHT;
                ViewBag.CunHuoRen = model_goodsbill.HuoZhu;
            }
            else
            {
                ViewBag.ChuanMing = model.ChuanMing;
                ViewBag.TiDanHao = model.TiDanHao;
                ViewBag.HuoMing = model.HuoMing;
                ViewBag.TiDanShu = model.TiDanShu;
                ViewBag.CunHuoRen = model.CunHuoRen;
                ViewBag.TiHuoRen = model.TiHuoRen;
            }

            //List<C_TB_CODE_CUSTOMER> list_CUSTOMER = db.C_TB_CODE_CUSTOMER.Where(n => n.YuanQuID == loginModel.YuanquID).OrderBy(n => n.ID).ToList();//委托人,收货人
            //List<SelectListItem> emplist_CUSTOMER = SelectHelp.CreateSelect<C_TB_CODE_CUSTOMER>(list_CUSTOMER, "Name", "Name", null);
            //ViewData["CUSTOMER_List"] = new SelectList(emplist_CUSTOMER, "Value", "Text", "是");
            List<C_GOODS> list_GOODS = db.C_GOODS.OrderBy(n => n.GoodsName).ToList();//货物
            List<SelectListItem> emplist_GOODS = SelectHelp.CreateSelect<C_GOODS>(list_GOODS, "GoodsName", "GoodsName", null);
            ViewData["GOODS_List"] = new SelectList(emplist_GOODS, "Value", "Text", "是");

            C_TB_HC_CONTRACT Model_CONTRACT = db.C_TB_HC_CONTRACT.FirstOrDefault(n => n.Guid == model_goodsbill.CONTRACT_Guid);//查找合同
            decimal? KuCun = 0;
            decimal? KuCunW = 0;
            List<C_TB_HC_CONSIGN> list_CONSIGN = db.C_TB_HC_CONSIGN.Where(n => n.GOODSBILL_ID == model_goodsbill.ID).OrderBy(n => n.ID).ToList();
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
            ViewBag.ShengYuShuLiang = KuCunW;
            ViewBag.GoodsBillId = id;
            return View(model);
        }
        [HttpPost]
        public JsonResult Add_C_TiHuoShengPi(BS_FangHuoNeiBuShenPi model)
        {
            int state_pd = 0;
            OperatorModel loginModel = OperatorProvider.Provider.GetCurrent();
            var yuanquid = loginModel.YuanquID ?? throw new ArgumentNullException("loginModel.YuanquID");
            if (model.Guid == null)
            {
                model.Guid = Guid.NewGuid().ToString();
                state_pd = 1;
            }
            model.CreatPeople = loginModel.UserName;
            model.CreatTime = DateTime.Now;
            model.State_CangDan = "仓单未确认";
            model.State_BaoGuan = "报关未放行";
            model.State_JiFei = "计费未完成";
            int count = db.Set<BS_FangHuoNeiBuShenPi>().Where(n => n.GoodsBillId == model.GoodsBillId && (n.State_BaoGuan != "结束" || n.State_JiFei != "计费已完成" || n.State_CangDan != "仓单提货已确认")).Count();
            if (state_pd == 1)
            {
                if (count >= 1)
                {
                    return Json("有未审批完的提货确认函");
                }
            }
           
            db.Set<BS_FangHuoNeiBuShenPi>().AddOrUpdate(model);
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

        #region 报关管理

        public ActionResult BaoGuanGuanliIndex()
        {
            OperatorModel loginModel = OperatorProvider.Provider.GetCurrent();
            ViewBag.roleId = loginModel.RoleId;
            return View();
        }
        [HttpGet]
        public object GetBaoGuanGuanliList(int limit, int offset, string GBNO, string C_GOODSAGENT_NAME, string CreatTime, string CreatTime1, string BLNO, string C_GOODS, string HuoZhu, string ShipName, string VGNO)
        {

            var wherelambda = ExtLinq.True<C_TB_HC_GOODSBILL>();
            OperatorModel loginModel = OperatorProvider.Provider.GetCurrent();
            wherelambda = wherelambda.And(t => t.YuanQuID == loginModel.YuanquID);
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
            int count = db.Set<C_TB_HC_GOODSBILL>().Where(wherelambda).OrderByDescending(n => n.CreatTime).Count();
            var list = db.Set<C_TB_HC_GOODSBILL>().Where(wherelambda).OrderByDescending(n => n.CreatTime).Skip(offset).Take(limit).AsQueryable();

            foreach (var itsms_GoodsBill in list)//计算库存
            {
                C_TB_HC_CONTRACT Model_CONTRACT = db.C_TB_HC_CONTRACT.FirstOrDefault(n => n.Guid == itsms_GoodsBill.CONTRACT_Guid);//查找合同
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
                decimal? ShiJiJInKu = 0;
                decimal? ShiJiChuKu = 0;
                itsms_GoodsBill.KunCun = KuCun.ToString();
                itsms_GoodsBill.KunCunW = KuCunW.ToDecimal(3).ToString();
                List<C_TB_JIESUAN> list_JieSuan = db.C_TB_JIESUAN.Where(n => n.GoodsBill_id == itsms_GoodsBill.ID).OrderBy(n => n.ID).ToList();
                itsms_GoodsBill.Lkjehj = list_JieSuan.Sum(n => n.LaiKuanJinE).ToDecimal(2);
                List<C_TB_CHENGBEN> list_ChengBen = db.C_TB_CHENGBEN.Where(n => n.GoodsBill_id == itsms_GoodsBill.ID).OrderBy(n => n.ID).ToList();
                itsms_GoodsBill.ChengBenJinE = list_ChengBen.Sum(n => n.ChengBenJinE).ToDecimal(2);
                if (list_JieSuan.Sum(n => n.KaiPiaoJinE) == 0)
                {
                    itsms_GoodsBill.HuiShouLv = 0;
                }
                else
                {
                    itsms_GoodsBill.HuiShouLv = (list_JieSuan.Sum(n => n.LaiKuanJinE) / list_JieSuan.Sum(n => n.KaiPiaoJinE)).ToDecimal(2);

                }
                List<C_TB_HC_CONSIGN> list_KuCunCONSIGN = db.C_TB_HC_CONSIGN.Where(n => n.GOODSBILL_ID == itsms_GoodsBill.ID).OrderBy(n => n.ID).ToList();
                foreach (var items_KuCunCONSIGN in list_KuCunCONSIGN)
                {
                    List<C_TB_HS_TALLYBILL> list_KuCunTALLYBILL = db.C_TB_HS_TALLYBILL.Where(n => n.CONSIGN_ID == items_KuCunCONSIGN.ID).OrderBy(n => n.ID).ToList();
                    ShiJiJInKu += list_KuCunTALLYBILL.Where(n => n.CODE_OPSTYPE == "进库" && n.Type != "清场").Sum(n => n.WEIGHT);
                    ShiJiChuKu += list_KuCunTALLYBILL.Where(n => n.CODE_OPSTYPE == "出库" && n.Type != "清场").Sum(n => n.WEIGHT);
                }
                itsms_GoodsBill.ShiJiJInKu = ShiJiJInKu;
                itsms_GoodsBill.ShiJiChuKu = ShiJiChuKu;
            }
            int total = count;
            object rows = list;
            return Json(new { total = total, rows = rows }, JsonRequestBehavior.AllowGet);

        }

        [System.Web.Http.HttpGet]
        public object GetBaoGuanList2(int GoodsBillId)
        {
            var wherelambda = ExtLinq.True<BS_BAOGUAN>();
            wherelambda = wherelambda.And(t => t.GoodsBillId == GoodsBillId);
            List<BS_BAOGUAN> list = db.Set<BS_BAOGUAN>().Where(wherelambda).OrderByDescending(n => n.CreatTime).ToList();

            int total = list.Count;
            object rows = list;
            return Json(new { total = total, rows = rows }, JsonRequestBehavior.AllowGet);

        }
        #endregion

        #region 仓单管理
        public ActionResult CangDanIndex()
        {
            return View();
        }
        [System.Web.Http.HttpGet]
        public object GetCangDanList2(int GoodsBillId)
        {
            var wherelambda = ExtLinq.True<BS_CANGDAN>();
            wherelambda = wherelambda.And(t => t.GoodsBillId == GoodsBillId);
            var list = db.Set<BS_CANGDAN>().Where(wherelambda).OrderByDescending(n => n.CreatTime).AsQueryable();
            int total = list.Count();
            object rows = list;
            return Json(new { total = total, rows = rows }, JsonRequestBehavior.AllowGet);

        }

        #endregion
        public JsonResult qx_shenhe(int id)
        {
            try
            {
                C_TB_HC_CONSIGN model_consign = db.C_TB_HC_CONSIGN.Find(id) ?? new C_TB_HC_CONSIGN();
                C_TB_HC_GOODSBILL model_goodsbill = db.C_TB_HC_GOODSBILL.Find(model_consign.GOODSBILL_ID) ?? new C_TB_HC_GOODSBILL();
                List<C_TB_HC_CONSIGN> list_Consign = db.C_TB_HC_CONSIGN.Where(n => n.GOODSBILL_ID == model_goodsbill.ID).OrderByDescending(n => n.ID).ToList();
                foreach (var items_consign in list_Consign)
                {
                    List<C_TB_HS_TALLYBILL> list_TallBill = db.C_TB_HS_TALLYBILL.Where(n => n.CONSIGN_ID == items_consign.ID).OrderByDescending(n => n.ID).ToList();
                    foreach (var items in list_TallBill)
                    {
                        items.State = "进行中";
                        db.Set<C_TB_HS_TALLYBILL>().AddOrUpdate(items);
                        db.SaveChanges();
                    }
                    items_consign.State = "进行中";
                    db.SaveChanges();
                }

                return Json(new AjaxResult { state = ResultType.success.ToString(), message = "成功" }.ToJson());
            }
            catch (Exception e)
            {
                return Json(new AjaxResult { state = ResultType.error.ToString(), message = e.Message }.ToJson());
            }
        }
        public JsonResult qkcc(int id)
        {
            decimal? allin = 0;
            decimal? allout = 0;
            List<C_TB_HS_TALLYBILL> list_TALLYBILL = db.C_TB_HS_TALLYBILL.Where(n => n.CONSIGN_ID == id && n.Type != "清空场存").OrderByDescending(n => n.ID).ToList();
            C_TB_HC_CONSIGN model_consign = db.C_TB_HC_CONSIGN.Find(id) ?? new C_TB_HC_CONSIGN();
            C_TB_HC_GOODSBILL model_goodsbill = db.C_TB_HC_GOODSBILL.Find(model_consign.GOODSBILL_ID) ?? new C_TB_HC_GOODSBILL();
            List<C_TB_HC_CONSIGN> list_consign = db.C_TB_HC_CONSIGN.Where(n => n.GOODSBILL_ID == model_goodsbill.ID).OrderByDescending(n => n.ID).ToList();
            foreach (var items_consign in list_consign)
            {
                List<C_TB_HS_TALLYBILL> list = db.C_TB_HS_TALLYBILL.Where(n => n.CONSIGN_ID == items_consign.ID && n.Type != "清空场存").OrderByDescending(n => n.ID).ToList();
                List<C_TB_HS_TALLYBILL> list_qc = db.C_TB_HS_TALLYBILL.Where(n => n.CONSIGN_ID == items_consign.ID && n.Type == "清空场存").OrderByDescending(n => n.ID).ToList();
                foreach (var items in list)
                {
                    if (items.CODE_OPSTYPE == "进库")
                    {
                        allin += items.WEIGHT;
                    }
                    if (items.CODE_OPSTYPE == "出库")
                    {
                        allout += items.WEIGHT;
                    }
                }
            }
            C_TB_HS_TALLYBILL model_TALLYBILL =  new C_TB_HS_TALLYBILL();
            model_TALLYBILL = list_TALLYBILL.FirstOrDefault();
            if (allin> allout)
            {
                model_TALLYBILL.CODE_OPSTYPE = "出库";
                model_TALLYBILL.WEIGHT = allin - allout;
                model_TALLYBILL.Type = "清空场存";
                model_TALLYBILL.State = "已完成";
            }
            if (allin < allout)
            {
                model_TALLYBILL.CODE_OPSTYPE = "进";
                model_TALLYBILL.WEIGHT = allout - allin;
                model_TALLYBILL.Type = "清空场存";
                model_TALLYBILL.State = "已完成";
            }
            try
            {
               
                if (list_TALLYBILL.Count != 0)
                {
                    model_consign.State = "已生成" + list_TALLYBILL.Count + "条信息";
                }
                db.Set<C_TB_HC_CONSIGN>().AddOrUpdate(model_consign);
                db.Set<C_TB_HS_TALLYBILL>().Add(model_TALLYBILL);
                db.SaveChanges();

                return Json(new AjaxResult { state = ResultType.success.ToString(), message = "成功" }.ToJson());
            }
            catch (Exception e)
            {
                return Json(new AjaxResult { state = ResultType.error.ToString(), message = e.Message }.ToJson());
            }
        }
        public JsonResult hykd(int id)
        {
            List<C_TB_HS_TALLYBILL> list_TALLYBILL = db.C_TB_HS_TALLYBILL.Where(n => n.CONSIGN_ID == id && n.Type == "清空场存").OrderByDescending(n => n.ID).ToList();
            foreach (var items in list_TALLYBILL)
            {
                items.Type = "还原亏吨";
                db.Set<C_TB_HS_TALLYBILL>().AddOrUpdate(items);
            }
            try
            {
                db.SaveChanges();
                return Json(new AjaxResult { state = ResultType.success.ToString(), message = "成功" }.ToJson());
            }
            catch (Exception e)
            {
                return Json(new AjaxResult { state = ResultType.error.ToString(), message = e.Message }.ToJson());
            }
        }
        public ActionResult PrintChuMenZheng(int id)
        {
            C_TB_HC_GOODSBILL entry = db.C_TB_HC_GOODSBILL.Find(id) ?? new C_TB_HC_GOODSBILL();
            return View(entry);
        }
        public ActionResult PrintCangChuZuoYe(int id)
        {
            C_TB_HC_GOODSBILL entry = db.C_TB_HC_GOODSBILL.Find(id) ?? new C_TB_HC_GOODSBILL();
            return View(entry);
        }
        public ActionResult AddTallyBll_pj_part(C_TB_HS_TALLYBILL model)
        {
            OperatorModel loginModel = OperatorProvider.Provider.GetCurrent();
            C_TB_HC_CONSIGN model_consign = db.C_TB_HC_CONSIGN.Find(model.CONSIGN_ID) ?? new C_TB_HC_CONSIGN();
            C_TB_HC_GOODSBILL model_goodsbill = db.C_TB_HC_GOODSBILL.Find(model_consign.GOODSBILL_ID) ?? new C_TB_HC_GOODSBILL();
            List<C_TB_HC_CONSIGN> list_consign = db.C_TB_HC_CONSIGN.Where(n => n.GOODSBILL_ID == model_goodsbill.ID).OrderByDescending(n => n.ID).ToList();
            TallyBllController tl = new TallyBllController();
            List<SelectListItem> STORAGEList = tl.GetChang();
            ViewData["STORAGEList"] = new SelectList(STORAGEList, "Value", "Text");
            ViewBag.CONSIGN_ID = model.CONSIGN_ID;
            ViewBag.CGNO = model_consign.CGNO;
            ViewBag.RoleId = loginModel.RoleId;
            ViewBag.type = model.Type;
            ViewBag.CONSIGN_ID = model.CONSIGN_ID;
            ViewBag.WeiTuoBianHao = model_consign.WeiTuoBianHao;
            return View(model);
        }
        [OperationLog(OperationLogAttribute.Operatetype.编辑, OperationLogAttribute.ImportantLevel.一般操作, "审核理货单")]
        [HttpPost]
        public JsonResult shenheTALLYBILL(int id)
        {
            OperatorModel loginModel = OperatorProvider.Provider.GetCurrent();
            C_TB_HC_CONSIGN model_consign = db.C_TB_HC_CONSIGN.Find(id) ?? new C_TB_HC_CONSIGN();
            C_TB_HC_GOODSBILL model_goodsbill = db.C_TB_HC_GOODSBILL.Find(model_consign.GOODSBILL_ID) ?? new C_TB_HC_GOODSBILL();
            List<C_TB_HC_CONSIGN> list_Consign = db.C_TB_HC_CONSIGN.Where(n => n.GOODSBILL_ID == model_goodsbill.ID).OrderByDescending(n => n.ID).ToList();
           
            try
            {
                foreach (var items_consign in list_Consign)
                {
                    List<C_TB_HS_TALLYBILL> list_TallBill = db.C_TB_HS_TALLYBILL.Where(n => n.CONSIGN_ID == items_consign.ID).OrderByDescending(n => n.ID).ToList();
                    foreach (var items in list_TallBill)
                    {
                        items.State = "已完成";
                        items.Shr = loginModel.UserName;
                        db.Set<C_TB_HS_TALLYBILL>().AddOrUpdate(items);
                        db.SaveChanges();
                    }
                    items_consign.State = "已完成";
                    db.SaveChanges();
                }
                return Json(new AjaxResult { state = ResultType.success.ToString(), message = "成功" }.ToJson());
            }
            catch (Exception e)
            {
                return Json(new AjaxResult { state = ResultType.error.ToString(), message = e.Message }.ToJson());
            }
        }
        #region 放货指令管理
        public ActionResult FhzlIndex()
        {
            return View();
        }
        public object GetFhzl(int GoodsBillId)
        {
            var wherelambda = ExtLinq.True<BS_FANGHUOZHILING>();
            wherelambda = wherelambda.And(t => t.GoodsBillId == GoodsBillId);
            var list = db.Set<BS_FANGHUOZHILING>().Where(wherelambda).OrderByDescending(n => n.CreatTime).AsQueryable();
            int total = list.Count();
            object rows = list;
            return Json(new { total = total, rows = rows }, JsonRequestBehavior.AllowGet);

        }
        #endregion
        public ActionResult TiDanList(int GoodsBillId)
        {
            OperatorModel loginModel = OperatorProvider.Provider.GetCurrent();
            ViewBag.GoodsBillId = GoodsBillId;
            ViewBag.RoleId = loginModel.RoleId;
            return View();
        }

        [System.Web.Http.HttpGet]
        public object GetTiDanList(int limit, int offset, int GoodsBillId)
        {
            var wherelambda = ExtLinq.True<BS_BAOGUAN>();
            wherelambda = wherelambda.And(t => t.GoodsBillId == GoodsBillId);
            var list = db.Set<BS_BAOGUAN>().Where(wherelambda).OrderByDescending(n => n.CreatTime).AsQueryable();
            int total = list.Count();
            object rows = list.Skip(offset).Take(limit).AsQueryable();
            return Json(new { total = total, rows = rows }, JsonRequestBehavior.AllowGet);

        }
    }

}