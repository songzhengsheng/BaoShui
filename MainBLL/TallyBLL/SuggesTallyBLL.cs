using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainBLL.TallyBLL
{
   public class SuggesTallyBLL
    {
        public string name { get; set; }
        public string ename { get; set; }

        public string jobtitle { get; set; }
        public string hengzhong { get; set; }
    }
    public class GoodBillAndConsign
    {
        public string GoodBLNO { get; set; }
        
      
        public string CODE_INOUT { get; set; }
        public string NWM { get; set; }
        public Nullable<decimal> C_GOODSAGENT_ID { get; set; }
        public string CODE_PACK_NAME { get; set; }
        public Nullable<decimal> GoodPIECEWEIGHT { get; set; }
        public string VGNO { get; set; }

        public string MARK_GOOGSBILLTYPE { get; set; }
        public string MARK { get; set; }

        public Nullable<decimal> GoodPLANAMOUNT { get; set; }
        public Nullable<decimal> GoodPLANWEIGHT { get; set; }


        public string C_GOODS { get; set; }
    
        public string BLNO { get; set; }

        public string CODE_OPERATION { get; set; }
        public string WeiTuoRen { get; set; }
        public string ShouHuoRen { get; set; }
        public string CODE_TRANS { get; set; }
        public Nullable<System.DateTime> WeiTuoTime { get; set; }

        public Nullable<decimal> PLANAMOUNT { get; set; }
        public Nullable<decimal> PLANWEIGHT { get; set; }
        public Nullable<decimal> PLANVOLUME { get; set; }
        public string CONTAINERTYPE { get; set; }
        public Nullable<decimal> CONTAINERNUM { get; set; }
        public string PAPERYNO { get; set; }
        public string BoolQuanLuYun { get; set; }
        public string SPONSOR { get; set; }
        public string Phone { get; set; }

        public string BeiZhu { get; set; }
        public string HengZhong { get; set; }
     
    }
    public class KuCun
    {
        public string YuanQU_Name { get; set; }
        public string SECTION_Name { get; set; }
        public string STORAG_Name { get; set; }
        public decimal STORAG { get; set; }
        public string CODE_SECTION_Name { get; set; }
        public decimal AMOUNT { get; set; }
        public decimal WEIGHT { get; set; }
    }
    public class KuCun_XQ
    {

        public string BGON { get; set; }
        public Nullable<decimal> AMOUNT { get; set; }
        public Nullable<decimal> WEIGHT { get; set; }
        public string REMARK { get; set; }
        public string huodai { get; set; }
        public string huowu { get; set; }
    }
    public class HeTongTallyBLL
    {
        public string ContoractNumber { get; set; }
        public string Guid { get; set; }
    }
}
