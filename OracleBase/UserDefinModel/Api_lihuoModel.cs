using MainBLL.TallyBLL;
using OracleBase.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace OracleBase.UserDefinModel
{
    class Api_lihuoModel
    {
        [DataMember]
        public String code { get; set; }
        [DataMember]
        public String msg { get; set; }
        [DataMember]
        public Api_lihuoModelData data { get; set; }
    }
    class Api_lihuoModelData
    {
        [DataMember]
        public List<C_TB_HS_TALLYBILL> list = new List<C_TB_HS_TALLYBILL>();
        [DataMember]
        public int totalCount { get; set; }

    }
}