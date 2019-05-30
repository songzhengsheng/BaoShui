using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IH.UserDefinModel
{
    public class PMonthCount
    {
        public int totalCount { get; set; }//总艘次
        public int inCount { get; set; }//内贸
        public int outCount { get; set; }//外贸
        public Nullable<double> tonnage { get; set; }//净吨
        public double? grosstons { get; set; }//船舶总吨

    }
}