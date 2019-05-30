using MainBLL.TallyBLL;
using OracleBase.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
namespace OracleBase.UserDefinModel
{
    class Api_kucunModel
    {
        [DataMember]
        public String code { get; set; }
        [DataMember]
        public String msg { get; set; }
        [DataMember]
        public Api_KuCunModelData data { get; set; }
    }
    class Api_KuCunModelData
    {
         public object list { get; set; }
        public int totalCount { get; set; }

    }
}