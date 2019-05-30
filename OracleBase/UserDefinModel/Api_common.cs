using System;
using System.Runtime.Serialization;

namespace OracleBase.UserDefinModel
{
    [DataContract]
    public class Api_common
    {
        [DataMember]
        public String code { get; set; }
        [DataMember]
        public String msg { get; set; }
    }
}