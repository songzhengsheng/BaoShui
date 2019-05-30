using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainBLL.GoodBill
{
    //票货的状态流程
    public enum GoodBillStateEnum
    {
        待提交审核,
        待经理审核,
        驳回,
        进行中,
        已生成x条委托,
        已完成
    }
   //委托的状态流程
    public enum WtStateEnum
   {
       待提交审核,
       待经理审核,
       驳回,
       进行中,
       已生成x条理货单,
       已完成
   }
}
