using OracleBase.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace OracleBase.UserDefinModel
{
    class API_piaohuoModel
    {
        [DataMember]
        public String code { get; set; }
        [DataMember]
        public String msg { get; set; }
        [DataMember]
       // public List<C_TB_HC_GOODSBILL> data = new List<C_TB_HC_GOODSBILL>();
        public Api_piaohuoModelData data { get; set; }
       

    }
    class Api_piaohuoModelData
    {
        [DataMember]
        public List<C_TB_HC_GOODSBILL> list = new List<C_TB_HC_GOODSBILL>();
        [DataMember]
        public int totalCount { get; set; }

    }
    public class Api_piaohuoModelDataList
    {
        [DataMember]
        public int id { get; set; }
        [DataMember]
        public String title { get; set; }
        [DataMember]
        public DateTime? createTime { get; set; }
        [DataMember]
        public String content { get; set; }
    }
}