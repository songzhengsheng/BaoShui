using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Migrations;
using System.IO;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using Main.HelpClass;
using MainBLL.Money;
using MainBLL.TallyBLL;
using MainBLL.ToOut;
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
    public class MoneyController : Controller
    {
        private Entities db = new Entities();
        // GET: ck/Money
        public ActionResult TallyBll_fy(int id)
        {
            ViewBag.id = id;
            return View();
        }
        public ActionResult TallyBll_dc(int id)
        {
            ViewBag.id = id;
            return View();
        }
        public ActionResult TallyBll_dc_k(int id)
        {
            ViewBag.id = id;
            return View();
        }
        [System.Web.Http.HttpGet]
        public object GetTallyBllList_fy(int id)
        {
            decimal? shuilv = 0;
            decimal? HuiShouLv = 0;
            List<Stock_Money> list = new List<Stock_Money>();
            int total = 0;
            var rows = list.ToList();
            //C_TB_HS_TALLYBILL Model_TALLYBILL = db.C_TB_HS_TALLYBILL.FirstOrDefault(n => n.ID == id);//查找理货单
            //C_TB_HC_CONSIGN Model_CONSIGN = db.C_TB_HC_CONSIGN.FirstOrDefault(n => n.ID == Model_TALLYBILL.CONSIGN_ID);//查找委托
            C_TB_HC_GOODSBILL Model_GOODSBILL = db.C_TB_HC_GOODSBILL.FirstOrDefault(n => n.ID == id);//查找票货
            if (Model_GOODSBILL.CONTRACT_Guid == null)
            {
                return Json(new { total = total, rows = rows }, JsonRequestBehavior.AllowGet);
            }
            C_TB_HC_CONTRACT Model_CONTRACT = db.C_TB_HC_CONTRACT.FirstOrDefault(n => n.Guid == Model_GOODSBILL.CONTRACT_Guid);//查找合同
            List<C_TB_HC_CONTRACT_DETAILED> list_DETAILED = db.C_TB_HC_CONTRACT_DETAILED.Where(n => n.CONTRACT_Guid == Model_CONTRACT.Guid).ToList();


            foreach (var items in list_DETAILED)
            {
                string KaiPiao_State = "";

                B_TB_KAIPIAOJILU Model_JilU = db.B_TB_KAIPIAOJILU.FirstOrDefault(n => n.GoodsBillId == id && n.FeiMuZhongLei == items.FeiMuZhongLei && n.Type == items.Type);
                if (Model_JilU != null)
                {
                    KaiPiao_State = "已提交";
                }
                else
                {
                    KaiPiao_State = "未提交";
                }


                C_TB_ZHIXINGFEILV model_ZHIXINGFEILV = db.C_TB_ZHIXINGFEILV.FirstOrDefault(n => n.FeiMuZhongLei == items.FeiMuZhongLei && n.Type == items.Type && n.GoodsBillId == id);

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
                List<C_TB_JIESUAN> list_JieSuan = db.C_TB_JIESUAN.Where(n => n.FeiMuZhongLei == items.FeiMuZhongLei && n.Type == type && n.GoodsBill_id == id).ToList();
                List<C_TB_CHENGBEN> list_ChengBen = db.C_TB_CHENGBEN.Where(n => n.FeiMuZhongLei == items.FeiMuZhongLei && n.Type == type && n.GoodsBill_id == id).ToList();
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
                List<C_TB_SHOUFEI> list_ShouFei = db.C_TB_SHOUFEI.Where(n => n.FeiMuZhongLei == items.FeiMuZhongLei && n.Type == items.Type && n.GoodsBill_id == id).ToList();
                decimal? feiyong = 0;
                decimal? ShuLiang = 0;
                //TimeSpan? ts = DateTime.Now - Model_TALLYBILL.SIGNDATE;//获取收费天数
                if (items.JiLiangDanWei.Contains("*天"))
                {
                    if (items.Type == "进库" || items.Type == "出库")
                    {
                        decimal? FeiYong, JiFeiShuLiang, duicun;
                        GetTallyBllList_hqdc(id, out FeiYong, out JiFeiShuLiang, out duicun);
                        feiyong = FeiYong.ToDecimal(2);
                        ShuLiang = JiFeiShuLiang;
                    }
                    else
                    {
                        decimal? FeiYong, JiFeiShuLiang, duicun;
                        GetTallyBllList_hqdc_k(id, out FeiYong, out JiFeiShuLiang, out duicun);
                        feiyong = FeiYong.ToDecimal(2);
                        ShuLiang = JiFeiShuLiang;
                    }
                }

                else
                {

                    decimal? FeiYong, shuLiang;
                    GetTallyBllList_hqqyfy(id, items.FeiMuZhongLei, items.Type, out FeiYong, out shuLiang);
                    ShuLiang = shuLiang;
                    feiyong = (shuLiang * ShiJiShuiLv).ToDecimal(2);

                }

                Stock_Money model = new Stock_Money()
                {
                    ID = Model_GOODSBILL.ID,
                    HuoMing = items.HuoMing,
                    DanJia = items.DanJia,
                    FMZhongLei = items.FeiMuZhongLei,
                    JiLiangDanWei = items.JiLiangDanWei,
                    BeiZhu = items.BeiZhu,
                    FeiYong = feiyong.ToDecimal(2),
                    MianCunQi = items.MianDuiCunTianShu,
                    ShuLiang = ShuLiang.ToDecimal(3),
                    GoodsBill_id = id,
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
                    KaiPiao_State = KaiPiao_State

                };
                list.Add(model);

            }
            Stock_Money model_hj = new Stock_Money()//合计
            {
                ID = Model_GOODSBILL.ID,
                HuoMing = "",
                DanJia = null,
                FMZhongLei = "合计",
                JiLiangDanWei = "",
                BeiZhu = "",
                FeiYong = list.Sum(n => n.FeiYong).ToDecimal(3),
                MianCunQi = null,
                ShuLiang = null,
                GoodsBill_id = id,
                Type = "",
                YiShou = list.Sum(n => n.YiShou).ToDecimal(2),
                WeiShou = list.Sum(n => n.WeiShou).ToDecimal(2),
                ShuiHouJinE = list.Sum(n => n.ShuiHouJinE).ToDecimal(2),
                KaiPiaoJinE = list.Sum(n => n.KaiPiaoJinE).ToDecimal(2),
                LaiKuanJinE = list.Sum(n => n.LaiKuanJinE).ToDecimal(2),
                WeiKaiPiaoJinE = list.Sum(n => n.WeiKaiPiaoJinE).ToDecimal(2),
                YinSHouYuE = list.Sum(n => n.YinSHouYuE).ToDecimal(2),
                HuiShouLv = null,
                WeiKaiPiaoShuiHou = list.Sum(n => n.WeiKaiPiaoShuiHou).ToDecimal(2),
                KaiPiaoShuiHou = list.Sum(n => n.KaiPiaoShuiHou).ToDecimal(2),
                WaiFuJinE = list.Sum(n => n.WaiFuJinE).ToDecimal(2),


            };
            list.Add(model_hj);
            total = list.Count();
            rows = list.ToList();
            return Json(new { total = total, rows = rows }, JsonRequestBehavior.AllowGet);

        }
        public object GetTallyBllList_fy_hj(int id, out decimal? Fyjehj, out decimal? Fyjehj_Sh, out decimal? Kpjehj_Sh)
        {

            Fyjehj = 0;//费用金额合计
            Fyjehj_Sh = 0;//税后金额合计
            Kpjehj_Sh = 0;//开票金额合计
            decimal? shuilv = 0;
            List<Stock_Money> list = new List<Stock_Money>();
            int total = 0;
            var rows = list.ToList();
            //C_TB_HS_TALLYBILL Model_TALLYBILL = db.C_TB_HS_TALLYBILL.FirstOrDefault(n => n.ID == id);//查找理货单
            //C_TB_HC_CONSIGN Model_CONSIGN = db.C_TB_HC_CONSIGN.FirstOrDefault(n => n.ID == Model_TALLYBILL.CONSIGN_ID);//查找委托
            C_TB_HC_GOODSBILL Model_GOODSBILL = db.C_TB_HC_GOODSBILL.FirstOrDefault(n => n.ID == id);//查找票货
            if (Model_GOODSBILL.CONTRACT_Guid == null)
            {
                return Json(new { total = total, rows = rows }, JsonRequestBehavior.AllowGet);
            }
            C_TB_HC_CONTRACT Model_CONTRACT = db.C_TB_HC_CONTRACT.FirstOrDefault(n => n.Guid == Model_GOODSBILL.CONTRACT_Guid);//查找合同
            if (Model_CONTRACT != null)
            {
                List<C_TB_HC_CONTRACT_DETAILED> list_DETAILED = db.C_TB_HC_CONTRACT_DETAILED.Where(n => n.CONTRACT_Guid == Model_CONTRACT.Guid).ToList();
                foreach (var items in list_DETAILED)
                {
                    C_TB_ZHIXINGFEILV model_ZHIXINGFEILV = db.C_TB_ZHIXINGFEILV.FirstOrDefault(n => n.FeiMuZhongLei == items.FeiMuZhongLei && n.Type == items.Type && n.GoodsBillId == id);

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
                    List<C_TB_JIESUAN> list_ShouFei = db.C_TB_JIESUAN.Where(n => n.FeiMuZhongLei == items.FeiMuZhongLei && n.Type == items.Type && n.GoodsBill_id == id).ToList();
                    decimal? feiyong = 0;
                    decimal? ShuLiang = 0;
                    //TimeSpan? ts = DateTime.Now - Model_TALLYBILL.SIGNDATE;//获取收费天数
                    if (items.JiLiangDanWei.Contains("*天"))
                    {
                        if (items.Type == "进库" || items.Type == "出库")
                        {
                            decimal? FeiYong, JiFeiShuLiang, duicun;
                            GetTallyBllList_hqdc(id, out FeiYong, out JiFeiShuLiang, out duicun);
                            feiyong = FeiYong.ToDecimal(2);
                            ShuLiang = JiFeiShuLiang;
                        }
                        else
                        {
                            decimal? FeiYong, JiFeiShuLiang, duicun;
                            GetTallyBllList_hqdc_k(id, out FeiYong, out JiFeiShuLiang, out duicun);
                            feiyong = FeiYong.ToDecimal(2);
                            ShuLiang = JiFeiShuLiang;
                        }

                    }
                    else
                    {
                        decimal? FeiYong, shuLiang;
                        GetTallyBllList_hqqyfy(id, items.FeiMuZhongLei, items.Type, out FeiYong, out shuLiang);
                        ShuLiang = shuLiang;
                        feiyong = (shuLiang * ShiJiShuiLv).ToDecimal(2);

                    }
                    Stock_Money model = new Stock_Money()
                    {
                        ID = Model_GOODSBILL.ID,
                        HuoMing = items.HuoMing,
                        DanJia = ShiJiShuiLv,
                        FMZhongLei = items.FeiMuZhongLei,
                        JiLiangDanWei = items.JiLiangDanWei,
                        BeiZhu = items.BeiZhu,
                        FeiYong = feiyong,
                        MianCunQi = items.MianDuiCunTianShu,
                        ShuLiang = ShuLiang,
                        GoodsBill_id = id,
                        Type = items.Type,
                        YiShou = list_ShouFei.Sum(n => n.KaiPiaoJinE),
                        WeiShou = feiyong - list_ShouFei.Sum(n => n.KaiPiaoJinE),
                        ShuiHouJinE = feiyong / Convert.ToDecimal((1 + (Convert.ToDecimal(items.ShuiE) * Convert.ToDecimal(0.01)))).ToDecimal(3),
                        KaiPiaoShuiHou = list_ShouFei.Sum(n => n.KaiPiaoJinE) / Convert.ToDecimal((1 + (Convert.ToDecimal(items.ShuiE) * Convert.ToDecimal(0.01)))).ToDecimal(3),

                    };
                    list.Add(model);

                }
                Fyjehj = list.Sum(n => n.FeiYong);
                Fyjehj_Sh = list.Sum(n => n.ShuiHouJinE);
                Kpjehj_Sh = list.Sum(n => n.KaiPiaoShuiHou);
            }




            total = list.Count();
            rows = list.ToList();
            return Json(new { total = total, rows = rows }, JsonRequestBehavior.AllowGet);

        }
        [System.Web.Http.HttpGet]
        public object GetTallyBllList_dc(int limit, int offset, int id, string Time1, string Time2)
        {
            List<Stock_Money_In> list_In = new List<Stock_Money_In>();//获取每天进库数量临时表
            List<Stock_Money_Out> list_Out = new List<Stock_Money_Out>();//获取每天出库数量临时表
            List<Stock_Money_FeiYong> list_FeiYong = new List<Stock_Money_FeiYong>();//最终前台绑定的表
            C_TB_HC_GOODSBILL Model_GOODSBILL = db.C_TB_HC_GOODSBILL.FirstOrDefault(n => n.ID == id);//查找票货
            C_TB_HC_CONTRACT Model_CONTRACT = db.C_TB_HC_CONTRACT.FirstOrDefault(n => n.Guid == Model_GOODSBILL.CONTRACT_Guid);//查找合同
            C_TB_HC_CONTRACT_DETAILED Model_DETAILED = db.C_TB_HC_CONTRACT_DETAILED.FirstOrDefault(n => n.CONTRACT_Guid == Model_CONTRACT.Guid && n.FeiMuZhongLei == "堆存费");
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
            else
            {
                danjia = Model_DETAILED.DanJia;
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
            if (!string.IsNullOrEmpty(Time1))
            {
                DateTime CreatTime_date1 = Convert.ToDateTime(Time1 + " 00:00:00");
                list_FeiYong = list_FeiYong.Where(n => n.date_Out >= CreatTime_date1).ToList();

            }
            if (!string.IsNullOrEmpty(Time2))
            {
                DateTime CreatTime2_date = Convert.ToDateTime(Time2 + " 23:59:59");
                list_FeiYong = list_FeiYong.Where(n => n.date_Out <= CreatTime2_date).ToList();

            }
            int total = list_FeiYong.Count();
            object rows = list_FeiYong.Skip(offset).Take(limit).AsQueryable();
            return Json(new { total = total, rows = rows }, JsonRequestBehavior.AllowGet);

        }//先进先出算法
        [System.Web.Http.HttpGet]
        public object GetTallyBllList_dc_k(int limit, int offset, int id, string Time1, string Time2)
        {
            C_TB_HC_GOODSBILL Model_GOODSBILL = db.C_TB_HC_GOODSBILL.FirstOrDefault(n => n.ID == id);//查找票货
            C_TB_HC_CONTRACT Model_CONTRACT = db.C_TB_HC_CONTRACT.FirstOrDefault(n => n.Guid == Model_GOODSBILL.CONTRACT_Guid);//查找合同
            C_TB_HC_CONTRACT_DETAILED Model_DETAILED = db.C_TB_HC_CONTRACT_DETAILED.FirstOrDefault(n => n.CONTRACT_Guid == Model_CONTRACT.Guid && n.FeiMuZhongLei == "堆存费");
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
            List<Stock_Money_DuiCun> list = new List<Stock_Money_DuiCun>();//将所有理货单排序
            C_TB_HS_TALLYBILL model_date = list_TALLYBILL_hc.FirstOrDefault(n => n.Type == "清场");
            list_TALLYBILL_hc = list_TALLYBILL_hc.OrderBy(n => n.SIGNDATE).ToList();
            int days = -1;
            TimeSpan? ts = new TimeSpan();
            TimeSpan? ts1 = new TimeSpan();
            if (list_TALLYBILL_hc.Count != 0)
            {
                if (!string.IsNullOrEmpty(Time1) && !string.IsNullOrEmpty(Time2))
                {
                    ts1 = Convert.ToDateTime(Time2) - Convert.ToDateTime(Time1);//获取循环天数
                }
                if (model_date != null && model_date.CreatTime != null)
                {
                    ts = model_date.CreatTime - list_TALLYBILL_hc[0].SIGNDATE;//获取循环天数
                }
                else
                {
                    ts = DateTime.Now - list_TALLYBILL_hc[0].SIGNDATE;//获取循环天数
                }


                days = ts.Value.Days+ts1.Value.Days;
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
            if (!string.IsNullOrEmpty(Time1))
            {
                DateTime CreatTime_date1 = Convert.ToDateTime(Time1 + " 00:00:00");
                list = list.Where(n => n.RiQi >= CreatTime_date1).ToList();

            }
            if (!string.IsNullOrEmpty(Time2))
            {
                DateTime CreatTime2_date = Convert.ToDateTime(Time2 + " 23:59:59");
                list = list.Where(n => n.RiQi <= CreatTime2_date).ToList();

            }
            int total = list.Count();
            object rows = list.OrderBy(n => n.RiQi).Skip(offset).Take(limit).AsQueryable();
            return Json(new { total = total, rows = rows }, JsonRequestBehavior.AllowGet);

        }//堆存算法
        public object GetTallyBllList_hqdc(int id, out decimal? FeiYong1, out decimal? JiFeiShuLiang1, out decimal? duicun)
        {
            List<Stock_Money_In> list_In = new List<Stock_Money_In>();//获取每天进库数量临时表
            List<Stock_Money_Out> list_Out = new List<Stock_Money_Out>();//获取每天出库数量临时表
            List<Stock_Money_FeiYong> list_FeiYong = new List<Stock_Money_FeiYong>();//最终前台绑定的表
            C_TB_HC_GOODSBILL Model_GOODSBILL = db.C_TB_HC_GOODSBILL.FirstOrDefault(n => n.ID == id);//查找票货
            C_TB_HC_CONTRACT Model_CONTRACT = db.C_TB_HC_CONTRACT.FirstOrDefault(n => n.Guid == Model_GOODSBILL.CONTRACT_Guid);//查找合同
            C_TB_HC_CONTRACT_DETAILED Model_DETAILED = db.C_TB_HC_CONTRACT_DETAILED.FirstOrDefault(n => n.CONTRACT_Guid == Model_CONTRACT.Guid && n.FeiMuZhongLei == "堆存费");
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
                else
                {
                    danjia = Model_DETAILED.DanJia;
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

            JiFeiShuLiang1 = 0;
            FeiYong1 = 0;
            duicun = list_FeiYong.Sum(n => n.DunTian);
            foreach (var items_Jf in list_FeiYong)
            {
                if (items_Jf.OutAllNum >= 0)
                {
                    JiFeiShuLiang1 += items_Jf.OutAllNum;
                }
                FeiYong1 += items_Jf.FeiYong;
            }

            return (FeiYong1);

        }//先进先出算法
        public object GetTallyBllList_hqdc_k(int id, out decimal? FeiYong1, out decimal? JiFeiShuLiang1, out decimal? duicun)
        {

            C_TB_HC_GOODSBILL Model_GOODSBILL = db.C_TB_HC_GOODSBILL.FirstOrDefault(n => n.ID == id);//查找票货
            C_TB_HC_CONTRACT Model_CONTRACT = db.C_TB_HC_CONTRACT.FirstOrDefault(n => n.Guid == Model_GOODSBILL.CONTRACT_Guid);//查找合同
            C_TB_HC_CONTRACT_DETAILED Model_DETAILED = db.C_TB_HC_CONTRACT_DETAILED.FirstOrDefault(n => n.CONTRACT_Guid == Model_CONTRACT.Guid && n.FeiMuZhongLei == "堆存费");
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
                List<Stock_Money_DuiCun> list = new List<Stock_Money_DuiCun>();//将所有理货单排序
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
                duicun = days;
                FeiYong1 = list.Sum(n => n.FeiYong);
                JiFeiShuLiang1 = list.Sum(n => n.ChuKu);
                return (FeiYong1);
            }
            else
            {
                duicun = 0;
                FeiYong1 = 0;
                JiFeiShuLiang1 = 0;
                return (FeiYong1);
            }




        }//堆存算法
        public decimal? GetTallyBllList_hqqyfy(int id, string FeiMuZhongLei, string Type, out decimal? FeiYong, out decimal? ShuLiang)
        {

            FeiYong = 0;//费用
            ShuLiang = 0;//数量
            C_TB_HC_GOODSBILL Model_GOODSBILL = db.C_TB_HC_GOODSBILL.FirstOrDefault(n => n.ID == id);//查找票货
            C_TB_HC_CONTRACT Model_CONTRACT = db.C_TB_HC_CONTRACT.FirstOrDefault(n => n.Guid == Model_GOODSBILL.CONTRACT_Guid);//查找合同
            C_TB_HC_CONTRACT_DETAILED Model_DETAILED = new C_TB_HC_CONTRACT_DETAILED();
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
            if (Type == "全部")
            {
                Model_DETAILED = db.C_TB_HC_CONTRACT_DETAILED.FirstOrDefault(n => n.CONTRACT_Guid == Model_CONTRACT.Guid && n.FeiMuZhongLei == FeiMuZhongLei);
            }
            if (Type == "计划量")
            {
                ShuLiang = Model_GOODSBILL.PLANWEIGHT;
                FeiYong = ShuLiang * danjia.ToDecimal(2);
                return (FeiYong);
            }
            else
            {
                Model_DETAILED = db.C_TB_HC_CONTRACT_DETAILED.FirstOrDefault(n => n.CONTRACT_Guid == Model_CONTRACT.Guid && n.FeiMuZhongLei == FeiMuZhongLei && n.Type == Type);
            }
            if (Model_DETAILED == null)
            {
                return (FeiYong);
            }

            List<C_TB_HC_CONSIGN> list_CONSIGN = db.C_TB_HC_CONSIGN.Where(n => n.GOODSBILL_ID == Model_GOODSBILL.ID).ToList();//查找委托
            List<C_TB_HS_TALLYBILL> list_TALLYBILL_hc = new List<C_TB_HS_TALLYBILL>();//获取货票下面所有理货单
            foreach (var items in list_CONSIGN)
            {
                List<C_TB_HS_TALLYBILL> list_TALLYBILL = new List<C_TB_HS_TALLYBILL>();
                if (Model_DETAILED.Type != "全部" && !string.IsNullOrEmpty(Model_DETAILED.Type))
                {
                    list_TALLYBILL = db.C_TB_HS_TALLYBILL.Where(n => n.CONSIGN_ID == items.ID && n.CODE_OPSTYPE == Model_DETAILED.Type && n.Type != "清场").ToList();
                }
                else
                {
                    list_TALLYBILL = db.C_TB_HS_TALLYBILL.Where(n => n.CONSIGN_ID == items.ID && n.Type != "清场").ToList();
                }
                foreach (var items_TallBill in list_TALLYBILL)
                {
                    if (Model_DETAILED.JiLiangDanWei.Contains("箱"))
                    {
                        if (items_TallBill.XIANGSHU != 0 && items_TallBill.XIANGSHU != null)
                        {
                            ShuLiang += items_TallBill.XIANGSHU;
                        }
                        else
                        {
                            ShuLiang += items_TallBill.WEIGHT;
                        }
                    }
                    else
                    {
                        ShuLiang += items_TallBill.WEIGHT;
                    }

                }


            }
            FeiYong = (danjia * ShuLiang).ToDecimal(4);


            return (FeiYong);

        }
        public ActionResult MoneyGOODSBILLList()
        {
            OperatorModel loginModel = OperatorProvider.Provider.GetCurrent();
            ViewBag.roleId = loginModel.RoleId;
            return View();
        }
        public ActionResult MoneyGOODSBILLList_wc()
        {
            OperatorModel loginModel = OperatorProvider.Provider.GetCurrent();
            ViewBag.roleId = loginModel.RoleId;
            return View();
        }
        [HttpPost]
        public JsonResult shenhe_jifei(int id)
        {

            try
            {
                C_TB_HC_GOODSBILL model_GoodsBill = db.C_TB_HC_GOODSBILL.Find(id) ?? new C_TB_HC_GOODSBILL();
                model_GoodsBill.State_JiFei = "已完成";
                db.SaveChanges();

                return Json(new AjaxResult { state = ResultType.success.ToString(), message = "成功" }.ToJson());
            }
            catch (Exception e)
            {
                return Json(new AjaxResult { state = ResultType.error.ToString(), message = e.Message }.ToJson());
            }
        }
        [HttpPost]
        public JsonResult shenhe_jifei_jf(int id)
        {
            C_TB_HC_GOODSBILL model_GoodsBill = db.C_TB_HC_GOODSBILL.Find(id) ?? new C_TB_HC_GOODSBILL();
            model_GoodsBill.State_FeiShou = "计费已完成";
            BS_FangHuoNeiBuShenPi model_sh = db.BS_FangHuoNeiBuShenPi.FirstOrDefault(n => n.GoodsBillId == id && n.State_JiFei != "计费已完成");
            if (model_sh != null)
            {
                model_sh.State_JiFei = "计费已完成";
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
        public JsonResult shenhe_jiesuan(int id)
        {

            try
            {
                C_TB_HC_GOODSBILL model_GoodsBill = db.C_TB_HC_GOODSBILL.Find(id) ?? new C_TB_HC_GOODSBILL();
                model_GoodsBill.State_JieSuan = "已完成";
                db.SaveChanges();

                return Json(new AjaxResult { state = ResultType.success.ToString(), message = "成功" }.ToJson());
            }
            catch (Exception e)
            {
                return Json(new AjaxResult { state = ResultType.error.ToString(), message = e.Message }.ToJson());
            }
        }
        public ActionResult AddMoney(int id, int GoodsBill_id, string Type, string FeiMuZhongLei)
        {
            C_TB_SHOUFEI model = db.C_TB_SHOUFEI.Find(id) ?? new C_TB_SHOUFEI();
            ViewBag.GoodsBill_id = GoodsBill_id;
            ViewBag.Type = Type;
            ViewBag.FeiMuZhongLei = FeiMuZhongLei;

            return View(model);
        }
        [OperationLog(OperationLogAttribute.Operatetype.添加或编辑, OperationLogAttribute.ImportantLevel.一般操作, "添加编辑收费信息")]
        [HttpPost]
        public JsonResult AddMoney(C_TB_SHOUFEI model)
        {
            if (string.IsNullOrEmpty(model.Type) || model.Type == "null")
            {
                model.Type = "全部";
            }
            OperatorModel loginModel = OperatorProvider.Provider.GetCurrent();
            db.Set<C_TB_SHOUFEI>().AddOrUpdate(model);
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
        public object GetFeiTongLis(int GoodsBill_id, string FeiMuZhongLei, string Type)
        {
            if (string.IsNullOrEmpty(Type) || Type == "null")
            {
                Type = "全部";
            }

            List<C_TB_SHOUFEI> list = db.C_TB_SHOUFEI.Where(n => n.FeiMuZhongLei == FeiMuZhongLei && n.Type == Type && n.GoodsBill_id == GoodsBill_id).ToList();
            int total = list.Count();
            object rows = list.AsQueryable();
            return Json(new { total = total, rows = rows }, JsonRequestBehavior.AllowGet);
        }
        public object GetJiFeiGuiZe(int GoodsBill_id, string FeiMuZhongLei, string Type)
        {
            if (string.IsNullOrEmpty(Type) || Type == "null")
            {
                Type = "全部";
            }

            List<C_DIC_GUIZE> list = db.C_DIC_GUIZE.Where(n => n.FeiMuZhongLei == FeiMuZhongLei && n.Type == Type && n.GoodsBillId == GoodsBill_id).ToList();
            int total = list.Count();
            object rows = list.AsQueryable();
            return Json(new { total = total, rows = rows }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult MoneyGOODSBILLList_JieSuan()
        {
            OperatorModel loginModel = OperatorProvider.Provider.GetCurrent();
            ViewBag.roleId = loginModel.RoleId;
            return View();
        }
        public ActionResult MoneyGOODSBILLList_JieSuan_wc()
        {
            OperatorModel loginModel = OperatorProvider.Provider.GetCurrent();
            ViewBag.roleId = loginModel.RoleId;
            return View();
        }
        public object GetFeiTongLis_JieSuan(int GoodsBill_id, string FeiMuZhongLei, string Type)
        {
            if (string.IsNullOrEmpty(Type) || Type == "null")
            {
                Type = "全部";
            }

            List<C_TB_JIESUAN> list = db.C_TB_JIESUAN.Where(n => n.FeiMuZhongLei == FeiMuZhongLei && n.Type == Type && n.GoodsBill_id == GoodsBill_id).ToList();
            int total = list.Count();
            object rows = list.AsQueryable();
            return Json(new { total = total, rows = rows }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult AddMoney_JieSuan(int id, int GoodsBill_id, string Type, string FeiMuZhongLei)
        {
            C_TB_HC_GOODSBILL Model_GOODSBILL = db.C_TB_HC_GOODSBILL.FirstOrDefault(n => n.ID == GoodsBill_id);//查找票货
            C_TB_JIESUAN model = db.C_TB_JIESUAN.Find(id) ?? new C_TB_JIESUAN();
            ViewBag.GoodsBill_id = GoodsBill_id;
            ViewBag.Type = Type;
            ViewBag.FeiMuZhongLei = FeiMuZhongLei;
            if (id == 0)
            {
                ViewBag.KaiPianDanWei = Model_GOODSBILL.C_GOODSAGENT_NAME;
            }
            else
            {
                ViewBag.KaiPianDanWei = model.KaiPianDanWei;
            }
            List<C_GOODSAGENT> list_GOODSAGENT = db.C_GOODSAGENT.OrderBy(n => n.Name).ToList();//货代
            List<SelectListItem> emplist_GOODSAGENT = SelectHelp.CreateSelect<C_GOODSAGENT>(list_GOODSAGENT, "FullName", "FullName", null);
            ViewData["GOODSAGENT_List"] = new SelectList(emplist_GOODSAGENT, "Value", "Text", "是");
            return View(model);
        }
        [OperationLog(OperationLogAttribute.Operatetype.添加或编辑, OperationLogAttribute.ImportantLevel.一般操作, "结算信息")]
        [HttpPost]
        public JsonResult AddMoney_JieSuan(C_TB_JIESUAN model)
        {
            if (string.IsNullOrEmpty(model.Type) || model.Type == "null")
            {
                model.Type = "全部";
            }
            OperatorModel loginModel = OperatorProvider.Provider.GetCurrent();
            db.Set<C_TB_JIESUAN>().AddOrUpdate(model);
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
        [HttpGet]
        public object GetGOODSBILLList(int limit, int offset, string GBNO, string C_GOODSAGENT_NAME, string CreatTime, string CreatTime1, string BLNO, string C_GOODS, string HuoZhu, string ShipName, string VGNO)
        {

            var wherelambda = ExtLinq.True<C_TB_HC_GOODSBILL>();
            OperatorModel loginModel = OperatorProvider.Provider.GetCurrent();
            wherelambda = wherelambda.And(t => t.YuanQuID == loginModel.YuanquID);
            wherelambda = wherelambda.And(t => t.State != "待审核");
            wherelambda = wherelambda.And(t => t.State != "被驳回");
            wherelambda = wherelambda.And(t => t.State_JiFei != "已完成" || t.State_JiFei == null);
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
                GetTallyBllList_fy_hj(Convert.ToInt32(itsms_GoodsBill.ID), out decimal? Fyjehj, out decimal? Fyjehj_Sh, out decimal? Kpjehj_Sh);
                itsms_GoodsBill.Fyjehj = Fyjehj.ToDecimal(2);
                itsms_GoodsBill.KunCun = KuCun.ToString();
                itsms_GoodsBill.KunCunW = KuCunW.ToDecimal(3).ToString();
                List<C_TB_JIESUAN> list_JieSuan = db.C_TB_JIESUAN.Where(n => n.GoodsBill_id == itsms_GoodsBill.ID).OrderBy(n => n.ID).ToList();
                itsms_GoodsBill.Lkjehj = list_JieSuan.Sum(n => n.LaiKuanJinE).ToDecimal(2);
                itsms_GoodsBill.Sryk = (Fyjehj - list_JieSuan.Sum(n => n.LaiKuanJinE)).ToDecimal(2);
                List<C_TB_CHENGBEN> list_ChengBen = db.C_TB_CHENGBEN.Where(n => n.GoodsBill_id == itsms_GoodsBill.ID).OrderBy(n => n.ID).ToList();
                itsms_GoodsBill.ChengBenJinE = list_ChengBen.Sum(n => n.ChengBenJinE).ToDecimal(2);
                itsms_GoodsBill.LiRunZongE = (Fyjehj - list_ChengBen.Sum(n => n.ChengBenJinE)).ToDecimal(2);
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
                itsms_GoodsBill.ShiJiJInKu = ShiJiJInKu.ToDecimal(3);
                itsms_GoodsBill.ShiJiChuKu = ShiJiChuKu.ToDecimal(3);
                itsms_GoodsBill.KaiPiaoJineHJ = list_JieSuan.Sum(n => n.KaiPiaoJinE).ToDecimal(2);
                itsms_GoodsBill.KaiPiaoShuiHouHJ = Kpjehj_Sh.ToDecimal(2);
                itsms_GoodsBill.WeiKaiPiaoJineHJ = (Fyjehj - list_JieSuan.Sum(n => n.KaiPiaoJinE)).ToDecimal(2);
                itsms_GoodsBill.WeiKaiPiaoShuiHouHJ = (Fyjehj_Sh - Kpjehj_Sh).ToDecimal(2);
                itsms_GoodsBill.YinShouZhangKuanYue = (list_JieSuan.Sum(n => n.LaiKuanJinE) - list_JieSuan.Sum(n => n.KaiPiaoJinE)).ToDecimal(2);
                itsms_GoodsBill.LiRunZongE = (Fyjehj - list_ChengBen.Sum(n => n.ChengBenJinE)).ToDecimal(2);
                itsms_GoodsBill.YikaiPiaoYinShouYuE = (list_JieSuan.Sum(n => n.KaiPiaoJinE) - list_JieSuan.Sum(n => n.LaiKuanJinE)).ToDecimal(2);
                itsms_GoodsBill.WeiKaiPiaoYingShouYuE = ((Fyjehj - list_JieSuan.Sum(n => n.KaiPiaoJinE)) - list_JieSuan.Sum(n => n.YuShouJinE)).ToDecimal(2);
            }

            int total = count;
            object rows = list;
            return Json(new { total = total, rows = rows }, JsonRequestBehavior.AllowGet);

        }
        [HttpGet]
        public object GetGOODSBILLList_jiesuan(int limit, int offset, string GBNO, string C_GOODSAGENT_NAME, string CreatTime, string CreatTime1, string BLNO, string C_GOODS, string HuoZhu, string ShipName, string VGNO)
        {

            var wherelambda = ExtLinq.True<C_TB_HC_GOODSBILL>();
            OperatorModel loginModel = OperatorProvider.Provider.GetCurrent();
            wherelambda = wherelambda.And(t => t.YuanQuID == loginModel.YuanquID);
            wherelambda = wherelambda.And(t => t.State != "待审核");
            wherelambda = wherelambda.And(t => t.State != "被驳回");
            wherelambda = wherelambda.And(t => t.State_JieSuan != "已完成" || t.State_JieSuan == null);
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
                GetTallyBllList_fy_hj(Convert.ToInt32(itsms_GoodsBill.ID), out decimal? Fyjehj, out decimal? Fyjehj_Sh, out decimal? Kpjehj_Sh);
                itsms_GoodsBill.Fyjehj = Fyjehj.ToDecimal(2);
                itsms_GoodsBill.KunCun = KuCun.ToString();
                itsms_GoodsBill.KunCunW = KuCunW.ToDecimal(3).ToString();
                List<C_TB_JIESUAN> list_JieSuan = db.C_TB_JIESUAN.Where(n => n.GoodsBill_id == itsms_GoodsBill.ID).OrderBy(n => n.ID).ToList();
                itsms_GoodsBill.Lkjehj = list_JieSuan.Sum(n => n.LaiKuanJinE).ToDecimal(2);
                itsms_GoodsBill.Sryk = (Fyjehj - list_JieSuan.Sum(n => n.LaiKuanJinE)).ToDecimal(2);
                List<C_TB_CHENGBEN> list_ChengBen = db.C_TB_CHENGBEN.Where(n => n.GoodsBill_id == itsms_GoodsBill.ID).OrderBy(n => n.ID).ToList();
                itsms_GoodsBill.ChengBenJinE = list_ChengBen.Sum(n => n.ChengBenJinE).ToDecimal(2);
                itsms_GoodsBill.LiRunZongE = (Fyjehj - list_ChengBen.Sum(n => n.ChengBenJinE)).ToDecimal(2);
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
                itsms_GoodsBill.ShiJiJInKu = ShiJiJInKu.ToDecimal(3);
                itsms_GoodsBill.ShiJiChuKu = ShiJiChuKu.ToDecimal(3);
                itsms_GoodsBill.KaiPiaoJineHJ = list_JieSuan.Sum(n => n.KaiPiaoJinE).ToDecimal(2);
                itsms_GoodsBill.KaiPiaoShuiHouHJ = Kpjehj_Sh.ToDecimal(2);
                itsms_GoodsBill.WeiKaiPiaoJineHJ = (Fyjehj - list_JieSuan.Sum(n => n.KaiPiaoJinE)).ToDecimal(2);
                itsms_GoodsBill.WeiKaiPiaoShuiHouHJ = (Fyjehj_Sh - Kpjehj_Sh).ToDecimal(2);
                itsms_GoodsBill.YinShouZhangKuanYue = (list_JieSuan.Sum(n => n.LaiKuanJinE) - list_JieSuan.Sum(n => n.KaiPiaoJinE)).ToDecimal(2);
                itsms_GoodsBill.LiRunZongE = (Fyjehj - list_ChengBen.Sum(n => n.ChengBenJinE)).ToDecimal(2);
                itsms_GoodsBill.YikaiPiaoYinShouYuE = (list_JieSuan.Sum(n => n.KaiPiaoJinE) - list_JieSuan.Sum(n => n.LaiKuanJinE)).ToDecimal(2);
                itsms_GoodsBill.WeiKaiPiaoYingShouYuE = ((Fyjehj - list_JieSuan.Sum(n => n.KaiPiaoJinE)) - list_JieSuan.Sum(n => n.YuShouJinE)).ToDecimal(2);
            }

            int total = count;
            object rows = list;
            return Json(new { total = total, rows = rows }, JsonRequestBehavior.AllowGet);

        }
        [HttpGet]
        public object GetGOODSBILLList_wc(int limit, int offset, string GBNO, string C_GOODSAGENT_NAME, string CreatTime, string CreatTime1, string BLNO, string C_GOODS, string HuoZhu, string ShipName, string VGNO)
        {

            var wherelambda = ExtLinq.True<C_TB_HC_GOODSBILL>();
            OperatorModel loginModel = OperatorProvider.Provider.GetCurrent();
            wherelambda = wherelambda.And(t => t.YuanQuID == loginModel.YuanquID);
            wherelambda = wherelambda.And(t => t.State != "待审核");
            wherelambda = wherelambda.And(t => t.State != "被驳回");
            wherelambda = wherelambda.And(t => t.State_JiFei == "已完成");

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
                GetTallyBllList_fy_hj(Convert.ToInt32(itsms_GoodsBill.ID), out decimal? Fyjehj, out decimal? Fyjehj_Sh, out decimal? Kpjehj_Sh);
                itsms_GoodsBill.Fyjehj = Fyjehj.ToDecimal(2);
                itsms_GoodsBill.KunCun = KuCun.ToString();
                itsms_GoodsBill.KunCunW = KuCunW.ToDecimal(3).ToString();
                List<C_TB_JIESUAN> list_JieSuan = db.C_TB_JIESUAN.Where(n => n.GoodsBill_id == itsms_GoodsBill.ID).OrderBy(n => n.ID).ToList();
                itsms_GoodsBill.Lkjehj = list_JieSuan.Sum(n => n.LaiKuanJinE).ToDecimal(2);
                itsms_GoodsBill.Sryk = (Fyjehj - list_JieSuan.Sum(n => n.LaiKuanJinE)).ToDecimal(2);
                List<C_TB_CHENGBEN> list_ChengBen = db.C_TB_CHENGBEN.Where(n => n.GoodsBill_id == itsms_GoodsBill.ID).OrderBy(n => n.ID).ToList();
                itsms_GoodsBill.ChengBenJinE = list_ChengBen.Sum(n => n.ChengBenJinE).ToDecimal(2);
                itsms_GoodsBill.LiRunZongE = (Fyjehj - list_ChengBen.Sum(n => n.ChengBenJinE)).ToDecimal(2);
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
                itsms_GoodsBill.ShiJiJInKu = ShiJiJInKu.ToDecimal(3);
                itsms_GoodsBill.ShiJiChuKu = ShiJiChuKu.ToDecimal(3);
                itsms_GoodsBill.KaiPiaoJineHJ = list_JieSuan.Sum(n => n.KaiPiaoJinE).ToDecimal(2);
                itsms_GoodsBill.KaiPiaoShuiHouHJ = Kpjehj_Sh.ToDecimal(2);
                itsms_GoodsBill.WeiKaiPiaoJineHJ = (Fyjehj - list_JieSuan.Sum(n => n.KaiPiaoJinE)).ToDecimal(2);
                itsms_GoodsBill.WeiKaiPiaoShuiHouHJ = (Fyjehj_Sh - Kpjehj_Sh).ToDecimal(2);
                itsms_GoodsBill.YinShouZhangKuanYue = (list_JieSuan.Sum(n => n.LaiKuanJinE) - list_JieSuan.Sum(n => n.KaiPiaoJinE)).ToDecimal(2);
                itsms_GoodsBill.LiRunZongE = (Fyjehj - list_ChengBen.Sum(n => n.ChengBenJinE)).ToDecimal(2);
                itsms_GoodsBill.YikaiPiaoYinShouYuE = (list_JieSuan.Sum(n => n.KaiPiaoJinE) - list_JieSuan.Sum(n => n.LaiKuanJinE)).ToDecimal(2);
                itsms_GoodsBill.WeiKaiPiaoYingShouYuE = ((Fyjehj - list_JieSuan.Sum(n => n.KaiPiaoJinE)) - list_JieSuan.Sum(n => n.YuShouJinE)).ToDecimal(2);
            }

            int total = count;
            object rows = list;
            return Json(new { total = total, rows = rows }, JsonRequestBehavior.AllowGet);

        }
        [HttpGet]
        public object GetGOODSBILLList_wc_jiesuan(int limit, int offset, string GBNO, string C_GOODSAGENT_NAME, string CreatTime, string CreatTime1, string BLNO, string C_GOODS, string HuoZhu, string ShipName, string VGNO)
        {

            var wherelambda = ExtLinq.True<C_TB_HC_GOODSBILL>();
            OperatorModel loginModel = OperatorProvider.Provider.GetCurrent();
            wherelambda = wherelambda.And(t => t.YuanQuID == loginModel.YuanquID);
            wherelambda = wherelambda.And(t => t.State != "待审核");
            wherelambda = wherelambda.And(t => t.State != "被驳回");
            wherelambda = wherelambda.And(t => t.State_JieSuan == "已完成");

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
                GetTallyBllList_fy_hj(Convert.ToInt32(itsms_GoodsBill.ID), out decimal? Fyjehj, out decimal? Fyjehj_Sh, out decimal? Kpjehj_Sh);
                itsms_GoodsBill.Fyjehj = Fyjehj.ToDecimal(2);
                itsms_GoodsBill.KunCun = KuCun.ToString();
                itsms_GoodsBill.KunCunW = KuCunW.ToDecimal(3).ToString();
                List<C_TB_JIESUAN> list_JieSuan = db.C_TB_JIESUAN.Where(n => n.GoodsBill_id == itsms_GoodsBill.ID).OrderBy(n => n.ID).ToList();
                itsms_GoodsBill.Lkjehj = list_JieSuan.Sum(n => n.LaiKuanJinE).ToDecimal(2);
                itsms_GoodsBill.Sryk = (Fyjehj - list_JieSuan.Sum(n => n.LaiKuanJinE)).ToDecimal(2);
                List<C_TB_CHENGBEN> list_ChengBen = db.C_TB_CHENGBEN.Where(n => n.GoodsBill_id == itsms_GoodsBill.ID).OrderBy(n => n.ID).ToList();
                itsms_GoodsBill.ChengBenJinE = list_ChengBen.Sum(n => n.ChengBenJinE).ToDecimal(2);
                itsms_GoodsBill.LiRunZongE = (Fyjehj - list_ChengBen.Sum(n => n.ChengBenJinE)).ToDecimal(2);
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
                itsms_GoodsBill.ShiJiJInKu = ShiJiJInKu.ToDecimal(3);
                itsms_GoodsBill.ShiJiChuKu = ShiJiChuKu.ToDecimal(3);
                itsms_GoodsBill.KaiPiaoJineHJ = list_JieSuan.Sum(n => n.KaiPiaoJinE).ToDecimal(2);
                itsms_GoodsBill.KaiPiaoShuiHouHJ = Kpjehj_Sh.ToDecimal(2);
                itsms_GoodsBill.WeiKaiPiaoJineHJ = (Fyjehj - list_JieSuan.Sum(n => n.KaiPiaoJinE)).ToDecimal(2);
                itsms_GoodsBill.WeiKaiPiaoShuiHouHJ = (Fyjehj_Sh - Kpjehj_Sh).ToDecimal(2);
                itsms_GoodsBill.YinShouZhangKuanYue = (list_JieSuan.Sum(n => n.LaiKuanJinE) - list_JieSuan.Sum(n => n.KaiPiaoJinE)).ToDecimal(2);
                itsms_GoodsBill.LiRunZongE = (Fyjehj - list_ChengBen.Sum(n => n.ChengBenJinE)).ToDecimal(2);
                itsms_GoodsBill.YikaiPiaoYinShouYuE = (list_JieSuan.Sum(n => n.KaiPiaoJinE) - list_JieSuan.Sum(n => n.LaiKuanJinE)).ToDecimal(2);
                itsms_GoodsBill.WeiKaiPiaoYingShouYuE = ((Fyjehj - list_JieSuan.Sum(n => n.KaiPiaoJinE)) - list_JieSuan.Sum(n => n.YuShouJinE)).ToDecimal(2);
            }

            int total = count;
            object rows = list;
            return Json(new { total = total, rows = rows }, JsonRequestBehavior.AllowGet);

        }
        public ActionResult MoneyGOODSBILLList_cb()//成本管理
        {
            OperatorModel loginModel = OperatorProvider.Provider.GetCurrent();
            ViewBag.roleId = loginModel.RoleId;
            return View();
        }
        [HttpGet]
        public object GetFeiTongLis_cb(int id)
        {

            List<C_TB_CHENGBEN> list = db.C_TB_CHENGBEN.Where(n => n.GoodsBill_id == id).ToList();
            int total = list.Count();
            object rows = list.AsQueryable();
            return Json(new { total = total, rows = rows }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult AddMoney_cb(int id, int GoodsBill_id, string FeiMuZhongLei)
        {
            C_TB_CHENGBEN model = db.C_TB_CHENGBEN.Find(id) ?? new C_TB_CHENGBEN();
            ViewBag.GoodsBill_id = GoodsBill_id;
            ViewBag.FeiMuZhongLei = FeiMuZhongLei;
            return View(model);
        }

        [OperationLog(OperationLogAttribute.Operatetype.添加或编辑, OperationLogAttribute.ImportantLevel.一般操作, "成本信息")]
        [HttpPost]
        public JsonResult AddMoney_cb(C_TB_CHENGBEN model)
        {
            string type = "";
            if (string.IsNullOrEmpty(model.Type))
            {
                type = "全部";
            }
            else
            {
                type = model.Type;
            }
            C_TB_HC_GOODSBILL Model_GOODSBILL = db.C_TB_HC_GOODSBILL.FirstOrDefault(n => n.ID == model.GoodsBill_id);//查找票货
            C_TB_HC_CONTRACT Model_CONTRACT = db.C_TB_HC_CONTRACT.FirstOrDefault(n => n.Guid == Model_GOODSBILL.CONTRACT_Guid);//查找合同
            C_TB_HC_CONTRACT_DETAILED Model_DETAILED = db.C_TB_HC_CONTRACT_DETAILED.FirstOrDefault(n => n.CONTRACT_Guid == Model_CONTRACT.Guid && n.FeiMuZhongLei == model.FeiMuZhongLei && n.ChengBenJiFeiYiJu == type);
            List<C_TB_HC_CONSIGN> list_CONSIGN = db.C_TB_HC_CONSIGN.Where(n => n.GOODSBILL_ID == Model_GOODSBILL.ID).ToList();//查找委托
            List<C_TB_HS_TALLYBILL> list_TALLYBILL_hc = new List<C_TB_HS_TALLYBILL>();


            db.Set<C_TB_CHENGBEN>().AddOrUpdate(model);
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
        public JsonResult WorkNumToExcel_ZhangDan(int GoodsBill_Id)
        {
            decimal? duicun = 0;
            decimal? shuilv = 0;
            decimal? HuiShouLv = 0;
            List<Stock_Money> list = new List<Stock_Money>();
            int total = 0;
            var rows = list.ToList();
            //C_TB_HS_TALLYBILL Model_TALLYBILL = db.C_TB_HS_TALLYBILL.FirstOrDefault(n => n.ID == id);//查找理货单
            //C_TB_HC_CONSIGN Model_CONSIGN = db.C_TB_HC_CONSIGN.FirstOrDefault(n => n.ID == Model_TALLYBILL.CONSIGN_ID);//查找委托
            C_TB_HC_GOODSBILL Model_GOODSBILL = db.C_TB_HC_GOODSBILL.FirstOrDefault(n => n.ID == GoodsBill_Id);//查找票货
            if (Model_GOODSBILL.CONTRACT_Guid == null)
            {
                return Json(new { total = total, rows = rows }, JsonRequestBehavior.AllowGet);
            }
            C_TB_HC_CONTRACT Model_CONTRACT = db.C_TB_HC_CONTRACT.FirstOrDefault(n => n.Guid == Model_GOODSBILL.CONTRACT_Guid);//查找合同
            List<C_TB_HC_CONTRACT_DETAILED> list_DETAILED = db.C_TB_HC_CONTRACT_DETAILED.Where(n => n.CONTRACT_Guid == Model_CONTRACT.Guid).ToList();


            foreach (var items in list_DETAILED)
            {
                string type = "";
                if (string.IsNullOrEmpty(items.Type))
                {
                    type = "全部";
                }
                else
                {
                    type = items.Type;
                }
                List<C_TB_JIESUAN> list_JieSuan = db.C_TB_JIESUAN.Where(n => n.FeiMuZhongLei == items.FeiMuZhongLei && n.Type == type && n.GoodsBill_id == GoodsBill_Id).ToList();
                List<C_TB_CHENGBEN> list_ChengBen = db.C_TB_CHENGBEN.Where(n => n.FeiMuZhongLei == items.FeiMuZhongLei && n.Type == type && n.GoodsBill_id == GoodsBill_Id).ToList();
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
                List<C_TB_SHOUFEI> list_ShouFei = db.C_TB_SHOUFEI.Where(n => n.FeiMuZhongLei == items.FeiMuZhongLei && n.Type == items.Type && n.GoodsBill_id == GoodsBill_Id).ToList();
                decimal? feiyong = 0;
                decimal? ShuLiang = 0;
                //TimeSpan? ts = DateTime.Now - Model_TALLYBILL.SIGNDATE;//获取收费天数
                if (items.JiLiangDanWei.Contains("*天"))
                {
                    if (items.Type == "进库" || items.Type == "出库")
                    {
                        decimal? FeiYong, JiFeiShuLiang;
                        GetTallyBllList_hqdc(GoodsBill_Id, out FeiYong, out JiFeiShuLiang, out duicun);
                        feiyong = FeiYong.ToDecimal(2);
                        ShuLiang = JiFeiShuLiang;
                    }
                    else
                    {
                        decimal? FeiYong, JiFeiShuLiang;
                        GetTallyBllList_hqdc_k(GoodsBill_Id, out FeiYong, out JiFeiShuLiang, out duicun);
                        feiyong = FeiYong.ToDecimal(2);
                        ShuLiang = JiFeiShuLiang;
                    }
                }
                else
                {
                    decimal? FeiYong, shuLiang;
                    duicun = 0;
                    GetTallyBllList_hqqyfy(GoodsBill_Id, items.FeiMuZhongLei, items.Type, out FeiYong, out shuLiang);
                    ShuLiang = shuLiang;
                    feiyong = (shuLiang * items.DanJia).ToDecimal(2);

                }
                Stock_Money model = new Stock_Money()
                {
                    ID = Model_GOODSBILL.ID,
                    HuoMing = items.HuoMing,
                    DanJia = items.DanJia,
                    FMZhongLei = items.FeiMuZhongLei,
                    JiLiangDanWei = items.JiLiangDanWei,
                    BeiZhu = items.BeiZhu,
                    FeiYong = feiyong.ToDecimal(2),
                    MianCunQi = items.MianDuiCunTianShu,
                    ShuLiang = ShuLiang.ToDecimal(3),
                    GoodsBill_id = GoodsBill_Id,
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
                    WaiFuJinE = list_ChengBen.Sum(n => n.ChengBenJinE)


                };
                list.Add(model);

            }
            string text1 = Server.MapPath("..//QueryOut//" + "账单明细" + ".xls");
            C_GOODSAGENT Model_GOODSAGENT = db.C_GOODSAGENT.FirstOrDefault(n => n.ID == Model_GOODSBILL.C_GOODSAGENT_ID);//查找公司全称

            StreamWriter writer1 = new StreamWriter(text1, false);
            StreamWriter writer2 = writer1;
            writer2.WriteLine("	<?xml version=\"1.0\"?>	");
            writer2.WriteLine("	<?mso-application progid=\"Excel.Sheet\"?>	");
            writer2.WriteLine("	<Workbook xmlns=\"urn:schemas-microsoft-com:office:spreadsheet\"	");
            writer2.WriteLine("	 xmlns:o=\"urn:schemas-microsoft-com:office:office\"	");
            writer2.WriteLine("	 xmlns:x=\"urn:schemas-microsoft-com:office:excel\"	");
            writer2.WriteLine("	 xmlns:ss=\"urn:schemas-microsoft-com:office:spreadsheet\"	");
            writer2.WriteLine("	 xmlns:html=\"http://www.w3.org/TR/REC-html40\">	");
            writer2.WriteLine("	 <DocumentProperties xmlns=\"urn:schemas-microsoft-com:office:office\">	");
            writer2.WriteLine("	  <Created>2015-06-05T18:19:34Z</Created>	");
            writer2.WriteLine("	  <LastSaved>2019-01-22T01:13:40Z</LastSaved>	");
            writer2.WriteLine("	  <Version>16.00</Version>	");
            writer2.WriteLine("	 </DocumentProperties>	");
            writer2.WriteLine("	 <OfficeDocumentSettings xmlns=\"urn:schemas-microsoft-com:office:office\">	");
            writer2.WriteLine("	  <AllowPNG/>	");
            writer2.WriteLine("	  <RemovePersonalInformation/>	");
            writer2.WriteLine("	 </OfficeDocumentSettings>	");
            writer2.WriteLine("	 <ExcelWorkbook xmlns=\"urn:schemas-microsoft-com:office:excel\">	");
            writer2.WriteLine("	  <WindowHeight>32767</WindowHeight>	");
            writer2.WriteLine("	  <WindowWidth>2370</WindowWidth>	");
            writer2.WriteLine("	  <WindowTopX>32767</WindowTopX>	");
            writer2.WriteLine("	  <WindowTopY>32767</WindowTopY>	");
            writer2.WriteLine("	  <ProtectStructure>False</ProtectStructure>	");
            writer2.WriteLine("	  <ProtectWindows>False</ProtectWindows>	");
            writer2.WriteLine("	 </ExcelWorkbook>	");
            writer2.WriteLine("	 <Styles>	");
            writer2.WriteLine("	  <Style ss:ID=\"Default\" ss:Name=\"Normal\">	");
            writer2.WriteLine("	   <Alignment ss:Vertical=\"Bottom\"/>	");
            writer2.WriteLine("	   <Borders/>	");
            writer2.WriteLine("	   <Font ss:FontName=\"等线\" x:CharSet=\"134\" ss:Size=\"11\" ss:Color=\"#000000\"/>	");
            writer2.WriteLine("	   <Interior/>	");
            writer2.WriteLine("	   <NumberFormat/>	");
            writer2.WriteLine("	   <Protection/>	");
            writer2.WriteLine("	  </Style>	");
            writer2.WriteLine("	  <Style ss:ID=\"m548789980\">	");
            writer2.WriteLine("	   <Alignment ss:Horizontal=\"Center\" ss:Vertical=\"Center\"/>	");
            writer2.WriteLine("	   <Borders>	");
            writer2.WriteLine("	    <Border ss:Position=\"Bottom\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Left\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Right\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Top\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	   </Borders>	");
            writer2.WriteLine("	   <Font ss:FontName=\"宋体\" x:CharSet=\"134\" ss:Size=\"12\" ss:Bold=\"1\"/>	");
            writer2.WriteLine("	  </Style>	");
            writer2.WriteLine("	  <Style ss:ID=\"m548790000\">	");
            writer2.WriteLine("	   <Alignment ss:Horizontal=\"Center\" ss:Vertical=\"Center\"/>	");
            writer2.WriteLine("	   <Borders>	");
            writer2.WriteLine("	    <Border ss:Position=\"Bottom\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Left\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Right\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Top\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	   </Borders>	");
            writer2.WriteLine("	  </Style>	");
            writer2.WriteLine("	  <Style ss:ID=\"m548790020\">	");
            writer2.WriteLine("	   <Alignment ss:Horizontal=\"Center\" ss:Vertical=\"Center\"/>	");
            writer2.WriteLine("	   <Borders>	");
            writer2.WriteLine("	    <Border ss:Position=\"Bottom\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Left\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Right\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Top\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	   </Borders>	");
            writer2.WriteLine("	  </Style>	");
            writer2.WriteLine("	  <Style ss:ID=\"m548790040\">	");
            writer2.WriteLine("	   <Alignment ss:Horizontal=\"Center\" ss:Vertical=\"Center\"/>	");
            writer2.WriteLine("	   <Borders>	");
            writer2.WriteLine("	    <Border ss:Position=\"Bottom\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Left\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Right\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Top\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	   </Borders>	");
            writer2.WriteLine("	  </Style>	");
            writer2.WriteLine("	  <Style ss:ID=\"m548790060\">	");
            writer2.WriteLine("	   <Alignment ss:Horizontal=\"Center\" ss:Vertical=\"Center\"/>	");
            writer2.WriteLine("	   <Borders>	");
            writer2.WriteLine("	    <Border ss:Position=\"Bottom\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Left\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Right\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Top\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	   </Borders>	");
            writer2.WriteLine("	  </Style>	");
            writer2.WriteLine("	  <Style ss:ID=\"s63\">	");
            writer2.WriteLine("	   <Alignment ss:Horizontal=\"Center\" ss:Vertical=\"Center\"/>	");
            writer2.WriteLine("	   <Font ss:FontName=\"宋体\" x:CharSet=\"134\" ss:Size=\"20\" ss:Bold=\"1\"/>	");
            writer2.WriteLine("	  </Style>	");
            writer2.WriteLine("	  <Style ss:ID=\"s64\">	");
            writer2.WriteLine("	   <Alignment ss:Vertical=\"Center\"/>	");
            writer2.WriteLine("	   <Font ss:FontName=\"等线\" x:CharSet=\"134\" ss:Size=\"11\" ss:Color=\"#000000\"/>	");
            writer2.WriteLine("	  </Style>	");
            writer2.WriteLine("	  <Style ss:ID=\"s65\">	");
            writer2.WriteLine("	   <Alignment ss:Horizontal=\"Center\" ss:Vertical=\"Center\"/>	");
            writer2.WriteLine("	   <Borders>	");
            writer2.WriteLine("	    <Border ss:Position=\"Bottom\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Left\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Right\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Top\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	   </Borders>	");
            writer2.WriteLine("	   <Font ss:FontName=\"宋体\" x:CharSet=\"134\" ss:Size=\"12\" ss:Bold=\"1\"/>	");
            writer2.WriteLine("	  </Style>	");
            writer2.WriteLine("	  <Style ss:ID=\"s72\">	");
            writer2.WriteLine("	   <Alignment ss:Horizontal=\"Center\" ss:Vertical=\"Center\"/>	");
            writer2.WriteLine("	   <Borders>	");
            writer2.WriteLine("	    <Border ss:Position=\"Bottom\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Left\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Right\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Top\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	   </Borders>	");
            writer2.WriteLine("	  </Style>	");
            writer2.WriteLine("	  <Style ss:ID=\"s73\">	");
            writer2.WriteLine("	   <Alignment ss:Horizontal=\"Center\" ss:Vertical=\"Center\"/>	");
            writer2.WriteLine("	   <Borders>	");
            writer2.WriteLine("	    <Border ss:Position=\"Bottom\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Left\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Right\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Top\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	   </Borders>	");
            writer2.WriteLine("	   <Interior/>	");
            writer2.WriteLine("	  </Style>	");
            writer2.WriteLine("	  <Style ss:ID=\"s74\">	");
            writer2.WriteLine("	   <Alignment ss:Horizontal=\"Center\" ss:Vertical=\"Center\"/>	");
            writer2.WriteLine("	   <Borders>	");
            writer2.WriteLine("	    <Border ss:Position=\"Bottom\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Left\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Right\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Top\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	   </Borders>	");
            writer2.WriteLine("	   <Interior/>	");
            writer2.WriteLine("	   <NumberFormat/>	");
            writer2.WriteLine("	  </Style>	");
            writer2.WriteLine("	 </Styles>	");
            writer2.WriteLine("	 <Worksheet ss:Name=\"Sheet1\">	");
            writer2.WriteLine("	  <Table ss:ExpandedColumnCount=\"12\" ss:ExpandedRowCount=\"6\" x:FullColumns=\"1\"	");
            writer2.WriteLine("	   x:FullRows=\"1\" ss:DefaultColumnWidth=\"54\" ss:DefaultRowHeight=\"14.25\">	");
            writer2.WriteLine("	   <Column ss:Index=\"4\" ss:AutoFitWidth=\"0\" ss:Width=\"151.5\"/>	");
            writer2.WriteLine("	   <Column ss:AutoFitWidth=\"0\" ss:Width=\"124.5\"/>	");
            writer2.WriteLine("	   <Column ss:AutoFitWidth=\"0\" ss:Width=\"99\"/>	");
            writer2.WriteLine("	   <Column ss:Index=\"8\" ss:AutoFitWidth=\"0\" ss:Width=\"99\"/>	");
            writer2.WriteLine("	   <Column ss:AutoFitWidth=\"0\" ss:Width=\"101.25\"/>	");
            writer2.WriteLine("	   <Row ss:AutoFitHeight=\"0\" ss:Height=\"25.5\">	");
            writer2.WriteLine("	    <Cell ss:MergeAcross=\"9\" ss:StyleID=\"s63\"><Data ss:Type=\"String\">账单明细</Data></Cell>	");
            writer2.WriteLine("	   </Row>	");
            writer2.WriteLine("	   <Row ss:AutoFitHeight=\"0\" ss:Height=\"29.0625\" ss:StyleID=\"s64\">	");
            writer2.WriteLine("	    <Cell ss:StyleID=\"s65\"><Data ss:Type=\"String\">船名</Data></Cell>	");
            writer2.WriteLine("	    <Cell ss:StyleID=\"s65\"><Data ss:Type=\"String\">货物</Data></Cell>	");
            writer2.WriteLine("	    <Cell ss:StyleID=\"s65\"><Data ss:Type=\"String\">费目</Data></Cell>	");
            writer2.WriteLine("	    <Cell ss:StyleID=\"s65\"><Data ss:Type=\"String\">计费数量（吨/立方）</Data></Cell>	");
            writer2.WriteLine("	    <Cell ss:StyleID=\"s65\"><Data ss:Type=\"String\">费率</Data></Cell>	");
            writer2.WriteLine("	    <Cell ss:StyleID=\"s65\"><Data ss:Type=\"String\">免堆期</Data></Cell>	");
            writer2.WriteLine("	    <Cell ss:StyleID=\"s65\"><Data ss:Type=\"String\">堆天</Data></Cell>	");
            writer2.WriteLine("	    <Cell ss:StyleID=\"s65\"><Data ss:Type=\"String\">价税金额（元）</Data></Cell>	");
            writer2.WriteLine("	    <Cell ss:StyleID=\"s65\"><Data ss:Type=\"String\">税后金额（元）</Data></Cell>	");
            writer2.WriteLine("	    <Cell ss:MergeAcross=\"2\" ss:StyleID=\"m548789980\"><Data ss:Type=\"String\">单位</Data></Cell>	");
            writer2.WriteLine("	   </Row>	");
            foreach (var items_Zd in list)
            {
                writer2.WriteLine("	   <Row ss:AutoFitHeight=\"0\">	");
                writer2.WriteLine("	    <Cell ss:StyleID=\"s72\"><Data ss:Type=\"String\">" + Model_GOODSBILL.ShipName + "</Data></Cell>	");
                writer2.WriteLine("	    <Cell ss:StyleID=\"s73\"><Data ss:Type=\"String\">" + Model_GOODSBILL.C_GOODS + "</Data></Cell>	");
                writer2.WriteLine("	    <Cell ss:StyleID=\"s73\"><Data ss:Type=\"String\">" + items_Zd.FMZhongLei + "</Data></Cell>	");
                writer2.WriteLine("	    <Cell ss:StyleID=\"s73\"><Data ss:Type=\"String\">" + items_Zd.ShuLiang + "</Data></Cell>	");
                writer2.WriteLine("	    <Cell ss:StyleID=\"s72\"><Data ss:Type=\"String\">" + items_Zd.DanJia + "</Data></Cell>	");
                writer2.WriteLine("	    <Cell ss:StyleID=\"s72\"><Data ss:Type=\"String\">" + items_Zd.MianCunQi + "</Data></Cell>	");
                writer2.WriteLine("	    <Cell ss:StyleID=\"s72\"><Data ss:Type=\"String\">" + duicun + "</Data></Cell>	");
                writer2.WriteLine("	    <Cell ss:StyleID=\"s74\"><Data ss:Type=\"String\">" + items_Zd.FeiYong + "</Data></Cell>	");
                writer2.WriteLine("	    <Cell ss:StyleID=\"s74\"><Data ss:Type=\"String\">" + items_Zd.ShuiHouJinE + "</Data></Cell>	");
                if (Model_GOODSAGENT!= null)
                {
                    writer2.WriteLine("	    <Cell ss:MergeAcross=\"2\" ss:StyleID=\"m548790000\"><Data ss:Type=\"String\">" + Model_GOODSAGENT.FullName + "</Data></Cell>	");
                }
                else
                {
                    writer2.WriteLine("	    <Cell ss:MergeAcross=\"2\" ss:StyleID=\"m548790000\"><Data ss:Type=\"String\"></Data></Cell>	");
                }
                writer2.WriteLine("	   </Row>	");
            }


            writer2.WriteLine("	  </Table>	");
            writer2.WriteLine("	  <WorksheetOptions xmlns=\"urn:schemas-microsoft-com:office:excel\">	");
            writer2.WriteLine("	   <PageSetup>	");
            writer2.WriteLine("	    <Header x:Margin=\"0.3\"/>	");
            writer2.WriteLine("	    <Footer x:Margin=\"0.3\"/>	");
            writer2.WriteLine("	    <PageMargins x:Bottom=\"0.75\" x:Left=\"0.7\" x:Right=\"0.7\" x:Top=\"0.75\"/>	");
            writer2.WriteLine("	   </PageSetup>	");
            writer2.WriteLine("	   <Unsynced/>	");
            writer2.WriteLine("	   <Print>	");
            writer2.WriteLine("	    <ValidPrinterInfo/>	");
            writer2.WriteLine("	    <PaperSizeIndex>9</PaperSizeIndex>	");
            writer2.WriteLine("	    <HorizontalResolution>600</HorizontalResolution>	");
            writer2.WriteLine("	    <VerticalResolution>0</VerticalResolution>	");
            writer2.WriteLine("	   </Print>	");
            writer2.WriteLine("	   <Selected/>	");
            writer2.WriteLine("	   <Panes>	");
            writer2.WriteLine("	    <Pane>	");
            writer2.WriteLine("	     <Number>3</Number>	");
            writer2.WriteLine("	     <ActiveRow>8</ActiveRow>	");
            writer2.WriteLine("	     <ActiveCol>3</ActiveCol>	");
            writer2.WriteLine("	    </Pane>	");
            writer2.WriteLine("	   </Panes>	");
            writer2.WriteLine("	   <ProtectObjects>False</ProtectObjects>	");
            writer2.WriteLine("	   <ProtectScenarios>False</ProtectScenarios>	");
            writer2.WriteLine("	  </WorksheetOptions>	");
            writer2.WriteLine("	 </Worksheet>	");
            writer2.WriteLine("	</Workbook>	");

            writer2 = null;
            writer1.Close();

            // MyComm.RegisterScript(UpdatePanel2, this, "window.open('../QueryOut/处罚信息表(" + sUserName + ").xls','','')");
            string filePath = string.Format("/QueryOut//" + "账单明细" + ".xls", DateTime.Now, "账单明细");
            return Json(new AjaxResult { state = ResultType.success.ToString(), message = filePath }.ToJson());
        }
        public JsonResult WorkNumToExcel_ChengBen(int GoodsBill_Id)
        {
            string text1 = Server.MapPath("..//QueryOut//" + "成本明细" + ".xls");
            C_TB_HC_GOODSBILL Model_GOODSBILL = db.C_TB_HC_GOODSBILL.FirstOrDefault(n => n.ID == GoodsBill_Id);//查找票货
            List<C_TB_CHENGBEN> list_ChengBen = db.C_TB_CHENGBEN.Where(n => n.GoodsBill_id == GoodsBill_Id).ToList();
            StreamWriter writer1 = new StreamWriter(text1, false);
            StreamWriter writer2 = writer1;
            writer2.WriteLine("	<?xml version=\"1.0\"?>	");
            writer2.WriteLine("	<?mso-application progid=\"Excel.Sheet\"?>	");
            writer2.WriteLine("	<Workbook xmlns=\"urn:schemas-microsoft-com:office:spreadsheet\"	");
            writer2.WriteLine("	 xmlns:o=\"urn:schemas-microsoft-com:office:office\"	");
            writer2.WriteLine("	 xmlns:x=\"urn:schemas-microsoft-com:office:excel\"	");
            writer2.WriteLine("	 xmlns:ss=\"urn:schemas-microsoft-com:office:spreadsheet\"	");
            writer2.WriteLine("	 xmlns:html=\"http://www.w3.org/TR/REC-html40\">	");
            writer2.WriteLine("	 <DocumentProperties xmlns=\"urn:schemas-microsoft-com:office:office\">	");
            writer2.WriteLine("	  <Created>2015-06-05T18:19:34Z</Created>	");
            writer2.WriteLine("	  <LastSaved>2019-01-22T01:13:40Z</LastSaved>	");
            writer2.WriteLine("	  <Version>16.00</Version>	");
            writer2.WriteLine("	 </DocumentProperties>	");
            writer2.WriteLine("	 <OfficeDocumentSettings xmlns=\"urn:schemas-microsoft-com:office:office\">	");
            writer2.WriteLine("	  <AllowPNG/>	");
            writer2.WriteLine("	  <RemovePersonalInformation/>	");
            writer2.WriteLine("	 </OfficeDocumentSettings>	");
            writer2.WriteLine("	 <ExcelWorkbook xmlns=\"urn:schemas-microsoft-com:office:excel\">	");
            writer2.WriteLine("	  <WindowHeight>32767</WindowHeight>	");
            writer2.WriteLine("	  <WindowWidth>2370</WindowWidth>	");
            writer2.WriteLine("	  <WindowTopX>32767</WindowTopX>	");
            writer2.WriteLine("	  <WindowTopY>32767</WindowTopY>	");
            writer2.WriteLine("	  <ProtectStructure>False</ProtectStructure>	");
            writer2.WriteLine("	  <ProtectWindows>False</ProtectWindows>	");
            writer2.WriteLine("	 </ExcelWorkbook>	");
            writer2.WriteLine("	 <Styles>	");
            writer2.WriteLine("	  <Style ss:ID=\"Default\" ss:Name=\"Normal\">	");
            writer2.WriteLine("	   <Alignment ss:Vertical=\"Bottom\"/>	");
            writer2.WriteLine("	   <Borders/>	");
            writer2.WriteLine("	   <Font ss:FontName=\"等线\" x:CharSet=\"134\" ss:Size=\"11\" ss:Color=\"#000000\"/>	");
            writer2.WriteLine("	   <Interior/>	");
            writer2.WriteLine("	   <NumberFormat/>	");
            writer2.WriteLine("	   <Protection/>	");
            writer2.WriteLine("	  </Style>	");
            writer2.WriteLine("	  <Style ss:ID=\"m414139828\">	");
            writer2.WriteLine("	   <Alignment ss:Horizontal=\"Center\" ss:Vertical=\"Center\"/>	");
            writer2.WriteLine("	   <Borders>	");
            writer2.WriteLine("	    <Border ss:Position=\"Bottom\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Left\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Right\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Top\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	   </Borders>	");
            writer2.WriteLine("	   <Font ss:FontName=\"宋体\" x:CharSet=\"134\" ss:Size=\"12\" ss:Bold=\"1\"/>	");
            writer2.WriteLine("	  </Style>	");
            writer2.WriteLine("	  <Style ss:ID=\"m414139848\">	");
            writer2.WriteLine("	   <Alignment ss:Horizontal=\"Center\" ss:Vertical=\"Center\"/>	");
            writer2.WriteLine("	   <Borders>	");
            writer2.WriteLine("	    <Border ss:Position=\"Bottom\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Left\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Right\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Top\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	   </Borders>	");
            writer2.WriteLine("	  </Style>	");
            writer2.WriteLine("	  <Style ss:ID=\"s63\">	");
            writer2.WriteLine("	   <Alignment ss:Horizontal=\"Center\" ss:Vertical=\"Center\"/>	");
            writer2.WriteLine("	   <Font ss:FontName=\"宋体\" x:CharSet=\"134\" ss:Size=\"20\" ss:Bold=\"1\"/>	");
            writer2.WriteLine("	  </Style>	");
            writer2.WriteLine("	  <Style ss:ID=\"s64\">	");
            writer2.WriteLine("	   <Alignment ss:Vertical=\"Center\"/>	");
            writer2.WriteLine("	  </Style>	");
            writer2.WriteLine("	  <Style ss:ID=\"s65\">	");
            writer2.WriteLine("	   <Alignment ss:Vertical=\"Center\"/>	");
            writer2.WriteLine("	   <Borders>	");
            writer2.WriteLine("	    <Border ss:Position=\"Bottom\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Left\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Right\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Top\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	   </Borders>	");
            writer2.WriteLine("	   <Font ss:FontName=\"宋体\" x:CharSet=\"134\" ss:Size=\"12\" ss:Bold=\"1\"/>	");
            writer2.WriteLine("	  </Style>	");
            writer2.WriteLine("	  <Style ss:ID=\"s66\">	");
            writer2.WriteLine("	   <Alignment ss:Horizontal=\"Center\" ss:Vertical=\"Center\"/>	");
            writer2.WriteLine("	   <Borders>	");
            writer2.WriteLine("	    <Border ss:Position=\"Bottom\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Left\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Right\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Top\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	   </Borders>	");
            writer2.WriteLine("	   <Font ss:FontName=\"宋体\" x:CharSet=\"134\" ss:Size=\"12\" ss:Bold=\"1\"/>	");
            writer2.WriteLine("	  </Style>	");
            writer2.WriteLine("	  <Style ss:ID=\"s67\">	");
            writer2.WriteLine("	   <Alignment ss:Horizontal=\"Center\" ss:Vertical=\"Center\"/>	");
            writer2.WriteLine("	   <Borders>	");
            writer2.WriteLine("	    <Border ss:Position=\"Bottom\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Left\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Right\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Top\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	   </Borders>	");
            writer2.WriteLine("	   <Font ss:FontName=\"宋体\" x:CharSet=\"134\" ss:Size=\"11\" ss:Bold=\"1\"/>	");
            writer2.WriteLine("	  </Style>	");
            writer2.WriteLine("	  <Style ss:ID=\"s73\">	");
            writer2.WriteLine("	   <Alignment ss:Horizontal=\"Center\" ss:Vertical=\"Center\"/>	");
            writer2.WriteLine("	   <Borders>	");
            writer2.WriteLine("	    <Border ss:Position=\"Bottom\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Left\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Right\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Top\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	   </Borders>	");
            writer2.WriteLine("	  </Style>	");
            writer2.WriteLine("	  <Style ss:ID=\"s74\">	");
            writer2.WriteLine("	   <Alignment ss:Horizontal=\"Center\" ss:Vertical=\"Center\"/>	");
            writer2.WriteLine("	   <Borders>	");
            writer2.WriteLine("	    <Border ss:Position=\"Bottom\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Left\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Right\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Top\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	   </Borders>	");
            writer2.WriteLine("	   <Interior/>	");
            writer2.WriteLine("	  </Style>	");
            writer2.WriteLine("	  <Style ss:ID=\"s80\">	");
            writer2.WriteLine("	   <Alignment ss:Horizontal=\"Center\" ss:Vertical=\"Center\"/>	");
            writer2.WriteLine("	   <Borders>	");
            writer2.WriteLine("	    <Border ss:Position=\"Bottom\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Left\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Right\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Top\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	   </Borders>	");
            writer2.WriteLine("	   <Interior/>	");
            writer2.WriteLine("	   <NumberFormat ss:Format=\"&quot;¥&quot;#,##0.00;&quot;¥&quot;\\-#,##0.00\"/>	");
            writer2.WriteLine("	  </Style>	");
            writer2.WriteLine("	 </Styles>	");
            writer2.WriteLine("	 <Worksheet ss:Name=\"Sheet1\">	");
            writer2.WriteLine("	  <Table x:FullColumns=\"1\"	");
            writer2.WriteLine("	   x:FullRows=\"1\" ss:DefaultColumnWidth=\"54\" ss:DefaultRowHeight=\"14.25\">	");
            writer2.WriteLine("	   <Column ss:Index=\"4\" ss:AutoFitWidth=\"0\" ss:Width=\"151.5\"/>	");
            writer2.WriteLine("	   <Column ss:AutoFitWidth=\"0\" ss:Width=\"124.5\"/>	");
            writer2.WriteLine("	   <Column ss:AutoFitWidth=\"0\" ss:Width=\"99\"/>	");
            writer2.WriteLine("	   <Column ss:Index=\"8\" ss:AutoFitWidth=\"0\" ss:Width=\"99\"/>	");
            writer2.WriteLine("	   <Column ss:AutoFitWidth=\"0\" ss:Width=\"101.25\"/>	");
            writer2.WriteLine("	   <Row ss:AutoFitHeight=\"0\" ss:Height=\"25.5\">	");
            writer2.WriteLine("	    <Cell ss:MergeAcross=\"9\" ss:StyleID=\"s63\"><Data ss:Type=\"String\">成本明细</Data></Cell>	");
            writer2.WriteLine("	   </Row>	");
            writer2.WriteLine("	   <Row ss:AutoFitHeight=\"0\" ss:Height=\"30\" ss:StyleID=\"s64\">	");
            writer2.WriteLine("	    <Cell ss:StyleID=\"s65\"><Data ss:Type=\"String\">外付日期</Data></Cell>	");
            writer2.WriteLine("	    <Cell ss:StyleID=\"s66\"><Data ss:Type=\"String\">船名</Data></Cell>	");
            writer2.WriteLine("	    <Cell ss:StyleID=\"s66\"><Data ss:Type=\"String\">货物</Data></Cell>	");
            writer2.WriteLine("	    <Cell ss:StyleID=\"s66\"><Data ss:Type=\"String\">成本费目</Data></Cell>	");
            writer2.WriteLine("	    <Cell ss:StyleID=\"s66\"><Data ss:Type=\"String\">计费数量（吨/立方）</Data></Cell>	");
            writer2.WriteLine("	    <Cell ss:StyleID=\"s67\"><Data ss:Type=\"String\">成本费率</Data></Cell>	");
            writer2.WriteLine("	    <Cell ss:MergeAcross=\"1\" ss:StyleID=\"m414139828\"><Data ss:Type=\"String\">成本金额</Data></Cell>	");
            writer2.WriteLine("	    <Cell ss:StyleID=\"s66\"><Data ss:Type=\"String\">外付单位</Data></Cell>	");
            writer2.WriteLine("	    <Cell ss:StyleID=\"s66\"><Data ss:Type=\"String\">状态</Data></Cell>	");
            writer2.WriteLine("	   </Row>	");
            foreach (var items_cb in list_ChengBen)
            {
                writer2.WriteLine("	   <Row ss:AutoFitHeight=\"0\">	");
                writer2.WriteLine("	    <Cell ss:StyleID=\"s73\"><Data ss:Type=\"String\">" + items_cb.WaiFuRiQi + "</Data></Cell>	");
                writer2.WriteLine("	    <Cell ss:StyleID=\"s74\"><Data ss:Type=\"String\">" + Model_GOODSBILL.ShipName + "</Data></Cell>	");
                writer2.WriteLine("	    <Cell ss:StyleID=\"s74\"><Data ss:Type=\"String\">" + Model_GOODSBILL.C_GOODS + "</Data></Cell>	");
                writer2.WriteLine("	    <Cell ss:StyleID=\"s74\"><Data ss:Type=\"String\">" + items_cb.FeiMuZhongLei + "</Data></Cell>	");
                writer2.WriteLine("	    <Cell ss:StyleID=\"s73\"><Data ss:Type=\"String\">" + items_cb.ShuLiang + "</Data></Cell>	");
                writer2.WriteLine("	    <Cell ss:StyleID=\"s73\"><Data ss:Type=\"String\">" + items_cb.ChengBenFeiLv + "</Data></Cell>	");
                writer2.WriteLine("	    <Cell ss:MergeAcross=\"1\" ss:StyleID=\"m414139848\"><Data ss:Type=\"String\">" + items_cb.ChengBenJinE + "</Data></Cell>	");
                writer2.WriteLine("	    <Cell ss:StyleID=\"s80\"><Data ss:Type=\"String\">" + items_cb.WaiFuDanWei + "</Data></Cell>	");
                writer2.WriteLine("	    <Cell ss:StyleID=\"s80\"><Data ss:Type=\"String\">" + items_cb.State + "</Data></Cell>	");
                writer2.WriteLine("	   </Row>	");
            }

            writer2.WriteLine("	  </Table>	");
            writer2.WriteLine("	  <WorksheetOptions xmlns=\"urn:schemas-microsoft-com:office:excel\">	");
            writer2.WriteLine("	   <PageSetup>	");
            writer2.WriteLine("	    <Header x:Margin=\"0.3\"/>	");
            writer2.WriteLine("	    <Footer x:Margin=\"0.3\"/>	");
            writer2.WriteLine("	    <PageMargins x:Bottom=\"0.75\" x:Left=\"0.7\" x:Right=\"0.7\" x:Top=\"0.75\"/>	");
            writer2.WriteLine("	   </PageSetup>	");
            writer2.WriteLine("	   <Unsynced/>	");
            writer2.WriteLine("	   <Print>	");
            writer2.WriteLine("	    <ValidPrinterInfo/>	");
            writer2.WriteLine("	    <PaperSizeIndex>9</PaperSizeIndex>	");
            writer2.WriteLine("	    <HorizontalResolution>600</HorizontalResolution>	");
            writer2.WriteLine("	    <VerticalResolution>0</VerticalResolution>	");
            writer2.WriteLine("	   </Print>	");
            writer2.WriteLine("	   <Selected/>	");
            writer2.WriteLine("	   <Panes>	");
            writer2.WriteLine("	    <Pane>	");
            writer2.WriteLine("	     <Number>3</Number>	");
            writer2.WriteLine("	     <ActiveRow>7</ActiveRow>	");
            writer2.WriteLine("	     <ActiveCol>8</ActiveCol>	");
            writer2.WriteLine("	    </Pane>	");
            writer2.WriteLine("	   </Panes>	");
            writer2.WriteLine("	   <ProtectObjects>False</ProtectObjects>	");
            writer2.WriteLine("	   <ProtectScenarios>False</ProtectScenarios>	");
            writer2.WriteLine("	  </WorksheetOptions>	");
            writer2.WriteLine("	 </Worksheet>	");
            writer2.WriteLine("	</Workbook>	");
            writer2.WriteLine("		");
            writer2.WriteLine("		");



            writer2 = null;
            writer1.Close();

            string filePath = string.Format("/QueryOut//" + "成本明细" + ".xls", DateTime.Now, "成本明细");
            return Json(new AjaxResult { state = ResultType.success.ToString(), message = filePath }.ToJson());
        }
        public JsonResult WorkNumToExcel_JieSuan(int GoodsBill_Id)
        {
            decimal? duicun = 0;
            decimal? shuilv = 0;
            decimal? HuiShouLv = 0;
            List<Stock_Money> list = new List<Stock_Money>();
            int total = 0;
            var rows = list.ToList();
            //C_TB_HS_TALLYBILL Model_TALLYBILL = db.C_TB_HS_TALLYBILL.FirstOrDefault(n => n.ID == id);//查找理货单
            //C_TB_HC_CONSIGN Model_CONSIGN = db.C_TB_HC_CONSIGN.FirstOrDefault(n => n.ID == Model_TALLYBILL.CONSIGN_ID);//查找委托
            C_TB_HC_GOODSBILL Model_GOODSBILL = db.C_TB_HC_GOODSBILL.FirstOrDefault(n => n.ID == GoodsBill_Id);//查找票货
            if (Model_GOODSBILL.CONTRACT_Guid == null)
            {
                return Json(new { total = total, rows = rows }, JsonRequestBehavior.AllowGet);
            }
            C_TB_HC_CONTRACT Model_CONTRACT = db.C_TB_HC_CONTRACT.FirstOrDefault(n => n.Guid == Model_GOODSBILL.CONTRACT_Guid);//查找合同
            List<C_TB_HC_CONTRACT_DETAILED> list_DETAILED = db.C_TB_HC_CONTRACT_DETAILED.Where(n => n.CONTRACT_Guid == Model_CONTRACT.Guid).ToList();


            foreach (var items in list_DETAILED)
            {
                string type = "";
                if (string.IsNullOrEmpty(items.Type))
                {
                    type = "全部";
                }
                else
                {
                    type = items.Type;
                }
                List<C_TB_JIESUAN> list_JieSuan = db.C_TB_JIESUAN.Where(n => n.FeiMuZhongLei == items.FeiMuZhongLei && n.Type == type && n.GoodsBill_id == GoodsBill_Id).ToList();
                List<C_TB_CHENGBEN> list_ChengBen = db.C_TB_CHENGBEN.Where(n => n.FeiMuZhongLei == items.FeiMuZhongLei && n.Type == type && n.GoodsBill_id == GoodsBill_Id).ToList();
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
                List<C_TB_SHOUFEI> list_ShouFei = db.C_TB_SHOUFEI.Where(n => n.FeiMuZhongLei == items.FeiMuZhongLei && n.Type == items.Type && n.GoodsBill_id == GoodsBill_Id).ToList();
                decimal? feiyong = 0;
                decimal? ShuLiang = 0;
                //TimeSpan? ts = DateTime.Now - Model_TALLYBILL.SIGNDATE;//获取收费天数
                if (items.JiLiangDanWei.Contains("*天"))
                {
                    if (items.Type == "进库" || items.Type == "出库")
                    {
                        decimal? FeiYong, JiFeiShuLiang;
                        GetTallyBllList_hqdc(GoodsBill_Id, out FeiYong, out JiFeiShuLiang, out duicun);
                        feiyong = FeiYong.ToDecimal(2);
                        ShuLiang = JiFeiShuLiang;
                    }
                    else
                    {
                        decimal? FeiYong, JiFeiShuLiang;
                        GetTallyBllList_hqdc_k(GoodsBill_Id, out FeiYong, out JiFeiShuLiang, out duicun);
                        feiyong = FeiYong.ToDecimal(2);
                        ShuLiang = JiFeiShuLiang;
                    }
                }
                else
                {
                    decimal? FeiYong, shuLiang;
                    duicun = 0;
                    GetTallyBllList_hqqyfy(GoodsBill_Id, items.FeiMuZhongLei, items.Type, out FeiYong, out shuLiang);
                    ShuLiang = shuLiang;
                    feiyong = (shuLiang * items.DanJia).ToDecimal(2);

                }
                Stock_Money model = new Stock_Money()
                {
                    ID = Model_GOODSBILL.ID,
                    HuoMing = items.HuoMing,
                    DanJia = items.DanJia,
                    FMZhongLei = items.FeiMuZhongLei,
                    JiLiangDanWei = items.JiLiangDanWei,
                    BeiZhu = items.BeiZhu,
                    FeiYong = feiyong.ToDecimal(2),
                    MianCunQi = items.MianDuiCunTianShu,
                    ShuLiang = ShuLiang.ToDecimal(3),
                    GoodsBill_id = GoodsBill_Id,
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
                    Shuilv = shuilv


                };
                list.Add(model);

            }
            string text1 = Server.MapPath("..//QueryOut//" + "开票明细" + ".xls");

            List<C_TB_JIESUAN> list_JieSuan_dc = db.C_TB_JIESUAN.Where(n => n.GoodsBill_id == GoodsBill_Id).OrderBy(n => n.KaiPiaoRiQi).ToList();
            StreamWriter writer1 = new StreamWriter(text1, false);
            StreamWriter writer2 = writer1;
            writer2.WriteLine("	<?xml version=\"1.0\"?>	");
            writer2.WriteLine("	<?mso-application progid=\"Excel.Sheet\"?>	");
            writer2.WriteLine("	<Workbook xmlns=\"urn:schemas-microsoft-com:office:spreadsheet\"	");
            writer2.WriteLine("	 xmlns:o=\"urn:schemas-microsoft-com:office:office\"	");
            writer2.WriteLine("	 xmlns:x=\"urn:schemas-microsoft-com:office:excel\"	");
            writer2.WriteLine("	 xmlns:ss=\"urn:schemas-microsoft-com:office:spreadsheet\"	");
            writer2.WriteLine("	 xmlns:html=\"http://www.w3.org/TR/REC-html40\">	");
            writer2.WriteLine("	 <DocumentProperties xmlns=\"urn:schemas-microsoft-com:office:office\">	");
            writer2.WriteLine("	  <Created>2015-06-05T18:19:34Z</Created>	");
            writer2.WriteLine("	  <LastSaved>2019-01-22T01:13:40Z</LastSaved>	");
            writer2.WriteLine("	  <Version>16.00</Version>	");
            writer2.WriteLine("	 </DocumentProperties>	");
            writer2.WriteLine("	 <OfficeDocumentSettings xmlns=\"urn:schemas-microsoft-com:office:office\">	");
            writer2.WriteLine("	  <AllowPNG/>	");
            writer2.WriteLine("	  <RemovePersonalInformation/>	");
            writer2.WriteLine("	 </OfficeDocumentSettings>	");
            writer2.WriteLine("	 <ExcelWorkbook xmlns=\"urn:schemas-microsoft-com:office:excel\">	");
            writer2.WriteLine("	  <WindowHeight>32767</WindowHeight>	");
            writer2.WriteLine("	  <WindowWidth>2370</WindowWidth>	");
            writer2.WriteLine("	  <WindowTopX>32767</WindowTopX>	");
            writer2.WriteLine("	  <WindowTopY>32767</WindowTopY>	");
            writer2.WriteLine("	  <ProtectStructure>False</ProtectStructure>	");
            writer2.WriteLine("	  <ProtectWindows>False</ProtectWindows>	");
            writer2.WriteLine("	 </ExcelWorkbook>	");
            writer2.WriteLine("	 <Styles>	");
            writer2.WriteLine("	  <Style ss:ID=\"Default\" ss:Name=\"Normal\">	");
            writer2.WriteLine("	   <Alignment ss:Vertical=\"Bottom\"/>	");
            writer2.WriteLine("	   <Borders/>	");
            writer2.WriteLine("	   <Font ss:FontName=\"等线\" x:CharSet=\"134\" ss:Size=\"11\" ss:Color=\"#000000\"/>	");
            writer2.WriteLine("	   <Interior/>	");
            writer2.WriteLine("	   <NumberFormat/>	");
            writer2.WriteLine("	   <Protection/>	");
            writer2.WriteLine("	  </Style>	");
            writer2.WriteLine("	  <Style ss:ID=\"m425481156\">	");
            writer2.WriteLine("	   <Alignment ss:Horizontal=\"Center\" ss:Vertical=\"Center\"/>	");
            writer2.WriteLine("	   <Borders>	");
            writer2.WriteLine("	    <Border ss:Position=\"Bottom\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Left\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Right\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Top\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	   </Borders>	");
            writer2.WriteLine("	   <Font ss:FontName=\"宋体\" x:CharSet=\"134\" ss:Size=\"12\" ss:Bold=\"1\"/>	");
            writer2.WriteLine("	  </Style>	");
            writer2.WriteLine("	  <Style ss:ID=\"m425481176\">	");
            writer2.WriteLine("	   <Alignment ss:Horizontal=\"Center\" ss:Vertical=\"Center\"/>	");
            writer2.WriteLine("	   <Borders>	");
            writer2.WriteLine("	    <Border ss:Position=\"Bottom\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Left\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Right\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Top\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	   </Borders>	");
            writer2.WriteLine("	   <Font ss:FontName=\"宋体\" x:CharSet=\"134\" ss:Size=\"12\" ss:Bold=\"1\"/>	");
            writer2.WriteLine("	  </Style>	");
            writer2.WriteLine("	  <Style ss:ID=\"m425481196\">	");
            writer2.WriteLine("	   <Alignment ss:Horizontal=\"Center\" ss:Vertical=\"Center\"/>	");
            writer2.WriteLine("	   <Borders>	");
            writer2.WriteLine("	    <Border ss:Position=\"Bottom\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Left\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Right\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Top\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	   </Borders>	");
            writer2.WriteLine("	  </Style>	");
            writer2.WriteLine("	  <Style ss:ID=\"m425481216\">	");
            writer2.WriteLine("	   <Alignment ss:Horizontal=\"Center\" ss:Vertical=\"Center\"/>	");
            writer2.WriteLine("	   <Borders>	");
            writer2.WriteLine("	    <Border ss:Position=\"Bottom\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Left\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Right\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Top\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	   </Borders>	");
            writer2.WriteLine("	  </Style>	");
            writer2.WriteLine("	  <Style ss:ID=\"s63\">	");
            writer2.WriteLine("	   <Alignment ss:Horizontal=\"Center\" ss:Vertical=\"Center\"/>	");
            writer2.WriteLine("	   <Font ss:FontName=\"宋体\" x:CharSet=\"134\" ss:Size=\"20\" ss:Bold=\"1\"/>	");
            writer2.WriteLine("	  </Style>	");
            writer2.WriteLine("	  <Style ss:ID=\"s64\">	");
            writer2.WriteLine("	   <Alignment ss:Vertical=\"Center\"/>	");
            writer2.WriteLine("	  </Style>	");
            writer2.WriteLine("	  <Style ss:ID=\"s65\">	");
            writer2.WriteLine("	   <Alignment ss:Vertical=\"Center\"/>	");
            writer2.WriteLine("	   <Borders>	");
            writer2.WriteLine("	    <Border ss:Position=\"Bottom\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Left\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Right\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Top\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	   </Borders>	");
            writer2.WriteLine("	   <Font ss:FontName=\"宋体\" x:CharSet=\"134\" ss:Size=\"12\" ss:Bold=\"1\"/>	");
            writer2.WriteLine("	  </Style>	");
            writer2.WriteLine("	  <Style ss:ID=\"s66\">	");
            writer2.WriteLine("	   <Alignment ss:Horizontal=\"Center\" ss:Vertical=\"Center\"/>	");
            writer2.WriteLine("	   <Borders>	");
            writer2.WriteLine("	    <Border ss:Position=\"Bottom\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Left\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Right\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Top\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	   </Borders>	");
            writer2.WriteLine("	   <Font ss:FontName=\"宋体\" x:CharSet=\"134\" ss:Size=\"12\" ss:Bold=\"1\"/>	");
            writer2.WriteLine("	  </Style>	");
            writer2.WriteLine("	  <Style ss:ID=\"s72\">	");
            writer2.WriteLine("	   <Alignment ss:Horizontal=\"Center\" ss:Vertical=\"Center\"/>	");
            writer2.WriteLine("	   <Borders>	");
            writer2.WriteLine("	    <Border ss:Position=\"Bottom\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Left\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Right\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Top\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	   </Borders>	");
            writer2.WriteLine("	  </Style>	");
            writer2.WriteLine("	  <Style ss:ID=\"s73\">	");
            writer2.WriteLine("	   <Alignment ss:Horizontal=\"Center\" ss:Vertical=\"Center\"/>	");
            writer2.WriteLine("	   <Borders>	");
            writer2.WriteLine("	    <Border ss:Position=\"Bottom\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Left\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Right\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Top\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	   </Borders>	");
            writer2.WriteLine("	   <Interior/>	");
            writer2.WriteLine("	  </Style>	");
            writer2.WriteLine("	 </Styles>	");
            writer2.WriteLine("	 <Worksheet ss:Name=\"Sheet1\">	");
            writer2.WriteLine("	  <Table x:FullColumns=\"1\"	");
            writer2.WriteLine("	   x:FullRows=\"1\" ss:DefaultColumnWidth=\"54\" ss:DefaultRowHeight=\"14.25\">	");
            writer2.WriteLine("	   <Column ss:Index=\"4\" ss:AutoFitWidth=\"0\" ss:Width=\"151.5\"/>	");
            writer2.WriteLine("	   <Column ss:AutoFitWidth=\"0\" ss:Width=\"124.5\"/>	");
            writer2.WriteLine("	   <Column ss:AutoFitWidth=\"0\" ss:Width=\"99\"/>	");
            writer2.WriteLine("	   <Column ss:Index=\"8\" ss:AutoFitWidth=\"0\" ss:Width=\"99\"/>	");
            writer2.WriteLine("	   <Column ss:AutoFitWidth=\"0\" ss:Width=\"144.75\"/>	");
            writer2.WriteLine("	   <Column ss:AutoFitWidth=\"0\" ss:Width=\"84\"/>	");
            writer2.WriteLine("	   <Column ss:AutoFitWidth=\"0\" ss:Width=\"73.5\"/>	");
            writer2.WriteLine("	   <Row ss:AutoFitHeight=\"0\" ss:Height=\"25.5\">	");
            writer2.WriteLine("	    <Cell ss:MergeAcross=\"9\" ss:StyleID=\"s63\"><Data ss:Type=\"String\">开票明细</Data></Cell>	");
            writer2.WriteLine("	   </Row>	");
            writer2.WriteLine("	   <Row ss:AutoFitHeight=\"0\" ss:Height=\"39\" ss:StyleID=\"s64\">	");
            writer2.WriteLine("	    <Cell ss:StyleID=\"s65\"><Data ss:Type=\"String\">开票日期</Data></Cell>	");
            writer2.WriteLine("	    <Cell ss:StyleID=\"s66\"><Data ss:Type=\"String\">船名</Data></Cell>	");
            writer2.WriteLine("	    <Cell ss:StyleID=\"s66\"><Data ss:Type=\"String\">货物</Data></Cell>	");
            writer2.WriteLine("	    <Cell ss:StyleID=\"s66\"><Data ss:Type=\"String\">费目</Data></Cell>	");
            writer2.WriteLine("	    <Cell ss:StyleID=\"s66\"><Data ss:Type=\"String\">计费数量（吨/立方）</Data></Cell>	");
            writer2.WriteLine("	    <Cell ss:StyleID=\"s66\"><Data ss:Type=\"String\">费率</Data></Cell>	");
            writer2.WriteLine("	    <Cell ss:MergeAcross=\"1\" ss:StyleID=\"m425481156\"><Data ss:Type=\"String\">价税金额（元）</Data></Cell>	");
            writer2.WriteLine("	    <Cell ss:StyleID=\"s66\"><Data ss:Type=\"String\">税后金额（元）</Data></Cell>	");
            writer2.WriteLine("	    <Cell ss:StyleID=\"s66\"><Data ss:Type=\"String\">来款金额</Data></Cell>	");
            writer2.WriteLine("	    <Cell ss:StyleID=\"s66\"><Data ss:Type=\"String\">未开票金额</Data></Cell>	");
            writer2.WriteLine("	    <Cell ss:MergeAcross=\"1\" ss:StyleID=\"m425481176\"><Data ss:Type=\"String\">单位</Data></Cell>	");
            writer2.WriteLine("	   </Row>	");
            foreach (var items_jiesuan in list_JieSuan_dc)
            {
                writer2.WriteLine("	   <Row ss:AutoFitHeight=\"0\">	");
                writer2.WriteLine("	    <Cell ss:StyleID=\"s72\"><Data ss:Type=\"String\">" + items_jiesuan.KaiPiaoRiQi + "</Data></Cell>	");
                writer2.WriteLine("	    <Cell ss:StyleID=\"s73\"><Data ss:Type=\"String\">" + Model_GOODSBILL.ShipName + "</Data></Cell>	");
                writer2.WriteLine("	    <Cell ss:StyleID=\"s73\"><Data ss:Type=\"String\">" + Model_GOODSBILL.C_GOODS + "</Data></Cell>	");
                writer2.WriteLine("	    <Cell ss:StyleID=\"s73\"><Data ss:Type=\"String\">" + items_jiesuan.FeiMuZhongLei + "</Data></Cell>");
                writer2.WriteLine("	    <Cell ss:StyleID=\"s72\"><Data ss:Type=\"String\">" + list.Where(n => n.FMZhongLei == items_jiesuan.FeiMuZhongLei && n.Type == items_jiesuan.Type).Sum(n => n.ShuLiang) + "</Data></Cell>	");
                writer2.WriteLine("	    <Cell ss:StyleID=\"s72\"><Data ss:Type=\"String\">" + list.FirstOrDefault(n => n.FMZhongLei == items_jiesuan.FeiMuZhongLei && n.Type == items_jiesuan.Type).DanJia + "</Data></Cell>	");
                writer2.WriteLine("	    <Cell ss:MergeAcross=\"1\" ss:StyleID=\"m425481196\"><Data ss:Type=\"String\">" + items_jiesuan.KaiPiaoJinE + "</Data></Cell>	");
                writer2.WriteLine("	    <Cell ss:StyleID=\"s72\"><Data ss:Type=\"String\">" + (items_jiesuan.KaiPiaoJinE / (1 + (list.FirstOrDefault(n => n.FMZhongLei == items_jiesuan.FeiMuZhongLei && n.Type == items_jiesuan.Type).Shuilv * Convert.ToDecimal(0.01)))).ToDecimal(2) + "</Data></Cell>	");
                writer2.WriteLine("	    <Cell ss:StyleID=\"s72\"><Data ss:Type=\"String\">" + items_jiesuan.LaiKuanJinE + "</Data></Cell>	");
                writer2.WriteLine("	    <Cell ss:StyleID=\"s72\"><Data ss:Type=\"String\">" + list.Where(n => n.FMZhongLei == items_jiesuan.FeiMuZhongLei && n.Type == items_jiesuan.Type).Sum(n => n.WeiKaiPiaoJinE) + "</Data></Cell>	");
                writer2.WriteLine("	    <Cell ss:MergeAcross=\"1\" ss:StyleID=\"m425481216\"><Data ss:Type=\"String\">" + items_jiesuan.KaiPianDanWei + "</Data></Cell>	");
                writer2.WriteLine("	   </Row>	");
            }


            writer2.WriteLine("	  </Table>	");
            writer2.WriteLine("	  <WorksheetOptions xmlns=\"urn:schemas-microsoft-com:office:excel\">	");
            writer2.WriteLine("	   <PageSetup>	");
            writer2.WriteLine("	    <Header x:Margin=\"0.3\"/>	");
            writer2.WriteLine("	    <Footer x:Margin=\"0.3\"/>	");
            writer2.WriteLine("	    <PageMargins x:Bottom=\"0.75\" x:Left=\"0.7\" x:Right=\"0.7\" x:Top=\"0.75\"/>	");
            writer2.WriteLine("	   </PageSetup>	");
            writer2.WriteLine("	   <Unsynced/>	");
            writer2.WriteLine("	   <Print>	");
            writer2.WriteLine("	    <ValidPrinterInfo/>	");
            writer2.WriteLine("	    <PaperSizeIndex>9</PaperSizeIndex>	");
            writer2.WriteLine("	    <HorizontalResolution>600</HorizontalResolution>	");
            writer2.WriteLine("	    <VerticalResolution>0</VerticalResolution>	");
            writer2.WriteLine("	   </Print>	");
            writer2.WriteLine("	   <Selected/>	");
            writer2.WriteLine("	   <LeftColumnVisible>4</LeftColumnVisible>	");
            writer2.WriteLine("	   <Panes>	");
            writer2.WriteLine("	    <Pane>	");
            writer2.WriteLine("	     <Number>3</Number>	");
            writer2.WriteLine("	     <ActiveRow>2</ActiveRow>	");
            writer2.WriteLine("	     <ActiveCol>11</ActiveCol>	");
            writer2.WriteLine("	     <RangeSelection>R3C12:R3C13</RangeSelection>	");
            writer2.WriteLine("	    </Pane>	");
            writer2.WriteLine("	   </Panes>	");
            writer2.WriteLine("	   <ProtectObjects>False</ProtectObjects>	");
            writer2.WriteLine("	   <ProtectScenarios>False</ProtectScenarios>	");
            writer2.WriteLine("	  </WorksheetOptions>	");
            writer2.WriteLine("	 </Worksheet>	");
            writer2.WriteLine("	</Workbook>	");
            writer2 = null;
            writer1.Close();

            string filePath = string.Format("/QueryOut//" + "开票明细" + ".xls", DateTime.Now, "开票明细");
            return Json(new AjaxResult { state = ResultType.success.ToString(), message = filePath }.ToJson());
        }
        public ActionResult AddMoney_SJFL(int GoodsBill_id, string Type, string FeiMuZhongLei)
        {
            C_TB_ZHIXINGFEILV model = db.C_TB_ZHIXINGFEILV.FirstOrDefault(n => n.GoodsBillId == GoodsBill_id && n.Type == Type && n.FeiMuZhongLei == FeiMuZhongLei) ?? new C_TB_ZHIXINGFEILV();
            ViewBag.GoodsBillId = GoodsBill_id;
            ViewBag.Type = Type;
            ViewBag.FeiMuZhongLei = FeiMuZhongLei;

            return View(model);
        }

        [OperationLog(OperationLogAttribute.Operatetype.添加或编辑, OperationLogAttribute.ImportantLevel.一般操作, "实际费率")]
        [HttpPost]
        public JsonResult AddMoney_SJFL(C_TB_ZHIXINGFEILV model)
        {
            if (model.Guid == null)
            {
                model.Guid = Guid.NewGuid().ToString();
            }

            db.Set<C_TB_ZHIXINGFEILV>().AddOrUpdate(model);
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
        public ActionResult AddMoney_SFGZ(int GoodsBill_id, string Type, string FeiMuZhongLei, string Guid)
        {
            C_DIC_GUIZE model = db.C_DIC_GUIZE.FirstOrDefault(n => n.GoodsBillId == GoodsBill_id && n.Type == Type && n.FeiMuZhongLei == FeiMuZhongLei && n.Guid == Guid) ?? new C_DIC_GUIZE();
            ViewBag.GoodsBillId = GoodsBill_id;
            ViewBag.Type = Type;
            ViewBag.FeiMuZhongLei = FeiMuZhongLei;
            ViewBag.Guid = Guid;

            return View(model);
        }

        [OperationLog(OperationLogAttribute.Operatetype.添加或编辑, OperationLogAttribute.ImportantLevel.一般操作, "收费规则")]
        [HttpPost]
        public JsonResult AddMoney_SFGZ(C_DIC_GUIZE model)
        {
            if (model.Time1 > model.Time2)
            {
                return Json("堆存天数区间开始数不得大于结束数！");
            }
            if (model.Time_start > model.Time_end)
            {
                return Json("免除区间区间开始时间不得大于结束时间！");
            }
            if (model.Guid == "0")
            {
                model.Guid = Guid.NewGuid().ToString();
            }
            db.Set<C_DIC_GUIZE>().AddOrUpdate(model);
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
        [HttpGet]
        public object GetGOODSBILLList_cb(int limit, int offset, string GBNO, string C_GOODSAGENT_NAME, string CreatTime, string CreatTime1, string BLNO, string C_GOODS, string HuoZhu, string ShipName, string VGNO)
        {

            var wherelambda = ExtLinq.True<C_TB_HC_GOODSBILL>();
            OperatorModel loginModel = OperatorProvider.Provider.GetCurrent();
            int yuanquid = loginModel.YuanquID.ToInt();
            wherelambda = wherelambda.And(t => t.YuanQuID == yuanquid);
            wherelambda = wherelambda.And(t => t.State != "待审核");
            wherelambda = wherelambda.And(t => t.State != "被驳回");
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
                // C_TB_HC_CONTRACT Model_CONTRACT = db.C_TB_HC_CONTRACT.FirstOrDefault(n => n.Guid == itsms_GoodsBill.CONTRACT_Guid);//查找合同
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
                GetTallyBllList_fy_hj(Convert.ToInt32(itsms_GoodsBill.ID), out decimal? Fyjehj, out decimal? Fyjehj_Sh, out decimal? Kpjehj_Sh);
                itsms_GoodsBill.Fyjehj = Fyjehj.ToDecimal(2);
                itsms_GoodsBill.KunCun = KuCun.ToString();
                itsms_GoodsBill.KunCunW = KuCunW.ToDecimal(3).ToString();
                List<C_TB_JIESUAN> list_JieSuan = db.C_TB_JIESUAN.Where(n => n.GoodsBill_id == itsms_GoodsBill.ID).OrderBy(n => n.ID).ToList();
                itsms_GoodsBill.Lkjehj = list_JieSuan.Sum(n => n.LaiKuanJinE).ToDecimal(2);
                itsms_GoodsBill.Sryk = (Fyjehj - list_JieSuan.Sum(n => n.LaiKuanJinE)).ToDecimal(2);
                List<C_TB_CHENGBEN> list_ChengBen = db.C_TB_CHENGBEN.Where(n => n.GoodsBill_id == itsms_GoodsBill.ID).OrderBy(n => n.ID).ToList();
                itsms_GoodsBill.ChengBenJinE = list_ChengBen.Sum(n => n.ChengBenJinE).ToDecimal(2);
                itsms_GoodsBill.LiRunZongE = (Fyjehj - list_ChengBen.Sum(n => n.ChengBenJinE)).ToDecimal(2);
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
                itsms_GoodsBill.ShiJiJInKu = ShiJiJInKu.ToDecimal(3);
                itsms_GoodsBill.ShiJiChuKu = ShiJiChuKu.ToDecimal(3);
                itsms_GoodsBill.KaiPiaoJineHJ = list_JieSuan.Sum(n => n.KaiPiaoJinE).ToDecimal(2);
                itsms_GoodsBill.KaiPiaoShuiHouHJ = Kpjehj_Sh.ToDecimal(2);
                itsms_GoodsBill.WeiKaiPiaoJineHJ = (Fyjehj - list_JieSuan.Sum(n => n.KaiPiaoJinE)).ToDecimal(2);
                itsms_GoodsBill.WeiKaiPiaoShuiHouHJ = (Fyjehj_Sh - Kpjehj_Sh).ToDecimal(2);
                itsms_GoodsBill.YinShouZhangKuanYue = (list_JieSuan.Sum(n => n.LaiKuanJinE) - list_JieSuan.Sum(n => n.KaiPiaoJinE)).ToDecimal(2);
                itsms_GoodsBill.LiRunZongE = (Fyjehj - list_ChengBen.Sum(n => n.ChengBenJinE)).ToDecimal(2);
                itsms_GoodsBill.YikaiPiaoYinShouYuE = (list_JieSuan.Sum(n => n.KaiPiaoJinE) - list_JieSuan.Sum(n => n.LaiKuanJinE)).ToDecimal(2);
                itsms_GoodsBill.WeiKaiPiaoYingShouYuE = ((Fyjehj - list_JieSuan.Sum(n => n.KaiPiaoJinE)) - list_JieSuan.Sum(n => n.YuShouJinE)).ToDecimal(2);
            }
            foreach (var itsms_GoodsBill in list)
            {
                List<C_TB_HC_CONSIGN> list_CONSIGN = db.C_TB_HC_CONSIGN.Where(n => n.GOODSBILL_ID == itsms_GoodsBill.ID).ToList();//查找委托
                List<C_TB_HS_TALLYBILL> list_TALLYBILL_hc = new List<C_TB_HS_TALLYBILL>();
                decimal? ShuLiang = 0;
                decimal? ChengBenFeiLv = 0;
                foreach (var items in list_CONSIGN)//获取票货下面所有理货单
                {
                    List<C_TB_HS_TALLYBILL> list_TALLYBILL = db.C_TB_HS_TALLYBILL.Where(n => n.CONSIGN_ID == items.ID).ToList();
                    foreach (var items_TallBill in list_TALLYBILL)
                    {
                        list_TALLYBILL_hc.Add(items_TallBill);
                    }
                }

                C_TB_HC_CONTRACT Model_CONTRACT = db.C_TB_HC_CONTRACT.FirstOrDefault(n => n.Guid == itsms_GoodsBill.CONTRACT_Guid);//查找合同
                if (Model_CONTRACT != null)
                {
                    List<C_TB_HC_CONTRACT_DETAILED> list_DETAILED = db.C_TB_HC_CONTRACT_DETAILED.Where(n => n.CONTRACT_Guid == Model_CONTRACT.Guid && n.ChengBenFeiLv != null).ToList();
                    foreach (var items in list_DETAILED)
                    {
                        C_TB_CHENGBEN model = db.C_TB_CHENGBEN.FirstOrDefault(n => n.GoodsBill_id == itsms_GoodsBill.ID && n.FeiMuZhongLei == items.FeiMuZhongLei && n.Type == items.Type) ?? new C_TB_CHENGBEN();
                        if (items.ChengBenJiFeiYiJu == "进库")
                        {
                            ShuLiang = list_TALLYBILL_hc.Where(n => n.CODE_OPSTYPE == "进库" && n.Type != "清场").Sum(n => n.WEIGHT);
                        }
                        if (items.ChengBenJiFeiYiJu == "出库")
                        {
                            ShuLiang = list_TALLYBILL_hc.Where(n => n.CODE_OPSTYPE == "出库" && n.Type != "清场").Sum(n => n.WEIGHT);
                        }
                        if (items.ChengBenJiFeiYiJu == "计划量")
                        {
                            ShuLiang = itsms_GoodsBill.PLANWEIGHT;
                        }
                        if (items.ChengBenJiFeiYiJu != "进库" && items.ChengBenJiFeiYiJu != "出库" && items.ChengBenJiFeiYiJu != "计划量")
                        {
                            ShuLiang = list_TALLYBILL_hc.Where(n => n.Type != "清场").Sum(n => n.WEIGHT);
                        }
                        if (!string.IsNullOrEmpty(items.ChengBenFeiLv.ToString()))
                        {
                            ChengBenFeiLv = items.ChengBenFeiLv;
                        }
                        model.ShuLiang = ShuLiang;
                        model.FeiMuZhongLei = items.FeiMuZhongLei;
                        model.ChengBenJinE = ShuLiang * ChengBenFeiLv;
                        model.ChengBenFeiLv = ChengBenFeiLv;
                        model.GoodsBill_id = itsms_GoodsBill.ID;
                        model.Type = items.Type;
                        db.Set<C_TB_CHENGBEN>().AddOrUpdate(model);

                    }
                    db.SaveChanges();
                }


            }

            int total = count;
            object rows = list;
            return Json(new { total = total, rows = rows }, JsonRequestBehavior.AllowGet);

        }
        public JsonResult DelByGuid(string Guid)
        {
            EFHelpler<C_DIC_GUIZE> ef = new EFHelpler<C_DIC_GUIZE>();
            try
            {

                C_DIC_GUIZE model_GUIZE = db.C_DIC_GUIZE.Find(Guid) ?? new C_DIC_GUIZE();
                ef.delete(model_GUIZE);

                return Json(new AjaxResult { state = ResultType.success.ToString(), message = "成功" }.ToJson());
            }
            catch (Exception e)
            {
                return Json(new AjaxResult { state = ResultType.error.ToString(), message = e.Message }.ToJson());
            }
        }
        public ActionResult TiJiao_FaPiao_ym(int id, string FMZhongLei, string Type)
        {
            C_TB_HC_GOODSBILL model = db.C_TB_HC_GOODSBILL.Find(id);
            ViewBag.FMZhongLei = FMZhongLei;
            ViewBag.Type = Type;
            return View(model);
        }
        /// <summary>
        /// 提交发票到电子口岸
        /// </summary>
        /// <param name="id"></param>
        /// <param name="INVOICETYPE"></param>
        /// <param name="CODE_NOTETYPE"></param>
        /// <param name="CODE_NOTETEMPLATE"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult TiJiao_FaPiao(int id, string FMZhongLei, string Type, string INVOICETYPE, string CODE_NOTETYPE, string CODE_NOTETEMPLATE, string IVDISPLAY, string REMARK)
        {
            IVDISPLAY = IVDISPLAY.Trim();
            REMARK = REMARK.Trim();
            OperatorModel loginModel = OperatorProvider.Provider.GetCurrent();
            try
            {
                List<FiITEM> list = new List<FiITEM>();
                C_TB_HC_GOODSBILL model_GoodsBill = db.C_TB_HC_GOODSBILL.Find(id) ?? new C_TB_HC_GOODSBILL();
                C_Dic_YuanQu model_YuanQu = db.C_Dic_YuanQu.Find(loginModel.YuanquID) ?? new C_Dic_YuanQu();
                C_TB_HC_CONTRACT model_CONTRACT = db.C_TB_HC_CONTRACT.FirstOrDefault(n => n.Guid == model_GoodsBill.CONTRACT_Guid);//查找合同
                GetTallyBllList_fy_hj(Convert.ToInt32(model_GoodsBill.ID), out decimal? Fyjehj, out decimal? Fyjehj_Sh, out decimal? Kpjehj_Sh);
                ToOutController ToOut = new ToOutController();
                DataTableController dt = new DataTableController();
                List<Stock_Money> list_dt = dt.Get_FeiMuXX_New(Convert.ToInt32(model_GoodsBill.ID), model_GoodsBill.CONTRACT_Guid, FMZhongLei, Type);
                int i = 0;
                foreach (var items in list_dt)
                {
                    i++;
                    FiITEM model = new FiITEM()
                    {
                        SERIAL = i.ToString(),
                        CODECHARGETYPE = items.FeiMuZhongLeiCode,
                        //CODECHARGETYPE = "CCGS1008",

                        CODECARGO = "",
                        CODEPACK = "",
                        SUMMARY = "",
                        QUANTITY = items.ShuLiang.ToString(),
                        RATE = items.DanJia.ToString(),
                        CODE_MEASURE = "01",
                        PRICE = items.FeiYong.ToString(),
                        ZKE = "0",
                        SFKC = "2",
                        TDH = "0",
                        FZ = "0",
                    };
                    list.Add(model);
                }
                string entrustId = null;
                if (model_CONTRACT != null && model_CONTRACT.EntrustID != null)
                {
                    entrustId = model_CONTRACT.EntrustID;
                }

                if (string.IsNullOrWhiteSpace(entrustId))
                {
                    return Json(new AjaxResult { state = ResultType.error.ToString(), message = "合同中委托人信息错误" });
                }

                string creattime = Convert.ToDateTime(model_GoodsBill.CreatTime).ToString("yyyyMMdd");//24小时
                string xml = ToOut.CreateXML(model_GoodsBill.GBNO, IVDISPLAY, model_YuanQu.CODECOMPANY, entrustId, Fyjehj.ToDouble(), REMARK, "1", "1", "", model_GoodsBill.ShipName, model_GoodsBill.VGNO, creattime, "82C2B359F1480218E053A86401690218", loginModel.UserName, model_GoodsBill.C_GOODSAGENT_NAME, "", "16", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "7", model_YuanQu.CODECOMPANY, DateTime.Now.Year.ToString(), DateTime.Now.ToDateString(), "", "", "", "", "", "", "", INVOICETYPE, "", "", "", "", "", "", "", "", "", "0", model_YuanQu.CODECOMPANY, CODE_NOTETYPE, CODE_NOTETEMPLATE, list);
                string resutl = ToOut.AppendRBill(xml);//发送到电子口岸
                if (resutl == "1")
                {
                    //保存到本地库
                    AddKAIPIAO(model_GoodsBill.GBNO, IVDISPLAY, model_YuanQu.CODECOMPANY, entrustId, Fyjehj.ToDouble(), REMARK, "1", "1", "", model_GoodsBill.ShipName, model_GoodsBill.VGNO, creattime, "82C2B359F1480218E053A86401690218", loginModel.UserName, model_GoodsBill.C_GOODSAGENT_NAME, "", "16", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "7", model_YuanQu.CODECOMPANY, DateTime.Now.Year.ToString(), DateTime.Now.ToDateString(), "", "", "", "", "", "", "", INVOICETYPE, "", "", "", "", "", "", "", "", "", "0", model_YuanQu.CODECOMPANY, CODE_NOTETYPE, CODE_NOTETEMPLATE, list);
                    B_TB_KAIPIAOJILU model_jILu = new B_TB_KAIPIAOJILU();
                    model_jILu.Guid = Guid.NewGuid().ToString();
                    model_jILu.GoodsBillId = id;
                    model_jILu.FeiMuZhongLei = FMZhongLei;
                    model_jILu.Type = Type;
                    model_jILu.CreatTime = DateTime.Now;
                    //model_GoodsBill.State_FaPiao = "开票信息已提交";
                    db.Set<B_TB_KAIPIAOJILU>().AddOrUpdate(model_jILu);
                    db.SaveChanges();
                    return Json(new AjaxResult { state = ResultType.success.ToString(), message = "成功" });
                }
                else
                {
                    return Json(new AjaxResult { state = ResultType.error.ToString(), message = resutl });
                }


            }
            catch (Exception e)
            {
                return Json(new AjaxResult { state = ResultType.error.ToString(), message = e.Message });
            }
        }
        public bool AddKAIPIAO(string IVNO, string IVDISPLAY, string CODECOMPANY, string CODECLIENT,
    double TOTAL, string REMARK, string CODEINOUT, string CODETRADE, string VGNO, string VESSEL, string VOYAGE,
    string CREATETIME, string CREATOR, string CREATORNAME, string PAYER, string PAPERNO, string CODEBILLTYPE, string TYR, string SHR, string FZ, string DZ, string YFZ,
    string ZDZ, string CZ, string FJ, string VEHICLES, string DCLC, string TOTALAMOUNT, string PLPB, string ZBPB, string TOTALWEIGHT, string QRZL,
    string UPPERTOTAL, string DRAWER, string CHECKER, string CODE_IVTEMPLATE, string CODE_BLINE, string CODE_DEPARTMENT, string PERIOD, string DRAWTIME, string QYD, string MDD
    , string FUZHU, string DLGRQ, string ZHG, string XHG, string YZM, string INVOICETYPE, string DEPOSER, string DEPOSETIME, string IVNO_HC, string BZ
    , string FPDM, string YFPDM, string YFPHM, string FPDM_ZZ, string PAPERNO_ZZ, string MARK_DS, string CODE_PAYER, string CODE_NOTETYPE, string CODE_NOTETEMPLATE, List<FiITEM> list)
        {

            using (var scope = new TransactionScope(TransactionScopeOption.Required))//异常回滚
            {

                try
                {
                    B_TB_KAIPIAO model = new B_TB_KAIPIAO();
                    model.Guid = Guid.NewGuid().ToString();
                    model.AddTime = DateTime.Now;
                    model.IVNO = IVNO;
                    model.IVDISPLAY = IVDISPLAY;
                    model.CODECOMPANY = CODECOMPANY;
                    model.CODECLIENT = CODECLIENT;
                    model.TOTAL = TOTAL.ToString();
                    model.CODEINOUT = CODEINOUT;
                    model.CODETRADE = CODETRADE;
                    model.VGNO = VGNO;
                    model.VESSEL = VESSEL;
                    model.VOYAGE = VOYAGE;
                    model.CREATETIME = CREATETIME;
                    model.CREATOR = CREATOR;
                    model.CREATORNAME = CREATORNAME;
                    model.PAYER = PAYER;
                    model.PAPERNO = PAPERNO;
                    model.CODEBILLTYPE = CODEBILLTYPE;
                    model.TYR = TYR;
                    model.SHR = SHR;
                    model.FZ = FZ;
                    model.DZ = DZ;
                    model.YFZ = YFZ;
                    model.ZDZ = ZDZ;
                    model.CZ = CZ;
                    model.FJ = FJ;
                    model.VEHICLES = VEHICLES;
                    model.DCLC = DCLC;
                    model.TOTALAMOUNT = TOTALAMOUNT;
                    model.PLPB = PLPB;
                    model.ZBPB = ZBPB;
                    model.TOTALWEIGHT = TOTALWEIGHT;
                    model.QRZL = QRZL;
                    model.UPPERTOTAL = UPPERTOTAL;
                    model.DRAWER = "开票人";
                    model.CHECKER = CHECKER;
                    model.CODE_IVTEMPLATE = CODE_IVTEMPLATE;
                    model.CODE_BLINE = CODE_BLINE;
                    model.CODE_DEPARTMENT = CODE_DEPARTMENT;
                    model.PERIOD = PERIOD;
                    model.DRAWTIME = DRAWTIME;
                    model.QYD = QYD;
                    model.MDD = MDD;
                    model.IVNO = IVNO;
                    model.FUZHU = FUZHU;
                    model.DLGRQ = DLGRQ;
                    model.ZHG = ZHG;
                    model.XHG = XHG;
                    model.YZM = YZM;
                    model.INVOICETYPE = INVOICETYPE;
                    model.DEPOSER = DEPOSER;
                    model.DEPOSETIME = DEPOSETIME;
                    model.IVNO_HC = IVNO_HC;
                    model.BZ = BZ;
                    model.FPDM = FPDM;
                    model.YFPDM = YFPDM;
                    model.YFPHM = YFPHM;
                    model.FPDM_ZZ = FPDM_ZZ;
                    model.PAPERNO_ZZ = PAPERNO_ZZ;
                    model.MARK_DS = MARK_DS;
                    model.CODE_PAYER = CODE_PAYER;
                    model.CODE_NOTETYPE = CODE_NOTETYPE;
                    model.CODE_NOTETEMPLATE = CODE_NOTETEMPLATE;
                    OperatorModel loginModel = OperatorProvider.Provider.GetCurrent();
                    model.YuanQuID = loginModel.YuanquID.ToInt();
                    db.Set<B_TB_KAIPIAO>().AddOrUpdate(model);
                    db.SaveChanges();
                    List<B_TB_KAIPIAO_XIANGXI> xiangxIlist = new List<B_TB_KAIPIAO_XIANGXI>();
                    foreach (var l in list)
                    {

                        B_TB_KAIPIAO_XIANGXI modelXiangxi = new B_TB_KAIPIAO_XIANGXI();
                        modelXiangxi.Guid = Guid.NewGuid().ToString();
                        modelXiangxi.GoodsBillId = IVNO;
                        modelXiangxi.SERIAL = l.SERIAL;
                        modelXiangxi.CODECARGO = l.CODECARGO;
                        modelXiangxi.CODEPACK = l.CODEPACK;
                        modelXiangxi.SUMMARY = l.SUMMARY;
                        modelXiangxi.QUANTITY = l.QUANTITY;
                        modelXiangxi.RATE = l.RATE;
                        modelXiangxi.CODE_MEASURE = l.CODE_MEASURE;
                        modelXiangxi.PRICE = l.PRICE;
                        modelXiangxi.ZKE = l.ZKE;
                        modelXiangxi.SFKC = l.SFKC;
                        modelXiangxi.TDH = l.TDH;
                        modelXiangxi.FZ = l.FZ;
                        xiangxIlist.Add(modelXiangxi);
                    }

                    EFHelpler<B_TB_KAIPIAO_XIANGXI> hl = new EFHelpler<B_TB_KAIPIAO_XIANGXI>();
                    hl.add(xiangxIlist.ToArray());
                    scope.Complete();
                    return true;
                }


                catch (Exception ex)
                {
                    return false;
                }


            }


        }
    }
}