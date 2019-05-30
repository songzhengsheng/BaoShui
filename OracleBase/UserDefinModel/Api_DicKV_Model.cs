using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace OracleBase.UserDefinModel
{
    [DataContract]
    public class Api_DicKV_Model
    {
        [DataMember]
        public String code { get; set; }
        [DataMember]
        public String msg { get; set; }
        [DataMember]
        public Api_DicKV_ModelList data { get; set; }
    }
    [DataContract]
    public class Api_DicKV_ModelList
    {
        [DataMember]
        public List<Api_DicKV_ModelListKV> list = new List<Api_DicKV_ModelListKV>();

    }

    public class Api_DicKV_ModelListKV
    {
        [DataMember]
        public int value { get; set; }
        [DataMember]
        public string text { get; set; }
    }
}