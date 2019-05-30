using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;
using Main.HelpClass;
using MainBLL.BaoShui;
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
    public class BaoShuiBaoBiaoController : Controller
    {
        private Entities db = new Entities();
        // GET: ck/BaoShuiBaoBiao
        public ActionResult Index()
        {
            return View();
        }
        //public object GetRiBaoBiaoList(int limit, int offset, string Time1, string Time2)
        //{

        //    DateTime start;
        //    DateTime end;
        //    OperatorModel loginModel = OperatorProvider.Provider.GetCurrent();
        //    List<C_GOODS> List_DuiCun =db.C_GOODS.ToList();
        //    start = Convert.ToDateTime(DateTime.Now.AddMonths(-1).ToString("yyyy-MM-21") + " 00:00:00");//上个月的21号
        //    end = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-20") + " 23:59:59");
        //    foreach (var items_Goods in List_DuiCun)
        //    {
        //        int jishu = 0;
        //        List<C_TB_HC_GOODSBILL> List_GOODSBILL = db.C_TB_HC_GOODSBILL.Where(n=>n.C_GOODS== items_Goods.GoodsName).ToList();
        //        foreach (var items_GOODSBILL in List_GOODSBILL)
        //        {
        //            jishu++;
        //            decimal? ZyInW = 0;
        //            decimal? AllInW = 0;
        //            decimal? ZyInN = 0;
        //            decimal? AllInN = 0;
        //            decimal? ZyOutW = 0;
        //            decimal? AllOutW = 0;
        //            decimal? ZyOutN = 0;
        //            decimal? AllOutN = 0;
        //            decimal? KuCunW = 0;
        //            decimal? KuCunN = 0;

        //            List<C_TB_HC_CONSIGN> List_CONSIGN = db.C_TB_HC_CONSIGN.Where(n => n.GOODSBILL_ID == items_GOODSBILL.ID).ToList();
        //            foreach (var items_CONSIGN in List_CONSIGN)
        //            {
        //                List<C_TB_HS_TALLYBILL> List_TALLYBILL = db.C_TB_HS_TALLYBILL.Where(n => n.CONSIGN_ID == items_CONSIGN.ID&&n.SIGNDATE<=end).ToList();
        //                foreach (var items_TALLYBILL in List_TALLYBILL)
        //                {
        //                    if (items_TALLYBILL.CODE_OPSTYPE == "进库")
        //                    {
        //                        KuCunN += items_TALLYBILL.AMOUNT ;
        //                        KuCunW += items_TALLYBILL.WEIGHT;
        //                        AllInW += items_TALLYBILL.WEIGHT;
        //                        AllInN += items_TALLYBILL.AMOUNT;

        //                    }
        //                    if (items_TALLYBILL.CODE_OPSTYPE == "出库")
        //                    {
        //                        KuCunN -= items_TALLYBILL.AMOUNT ;
        //                        KuCunW -= items_TALLYBILL.WEIGHT;
        //                        AllOutW += items_TALLYBILL.WEIGHT;
        //                        AllOutN += items_TALLYBILL.AMOUNT;
        //                    }
        //                }
        //                List<C_TB_HS_TALLYBILL> List_TALLYBILL_Today = db.C_TB_HS_TALLYBILL.Where(n => n.CONSIGN_ID == items_CONSIGN.ID && n.SIGNDATE >=Convert.ToDateTime(Time1+ " 00:00:00") && n.SIGNDATE <= Convert.ToDateTime(Time1 + " 23:59:59")).ToList();
        //                foreach (var items_TALLYBILL_Today in List_TALLYBILL_Today)
        //                {
        //                    if (items_TALLYBILL_Today.CODE_OPSTYPE == "进库")
        //                    {
        //                        ZyInW += items_TALLYBILL_Today.WEIGHT;
        //                        ZyInN += items_TALLYBILL_Today.AMOUNT;

        //                    }
        //                    if (items_TALLYBILL_Today.CODE_OPSTYPE == "出库")
        //                    {
        //                        ZyOutW += items_TALLYBILL_Today.WEIGHT;
        //                        ZyOutN += items_TALLYBILL_Today.AMOUNT;
        //                    }
        //                }

        //            }
        //            if (jishu<= List_GOODSBILL.Count)
        //            {
        //                RiBaoBiao model = new RiBaoBiao()
        //                {
        //                    HuoZHong = items_GOODSBILL.C_GOODS,
        //                    ChuanMing = items_GOODSBILL.ShipName,
        //                    DanWei = "吨",
        //                    TiDanShu= items_GOODSBILL.PIECEWEIGHT,
        //                    ZyInW= ZyInW,
        //                    AllInW = AllInW,
        //                    ZyInN = ZyInN,
        //                    AllInN = AllInN,
        //                    ZyOutW = ZyOutW,
        //                    AllOutW = AllOutW,
        //                    ZyOutN = ZyOutN,
        //                    AllOutN = AllOutN,
        //                    KuCunW = KuCunW,
        //                    KuCunN = KuCunN,
        //                    Beizhu="",


        //                };
        //            }
                   
        //        }

        //    }
        //}
    }
}