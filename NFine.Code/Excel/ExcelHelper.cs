/*******************************************************************************
 * Copyright © 2016 NFine.Framework 版权所有
 * Author: NFine
 * Description: NFine快速开发平台
 * Website：http://www.nfine.cn
*********************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace NFine.Code
{
    public class ExcelHelper
    {
        /// <summary> /// 将一组对象导出成EXCEL /// </summary>
        /// <typeparam name="T">要导出对象的类型</typeparam>
        /// <param name="objList">一组对象</param>
       /// <param name="FileName">导出后的文件名</param>
       /// <param name="columnInfo">列名信息</param>
        public void ExExcel<T>(List<T> objList, Dictionary<string, string> columnInfo,string FileName) {
            if (columnInfo.Count == 0)
            {
                return;
            }

            if (objList.Count == 0)
            {
                return;
            } //生成EXCEL的HTML
              string excelStr = "";
            Type myType = objList[0].GetType(); //根据反射从传递进来的属性名信息得到要显示的属性
            List<System.Reflection.PropertyInfo> myPro = new List<System.Reflection.PropertyInfo>();
            foreach (string cName in columnInfo.Keys)
            {
                System.Reflection.PropertyInfo p = myType.GetProperty(cName);
                if (p != null)
                {
                    myPro.Add(p);
                    excelStr += columnInfo[cName] + "\t'";
                }
            } //如果没有找到可用的属性则结束

            if (myPro.Count == 0)
            {
                return;
            } excelStr += "\r'";
            foreach (T obj in objList)
            {
                foreach (System.Reflection.PropertyInfo p in myPro)
                {
                    excelStr += p.GetValue(obj, null) + "\t'";
                }
                excelStr += "\r'";
            } 
            //输出EXCEL
            HttpResponse rs = System.Web.HttpContext.Current.Response; rs.ContentEncoding = System.Text.Encoding.GetEncoding("GB2312");
            rs.AppendHeader("content-disposition", "attachment;filename=" + System.Web.HttpUtility.UrlEncode(FileName, System.Text.Encoding.UTF8) + ".xls");
            rs.ContentType = "application/ms-excel";
            rs.Write(excelStr);
            rs.End();
        }

    }
}
