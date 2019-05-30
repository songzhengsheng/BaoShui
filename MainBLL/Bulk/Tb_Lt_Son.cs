using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainBLL.Bulk
{
  public  class Tb_Lt_Son
    {
        public int ID { get; set; }
        public int FatherID { get; set; }
        public string Info { get; set; }
        public Nullable<System.DateTime> Addtime { get; set; }
        public string UserName { get; set; }
        public string UserId { get; set; }

 
    }
}
