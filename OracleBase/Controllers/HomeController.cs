using System;
using System.Collections;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Main.Handler;
using Main.HelpClass;
using NFine.Code;
using NFine.Code.Mail;
using OracleBase.Models;

namespace Main.Controllers
{
    public class HomeController : Controller
    {
        private Entities db = new Entities();

        /// <summary>
        /// 框架页面
        /// </summary>
        /// <returns></returns>
        [SignLoginAuthorize]
        public ActionResult Index()
        {
            OperatorModel loginModel = OperatorProvider.Provider.GetCurrent();           
            ViewBag.PeopleName = loginModel.UserName;
            int roleid = Convert.ToInt32(loginModel.RoleId);
            Sys_Role role = db.Sys_Role.FirstOrDefault(n => n.roleId == roleid);
            if (role != null) ViewBag.Name = role.roleName;            
            //拼接菜单
            CreatMenu cm = new CreatMenu();
            ViewBag.menu = cm.CreatMenusHPlus(roleid);
            ViewBag.role = loginModel.RoleId;
            var yuanquid = loginModel.YuanquID;
            C_Dic_YuanQu yuanquModel = db.C_Dic_YuanQu.Find(yuanquid);
            if (yuanquModel != null) ViewBag.yuanquname = yuanquModel.YuanQuName;
            if (loginModel.RoleId=="7")
            {
                int Ht = db.Set<C_TB_HC_CONTRACT>().Where(n => n.State == "待经理审核" && n.YuanQuID == loginModel.YuanquID).Count();
                int Ph = db.Set<C_TB_HC_GOODSBILL>().Where(n => n.State == "待经理审核" && n.YuanQuID == loginModel.YuanquID).Count();
                int Wt = db.Set<C_TB_HC_CONSIGN>().Where(n => n.State == "待经理审核" && n.YuanQuID == loginModel.YuanquID).Count();
                ViewBag.Ht = Ht;
                ViewBag.Ph = Ph;
                ViewBag.Wt = Wt;
                ViewBag.sum = Ht+ Ph+ Wt;
            }
            if (loginModel.RoleId == "3")
            {
                int Wt = db.Set<C_TB_HC_CONSIGN>().Where(n => n.State == "待审核" && n.YuanQuID == loginModel.YuanquID).Count();
                ViewBag.Wt = Wt;
            }
            return View();
        }

        //默认页
        [SignLoginAuthorize]
        public ActionResult HomePage()
        {
            OperatorModel loginModel = OperatorProvider.Provider.GetCurrent();
            decimal? YuanQu_id = loginModel.YuanquID;
            if (!string.IsNullOrEmpty(loginModel.YuanquID.ToString()))
            {
                C_Dic_YuanQu YuanQuModel = db.C_Dic_YuanQu.Find(YuanQu_id);
                if (!string.IsNullOrEmpty(YuanQuModel.PicPach))
                {
                    ViewBag.PicPach = YuanQuModel.PicPach;

                }
                else
                {
                    ViewBag.PicPach = "demo_picture.jpg";

                }
            }
            else
            {
                ViewBag.PicPach = "demo_picture.jpg";

            }
            return View();
        }

        /// <summary>
        /// 登录
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Login()
        {
         
            return View();
        }
        [HttpPost]
        public ActionResult Login(string userName, string passWord, string code)
        {
            //if (Session["nfine_session_verifycode"].IsEmpty() || Md5.md5(code.ToLower(), 16) != Session["nfine_session_verifycode"].ToString())
            //{
            //    return Content(new AjaxResult { state = ResultType.error.ToString(), message = "验证码错误，请重新输入" }.ToJson());
            //}
            passWord = EncryptHelper.AESEncrypt(passWord);
            try
            {       
                Sys_User m = db.Sys_User.FirstOrDefault(n => n.userName == userName && n.passWord == passWord && n.state =="激活");
                if (m != null)
                {                  
                    OperatorModel operatorModel = new OperatorModel
                    {
                        UserId = m.ID.ToString(),
                        UserName = m.userName,
                        RoleId = m.roleId.ToString(),                     
                        UserCode = Guid.NewGuid().ToString(),
                        YuanquID = m.YuanQuId
                        
                    };
                    OperatorProvider.Provider.AddCurrent(operatorModel);
                    LoginLog();
                    Load1(m.userName);
                    return Content(new AjaxResult { state = ResultType.success.ToString(), message = "登录成功。" }.ToJson());
                  
                }
                return Content(new AjaxResult { state = ResultType.error.ToString(), message = "用户名或密码错误" }.ToJson());
            
            }
            catch (Exception exception)
            {
                Log4NetHelper log = new Log4NetHelper();
                log.Error(exception.Message, exception);
                return Content(new AjaxResult { state = ResultType.error.ToString(), message = exception.Message }.ToJson());
            }

        }
        [HttpGet]
        public ActionResult GetAuthCode()
        {
            return File(new VerifyCode().GetVerifyCode(), @"image/Gif");
        }
        /// <summary>
        /// 处理重复登录
        /// </summary>
        /// <param name="username"></param>
        public void Load1(string username)
        {
            OperatorModel operatorModel = OperatorProvider.Provider.GetCurrent();
            var sessionId = operatorModel.UserCode;
            HttpContext httpContext = System.Web.HttpContext.Current;
            Hashtable userOnline = (Hashtable)httpContext.Application["Online"];
            if (userOnline != null)
            {
                IDictionaryEnumerator idE = userOnline.GetEnumerator();
                string strKey = string.Empty;
                while (idE.MoveNext())
                {
                    if (idE.Value != null && idE.Value.ToString().Equals(username))
                    {
                        strKey = idE.Key.ToString();
                        userOnline[strKey] = "XXXXXX";
                        break;
                    }
                }
            }
            else
            {
                userOnline = new Hashtable();
            }
            userOnline[sessionId] = username;
            httpContext.Application.Lock();
            httpContext.Application["Online"] = userOnline;
            httpContext.Application.UnLock();
        }

        /// <summary>
        /// 退出
        /// </summary>
        /// <returns></returns>
        public ActionResult LogOff()
        {
            LoginOutLog();
            FormsAuthentication.SignOut();
            OperatorProvider.Provider.RemoveCurrent();
            return RedirectToAction("LogIn");
        }
        /// <summary>
        /// 登录日志
        /// </summary>
        public void LoginLog()
        {

            try
            {
                OperatorModel loginModel = OperatorProvider.Provider.GetCurrent();
                SYS_LOGINLOG log = db.SYS_LOGINLOG.FirstOrDefault(n => n.USERID == loginModel.UserId);
                if (log != null)
                {
                    log.LOGINTIME = DateTime.Now;
                }
                else
                {
                    log = new SYS_LOGINLOG()
                    {
                        GUID = Guid.NewGuid().ToString(),
                        USERID = loginModel.UserId,
                        USERNAME = loginModel.UserName,
                        LOGINTIME = DateTime.Now

                    };
                }
                db.SYS_LOGINLOG.AddOrUpdate(log);
                db.SaveChanges();
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            {
                var msg = string.Empty;
                var errors = (from u in ex.EntityValidationErrors select u.ValidationErrors).ToList();
                foreach (var item in errors)
                    msg += item.FirstOrDefault().ErrorMessage;

               
            }
       

        }
        /// <summary>
        /// 登出日志
        /// </summary>
        [SignLoginAuthorize]
        public void LoginOutLog()
        {
            OperatorModel loginModel = OperatorProvider.Provider.GetCurrent();
            SYS_LOGINLOG log = db.SYS_LOGINLOG.FirstOrDefault(n => n.USERID == loginModel.UserId);
            if (log != null)
            {
                log.EXITTIME = DateTime.Now;
            }
            db.SYS_LOGINLOG.AddOrUpdate(log);
            db.SaveChanges();
        }
        #region 找回密码

        /// <summary>
        /// 找回密码
        /// </summary>
        /// <returns></returns>
        public ActionResult FindPassword()
        {
            return View();
        }
        [HttpPost]
        public JsonResult FindPassword(string username)
        {
            Sys_User model = db.Sys_User.FirstOrDefault(n => n.userName == username);
            if (model != null)
            {
                string emali = model.email;
                string password = model.passWord;
                password = EncryptHelper.AESDecrypt(password);
                EmailHelp hp = new EmailHelp();
                int b = hp.SendMailUse(emali, password);
                if (b == 1)
                {
                    return Json("密码已发送到关联邮箱，请注意查看");
                }
                else if (b == 0)
                {
                    return Json("请配置你的邮箱账户");
                }
                else
                {
                    return Json("发送失败,系统只支持163网易邮箱，请正确配置你的邮箱账户");
                }

            }
            return Json("发送失败");
        }

        public ActionResult NoAuthorize()
        {
            return View();
        }


        #endregion


    }
}