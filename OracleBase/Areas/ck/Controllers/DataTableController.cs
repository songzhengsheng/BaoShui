using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;
using Main.HelpClass;
using MainBLL.Money;
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
    public class DataTableController : Controller
    {
        private Entities db = new Entities();
        // GET: ck/DataTable
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult WeiKaiPiaoList()
        {
            OperatorModel loginModel = OperatorProvider.Provider.GetCurrent();
            ViewBag.roleId = loginModel.RoleId;
            return View();
        }
        public object GetWeiKaiPiaoList(int limit, int offset, string GBNO, string C_GOODSAGENT_NAME, string CreatTime, string CreatTime1, string BLNO, string C_GOODS, string HuoZhu, string ShipName, string VGNO)
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
            List<Stock_Money_WeiKaiPiao> list_WeiKaiPiao = new List<Stock_Money_WeiKaiPiao>();
            foreach (var itsms_GoodsBill in list)//遍历票货
            {
                string Guid_Get = "";
                string QiaTaFeiYong = "";
                C_TB_HC_CONTRACT Model_CONTRACT = db.C_TB_HC_CONTRACT.FirstOrDefault(n => n.Guid == itsms_GoodsBill.CONTRACT_Guid);//查找合同
                if (Model_CONTRACT != null)
                {
                    Guid_Get = Model_CONTRACT.Guid;
                }
                List<Stock_Money> list_GetList = Get_FeiMuXX(Convert.ToInt32(itsms_GoodsBill.ID), Guid_Get);
                foreach (var items_GetList in list_GetList)
                {
                    if (items_GetList.FMZhongLei != "包干费" && items_GetList.FMZhongLei != "堆存费" && items_GetList.FMZhongLei != "运输费" && items_GetList.FMZhongLei != "过磅费")
                    {
                        QiaTaFeiYong += "(" + items_GetList.FMZhongLei.ToString() + ":" + items_GetList.WeiKaiPiaoJinE + ")";
                    }
                }


                Stock_Money_WeiKaiPiao model = new Stock_Money_WeiKaiPiao()
                {
                    DanWei = itsms_GoodsBill.C_GOODSAGENT_NAME,
                    HuoZhong = itsms_GoodsBill.C_GOODS,
                    ChuanMing = itsms_GoodsBill.ShipName,
                    BaoGanFei = list_GetList.Where(n => n.FMZhongLei == "包干费").Sum(n => n.WeiKaiPiaoShuiHou).ToDecimal(2),
                    DuiCunFei = list_GetList.Where(n => n.FMZhongLei == "堆存费").Sum(n => n.WeiKaiPiaoShuiHou).ToDecimal(2),
                    YunFei = list_GetList.Where(n => n.FMZhongLei == "运输费").Sum(n => n.WeiKaiPiaoShuiHou.ToDecimal(2)),
                    GuoBangFei = list_GetList.Where(n => n.FMZhongLei == "过磅费").Sum(n => n.WeiKaiPiaoShuiHou).ToDecimal(2),
                    QiaTaFeiYong = list_GetList.Where(n => n.FMZhongLei != "包干费" && n.FMZhongLei != "堆存费" && n.FMZhongLei != "运输费" && n.FMZhongLei != "过磅费").Sum(n => n.WeiKaiPiaoShuiHou).ToDecimal(2) + QiaTaFeiYong,
                    HeJi = list_GetList.Sum(n => n.WeiKaiPiaoShuiHou).ToDecimal(2),
                    HangCi= itsms_GoodsBill.VGNO,
                    TiDanHao= itsms_GoodsBill.BLNO,
                    QiTa = list_GetList.Where(n => n.FMZhongLei != "包干费" && n.FMZhongLei != "堆存费" && n.FMZhongLei != "运输费" && n.FMZhongLei != "过磅费").Sum(n => n.WeiKaiPiaoShuiHou).ToDecimal(2),

                };

                list_WeiKaiPiao.Add(model);
            }
            int total = count;
            object rows = list_WeiKaiPiao;
            return Json(new { total = total, rows = rows }, JsonRequestBehavior.AllowGet);

        }
        public ActionResult ShouRuList()
        {
            OperatorModel loginModel = OperatorProvider.Provider.GetCurrent();
            ViewBag.roleId = loginModel.RoleId;
            return View();
        }
        public object GetShouRuList(int limit, int offset, string GBNO, string C_GOODSAGENT_NAME, string CreatTime, string CreatTime1, string BLNO, string C_GOODS, string HuoZhu, string ShipName, string VGNO)
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
            List<Stock_Money_ShouRuHuiZOngBiao> list_ShouRuHuiZOngBiao = new List<Stock_Money_ShouRuHuiZOngBiao>();
            foreach (var itsms_GoodsBill in list)//遍历票货
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
                        decimal? XIANGSHU = 0;
                        if (!string.IsNullOrEmpty(items_TALLYBILL.XIANGSHU.ToString()))
                        {
                            XIANGSHU = 0;
                        }
                        if (items_TALLYBILL.CODE_OPSTYPE == "进库")
                        {
                            KuCun += (items_TALLYBILL.AMOUNT + XIANGSHU);
                            KuCunW += items_TALLYBILL.WEIGHT;
                        }
                        if (items_TALLYBILL.CODE_OPSTYPE == "出库")
                        {
                            KuCun -= (items_TALLYBILL.AMOUNT + XIANGSHU);
                            KuCunW -= items_TALLYBILL.WEIGHT;
                        }
                    }

                }
                decimal? ShiJiJInKu = 0;
                decimal? ShiJiChuKu = 0;
                List<C_TB_HC_CONSIGN> list_KuCunCONSIGN = db.C_TB_HC_CONSIGN.Where(n => n.GOODSBILL_ID == itsms_GoodsBill.ID).OrderBy(n => n.ID).ToList();
                foreach (var items_KuCunCONSIGN in list_KuCunCONSIGN)
                {
                    List<C_TB_HS_TALLYBILL> list_KuCunTALLYBILL = db.C_TB_HS_TALLYBILL.Where(n => n.CONSIGN_ID == items_KuCunCONSIGN.ID).OrderBy(n => n.ID).ToList();
                    ShiJiJInKu += list_KuCunTALLYBILL.Where(n => n.CODE_OPSTYPE == "进库" && n.Type != "清场").Sum(n => n.WEIGHT);
                    ShiJiChuKu += list_KuCunTALLYBILL.Where(n => n.CODE_OPSTYPE == "出库" && n.Type != "清场").Sum(n => n.WEIGHT);
                }
                string Guid_Get = "";
                string QiaTaFeiYong = "";
                if (Model_CONTRACT != null)
                {
                    Guid_Get = Model_CONTRACT.Guid;
                }
                List<Stock_Money> list_GetList = Get_FeiMuXX(Convert.ToInt32(itsms_GoodsBill.ID), Guid_Get);
                foreach (var items_GetList in list_GetList)
                {
                    if (items_GetList.FMZhongLei != "监管费" && items_GetList.FMZhongLei != "包干费" && items_GetList.FMZhongLei != "堆存费" && items_GetList.FMZhongLei != "运输费" && items_GetList.FMZhongLei != "过磅费")
                    {
                        QiaTaFeiYong += "(" + items_GetList.FMZhongLei.ToString() + ":" + items_GetList.WeiKaiPiaoJinE + ")";
                    }
                }


                Stock_Money_ShouRuHuiZOngBiao model = new Stock_Money_ShouRuHuiZOngBiao()
                {
                    HuoDai = itsms_GoodsBill.C_GOODSAGENT_NAME,
                    HuoWu = itsms_GoodsBill.C_GOODS,
                    ChuanMing = itsms_GoodsBill.ShipName,
                    JianPiaoRiQi = itsms_GoodsBill.CreatTime,
                    ShiJiJinKuLiang = ShiJiJInKu,
                    ShiJiChuKuLiang = ShiJiChuKu,
                    KuCunN = KuCun,
                    KuCunW = KuCunW,
                    JianGuanFei = list_GetList.Where(n => n.FMZhongLei == "监管费").Sum(n => n.FeiYong).ToDecimal(2),
                    BaoGanFei = list_GetList.Where(n => n.FMZhongLei == "包干费").Sum(n => n.FeiYong).ToDecimal(2),
                    DuiCunFei = list_GetList.Where(n => n.FMZhongLei == "堆存费").Sum(n => n.FeiYong).ToDecimal(2),
                    GuoBangFei = list_GetList.Where(n => n.FMZhongLei == "过磅费").Sum(n => n.FeiYong).ToDecimal(2),
                    YunFei = list_GetList.Where(n => n.FMZhongLei == "运输费").Sum(n => n.FeiYong).ToDecimal(2),
                    QiYaFeiYong = list_GetList.Where(n => n.FMZhongLei != "监管费" && n.FMZhongLei != "包干费" && n.FMZhongLei != "堆存费" && n.FMZhongLei != "运输费" && n.FMZhongLei != "过磅费").Sum(n => n.WeiKaiPiaoJinE).ToDecimal(2) + QiaTaFeiYong,
                    FeiYongHeJi = list_GetList.Sum(n => n.FeiYong).ToDecimal(2),
                    YearKaiPiaoShu = list_GetList.Sum(n => n.YearKaiPiaoJinE).ToDecimal(2),
                    LeiJiKaiPiaoShu = list_GetList.Sum(n => n.KaiPiaoJinE).ToDecimal(2),
                    LeiJiWeiKaiPiaoShu = list_GetList.Sum(n => n.WeiKaiPiaoJinE).ToDecimal(2),
                    LaiKuanJinE = list_GetList.Sum(n => n.LaiKuanJinE).ToDecimal(2),
                    YinShouZhangKuanYuE = list_GetList.Sum(n => n.YinSHouYuE).ToDecimal(2),
                    ChengBenJinE = list_GetList.Sum(n => n.WaiFuJinE).ToDecimal(2),
                    LiRun = (list_GetList.Sum(n => n.FeiYong) - list_GetList.Sum(n => n.WaiFuJinE)).ToDecimal(2),
                };

                list_ShouRuHuiZOngBiao.Add(model);
            }
            int total = count;
            object rows = list_ShouRuHuiZOngBiao;
            return Json(new { total = total, rows = rows }, JsonRequestBehavior.AllowGet);

        }
        public ActionResult WanChengList()
        {
            OperatorModel loginModel = OperatorProvider.Provider.GetCurrent();
            ViewBag.roleId = loginModel.RoleId;
            return View();
        }
        public object GetWanChengList(int limit, int offset, string GBNO, string C_GOODSAGENT_NAME, string CreatTime, string CreatTime1, string BLNO, string C_GOODS, string HuoZhu, string ShipName, string VGNO)
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
            List<Stock_Money_WanChengQingKuangBiao> list_WeiKaiPiao = new List<Stock_Money_WanChengQingKuangBiao>();
            foreach (var itsms_GoodsBill in list)//遍历票货
            {
                string Guid_Get = "";
                string QiaTaFeiYong = "";
                C_TB_HC_CONTRACT Model_CONTRACT = db.C_TB_HC_CONTRACT.FirstOrDefault(n => n.Guid == itsms_GoodsBill.CONTRACT_Guid);//查找合同
                if (Model_CONTRACT != null)
                {
                    Guid_Get = Model_CONTRACT.Guid;
                }
                List<Stock_Money> list_GetList = Get_FeiMuXX(Convert.ToInt32(itsms_GoodsBill.ID), Guid_Get);
                foreach (var items_GetList in list_GetList)
                {
                    if (items_GetList.FMZhongLei != "包干费" && items_GetList.FMZhongLei != "堆存费" && items_GetList.FMZhongLei != "运输费" && items_GetList.FMZhongLei != "过磅费" && items_GetList.FMZhongLei != "监管费")
                    {
                        QiaTaFeiYong += "(" + items_GetList.FMZhongLei.ToString() + ":" + items_GetList.WeiKaiPiaoJinE + ")";
                    }
                }
                DateTime dt1 = Convert.ToDateTime("2019-01-01 00:00:00");
                DateTime dt2 = Convert.ToDateTime("2019-12-31 23:59:59");
                List<C_TB_JIESUAN> list_JieSuan = db.C_TB_JIESUAN.Where(n => n.GoodsBill_id == itsms_GoodsBill.ID).ToList();
                decimal? one_fy = 0;
                decimal? two_fy = 0;
                decimal? three_fy = 0;
                decimal? four_fy = 0;
                List<Stock_Money_FeiYong> List_DuiCun = GetTallyBllList_dc(Convert.ToInt32(itsms_GoodsBill.ID));
                List<Stock_Money_DuiCun> List_DuiCun_k = GetTallyBllList_dc_k(Convert.ToInt32(itsms_GoodsBill.ID));
                List<Stock_Money_RiShouRu> List_RiShouRu = GetTallyBllList_RiShouRu(Convert.ToInt32(itsms_GoodsBill.ID));
                one_fy = (List_DuiCun.Where(n => n.date_Out >= dt1.AddYears(-3) && n.date_Out <= dt2.AddYears(-3)).Sum(n => n.FeiYong) + List_DuiCun_k.Where(n => n.RiQi >= dt1.AddYears(-3) && n.RiQi <= dt2.AddYears(-3)).Sum(n => n.FeiYong) + List_RiShouRu.Where(n => n.Date >= dt1.AddYears(-3) && n.Date <= dt2.AddYears(-3)).Sum(n => n.FeiYong)) - list_JieSuan.Where(n => n.Time1 >= dt1.AddYears(-3) && n.Time1 <= dt2.AddYears(-3)).Sum(n => n.KaiPiaoJinE);
                two_fy = (List_DuiCun.Where(n => n.date_Out >= dt1.AddYears(-2) && n.date_Out <= dt2.AddYears(-2)).Sum(n => n.FeiYong) + List_DuiCun_k.Where(n => n.RiQi >= dt1.AddYears(-2) && n.RiQi <= dt2.AddYears(-2)).Sum(n => n.FeiYong) + List_RiShouRu.Where(n => n.Date >= dt1.AddYears(-2) && n.Date <= dt2.AddYears(-2)).Sum(n => n.FeiYong)) - list_JieSuan.Where(n => n.Time1 >= dt1.AddYears(-2) && n.Time1 <= dt2.AddYears(-2)).Sum(n => n.KaiPiaoJinE);
                three_fy = (List_DuiCun.Where(n => n.date_Out >= dt1.AddYears(-1) && n.date_Out <= dt2.AddYears(-1)).Sum(n => n.FeiYong) + List_DuiCun_k.Where(n => n.RiQi >= dt1.AddYears(-1) && n.RiQi <= dt2.AddYears(-1)).Sum(n => n.FeiYong) + List_RiShouRu.Where(n => n.Date >= dt1.AddYears(-1) && n.Date <= dt2.AddYears(-1)).Sum(n => n.FeiYong)) - list_JieSuan.Where(n => n.Time1 >= dt1.AddYears(-1) && n.Time1 <= dt2.AddYears(-1)).Sum(n => n.KaiPiaoJinE);
                four_fy = (List_DuiCun.Where(n => n.date_Out >= dt1 && n.date_Out <= dt2).Sum(n => n.FeiYong) + List_DuiCun_k.Where(n => n.RiQi >= dt1 && n.RiQi <= dt2).Sum(n => n.FeiYong) + List_RiShouRu.Where(n => n.Date >= dt1 && n.Date <= dt2).Sum(n => n.FeiYong)) - list_JieSuan.Where(n => n.Time1 >= dt1 && n.Time1 <= dt2).Sum(n => n.KaiPiaoJinE);
                Stock_Money_WanChengQingKuangBiao model = new Stock_Money_WanChengQingKuangBiao()
                {
                    ID = itsms_GoodsBill.ID,
                    HuoDai = itsms_GoodsBill.C_GOODSAGENT_NAME,
                    HuoWu = itsms_GoodsBill.C_GOODS,
                    ChuanMing = itsms_GoodsBill.ShipName,
                    BaoGanFei = list_GetList.Where(n => n.FMZhongLei == "包干费").Sum(n => n.FeiYong).ToDecimal(2),
                    DuiCunFei = list_GetList.Where(n => n.FMZhongLei == "堆存费").Sum(n => n.FeiYong).ToDecimal(2),
                    YunFei = list_GetList.Where(n => n.FMZhongLei == "运输费").Sum(n => n.FeiYong.ToDecimal(2)),
                    GuoBangFei = list_GetList.Where(n => n.FMZhongLei == "过磅费" || n.FMZhongLei == "监管费").Sum(n => n.FeiYong).ToDecimal(2),
                    QiYaFeiYong = list_GetList.Where(n => n.FMZhongLei != "包干费" && n.FMZhongLei != "堆存费" && n.FMZhongLei != "运输费" && n.FMZhongLei != "过磅费" && n.FMZhongLei != "监管费").Sum(n => n.WeiKaiPiaoShuiHou).ToDecimal(2) + QiaTaFeiYong,
                    FeiYongHeJi = list_GetList.Sum(n => n.FeiYong).ToDecimal(2),
                    WeiKaiPiao = list_GetList.Sum(n => n.WeiKaiPiaoShuiHou).ToDecimal(2),
                    one = one_fy.ToDecimal(2),
                    two = two_fy.ToDecimal(2),
                    three = three_fy.ToDecimal(2),
                    four = four_fy.ToDecimal(2),
                    QiTa= list_GetList.Where(n => n.FMZhongLei != "包干费" && n.FMZhongLei != "堆存费" && n.FMZhongLei != "运输费" && n.FMZhongLei != "过磅费" && n.FMZhongLei != "监管费").Sum(n => n.WeiKaiPiaoShuiHou).ToDecimal(2)


                };

                list_WeiKaiPiao.Add(model);
            }
            int total = count;
            object rows = list_WeiKaiPiao;
            return Json(new { total = total, rows = rows }, JsonRequestBehavior.AllowGet);

        }
        public List<Stock_Money> Get_FeiMuXX(int goodsbillid, string Guid)
        {
            List<Stock_Money> list = new List<Stock_Money>();
            List<C_TB_HC_CONTRACT_DETAILED> list_DETAILED = db.C_TB_HC_CONTRACT_DETAILED.Where(n => n.CONTRACT_Guid == Guid).ToList();


            foreach (var items in list_DETAILED)
            {
                MoneyController mc = new MoneyController();
                decimal? shuilv = 0;
                decimal? HuiShouLv = 0;
                C_TB_ZHIXINGFEILV model_ZHIXINGFEILV = db.C_TB_ZHIXINGFEILV.FirstOrDefault(n => n.FeiMuZhongLei == items.FeiMuZhongLei && n.Type == items.Type && n.GoodsBillId == goodsbillid);
                decimal? ShiJiShuiLv = 0;
                if (model_ZHIXINGFEILV != null)
                {
                    if (!string.IsNullOrEmpty(model_ZHIXINGFEILV.FeiLv.ToString()))
                    {
                        ShiJiShuiLv = model_ZHIXINGFEILV.FeiLv;
                    }
                    else
                    {
                        ShiJiShuiLv = items.DanJia;
                    }
                }
                else
                {
                    ShiJiShuiLv = items.DanJia;
                }
                string type = "";
                if (string.IsNullOrEmpty(items.Type))
                {
                    type = "全部";
                }
                else
                {
                    type = items.Type;
                }
                List<C_TB_JIESUAN> list_JieSuan = db.C_TB_JIESUAN.Where(n => n.FeiMuZhongLei == items.FeiMuZhongLei && n.Type == type && n.GoodsBill_id == goodsbillid).ToList();
                List<C_TB_CHENGBEN> list_ChengBen = db.C_TB_CHENGBEN.Where(n => n.FeiMuZhongLei == items.FeiMuZhongLei && n.Type == type && n.GoodsBill_id == goodsbillid).ToList();
                if (list_JieSuan.Sum(n => n.KaiPiaoJinE) == 0)
                {
                    HuiShouLv = 0;
                }
                else
                {
                    HuiShouLv = list_JieSuan.Sum(n => n.LaiKuanJinE) / list_JieSuan.Sum(n => n.KaiPiaoJinE);
                }
                if (!string.IsNullOrEmpty(items.ShuiE) && items.ShuiE != null)
                {
                    shuilv = Convert.ToDecimal(items.ShuiE);
                }
                else
                {
                    shuilv = 0;
                }
                if (string.IsNullOrEmpty(items.Type) || items.Type == "null")
                {
                    items.Type = "全部";
                }
                List<C_TB_SHOUFEI> list_ShouFei = db.C_TB_SHOUFEI.Where(n => n.FeiMuZhongLei == items.FeiMuZhongLei && n.Type == items.Type && n.GoodsBill_id == goodsbillid).ToList();
                decimal? feiyong = 0;
                decimal? ShuLiang = 0;
                //TimeSpan? ts = DateTime.Now - Model_TALLYBILL.SIGNDATE;//获取收费天数
                if (items.JiLiangDanWei.Contains("*天"))
                {
                    if (items.Type == "进库" || items.Type == "出库")
                    {
                        decimal? FeiYong, JiFeiShuLiang, duicun;
                        mc.GetTallyBllList_hqdc(goodsbillid, out FeiYong, out JiFeiShuLiang, out duicun);
                        feiyong = FeiYong.ToDecimal(2);
                        ShuLiang = JiFeiShuLiang;
                    }
                    else
                    {
                        decimal? FeiYong, JiFeiShuLiang, duicun;
                        mc.GetTallyBllList_hqdc_k(goodsbillid, out FeiYong, out JiFeiShuLiang, out duicun);
                        feiyong = FeiYong.ToDecimal(2);
                        ShuLiang = JiFeiShuLiang;
                    }
                }

                else
                {

                    decimal? FeiYong, shuLiang;
                    mc.GetTallyBllList_hqqyfy(goodsbillid, items.FeiMuZhongLei, items.Type, out FeiYong, out shuLiang);
                    ShuLiang = shuLiang;
                    feiyong = (shuLiang * ShiJiShuiLv).ToDecimal(2);

                }
                DateTime dt1 = Convert.ToDateTime("2019-01-01 00:00:00");
                DateTime dt2 = Convert.ToDateTime("2019-12-31 23:59:59");
                Stock_Money model = new Stock_Money()
                {
                    ID = goodsbillid,
                    HuoMing = items.HuoMing,
                    DanJia = items.DanJia,
                    FMZhongLei = items.FeiMuZhongLei,
                    JiLiangDanWei = items.JiLiangDanWei,
                    BeiZhu = items.BeiZhu,
                    FeiYong = feiyong.ToDecimal(2),
                    MianCunQi = items.MianDuiCunTianShu,
                    ShuLiang = ShuLiang.ToDecimal(3),
                    GoodsBill_id = goodsbillid,
                    Type = items.Type,
                    YiShou = list_ShouFei.Sum(n => n.JinE).ToDecimal(2),
                    WeiShou = feiyong - list_ShouFei.Sum(n => n.JinE).ToDecimal(2),
                    ShuiHouJinE = (feiyong / Convert.ToDecimal((1 + (Convert.ToDecimal(shuilv) * Convert.ToDecimal(0.01))))).ToDecimal(2),
                    KaiPiaoJinE = list_JieSuan.Sum(n => n.KaiPiaoJinE).ToDecimal(2),
                    KaiPiaoShuiHou = (list_JieSuan.Sum(n => n.KaiPiaoJinE).ToDecimal(2) / Convert.ToDecimal((1 + (Convert.ToDecimal(shuilv) * Convert.ToDecimal(0.01))))).ToDecimal(2),
                    LaiKuanJinE = list_JieSuan.Sum(n => n.LaiKuanJinE).ToDecimal(2),
                    WeiKaiPiaoJinE = (feiyong - list_JieSuan.Sum(n => n.KaiPiaoJinE)).ToDecimal(2),
                    WeiKaiPiaoShuiHou = ((feiyong - list_JieSuan.Sum(n => n.KaiPiaoJinE)) / Convert.ToDecimal((1 + (Convert.ToDecimal(shuilv) * Convert.ToDecimal(0.01))))).ToDecimal(2),
                    YinSHouYuE = (list_JieSuan.Sum(n => n.KaiPiaoJinE) - list_JieSuan.Sum(n => n.LaiKuanJinE)).ToDecimal(2),
                    HuiShouLv = HuiShouLv.ToDecimal(3),
                    WaiFuJinE = list_ChengBen.Sum(n => n.ChengBenJinE),
                    ShiJiShuiLv = ShiJiShuiLv,
                    ChengBenJiFeiYiJu = items.ChengBenJiFeiYiJu,
                    ChengBenFeiLv = items.ChengBenFeiLv,
                    YearKaiPiaoJinE = list_JieSuan.Where(n => n.KaiPiaoRiQi >= dt1 && n.KaiPiaoRiQi <= dt2).Sum(n => n.KaiPiaoJinE).ToDecimal(2),
                    FeiMuZhongLeiCode = items.FeiMuZhongLeiCode,
                    HuoMingCode = items.HuoMingCode,
                    ShuiLv= shuilv,


                };
                list.Add(model);

            }
            return list;

        }
        public List<Stock_Money> Get_FeiMuXX_New(int goodsbillid, string Guid, string FMZhongLei, string Type)
        {
            List<Stock_Money> list = new List<Stock_Money>();
            List<C_TB_HC_CONTRACT_DETAILED> list_DETAILED = db.C_TB_HC_CONTRACT_DETAILED.Where(n => n.CONTRACT_Guid == Guid&&n.FeiMuZhongLei== FMZhongLei&&n.Type== Type).ToList();
            foreach (var items in list_DETAILED)
            {
                MoneyController mc = new MoneyController();
                decimal? shuilv = 0;
                decimal? HuiShouLv = 0;
                C_TB_ZHIXINGFEILV model_ZHIXINGFEILV = db.C_TB_ZHIXINGFEILV.FirstOrDefault(n => n.FeiMuZhongLei == items.FeiMuZhongLei && n.Type == items.Type && n.GoodsBillId == goodsbillid);
                decimal? ShiJiShuiLv = 0;
                if (model_ZHIXINGFEILV != null)
                {
                    if (!string.IsNullOrEmpty(model_ZHIXINGFEILV.FeiLv.ToString()))
                    {
                        ShiJiShuiLv = model_ZHIXINGFEILV.FeiLv;
                    }
                    else
                    {
                        ShiJiShuiLv = items.DanJia;
                    }
                }
                else
                {
                    ShiJiShuiLv = items.DanJia;
                }
                string type = "";
                if (string.IsNullOrEmpty(items.Type))
                {
                    type = "全部";
                }
                else
                {
                    type = items.Type;
                }
                List<C_TB_JIESUAN> list_JieSuan = db.C_TB_JIESUAN.Where(n => n.FeiMuZhongLei == items.FeiMuZhongLei && n.Type == type && n.GoodsBill_id == goodsbillid).ToList();
                List<C_TB_CHENGBEN> list_ChengBen = db.C_TB_CHENGBEN.Where(n => n.FeiMuZhongLei == items.FeiMuZhongLei && n.Type == type && n.GoodsBill_id == goodsbillid).ToList();
                if (list_JieSuan.Sum(n => n.KaiPiaoJinE) == 0)
                {
                    HuiShouLv = 0;
                }
                else
                {
                    HuiShouLv = list_JieSuan.Sum(n => n.LaiKuanJinE) / list_JieSuan.Sum(n => n.KaiPiaoJinE);
                }
                if (!string.IsNullOrEmpty(items.ShuiE) && items.ShuiE != null)
                {
                    shuilv = Convert.ToDecimal(items.ShuiE);
                }
                else
                {
                    shuilv = 0;
                }
                if (string.IsNullOrEmpty(items.Type) || items.Type == "null")
                {
                    items.Type = "全部";
                }
                List<C_TB_SHOUFEI> list_ShouFei = db.C_TB_SHOUFEI.Where(n => n.FeiMuZhongLei == items.FeiMuZhongLei && n.Type == items.Type && n.GoodsBill_id == goodsbillid).ToList();
                decimal? feiyong = 0;
                decimal? ShuLiang = 0;
                //TimeSpan? ts = DateTime.Now - Model_TALLYBILL.SIGNDATE;//获取收费天数
                if (items.JiLiangDanWei.Contains("*天"))
                {
                    if (items.Type == "进库" || items.Type == "出库")
                    {
                        decimal? FeiYong, JiFeiShuLiang, duicun;
                        mc.GetTallyBllList_hqdc(goodsbillid, out FeiYong, out JiFeiShuLiang, out duicun);
                        feiyong = FeiYong.ToDecimal(2);
                        ShuLiang = JiFeiShuLiang;
                    }
                    else
                    {
                        decimal? FeiYong, JiFeiShuLiang, duicun;
                        mc.GetTallyBllList_hqdc_k(goodsbillid, out FeiYong, out JiFeiShuLiang, out duicun);
                        feiyong = FeiYong.ToDecimal(2);
                        ShuLiang = JiFeiShuLiang;
                    }
                }

                else
                {

                    decimal? FeiYong, shuLiang;
                    mc.GetTallyBllList_hqqyfy(goodsbillid, items.FeiMuZhongLei, items.Type, out FeiYong, out shuLiang);
                    ShuLiang = shuLiang;
                    feiyong = (shuLiang * ShiJiShuiLv).ToDecimal(2);

                }
                DateTime dt1 = Convert.ToDateTime("2019-01-01 00:00:00");
                DateTime dt2 = Convert.ToDateTime("2019-12-31 23:59:59");
                Stock_Money model = new Stock_Money()
                {
                    ID = goodsbillid,
                    HuoMing = items.HuoMing,
                    DanJia = items.DanJia,
                    FMZhongLei = items.FeiMuZhongLei,
                    JiLiangDanWei = items.JiLiangDanWei,
                    BeiZhu = items.BeiZhu,
                    FeiYong = feiyong.ToDecimal(2),
                    MianCunQi = items.MianDuiCunTianShu,
                    ShuLiang = ShuLiang.ToDecimal(3),
                    GoodsBill_id = goodsbillid,
                    Type = items.Type,
                    YiShou = list_ShouFei.Sum(n => n.JinE).ToDecimal(2),
                    WeiShou = feiyong - list_ShouFei.Sum(n => n.JinE).ToDecimal(2),
                    ShuiHouJinE = (feiyong / Convert.ToDecimal((1 + (Convert.ToDecimal(shuilv) * Convert.ToDecimal(0.01))))).ToDecimal(2),
                    KaiPiaoJinE = list_JieSuan.Sum(n => n.KaiPiaoJinE).ToDecimal(2),
                    KaiPiaoShuiHou = (list_JieSuan.Sum(n => n.KaiPiaoJinE).ToDecimal(2) / Convert.ToDecimal((1 + (Convert.ToDecimal(shuilv) * Convert.ToDecimal(0.01))))).ToDecimal(2),
                    LaiKuanJinE = list_JieSuan.Sum(n => n.LaiKuanJinE).ToDecimal(2),
                    WeiKaiPiaoJinE = (feiyong - list_JieSuan.Sum(n => n.KaiPiaoJinE)).ToDecimal(2),
                    WeiKaiPiaoShuiHou = ((feiyong - list_JieSuan.Sum(n => n.KaiPiaoJinE)) / Convert.ToDecimal((1 + (Convert.ToDecimal(shuilv) * Convert.ToDecimal(0.01))))).ToDecimal(2),
                    YinSHouYuE = (list_JieSuan.Sum(n => n.KaiPiaoJinE) - list_JieSuan.Sum(n => n.LaiKuanJinE)).ToDecimal(2),
                    HuiShouLv = HuiShouLv.ToDecimal(3),
                    WaiFuJinE = list_ChengBen.Sum(n => n.ChengBenJinE),
                    ShiJiShuiLv = ShiJiShuiLv,
                    ChengBenJiFeiYiJu = items.ChengBenJiFeiYiJu,
                    ChengBenFeiLv = items.ChengBenFeiLv,
                    YearKaiPiaoJinE = list_JieSuan.Where(n => n.KaiPiaoRiQi >= dt1 && n.KaiPiaoRiQi <= dt2).Sum(n => n.KaiPiaoJinE).ToDecimal(2),
                    FeiMuZhongLeiCode = items.FeiMuZhongLeiCode,
                    HuoMingCode = items.HuoMingCode,
                    ShuiLv = shuilv,


                };
                list.Add(model);

            }
           
            return list;

        }
        public List<Stock_Money_FeiYong> GetTallyBllList_dc(int id)
        {
            List<Stock_Money_In> list_In = new List<Stock_Money_In>();//获取每天进库数量临时表
            List<Stock_Money_Out> list_Out = new List<Stock_Money_Out>();//获取每天出库数量临时表
            List<Stock_Money_FeiYong> list_FeiYong = new List<Stock_Money_FeiYong>();//最终前台绑定的表
            C_TB_HC_GOODSBILL Model_GOODSBILL = db.C_TB_HC_GOODSBILL.FirstOrDefault(n => n.ID == id);//查找票货
            C_TB_HC_CONTRACT Model_CONTRACT = db.C_TB_HC_CONTRACT.FirstOrDefault(n => n.Guid == Model_GOODSBILL.CONTRACT_Guid);//查找合同
            if (Model_CONTRACT != null)
            {
                C_TB_HC_CONTRACT_DETAILED Model_DETAILED = db.C_TB_HC_CONTRACT_DETAILED.FirstOrDefault(n => n.CONTRACT_Guid == Model_CONTRACT.Guid && n.FeiMuZhongLei == "堆存费" && n.Type == "出库");
                if (Model_DETAILED != null)
                {
                    List<C_TB_HC_CONSIGN> list_CONSIGN = db.C_TB_HC_CONSIGN.Where(n => n.GOODSBILL_ID == Model_GOODSBILL.ID).ToList();//查找委托
                    List<C_TB_HS_TALLYBILL> list_TALLYBILL_hc = new List<C_TB_HS_TALLYBILL>();
                    foreach (var items in list_CONSIGN)//获取票货下面所有理货单
                    {
                        List<C_TB_HS_TALLYBILL> list_TALLYBILL = db.C_TB_HS_TALLYBILL.Where(n => n.CONSIGN_ID == items.ID).ToList();
                        foreach (var items_TallBill in list_TALLYBILL)
                        {
                            list_TALLYBILL_hc.Add(items_TallBill);
                        }
                    }
                    list_TALLYBILL_hc = list_TALLYBILL_hc.OrderBy(n => n.SIGNDATE).ToList();//将所有理货单排序
                    int days = -1;
                    if (list_TALLYBILL_hc.Where(n => n.CODE_OPSTYPE == "进库").ToList().Count != 0)//获取所有进库理货单
                    {
                        TimeSpan? ts = DateTime.Now - list_TALLYBILL_hc[0].SIGNDATE;//获取循环天数
                        days = ts.Value.Days;
                    }
                    for (int i = 0; i <= days; i++)
                    {
                        DateTime? StartTime_jck = Convert.ToDateTime(list_TALLYBILL_hc[0].SIGNDATE.ToDateString().Substring(0, 10) + " 00:00:00").AddDays(i);
                        DateTime? EndTime = Convert.ToDateTime(list_TALLYBILL_hc[0].SIGNDATE.ToDateString().Substring(0, 10) + " 23:59:59").AddDays(i);
                        decimal? InAllNum = 0;//进库数
                        DateTime? date_in = DateTime.Now;//进库时间
                        decimal? OutAllNum = 0;//出库数
                        DateTime? date_Out = DateTime.Now;//出库时间
                        foreach (var items_In in list_TALLYBILL_hc.Where(n => n.CODE_OPSTYPE == "进库" && n.SIGNDATE >= StartTime_jck && n.SIGNDATE <= EndTime && n.Type != "清场"))
                        {
                            if (Model_DETAILED.JiLiangDanWei.Contains("箱"))
                            {
                                if (items_In.XIANGSHU != 0 && items_In.XIANGSHU != null)
                                {
                                    InAllNum += items_In.XIANGSHU;
                                }
                                else
                                {
                                    InAllNum += items_In.WEIGHT;
                                }
                            }
                            else
                            {
                                InAllNum += items_In.WEIGHT;
                            }
                            date_in = items_In.SIGNDATE;

                        }
                        foreach (var items_Out in list_TALLYBILL_hc.Where(n => n.CODE_OPSTYPE == "出库" && n.SIGNDATE >= StartTime_jck && n.SIGNDATE <= EndTime && n.Type != "清场"))
                        {
                            if (Model_DETAILED.JiLiangDanWei.Contains("箱"))
                            {
                                if (items_Out.XIANGSHU != 0 && items_Out.XIANGSHU != null)
                                {
                                    OutAllNum += items_Out.XIANGSHU;
                                }
                                else
                                {
                                    OutAllNum += items_Out.WEIGHT;
                                }
                            }
                            else
                            {
                                OutAllNum += items_Out.WEIGHT;
                            }
                            date_Out = items_Out.SIGNDATE;

                        }
                        Stock_Money_In model_In = new Stock_Money_In()
                        {
                            InAllNum = InAllNum,
                            date_in = Convert.ToDateTime(date_in.ToDateString())
                        };
                        if (InAllNum != 0)
                        {
                            list_In.Add(model_In);
                        }
                        Stock_Money_Out model_Out = new Stock_Money_Out()
                        {
                            OutAllNum = OutAllNum,
                            date_Out = Convert.ToDateTime(date_Out.ToDateString())
                        };
                        if (OutAllNum != 0)
                        {
                            list_Out.Add(model_Out);
                        }

                    }
                    list_In = list_In.OrderBy(n => n.date_in).ToList();//将所有进库理货单排序
                    list_Out = list_Out.OrderBy(n => n.date_Out).ToList();//将所有出库理货单排序
                    decimal? SuoYouIn = list_In.Sum(n => n.InAllNum);//获取此理货单下所有金库数量
                    decimal? SuoYouOut = 0;
                    int num = 0;//获取出库来源
                    decimal? danjia = 0;
                    C_TB_ZHIXINGFEILV model_ZHIXINGFEILV = db.C_TB_ZHIXINGFEILV.FirstOrDefault(n => n.GoodsBillId == Model_GOODSBILL.ID && n.FeiMuZhongLei == "堆存费");
                    if (model_ZHIXINGFEILV != null)//执行费率
                    {
                        danjia = model_ZHIXINGFEILV.FeiLv;
                    }
                    if (Model_DETAILED != null)
                    {
                        danjia = Model_DETAILED.DanJia;
                    }
                    else
                    {
                        danjia = 0;
                    }
                    foreach (var items_FeiYong in list_Out)
                    {
                        decimal? duoyu = items_FeiYong.OutAllNum;//一单里出完库不够的数量
                                                                 // AllOut += items_FeiYong.OutAllNum;//已累计出库量
                        for (int i = 0; i != 1;)
                        {
                            if (SuoYouIn - SuoYouOut <= 0)
                            {
                                Stock_Money_FeiYong model_FeiYong = new Stock_Money_FeiYong()
                                {
                                    date_Out = items_FeiYong.date_Out,//出库日期
                                    OutAllNum = duoyu,//出库数
                                    InAllYD = -duoyu,//余吨
                                    MianDuiQi = Model_DETAILED.MianDuiCunTianShu,//免堆期
                                    DunTian = 0,//收费天数
                                    FeiLv = 0,//单价
                                    FeiYong = 0//费用=出库数*单价*收费天数
                                };
                                list_FeiYong.Add(model_FeiYong);
                                duoyu = 0;
                                i = 1;
                                continue;
                            }
                            TimeSpan? ts_FeiYong = null;
                            if (Model_GOODSBILL.C_GOODS.Contains("木"))
                            {
                                ts_FeiYong = items_FeiYong.date_Out - list_In[0].date_in;//出进库之间天数

                            }
                            else
                            {
                                ts_FeiYong = items_FeiYong.date_Out - list_In[num].date_in;//出进库之间天数
                            }
                            int days_items_FeiYong = 0;

                            if (Model_GOODSBILL.C_GOODS.Contains("木"))//木材堆存期额外+1
                            {
                                days_items_FeiYong = ts_FeiYong.Value.Days + 1 - Convert.ToInt32(Model_DETAILED.MianDuiCunTianShu);
                            }
                            else
                            {
                                days_items_FeiYong = ts_FeiYong.Value.Days + 1 - Convert.ToInt32(Model_DETAILED.MianDuiCunTianShu);
                            }
                            if (days_items_FeiYong <= 0)
                            {
                                days_items_FeiYong = 0;
                            }
                            if (list_In[num].InAllNum == 0)//当某日没有库存 跳过
                            {
                                num += 1;
                                continue;
                            }
                            if (list_In[num].InAllNum - duoyu >= 0)//如果某单日库存大于出库数量
                            {


                                list_In[num].InAllNum = list_In[num].InAllNum - duoyu;//余吨
                                Stock_Money_FeiYong model_FeiYong = new Stock_Money_FeiYong()
                                {
                                    date_Out = items_FeiYong.date_Out,//出库日期
                                    OutAllNum = duoyu,//出库数
                                    date_in = list_In[num].date_in,//进库日期
                                    InAllYD = list_In[num].InAllNum,//余吨
                                    MianDuiQi = Model_DETAILED.MianDuiCunTianShu,//免堆期
                                    DunTian = days_items_FeiYong,//收费天数
                                    FeiLv = danjia,//单价
                                    FeiYong = (duoyu * danjia * days_items_FeiYong).ToDecimal(4)//费用=出库数*单价*收费天数
                                };
                                list_FeiYong.Add(model_FeiYong);
                                SuoYouOut += duoyu;//获取已出库量
                                i = 1;
                                continue;

                            }
                            else
                            {
                                Stock_Money_FeiYong model_FeiYong = new Stock_Money_FeiYong()
                                {
                                    date_Out = items_FeiYong.date_Out,//出库日期
                                    OutAllNum = list_In[num].InAllNum,//出库数
                                    date_in = list_In[num].date_in,//进库日期
                                    InAllYD = 0,//余吨
                                    MianDuiQi = Model_DETAILED.MianDuiCunTianShu,//免堆期
                                    DunTian = days_items_FeiYong,//收费天数
                                    FeiLv = danjia,//单价
                                    FeiYong = (list_In[num].InAllNum * danjia * days_items_FeiYong).ToDecimal(4)//费用=出库数*单价*收费天数
                                };
                                list_FeiYong.Add(model_FeiYong);
                                duoyu = duoyu - list_In[num].InAllNum;
                                SuoYouOut += list_In[num].InAllNum;//获取已出库量
                                num += 1;
                                continue;
                            }




                        }


                    }
                    foreach (var item_fushu in list_FeiYong)//如果库存是负数，按照第一个理货单的进库日期计算
                    {
                        if (string.IsNullOrEmpty(item_fushu.date_in.ToString()))
                        {
                            TimeSpan? ts_fushu = item_fushu.date_Out - list_TALLYBILL_hc[0].SIGNDATE;
                            int days_fushu = ts_fushu.Value.Days;
                            item_fushu.DunTian = days_fushu - Model_DETAILED.MianDuiCunTianShu + 1;
                            item_fushu.FeiYong = (item_fushu.OutAllNum * danjia * (days_fushu - Model_DETAILED.MianDuiCunTianShu + 1)).ToDecimal(4);//费用=出库数*单价*收费天数
                            item_fushu.FeiLv = danjia;

                        }
                    }
                }
            }




            return list_FeiYong;

        }
        public List<Stock_Money_DuiCun> GetTallyBllList_dc_k(int id)
        {
            List<Stock_Money_DuiCun> list = new List<Stock_Money_DuiCun>();
            C_TB_HC_GOODSBILL Model_GOODSBILL = db.C_TB_HC_GOODSBILL.FirstOrDefault(n => n.ID == id);//查找票货
            C_TB_HC_CONTRACT Model_CONTRACT = db.C_TB_HC_CONTRACT.FirstOrDefault(n => n.Guid == Model_GOODSBILL.CONTRACT_Guid);//查找合同
            if (Model_CONTRACT != null)
            {
                C_TB_HC_CONTRACT_DETAILED Model_DETAILED = db.C_TB_HC_CONTRACT_DETAILED.FirstOrDefault(n => n.CONTRACT_Guid == Model_CONTRACT.Guid && n.FeiMuZhongLei == "堆存费" && n.Type != "出库");
                if (Model_DETAILED != null)
                {
                    List<C_TB_HC_CONSIGN> list_CONSIGN = db.C_TB_HC_CONSIGN.Where(n => n.GOODSBILL_ID == Model_GOODSBILL.ID).ToList();//查找委托
                    List<C_TB_HS_TALLYBILL> list_TALLYBILL_hc = new List<C_TB_HS_TALLYBILL>();//获取货票下面所有理货单
                    foreach (var items in list_CONSIGN)
                    {
                        List<C_TB_HS_TALLYBILL> list_TALLYBILL = db.C_TB_HS_TALLYBILL.Where(n => n.CONSIGN_ID == items.ID).ToList();
                        foreach (var items_TallBill in list_TALLYBILL)
                        {
                            list_TALLYBILL_hc.Add(items_TallBill);
                        }

                    }
                    C_TB_HS_TALLYBILL model_date = list_TALLYBILL_hc.FirstOrDefault(n => n.Type == "清场");
                    list_TALLYBILL_hc = list_TALLYBILL_hc.OrderBy(n => n.SIGNDATE).ToList();
                    int days = -1;
                    TimeSpan? ts = new TimeSpan();
                    if (list_TALLYBILL_hc.Count != 0)
                    {
                        if (model_date != null && model_date.CreatTime != null)
                        {
                            ts = model_date.CreatTime - list_TALLYBILL_hc[0].SIGNDATE;//获取循环天数
                        }
                        else
                        {
                            ts = DateTime.Now - list_TALLYBILL_hc[0].SIGNDATE;//获取循环天数
                        }

                        days = ts.Value.Days;
                    }
                    for (int i = 0; i <= days; i++)
                    {
                        DateTime? StartTime = Convert.ToDateTime(list_TALLYBILL_hc[0].SIGNDATE.ToDateString().Substring(0, 10) + " 00:00:00");
                        DateTime? StartTime_jck = Convert.ToDateTime(list_TALLYBILL_hc[0].SIGNDATE.ToDateString().Substring(0, 10) + " 00:00:00").AddDays(i);
                        DateTime? EndTime = Convert.ToDateTime(list_TALLYBILL_hc[0].SIGNDATE.ToDateString().Substring(0, 10) + " 23:59:59").AddDays(i);
                        decimal? ShuLiang = 0;
                        decimal? ShuLiang_jck = 0;
                        decimal? DuiCunShuLiang = 0;//堆存数量
                        decimal? BuJiFeiShuLiang = 0;//不计费数量
                        decimal? JinKu = 0;//进库数量
                        decimal? ChuKu = 0;//出库数量
                        List<C_TB_HS_TALLYBILL> list_TALLYBILL_js = list_TALLYBILL_hc.Where(n => n.SIGNDATE >= StartTime && n.SIGNDATE <= EndTime).ToList();//获取时间区间内的所有理货单
                        List<C_TB_HS_TALLYBILL> list_TALLYBILL_jck = list_TALLYBILL_hc.Where(n => n.SIGNDATE >= StartTime_jck && n.SIGNDATE <= EndTime && n.Type != "清场").ToList();//获取当前循环时间内的所有理货单
                        TimeSpan? ts_miandui = Convert.ToDateTime(list_TALLYBILL_hc[0].SIGNDATE).AddDays(i) - list_TALLYBILL_hc[0].SIGNDATE;//获取循环天数
                        int days_miandui = 0;
                        days_miandui = ts_miandui.Value.Days;
                        foreach (var items_jck in list_TALLYBILL_jck)
                        {

                            if (items_jck.CODE_OPSTYPE == "进库")
                            {
                                if (Model_DETAILED.JiLiangDanWei.Contains("箱"))
                                {
                                    if (items_jck.XIANGSHU != 0 && items_jck.XIANGSHU != null)
                                    {
                                        JinKu += items_jck.XIANGSHU;
                                        ShuLiang_jck = items_jck.XIANGSHU;
                                    }
                                    else
                                    {
                                        JinKu += items_jck.WEIGHT;
                                        ShuLiang_jck = items_jck.WEIGHT;
                                    }
                                }
                                else
                                {
                                    JinKu += items_jck.WEIGHT;
                                    ShuLiang_jck = items_jck.WEIGHT;
                                }
                            }
                            if (items_jck.CODE_OPSTYPE == "出库")
                            {
                                if (Model_DETAILED.JiLiangDanWei.Contains("箱"))
                                {
                                    if (items_jck.XIANGSHU != 0 && items_jck.XIANGSHU != null)
                                    {
                                        ChuKu += items_jck.XIANGSHU;
                                        ShuLiang_jck = items_jck.XIANGSHU;
                                    }
                                    else
                                    {
                                        ChuKu += items_jck.WEIGHT;
                                        ShuLiang_jck = items_jck.XIANGSHU;
                                    }
                                }
                                else
                                {
                                    ChuKu += items_jck.WEIGHT;
                                    ShuLiang_jck = items_jck.WEIGHT;
                                }
                            }

                        }
                        foreach (var items_js in list_TALLYBILL_js)
                        {
                            TimeSpan? ts_Mdc = Convert.ToDateTime(list_TALLYBILL_hc[0].SIGNDATE).AddDays(i) - items_js.SIGNDATE.Value.AddDays(Model_DETAILED.MianDuiCunTianShu.ToDouble());//获取免堆天数
                            int Days_Mdq = ts_Mdc.Value.Days;
                            if (items_js.CODE_OPSTYPE == "进库")
                            {
                                if (Model_DETAILED.JiLiangDanWei.Contains("箱"))
                                {
                                    if (items_js.XIANGSHU != 0 && items_js.XIANGSHU != null)
                                    {
                                        ShuLiang = items_js.XIANGSHU;
                                        DuiCunShuLiang += items_js.XIANGSHU;
                                    }
                                    else
                                    {
                                        ShuLiang = items_js.WEIGHT;
                                        DuiCunShuLiang += items_js.WEIGHT;
                                    }
                                }
                                else
                                {
                                    ShuLiang = items_js.WEIGHT;
                                    DuiCunShuLiang += items_js.WEIGHT;
                                }
                            }
                            if (items_js.CODE_OPSTYPE == "出库")
                            {
                                if (Model_DETAILED.JiLiangDanWei.Contains("箱"))
                                {
                                    if (items_js.XIANGSHU != 0 && items_js.XIANGSHU != null)
                                    {
                                        ShuLiang = items_js.XIANGSHU;
                                        DuiCunShuLiang -= items_js.XIANGSHU;
                                    }
                                    else
                                    {
                                        ShuLiang = items_js.WEIGHT;
                                        DuiCunShuLiang -= items_js.WEIGHT;
                                    }
                                }
                                else
                                {
                                    ShuLiang = items_js.WEIGHT;
                                    DuiCunShuLiang -= items_js.WEIGHT;
                                }

                            }
                        }
                        decimal? danjia = 0;
                        C_TB_ZHIXINGFEILV model_ZHIXINGFEILV = db.C_TB_ZHIXINGFEILV.FirstOrDefault(n => n.GoodsBillId == Model_GOODSBILL.ID && n.FeiMuZhongLei == "堆存费");
                        if (model_ZHIXINGFEILV != null)//执行费率
                        {
                            danjia = model_ZHIXINGFEILV.FeiLv;
                        }
                        else
                        {
                            danjia = Model_DETAILED.DanJia;
                        }
                        decimal? FeiYong = 0;
                        decimal? JiFeiShuLiang = 0;
                        if (days_miandui < Model_DETAILED.MianDuiCunTianShu)
                        {
                            FeiYong = 0;
                            JiFeiShuLiang = 0;
                        }
                        else
                        {
                            FeiYong = ((DuiCunShuLiang + ChuKu) * danjia).ToDecimal(4);
                            JiFeiShuLiang = DuiCunShuLiang + ChuKu;
                        }

                        Stock_Money_DuiCun model = new Stock_Money_DuiCun()
                        {
                            RiQi = Convert.ToDateTime(list_TALLYBILL_hc[0].SIGNDATE.ToDateString().Substring(0, 10)).AddDays(i),
                            DuiCunShuLiang = DuiCunShuLiang,
                            MianCunQi = Model_DETAILED.MianDuiCunTianShu,
                            JiFeiShuLiang = JiFeiShuLiang,
                            BuJiFeiShuLiang = BuJiFeiShuLiang,
                            DanJia = danjia,
                            FeiYong = FeiYong.ToDecimal(4),
                            JinKu = JinKu,
                            ChuKu = ChuKu

                        };
                        list.Add(model);
                    }

                    List<C_DIC_GUIZE> List_GUIZE = db.C_DIC_GUIZE.Where(n => n.FeiMuZhongLei == "堆存费" && n.GoodsBillId == id).ToList();//获取计费规则
                    foreach (var items_GUIZE in List_GUIZE)
                    {
                        if (items_GUIZE.Time1 != null && items_GUIZE.Time2 != null && items_GUIZE.FeiLv != null)//区间费率规则
                        {
                            int start = Convert.ToInt32(items_GUIZE.Time1);
                            int end = Convert.ToInt32(items_GUIZE.Time2) - Convert.ToInt32(items_GUIZE.Time1) + 1;
                            decimal? Gz_FeiLv = Convert.ToDecimal(items_GUIZE.FeiLv);
                            for (int i_start = start; i_start <= end; i_start++)
                            {
                                list[i_start - 1].DanJia = Gz_FeiLv;
                                list[i_start - 1].FeiYong = Gz_FeiLv * list[i_start - 1].JiFeiShuLiang;
                            }
                        }
                        if (items_GUIZE.Time_start != null && items_GUIZE.Time_end != null && items_GUIZE.Num != null)//区间免除规则
                        {

                            TimeSpan? ts_jfgz = Convert.ToDateTime(items_GUIZE.Time_end) - items_GUIZE.Time_start;
                            int days_jfgz = ts_jfgz.Value.Days;
                            for (int i_jfgz = 0; i_jfgz <= days_jfgz; i_jfgz++)
                            {
                                DateTime st_jfgz = Convert.ToDateTime(items_GUIZE.Time_start).AddDays(i_jfgz);
                                Stock_Money_DuiCun model_Gz = list.FirstOrDefault(n => n.RiQi == st_jfgz);
                                if (model_Gz != null)
                                {
                                    decimal? Jf = list.FirstOrDefault(n => n.RiQi == st_jfgz).JiFeiShuLiang = list.FirstOrDefault(n => n.RiQi == st_jfgz).JiFeiShuLiang - items_GUIZE.Num;
                                    if (Jf <= 0)
                                    {
                                        Jf = 0;
                                    }
                                    list.FirstOrDefault(n => n.RiQi == st_jfgz).FeiYong = list.FirstOrDefault(n => n.RiQi == st_jfgz).DanJia * Jf;

                                }

                            }
                        }

                    }
                }
            }



            return list;

        }//堆存算法
        public List<Stock_Money_RiShouRu> GetTallyBllList_RiShouRu(int id)//其他费用
        {
            List<Stock_Money_RiShouRu> list = new List<Stock_Money_RiShouRu>();//获取每天进库数量临时表
            C_TB_HC_GOODSBILL Model_GOODSBILL = db.C_TB_HC_GOODSBILL.FirstOrDefault(n => n.ID == id);//查找票货
            C_TB_HC_CONTRACT Model_CONTRACT = db.C_TB_HC_CONTRACT.FirstOrDefault(n => n.Guid == Model_GOODSBILL.CONTRACT_Guid);//查找合同
            if (Model_CONTRACT != null)
            {
                List<C_TB_HC_CONSIGN> list_CONSIGN = db.C_TB_HC_CONSIGN.Where(n => n.GOODSBILL_ID == Model_GOODSBILL.ID).ToList();//查找委托
                List<C_TB_HS_TALLYBILL> list_TALLYBILL_hc = new List<C_TB_HS_TALLYBILL>();
                foreach (var items in list_CONSIGN)//获取票货下面所有理货单
                {
                    List<C_TB_HS_TALLYBILL> list_TALLYBILL = db.C_TB_HS_TALLYBILL.Where(n => n.CONSIGN_ID == items.ID).ToList();
                    foreach (var items_TallBill in list_TALLYBILL)
                    {
                        list_TALLYBILL_hc.Add(items_TallBill);
                    }
                }
                list_TALLYBILL_hc = list_TALLYBILL_hc.OrderBy(n => n.SIGNDATE).ToList();//将所有理货单排序
                List<C_TB_HS_TALLYBILL> list_TALLYBILL_In = list_TALLYBILL_hc.Where(n => n.CODE_OPSTYPE == "进库" && n.Type != "清场").ToList();
                List<C_TB_HS_TALLYBILL> list_TALLYBILL_Out = list_TALLYBILL_hc.Where(n => n.CODE_OPSTYPE == "出库" && n.Type != "清场").ToList();
                List<C_TB_HS_TALLYBILL> list_TALLYBILL_All = list_TALLYBILL_hc.Where(n => n.Type != "清场").ToList();
                List<C_TB_HC_CONTRACT_DETAILED> list_DETAILED = db.C_TB_HC_CONTRACT_DETAILED.Where(n => n.CONTRACT_Guid == Model_CONTRACT.Guid && n.FeiMuZhongLei != "堆存费").ToList();
                foreach (var items_ht in list_DETAILED)
                {
                    decimal? danjia = 0;
                    C_TB_ZHIXINGFEILV model_ZHIXINGFEILV = db.C_TB_ZHIXINGFEILV.FirstOrDefault(n => n.GoodsBillId == Model_GOODSBILL.ID && n.FeiMuZhongLei == items_ht.FeiMuZhongLei && n.Type == items_ht.Type);
                    if (model_ZHIXINGFEILV != null)//执行费率
                    {
                        danjia = model_ZHIXINGFEILV.FeiLv;
                    }
                    else
                    {
                        danjia = items_ht.DanJia;
                    }
                    if (items_ht.Type == "进库")
                    {
                        foreach (var items_lh_In in list_TALLYBILL_In)
                        {
                            Stock_Money_RiShouRu model = new Stock_Money_RiShouRu()
                            {
                                Date = items_lh_In.SIGNDATE,
                                FeiYong = danjia * items_lh_In.WEIGHT,
                                FeiMuZhongLei = items_ht.FeiMuZhongLei,
                                Type = items_ht.Type
                            };
                            list.Add(model);
                        }
                    }
                    if (items_ht.Type == "出库")
                    {
                        foreach (var items_lh_Out in list_TALLYBILL_Out)
                        {
                            Stock_Money_RiShouRu model = new Stock_Money_RiShouRu()
                            {
                                Date = items_lh_Out.SIGNDATE,
                                FeiYong = danjia * items_lh_Out.WEIGHT,
                                FeiMuZhongLei = items_ht.FeiMuZhongLei,
                                Type = items_ht.Type
                            };
                            list.Add(model);
                        }
                    }
                    if (string.IsNullOrEmpty(items_ht.Type.ToString()) || items_ht.Type == "全部")
                    {
                        foreach (var items_lh_All in list_TALLYBILL_All)
                        {
                            Stock_Money_RiShouRu model = new Stock_Money_RiShouRu()
                            {
                                Date = items_lh_All.SIGNDATE,
                                FeiYong = danjia * items_lh_All.WEIGHT,
                                FeiMuZhongLei = items_ht.FeiMuZhongLei,
                                Type = items_ht.Type
                            };
                            list.Add(model);
                        }
                    }
                    if (items_ht.Type == "计划量")
                    {
                        Stock_Money_RiShouRu model = new Stock_Money_RiShouRu()
                        {
                            Date = Model_GOODSBILL.CreatTime,
                            FeiYong = danjia * Model_GOODSBILL.PLANWEIGHT,
                            FeiMuZhongLei = items_ht.FeiMuZhongLei,
                            Type = items_ht.Type
                        };
                        list.Add(model);
                    }

                }


            }
            return list;

        }
        public ActionResult EveryDayFeiYongList(int id)
        {
            OperatorModel loginModel = OperatorProvider.Provider.GetCurrent();
            ViewBag.roleId = loginModel.RoleId;
            ViewBag.id = id;
            return View();
        }
        public object GetEveryDayFeiYongList(int limit, int offset, int id, string Time1, string Time2)
        {
            string Guid_Get = "";
           
            DateTime start;
            DateTime end;
            if (!string.IsNullOrEmpty(Time1))
            {
                start = Convert.ToDateTime(Time1 + " 00:00:00");
            }
            else
            {
                start = DateTime.Now.AddDays(1 - DateTime.Now.Day).Date;
            }
            if (!string.IsNullOrEmpty(Time2))
            {
                end = Convert.ToDateTime(Time2 + " 23:59:59");
            }
            else
            {
                end = DateTime.Now.AddDays(1 - DateTime.Now.Day).Date.AddMonths(1).AddSeconds(-1);
            }

            List<Stock_Money_EveryDayFeiYong> list_EveryDayFeiYong = new List<Stock_Money_EveryDayFeiYong>();
            C_TB_HC_GOODSBILL Model_GOODSBILL = db.C_TB_HC_GOODSBILL.FirstOrDefault(n => n.ID == id);//查找票货
            List<Stock_Money_FeiYong> List_DuiCun = GetTallyBllList_dc(id);
            List<Stock_Money_DuiCun> List_DuiCun_k = GetTallyBllList_dc_k(id);
            List<Stock_Money_RiShouRu> List_RiShouRu = GetTallyBllList_RiShouRu(id);
            TimeSpan? ts = new TimeSpan();
            ts = end - start;
            int days = ts.Value.Days;
            C_TB_HC_CONTRACT Model_CONTRACT = db.C_TB_HC_CONTRACT.FirstOrDefault(n => n.Guid == Model_GOODSBILL.CONTRACT_Guid);//查找合同
            if (Model_CONTRACT != null)
            {
                Guid_Get = Model_CONTRACT.Guid;
            }

            for (int i = 0; i <= days; i++)
            {
                string QiaTaFeiYong = "";
                DateTime? StartTime = Convert.ToDateTime(start.AddDays(i).ToDateString().Substring(0, 10) + " 00:00:00");
                DateTime? EndTime = Convert.ToDateTime(start.AddDays(i).ToDateString().Substring(0, 10) + " 23:59:59");
                List<Stock_Money_RiShouRu> list_GetList = List_RiShouRu.Where(n => n.Date >= StartTime && n.Date <= EndTime).ToList();
                foreach (var items_GetList in list_GetList)
                {
                    if (items_GetList.FeiMuZhongLei != "包干费" && items_GetList.FeiMuZhongLei != "堆存费" && items_GetList.FeiMuZhongLei != "运输费" && items_GetList.FeiMuZhongLei != "过磅费")
                    {
                        QiaTaFeiYong += "(" + items_GetList.FeiMuZhongLei.ToString() + ":" + items_GetList.FeiYong + ")";
                    }
                }

                Stock_Money_EveryDayFeiYong model = new Stock_Money_EveryDayFeiYong()
                {
                    Date = start.AddDays(i),
                    HuoDai = Model_GOODSBILL.C_GOODSAGENT_NAME,
                    HuoWu = Model_GOODSBILL.C_GOODS,
                    ChuanMing = Model_GOODSBILL.ShipName,
                    HangCi = Model_GOODSBILL.VGNO,
                    BaoGanFei = List_RiShouRu.Where(n => n.FeiMuZhongLei == "包干费" && n.Date >= StartTime && n.Date <= EndTime).Sum(n => n.FeiYong),
                    DuiCunFei = List_DuiCun.Where(n => n.date_Out >= StartTime && n.date_Out <= EndTime).Sum(n => n.FeiYong) + List_DuiCun_k.Where(n => n.RiQi >= StartTime && n.RiQi <= EndTime).Sum(n => n.FeiYong),
                    YunFei = List_RiShouRu.Where(n => n.FeiMuZhongLei == "运输费" && n.Date >= StartTime && n.Date <= EndTime).Sum(n => n.FeiYong),
                    GuoBangFei = List_RiShouRu.Where(n => n.FeiMuZhongLei == "过磅费" && n.Date >= StartTime && n.Date <= EndTime).Sum(n => n.FeiYong),
                    QiTaFeiYong = List_RiShouRu.Where(n => n.FeiMuZhongLei != "包干费" && n.FeiMuZhongLei != "堆存费" && n.FeiMuZhongLei != "运输费" && n.FeiMuZhongLei != "过磅费" && n.Date >= StartTime && n.Date <= EndTime).Sum(n => n.FeiYong).ToDecimal(2) + QiaTaFeiYong,



                };
                list_EveryDayFeiYong.Add(model);
            }
           
            int total = list_EveryDayFeiYong.Count();
            object rows = list_EveryDayFeiYong.OrderBy(n => n.Date).Skip(offset).Take(limit).AsQueryable();
            return Json(new { total = total, rows = rows }, JsonRequestBehavior.AllowGet);

        }
        public ActionResult RiBaoBiaoList()
        {
            OperatorModel loginModel = OperatorProvider.Provider.GetCurrent();
            ViewBag.roleId = loginModel.RoleId;
            return View();
        }
        public object GetRiBaoBiaoList(int limit, int offset, string Time,string Huowu,string HuoDai)
        {
            var wherelambda = ExtLinq.True<C_TB_HC_GOODSBILL>();
            if (!string.IsNullOrEmpty(Huowu))
            {
                wherelambda = wherelambda.And(t => t.C_GOODS.Contains(Huowu));
            }
            if (!string.IsNullOrEmpty(HuoDai))
            {
                wherelambda = wherelambda.And(t => t.C_GOODSAGENT_NAME.Contains(HuoDai));
            }
            if (string.IsNullOrEmpty(Time))
            {
                wherelambda = wherelambda.And(t => t.State!="已完成");
            }
            List<Stock_Money_RiBaoBiao> list_RiBaoBiao = new List<Stock_Money_RiBaoBiao>();
            string date = "";
            if (!string.IsNullOrEmpty(Time))
            {
                date = Time;
            }
            else
            {
                date = DateTime.Now.ToDateString();
            }
            OperatorModel loginModel = OperatorProvider.Provider.GetCurrent();
            wherelambda = wherelambda.And(t => t.YuanQuID == loginModel.YuanquID);
            C_Dic_YuanQu model_yuanqu = db.C_Dic_YuanQu.FirstOrDefault(n => n.ID == loginModel.YuanquID);
            DateTime starttime = Convert.ToDateTime(date + " 00:00:00");
            DateTime endtime = Convert.ToDateTime(date + " 23:59:59");
            List<C_GOODS> list_Goods = db.C_GOODS.OrderBy(n => n.ID).ToList();
            int count = db.Set<C_TB_HC_GOODSBILL>().Where(wherelambda).OrderByDescending(n => n.C_GOODS).Count();
            var list_GOODSBILL = db.Set<C_TB_HC_GOODSBILL>().Where(wherelambda).OrderByDescending(n => n.C_GOODS).Skip(offset).Take(limit).AsQueryable();
            foreach (var items_GoodBill in list_GOODSBILL)
                {
                    decimal? InCar = 0;
                    decimal? InN = 0;
                    decimal? InW = 0;
                    decimal? OutCar = 0;
                    decimal? OutN = 0;
                    decimal? OutW = 0;
                    decimal? zkuj = 0;
                    decimal? zkus = 0;
                    decimal? yuein = 0;
                    decimal? yueout = 0;
                    decimal? allin = 0;
                    decimal? allout = 0;
                    List<C_TB_HC_CONSIGN> list_CONSIGN = db.C_TB_HC_CONSIGN.Where(n => n.GOODSBILL_ID == items_GoodBill.ID).OrderBy(n => n.ID).ToList();
                    foreach (var item_Consign in list_CONSIGN)
                    {
                        int days = DateTime.DaysInMonth(Convert.ToInt32(starttime.Year), starttime.Month);
                        DateTime yuestart = Convert.ToDateTime(starttime.Year + "-" + starttime.Month + "-" + "01" + " 00:00:00");//得到月的第一天
                        List<C_TB_HS_TALLYBILL> list_TALLYBILL = db.C_TB_HS_TALLYBILL.Where(n => n.CONSIGN_ID == item_Consign.ID).OrderBy(n => n.ID).ToList();
                        List<C_TB_HS_TALLYBILL> list_TALLYBILL_1 = db.C_TB_HS_TALLYBILL.Where(n => n.CONSIGN_ID == item_Consign.ID && n.SIGNDATE >= starttime && n.SIGNDATE <= endtime).OrderBy(n => n.ID).ToList();

                        zkuj += list_TALLYBILL.Where(n => n.SIGNDATE <= endtime && (n.CODE_OPSTYPE == "进库")).Sum(n => n.AMOUNT);
                        zkuj -= list_TALLYBILL.Where(n => n.SIGNDATE <= endtime && (n.CODE_OPSTYPE == "出库")).Sum(n => n.AMOUNT);
                        zkus += list_TALLYBILL.Where(n => n.SIGNDATE <= endtime && (n.CODE_OPSTYPE == "进库")).Sum(n => n.WEIGHT);
                        zkus -= list_TALLYBILL.Where(n => n.SIGNDATE <= endtime && (n.CODE_OPSTYPE == "出库")).Sum(n => n.WEIGHT);
                        yuein += list_TALLYBILL.Where(n => n.SIGNDATE != null && (n.CODE_OPSTYPE == "进库" && n.SIGNDATE >= yuestart && n.SIGNDATE <= endtime)).Sum(n => n.WEIGHT);
                        yueout += list_TALLYBILL.Where(n => n.SIGNDATE != null && (n.CODE_OPSTYPE == "出库" && n.SIGNDATE >= yuestart && n.SIGNDATE <= endtime)).Sum(n => n.WEIGHT);
                        allin += list_TALLYBILL.Where(n => n.SIGNDATE <= endtime && (n.CODE_OPSTYPE == "进库")).Sum(n => n.WEIGHT);
                        allout += list_TALLYBILL.Where(n => n.SIGNDATE <= endtime && (n.CODE_OPSTYPE == "出库")).Sum(n => n.WEIGHT);

                        foreach (var item_TallBill in list_TALLYBILL_1)
                        {
                            if (item_TallBill.CODE_OPSTYPE == "进库")
                            {

                                InN += item_TallBill.AMOUNT;
                                InW += item_TallBill.WEIGHT;
                            }
                            if (item_TallBill.CODE_OPSTYPE == "出库")
                            {
                                OutN += item_TallBill.AMOUNT;
                                OutW += item_TallBill.WEIGHT;
                            }
                            if (item_TallBill.CODE_OPSTYPE == "进库")
                            {
                                InCar += item_TallBill.TRAINNUM;
                            }
                            if (item_TallBill.CODE_OPSTYPE == "出库")
                            {
                                OutCar += item_TallBill.TRAINNUM;
                            }
                        }

                    }
                    Stock_Money_RiBaoBiao model = new Stock_Money_RiBaoBiao()
                    {
                        ID= items_GoodBill.ID,
                        PiaoHuoBianMa = items_GoodBill.GBNO,
                        ChuanMing = items_GoodBill.ShipName,
                        TiDanHao= items_GoodBill.BLNO,
                        HuoDai = items_GoodBill.C_GOODSAGENT_NAME,
                        HuoZhong= items_GoodBill.C_GOODS,
                        TiDanCaiJi= items_GoodBill.PLANWEIGHT,
                        JianCeZhiShu= items_GoodBill.jcjs,
                        JianCeCaiJi= items_GoodBill.jccj,
                        InCar= InCar,
                        InN= InN,
                        InW= InW,
                        OutCar= OutCar,
                        OutN= OutN,
                        OutW= OutW,
                        zkuj= zkuj,
                        zkus= zkus,
                        yuein= yuein,
                        yueout= yueout,
                        allin= allin,
                        allout= allout,
                        Hangci = items_GoodBill.VGNO,
                    };
                    list_RiBaoBiao.Add(model);
                }
            int total = count;
            object rows = list_RiBaoBiao;
            return Json(new { total = total, rows = rows }, JsonRequestBehavior.AllowGet);
        }
    }
}
