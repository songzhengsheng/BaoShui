using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainBLL.Money
{
    /// <summary>
    /// 库存的月报表
    /// </summary>
    public class Stock_Money
    {
        public decimal ID;
        public string HuoMing;
        public decimal? ShuiLv;
        public decimal? DanJia;
        public string FMZhongLei;

        public string JiLiangDanWei;
        public string BeiZhu;
        public decimal? ShuLiang;
        public decimal? FeiYong;
        public DateTime? LiHuoShiJian;
        public decimal? MianCunQi;
        public int DuiCunShuLiang;
        public int GoodsBill_id;
        public string Type;
        public decimal? YiShou;
        public decimal? WeiShou;
        public decimal? ShuiHouJinE;
        public decimal? KaiPiaoJinE;
        public decimal? KaiPiaoShuiHou;
        public decimal? LaiKuanJinE;
        public decimal? WeiKaiPiaoJinE;
        public decimal? WeiKaiPiaoShuiHou;
        public decimal? YinSHouYuE;
        public decimal? HuiShouLv;
        public decimal? LaiKuanShuiHou;
        public decimal? WaiFuJinE;
        public decimal? Shuilv;
        public decimal? ShiJiShuiLv;
        public decimal? ChengBenFeiLv;
        public string ChengBenJiFeiYiJu;
        public decimal? YearKaiPiaoJinE;
        public decimal? one;
        public decimal? two;
        public decimal? three;
        public decimal? four;
        public string FeiMuZhongLeiCode;
        public string HuoMingCode;
        public string KaiPiao_State;



    }
    public class Stock_Money_DuiCun
    {
        public string HuoMing;
        public decimal? DanJia;
        public decimal? ShuLiang;
        public string FMZhongLei;
        public string JiLiangDanWei;
        public DateTime? RiQi;
        public decimal? MianCunQi;
        public decimal? DuiCunShuLiang;
        public decimal? JiFeiShuLiang;
        public decimal? BuJiFeiShuLiang;
        public decimal? FeiYong;
        public decimal? JinKu;
        public decimal? ChuKu;
        public string type;

    }
    public class Stock_Money_In
    {
        public DateTime? date_in;
        public decimal? InAllNum;

    }
    public class Stock_Money_Out
    {
        public DateTime? date_Out;
        public decimal? OutAllNum;

    }
    public class Stock_Money_FeiYong
    {
        public DateTime? date_in;
        public decimal? InAllYD;//余吨
        public DateTime? date_Out;
        public decimal? OutAllNum;
        public decimal? MianDuiQi;
        public decimal? DunTian;
        public decimal? FeiLv;
        public decimal? FeiYong;
    }
    public class Stock_Money_WeiKaiPiao//新路带累计未开票
    {
        public string DanWei;
        public string HuoZhong;
        public string ChuanMing;
        public decimal? BaoGanFei;
        public decimal? DuiCunFei;
        public decimal? YunFei;
        public decimal? GuoBangFei;
        public string QiaTaFeiYong;
        public decimal?HeJi;
        public string HangCi;
        public string TiDanHao;
        public decimal? QiTa;
    }
    public class Stock_Money_ShouRuHuiZOngBiao//新路带货物信息收入汇总表
    {
        public string HuoDai;
        public string HuoWu;
        public string ChuanMing;
        public DateTime? JianPiaoRiQi;
        public decimal? ShiJiJinKuLiang;
        public decimal? ShiJiChuKuLiang;
        public decimal? KuCunN;
        public decimal? KuCunW;
        public decimal? JianGuanFei;
        public decimal? BaoGanFei;
        public decimal? DuiCunFei;
        public decimal? GuoBangFei;
        public decimal? YunFei;
        public string QiYaFeiYong;
        public decimal? FeiYongHeJi;
        public decimal? YearKaiPiaoShu;
        public decimal? LeiJiKaiPiaoShu;
        public decimal? LeiJiWeiKaiPiaoShu;
        public decimal? LaiKuanJinE;
        public decimal? YinShouZhangKuanYuE;
        public decimal? ChengBenJinE;
        public decimal? LiRun;
        
    }
    public class Stock_Money_WanChengQingKuangBiao//新路带收入完成情况表
    {
        public decimal? ID;
        public string HuoDai;
        public string HuoWu;
        public string ChuanMing;
        public decimal? BaoGanFei;
        public decimal? DuiCunFei;
        public decimal? GuoBangFei;
        public decimal? YunFei;
        public string QiYaFeiYong;
        public decimal? FeiYongHeJi;
        public decimal? WeiKaiPiao;
        public decimal? one;
        public decimal? two;
        public decimal? three;
        public decimal? four;
        public decimal? QiTa;


    }
    public class Stock_Money_RiShouRu//日收入详情
    {
        public DateTime? Date;
        public decimal? FeiYong;
        public string FeiMuZhongLei;
        public string Type;


    }
    public class Stock_Money_EveryDayFeiYong//日收入详情
    {
        public DateTime? Date;
        public string HuoDai;
        public string HuoWu;
        public string ChuanMing;
        public string HangCi;
        public decimal? BaoGanFei;
        public decimal? DuiCunFei;
        public decimal? YunFei;
        public decimal? GuoBangFei;
        public string QiTaFeiYong;
    }
    public class Stock_Money_RiBaoBiao//日收入详情
    {
        public decimal ID;
        public string PiaoHuoBianMa;
        public string ChuanMing;
        public string TiDanHao;
        public string HuoDai;
        public string HuoZhong;
        public decimal? TiDanCaiJi;
        public string JianCeCaiJi;
        public string JianCeZhiShu;
        public decimal? InCar = 0;
        public decimal? InN = 0;
        public decimal? InW = 0;
        public decimal? OutCar = 0;
        public decimal? OutN = 0;
        public decimal? OutW = 0;
        public decimal? zkuj = 0;
        public decimal? zkus = 0;
        public decimal? yuein = 0;
        public decimal? yueout = 0;
        public decimal? allin = 0;
        public decimal? allout = 0;
        public string Hangci;


    }
  }
