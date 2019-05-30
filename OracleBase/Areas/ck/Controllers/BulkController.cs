using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using Main.HelpClass;
using MainBLL.Big;
using MainBLL.TallyBLL;
using Microsoft.AspNet.SignalR;
using NFine.Code;
using OracleBase.HelpClass;
using OracleBase.HelpClass.Sys;
using OracleBase.Models;

namespace OracleBase.Areas.ck.Controllers
{
    [SignLoginAuthorize]
    public class BulkController : Controller
    {
        string url = "http://5n3mps.natappfree.cc";
        private Entities db = new Entities();
        // GET: ck/Bulk
        public ActionResult SureWarehouseIndex()
        {
            return View();
        }
        [System.Web.Http.HttpGet]
        public object GetSureList(int limit, int offset,string HUODAI,string GoodsName)
        {
            OperatorModel loginModel = OperatorProvider.Provider.GetCurrent();
            var yuanquid = loginModel.YuanquID ?? throw new ArgumentNullException("loginModel.YuanquID");
            var wherelambda = ExtLinq.True<B_TB_Sure>();
            wherelambda = wherelambda.And(t => t.YuanQuID == yuanquid&& t.State == "未处理");
        
            if (!string.IsNullOrWhiteSpace(HUODAI))
            {
                wherelambda = wherelambda.And(t => t.HUODAI.Contains(HUODAI));
            }
            if (!string.IsNullOrWhiteSpace(GoodsName))
            {
                wherelambda = wherelambda.And(t => t.GoodsName.Contains(GoodsName));
            }

            var list = db.Set<B_TB_Sure>().Where(wherelambda).OrderByDescending(n => n.CreatTime).AsQueryable();
            int total = list.Count();
            object rows = list.Skip(offset).Take(limit).AsQueryable();
            return Json(new { total = total, rows = rows }, JsonRequestBehavior.AllowGet);

        }
        public ActionResult SureWarehouseIndex2()
        {
            return View();
        }
        [System.Web.Http.HttpGet]
        public object GetSureList2(int limit, int offset, string HUODAI, string GoodsName)
        {
            OperatorModel loginModel = OperatorProvider.Provider.GetCurrent();
            var yuanquid = loginModel.YuanquID ?? throw new ArgumentNullException("loginModel.YuanquID");

            var wherelambda = ExtLinq.True<B_TB_Sure>();
            wherelambda = wherelambda.And(t => t.YuanQuID == yuanquid);
            wherelambda = wherelambda.And(t => t.State == "货代确认有货");
            if (!string.IsNullOrWhiteSpace(HUODAI))
            {
                wherelambda = wherelambda.And(t => t.HUODAI.Contains(HUODAI));
            }
            if (!string.IsNullOrWhiteSpace(GoodsName))
            {
                wherelambda = wherelambda.And(t => t.GoodsName.Contains(GoodsName));
            }
            var list = db.Set<B_TB_Sure>().Where(wherelambda).OrderByDescending(n => n.CreatTime).AsQueryable();
            int total = list.Count();
            object rows = list.Skip(offset).Take(limit).AsQueryable();
            return Json(new { total = total, rows = rows }, JsonRequestBehavior.AllowGet);

        }
        /// <summary>
        /// 历史仓单确认
        /// </summary>
        /// <returns></returns>
        public ActionResult SureWarehouseIndex3()
        {
            return View();
        }
        [System.Web.Http.HttpGet]
        public object GetSureList3(int limit, int offset, string HUODAI, string GoodsName)
        {
            OperatorModel loginModel = OperatorProvider.Provider.GetCurrent();
            var yuanquid = loginModel.YuanquID ?? throw new ArgumentNullException("loginModel.YuanquID");

            var wherelambda = ExtLinq.True<B_TB_Sure>();
            wherelambda = wherelambda.And(t => t.YuanQuID == yuanquid);
            wherelambda = wherelambda.And(t => t.State != "货代确认有货"&& t.State != "未处理");
            if (!string.IsNullOrWhiteSpace(HUODAI))
            {
                wherelambda = wherelambda.And(t => t.HUODAI.Contains(HUODAI));
            }
            if (!string.IsNullOrWhiteSpace(GoodsName))
            {
                wherelambda = wherelambda.And(t => t.GoodsName.Contains(GoodsName));
            }
            var list = db.Set<B_TB_Sure>().Where(wherelambda).OrderByDescending(n => n.CreatTime).AsQueryable();
            int total = list.Count();
            object rows = list.Skip(offset).Take(limit).AsQueryable();
            return Json(new { total = total, rows = rows }, JsonRequestBehavior.AllowGet);

        }
        /// <summary>
        /// 仓单审核
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        [OperationLog(OperationLogAttribute.Operatetype.编辑, OperationLogAttribute.ImportantLevel.危险操作, "审核仓单")]
        [HttpPost]
        public ActionResult ShSure(string guid,string state)
        {

            using (var scope = new TransactionScope(TransactionScopeOption.Required))
            {
                B_TB_Sure model = new B_TB_Sure() { GUID = guid };
                model.State = state;
                DbEntityEntry<B_TB_Sure> entry = db.Entry<B_TB_Sure>(model);
                entry.State = EntityState.Unchanged;
                entry.Property(t => t.State).IsModified = true; //设置要更新的属性

                int c = db.SaveChanges();
                if (c > 0)
                {
                    bool b=false;
                    if (state == "仓库确认有货")
                    {
                        b= ckGoodsSure(guid, 1);
                    }
                    else if (state == "仓库确认无货")
                    {
                        b= ckGoodsSure(guid, 0);
                    }

                    if (b)
                    {
                        scope.Complete();
                    }
                  
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

        }

        /// <summary>
        /// 确认信息推送给大宗平台
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        private bool ckGoodsSure(string guid,int status)
        {
            string url = "http://uq4kds.natappfree.cc/explatform/exp/thrid/ckInterface/ckGoodsSure.do";
          //  string guid = "483cc9dc-dcb6-4f41-a83b-36ef71bc03cd";
            url += string.Format("?guid='{0}'&&status={1}", guid, status);
            var r = HttpMethods.HttpPost(url);
            BigResult result = JsonHelper.JsonToObject<BigResult>(r);
            if (result.code==0)
            {
                return true;
            }
            return false;
        }

        public ActionResult SureInfoPage(string GUID)
        {
            B_TB_Sure entity = db.B_TB_Sure.Find(GUID);
            return View(entity);
        }


        #region 消息推送
        /// <summary>
        /// 测试实时通信
        /// </summary>
        /// <returns></returns>
        public ActionResult SureWarehouseList()
        {
            ViewBag.fatherid = 123123;
            OperatorModel loginModel = OperatorProvider.Provider.GetCurrent();
            ViewBag.username = loginModel.UserName;
            ViewBag.userid = loginModel.UserId;
            return View();
        }
        public ActionResult SetMessage()
        {
            ViewBag.fatherid = 123123;
            OperatorModel loginModel = OperatorProvider.Provider.GetCurrent();
            ViewBag.username = loginModel.UserName;
            ViewBag.userid = loginModel.UserId;
            return View();
        }
        /// <summary>
        /// 
        /// </summary>
        [HttpPost]
        public void PostSetMessage()
        {
            OperatorModel loginModel = OperatorProvider.Provider.GetCurrent();
            ViewBag.username = loginModel.UserName;
            ViewBag.userid = loginModel.UserId;
            var hub = GlobalHost.ConnectionManager.GetHubContext<MyHub1>();
            hub.Clients.All.receiveMessage(loginModel.UserName, "msg", loginModel.UserId, 22);
        }
        #endregion

        #region 锁货

        public ActionResult SuoHuo()
        {     
            return View();
        }
        [HttpPost]
        public ActionResult SuoHuo(C_TB_HC_GOODSBILL model)
        {
            try
            {
                double.TryParse(model.KunCun, out var kunCun);
                double.TryParse(model.KunCunW, out var kunCunW);
                double suoHuoKunCun = (double)model.SuoHuoKunCun;
                double suoHuoKunCunW = (double)model.SuoHuoKunCunW;
                if (suoHuoKunCun > kunCun)
                {
                    return Json(new AjaxResult { state = ResultType.error.ToString(), message = "失败！锁定库存件数大于剩余" });
                }
                if (suoHuoKunCunW > kunCunW)
                { 
                    return Json(new AjaxResult { state = ResultType.error.ToString(), message = "失败！锁定库存吨数/材积大于剩余" });
                }

                C_TB_HC_GOODSBILL m = db.C_TB_HC_GOODSBILL.FirstOrDefault(n => n.GBNO == model.GBNO);
                if (m != null)
                {
                    m.SuoHuoKunCun = model.SuoHuoKunCun;
                    m.SuoHuoKunCunW = model.SuoHuoKunCunW;
                }

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
            catch (Exception e)
            {
                return Json(new AjaxResult { state = ResultType.error.ToString(), message = e.Message });
               
            }

        }
        /// <summary>
        /// 根据委托号，获取委托的信息
        /// </summary>
        /// <param name="GBNO">委托号</param>
        /// <returns></returns>
        [HttpGet]
        public JsonResult GetInfoByWtHao(string GBNO)
        {
            try
            {
                C_TB_HC_GOODSBILL goodsbill = db.C_TB_HC_GOODSBILL.FirstOrDefault(n=>n.GBNO== GBNO);
               
                    decimal? KuCun = 0;
                    decimal? KuCunW = 0;
                    List<C_TB_HC_CONSIGN> listConsign = db.C_TB_HC_CONSIGN.Where(n => n.GOODSBILL_ID == goodsbill.ID).OrderBy(n => n.ID).ToList();
                    foreach (var itemsConsign in listConsign)
                    {
                        List<C_TB_HS_TALLYBILL> listTallybill = db.C_TB_HS_TALLYBILL.Where(n => n.CONSIGN_ID == itemsConsign.ID).OrderBy(n => n.ID).ToList();
                        foreach (var itemsTallybill in listTallybill)
                        {
                            if (itemsTallybill.CODE_OPSTYPE == "进库")
                            {
                                KuCun += itemsTallybill.AMOUNT;
                                KuCunW += itemsTallybill.WEIGHT;
                            }
                            if (itemsTallybill.CODE_OPSTYPE == "出库")
                            {
                                KuCun -= itemsTallybill.AMOUNT;
                                KuCunW -= itemsTallybill.WEIGHT;
                            }
                        }
                    }

               
                    goodsbill.KunCun = KuCun.ToString();
                    goodsbill.KunCunW = KuCunW.ToString();


                    return Json(goodsbill, JsonRequestBehavior.AllowGet);
                
            }
            catch (Exception e)
            {
                return Json(new AjaxResult { state = ResultType.error.ToString(), message = e.Message }.ToJson());
            }
        }
        #endregion

        #region 货权转移
        public ActionResult ZhuanYiIndex()
        {
            return View();
        }
        [System.Web.Http.HttpGet]
        public object GetZhuanYiList(int limit, int offset)
        {
            OperatorModel loginModel = OperatorProvider.Provider.GetCurrent();
            var yuanquid = loginModel.YuanquID ?? throw new ArgumentNullException("loginModel.YuanquID");

            var wherelambda = ExtLinq.True<B_TB_ZhuanYiInfo>();
            wherelambda = wherelambda.And(t => t.YuanQuID == yuanquid);
            wherelambda = wherelambda.And(t => t.State == "未处理"|| t.State == "已处理");

            var list = db.Set<B_TB_ZhuanYiInfo>().Where(wherelambda).OrderByDescending(n => n.CreatTime).AsQueryable();
            int total = list.Count();
            object rows = list.Skip(offset).Take(limit).AsQueryable();
            return Json(new { total = total, rows = rows }, JsonRequestBehavior.AllowGet);

        }
        [HttpPost]
        //行内编辑使用
        public JsonResult EditZhuanYi(B_TB_ZhuanYiInfo model)
        {
            if (model.State== "已处理")
            {
                return Json("该状态下不能进行编辑");
            }
            model.CreatTime = DateTime.Now;
            db.B_TB_ZhuanYiInfo.AddOrUpdate(model);
            db.SaveChanges();
            return Json("成功");
        }

        public ActionResult ZhuanYiInfoPage(string guid)
        {
            B_TB_ZhuanYiInfo entry = db.B_TB_ZhuanYiInfo.Find(guid);
            return View(entry);
        }
        public JsonResult AddZhuanYi(string guid)
        {

            /*
             * 1判断票货编码是否存在
             * 2把原来的票货添加一个出库的委托，在出库的委托下面添加一个出库的理货单
             * 3添加一个新的进库票货，在该票货下添加一个新的进库委托，在委托下添加一个新的进库理货单
             */
           
            B_TB_ZhuanYiInfo infoModel = db.B_TB_ZhuanYiInfo.Find(guid);
            string goodsBillId = infoModel.GoodsBillID;//票货编码
            C_TB_HC_GOODSBILL goodsBillmodel = db.C_TB_HC_GOODSBILL.FirstOrDefault(n=>n.GBNO==goodsBillId);
          
            if (goodsBillmodel == null)
            {
                return Json(new AjaxResult { state = ResultType.error.ToString(), message = "不存在对应票货信息！"});
            }
            decimal? KuCun = 0;
            decimal? KuCunW = 0;
            decimal? KuCUnX = 0;
            List<C_TB_HC_CONSIGN> list_CONSIGN = db.C_TB_HC_CONSIGN.Where(n => n.GOODSBILL_ID == goodsBillmodel.ID).OrderBy(n => n.ID).ToList();
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
          
           // decimal? jianshu = 0;//件数
            decimal? zhongliang = 0;//重量
            //decimal? xiangshu = 0;//箱数
            //if (!string.IsNullOrEmpty(infoModel.KunCun.ToString()))
            //{
            //    jianshu = infoModel.KunCun;
            //}
            if (!string.IsNullOrEmpty(infoModel.KunCunW.ToString()))
            {
                zhongliang = infoModel.ShuLiang;
            }
            //if (!string.IsNullOrEmpty(infoModel.XiangShu.ToString()))
            //{
            //    xiangshu = infoModel.XiangShu;
            //}
            //if (KuCun< jianshu)
            //{
            //    return Json(new AjaxResult { state = ResultType.error.ToString(), message ="库存件数不足" });
            //}
            if (KuCunW < zhongliang)
            {
                return Json(new AjaxResult { state = ResultType.error.ToString(), message = "库存重量不足" });
            }
            //if (KuCUnX < xiangshu)
            //{
            //    return Json(new AjaxResult { state = ResultType.error.ToString(), message = "库存箱数不足" });
            //}
            try
            {
                using (var scope = new TransactionScope(TransactionScopeOption.Required))
                {

                    //if (goodsBillmodel.SuoHuoKunCun - jianshu < 0)
                    //{
                    //    goodsBillmodel.SuoHuoKunCun = 0;
                    //}
                    //else
                    //{
                    //    goodsBillmodel.SuoHuoKunCun = goodsBillmodel.SuoHuoKunCun - jianshu;
                    //}
                    if (goodsBillmodel.SuoHuoKunCunW - zhongliang < 0)
                    {
                        goodsBillmodel.SuoHuoKunCunW = 0;//减掉锁货量
                    }
                    else
                    {
                        goodsBillmodel.SuoHuoKunCunW = goodsBillmodel.SuoHuoKunCunW - zhongliang;//减掉锁货量
                    }
               
                db.C_TB_HC_GOODSBILL.AddOrUpdate(goodsBillmodel);
                db.SaveChanges();
                C_TB_HC_CONSIGN model_Consign = new C_TB_HC_CONSIGN()//为原来票货添加委托
                {
                    GOODSBILL_ID = goodsBillmodel.ID,//票货Id
                    BLNO = goodsBillmodel.BLNO,//提单号
                    WeiTuoRen = goodsBillmodel.C_GOODSAGENT_NAME,//委托人
                    GoodsName = goodsBillmodel.C_GOODS,//货物
                    ShipName = goodsBillmodel.ShipName,//船名
                    VGNO = goodsBillmodel.VGNO,//航次
                    WeiTuoTime = DateTime.Now,//委托时间
                    PAPERYNO = "无",//纸质委托号
                    CODE_OPERATION = "场-汽",//作业过程
                    HengZhong = "否",//衡重
                    GoodsType = goodsBillmodel.GoodsType,//货物类型
                    BeiZhu = "货权转移自动生成",
                    State = "已完成",//状态
                    YuanQuID = infoModel.YuanQuID//园区id

                };
                Add_C_CONSIGN(model_Consign, out decimal ConId);
               C_TB_HC_CONSIGN get_Con = db.C_TB_HC_CONSIGN.FirstOrDefault(n => n.ID == ConId);
                List<C_TB_HC_CONSIGN> get_Con_Goods = db.C_TB_HC_CONSIGN.Where(n => n.GOODSBILL_ID == goodsBillmodel.ID).OrderBy(n => n.ID).ToList();
                List<C_TB_HS_TALLYBILL> TALLYBILL = new List<C_TB_HS_TALLYBILL>();
                foreach (var items_Con in get_Con_Goods)
                {
                C_TB_HS_TALLYBILL get_TallBill_add = db.C_TB_HS_TALLYBILL.FirstOrDefault(n => n.CONSIGN_ID == items_Con.ID);
                    if (get_TallBill_add != null)
                    {
                        TALLYBILL.Add(get_TallBill_add);
                    }

                }
                C_TB_HS_TALLYBILL get_TallBill = TALLYBILL.FirstOrDefault();
                C_TB_HS_TALLYBILL model_TallBill = new C_TB_HS_TALLYBILL()//为原来票货添加出库理货单
                {
                    CONSIGN_ID = ConId,//委托id
                    CGNO = get_Con.CGNO,//委托号
                    CODE_SECTION = get_TallBill.CODE_SECTION,//位
                    SIGNDATE = DateTime.Now,//理货日期
                    CODE_OPSTYPE = "出库",//进出库
                   // AMOUNT = jianshu,//件数
                    WEIGHT = zhongliang,//重量
                    CODE_QUALITY = get_TallBill.CODE_QUALITY,//箱数
                    CODE_WORKTEAM = get_TallBill.CODE_WORKTEAM,//苫盖
                    TALLYMAN = "系统",//创建人
                    State = "已完成",//状态
                    CAOZUO = "场-汽",//作业过程
                    STORAG = get_TallBill.STORAG,//场
                    Type = "清场",//类型
                    GoodsName = get_TallBill.GoodsName,//货物名称
                    YuanQuID = get_TallBill.YuanQuID,//园区id
                    ChuanMing = get_TallBill.ChuanMing,//船名
                    HangCi = get_TallBill.HangCi,//航次
                    HuoDai = get_TallBill.HuoDai,//货代
                    BanCi = get_TallBill.BanCi,//班次
                  //  XIANGSHU = xiangshu,//箱数

                };
                AddTallyBll(model_TallBill);
                C_TB_HC_GOODSBILL model_GoodsBill = new C_TB_HC_GOODSBILL()//新增一个票货
                {
                    NWM = infoModel.NWM,
                    C_GOODSAGENT_ID = infoModel.HUODAIID,
                    C_GOODSAGENT_NAME = infoModel.HUODAI,
                    PIECEWEIGHT = infoModel.PIECEWEIGHT,
                    VGNO = infoModel.VGNO,
                    BLNO = infoModel.BLNO,
                    MARK_GOOGSBILLTYPE = infoModel.MARK_GOOGSBILLTYPE,
                    MARK = infoModel.MARK,
                    HETONGHAO = infoModel.HETONGHAO,
                    C_GOODS = infoModel.GoodsName,
                    YuanQuID = infoModel.YuanQuID,
                    ShipName = infoModel.ShipName,
                    GoodsType = infoModel.GoodsType,
                    CreatPeople= goodsBillmodel.CreatPeople,
                    SysUserID= goodsBillmodel.SysUserID,
                    //KunCun = infoModel.KunCun.ToString(),
                    //KunCunW = infoModel.KunCunW.ToString(),
                    HuoZhu = infoModel.HuoZhu,

                };
                Add_C_GOODSBILL(model_GoodsBill, model_GoodsBill.YuanQuID, out decimal GoodsId);



                C_TB_HC_CONSIGN model_Consign_xz = new C_TB_HC_CONSIGN()//为新增票货添加委托
                {
                    GOODSBILL_ID = GoodsId,//票货Id
                    BLNO = model_GoodsBill.BLNO,//提单号
                    WeiTuoRen = model_GoodsBill.C_GOODSAGENT_NAME,//委托人
                    GoodsName = model_GoodsBill.C_GOODS,//货物
                    ShipName = model_GoodsBill.ShipName,//船名
                    VGNO = model_GoodsBill.VGNO,//航次
                    WeiTuoTime = DateTime.Now,//委托时间
                    PAPERYNO = "无",//纸质委托号
                    CODE_OPERATION = "场-汽",//作业过程
                    HengZhong = "否",//衡重
                    GoodsType = model_GoodsBill.GoodsType,//货物类型
                    BeiZhu = "货权转移自动生成",
                    State = "已完成",//状态
                    YuanQuID = model_GoodsBill.YuanQuID//园区id

                };
                Add_C_CONSIGN(model_Consign_xz, out decimal ConId_xz);
                C_TB_HC_CONSIGN get_Con_xz = db.C_TB_HC_CONSIGN.FirstOrDefault(n => n.ID == ConId_xz);
                C_TB_HS_TALLYBILL model_TallBill_xz = new C_TB_HS_TALLYBILL()//为新增票货添加进库理货单
                {
                    CONSIGN_ID = ConId_xz,//委托id
                    CGNO = get_Con_xz.CGNO,//委托号
                    CODE_SECTION = get_TallBill.CODE_SECTION,//位
                    SIGNDATE = DateTime.Now,//理货日期
                    CODE_OPSTYPE = "进库",//进出库
                   // AMOUNT = jianshu,//件数
                    WEIGHT = zhongliang,//重量
                    CODE_QUALITY = get_TallBill.CODE_QUALITY,//箱数
                    CODE_WORKTEAM = get_TallBill.CODE_WORKTEAM,//苫盖
                    TALLYMAN = "系统",//创建人
                    State = "已完成",//状态
                    CAOZUO = "场-汽",//作业过程
                    STORAG = get_TallBill.STORAG,//场
                    Type = "清场",//类型
                    GoodsName = get_TallBill.GoodsName,//货物名称
                    YuanQuID = get_TallBill.YuanQuID,//园区id
                    ChuanMing = get_TallBill.ChuanMing,//船名
                    HangCi = get_TallBill.HangCi,//航次
                    HuoDai = get_TallBill.HuoDai,//货代
                    BanCi = get_TallBill.BanCi,//班次
                   // XIANGSHU = xiangshu,//箱数

                };
                AddTallyBll(model_TallBill_xz);
                infoModel.State = "已处理";
                db.Set<B_TB_ZhuanYiInfo>().AddOrUpdate(infoModel);
                db.SaveChanges();
                    scope.Complete();
                }
                return Json(new AjaxResult { state = ResultType.success.ToString(), message = "成功！" });
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            {
                var msg = string.Empty;
                var errors = (from u in ex.EntityValidationErrors select u.ValidationErrors).ToList();
                foreach (var item in errors)
                    msg += item.FirstOrDefault().ErrorMessage;
                return Json(new AjaxResult { state = ResultType.error.ToString(), message = ex.Message });

            }

        }

        [OperationLog(OperationLogAttribute.Operatetype.添加, OperationLogAttribute.ImportantLevel.一般操作, "大宗添加票货")]
        [HttpPost]
        public void Add_C_GOODSBILL(C_TB_HC_GOODSBILL model,decimal? yuanquid,out decimal GoodsId)
        {
            GoodsId = 0;
            if (string.IsNullOrEmpty(model.GBNO))         
            {
                string todayTime = DateTime.Today.ToString("yyyyMMdd");
                C_TB_HC_GOODSBILL Num = db.C_TB_HC_GOODSBILL.OrderByDescending(n => n.CreatTime).ToList().FirstOrDefault();
                if (Num != null)
                {
                    if (!string.IsNullOrEmpty(Num.GBNO))
                    {
                        if (Num.GBNO.Substring(2, 8) == todayTime)
                        {
                            model.GBNO = "PH" + todayTime + (Convert.ToInt32(Num.GBNO.Replace("PH" + todayTime, "")) + 1).ToString("0000");
                        }
                        else
                        {
                            model.GBNO = "PH" + todayTime + "0001";
                        }
                    }
                }

                else
                {
                    model.GBNO = "PH" + todayTime + "0001";
                }

            }
            model.YuanQuID = yuanquid;
            model.C_GOODSAGENT_NAME = db.C_GOODSAGENT.FirstOrDefault(n => n.ID == model.C_GOODSAGENT_ID)?.Name.ToString();
                model.CreatPeople = "系统";
                model.CreatTime = DateTime.Now;
                model.State = "进行中";
                model.KunCun = "0";
                model.KunCunW = "0";
            if (!string.IsNullOrEmpty(model.ContoractNumber))
            {
                C_TB_HC_CONTRACT modelHt = db.C_TB_HC_CONTRACT.FirstOrDefault(n => n.ContoractNumber == model.ContoractNumber);
                if (modelHt == null)
                {
                }
                else
                {
                    model.CONTRACT_Guid = modelHt.Guid;
                }
            }
            db.Set<C_TB_HC_GOODSBILL>().AddOrUpdate(model);
            try
            {


                db.SaveChanges();
                GoodsId = model.ID;
            }
            catch (Exception ex)
            {
            }

        }
        public void Add_C_CONSIGN(C_TB_HC_CONSIGN model,out decimal ConId)
        {
            ConId = 0;
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
                model.CREATORNAME = "系统";
                model.CREATETIME = DateTime.Now;

            db.Set<C_TB_HC_CONSIGN>().AddOrUpdate(model);

            try
            {

                db.SaveChanges();
                ConId = model.ID;
               

            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            {
                var msg = string.Empty;
                var errors = (from u in ex.EntityValidationErrors select u.ValidationErrors).ToList();
                foreach (var item in errors)
                    msg += item.FirstOrDefault().ErrorMessage;
            }


        }
        public void AddTallyBll(C_TB_HS_TALLYBILL model)
        {
            OperatorModel loginModel = OperatorProvider.Provider.GetCurrent();
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
            C_TB_HC_CONSIGN con = db.C_TB_HC_CONSIGN.FirstOrDefault(n => n.CGNO == cgno);
            if (con != null) model.CONSIGN_ID = con.ID;
           
            try
            {

                db.C_TB_HS_TALLYBILL.AddOrUpdate(model);
                db.SaveChanges();
                bool b = EidSTOCKDORMANT(model, AorU);
                C_TB_HC_CONSIGN model_CONSIGN = db.C_TB_HC_CONSIGN.Find(model.CONSIGN_ID) ?? new C_TB_HC_CONSIGN();

                if (b)
                {
            
                }
                else
                {
                    db.C_TB_HS_TALLYBILL.Remove(model);
                    db.SaveChanges();
                   
                }
            }

            catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            {
                var msg = string.Empty;
                var errors = (from u in ex.EntityValidationErrors select u.ValidationErrors).ToList();
                foreach (var item in errors)
                    msg += item.FirstOrDefault().ErrorMessage;
            }



        }
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
        #endregion

        #region 出库申请

        public ActionResult CkSqIndex()
        {
            return View();
        }
        [System.Web.Http.HttpGet]
        public object GetCkSqList(int limit, int offset)
        {
            OperatorModel loginModel = OperatorProvider.Provider.GetCurrent();
            var yuanquid = loginModel.YuanquID ?? throw new ArgumentNullException("loginModel.YuanquID");

            var wherelambda = ExtLinq.True<B_TB_ChuKuShenQing>();
            wherelambda = wherelambda.And(t => t.YuanQuID == yuanquid);
            //wherelambda = wherelambda.And(t => t.State == "未处理");

            var list = db.Set<B_TB_ChuKuShenQing>().Where(wherelambda).OrderByDescending(n => n.CreatTime).AsQueryable();
            int total = list.Count();
            object rows = list.Skip(offset).Take(limit).AsQueryable();
            return Json(new { total = total, rows = rows }, JsonRequestBehavior.AllowGet);

        }

        /// <summary>
        /// 出库完成接口
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="sl2ok"></param>
        /// <param name="sl2"></param>
        /// <returns></returns>
        [OperationLog(OperationLogAttribute.Operatetype.添加, OperationLogAttribute.ImportantLevel.一般操作, "完成出库")]
        [HttpPost]
        public ActionResult DoneCk(string guid, decimal sl2ok,decimal sl2)
        {
            try
            {
                url += string.Format("/explatform/exp/thrid/ckInterface/ckDeliver.do?guid='{0}'&&sl2ok={1}&&sl2={2}&&status=1", guid, sl2ok, sl2);
                var r = HttpMethods.HttpPost(url);
                BigResult result = JsonHelper.JsonToObject<BigResult>(r);
                if (result.code == 0)
                {
                    B_TB_ChuKuShenQing ckmodel = db.B_TB_ChuKuShenQing.Find(guid);
                    if (ckmodel != null) ckmodel.State = "已完成";
                    db.SaveChanges();
                    return Json(new AjaxResult { state = ResultType.success.ToString(), message = "成功！" });
                }
                return Json(new AjaxResult { state = ResultType.error.ToString(), message = "失败！" });
            }
            catch (Exception e)
            {
                return Json(new AjaxResult { state = ResultType.error.ToString(), message =e.Message });
              
            }
        
        }
        public ActionResult CkSqInfoPage(string Guid)
        {
            B_TB_ChuKuShenQing empty = db.B_TB_ChuKuShenQing.Find(Guid);
            return View(empty);
        }
        #endregion
    }
}