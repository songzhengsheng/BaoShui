//------------------------------------------------------------------------------
// <auto-generated>
//    此代码是根据模板生成的。
//
//    手动更改此文件可能会导致应用程序中发生异常行为。
//    如果重新生成代码，则将覆盖对此文件的手动更改。
// </auto-generated>
//------------------------------------------------------------------------------

namespace OracleBase.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class SYS_OperateLogs
    {
        public string Id { get; set; }
        public string OperatorId { get; set; }
        public string Operator { get; set; }
        public Nullable<System.DateTime> OperateTime { get; set; }
        public string Operate { get; set; }
        public string Level { get; set; }
        public Nullable<decimal> LevelEnum { get; set; }
        public string Description { get; set; }
        public string Req { get; set; }
        public string reqUrl { get; set; }
        public string content { get; set; }
    }
}