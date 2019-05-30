using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainBLL.ToOut
{
   public class FI
    {

    }

   public class FiKeHuData
   {
       public string CODE { get; set; }
       public string NAME { get; set; }
        public string LOGOGRAM { get; set; }
        public string FULLNAME { get; set; }
        public string INVOICENAME { get; set; }
    }

   public class FiITEM
    {
        [Description("费目编码 需要对照BR元数据")]
       public string SERIAL { get; set; }/*!--费目编码 需要对照BR元数据--!*/
       public string CODECHARGETYPE { get; set; }
       public string CODECARGO { get; set; }
       public string CODEPACK { get; set; }
       public string SUMMARY { get; set; }
       public string QUANTITY { get; set; }
        public string CODE_MEASURE { get; set; }
        public string RATE { get; set; }
       public string PRICE { get; set; }
       public string ZKE { get; set; }
       public string SFKC { get; set; }
       public string TDH { get; set; }
       public string FZ { get; set; }
        

    }
}
