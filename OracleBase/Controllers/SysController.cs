using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Main.Handler;

using NFine.Code;
using OracleBase.Models;

namespace Main.Controllers
{
      [HandlerLogin]
    public class SysController : Controller
    {
        private Entities db = new Entities();
        #region 修改密码
        /// <summary>
        /// 修改密码
        /// </summary>
        /// <returns></returns>

        public ActionResult UpdatePwd()
        {
            return View();
        }
        [HttpPost]
        public JsonResult UpdatePwd(string a)
        {
            try
            {
                OperatorModel loginModel = OperatorProvider.Provider.GetCurrent();
                var oldpwd = loginModel.UserPwd;
                var inputoldpwd = Request["oldpwd"];
                inputoldpwd = EncryptHelper.AESEncrypt(inputoldpwd);

                if (oldpwd != inputoldpwd)
                {
                    return Json("旧密码错误");
                }
                else
                {
                    var pwd = EncryptHelper.AESEncrypt(Request["newpwd"]);// Base64.Md5(Request["newpwd"]);
                    var userModel =
                        db.Sys_User.FirstOrDefault(
                            n => n.userName == loginModel.UserName && n.passWord == loginModel.UserPwd);
                    if (userModel != null)
                    {
                        userModel.passWord = pwd;
                        var c = db.SaveChanges();
                        if (c > 0)
                        {

                            return Json("修改密码成功");
                        }
                        else
                        {
                            return Json("修改密码失败");
                        }

                    }
                }
                return Json("修改密码失败");
            }
            catch (Exception e)
            {
                return Json(e.Message);
               // Console.WriteLine(e);
                throw;
            }
 
        }
        #endregion

        #region 角色管理
        [Description("角色管理")]
        public ActionResult RoleManageMenu(int roleid)
        {
            ViewBag.roleid = roleid;
            Sys_Role roleModel = db.Sys_Role.Find(roleid);
            ViewBag.menusstring = roleModel.menuId;
            return View();
        }
        [Description("获取所有菜单")]
        [HttpPost]
        public JsonResult GetMenus()
        {
            var  sysMenulist = db.Sys_Menu.Select(n => new { id = n.ID, pId = n.fatherId, name = n.menuName, sort =n.sort}).OrderBy(n => n.sort).ToList();
            return Json(sysMenulist);

        }

        /// <summary>
        /// 异步修改菜单权限
        /// </summary>
        /// <param name="RoleId"></param>
        /// <param name="MenuValues"></param>
        /// <returns></returns>
        public async Task<JsonResult> UpMenuValue(int RoleId, string MenuValues)
        {
            Sys_Role roleModel =  db.Sys_Role.FirstOrDefault(n => n.ID == RoleId);
            roleModel.menuId = MenuValues;
             db.SaveChanges();
            return null;
        }
        #endregion

        public ActionResult MenuManage()
        {
            List<Sys_Menu> list = db.Sys_Menu.Where(n => n.fatherId == 0).OrderBy(n => n.sort).ToList();
            ViewBag.list2 = list;
            return View();
        }
        public JsonResult MenuManageList(int limit, int offset)
        {
            List<Sys_Menu> list = db.Sys_Menu.Where(n => n.fatherId == 0).OrderBy(n => n.sort).ToList();
            var total = list.Count;
            var rows = list.Skip(offset).Take(limit).ToList();
            return Json(new { total = total, rows = rows }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetSonMenuManageList( int fatherId)
        {
            List<Sys_Menu> list = db.Sys_Menu.Where(n => n.fatherId == fatherId).OrderBy(n => n.sort).ToList();
            var total = list.Count;
            var rows = list.ToList();
            return Json(new { total = total, rows = rows }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        //行内编辑使用
        public JsonResult EditMenu(Sys_Menu model)
        {
            db.Sys_Menu.AddOrUpdate(model);
            db.SaveChanges();
            return Json("成功");
        }
        /// <summary>
        /// 添加一级菜单
        /// </summary>
        /// <param name="menu"></param>
        /// <returns></returns>
        public ActionResult AddMenu(Sys_Menu menu)
        {
            db.Sys_Menu.Add(menu);
            db.SaveChanges();
            return RedirectToAction("MenuManage");

        }


        /// <summary>
        /// 删除菜单
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult DelMenu(int id)
        {
           
            Sys_Menu menu = db.Sys_Menu.Find(id) ?? new Sys_Menu();
            db.Sys_Menu.Remove(menu);
            var fatherid = menu.ID;
            string delson = string.Format("delete  from \"Sys_Menu\" where \"fatherId\"={0}", fatherid);
            db.Database.ExecuteSqlCommand(delson);

            int c = db.SaveChanges();
            if (c > 0)
            {
                return Json("成功");
            }
            return Json("失败");
        }

        [HttpGet]
        public ActionResult AddSonMenu(int id)
        {
            ViewBag.fatherId = id;
            return View();
        }
        /// <summary>
        /// 添加二级菜单
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult AddSonMenu(Sys_Menu model)
        {
            try
            {
                db.Sys_Menu.Add(model);
                db.SaveChanges();
                return Json("成功");
            }
            catch (Exception exception)
            {

                Log4NetHelper log = new Log4NetHelper();
                log.Error(exception.Message, exception);
                return Json(exception.InnerException.InnerException.Message);

            }


        }
 

        public ActionResult RoleManage()
        {
            List<Sys_Role> list = db.Sys_Role.ToList();
            ViewBag.sysrolelist = list;
            return View();
        }
        /// <summary>
        /// 添加角色
        /// </summary>
        /// <param name="rolename"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult AddRole(string rolename, string id)
        {
            Sys_Role role = new Sys_Role();
            role.roleName = rolename;
            role.roleId = Convert.ToInt32(id);
            db.Sys_Role.Add(role);
            db.SaveChanges();
   
            return RedirectToAction("RoleManage");
        }
        /// <summary>
        /// 删除角色
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult DelRole(int id)
        {
            Sys_Role role = db.Sys_Role.Find(id) ?? new Sys_Role();
            db.Sys_Role.Remove(role);
            db.SaveChanges();
            return RedirectToAction("RoleManage");
        }
        /// <summary>
        /// 查找角色a
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public JsonResult FindRole(int id)
        {

            return Json(db.Sys_Role.Find(id));
        }
        /// <summary>
        /// 编辑角色
        /// </summary>
        /// <param name="role"></param>
        /// <returns></returns>
        public ActionResult EditRole(Sys_Role role)
        {

            db.Set<Sys_Role>().AddOrUpdate(role);
            db.SaveChanges();
            return RedirectToAction("RoleManage");
        }
        /// <summary>
        /// 查重
        /// </summary>
        /// <returns></returns>
        public JsonResult CheckRoleRepeatName(string roleName)
        {
            List<Sys_Role> list = db.Sys_Role.ToList();
            string flag = "success";
            foreach (var item in list)
            {
                if (item.roleName == roleName)
                {
                    flag = "fail";
                    break;
                }

            }

            return Json(flag);
        }
        public JsonResult CheckRoleRepeatId(string id)
        {
            List<Sys_Role> list = db.Sys_Role.ToList();
            string flag = "success";
            foreach (var item in list)
            {
                if (item.roleId == Convert.ToInt32(id))
                {
                    flag = "fail";
                    break;
                }
            }
            return Json(flag);
        }

        /// <summary>
        /// 父级菜单列表
        /// </summary>
        /// <returns></returns>
        public JsonResult ListFatherMenu()
        {
            var list = db.Sys_Menu.Where(w => (w.fatherId == 0));
            return Json(list);
        }
        /// <summary>
        /// 子级菜单列表
        /// </summary>
        /// <returns></returns>
        public JsonResult ListSonMenu()
        {
            var list = db.Sys_Menu.Where(w => (w.fatherId != 0));
            return Json(list);
        }
        public void AddMenuId(string id, string pkID)
        {
            Sys_Role role = db.Sys_Role.Find(Convert.ToInt32(pkID));
            if (role.menuId != null)
            {
                role.menuId = role.menuId.Insert(role.menuId.Length, "," + id);
                db.Set<Sys_Role>().AddOrUpdate(role);
                db.SaveChanges();
            }
            else
            {
                role.menuId = id;
                db.Set<Sys_Role>().AddOrUpdate(role);
                db.SaveChanges();
            }

        }
        /// <summary>
        /// 删除菜单
        /// </summary>
        /// <param name="id"></param>
        /// <param name="pkID"></param>
        public void DelMenuId(string id, string pkID)
        {
            Sys_Role role = db.Sys_Role.Find(Convert.ToInt32(pkID));
            string[] menuid = role.menuId.Split(',');
            ArrayList al = new ArrayList(menuid);
            al.Remove(id);
            StringBuilder str = new StringBuilder();
            foreach (var item in al)
            {
                str.Append(item + ",");
            }
            str.Remove(str.Length - 1, 1);
            role.menuId = str.ToString();
            db.Set<Sys_Role>().AddOrUpdate(role);
            db.SaveChanges();
        }

        #region 用户管理
        /// <summary>
        /// 用户管理
        /// </summary>
        /// <returns></returns>
        public ActionResult UserManage()
        {
            List<Sys_Role> rolelist = db.Sys_Role.ToList();
            OperatorModel loginModel = OperatorProvider.Provider.GetCurrent();      
            int roleid = Convert.ToInt32(loginModel.RoleId);
            List<Sys_User> list;
            if (roleid == 1)
            {
                list = db.Sys_User.ToList();
            }
            else
            {
                list = db.Sys_User.Where(n => n.roleId != 1&&n.YuanQuId==loginModel.YuanquID).ToList();
            }

            foreach (Sys_User item in list)
            {

                Sys_Role roles = rolelist.FirstOrDefault(n => n.roleId == item.roleId) ?? new Sys_Role();
                item.roleName = roles.roleName;

            }

            ViewBag.sysuserlist = list;
            return View();

        }
        /// <summary>
        /// 添加修改用户
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>

        public ActionResult AddUser(int? id)
        {
            List<Sys_Role> rolelist = db.Sys_Role.Where(n => n.roleId != 1).ToList();
            ViewBag.rolelist = rolelist;
            List<C_Dic_YuanQu> YuanQulist = db.C_Dic_YuanQu.ToList();
            ViewBag.YuanQulist = YuanQulist;
            List<C_GOODSAGENT> GOODSAGENTlist = db.C_GOODSAGENT.ToList();
            ViewBag.GOODSAGENTlist = GOODSAGENTlist;
            Sys_User m = db.Sys_User.Find(id) ?? new Sys_User();
            return View(m);
        }


        [HttpPost]
        public JsonResult AddUser(Sys_User model)
        {
            Sys_User m = db.Sys_User.SingleOrDefault(n => n.ID == model.ID);
            if (m != null)
            {
                if (m.passWord != model.passWord)
                {

                    model.passWord = EncryptHelper.AESEncrypt(model.passWord);// Base64.Md5(model.passWord);
                }
            }
            else
            {
                model.passWord = EncryptHelper.AESEncrypt(model.passWord);
            }
            if (model.roleId == 8)
            {
                model.YuanQuId = null;
            }
            else
            {
                model.HuoDaiId = null;
            }
            model.state ="激活";
            db.Set<Sys_User>().AddOrUpdate(model);
            try
            {
                db.SaveChanges();
                return Json("成功");
            }
            catch (Exception ex)
            {
                return Json(ex.InnerException.InnerException.Message);
            }

        }

        [HttpPost]
        public JsonResult DelUser(int id)
        {
            try
            {
                Sys_User model = db.Sys_User.Find(id) ?? new Sys_User();
                db.Sys_User.Remove(model);
                int c = db.SaveChanges();
                if (c > 0)
                {
                    return Json("成功");
                }
                return Json("失败");
            }
            catch (Exception exception)
            {

                return Json(exception.InnerException.InnerException.Message);
                throw;
            }

        }

        [HttpPost]
        public JsonResult JiHuo(int id, string state)
        {
            try
            {
                Sys_User model = db.Sys_User.Find(id) ?? new Sys_User();
                model.state = state;
                int c = db.SaveChanges();
                if (c > 0)
                {
                    return Json("成功");
                }
                return Json("失败");
            }
            catch (Exception exception)
            {
                return Json(exception.InnerException.InnerException.Message);
                throw;
            }

        }
        #endregion

        #region 登录日志
        /// <summary>
        /// 登录日志
        /// </summary>
        /// <returns></returns>
        public ActionResult loginlog()
        {
            return View();
        }
        [System.Web.Http.HttpGet]
        public object GetloginlogList(int limit, int offset)
        {
            var list = db.Set<SYS_LOGINLOG>().OrderByDescending(n => n.LOGINTIME).AsQueryable();
            int total = list.Count();
            object rows = list.Skip(offset).Take(limit).AsQueryable();
            return Json(new { total = total, rows = rows }, JsonRequestBehavior.AllowGet);

        }

        #endregion

        #region 操作日志

        public ActionResult OperationLog()
        {
            return View();
        }
        public object GetOperationLogList(int limit, int offset)
        {
            var list = db.Set<SYS_OperateLogs>().OrderByDescending(n => n.OperateTime).AsQueryable();
            int total = list.Count();
            object rows = list.Skip(offset).Take(limit).AsQueryable();
            return Json(new { total = total, rows = rows }, JsonRequestBehavior.AllowGet);

        }

        #endregion

    }
}