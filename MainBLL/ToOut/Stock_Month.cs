using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainBLL.ToOut
{
    /// <summary>
    /// 库存的月报表
    /// </summary>
   public  class Stock_Month
    {
        public int ID;
        public DateTime Time;
        public string Goodname;

        public string InAMOUNT;
        public string InWEIGHT;

        public string OutAMOUNT;
        public string OutWEIGHT;

        public string AllAMOUNT;
        public string AllWEIGHT;


    }
    /// <summary>
    /// 客户分析模型
    /// </summary>
    public class KehuFenxiEntry
    {
        public string name;
        public decimal? zuoyeliang;
        public decimal? shouru;
    }
    /// <summary>
    /// 客户环比模型
    /// </summary>
    public class HuanBiEntry
    {
        public string date;
        public decimal? NowYear;
        public decimal? LastYear;
        public decimal? TongBi;

    }
    /// <summary>
    /// 连云港新路带物流有限公司年度开票明细
    /// </summary>
    public class KaiPiaoMingXientry
    {
        public string KaiPiaoDanWei;
        public string VGNO;
        public string BLNO;
        public decimal? KaiPiaoJinE;
        public decimal? ChengBenJinE;
        public decimal? LiRun;
        public string ChuanMing;
        public DateTime? RiQi;
        public string HuoZhong;
        public string FeiYongLeiXing;
        public decimal? FeiLv;
        public decimal? ShuLiang;
        public decimal? FeiYong1;
        public decimal? FeiYong2;
        public decimal? KaiPiaoYinSHouFuKuanYuE;
        public string FuKuanDanWei;
        public decimal? LaiKuanJinE;
        public string BeiZhu;
    }
}
