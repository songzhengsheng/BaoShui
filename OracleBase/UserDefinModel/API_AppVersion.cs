using System;
using System.Runtime.Serialization;

namespace OracleBase.UserDefinModel
{
    public class API_AppVersion
    {

        [DataMember]
        public String code { get; set; }
        [DataMember]
        public String msg { get; set; }
        [DataMember]
        public AppData data { get; set; }

    }

    public class AppData
    {
        [DataMember]
        public string status { get; set; }
        [DataMember]
        public string version { get; set; }
        [DataMember]
        public string title { get; set; }
        [DataMember]
        public string note { get; set; }
        [DataMember]
        public string url { get; set; }
    }
}