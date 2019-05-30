using System.Collections.Generic;
using System.Web.Mvc;
namespace NFine.Code.Select
{
    /// <summary>
    /// 下拉选项帮助类
    /// </summary>
    public class SelectHelp
    {
        /// <summary>
        /// List转SelectListItem
        /// </summary>
        /// <typeparam name="T">Model对象</typeparam>
        /// <param name="t">集合</param>
        /// <param name="text">显示值-属性名</param>
        /// <param name="value">显示值-属性名</param>
        /// <param name="empId"></param>
        /// <returns></returns>
        public static List<SelectListItem> CreateSelect<T>(IList<T> t, string text, string value, string empId)
        {
            List<SelectListItem> l = new List<SelectListItem>();
            foreach (var item in t)
            {
                var propers = item.GetType().GetProperty(text);
                var valpropers = item.GetType().GetProperty(value);
                l.Add(new SelectListItem
                {
                    Text = propers.GetValue(item, null).ToString(),
                    Value = valpropers.GetValue(item, null).ToString(),
                    Selected = valpropers.GetValue(item, null).ToString() == empId
                });
            }
            return l;
        }
    }
}