using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace OracleBase.UserDefinModel
{
    public class Api_KeyValue
    {
        [DataMember]
        public String code { get; set; }
        [DataMember]
        public String msg { get; set; }
        [DataMember]
        public Api_KeyValueModelList data { get; set; }
    }
    [DataContract]
    public class Api_KeyValueModelList
    {
        [DataMember]
        public List<Api_KeyValueModelListKV> list = new List<Api_KeyValueModelListKV>();

    }

    public class Api_KeyValueModelListKV
    {
        [DataMember]
        public string value { get; set; }
        [DataMember]
        public string text { get; set; }
    }
}