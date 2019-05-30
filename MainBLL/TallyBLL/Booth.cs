using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainBLL.TallyBLL
{
   public class Booth
    {
        public decimal ID { get; set; }
        public string BoothName { get; set; }
    }
    /// <summary>
    /// 场
    /// </summary>
    public class Storage
    {
        public decimal ID { get; set; }
        public string StorageName { get; set; }
    }

    public class Box
    {
        public decimal id { get; set; }
        public string text { get; set; }
        public string color { get; set; }
        public decimal? height { get; set; }
        public decimal? width { get; set; }
        public decimal? pageX { get; set; }
        public decimal? pageY { get; set; }
    }
}
