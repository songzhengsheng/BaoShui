/*
 * 这是一个操作日志记录类
 * 使用方法在方法名上加 [OperationLog(OperationLogAttribute.Operatetype.删除, OperationLogAttribute.ImportantLevel.危险操作, "删除合同附件")]
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MainBLL.SysModel;
using NFine.Code;
using OracleBase.Models;

namespace OracleBase.HelpClass.Sys
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class OperationLogAttribute: System.Web.Mvc.ActionFilterAttribute
    {
        private Entities db = new Entities();
        /// <summary>
        /// 操作日志
        /// </summary>
        /// <param name="operate">具体操作</param>
        public OperationLogAttribute(Operatetype operate, ImportantLevel level = ImportantLevel.一般操作, string description = null)
        {
            this.Operate = operate;
            Level = level;
            Description = description;
        }
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            filterContext.RequestContext.HttpContext.Request.InputStream.Position = 0;
            var byts = new byte[filterContext.RequestContext.HttpContext.Request.InputStream.Length];
            filterContext.RequestContext.HttpContext.Request.InputStream.Read(byts, 0, byts.Length);
            var req = System.Text.Encoding.Default.GetString(byts);
            //请求文本
            req = filterContext.RequestContext.HttpContext.Server.UrlDecode(req);
            //请求地址
            var reqUrl = filterContext.RequestContext.HttpContext.Request
                .AppRelativeCurrentExecutionFilePath;
            //返回结果
            var content = "";
            if (filterContext.Result != null)
            {
                content = "执行成功";
            }
            else
            {
                content = filterContext.Exception.Message + "," + filterContext.Exception.Source;
            }

            try
            {
                OperatorModel loginModel = OperatorProvider.Provider.GetCurrent();
                SYS_OperateLogs operateLog = new SYS_OperateLogs();
                operateLog.Id = Guid.NewGuid().ToString();
                operateLog.OperateTime = DateTime.Now;
                operateLog.Operate = Operate.ToString();
                operateLog.Description = Description;
                operateLog.Level = Level.ToString();
                operateLog.OperatorId = loginModel.UserId;
                operateLog.Operator = loginModel.UserName;
                operateLog.Req = req;
                operateLog.reqUrl = reqUrl;
                operateLog.content = content;
                db.SYS_OperateLogs.Add(operateLog);
                db.SaveChanges();
                base.OnActionExecuted(filterContext);
            }
            catch (Exception e)
            {
              
            
            }
   
        }



        /// <summary>
        /// 具体操作
        /// </summary>
        public Operatetype Operate { get; set; }
        public enum Operatetype { 添加,编辑,删除, 添加或编辑,审核 }
        /// <summary>
        /// 危险等级
        /// </summary>
        public ImportantLevel Level { get; set; }
        public enum ImportantLevel { 一般操作 = 0, 危险操作 = 1 }
        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; set; }
    }
}