using OracleBase.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace OracleBase.UserDefinModel
{
     class Api_weituoModel
    {
        [DataMember]
        public String code { get; set; }
        [DataMember]
        public String msg { get; set; }
        [DataMember]
        public Api_weituoModelData data { get; set; }


    }
    class Api_weituoModelData
    {
        [DataMember]
        public List<C_TB_HC_CONSIGN> list = new List<C_TB_HC_CONSIGN>();
        [DataMember]
        public int totalCount { get; set; }

    }
}