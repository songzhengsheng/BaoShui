using System;
using OracleBase.Models;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace OracleBase.UserDefinModel
{
    class Api_Daiban
    {
        [DataMember]
        public String code { get; set; }
        [DataMember]
        public String msg { get; set; }
        [DataMember]
        public Api_DaibanData data { get; set; }
    }
    class Api_DaibanData
    {
        [DataMember]
        public List<C_TB_HC_CONTRACT> htList = new List<C_TB_HC_CONTRACT>();
        [DataMember]
        public List<C_TB_HC_GOODSBILL> phList = new List<C_TB_HC_GOODSBILL>();
        [DataMember]
        public List<C_TB_HC_CONSIGN> wtList = new List<C_TB_HC_CONSIGN>();
        [DataMember]
        public int totalCount { get; set; }

    }
}