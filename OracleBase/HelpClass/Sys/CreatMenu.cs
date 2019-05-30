//**************************************************
// Copyright (c) 
// 文件名:HelpClass.cs
// 功能描述:计划类别管理
// 创建人:宋正胜(songitnet@163.com)
// 创建时间:2017年3月22日
//
// 修改描述:三级菜单后面的二级菜单样式错误
// 修改人:宋正胜
// 修改时间:2017-10-17
// **************************************************

using System;
using System.Linq;
using System.Text;
using OracleBase.Models;

namespace Main.HelpClass
{
    public class CreatMenu
    {
        private Entities db = new Entities();
        public string CreatMenusHPlus(int roleid)
        {
            var rmos = db.Sys_Role.FirstOrDefault(n => n.roleId == roleid);
            if (string.IsNullOrEmpty(rmos.menuId))
            {
                return null;
            }
            string[] menuIds = rmos.menuId.TrimEnd(',').Split(',');
   
            var id = new decimal[menuIds.Length];
            for (var i = 0; i < menuIds.Length; i++)
                id[i] = Convert.ToInt32(menuIds[i]);

            var menus = db.Sys_Menu.Where(n =>  id.Contains(n.ID))
                .OrderBy(n => n.sort).ToList();

            var oneMenus = menus.Where(n => n.fatherId == 0).OrderBy(n => n.sort).ToList(); //1
            var sb = new StringBuilder();
            foreach (var v in oneMenus)
            {
                sb.AppendFormat(
                    "<li><a  href=\"#\"> <i class=\"fa fa-home\"></i><span class=\"nav-label\">{0}</span><span class=\"fa arrow\"></span></a>" +
                    "<ul class=\"nav nav-second-level\">",v.menuName);
                var thistwoMenus = menus.Where(n => n.fatherId == v.ID).OrderBy(n => n.sort).ToList();
                if (thistwoMenus.Count > 0)
                {
                    sb.Append("<li>");
                    foreach (var v2 in thistwoMenus)
                    {
                        var thdMenu = menus.Where(a => a.fatherId == v2.ID).OrderBy(n => n.sort).ToList();
                        if (thdMenu.Count > 0)
                        {
                            sb.AppendFormat(
                                "<li><a  href=\"#\"><span class=\"nav-label\">{0}</span><span class=\"fa arrow\"></span></a>" +
                                "<ul class=\"nav nav-second-level\">", v2.menuName);
                            sb.Append("<li>");
                            foreach (Sys_Menu menu in thdMenu)
                            {
                                var strPath = menu.url;

                                sb.Append("   <a class=\"J_menuItem\" href=" + strPath + " style='padding-left:72px'>" + menu.menuName + "</a>");
                            }
                            sb.Append("  </li></ul></li>");
                        }
                        else
                        {
                            var strPath = v2.url;

                            sb.Append("  <li> <a class=\"J_menuItem\" href=" + strPath + ">" + v2.menuName + "</a></li>");
                        }
                    }
                    sb.Append("  </li>");
                }
                sb.Append("  </ul></li>");
            }
            return sb.ToString();
        }
    }
}