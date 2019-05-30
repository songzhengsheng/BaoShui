using System;
using System.Runtime.Serialization;

namespace OracleBase.UserDefinModel
{
    [DataContract]
     class API_login
    {
        [DataMember]
        public String code { get; set; }
        [DataMember]
        public String msg { get; set; }
        [DataMember]
        public Data data { get; set; }

    }
    [DataContract]
    class Data
    {
        [DataMember]
        public int userId { get; set; }
        [DataMember]
        public String userName { get; set; }
        [DataMember]
        public decimal? roleId { get; set; }
        [DataMember]
        public String roleName { get; set; }
        [DataMember]
        public String email { get; set; }
        [DataMember]
        public decimal? YuanQuId { get; set; }
        [DataMember]
        public decimal? huodaiId { get; set; }
        [DataMember]
        public String huodaiName { get; set; }

    }
    
}