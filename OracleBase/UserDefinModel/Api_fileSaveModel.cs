using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace OracleBase.UserDefinModel
{
    public class Api_fileSaveModel
    {

        [DataMember]
        public String code { get; set; }
        [DataMember]
        public String msg { get; set; }
        [DataMember]
        public Api_fileSaveData data { get; set; }

    }

    public class Api_fileSaveData
    {
        [DataMember]
        public string fileName { get; set; }
        [DataMember]
        public string fileType { get; set; }
        [DataMember]
        public string fileUrl { get; set; }
        [DataMember]
        public string guid { get; set; }
    }
}