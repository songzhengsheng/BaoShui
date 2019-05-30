using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using System.Web.Http.Results;
using System.Web.Script.Serialization;
using MainBLL.SysModel;
using MainBLL.TallyBLL;
using NFine.Code;
using OracleBase.HelpClass;
using OracleBase.Models;
using OracleBase.UserDefinModel;
using System.Text.RegularExpressions;
using OracleBase.HelpClass.Sys;
using OracleBase.Areas.ck.Controllers;
using MainBLL.Money;
using System.IO;
using System.Web;
using MainBLL.ToOut;
using System.Transactions;

namespace OracleBase.API
{
    public class MapiController : ApiController
    {

        private Entities db = new Entities();


        ///http://localhost:55763/api/mapi/UserList?limit=10&&offset=0


        [Route("api/Mapi/Add")]
        [System.Web.Http.HttpPost]
        public object Add([FromBody]dynamic json)
        {
            string tableName = json.tableName;
            string form = json.form;
            tableName = "OracleBase.Models." + tableName;
            object model = ReflectionHelper.CreateInstance<object>(tableName);
            var jsonForm = NFine.Code.Json.ToJson(form);
            model = NFine.Code.JsonHelper.JsonToObject(jsonForm.ToString(), model);

            CommonEFhelp h = new CommonEFhelp();

            bool b = h.add(model);
            if (b)
            {
                return Json(new AjaxResult { state = ResultType.success.ToString(), message = "成功！" }.ToJson());

            }
            return Json(new AjaxResult { state = ResultType.error.ToString(), message = "失败！" }.ToJson());
        }


        [Route("api/Mapi/Edit")]
        [System.Web.Http.HttpPost]
        public object Edit([FromBody]dynamic json)
        {
            string tableName = json.tableName;
            string form = json.form;
            tableName = "OracleBase.Models." + tableName;
            object model = ReflectionHelper.CreateInstance<object>(tableName);
            var jsonForm = NFine.Code.Json.ToJson(form);
            model = NFine.Code.JsonHelper.JsonToObject(jsonForm.ToString(), model);

            CommonEFhelp h = new CommonEFhelp();
            bool b = h.Edit(model);
            if (b)
            {
                return Json(new AjaxResult { state = ResultType.success.ToString(), message = "成功！" }.ToJson());

            }
            return Json(new AjaxResult { state = ResultType.error.ToString(), message = "失败！" }.ToJson());
        }
        //删除
        [Route("api/Mapi/DelById")]
        [System.Web.Http.HttpPost]
        public object DelById([FromBody]dynamic json)
        {
            string tableName = json.tableName;
            string ID = json.ID;
            tableName = "OracleBase.Models." + tableName;
            object model = ReflectionHelper.CreateInstance<object>(tableName);

            var jsonForm = NFine.Code.Json.ToJson(ID);
            model = NFine.Code.JsonHelper.JsonToObject(jsonForm.ToString(), model);

            CommonEFhelp h = new CommonEFhelp();
            bool b = h.del(model);
            if (b)
            {
                return Json(new AjaxResult { state = ResultType.success.ToString(), message = "删除成功。" }.ToJson());

            }
            return Json(new AjaxResult { state = ResultType.error.ToString(), message = "删除失败。" }.ToJson());
        }
        [Route("api/Mapi/DelById_Guid")]
        [System.Web.Http.HttpPost]
        public object DelById_Guid([FromBody]dynamic json)
        {
            string tableName = json.tableName;
            string Guid = json.Guid;
            tableName = "OracleBase.Models." + tableName;
            object model = ReflectionHelper.CreateInstance<object>(tableName);

            var jsonForm = NFine.Code.Json.ToJson(Guid);
            model = NFine.Code.JsonHelper.JsonToObject(jsonForm.ToString(), model);

            CommonEFhelp h = new CommonEFhelp();
            bool b = h.del(model);
            if (b)
            {
                return Json(new AjaxResult { state = ResultType.success.ToString(), message = "删除成功。" }.ToJson());

            }
            return Json(new AjaxResult { state = ResultType.error.ToString(), message = "删除失败。" }.ToJson());
        }
        public object GetTb_ZdcbList(int limit, int offset, string Code)
        {
            var wherelambda = ExtLinq.True<C_GOODSAGENT>();
            if (!string.IsNullOrEmpty(Code))
            {
                wherelambda = wherelambda.And(t => t.Code.Contains(Code));
            }
            var list = db.Set<C_GOODSAGENT>().Where(wherelambda).OrderByDescending(n => n.ID).AsQueryable();
            int total = list.Count();
            object rows = list.Skip(offset).Take(limit).ToList();
            //return Json(new BootStropJson { total = total, rows=rows}.ToJson());
            return Json(new { total = total, rows = rows });
        }


        [Route("api/Mapi/GetYuanQuList")]
        [HttpGet]
        public object GetYuanQuList(int limit, int offset, string YuanQuName)
        {
            var wherelambda = ExtLinq.True<C_Dic_YuanQu>();
            if (!string.IsNullOrEmpty(YuanQuName))
            {
                wherelambda = wherelambda.And(t => t.YuanQuName.Contains(YuanQuName));
            }
            var list = db.Set<C_Dic_YuanQu>().Where(wherelambda).OrderByDescending(n => n.ID).AsQueryable();
            int total = list.Count();
            object rows = list.Skip(offset).Take(limit).AsQueryable();
            return Json(new { total = total, rows = rows });

        }
        [Route("api/Mapi/Add_C_GOODSAGENT")]
        [System.Web.Http.HttpPost]
        public object Add_C_GOODSAGENT([FromBody]dynamic json)
        {
            var loginModel = OperatorProvider.Provider.GetCurrent();
            string form = json.form;
            var jsonForm = NFine.Code.Json.ToJson(form);
            C_GOODSAGENT model = NFine.Code.JsonHelper.JsonDeserialize<C_GOODSAGENT>(jsonForm.ToString());
            model.State = "0";
            model.creater = loginModel.UserName;
            model.creatTime = DateTime.Now;
            model.ExamineTime = DateTime.Now;
            model.ModifyTime = DateTime.Now;
            CommonEFhelp h = new CommonEFhelp();

            bool b = h.add(model);
            if (b)
            {
                return Json(new AjaxResult { state = ResultType.success.ToString(), message = "成功！" }.ToJson());

            }
            return Json(new AjaxResult { state = ResultType.error.ToString(), message = "失败！" }.ToJson());
        }



        // 转为json方法
        public HttpResponseMessage ToJson(Object obj)
        {
            String str;
            if (obj is String || obj is Char)
            {
                str = obj.ToString();
            }
            else
            {
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                str = serializer.Serialize(obj);
            }
            HttpResponseMessage result = new HttpResponseMessage { Content = new StringContent(str, Encoding.GetEncoding("UTF-8"), "application/json") };
            return result;
        }

        public Decimal? StrToDecimal(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return null;
            }
            return Convert.ToDecimal(str);

        }



        #region  //登录接口
        [Route("api/login")]
        [System.Web.Http.HttpPost]
        
        public object login([FromBody]dynamic obj)
        {
            string userName = obj.userName;
            string passWord = obj.passWord;
            passWord = EncryptHelper.AESEncrypt(passWord);
            Sys_User m = db.Sys_User.FirstOrDefault(n => n.userName == userName && n.passWord == passWord && n.state == "激活");
            
            API_login user = new API_login();
            if (m != null)
            {
                if (m.roleId == 3 || m.roleId == 4 || m.roleId == 6 || m.roleId == 7 || m.roleId == 8 || m.roleId == 30 || m.roleId == 31 || m.roleId == 32 || m.roleId == 34)
                {
                    
                }
                else
                {
                    user = new API_login()
                    {
                        code = "2",
                        msg = "禁止登陆手机端",
                    };
                    return ToJson(user);
                }
                Sys_Role r = db.Sys_Role.FirstOrDefault(n => n.roleId == m.roleId);
                C_GOODSAGENT huodaiModel = null;
                string huodaiName = "";
                if (m.HuoDaiId>0)
                {
                    huodaiModel = db.C_GOODSAGENT.FirstOrDefault(n => n.ID == m.HuoDaiId);
                    if (huodaiModel != null)
                    {
                        huodaiName = huodaiModel.Name;
                    }
                }
                user = new API_login()
                {
                    code = "0",
                    msg = "登录成功",
                    data = new Data
                    {
                        userId = m.ID.ToInt(),
                        userName = m.userName,
                        roleId = m.roleId,
                        email = m.email,
                        YuanQuId = m.YuanQuId,
                        roleName = r.roleName,
                        huodaiId = m.HuoDaiId,
                        huodaiName = huodaiName
                    }
                };

            }
            else
            {
                user = new API_login()
                {
                    code = "1",
                    msg = "用户名密码错误",
                };
            }
            return ToJson(user);
        }
        #endregion

        #region  //用户注册接口
        [Route("api/register")]
        [System.Web.Http.HttpPost]
        public object register([FromBody]dynamic obj)
        {
            string userName = obj.userName;
            string passWord = obj.passWord;
            passWord = EncryptHelper.AESEncrypt(passWord);
            Sys_User m = db.Sys_User.FirstOrDefault(n => n.userName == userName);
            Api_common save = new Api_common();
            if (m == null)
            {
                m = new Sys_User();
                m.userName = userName;
                m.passWord = passWord;
                m.roleId = obj.roleId;
                //m.YuanQuId = obj.yuanquId;
                m.email = obj.email;
                m.state = "禁用";
                m.HuoDaiId = obj.huodaiId;
                
                try
                {
                    db.Set<Sys_User>().AddOrUpdate(m);
                    db.SaveChanges();
                    save.code = "0";
                    save.msg = "注册成功";
                }
                catch (Exception e)
                {
                    Log4NetHelper log = new Log4NetHelper();
                    log.Error(e.Message, e);
                    save.code = "2";
                    save.msg = "数据格式不正确";
                    return ToJson(save);
                }
            }
            else
            {
                if (m.state != "激活")
                {
                    save = new Api_common()
                    {
                        code = "2",
                        msg = "正在等待审核",
                    };
                }
                else
                {
                    save = new Api_common()
                    {
                        code = "1",
                        msg = "用户名已存在",
                    };
                }
                
            }
            return ToJson(save);
        }
        #endregion

        #region 角色选项
        [Route("api/dic/role")]
        [System.Web.Http.HttpGet]
        public object dicRole()
        {
            List<Sys_Role> dicvisibility = db.Set<Sys_Role>().Where(n => n.roleId != 1).ToList();
            Api_DicKV_Model a = null;
            if (dicvisibility != null)
            {
                a = new Api_DicKV_Model()
                {
                    code = "0",
                    msg = "请求成功",
                    data = new Api_DicKV_ModelList { }
                };

                foreach (var item in dicvisibility)
                {
                    a.data.list.Add(new Api_DicKV_ModelListKV()
                    {
                        value = item.ID.ToInt(),
                        text = item.roleName
                    });

                }
            }
            else
            {
                a = new Api_DicKV_Model()
                {
                    code = "1",
                    msg = "用户id请求错误",
                };
            }
            return ToJson(a);
        }
        #endregion

        #region 园区选项
        [Route("api/dic/yuanqu")]
        [System.Web.Http.HttpGet]
        public object dicYuanQu()
        {
            List<C_Dic_YuanQu> dicvisibility = db.Set<C_Dic_YuanQu>().ToList();
            Api_DicKV_Model a = null;
            if (dicvisibility != null)
            {
                a = new Api_DicKV_Model()
                {
                    code = "0",
                    msg = "请求成功",
                    data = new Api_DicKV_ModelList { }
                };

                foreach (var item in dicvisibility)
                {
                    a.data.list.Add(new Api_DicKV_ModelListKV()
                    {
                        value = item.ID.ToInt(),
                        text = item.YuanQuName
                    });

                }
            }
            else
            {
                a = new Api_DicKV_Model()
                {
                    code = "1",
                    msg = "用户id请求错误",
                };
            }
            return ToJson(a);
        }
        #endregion

        #region  //票货管理列表
        [Route("api/piaohuoManage/list")]
        [System.Web.Http.HttpGet]
        public object piaohuoManageList(int limit, int page, string GBNO, int userId)
        {
            var wherelambda = ExtLinq.True<C_TB_HC_GOODSBILL>();
            Sys_User user = db.Sys_User.FirstOrDefault(n => n.ID == userId);
            if (!string.IsNullOrEmpty(GBNO))
            {
                wherelambda = wherelambda.And(t => t.C_GOODSAGENT_NAME.Contains(GBNO) || t.GBNO.Contains(GBNO));

            }else if (user.HuoDaiId > 0)
            {
                wherelambda = wherelambda.And(t => t.C_GOODSAGENT_ID == user.HuoDaiId);
            }
            else if (user.roleId == 3)
            {
                wherelambda = wherelambda.And(t => t.SysUserID == userId || t.CreatorRoleId == 8);//只能看到自己和货代
            }
            else if (user.YuanQuId > 0)
            {
                wherelambda = wherelambda.And(t => t.YuanQuID == user.YuanQuId);
            }
            List<C_TB_HC_GOODSBILL> mlist = db.Set<C_TB_HC_GOODSBILL>().Where(wherelambda).OrderByDescending(n => n.CreatTime).Skip<C_TB_HC_GOODSBILL>((page - 1) * limit).Take<C_TB_HC_GOODSBILL>(limit).AsQueryable().ToList();

            foreach (var itsms_GoodsBill in mlist)//计算库存
            {
                decimal? KuCun = 0;
                decimal? KuCunW = 0;
                List<C_TB_HC_CONSIGN> list_CONSIGN = db.C_TB_HC_CONSIGN.Where(n => n.GOODSBILL_ID == itsms_GoodsBill.ID).OrderBy(n => n.ID).ToList();
                foreach (var items_Consign in list_CONSIGN)
                {
                    List<C_TB_HS_TALLYBILL> list_TALLYBILL = db.C_TB_HS_TALLYBILL.Where(n => n.CONSIGN_ID == items_Consign.ID).OrderBy(n => n.ID).ToList();
                    foreach (var items_TALLYBILL in list_TALLYBILL)
                    {
                        if (items_TALLYBILL.CODE_OPSTYPE == "进库")
                        {
                            KuCun += items_TALLYBILL.AMOUNT;
                            KuCunW += items_TALLYBILL.WEIGHT;
                        }
                        if (items_TALLYBILL.CODE_OPSTYPE == "出库")
                        {
                            KuCun -= items_TALLYBILL.AMOUNT;
                            KuCunW -= items_TALLYBILL.WEIGHT;
                        }
                    }
                }
                itsms_GoodsBill.KunCun = KuCun.ToString();
                itsms_GoodsBill.KunCunW = KuCunW.ToDecimal(3).ToString();
                // 统计委托完成进度条
                decimal? weituoNum = 0;//每条票货下委托总数
                decimal? weituoFinishNum = 0;//每条票货下委托总数,已完成数量
                decimal? progress = 0;//进度
                weituoNum = list_CONSIGN.Count();
                weituoFinishNum = list_CONSIGN.Where(n => n.State == "已完成").Count();
                if (weituoNum > 0)
                {
                    progress = (weituoFinishNum / weituoNum).ToDecimal(2) * 100;
                }
                itsms_GoodsBill.CONTRACT_Guid = progress.ToString();//临时占用一下，不能存数据库
            }

            var a = db.C_TB_HC_GOODSBILL.Count();
            double cs = (double)a / limit;
            double b = Math.Ceiling(cs);
            API_piaohuoModel ac = null;
            ac = new API_piaohuoModel()
            {
                code = "0",
                msg = "请求成功",
                data = new Api_piaohuoModelData()
                {
                    totalCount = a,
                    list = mlist,
                }


            };
            return ToJson(ac);
        }
        #endregion

        #region 进出口选项
        [Route("api/dic/jinchukou")]
        [System.Web.Http.HttpGet]
        public object dicJinchukou()
        {
            List<C_TB_CODE_INOUT> dicvisibility = db.Set<C_TB_CODE_INOUT>().ToList();
            Api_DicKV_Model a = null;
            if (dicvisibility != null)
            {
                a = new Api_DicKV_Model()
                {
                    code = "0",
                    msg = "请求成功",
                    data = new Api_DicKV_ModelList { }
                };

                foreach (var item in dicvisibility)
                {
                    a.data.list.Add(new Api_DicKV_ModelListKV()
                    {
                        value = item.ID.ToInt(),
                        text = item.Name
                    });

                }
            }
            else
            {
                a = new Api_DicKV_Model()
                {
                    code = "1",
                    msg = "用户id请求错误",
                };
            }
            return ToJson(a);
        }
        #endregion

        #region 内外贸选项
        [Route("api/dic/neiwaimao")]
        [System.Web.Http.HttpGet]
        public object dicNeiwaimao()
        {
            List<C_TB_CODE_TRADE> dicvisibility = db.Set<C_TB_CODE_TRADE>().ToList();
            Api_DicKV_Model a = null;
            if (dicvisibility != null)
            {
                a = new Api_DicKV_Model()
                {
                    code = "0",
                    msg = "请求成功",
                    data = new Api_DicKV_ModelList { }
                };

                foreach (var item in dicvisibility)
                {
                    a.data.list.Add(new Api_DicKV_ModelListKV()
                    {
                        value = item.ID.ToInt(),
                        text = item.Name
                    });

                }
            }
            else
            {
                a = new Api_DicKV_Model()
                {
                    code = "1",
                    msg = "用户id请求错误",
                };
            }
            return ToJson(a);
        }
        #endregion

        #region 货代选项
        [Route("api/dic/huodai")]
        [System.Web.Http.HttpGet]
        public object dicHuodai()
        {
            List<C_GOODSAGENT> dicvisibility = db.Set<C_GOODSAGENT>().ToList();
            Api_DicKV_Model a = null;
            if (dicvisibility != null)
            {
                a = new Api_DicKV_Model()
                {
                    code = "0",
                    msg = "请求成功",
                    data = new Api_DicKV_ModelList { }
                };

                foreach (var item in dicvisibility)
                {
                    a.data.list.Add(new Api_DicKV_ModelListKV()
                    {
                        value = item.ID.ToInt(),
                        text = item.Name
                    });

                }
            }
            else
            {
                a = new Api_DicKV_Model()
                {
                    code = "1",
                    msg = "用户id请求错误",
                };
            }
            return ToJson(a);
        }
        #endregion

        #region 货物选项
        [Route("api/dic/huowu")]
        [System.Web.Http.HttpGet]
        public object dicHuowu()
        {
            List<C_GOODS> dicvisibility = db.Set<C_GOODS>().ToList();
            Api_DicKV_Model a = null;
            if (dicvisibility != null)
            {
                a = new Api_DicKV_Model()
                {
                    code = "0",
                    msg = "请求成功",
                    data = new Api_DicKV_ModelList { }
                };

                foreach (var item in dicvisibility)
                {
                    a.data.list.Add(new Api_DicKV_ModelListKV()
                    {
                        value = item.ID.ToInt(),
                        text = item.GoodsName
                    });

                }
            }
            else
            {
                a = new Api_DicKV_Model()
                {
                    code = "1",
                    msg = "用户id请求错误",
                };
            }
            return ToJson(a);
        }
        #endregion

        #region 包装选项
        [Route("api/dic/baozhuang")]
        [System.Web.Http.HttpGet]
        public object dicBaozhuang()
        {
            List<C_TB_CODE_PACK> dicvisibility = db.Set<C_TB_CODE_PACK>().ToList();
            Api_DicKV_Model a = null;
            if (dicvisibility != null)
            {
                a = new Api_DicKV_Model()
                {
                    code = "0",
                    msg = "请求成功",
                    data = new Api_DicKV_ModelList { }
                };

                foreach (var item in dicvisibility)
                {
                    a.data.list.Add(new Api_DicKV_ModelListKV()
                    {
                        value = item.ID.ToInt(),
                        text = item.Name
                    });

                }
            }
            else
            {
                a = new Api_DicKV_Model()
                {
                    code = "1",
                    msg = "用户id请求错误",
                };
            }
            return ToJson(a);
        }
        #endregion

        #region 航次选项
        [Route("api/dic/hangci")]
        [System.Web.Http.HttpGet]
        public object dicHangci()
        {
            List<C_TB_CODE_VOYAGE> dicvisibility = db.Set<C_TB_CODE_VOYAGE>().ToList();
            Api_DicKV_Model a = null;
            if (dicvisibility != null)
            {
                a = new Api_DicKV_Model()
                {
                    code = "0",
                    msg = "请求成功",
                    data = new Api_DicKV_ModelList { }
                };

                foreach (var item in dicvisibility)
                {
                    a.data.list.Add(new Api_DicKV_ModelListKV()
                    {
                        value = item.ID.ToInt(),
                        text = item.Name
                    });

                }
            }
            else
            {
                a = new Api_DicKV_Model()
                {
                    code = "1",
                    msg = "用户id请求错误",
                };
            }
            return ToJson(a);
        }
        #endregion


        #region  //票货添加 和 修改
        [Route("api/piaohuo/save")]
        [System.Web.Http.HttpPost]
        public object piaohuoSave([FromBody]dynamic obj)
        {
            int userId = Convert.ToInt32(obj.userId);
            string GBNO = obj.GBNO;
            string hetonghao = obj.hetonghao;
            string tidan = obj.tidan;
            string chuanming = obj.chuanming;
            string huowuleixing = obj.huowuleixing;
            string neiwaimao = obj.neiwaimao;
            string hangci = obj.hangci;
            string huodaiId = obj.huodaiId;
            string huodaiName = obj.huodaiName;
            string huowu = obj.huowu;
            string shifoujzx = obj.shifoujzx;
            string jianzhong = obj.jianzhong;
            string jihuajianshu = obj.jihuajianshu;
            string jihuazhongliang = obj.jihuazhongliang;
            string jianchijianshu = obj.jianchijianshu;
            string jianchicaiji = obj.jianchicaiji;
            string beizhu = obj.beizhu;
            string huozhu = obj.huozhu;
            string huozhi = obj.huozhi;

            C_TB_HC_GOODSBILL model;
            if (!string.IsNullOrEmpty(GBNO))
            {
                model = db.C_TB_HC_GOODSBILL.FirstOrDefault(n => n.GBNO == GBNO);
            }
            else
            {
                model = new C_TB_HC_GOODSBILL();
            }

            model.ContoractNumber = hetonghao;
            model.BLNO = tidan;
            model.ShipName = chuanming;
            model.GoodsType = huowuleixing;
            model.NWM = neiwaimao;
            model.VGNO = hangci;
            model.C_GOODSAGENT_ID = Convert.ToDecimal(huodaiId ?? "0");
            model.C_GOODSAGENT_NAME = huodaiName;
            model.C_GOODS = huowu;
            model.MARK_GOOGSBILLTYPE = shifoujzx;
            model.PIECEWEIGHT = StrToDecimal(jianzhong);
            model.PLANAMOUNT = StrToDecimal(jihuajianshu);
            model.PLANWEIGHT = StrToDecimal(jihuazhongliang);
            model.jcjs = jianchijianshu;
            model.jccj = jianchicaiji;
            model.MARK = beizhu;
            model.HuoZhu = huozhu;
            model.HuoZhi = StrToDecimal(huozhi);
            model.SysUserID = userId;
            if (string.IsNullOrEmpty(model.GBNO))           //新建时生成流水号
            {
                string TodayTime = DateTime.Today.ToString("yyyyMMdd");
                C_TB_HC_GOODSBILL Num = db.C_TB_HC_GOODSBILL.OrderByDescending(n => n.CreatTime).FirstOrDefault();
                if (Num != null)
                {
                    if (!string.IsNullOrEmpty(Num.GBNO))
                    {
                        if (Num.GBNO.Substring(2, 8) == TodayTime)
                        {
                            model.GBNO = "PH" + TodayTime + (Convert.ToInt32(Num.GBNO.Replace("PH" + TodayTime, "")) + 1).ToString("0000");
                        }
                        else
                        {
                            model.GBNO = "PH" + TodayTime + "0001";
                        }
                    }
                }

                else
                {
                    model.GBNO = "PH" + TodayTime + "0001";
                }

            }
            Sys_User user = db.Sys_User.FirstOrDefault(n => n.ID == userId);
            var yuanquid = user.YuanQuId;
            C_Dic_YuanQu yq = db.C_Dic_YuanQu.Find(yuanquid);//所属园区
            if (yq != null)
            {
                model.YuanQuID = yq.ID;
            }else if (obj.yuanquId != "0")
            {
                model.YuanQuID = obj.yuanquId;
            }
            
            //model.State = "0";
            if (model.ID == 0)
            {
                model.CreatPeople = user.userName;
                model.CreatTime = DateTime.Now;
                model.State = "待提交审核";
                if (user.roleId == 8)
                {
                    model.State = "待审核";
                }
                model.KunCun = "0";
                model.KunCunW = "0";

            }
            else
            {
                if (user.roleId == 8 && model.State == "被驳回")
                {
                    model.State = "待审核";
                }
                else if (user.roleId == 3 && model.State == "驳回")
                {
                    model.State = "待提交审核";
                }
            }

            Api_common save = new Api_common();

            if (!string.IsNullOrEmpty(model.ContoractNumber))
            {
                C_TB_HC_CONTRACT model_ht = db.C_TB_HC_CONTRACT.FirstOrDefault(n => n.ContoractNumber == model.ContoractNumber);
                if (model_ht == null)
                {
                    save.code = "0";
                    save.msg = "找不到对应的合同号";
                    return ToJson(save);
                    //return Json("找不到对应的合同号");
                }
                else
                {
                    model.CONTRACT_Guid = model_ht.Guid;
                }
            }


            try
            {
                db.Set<C_TB_HC_GOODSBILL>().AddOrUpdate(model);
                db.SaveChanges();
                save.code = "0";
                save.msg = "提交成功";
            }
            catch (Exception e)
            {
                Log4NetHelper log = new Log4NetHelper();
                log.Error(e.Message, e);
                save.code = "1";
                save.msg = "数据格式不正确";
                return ToJson(save);
            }
            return ToJson(save);
        }
        #endregion

 //       [OperationLog(OperationLogAttribute.Operatetype.删除, OperationLogAttribute.ImportantLevel.危险操作, "删除票货")]

        #region  //票货删除
        [Route("api/piaohuo/delete")]
        [System.Web.Http.HttpPost]
        public object piaohuoDelete([FromBody]dynamic obj)
        {
            int ID = obj.ID;
            Api_common response = new Api_common();
            C_TB_HC_GOODSBILL model;
            if (ID > 0)
            {
                model = db.C_TB_HC_GOODSBILL.FirstOrDefault(n => n.ID == ID);
            }
            else
            {
                response.code = "2";
                response.msg = "数据格式不正确";
                return ToJson(response);
            }

            try
            {
                db.Set<C_TB_HC_GOODSBILL>().Remove(model);
                db.SaveChanges();
                response.code = "0";
                response.msg = "提交成功";
            }
            catch (Exception e)
            {
                Log4NetHelper log = new Log4NetHelper();
                log.Error(e.Message, e);
                response.code = "1";
                response.msg = "数据格式不正确";
                return ToJson(response);
            }
            return ToJson(response);
        }
        #endregion

        #region  //票货已完成
        [Route("api/piaohuo/finish")]
        [System.Web.Http.HttpPost]
        public object piaohuoFinish([FromBody]dynamic obj)
        {
            int ID = obj.ID;
            Api_common response = new Api_common();
            if (ID > 0)
            {
            }
            else
            {
                response.code = "2";
                response.msg = "数据格式不正确";
                return ToJson(response);
            }

            try
            {
                C_TB_HC_GOODSBILL model_GoodsBill = db.C_TB_HC_GOODSBILL.Find(ID) ?? new C_TB_HC_GOODSBILL();
                model_GoodsBill.State = "已完成";
                db.Set<C_TB_HC_GOODSBILL>().AddOrUpdate(model_GoodsBill);
                db.SaveChanges();
                List<C_TB_HC_CONSIGN> list_CONSIGN = db.C_TB_HC_CONSIGN.Where(n => n.GOODSBILL_ID == model_GoodsBill.ID).OrderBy(n => n.ID).ToList();
                foreach (var items_CONSIGN in list_CONSIGN)
                {
                    C_TB_HC_CONSIGN model_CONSIGN = db.C_TB_HC_CONSIGN.Find(items_CONSIGN.ID) ?? new C_TB_HC_CONSIGN();
                    model_CONSIGN.State = "已完成";
                    db.Set<C_TB_HC_CONSIGN>().AddOrUpdate(model_CONSIGN);
                    db.SaveChanges();
                    List<C_TB_HS_TALLYBILL> list_TALLYBILL = db.C_TB_HS_TALLYBILL.Where(n => n.CONSIGN_ID == model_CONSIGN.ID).OrderBy(n => n.ID).ToList();
                    foreach (var items_TALLYBILL in list_TALLYBILL)
                    {
                        C_TB_HS_TALLYBILL model_TALLYBILL = db.C_TB_HS_TALLYBILL.Find(items_TALLYBILL.ID) ?? new C_TB_HS_TALLYBILL();
                        model_TALLYBILL.State = "已完成";
                        db.Set<C_TB_HS_TALLYBILL>().AddOrUpdate(model_TALLYBILL);
                        db.SaveChanges();
                    }
                }
                response.code = "0";
                response.msg = "提交成功";
            }
            catch (Exception e)
            {
                Log4NetHelper log = new Log4NetHelper();
                log.Error(e.Message, e);
                response.code = "1";
                response.msg = "数据格式不正确";
                return ToJson(response);
            }
            return ToJson(response);
        }
        #endregion

        #region  //票货取消审核
        [Route("api/piaohuo/cancleFinish")]
        [System.Web.Http.HttpPost]
        public object piaohuoCancleFinish([FromBody]dynamic obj)
        {
            int id = obj.id;
            Api_common response = new Api_common();
            try
            {
                C_TB_HC_GOODSBILL model_GoodsBill = db.C_TB_HC_GOODSBILL.Find(id) ?? new C_TB_HC_GOODSBILL();
                List<C_TB_HC_CONSIGN> list_CONSIGN = db.C_TB_HC_CONSIGN.Where(n => n.GOODSBILL_ID == id).OrderBy(n => n.ID).ToList();
                if (list_CONSIGN.Count != 0)
                {
                    model_GoodsBill.State = "已生成" + list_CONSIGN.Count + "条委托";
                }
                else
                {
                    model_GoodsBill.State = "进行中";
                }
                db.Set<C_TB_HC_GOODSBILL>().AddOrUpdate(model_GoodsBill);
                db.SaveChanges();

                response.code = "0";
                response.msg = "提交成功";
            }
            catch (Exception e)
            {
                Log4NetHelper log = new Log4NetHelper();
                log.Error(e.Message, e);
                response.code = "2";
                response.msg = "数据格式不正确";
            }
            return ToJson(response);
        }
        #endregion

        #region  //委托管理列表
        [Route("api/weituoManage/list")]
        [System.Web.Http.HttpGet]
        public object weituoManageList(int userId, int limit, int page, string CGNO, int? goodBillId)
        {
            var wherelambda = ExtLinq.True<C_TB_HC_CONSIGN>();
            Sys_User user = db.Sys_User.FirstOrDefault(n => n.ID == userId);
            if (goodBillId != null)
            {
                wherelambda = wherelambda.And(t => t.GOODSBILL_ID == goodBillId);
            }
            if (!string.IsNullOrEmpty(CGNO))
            {
                wherelambda = wherelambda.And(t => t.CGNO.Contains(CGNO));
            }
            //else if (user.HuoDaiId > 0) 
            //{
            //    wherelambda = wherelambda.And(t => t. == goodBillId);
            //}
            List<C_TB_HC_CONSIGN> mlist = db.Set<C_TB_HC_CONSIGN>().Where(wherelambda).OrderByDescending(n => n.ID).Skip<C_TB_HC_CONSIGN>((page - 1) * limit).Take<C_TB_HC_CONSIGN>(limit).AsQueryable().ToList();


            foreach (var items_Consign in mlist)
            {
                decimal? FactNum = 0;
                decimal? FactNumW = 0;
                List<C_TB_HS_TALLYBILL> list_TALLYBILL = db.C_TB_HS_TALLYBILL.Where(n => n.CONSIGN_ID == items_Consign.ID).OrderBy(n => n.ID).ToList();
                foreach (var items_TALLYBILL in list_TALLYBILL)
                {
                    if (items_TALLYBILL.CODE_OPSTYPE == "进库")
                    {
                        FactNum += items_TALLYBILL.AMOUNT;
                        FactNumW += items_TALLYBILL.WEIGHT;
                    }
                    if (items_TALLYBILL.CODE_OPSTYPE == "出库")
                    {
                        FactNum += items_TALLYBILL.AMOUNT;
                        FactNumW += items_TALLYBILL.WEIGHT;
                    }
                }
                items_Consign.FactNum = FactNum;
                items_Consign.FactNumW = FactNumW;
            }





            var a = db.C_TB_HC_GOODSBILL.Count();
            double cs = (double)a / limit;
            double b = Math.Ceiling(cs);
            Api_weituoModel ac = null;
            ac = new Api_weituoModel()
            {
                code = "0",
                msg = "请求成功",
                data = new Api_weituoModelData()
                {
                    totalCount = a,
                    list = mlist,
                }


            };
            return ToJson(ac);
        }
        #endregion

        #region  //委托单删除
        [Route("api/weituo/delete")]
        [System.Web.Http.HttpPost]
        public object weituoDelete([FromBody]dynamic obj)
        {
            EFHelpler<C_TB_HC_CONSIGN> ef = new EFHelpler<C_TB_HC_CONSIGN>();
            int ID = obj.ID;
            Api_common response = new Api_common();
            C_TB_HC_CONSIGN model;
            if (ID > 0)
            {
                model = db.C_TB_HC_CONSIGN.FirstOrDefault(n => n.ID == ID);
            }
            else
            {
                response.code = "2";
                response.msg = "数据格式不正确";
                return ToJson(response);
            }

            try
            {
                C_TB_HC_CONSIGN model_CONSIGN = db.C_TB_HC_CONSIGN.Find(ID) ?? new C_TB_HC_CONSIGN();
                C_TB_HC_CONSIGN u = new C_TB_HC_CONSIGN() { ID = ID };
                ef.delete(u);

                C_TB_HC_GOODSBILL model_GoodsBill = db.C_TB_HC_GOODSBILL.Find(model_CONSIGN.GOODSBILL_ID) ?? new C_TB_HC_GOODSBILL();
                List<C_TB_HC_CONSIGN> list_CONSIGN = db.C_TB_HC_CONSIGN.Where(n => n.GOODSBILL_ID == model_CONSIGN.GOODSBILL_ID).OrderBy(n => n.ID).ToList();
                if (list_CONSIGN.Count != 0)
                {
                    model_GoodsBill.State = "已生成" + list_CONSIGN.Count + "条委托";
                }
                else
                {
                    model_GoodsBill.State = "进行中";
                }
                db.Set<C_TB_HC_GOODSBILL>().AddOrUpdate(model_GoodsBill);
                db.SaveChanges();
                response.code = "0";
                response.msg = "提交成功";
            }
            catch (Exception e)
            {
                Log4NetHelper log = new Log4NetHelper();
                log.Error(e.Message, e);
                response.code = "1";
                response.msg = "数据格式不正确";
                return ToJson(response);
            }
            return ToJson(response);
        }
        #endregion

        #region 委托人和收货人选项
        [Route("api/dic/weituoren")]
        [System.Web.Http.HttpGet]
        public object dicWeituoren()
        {
            List<C_TB_CODE_CUSTOMER> dicvisibility = db.Set<C_TB_CODE_CUSTOMER>().ToList();
            Api_DicKV_Model a = null;
            if (dicvisibility != null)
            {
                a = new Api_DicKV_Model()
                {
                    code = "0",
                    msg = "请求成功",
                    data = new Api_DicKV_ModelList { }
                };

                foreach (var item in dicvisibility)
                {
                    a.data.list.Add(new Api_DicKV_ModelListKV()
                    {
                        value = item.ID.ToInt(),
                        text = item.Name
                    });

                }
            }
            else
            {
                a = new Api_DicKV_Model()
                {
                    code = "1",
                    msg = "用户id请求错误",
                };
            }
            return ToJson(a);
        }
        #endregion

        #region 承运人选项
        [Route("api/dic/chengyunren")]
        [System.Web.Http.HttpGet]
        public object dicChengyunren()
        {
            List<C_GOODSAGENT> dicvisibility = db.Set<C_GOODSAGENT>().ToList();
            Api_DicKV_Model a = null;
            if (dicvisibility != null)
            {
                a = new Api_DicKV_Model()
                {
                    code = "0",
                    msg = "请求成功",
                    data = new Api_DicKV_ModelList { }
                };

                foreach (var item in dicvisibility)
                {
                    a.data.list.Add(new Api_DicKV_ModelListKV()
                    {
                        value = item.ID.ToInt(),
                        text = item.Name
                    });

                }
            }
            else
            {
                a = new Api_DicKV_Model()
                {
                    code = "1",
                    msg = "用户id请求错误",
                };
            }
            return ToJson(a);
        }
        #endregion

        #region  //委托单添加 和 修改
        [Route("api/weituo/save")]
        [System.Web.Http.HttpPost]
        public object weituoSave([FromBody]dynamic obj)
        {
            int userId = Convert.ToInt32(obj.userId);
            string ID = obj.ID;
            C_TB_HC_CONSIGN model;
            if (!string.IsNullOrEmpty(ID))
            {
                int id = obj.ID;
                model = db.C_TB_HC_CONSIGN.FirstOrDefault(n => n.ID == id);
            }
            else
            {
                model = new C_TB_HC_CONSIGN();
                string TodayTime = DateTime.Today.ToString("yyyyMMdd");
                C_TB_HC_CONSIGN Num = db.C_TB_HC_CONSIGN.OrderByDescending(n => n.CREATETIME).ToList().FirstOrDefault();
                if (Num != null)
                {
                    if (!string.IsNullOrEmpty(Num.CGNO))
                    {
                        if (Num.CGNO.Substring(2, 8) == TodayTime)
                        {
                            model.CGNO = "WT" + TodayTime + (Convert.ToInt32(Num.CGNO.Replace("WT" + TodayTime, "")) + 1).ToString("0000");
                        }
                        else
                        {
                            model.CGNO = "WT" + TodayTime + "0001";
                        }
                    }
                }

                else
                {
                    model.CGNO = "WT" + TodayTime + "0001";
                }
            }
            int stateid = 0;
            Sys_User user = db.Sys_User.FirstOrDefault(n => n.ID == userId);
            if (model.ID == 0)
            {
                model.CREATORNAME = user.userName;
                model.CREATETIME = DateTime.Now;
                stateid = 1;
                model.State = "待提交审核";
                if (user.roleId == 8)
                {
                    model.State = "待审核";
                    model.LaiYuan = "货代";
                }
                else if (user.roleId == 3)
                {
                    model.LaiYuan = "业务员";
                 }

            }
            else
            {
                if (user.roleId == 8 && model.State == "被驳回")
                {
                    model.State = "待审核";
                }
            }
            model.GOODSBILL_ID = obj.GOODSBILL_ID;
            model.WeiTuoTime = obj.weituoriqi;
            model.PAPERYNO = obj.rweituohao;
            model.CODE_OPERATION = obj.zuoyeguocheng;
            model.HengZhong = obj.hengzhong;
            model.PLANAMOUNT = StrToDecimal(obj.jihuajianshu.ToString());
            model.PLANWEIGHT = StrToDecimal(obj.jihuazhongliang.ToString());
            model.CONTAINERTYPE = obj.xiangxing;
            model.CONTAINERNUM = StrToDecimal(obj.xiangshuliang.ToString());
            model.jbr = obj.jingbanren;
            model.Phone = obj.phone;
            model.BeiZhu = obj.beizhu;
            if (model.GOODSBILL_ID > 0)
            {
                C_TB_HC_GOODSBILL piaohuoModel = db.C_TB_HC_GOODSBILL.Find(model.GOODSBILL_ID);
                model.WeiTuoRen = piaohuoModel.C_GOODSAGENT_NAME;
                model.ShipName = piaohuoModel.ShipName;
                model.VGNO = piaohuoModel.VGNO;
                model.GoodsName = piaohuoModel.C_GOODS;
                model.BLNO = piaohuoModel.BLNO;
                model.GoodsType = piaohuoModel.GoodsType;
                model.GoodsBill_Num = piaohuoModel.GBNO;
                model.YuanQuID = piaohuoModel.YuanQuID;
            }

            Api_common save = new Api_common();
            try
            {
                db.Set<C_TB_HC_CONSIGN>().AddOrUpdate(model);
                db.SaveChanges();

                //C_TB_HC_CONSIGN model_HZ = db.C_TB_HC_CONSIGN.Find(model.ID) ?? new C_TB_HC_CONSIGN();

                //if (stateid == 1 && model.HengZhong == "是")
                //{
                //    model_HZ.HengZhong = "否";
                //    db.Set<C_TB_HC_CONSIGN>().Add(model_HZ);
                //    db.SaveChanges();
                //}
                C_TB_HC_GOODSBILL model_GoodsBill = db.C_TB_HC_GOODSBILL.Find(model.GOODSBILL_ID) ?? new C_TB_HC_GOODSBILL();
                List<C_TB_HC_CONSIGN> list_CONSIGN = db.C_TB_HC_CONSIGN.Where(n => n.GOODSBILL_ID == model.GOODSBILL_ID).OrderBy(n => n.ID).ToList();
                if (list_CONSIGN.Count != 0)
                {
                    model_GoodsBill.State = "已生成" + list_CONSIGN.Count + "条委托";
                }
                else
                {
                    model_GoodsBill.State = "进行中";
                }
                db.Set<C_TB_HC_GOODSBILL>().AddOrUpdate(model_GoodsBill);
                db.SaveChanges();

                save.code = "0";
                save.msg = "提交成功";
            }
            catch (Exception e)
            {
                Log4NetHelper log = new Log4NetHelper();
                log.Error(e.Message, e);
                save.code = "1";
                save.msg = "数据格式不正确";
                return ToJson(save);
            }
            return ToJson(save);
        }
        #endregion

        #region  //委托已完成
        [Route("api/weituo/finish")]
        [System.Web.Http.HttpPost]
        public object weituoFinish([FromBody]dynamic obj)
        {
            int id = obj.ID;
            Api_common response = new Api_common();
            if (id > 0)
            {
            }
            else
            {
                response.code = "2";
                response.msg = "数据格式不正确";
                return ToJson(response);
            }

            try
            {
                using (var scope = new TransactionScope(TransactionScopeOption.Required))
                {
                    C_TB_HC_CONSIGN model_CONSIGN = db.C_TB_HC_CONSIGN.Find(id) ?? new C_TB_HC_CONSIGN();
                    model_CONSIGN.State = "已完成";
                    db.Set<C_TB_HC_CONSIGN>().AddOrUpdate(model_CONSIGN);
                    db.SaveChanges();
                    List<C_TB_HS_TALLYBILL> list_TALLYBILL = db.C_TB_HS_TALLYBILL
                        .Where(n => n.CONSIGN_ID == model_CONSIGN.ID).OrderBy(n => n.ID).ToList();
                    foreach (var items_TALLYBILL in list_TALLYBILL)
                    {
                        C_TB_HS_TALLYBILL model_TALLYBILL =
                            db.C_TB_HS_TALLYBILL.Find(items_TALLYBILL.ID) ?? new C_TB_HS_TALLYBILL();
                        model_TALLYBILL.State = "已完成";
                        db.Set<C_TB_HS_TALLYBILL>().AddOrUpdate(model_TALLYBILL);
                        db.SaveChanges();
                    }

                    if (!string.IsNullOrEmpty(model_CONSIGN.Ckdh))
                    {
                        BigController big = new BigController();
                        bool a = big.PostEditCkSt(id);
                        if (a == true)
                        {
                            scope.Complete();
                            response.code = "0";
                            response.msg = "提交成功";
                            return ToJson(response);
                        }
                        else
                        {
                            response.code = "2";
                            response.msg = "提交失败，请联系管理员";
                            return ToJson(response);
                        }
                    }
                    else
                    {
                        scope.Complete();
                    }
                    response.code = "0";
                    response.msg = "提交成功";
                    return ToJson(response);
                    //return Json(new AjaxResult { state = ResultType.success.ToString(), message = "成功" }.ToJson());
                }













                //C_TB_HC_CONSIGN model_CONSIGN = db.C_TB_HC_CONSIGN.Find(ID) ?? new C_TB_HC_CONSIGN();
                //model_CONSIGN.State = "已完成";
                //db.Set<C_TB_HC_CONSIGN>().AddOrUpdate(model_CONSIGN);
                //db.SaveChanges();
                //List<C_TB_HS_TALLYBILL> list_TALLYBILL = db.C_TB_HS_TALLYBILL.Where(n => n.CONSIGN_ID == model_CONSIGN.ID).OrderBy(n => n.ID).ToList();
                //foreach (var items_TALLYBILL in list_TALLYBILL)
                //{
                //    C_TB_HS_TALLYBILL model_TALLYBILL = db.C_TB_HS_TALLYBILL.Find(items_TALLYBILL.ID) ?? new C_TB_HS_TALLYBILL();
                //    model_TALLYBILL.State = "已完成";
                //    db.Set<C_TB_HS_TALLYBILL>().AddOrUpdate(model_TALLYBILL);
                //    db.SaveChanges();
                //}
                //response.code = "0";
                //response.msg = "提交成功";
            }
            catch (Exception e)
            {
                Log4NetHelper log = new Log4NetHelper();
                log.Error(e.Message, e);
                response.code = "1";
                response.msg = "数据格式不正确";
                return ToJson(response);
            }
            //return ToJson(response);
        }
        #endregion

        #region  //委托取消审核
        [Route("api/weituo/cancleFinish")]
        [System.Web.Http.HttpPost]
        public object weituoCancleFinish([FromBody]dynamic obj)
        {
            int id = obj.id;
            Api_common response = new Api_common();
            try
            {
                C_TB_HC_CONSIGN model_CONSIGN = db.C_TB_HC_CONSIGN.Find(id) ?? new C_TB_HC_CONSIGN();
                List<C_TB_HS_TALLYBILL> list_TALLYBILL = db.C_TB_HS_TALLYBILL.Where(n => n.CONSIGN_ID == id).OrderBy(n => n.ID).ToList();
                if (list_TALLYBILL.Count != 0)
                {
                    model_CONSIGN.State = "已生成" + list_TALLYBILL.Count + "条理货";
                }
                else
                {
                    model_CONSIGN.State = "进行中";
                }
                db.Set<C_TB_HC_CONSIGN>().AddOrUpdate(model_CONSIGN);
                db.SaveChanges();

                response.code = "0";
                response.msg = "提交成功";
            }
            catch (Exception e)
            {
                Log4NetHelper log = new Log4NetHelper();
                log.Error(e.Message, e);
                response.code = "2";
                response.msg = "数据格式不正确";
            }

            return ToJson(response);
        }
        #endregion


        #region  //理货管理列表
        [Route("api/lihuoManage/list")]
        [System.Web.Http.HttpGet]
        public object lihuoManageList(int limit, int page, string TBNO, int? consignId)
        {
            var wherelambda = ExtLinq.True<C_TB_HS_TALLYBILL>();
            wherelambda = wherelambda.And(t => t.Type != "清场");
            if (consignId != null)
            {
                wherelambda = wherelambda.And(t => t.CONSIGN_ID == consignId);
            }
            if (!string.IsNullOrEmpty(TBNO))
            {
                wherelambda = wherelambda.And(t => t.TBNO.Contains(TBNO));
            }
            List<C_TB_HS_TALLYBILL> mlist = db.Set<C_TB_HS_TALLYBILL>().Where(wherelambda).OrderByDescending(n => n.ID).Skip<C_TB_HS_TALLYBILL>((page - 1) * limit).Take<C_TB_HS_TALLYBILL>(limit).AsQueryable().ToList();
            var a = db.C_TB_HC_GOODSBILL.Count();
            double cs = (double)a / limit;
            double b = Math.Ceiling(cs);
            Api_lihuoModel ac = null;
            ac = new Api_lihuoModel()
            {
                code = "0",
                msg = "请求成功",
                data = new Api_lihuoModelData()
                {
                    totalCount = a,
                    list = mlist,
                }


            };
            return ToJson(ac);
        }
        #endregion

        #region  //理货单删除
        [Route("api/lihuo/delete")]
        [System.Web.Http.HttpPost]
        public object lihuoDelete([FromBody]dynamic obj)
        {
            EFHelpler<C_TB_HS_TALLYBILL> ef = new EFHelpler<C_TB_HS_TALLYBILL>();
            EFHelpler<C_TB_HS_STOCKDORMANT> ef1 = new EFHelpler<C_TB_HS_STOCKDORMANT>();
            int ID = obj.ID;
            Api_common response = new Api_common();
            C_TB_HS_TALLYBILL model;
            if (ID > 0)
            {
                model = db.C_TB_HS_TALLYBILL.FirstOrDefault(n => n.ID == ID);
            }
            else
            {
                response.code = "2";
                response.msg = "数据格式不正确";
                return ToJson(response);
            }

            try
            {
                C_TB_HS_TALLYBILL model_TALLYBILL = db.C_TB_HS_TALLYBILL.Find(ID) ?? new C_TB_HS_TALLYBILL();
                C_TB_HS_STOCKDORMANT stocModel = db.C_TB_HS_STOCKDORMANT.FirstOrDefault(n => n.GBNO == model_TALLYBILL.CGNO);//委托号

                ef.delete(model_TALLYBILL);
                ef1.delete(stocModel);
                C_TB_HC_CONSIGN model_CONSIGN = db.C_TB_HC_CONSIGN.Find(model_TALLYBILL.CONSIGN_ID) ?? new C_TB_HC_CONSIGN();
                List<C_TB_HS_TALLYBILL> list_ALLYBILL = db.C_TB_HS_TALLYBILL.Where(n => n.CONSIGN_ID == model_TALLYBILL.CONSIGN_ID).OrderBy(n => n.ID).ToList();
                if (list_ALLYBILL.Count != 0)
                {
                    model_CONSIGN.State = "已生成" + list_ALLYBILL.Count + "条理货";
                }
                else
                {
                    model_CONSIGN.State = "进行中";
                }
                db.Set<C_TB_HC_CONSIGN>().AddOrUpdate(model_CONSIGN);
                db.SaveChanges();
                response.code = "0";
                response.msg = "提交成功";
            }
            catch (Exception e)
            {
                Log4NetHelper log = new Log4NetHelper();
                log.Error(e.Message, e);
                response.code = "1";
                response.msg = "数据格式不正确";
                return ToJson(response);
            }
            return ToJson(response);
        }
        #endregion

        #region  //理货取消审核
        [Route("api/lihuo/cancleFinish")]
        [System.Web.Http.HttpPost]
        public object lihuoCancleFinish([FromBody]dynamic obj)
        {
            int id = obj.id;
            Api_common response = new Api_common();
            try
            {
                C_TB_HS_TALLYBILL model_TALLYBILL = db.C_TB_HS_TALLYBILL.Find(id) ?? new C_TB_HS_TALLYBILL();
                model_TALLYBILL.State = "进行中";
                db.Set<C_TB_HS_TALLYBILL>().AddOrUpdate(model_TALLYBILL);
                db.SaveChanges();

                response.code = "0";
                response.msg = "提交成功";
            }
            catch (Exception e)
            {
                Log4NetHelper log = new Log4NetHelper();
                log.Error(e.Message, e);
                response.code = "2";
                response.msg = "数据格式不正确";
            }


            return ToJson(response);
        }
        #endregion

        #region 作业选项
        [Route("api/dic/zuoye")]
        [System.Web.Http.HttpGet]
        public object dicZuoye()
        {
            List<C_TB_CODE_CAOZUO> dicvisibility = db.Set<C_TB_CODE_CAOZUO>().ToList();
            Api_DicKV_Model a = null;
            if (dicvisibility != null)
            {
                a = new Api_DicKV_Model()
                {
                    code = "0",
                    msg = "请求成功",
                    data = new Api_DicKV_ModelList { }
                };

                foreach (var item in dicvisibility)
                {
                    a.data.list.Add(new Api_DicKV_ModelListKV()
                    {
                        value = item.ID.ToInt(),
                        text = item.NAME
                    });

                }
            }
            else
            {
                a = new Api_DicKV_Model()
                {
                    code = "1",
                    msg = "用户id请求错误",
                };
            }
            return ToJson(a);
        }
        #endregion

        #region 委托号选项
        [Route("api/dic/weituohao")]
        [System.Web.Http.HttpGet]
        public object dicWeituohao()
        {
            List<C_TB_HC_CONSIGN> dicvisibility = db.Set<C_TB_HC_CONSIGN>().ToList();
            Api_DicKV_Model a = null;
            if (dicvisibility != null)
            {
                a = new Api_DicKV_Model()
                {
                    code = "0",
                    msg = "请求成功",
                    data = new Api_DicKV_ModelList { }
                };

                foreach (var item in dicvisibility)
                {
                    a.data.list.Add(new Api_DicKV_ModelListKV()
                    {
                        value = item.ID.ToInt(),
                        text = item.CGNO
                    });

                }
            }
            else
            {
                a = new Api_DicKV_Model()
                {
                    code = "1",
                    msg = "用户id请求错误",
                };
            }
            return ToJson(a);
        }
        #endregion

        #region 场选项
        [Route("api/dic/chang")]
        [System.Web.Http.HttpGet]
        public object dicChang()
        {
            List<C_TB_CODE_STORAGE> dicvisibility = db.Set<C_TB_CODE_STORAGE>().ToList();
            Api_DicKV_Model a = null;
            if (dicvisibility != null)
            {
                a = new Api_DicKV_Model()
                {
                    code = "0",
                    msg = "请求成功",
                    data = new Api_DicKV_ModelList { }
                };

                foreach (var item in dicvisibility)
                {
                    List<C_TB_CODE_BOOTH> chang = db.C_TB_CODE_BOOTH.Where(n => n.Storage_ID == item.ID).ToList();
                    if (chang.Count > 0)
                    {
                        a.data.list.Add(new Api_DicKV_ModelListKV()
                        {
                            value = item.ID.ToInt(),
                            text = item.STORAGEName
                        });
                    }
                }
            }
            else
            {
                a = new Api_DicKV_Model()
                {
                    code = "1",
                    msg = "用户id请求错误",
                };
            }
            return ToJson(a);
        }
        #endregion

        #region 场选项 带园区id
        [Route("api/dic/yuanquChang")]
        [System.Web.Http.HttpGet]
        public object dicYuanquChang(int yuanquId)
        {
            List<C_TB_CODE_SECTION> sections = db.C_TB_CODE_SECTION.Where(n => n.CODE_COMPANY == yuanquId).ToList();
            decimal[] id = new decimal[sections.Count];
            for (int j = 0; j < sections.Count; j++)
            {
                id[j] = (int)sections[j].ID;
            }

            //List<C_TB_CODE_STORAGE> storages = db.C_TB_CODE_STORAGE.Where(n => id.Contains(n.SECTION_ID)).ToList();
            List<C_TB_CODE_STORAGE> dicvisibility = db.C_TB_CODE_STORAGE.Where(n => id.Contains(n.SECTION_ID)).ToList();
            Api_DicKV_Model a = null;
            if (dicvisibility != null)
            {
                a = new Api_DicKV_Model()
                {
                    code = "0",
                    msg = "请求成功",
                    data = new Api_DicKV_ModelList { }
                };

                foreach (var item in dicvisibility)
                {
                    List<C_TB_CODE_BOOTH> chang = db.C_TB_CODE_BOOTH.Where(n => n.Storage_ID == item.ID).ToList();
                    if (chang.Count > 0)
                    {
                        a.data.list.Add(new Api_DicKV_ModelListKV()
                        {
                            value = item.ID.ToInt(),
                            text = item.STORAGEName
                        });
                    }
                }
            }
            else
            {
                a = new Api_DicKV_Model()
                {
                    code = "1",
                    msg = "用户id请求错误",
                };
            }
            return ToJson(a);
        }
        #endregion

        #region 位选项
        [Route("api/dic/wei")]
        [System.Web.Http.HttpGet]
        public object dicWei(int storageId)
        {
            List<C_TB_CODE_BOOTH> dicvisibility = db.C_TB_CODE_BOOTH.Where(n => n.Storage_ID == storageId).ToList();
            Api_DicKV_Model a = null;
            if (dicvisibility != null)
            {
                a = new Api_DicKV_Model()
                {
                    code = "0",
                    msg = "请求成功",
                    data = new Api_DicKV_ModelList { }
                };

                foreach (var item in dicvisibility)
                {
                    a.data.list.Add(new Api_DicKV_ModelListKV()
                    {
                        value = item.ID.ToInt(),
                        text = item.BOOTH
                    });

                }
            }
            else
            {
                a = new Api_DicKV_Model()
                {
                    code = "1",
                    msg = "用户id请求错误",
                };
            }
            return ToJson(a);
        }
        #endregion

        #region 位id转name
        [Route("api/wei/name")]
        [System.Web.Http.HttpGet]
        public object weiName(int weiId)
        {
            C_TB_CODE_BOOTH dicvisibility = db.C_TB_CODE_BOOTH.FirstOrDefault(n => n.ID == weiId);
            Api_DicKV_Model a = null;
            if (dicvisibility != null)
            {
                a = new Api_DicKV_Model()
                {
                    code = "0",
                    msg = "请求成功",
                    data = new Api_DicKV_ModelList { }
                };

                a.data.list.Add(new Api_DicKV_ModelListKV()
                {
                    value = dicvisibility.ID.ToInt(),
                    text = dicvisibility.BOOTH
                });
            }
            else
            {
                a = new Api_DicKV_Model()
                {
                    code = "1",
                    msg = "用户id请求错误",
                };
            }
            return ToJson(a);
        }
        #endregion

        #region 场id转name
        [Route("api/chang/name")]
        [System.Web.Http.HttpGet]
        public object changName(int changId)
        {
            C_TB_CODE_STORAGE dicvisibility = db.C_TB_CODE_STORAGE.FirstOrDefault(n => n.ID == changId);
            Api_DicKV_Model a = null;
            if (dicvisibility != null)
            {
                a = new Api_DicKV_Model()
                {
                    code = "0",
                    msg = "请求成功",
                    data = new Api_DicKV_ModelList { }
                };

                a.data.list.Add(new Api_DicKV_ModelListKV()
                {
                    value = dicvisibility.ID.ToInt(),
                    text = dicvisibility.STORAGEName
                });
            }
            else
            {
                a = new Api_DicKV_Model()
                {
                    code = "1",
                    msg = "用户id请求错误",
                };
            }
            return ToJson(a);
        }
        #endregion

        #region  //理货单添加 和 修改
        [Route("api/lihuo/save")]
        [System.Web.Http.HttpPost]
        public object lihuoSave([FromBody]dynamic obj)
        {
            int userId = Convert.ToInt32(obj.userId);
            Sys_User user = db.Sys_User.FirstOrDefault(n => n.ID == userId);
            string ID = obj.ID;
            C_TB_HS_TALLYBILL model;
            if (!string.IsNullOrEmpty(ID))
            {
                int id = obj.ID;
                model = db.C_TB_HS_TALLYBILL.FirstOrDefault(n => n.ID == id);
            }
            else
            {
                model = new C_TB_HS_TALLYBILL();
                model.State = "进行中";
                model.Type = "进出库";
            }
            int CONSIGN_ID = obj.CONSIGN_ID;
            C_TB_HC_CONSIGN con = db.C_TB_HC_CONSIGN.FirstOrDefault(n => n.ID == CONSIGN_ID);
            if (model.TBNO == null)
            {


                string TodayTime = DateTime.Today.ToString("yyyyMMdd");
                C_TB_HS_TALLYBILL Num = db.C_TB_HS_TALLYBILL.Where(n => n.Type != "清场").OrderByDescending(n => n.TBNO).FirstOrDefault();
                if (Num != null)
                {
                    if (!string.IsNullOrEmpty(Num.TBNO))
                    {
                        if (Num.TBNO.Substring(2, 8) == TodayTime)
                        {
                            model.TBNO = "LH" + TodayTime +
                                         (Convert.ToInt32(Num.TBNO.Replace("LH" + TodayTime, "")) + 1).ToString("0000");
                        }
                        else
                        {
                            model.TBNO = "LH" + TodayTime + "0001";
                        }
                    }
                }

                else
                {
                    model.TBNO = "LH" + TodayTime + "0001";
                }
            }
            model.CONSIGN_ID = con.ID;
            model.CGNO = con.CGNO;
            model.GONGBAN = obj.zuoyegongban;
            model.TRAINNUM = StrToDecimal(obj.cheshu.ToString());
            model.SIGNDATE = obj.lihuoshijian;
            model.CODE_OPSTYPE = obj.jinchuku;
            model.STORAG = obj.changId;
            model.CODE_SECTION = obj.CODE_SECTION;
            model.BanCi = obj.banci;
            model.PIECEWEIGHT = StrToDecimal(obj.shijijianzhong.ToString());
            model.AMOUNT = StrToDecimal(obj.shijijianshu.ToString());
            model.WEIGHT = StrToDecimal(obj.shijizhongliang.ToString());
            model.XIANGSHU = StrToDecimal(obj.shijixiangshu.ToString());
            model.CODE_QUALITY = obj.guobang;
            model.CODE_WORKTEAM = obj.shangai;
            model.DuanMu = StrToDecimal(obj.duanmu.ToString());
            model.REMARK = obj.beizhu;
            if (model.CONSIGN_ID > 0)
            {
                C_TB_HC_CONSIGN weituoModel = db.C_TB_HC_CONSIGN.Find(model.CONSIGN_ID);
                model.TALLYMAN = user.userName;
                model.CAOZUO = weituoModel.CODE_OPERATION;
                model.HangCi = weituoModel.VGNO;
                model.GoodsName = weituoModel.GoodsName;
                model.YuanQuID = weituoModel.YuanQuID;
                model.HuoDai = weituoModel.WeiTuoRen;
                model.ChuanMing = weituoModel.ShipName;
            }

            Api_common save = new Api_common();
            try
            {

                db.Set<C_TB_HS_TALLYBILL>().AddOrUpdate(model);
                db.SaveChanges();
                bool b = EidSTOCKDORMANT(model, model.ID);
                C_TB_HC_CONSIGN model_CONSIGN = db.C_TB_HC_CONSIGN.Find(model.CONSIGN_ID) ?? new C_TB_HC_CONSIGN();
                List<C_TB_HS_TALLYBILL> list_TALLYBILL = db.C_TB_HS_TALLYBILL.Where(n => n.CONSIGN_ID == model.CONSIGN_ID).OrderBy(n => n.ID).ToList();
                if (list_TALLYBILL.Count != 0)
                {
                    model_CONSIGN.State = "已生成" + list_TALLYBILL.Count + "条理货";
                }
                else
                {
                    model_CONSIGN.State = "进行中";
                }
                db.Set<C_TB_HC_CONSIGN>().AddOrUpdate(model_CONSIGN);
                db.SaveChanges();
                save.code = "0";
                save.msg = "提交成功";
            }
            catch (Exception e)
            {
                Log4NetHelper log = new Log4NetHelper();
                log.Error(e.Message, e);
                save.code = "1";
                save.msg = "数据格式不正确";
                return ToJson(save);
            }
            return ToJson(save);
        }
        #endregion

        #region  //理货已完成
        [Route("api/lihuo/finish")]
        [System.Web.Http.HttpPost]
        public object lihuoFinish([FromBody]dynamic obj)
        {
            int ID = obj.ID;
            Api_common response = new Api_common();
            if (ID > 0)
            {
            }
            else
            {
                response.code = "2";
                response.msg = "数据格式不正确";
                return ToJson(response);
            }

            try
            {
                C_TB_HS_TALLYBILL model_TALLYBILL = db.C_TB_HS_TALLYBILL.Find(ID) ?? new C_TB_HS_TALLYBILL();
                model_TALLYBILL.State = "已完成";
                db.Set<C_TB_HS_TALLYBILL>().AddOrUpdate(model_TALLYBILL);
                db.SaveChanges();
                response.code = "0";
                response.msg = "提交成功";
            }
            catch (Exception e)
            {
                Log4NetHelper log = new Log4NetHelper();
                log.Error(e.Message, e);
                response.code = "1";
                response.msg = "数据格式不正确";
                return ToJson(response);
            }
            return ToJson(response);
        }
        #endregion


        /// <summary>
        ///编辑库存
        /// </summary>
        /// <returns></returns>
        public bool EidSTOCKDORMANT(C_TB_HS_TALLYBILL model, decimal AorU)
        {
            string Id = model.ID.ToString();
            C_TB_HS_STOCKDORMANT stocModel = db.C_TB_HS_STOCKDORMANT.FirstOrDefault(n => n.TALLYBILL_ID == Id) ?? new C_TB_HS_STOCKDORMANT();//理货单号
            C_TB_HC_CONSIGN Model_CONSIGN = db.C_TB_HC_CONSIGN.FirstOrDefault(n => n.ID == model.CONSIGN_ID);//委托号
            C_TB_HC_GOODSBILL Model_GoodsBill = db.C_TB_HC_GOODSBILL.FirstOrDefault(n => n.ID == Model_CONSIGN.GOODSBILL_ID);//票货编号
            stocModel.ID = stocModel.ID;
            stocModel.TALLYBILL_ID = model.ID.ToString();
            stocModel.CODE_STORAGE = model.STORAG;//场
            stocModel.CODE_BOOTH = model.CODE_SECTION;//位
            stocModel.GBNO = model.CGNO;//委托
            if (model.CODE_OPSTYPE == "进库")
            {
                stocModel.AMOUNT = model.AMOUNT;//件数
                stocModel.WEIGHT = model.WEIGHT;//重量
                stocModel.VOLUME = model.VOLUME;//体积
            }
            else
            {
                stocModel.AMOUNT = -model.AMOUNT;//件数
                stocModel.WEIGHT = -model.WEIGHT;//重量
                stocModel.VOLUME = -model.VOLUME;//体积
            }

            stocModel.FIRST_INDATE = DateTime.Now;
            stocModel.BOOTH_INDATE = DateTime.Now;
            stocModel.REMARK = model.CODE_OPSTYPE;
            stocModel.LastEidTime = DateTime.Now;
            stocModel.HuoDai = Model_GoodsBill.C_GOODSAGENT_NAME;
            stocModel.GoodsName = Model_GoodsBill.C_GOODS;
            db.C_TB_HS_STOCKDORMANT.AddOrUpdate(stocModel);
            try
            {
                db.SaveChanges();
                return true;
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            {
                var msg = string.Empty;
                var errors = (from u in ex.EntityValidationErrors select u.ValidationErrors).ToList();
                foreach (var item in errors)
                    msg += item.FirstOrDefault().ErrorMessage;

                return false;
            }
            catch (Exception e)
            {
                Log4NetHelper log = new Log4NetHelper();
                log.Error(e.Message, e);
                return false;
            }
        }
        #region 车队类型选项
        [Route("api/dic/dicCarTeam")]
        [System.Web.Http.HttpGet]
        public object dicCarTeam()
        {
            List<C_CARTEAM> dicvisibility = db.Set<C_CARTEAM>().ToList();
            Api_DicKV_Model a = null;
            if (dicvisibility != null)
            {
                a = new Api_DicKV_Model()
                {
                    code = "0",
                    msg = "请求成功",
                    data = new Api_DicKV_ModelList { }
                };

                foreach (var item in dicvisibility)
                {
                    a.data.list.Add(new Api_DicKV_ModelListKV()
                    {
                        value = item.ID.ToInt(),
                        text = item.GoodsName
                    });

                }
            }
            else
            {
                a = new Api_DicKV_Model()
                {
                    code = "1",
                    msg = "用户id请求错误",
                };
            }
            return ToJson(a);
        }
        #endregion

        #region 货物类型选项
        [Route("api/dic/dicGoodsType")]
        [System.Web.Http.HttpGet]
        public object dicGoodsType()
        {
            List<C_GOODSTYPE> dicvisibility = db.Set<C_GOODSTYPE>().ToList();
            Api_DicKV_Model a = null;
            if (dicvisibility != null)
            {
                a = new Api_DicKV_Model()
                {
                    code = "0",
                    msg = "请求成功",
                    data = new Api_DicKV_ModelList { }
                };

                foreach (var item in dicvisibility)
                {
                    a.data.list.Add(new Api_DicKV_ModelListKV()
                    {
                        value = item.ID.ToInt(),
                        text = item.GoodsName
                    });

                }
            }
            else
            {
                a = new Api_DicKV_Model()
                {
                    code = "1",
                    msg = "用户id请求错误",
                };
            }
            return ToJson(a);
        }
        #endregion

        #region  //库存管理列表
        [Route("api/kucun/list")]
        [System.Web.Http.HttpGet]
        public object KucunManageList(int userId, int limit, int page, string YuanQU_Name, string SECTION_Name)
        {
            Sys_User loginModel = db.Sys_User.FirstOrDefault(n => n.ID == userId);
            List<KuCun> kclist = new List<KuCun>();
            List<C_TB_CODE_BOOTH> list_Booth = db.C_TB_CODE_BOOTH.Where(n => n.CompanyId == loginModel.YuanQuId).OrderBy(n => n.ID).ToList();
            string CODE_SECTION = "";
            var storages = db.C_TB_CODE_STORAGE.AsQueryable();
            var SECTIONs = db.C_TB_CODE_SECTION.AsQueryable();
            var yuanqus = db.C_Dic_YuanQu.AsQueryable();
            foreach (var items_Booth in list_Booth)
            {
                KuCun kc = new KuCun();
                C_TB_CODE_STORAGE model_STORAGE = storages.FirstOrDefault(n => n.ID == items_Booth.Storage_ID);
                kc.STORAG_Name = model_STORAGE.STORAGEName;//获取场名称

                C_TB_CODE_SECTION model_SECTION = SECTIONs.FirstOrDefault(n => n.ID == model_STORAGE.SECTION_ID);
                kc.SECTION_Name = model_SECTION.SECTION;//获取库名称

                C_Dic_YuanQu model_YuanQu = yuanqus.FirstOrDefault(n => n.ID == model_SECTION.CODE_COMPANY);
                kc.YuanQU_Name = model_YuanQu.YuanQuName;//获取公司名称

                CODE_SECTION = items_Booth.ID.ToString();
                List<C_TB_HS_STOCKDORMANT> list_STOCKDORMANT = db.C_TB_HS_STOCKDORMANT.Where(n => n.CODE_BOOTH == CODE_SECTION).OrderBy(n => n.ID).ToList();
                kc.CODE_SECTION_Name = items_Booth.BOOTH;//获取位信息
                kc.STORAG = items_Booth.ID;//获取位信息
                foreach (var items_STOCKDORMANT in list_STOCKDORMANT)
                {

                    kc.AMOUNT += Convert.ToDecimal(items_STOCKDORMANT.AMOUNT);
                    kc.WEIGHT += Convert.ToDecimal(items_STOCKDORMANT.WEIGHT);
                }
                if (string.IsNullOrEmpty(YuanQU_Name) && string.IsNullOrEmpty(SECTION_Name))
                {
                    kclist.Add(kc);

                }
                else if (!string.IsNullOrEmpty(SECTION_Name) || !string.IsNullOrEmpty(YuanQU_Name))
                {
                    if (kc.SECTION_Name.Contains(SECTION_Name) && kc.YuanQU_Name.Contains(YuanQU_Name))
                    {
                        kclist.Add(kc);
                    }
                }

            }

            var mlist = kclist.OrderByDescending(n => n.YuanQU_Name).Skip(limit * (page - 1)).Take(limit);
            var a = kclist.Count();
            double cs = (double)a / limit;
            double b = Math.Ceiling(cs);
            Api_kucunModel ac = null;
            ac = new Api_kucunModel()
            {
                code = "0",
                msg = "请求成功",
                data = new Api_KuCunModelData()
                {
                    totalCount = a,
                    list = mlist,
                }


            };
            return ToJson(ac);
        }
        #endregion

        #region  //合同列表 第一级
        [Route("api/hetong/list")]
        [System.Web.Http.HttpGet]
        public object HetongList(int limit, int page, string key,string state,int userId)
        {
            Sys_User loginModel = db.Sys_User.FirstOrDefault(n => n.ID == userId && n.state == "激活");
            var wherelambda = ExtLinq.True<C_TB_HC_CONTRACT>();
            wherelambda = wherelambda.And(n => n.YuanQuID == loginModel.YuanQuId);
            if (!string.IsNullOrEmpty(key))
            {
                wherelambda = wherelambda.And(t => t.ContoractNumber.Contains(key) || t.EntrustPeople.Contains(key));
            }
            else if (!string.IsNullOrEmpty(state))
            {
                wherelambda = wherelambda.And(t => t.State == "通过" || t.State == "已完成" || t.State == "锁定");
            }
            var mlist = db.Set<C_TB_HC_CONTRACT>().Where(wherelambda).OrderByDescending(n => n.LastEdiTime).Skip<C_TB_HC_CONTRACT>((page - 1) * limit).Take<C_TB_HC_CONTRACT>(limit).AsQueryable();
            var a = mlist.Count();
            double cs = (double)a / limit;
            double b = Math.Ceiling(cs);
            Api_kucunModel ac = null;
            ac = new Api_kucunModel()
            {
                code = "0",
                msg = "请求成功",
                data = new Api_KuCunModelData()
                {
                    totalCount = a,
                    list = mlist,
                }


            };
            return ToJson(ac);
        }
        #endregion

        #region  //合同列表 第二级
        [Route("api/hetongNext/list")]
        [System.Web.Http.HttpGet]
        public object HetongNextList(int limit, int page, string key, string guid)
        {
            var wherelambda = ExtLinq.True<C_TB_HC_CONTRACT_DETAILED>();

            if (!string.IsNullOrEmpty(key))
            {
                wherelambda = wherelambda.And(t => t.HuoMing.Contains(key));
            }else if (!string.IsNullOrEmpty(guid))
            {
                wherelambda = wherelambda.And(t => t.CONTRACT_Guid == guid);
            }
            var mlist = db.Set<C_TB_HC_CONTRACT_DETAILED>().Where(wherelambda).OrderByDescending(n => n.LastEdiTime).Skip<C_TB_HC_CONTRACT_DETAILED>((page - 1) * limit).Take<C_TB_HC_CONTRACT_DETAILED>(limit).AsQueryable();
            var a = db.C_TB_HC_CONTRACT.Count();
            double cs = (double)a / limit;
            double b = Math.Ceiling(cs);
            Api_kucunModel ac = null;
            ac = new Api_kucunModel()
            {
                code = "0",
                msg = "请求成功",
                data = new Api_KuCunModelData()
                {
                    totalCount = a,
                    list = mlist,
                }


            };
            return ToJson(ac);
        }
        #endregion

        #region 是否需要更新版本
        [Route("api/check/update")]
        [System.Web.Http.HttpGet]
        public object  Versions(string version)
        {
            C_APPVERSION appVersion = db.C_APPVERSION.ToList().FirstOrDefault()??new C_APPVERSION();
            API_AppVersion api = null;
            if (appVersion.AppVersion ==null || appVersion.AppVersion == version || appVersion.State == "0" || appVersion.State == null || appVersion.State == "")//版本相同/或没有，不更新 0版本未激活
            {
                api = new API_AppVersion()
                {
                    code = "1",
                    msg = "请求成功",
                    data = new AppData
                    {
                        status = "0",
                        version = appVersion.AppVersion,
                        title = appVersion.AppTitle,
                        note = appVersion.AppNote,
                        url = appVersion.AppApkUrl
                    }
                };
            }
            ////else//版本不同，需要更新 state >0
            
            else{
                api = new API_AppVersion()
                {
                    code = "0",
                    msg = "请求成功",
                    data = new AppData
                    {
                        status = "1",
                        version = appVersion.AppVersion,
                        title = appVersion.AppTitle,
                        note = appVersion.AppNote,
                        url = appVersion.AppApkUrl
                    }
                };
            }

            //API_AppVersion api = null;
            //api = new API_AppVersion()
            //{
            //    code = "0",
            //    msg = "请求成功",
            //    data = new AppData
            //    {
            //        status = "1",
            //        version = version,
            //        title = "版本更新",
            //        note = "添加新年祝福",
            //        url = "/Upload/demo_picture.jpg"
            //    }
            //};

            return ToJson(api);
        }
        #endregion

        #region  //货代票货被业务员审核接口
        [Route("api/piaohuo/ywyVerify")]
        [System.Web.Http.HttpPost]
        public object piaohuoYwyVerify([FromBody]dynamic obj)
        {//state = 0 驳回  1 通过
            int id = obj.id;
            C_TB_HC_GOODSBILL model = db.C_TB_HC_GOODSBILL.Find(id);
            if (obj.state == 0)
            {
                model.State = "被驳回";
                model.RejectReason = obj.RejectReason;
            }else  if (obj.state == 1)
            {
                model.State = "待提交审核";
            }
            Api_common save = new Api_common();
            try
            {
                db.Set<C_TB_HC_GOODSBILL>().AddOrUpdate(model);
                db.SaveChanges();
                save.code = "0";
                save.msg = "提交成功";
            }
            catch (Exception e)
            {
                Log4NetHelper log = new Log4NetHelper();
                log.Error(e.Message, e);
                save.code = "1";
                save.msg = "数据格式不正确";
                return ToJson(save);
            }
            return ToJson(save);
        }
        #endregion

        #region  //货代委托被业务员审核接口
        [Route("api/weituo/ywyVerify")]
        [System.Web.Http.HttpPost]
        public object weituoYwyVerify([FromBody]dynamic obj)
        {//state = 0 驳回  1 通过
            int id = obj.id;
            C_TB_HC_CONSIGN model = db.C_TB_HC_CONSIGN.Find(id);
            if (obj.state == 0)
            {
                model.State = "被驳回";
                model.RejectReason = obj.RejectReason;
            }
            else if (obj.state == 1)
            {
                model.State = "待提交审核";
            }
            Api_common save = new Api_common();
            try
            {
                db.Set<C_TB_HC_CONSIGN>().AddOrUpdate(model);
                db.SaveChanges();
                save.code = "0";
                save.msg = "提交成功";
            }
            catch (Exception e)
            {
                Log4NetHelper log = new Log4NetHelper();
                log.Error(e.Message, e);
                save.code = "1";
                save.msg = "数据格式不正确";
                return ToJson(save);
            }
            return ToJson(save);
        }
        #endregion

        #region 园区id转name
        [Route("api/yuanqu/name")]
        [System.Web.Http.HttpGet]
        public object yuanquName(int id)
        {
            C_Dic_YuanQu dicvisibility = db.C_Dic_YuanQu.FirstOrDefault(n => n.ID == id);
            Api_DicKV_Model a = null;
            if (dicvisibility != null)
            {
                a = new Api_DicKV_Model()
                {
                    code = "0",
                    msg = "请求成功",
                    data = new Api_DicKV_ModelList { }
                };

                a.data.list.Add(new Api_DicKV_ModelListKV()
                {
                    value = dicvisibility.ID.ToInt(),
                    text = dicvisibility.YuanQuName
                });
            }
            else
            {
                a = new Api_DicKV_Model()
                {
                    code = "1",
                    msg = "用户id请求错误",
                };
            }
            return ToJson(a);
        }
        #endregion

        #region  //结算管理-第一级
        [Route("api/jiesuan/ywyVerify")]
        [System.Web.Http.HttpPost]
        public object jiesuanManage([FromBody]dynamic obj)
        {
            string GBNO = obj.GBNO;
            string searchKey = obj.searchKey;
            int page = obj.page;
            int limit = obj.limit;
            int userId = obj.userId;
            var wherelambda = ExtLinq.True<C_TB_HC_GOODSBILL>();
            Sys_User loginModel = db.Sys_User.Find(userId );
            if (loginModel.HuoDaiId > 0)
            {
                wherelambda = wherelambda.And(t => t.C_GOODSAGENT_ID == loginModel.HuoDaiId);
            }
            else if (!string.IsNullOrEmpty(searchKey))
            {
                wherelambda = wherelambda.And(t => t.C_GOODSAGENT_NAME.Contains(searchKey) || t.GBNO.Contains(searchKey));
            }
            else
            {
                wherelambda = wherelambda.And(t => t.YuanQuID == loginModel.YuanQuId);
            }
            wherelambda = wherelambda.And(t => t.State != "已完成");
            wherelambda = wherelambda.And(t => t.State != "待审核");
            wherelambda = wherelambda.And(t => t.State != "被驳回");
            if (!string.IsNullOrEmpty(GBNO))
            {
                wherelambda = wherelambda.And(t => t.GBNO.Contains(GBNO));
            } 
            var list = db.Set<C_TB_HC_GOODSBILL>().Where(wherelambda).OrderByDescending(n => n.ID).AsQueryable();

            foreach (var itsms_GoodsBill in list)//计算库存
            {
                C_TB_HC_CONTRACT Model_CONTRACT = db.C_TB_HC_CONTRACT.FirstOrDefault(n => n.Guid == itsms_GoodsBill.CONTRACT_Guid);//查找合同
                decimal? KuCun = 0;
                decimal? KuCunW = 0;
                List<C_TB_HC_CONSIGN> list_CONSIGN = db.C_TB_HC_CONSIGN.Where(n => n.GOODSBILL_ID == itsms_GoodsBill.ID).OrderBy(n => n.ID).ToList();
                foreach (var items_Consign in list_CONSIGN)
                {
                    List<C_TB_HS_TALLYBILL> list_TALLYBILL = db.C_TB_HS_TALLYBILL.Where(n => n.CONSIGN_ID == items_Consign.ID).OrderBy(n => n.ID).ToList();
                    foreach (var items_TALLYBILL in list_TALLYBILL)
                    {
                        if (items_TALLYBILL.CODE_OPSTYPE == "进库")
                        {
                            KuCun += items_TALLYBILL.AMOUNT;
                            KuCunW += items_TALLYBILL.WEIGHT;
                        }
                        if (items_TALLYBILL.CODE_OPSTYPE == "出库")
                        {
                            KuCun -= items_TALLYBILL.AMOUNT;
                            KuCunW -= items_TALLYBILL.WEIGHT;
                        }
                    }

                }
                decimal? ShiJiJInKu = 0;
                decimal? ShiJiChuKu = 0;
                MoneyController money = new MoneyController();
                money.GetTallyBllList_fy_hj(Convert.ToInt32(itsms_GoodsBill.ID), out decimal? Fyjehj, out decimal? Fyjehj_Sh, out decimal? Kpjehj_Sh);
                itsms_GoodsBill.Fyjehj = Fyjehj.ToDecimal(2);
                itsms_GoodsBill.KunCun = KuCun.ToString();
                itsms_GoodsBill.KunCunW = KuCunW.ToDecimal(3).ToString();
                List<C_TB_JIESUAN> list_JieSuan = db.C_TB_JIESUAN.Where(n => n.GoodsBill_id == itsms_GoodsBill.ID).OrderBy(n => n.ID).ToList();
                itsms_GoodsBill.Lkjehj = list_JieSuan.Sum(n => n.LaiKuanJinE).ToDecimal(2);
                itsms_GoodsBill.Sryk = (Fyjehj - list_JieSuan.Sum(n => n.LaiKuanJinE)).ToDecimal(2);
                List<C_TB_CHENGBEN> list_ChengBen = db.C_TB_CHENGBEN.Where(n => n.GoodsBill_id == itsms_GoodsBill.ID).OrderBy(n => n.ID).ToList();
                itsms_GoodsBill.ChengBenJinE = list_ChengBen.Sum(n => n.ChengBenJinE).ToDecimal(2);
                itsms_GoodsBill.LiRunZongE = (Fyjehj - list_ChengBen.Sum(n => n.ChengBenJinE)).ToDecimal(2);
                if (list_JieSuan.Sum(n => n.KaiPiaoJinE) == 0)
                {
                    itsms_GoodsBill.HuiShouLv = 0;
                }
                else
                {
                    itsms_GoodsBill.HuiShouLv = (list_JieSuan.Sum(n => n.LaiKuanJinE) / list_JieSuan.Sum(n => n.KaiPiaoJinE)).ToDecimal(2);

                }
                List<C_TB_HC_CONSIGN> list_KuCunCONSIGN = db.C_TB_HC_CONSIGN.Where(n => n.GOODSBILL_ID == itsms_GoodsBill.ID).OrderBy(n => n.ID).ToList();
                foreach (var items_KuCunCONSIGN in list_KuCunCONSIGN)
                {
                    List<C_TB_HS_TALLYBILL> list_KuCunTALLYBILL = db.C_TB_HS_TALLYBILL.Where(n => n.CONSIGN_ID == items_KuCunCONSIGN.ID).OrderBy(n => n.ID).ToList();
                    ShiJiJInKu += list_KuCunTALLYBILL.Where(n => n.CODE_OPSTYPE == "进库" && n.Type != "清场").Sum(n => n.WEIGHT);
                    ShiJiChuKu += list_KuCunTALLYBILL.Where(n => n.CODE_OPSTYPE == "出库" && n.Type != "清场").Sum(n => n.WEIGHT);
                }
                itsms_GoodsBill.ShiJiJInKu = ShiJiJInKu.ToDecimal(3);
                itsms_GoodsBill.ShiJiChuKu = ShiJiChuKu.ToDecimal(3);
                itsms_GoodsBill.KaiPiaoJineHJ = list_JieSuan.Sum(n => n.KaiPiaoJinE).ToDecimal(2);
                itsms_GoodsBill.KaiPiaoShuiHouHJ = Kpjehj_Sh.ToDecimal(2);
                itsms_GoodsBill.WeiKaiPiaoJineHJ = (Fyjehj - list_JieSuan.Sum(n => n.KaiPiaoJinE)).ToDecimal(2);
                itsms_GoodsBill.WeiKaiPiaoShuiHouHJ = (Fyjehj_Sh - Kpjehj_Sh).ToDecimal(2);
                itsms_GoodsBill.YinShouZhangKuanYue = (list_JieSuan.Sum(n => n.LaiKuanJinE) - list_JieSuan.Sum(n => n.KaiPiaoJinE)).ToDecimal(2);
                itsms_GoodsBill.LiRunZongE = (Fyjehj - list_ChengBen.Sum(n => n.ChengBenJinE)).ToDecimal(2);
            }

            int total = list.Count();
            List<C_TB_HC_GOODSBILL> rows = list.Skip((page-1)*limit).Take(limit).AsQueryable().ToList();


            API_piaohuoModel ac = null;
            ac = new API_piaohuoModel()
            {
                code = "0",
                msg = "请求成功",
                data = new Api_piaohuoModelData()
                {
                    totalCount = total,
                    list = rows,
                }


            };
            return ToJson(ac);

        }
        #endregion

        #region  //合同管理经理审核 业务提交、撤回接口
        [Route("api/hetong/jlVerify")]
        [System.Web.Http.HttpPost]
        public object HeTongjlVerify([FromBody]dynamic obj)
        {
            string guid = obj.guid;
            string state = obj.state;
            Api_common save = new Api_common();
            C_TB_HC_CONTRACT u = db.C_TB_HC_CONTRACT.Find(guid);
            u.State = state;
            int c = db.SaveChanges();
            if (c > 0)
            {
                save.code = "0";
                save.msg = "提交成功";
                return ToJson(save);
            }
            else
            {
                if (c == 0)
                {
                    save.code = "1";
                    save.msg = "无更新";
                    return ToJson(save);
                }
                save.code = "2";
                save.msg = "失败";
                return ToJson(save);
            }
        }
        #endregion

        #region  //合同管理 删除
        [Route("api/hetong/delete")]
        [System.Web.Http.HttpPost]
        public object HeTongDel([FromBody]dynamic obj)
        {
            string guid = obj.guid;
            Api_common save = new Api_common();

            C_TB_HC_CONTRACT u = new C_TB_HC_CONTRACT() { Guid = guid };
            db.C_TB_HC_CONTRACT.Attach(u);
            db.C_TB_HC_CONTRACT.Remove(u);
            string delsql = "delete from C_TB_HC_CONTRACT_DETAILED where 'CONTRACT_Guid'='" + guid + "'";
            db.Database.ExecuteSqlCommand(delsql);
            delsql = "delete from C_TB_HC_CONTRACT_FILES where 'CONTRACT_Guid'='" + guid + "'";
            db.Database.ExecuteSqlCommand(delsql);
            int c = db.SaveChanges();
            if (c > 0)
            {
                save.code = "0";
                save.msg = "提交成功";
                return ToJson(save);
            }
            else
            {
                if (c == 0)
                {
                    save.code = "1";
                    save.msg = "无更新";
                    return ToJson(save);
                }
                save.code = "2";
                save.msg = "失败";
                return ToJson(save);
            }
        }
        #endregion

        #region  //合同管理 锁定
        [Route("api/hetong/lock")]
        [System.Web.Http.HttpPost]
        public object HeTongLock([FromBody]dynamic obj)
        {
            string guid = obj.guid;
            Api_common save = new Api_common();

            C_TB_HC_CONTRACT u = db.C_TB_HC_CONTRACT.Find(guid);
            u.State = "已完成";

            string delsql = "update  C_TB_HC_CONTRACT_DETAILED set C_TB_HC_CONTRACT_DETAILED.\"State\"='已完成' where C_TB_HC_CONTRACT_DETAILED.\"CONTRACT_Guid\"='" + guid + "'";
            db.Database.ExecuteSqlCommand(delsql);


            delsql = "update  C_TB_HC_CONTRACT_FILES set C_TB_HC_CONTRACT_FILES.\"State\"='已完成' where C_TB_HC_CONTRACT_FILES.\"CONTRACT_Guid\"='" + guid + "'";
            db.Database.ExecuteSqlCommand(delsql);
            int c = db.SaveChanges();
            if (c > 0)
            {
                save.code = "0";
                save.msg = "提交成功";
                return ToJson(save);
            }
            else
            {
                if (c == 0)
                {
                    save.code = "1";
                    save.msg = "无更新";
                    return ToJson(save);
                }
                save.code = "2";
                save.msg = "失败";
                return ToJson(save);
            }
        }
        #endregion

        #region  //合同管理 锁定
        [Route("api/hetongSon/delete")]
        [System.Web.Http.HttpPost]
        public object HeTongSonDelete([FromBody]dynamic obj)
        {
            string guid = obj.guid;
            Api_common save = new Api_common();

            C_TB_HC_CONTRACT_DETAILED u = new C_TB_HC_CONTRACT_DETAILED() { Guid = guid };
            db.C_TB_HC_CONTRACT_DETAILED.Attach(u);
            db.C_TB_HC_CONTRACT_DETAILED.Remove(u);
            int c = db.SaveChanges();
            if (c > 0)
            {
                save.code = "0";
                save.msg = "提交成功";
                return ToJson(save);
            }
            else
            {
                if (c == 0)
                {
                    save.code = "1";
                    save.msg = "无更新";
                    return ToJson(save);
                }
                save.code = "2";
                save.msg = "失败";
                return ToJson(save);
            }
        }
        #endregion

        #region 费用详情
        [Route("api/feiyong/datalist")]
        [System.Web.Http.HttpPost]
        public object FeiyongDatalist([FromBody]dynamic obj)
        {
            int gooodBillId = obj.goodBillId;
            C_TB_HC_GOODSBILL goodBill = db.C_TB_HC_GOODSBILL.Find(gooodBillId);
            Api_KeyValueModelListKV hd = new Api_KeyValueModelListKV()
            {
                text = "货代",
                value = goodBill.C_GOODSAGENT_NAME ?? ""
            };
            Api_KeyValueModelListKV hw = new Api_KeyValueModelListKV()
            {
                text = "货物",
                value = goodBill.C_GOODS ?? ""
            };
            Api_KeyValueModelListKV cm = new Api_KeyValueModelListKV()
            {
                text = "船名",
                value = goodBill.ShipName ?? ""
            };
            Api_KeyValueModelListKV hc = new Api_KeyValueModelListKV()
            {
                text = "航次",
                value = goodBill.VGNO ?? ""
            };


            C_TB_HC_CONTRACT Model_CONTRACT = db.C_TB_HC_CONTRACT.FirstOrDefault(n => n.Guid == goodBill.CONTRACT_Guid);//查找合同
            decimal? KuCun = 0;
            decimal? KuCunW = 0;
            List<C_TB_HC_CONSIGN> list_CONSIGN = db.C_TB_HC_CONSIGN.Where(n => n.GOODSBILL_ID == goodBill.ID).OrderBy(n => n.ID).ToList();
            foreach (var items_Consign in list_CONSIGN)
            {
                List<C_TB_HS_TALLYBILL> list_TALLYBILL = db.C_TB_HS_TALLYBILL.Where(n => n.CONSIGN_ID == items_Consign.ID).OrderBy(n => n.ID).ToList();
                foreach (var items_TALLYBILL in list_TALLYBILL)
                {
                    if (items_TALLYBILL.CODE_OPSTYPE == "进库")
                    {
                        KuCun += items_TALLYBILL.AMOUNT;
                        KuCunW += items_TALLYBILL.WEIGHT;
                    }
                    if (items_TALLYBILL.CODE_OPSTYPE == "出库")
                    {
                        KuCun -= items_TALLYBILL.AMOUNT;
                        KuCunW -= items_TALLYBILL.WEIGHT;
                    }
                }

            }
            decimal? ShiJiJInKu = 0;
            decimal? ShiJiChuKu = 0;
            MoneyController money = new MoneyController();
            money.GetTallyBllList_fy_hj(Convert.ToInt32(goodBill.ID), out decimal? Fyjehj, out decimal? Fyjehj_Sh, out decimal? Kpjehj_Sh);
            goodBill.Fyjehj = Fyjehj.ToDecimal(2);
            goodBill.KunCun = KuCun.ToString();
            goodBill.KunCunW = KuCunW.ToDecimal(3).ToString();
            List<C_TB_JIESUAN> list_JieSuan = db.C_TB_JIESUAN.Where(n => n.GoodsBill_id == goodBill.ID).OrderBy(n => n.ID).ToList();
            goodBill.Lkjehj = list_JieSuan.Sum(n => n.LaiKuanJinE).ToDecimal(2);
            goodBill.Sryk = (Fyjehj - list_JieSuan.Sum(n => n.LaiKuanJinE)).ToDecimal(2);
            List<C_TB_CHENGBEN> list_ChengBen = db.C_TB_CHENGBEN.Where(n => n.GoodsBill_id == goodBill.ID).OrderBy(n => n.ID).ToList();
            goodBill.ChengBenJinE = list_ChengBen.Sum(n => n.ChengBenJinE).ToDecimal(2);
            goodBill.LiRunZongE = (Fyjehj - list_ChengBen.Sum(n => n.ChengBenJinE)).ToDecimal(2);
            if (list_JieSuan.Sum(n => n.KaiPiaoJinE) == 0)
            {
                goodBill.HuiShouLv = 0;
            }
            else
            {
                goodBill.HuiShouLv = (list_JieSuan.Sum(n => n.LaiKuanJinE) / list_JieSuan.Sum(n => n.KaiPiaoJinE)).ToDecimal(2);

            }
            List<C_TB_HC_CONSIGN> list_KuCunCONSIGN = db.C_TB_HC_CONSIGN.Where(n => n.GOODSBILL_ID == goodBill.ID).OrderBy(n => n.ID).ToList();
            foreach (var items_KuCunCONSIGN in list_KuCunCONSIGN)
            {
                List<C_TB_HS_TALLYBILL> list_KuCunTALLYBILL = db.C_TB_HS_TALLYBILL.Where(n => n.CONSIGN_ID == items_KuCunCONSIGN.ID).OrderBy(n => n.ID).ToList();
                ShiJiJInKu += list_KuCunTALLYBILL.Where(n => n.CODE_OPSTYPE == "进库" && n.Type != "清场").Sum(n => n.WEIGHT);
                ShiJiChuKu += list_KuCunTALLYBILL.Where(n => n.CODE_OPSTYPE == "出库" && n.Type != "清场").Sum(n => n.WEIGHT);
            }
            goodBill.ShiJiJInKu = ShiJiJInKu.ToDecimal(3);
            goodBill.ShiJiChuKu = ShiJiChuKu.ToDecimal(3);
            goodBill.KaiPiaoJineHJ = list_JieSuan.Sum(n => n.KaiPiaoJinE).ToDecimal(2);
            goodBill.KaiPiaoShuiHouHJ = Kpjehj_Sh.ToDecimal(2);
            goodBill.WeiKaiPiaoJineHJ = (Fyjehj - list_JieSuan.Sum(n => n.KaiPiaoJinE)).ToDecimal(2);
            goodBill.WeiKaiPiaoShuiHouHJ = (Fyjehj_Sh - Kpjehj_Sh).ToDecimal(2);
            goodBill.YinShouZhangKuanYue = (list_JieSuan.Sum(n => n.LaiKuanJinE) - list_JieSuan.Sum(n => n.KaiPiaoJinE)).ToDecimal(2);
            goodBill.LiRunZongE = (Fyjehj - list_ChengBen.Sum(n => n.ChengBenJinE)).ToDecimal(2);
            goodBill.YikaiPiaoYinShouYuE = (list_JieSuan.Sum(n => n.KaiPiaoJinE) - list_JieSuan.Sum(n => n.LaiKuanJinE)).ToDecimal(2);
            goodBill.WeiKaiPiaoYingShouYuE = ((Fyjehj - list_JieSuan.Sum(n => n.KaiPiaoJinE)) - list_JieSuan.Sum(n => n.YuShouJinE)).ToDecimal(2);


            Api_KeyValueModelListKV fyjehjkv = new Api_KeyValueModelListKV()
            {
                text = "费用金额合计",
                value = goodBill.Fyjehj.ToString() ?? ""
            };
            Api_KeyValueModelListKV kphj_kv = new Api_KeyValueModelListKV()
            {
                text = "开票金额合计",
                value = goodBill.KaiPiaoJineHJ.ToString() ?? ""
            };
            Api_KeyValueModelListKV wkp_kv = new Api_KeyValueModelListKV()
            {
                text = "未开票金额合计",
                value = goodBill.WeiKaiPiaoJineHJ.ToString() ?? ""
            };

            Api_KeyValueModelListKV cbje_kv = new Api_KeyValueModelListKV()
            {
                text = "成本金额合计",
                value = goodBill.ChengBenJinE.ToString() ?? ""
            };
            Api_KeyValueModelListKV lrje_kv = new Api_KeyValueModelListKV()
            {
                text = "利润总额",
                value = goodBill.LiRunZongE.ToString() ?? ""
            };


            // 费用种类和成本利率在子合同中
            //List<C_TB_HC_CONTRACT_DETAILED> contractSonList = db.C_TB_HC_CONTRACT_DETAILED.Where(n => n.CONTRACT_Guid == Model_CONTRACT.Guid).ToList();//查找子合同
            //foreach (var item in contractSonList)
            //{
            //    Api_KeyValueModelListKV a = new Api_KeyValueModelListKV()
            //    {
            //        text = item.FeiMuZhongLei+"/利率",
            //        value = item.ChengBenFeiLv.ToString() ?? "" + "/" + (item.ChengBenFeiLv.ToString() ?? "")
            //    };
            //}

            DataTableController dataTable = new DataTableController();
            List<Stock_Money> slist = dataTable.Get_FeiMuXX(gooodBillId, goodBill.CONTRACT_Guid);
            List<Api_KeyValueModelListKV> tem = new List<Api_KeyValueModelListKV>();
            foreach (var item in slist)
            {
                Api_KeyValueModelListKV one = new Api_KeyValueModelListKV()
                {
                    text = item.FMZhongLei + "/成本费率",
                    value = (item.FeiYong.ToString() ?? "") + "/" + (item.ChengBenFeiLv.ToString() ?? "")
                };
                tem.Add(one);
            }

            Api_KeyValue a = null;
            a = new Api_KeyValue()
            {
                code = "0",
                msg = "请求成功",
                data = new Api_KeyValueModelList { }
            };
            a.data.list.Add(hd);
            a.data.list.Add(hw);
            a.data.list.Add(cm);
            a.data.list.Add(hc);
            foreach (var item in tem)
            {
                a.data.list.Add(item);
            }
            a.data.list.Add(fyjehjkv);
            a.data.list.Add(kphj_kv);
            a.data.list.Add(wkp_kv);
            a.data.list.Add(cbje_kv);
            a.data.list.Add(lrje_kv);

            return ToJson(a);
        }
        #endregion

        #region  //票货管理经理审核 业务提交、撤回接口
        [Route("api/piaohuo/verify")]
        [System.Web.Http.HttpPost]
        public object PiaohuoVerify([FromBody]dynamic obj)
        {
            int id = obj.id;
            string state = obj.state;
            Api_common save = new Api_common();
            C_TB_HC_GOODSBILL u = db.C_TB_HC_GOODSBILL.FirstOrDefault(n => n.ID == id);
            u.State = state;
            int c = db.SaveChanges();
            if (c > 0)
            {
                save.code = "0";
                save.msg = "提交成功";
                return ToJson(save);
            }
            else
            {
                if (c == 0)
                {
                    save.code = "1";
                    save.msg = "无更新";
                    return ToJson(save);
                }
                save.code = "2";
                save.msg = "失败";
                return ToJson(save);
            }
        }
        #endregion

        #region  //委托管理经理审核 业务提交、撤回接口
        [Route("api/weituo/verify")]
        [System.Web.Http.HttpPost]
        public object WeituoVerify([FromBody]dynamic obj)
        {
            int id = obj.id;
            string state = obj.state;
            Api_common save = new Api_common();
            C_TB_HC_CONSIGN u = db.C_TB_HC_CONSIGN.FirstOrDefault(n => n.ID == id);
            u.State = state;
            int c = db.SaveChanges();
            if (c > 0)
            {
                save.code = "0";
                save.msg = "提交成功";
                return ToJson(save);
            }
            else
            {
                if (c == 0)
                {
                    save.code = "1";
                    save.msg = "无更新";
                    return ToJson(save);
                }
                save.code = "2";
                save.msg = "失败";
                return ToJson(save);
            }
        }
        #endregion


        #region  //上传 委托文件
        [Route("api/upload/weituofile")]
        [System.Web.Http.HttpPost]
        public object UploadFile()
        {
            string CONSING_ID = HttpContext.Current.Request.Form["weituoId"];
            HttpFileCollection filelist = HttpContext.Current.Request.Files;

            string urlPath = "Upload/" + CONSING_ID;
            string filePathName = string.Empty;
            Api_fileSaveModel save = null;
            string localPath = Path.Combine(HttpRuntime.AppDomainAppPath, urlPath);
            if (filelist == null || filelist.Count == 0)
            {
                save = new Api_fileSaveModel()
                {
                    code = "1",
                    msg = "请求失败"
                };
                return ToJson(save);
            }
            string guid = "";
            for (int i = 0; i < filelist.Count; i++)
            {
                HttpPostedFile file = filelist[i];
                filePathName = file.FileName;
                if (!System.IO.Directory.Exists(localPath))
                {
                    System.IO.Directory.CreateDirectory(localPath);
                }
                file.SaveAs(Path.Combine(localPath, filePathName));
                C_TB_HC_CONSIGN_FILES f = new C_TB_HC_CONSIGN_FILES()
                {
                    CONSING_ID = CONSING_ID,
                    path = urlPath + "/" + filePathName,
                    FileName = filePathName,
                    Guid = Guid.NewGuid().ToString(),
                    State = "进行中"

                };
                db.C_TB_HC_CONSIGN_FILES.Add(f);
                db.SaveChanges();
                guid = f.Guid;
            }

            save = new Api_fileSaveModel()
            {
                code = "0",
                msg = "请求成功",
                data = new Api_fileSaveData
                {
                    fileName = filePathName,
                    fileUrl = urlPath + "/" + filePathName,
                    guid = guid
                }
            };
            return ToJson(save);


            //const string saveTempPath = "~/Upload/";
            //var dirTempPath = System.Web.HttpContext.Current.Server.MapPath(saveTempPath);

            //string result = "";
            //var str = HttpContext.Current.Request.Form["goodid"];
            //HttpFileCollection filelist = HttpContext.Current.Request.Files;
            //if (filelist != null && filelist.Count > 0)
            //{
            //    for (int i = 0; i < filelist.Count; i++)
            //    {
            //        HttpPostedFile file = filelist[i];
            //        String Tpath = "/" + DateTime.Now.ToString("yyyy-MM-dd") + "/";
            //        string filename = file.FileName;
            //        string FileName = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            //        string FilePath = dirTempPath + "\\" + Tpath + "\\";
            //        DirectoryInfo di = new DirectoryInfo(FilePath);
            //        if (!di.Exists) { di.Create(); }
            //        try
            //        {
            //            file.SaveAs(FilePath + FileName);
            //            //result.obj = (Tpath + FileName).Replace("\\", "/");
            //        }
            //        catch (Exception ex)
            //        {
            //            result = "上传文件写入失败：" + ex.Message;
            //        }
            //    }
            //}
            //else
            //{
            //    result = "上传的文件信息不存在！";
            //}

            //return result;

            //string imgStr = obj.imageStr;
            ////// 文件保存目录路径 
            //const string saveTempPath = "~/Upload/";
            //var dirTempPath = System.Web.HttpContext.Current.Server.MapPath(saveTempPath);
            ////将Base64String转为图片并保存
            //byte[] arr2 = Convert.FromBase64String(imgStr);
            //using (MemoryStream ms2 = new MemoryStream(arr2))
            //{
            //    System.Drawing.Bitmap bmp2 = new System.Drawing.Bitmap(ms2);
            //    bmp2.Save(dirTempPath + "avatar.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
            //    //bmp2.Save(filePath + ".bmp", System.Drawing.Imaging.ImageFormat.Bmp);
            //    //bmp2.Save(filePath + ".gif", System.Drawing.Imaging.ImageFormat.Gif);
            //    //bmp2.Save(filePath + ".png", System.Drawing.Imaging.ImageFormat.Png);
            //}
            //return ToJson(imgStr);
        }
        #endregion

        #region  //删除上传文件 委托
        [Route("api/delete/weituofile")]
        [System.Web.Http.HttpPost]
        public object DeleteWeituoFile([FromBody]dynamic obj)
        {
            string guid = obj.guid;
            C_TB_HC_CONSIGN_FILES u = new C_TB_HC_CONSIGN_FILES() { Guid = guid };
            db.C_TB_HC_CONSIGN_FILES.Attach(u);
            db.C_TB_HC_CONSIGN_FILES.Remove(u);
            Api_common save = new Api_common();
            int c = db.SaveChanges();
            if (c > 0)
            {
                save.code = "0";
                save.msg = "提交成功";
                return ToJson(save);
            }
            else
            {
                if (c == 0)
                {
                    save.code = "1";
                    save.msg = "无更新";
                    return ToJson(save);
                }
                save.code = "2";
                save.msg = "失败";
                return ToJson(save);
            }
        }
        #endregion

        #region  //获取文件 委托
        [Route("api/get/weituofile")]
        [System.Web.Http.HttpPost]
        public object GetWeituoFile([FromBody]dynamic obj)
        {
            string weituoId = obj.weituoId;
            var mlist = db.Set<C_TB_HC_CONSIGN_FILES>().Where(n => n.CONSING_ID == weituoId).AsQueryable();
            Api_kucunModel ac = null;
            ac = new Api_kucunModel()
            {
                code = "0",
                msg = "请求成功",
                data = new Api_KuCunModelData()
                {
                    list = mlist
                }


            };
            return ToJson(ac);
        }
        #endregion


        #region  //图标 客户分析
        [Route("api/chart/khfx")]
        [System.Web.Http.HttpPost]
        public object ChartKhfx([FromBody]dynamic obj)
        {
            int userId = obj.userId;
            DateTime? startTime = null;
            DateTime? endTime = null;
            if (!string.IsNullOrEmpty(obj.startTime.ToString()))
            {
                startTime = obj.startTime;
            }
            else if (!string.IsNullOrEmpty(obj.endTime.ToString()))
            {
                endTime = obj.endTime;
            }

            Api_kucunModel ac = null;
            Sys_User loginModel = db.Sys_User.FirstOrDefault(n => n.ID == userId);
            try
            {
                //1从票货表中获取所有的货代信息（客户），2从委托表中获取所理货单数据3从理货单中计算数据
                var yuanquid = loginModel.YuanQuId ?? throw new ArgumentNullException("loginModel.YuanquID");
                var wherelambda = ExtLinq.True<C_TB_HC_GOODSBILL>();
                wherelambda = wherelambda.And(t => t.YuanQuID == yuanquid);
                if (startTime != null)
                {
                    wherelambda = wherelambda.And(t => t.CreatTime >= startTime);
                }
                if (endTime != null)
                {
                    wherelambda = wherelambda.And(t => t.CreatTime <= endTime);
                }
                Array HuoDaiIDArray = db.C_TB_HC_GOODSBILL.Where(wherelambda).Select(n => n.C_GOODSAGENT_ID).Distinct().ToArray();
                List<int> goodsagentid = new List<int>();//货代id
                foreach (var t in HuoDaiIDArray)
                {
                    if (t != null)
                    {
                        goodsagentid.Add(t.ToInt());
                    }
                }
                List<KehuFenxiEntry> listentry = new List<KehuFenxiEntry>();
                DataTableController dc = new DataTableController();
                foreach (var t in goodsagentid)
                {
                    KehuFenxiEntry entry = new KehuFenxiEntry();
                    decimal? ywushuliang = 0;
                    decimal? shouru = 0;
                    List<C_TB_HC_GOODSBILL> piaoHuolist = db.C_TB_HC_GOODSBILL.Where(n => n.C_GOODSAGENT_ID == t).ToList();
                    foreach (var p in piaoHuolist)
                    {
                        List<Stock_Money> listStock_Money = new List<Stock_Money>();
                        try
                        {
                            listStock_Money = dc.Get_FeiMuXX(p.ID.ToInt(), p.CONTRACT_Guid);
                            shouru = listStock_Money.Sum(n => n.FeiYong);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                            throw;
                        }
                        entry.name = p.C_GOODSAGENT_NAME;
                        List<C_TB_HC_CONSIGN> weituoList = db.C_TB_HC_CONSIGN.Where(n => n.GOODSBILL_ID == p.ID && n.CODE_OPERATION == "汽-场").ToList();
                        foreach (var w in weituoList)
                        {
                            List<C_TB_HS_TALLYBILL> LihuoDanList2 = db.C_TB_HS_TALLYBILL.Where(n => n.CONSIGN_ID == w.ID && n.CODE_OPSTYPE == "进库").ToList();
                            foreach (var l in LihuoDanList2)
                            {
                                ywushuliang += l.AMOUNT + l.WEIGHT;
                            }
                        }
                        entry.zuoyeliang = ywushuliang;
                        entry.shouru = shouru;

                    }
                    listentry.Add(entry);
                }
                ac = new Api_kucunModel()
                {
                    code = "0",
                    msg = "请求成功",
                    data = new Api_KuCunModelData()
                    {
                        list = listentry
                    }


                };
                return ToJson(ac);
            }
            catch (Exception e)
            {
                Log4NetHelper log = new Log4NetHelper();
                log.Error(e.Message, e);
                ac = new Api_kucunModel()
                {
                    code = "1",
                    msg = "请求失败",
                };
                return ToJson(ac);
                throw;
            }
        }
        #endregion


        #region  //图标 客户作业量分析    GetKehufenxi2
        [Route("api/chart/khzylfx")]
        [System.Web.Http.HttpPost]
        public object ChartKhzylfx([FromBody]dynamic obj)
        {
            int userId = obj.userId;
            DateTime? startTime=null;
            DateTime? endTime=null;
            if (!string.IsNullOrEmpty(obj.startTime.ToString()))
            {
                startTime = obj.startTime;
            }
            else if (!string.IsNullOrEmpty(obj.endTime.ToString()))
            {
                endTime = obj.endTime;
            }
            
            Api_kucunModel ac = null;
            Sys_User loginModel = db.Sys_User.FirstOrDefault(n => n.ID == userId);
            try
            {
                //1从票货表中获取所有的货代信息（客户），2从委托表中获取所理货单数据3从理货单中计算数据
                var yuanquid = loginModel.YuanQuId ?? throw new ArgumentNullException("loginModel.YuanquID");
                var wherelambda = ExtLinq.True<C_TB_HC_GOODSBILL>();
                wherelambda = wherelambda.And(t => t.YuanQuID == yuanquid);
                if (startTime != null)
                {
                    wherelambda = wherelambda.And(t => t.CreatTime >= startTime);
                }
                if (endTime != null)
                {
                    wherelambda = wherelambda.And(t => t.CreatTime <= endTime);
                }
                Array HuoDaiIDArray = db.C_TB_HC_GOODSBILL.Where(wherelambda).Select(n => n.C_GOODSAGENT_ID).Distinct().ToArray();
                List<int> goodsagentid = new List<int>();//货代id
                foreach (var t in HuoDaiIDArray)
                {
                    if (t != null)
                    {
                        goodsagentid.Add(t.ToInt());
                    }
                }
                List<KehuFenxiEntry> listentry = new List<KehuFenxiEntry>();

                foreach (var t in goodsagentid)
                {
                    KehuFenxiEntry entry = new KehuFenxiEntry();
                    decimal? ywushuliang = 0;

                    List<C_TB_HC_GOODSBILL> piaoHuolist = db.C_TB_HC_GOODSBILL.Where(n => n.C_GOODSAGENT_ID == t).ToList();
                    foreach (var p in piaoHuolist)
                    {

                        entry.name = p.C_GOODSAGENT_NAME;
                        List<C_TB_HC_CONSIGN> weituoList = db.C_TB_HC_CONSIGN.Where(n => n.GOODSBILL_ID == p.ID && n.CODE_OPERATION == "汽-场").ToList();
                        foreach (var w in weituoList)
                        {
                            List<C_TB_HS_TALLYBILL> LihuoDanList2 = db.C_TB_HS_TALLYBILL.Where(n => n.CONSIGN_ID == w.ID && n.CODE_OPSTYPE == "进库").ToList();
                            foreach (var l in LihuoDanList2)
                            {
                                ywushuliang += l.AMOUNT + l.WEIGHT;
                            }
                        }
                        entry.zuoyeliang = ywushuliang;


                    }
                    listentry.Add(entry);
                }
                ac = new Api_kucunModel()
                {
                    code = "0",
                    msg = "请求成功",
                    data = new Api_KuCunModelData()
                    {
                        list = listentry
                    }


                };
                return ToJson(ac);
            }
            catch (Exception e)
            {
                Log4NetHelper log = new Log4NetHelper();
                log.Error(e.Message, e);
                ac = new Api_kucunModel()
                {
                    code = "1",
                    msg = "请求失败",
                };
                return ToJson(ac);
                throw;
            }
        }
        #endregion


        #region  //客户费用饼状图    GetKehufenxi3
        [Route("api/chart/khfybzt")]
        [System.Web.Http.HttpPost]
        public object ChartKhfybzt([FromBody]dynamic obj)
        {
            int userId = obj.userId;
            DateTime? startTime = null;
            DateTime? endTime = null;
            if (!string.IsNullOrEmpty(obj.startTime.ToString()))
            {
                startTime = obj.startTime;
            }
            else if (!string.IsNullOrEmpty(obj.endTime.ToString()))
            {
                endTime = obj.endTime;
            }

            Api_kucunModel ac = null;
            Sys_User loginModel = db.Sys_User.FirstOrDefault(n => n.ID == userId);
            try
            {
                var yuanquid = loginModel.YuanQuId ?? throw new ArgumentNullException("loginModel.YuanquID");
                var wherelambda = ExtLinq.True<C_TB_HC_GOODSBILL>();
                wherelambda = wherelambda.And(t => t.YuanQuID == yuanquid);
                if (startTime != null)
                {
                    wherelambda = wherelambda.And(t => t.CreatTime >= startTime);
                }
                if (endTime != null)
                {
                    wherelambda = wherelambda.And(t => t.CreatTime <= endTime);
                }
                Array HuoDaiIDArray = db.C_TB_HC_GOODSBILL.Where(wherelambda).Select(n => n.C_GOODSAGENT_ID).Distinct().ToArray();
                List<int> goodsagentid = new List<int>();//货代id
                foreach (var t in HuoDaiIDArray)
                {
                    if (t != null)
                    {
                        goodsagentid.Add(t.ToInt());
                    }
                }
                List<KehuFenxiEntry> listentry = new List<KehuFenxiEntry>();
                DataTableController dc = new DataTableController();
                foreach (var t in goodsagentid)
                {
                    KehuFenxiEntry entry = new KehuFenxiEntry();
                    //decimal? ywushuliang = 0;
                    decimal? shouru = 0;
                    List<C_TB_HC_GOODSBILL> piaoHuolist = db.C_TB_HC_GOODSBILL.Where(n => n.C_GOODSAGENT_ID == t).ToList();
                    foreach (var p in piaoHuolist)
                    {
                        List<Stock_Money> listStock_Money = new List<Stock_Money>();
                        try
                        {
                            listStock_Money = dc.Get_FeiMuXX(p.ID.ToInt(), p.CONTRACT_Guid);
                            shouru = listStock_Money.Sum(n => n.FeiYong);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                            throw;
                        }
                        entry.name = p.C_GOODSAGENT_NAME;


                        entry.shouru = shouru;

                    }
                    listentry.Add(entry);
                }
                ac = new Api_kucunModel()
                {
                    code = "0",
                    msg = "请求成功",
                    data = new Api_KuCunModelData()
                    {
                        list = listentry
                    }


                };
                return ToJson(ac);
            }
            catch (Exception e)
            {
                Log4NetHelper log = new Log4NetHelper();
                log.Error(e.Message, e);
                ac = new Api_kucunModel()
                {
                    code = "1",
                    msg = "请求失败",
                };
                return ToJson(ac);
                throw;
            }
        }
        #endregion



        #region  //图表 货种分析    GetKehufenxi2
        [Route("api/chart/hzfx")]
        [System.Web.Http.HttpPost]
        public object Charthzfx([FromBody]dynamic obj)
        {
            int userId = obj.userId;
            DateTime? startTime = null;
            DateTime? endTime = null;
            if (!string.IsNullOrEmpty(obj.startTime.ToString()))
            {
                startTime = obj.startTime;
            }
            else if (!string.IsNullOrEmpty(obj.endTime.ToString()))
            {
                endTime = obj.endTime;
            }

            Api_kucunModel ac = null;
            Sys_User loginModel = db.Sys_User.FirstOrDefault(n => n.ID == userId);
            try
            {
                var yuanquid = loginModel.YuanQuId ?? throw new ArgumentNullException("loginModel.YuanquID");
                var wherelambda = ExtLinq.True<C_TB_HS_TALLYBILL>();
                wherelambda = wherelambda.And(t => t.YuanQuID == yuanquid);
                if (startTime != null)
                {
                    wherelambda = wherelambda.And(t => t.SIGNDATE >= startTime);
                }
                if (endTime != null)
                {
                    wherelambda = wherelambda.And(t => t.SIGNDATE <= endTime);
                }
                wherelambda = wherelambda.And(t => t.CODE_OPSTYPE == "进库");
                List<KehuFenxiEntry> listentry = new List<KehuFenxiEntry>();

                Array goodsNameArray = db.C_TB_HS_TALLYBILL.Select(n => n.GoodsName).Distinct().ToArray();
                foreach (var g in goodsNameArray)
                {
                    KehuFenxiEntry entry = new KehuFenxiEntry();
                    entry.name = g.ToString();
                    decimal? ywushuliang = 0;
                    List<C_TB_HS_TALLYBILL> liHuoDanlist = db.C_TB_HS_TALLYBILL.Where(wherelambda).Where(n => n.GoodsName == g).ToList();
                    foreach (var l in liHuoDanlist)
                    {
                        ywushuliang += l.AMOUNT + l.WEIGHT;
                    }

                    entry.zuoyeliang = ywushuliang;
                    listentry.Add(entry);
                }
                ac = new Api_kucunModel()
                {
                    code = "0",
                    msg = "请求成功",
                    data = new Api_KuCunModelData()
                    {
                        list = listentry
                    }


                };
                return ToJson(ac);
            }
            catch (Exception e)
            {
                Log4NetHelper log = new Log4NetHelper();
                log.Error(e.Message, e);
                ac = new Api_kucunModel()
                {
                    code = "1",
                    msg = "请求失败",
                };
                return ToJson(ac);
                throw;
            }
        }
        #endregion

        #region  // 测试
        [Route("api/test")]
        [System.Web.Http.HttpGet]
        public object apitest([FromBody]dynamic obj)
        {
            var str = "1";
            //return str;

            var obj1 = Request.Headers.Authorization;
            if (obj1.ToString() == "123")
            {
                return "1234";
            }
            //////var obj2 = HttpRequest;
            HttpRequestMessage re = new HttpRequestMessage();
            var headers = re.Headers;

            if (headers.Contains("token"))
            {
                string token = headers.GetValues("token").First();
            }
            return 'a';
        }
        #endregion

        #region  //验证密码
        [Route("api/checkPwd")]
        [System.Web.Http.HttpPost]
        public object CheckPwd([FromBody]dynamic obj)
        {
            string userName = obj.userName ?? "";
            string passWord = obj.passWord ?? ""  ;
            passWord = EncryptHelper.AESEncrypt(passWord);
            Sys_User m = db.Sys_User.FirstOrDefault(n => n.userName == userName && n.passWord == passWord && n.state == "激活");
            API_login user = new API_login();
            if (m != null)
            {
                user = new API_login()
                {
                    code = "0",
                    msg = "登录成功",
                };

            }
            else
            {
                user = new API_login()
                {
                    code = "1",
                    msg = "用户名密码错误",
                };
            }
            return ToJson(user);
        }
        #endregion

        #region  //待办
        [Route("api/daiban/list")]
        [System.Web.Http.HttpPost]
        public object daiban([FromBody]dynamic obj)
        {
            int userId = obj.userId;
            Sys_User user = db.Sys_User.Find(userId);
            decimal? roleId = user.roleId;
            decimal? YuanquID = user.YuanQuId;
            List<C_TB_HC_CONTRACT> htList = new List<C_TB_HC_CONTRACT>();
            List<C_TB_HC_GOODSBILL> phList = new List<C_TB_HC_GOODSBILL>();
            List<C_TB_HC_CONSIGN> wtList = new List<C_TB_HC_CONSIGN>();

            if (roleId == 7)
            {
                htList = db.Set<C_TB_HC_CONTRACT>().Where(n => n.State == "待经理审核" && n.YuanQuID == YuanquID).ToList();
                phList = db.Set<C_TB_HC_GOODSBILL>().Where(n => n.State == "待经理审核" && n.YuanQuID == YuanquID).ToList();
                wtList = db.Set<C_TB_HC_CONSIGN>().Where(n => n.State == "待经理审核" && n.YuanQuID == YuanquID).ToList();
            }
            if (roleId == 3)
            {
                wtList = db.Set<C_TB_HC_CONSIGN>().Where(n => n.State == "待审核" && n.YuanQuID == YuanquID).ToList();
            }
            Api_Daiban api = new Api_Daiban()
            {
                code = "0",
                msg = "登录成功",
                data = new Api_DaibanData()
                {
                    htList = htList,
                    phList = phList,
                    wtList = wtList
                }
            };
            return ToJson(api);
        }
        #endregion

        #region  //图表 开票同比图    ToOutController GetKaiPiaoTongBi()
        [Route("api/chart/kptbt")]
        [System.Web.Http.HttpPost]
        public object Charkptbt([FromBody]dynamic obj)
        {
            int userId = obj.userId;
            Api_kucunModel ac = null;
            Sys_User loginModel = db.Sys_User.FirstOrDefault(n => n.ID == userId);
            DateTime starTime = DateTime.Now.AddMonths(1 - DateTime.Now.Month).AddDays(1 - DateTime.Now.Day).Date;//今年的第一月的第一天
            DateTime endTime = DateTime.Now.AddMonths(1 - DateTime.Now.Month).AddDays(1 - DateTime.Now.Day).Date.AddMonths(1).AddSeconds(-1);//今年的第一月的最后一天
            List<HuanBiEntry> listentry = new List<HuanBiEntry>();
            Array GoodsBillIDArray = db.C_TB_HC_GOODSBILL.Where(n => n.YuanQuID == loginModel.YuanQuId).Select(n => n.ID).ToArray();
            List<decimal?> goodsagentid = new List<decimal?>();
            foreach (var items in GoodsBillIDArray)
            {
                goodsagentid.Add(items.ToDecimal());
            }//获取该园区所有的GoodsBillId

            List<C_TB_JIESUAN> JIESUAN_List = db.C_TB_JIESUAN.Where(n => goodsagentid.Contains(n.GoodsBill_id)).ToList();//获取该园区所有的结算信息
            try
            {

                for (int i = 0; i < 12; i++)
                {
                    HuanBiEntry entry = new HuanBiEntry();
                    entry.date = i + 1 + "月";
                    entry.NowYear = JIESUAN_List.Where(n => n.KaiPiaoRiQi >= starTime.AddMonths(i) && n.KaiPiaoRiQi <= endTime.AddMonths(i)).Sum(n => n.KaiPiaoJinE);
                    entry.LastYear = JIESUAN_List.Where(n => n.KaiPiaoRiQi >= starTime.AddYears(-1).AddMonths(i) && n.KaiPiaoRiQi <= endTime.AddYears(-1).AddMonths(i)).Sum(n => n.KaiPiaoJinE);
                    entry.TongBi = ((JIESUAN_List.Where(n => n.KaiPiaoRiQi >= starTime.AddYears(-1).AddMonths(i) && n.KaiPiaoRiQi <= endTime.AddYears(-1).AddMonths(i)).Sum(n => n.KaiPiaoJinE) - JIESUAN_List.Where(n => n.KaiPiaoRiQi >= starTime.AddMonths(i) && n.KaiPiaoRiQi <= endTime.AddMonths(i)).Sum(n => n.KaiPiaoJinE)) / 100).ToDecimal(2);
                    listentry.Add(entry);
                }
                ac = new Api_kucunModel()
                {
                    code = "0",
                    msg = "请求成功",
                    data = new Api_KuCunModelData()
                    {
                        list = listentry
                    }


                };
                return ToJson(ac);
            }
            catch (Exception e)
            {
                Log4NetHelper log = new Log4NetHelper();
                log.Error(e.Message, e);
                ac = new Api_kucunModel()
                {
                    code = "1",
                    msg = "请求失败",
                };
                return ToJson(ac);
                throw;
            }
        }
        #endregion

        #region  //图表 作业量同比图    ToOutController GetKaiPiaoTongBi()
        [Route("api/chart/zyltbt")]
        [System.Web.Http.HttpPost]
        public object Charzyltbt([FromBody]dynamic obj)
        {
            int userId = obj.userId;
            Api_kucunModel ac = null;
            Sys_User loginModel = db.Sys_User.FirstOrDefault(n => n.ID == userId);
            DateTime starTime = DateTime.Now.AddMonths(1 - DateTime.Now.Month).AddDays(1 - DateTime.Now.Day).Date;//今年的第一月的第一天
            DateTime endTime = DateTime.Now.AddMonths(1 - DateTime.Now.Month).AddDays(1 - DateTime.Now.Day).Date.AddMonths(1).AddSeconds(-1);//今年的第一月的最后一天
            List<HuanBiEntry> listentry = new List<HuanBiEntry>();
            Array GoodsBillIDArray = db.C_TB_HC_GOODSBILL.Where(n => n.YuanQuID == loginModel.YuanQuId).Select(n => n.ID).ToArray();
            List<C_TB_HS_TALLYBILL> List_JiSuan = new List<C_TB_HS_TALLYBILL>();
            List<decimal?> goodsagentid = new List<decimal?>();
            foreach (var items in GoodsBillIDArray)
            {
                decimal id = items.ToDecimal();
                List<C_TB_HC_CONSIGN> weituoList = db.C_TB_HC_CONSIGN.Where(n => n.GOODSBILL_ID == id && n.CODE_OPERATION == "汽-场").ToList();
                foreach (var w in weituoList)
                {
                    List<C_TB_HS_TALLYBILL> LihuoDanList2 = db.C_TB_HS_TALLYBILL.Where(n => n.CONSIGN_ID == w.ID && n.CODE_OPSTYPE == "进库").ToList();
                    foreach (var Tall in LihuoDanList2)
                    {
                        List_JiSuan.Add(Tall);
                    }
                }

                goodsagentid.Add(items.ToDecimal());
            }//获取该园区所有的GoodsBillId

            List<C_TB_JIESUAN> JIESUAN_List = db.C_TB_JIESUAN.Where(n => goodsagentid.Contains(n.GoodsBill_id)).ToList();//获取该园区所有的结算信息
            try
            {

                for (int i = 0; i < 12; i++)
                {
                    HuanBiEntry entry = new HuanBiEntry();
                    entry.date = i + 1 + "月";
                    entry.NowYear = List_JiSuan.Where(n => n.SIGNDATE >= starTime.AddMonths(i) && n.SIGNDATE <= endTime.AddMonths(i)).Sum(n => n.WEIGHT);
                    entry.LastYear = List_JiSuan.Where(n => n.SIGNDATE >= starTime.AddYears(-1).AddMonths(i) && n.SIGNDATE <= endTime.AddYears(-1).AddMonths(i)).Sum(n => n.WEIGHT);
                    entry.TongBi = ((List_JiSuan.Where(n => n.SIGNDATE >= starTime.AddYears(-1).AddMonths(i) && n.SIGNDATE <= endTime.AddYears(-1).AddMonths(i)).Sum(n => n.WEIGHT) - List_JiSuan.Where(n => n.SIGNDATE >= starTime.AddMonths(i) && n.SIGNDATE <= endTime.AddMonths(i)).Sum(n => n.WEIGHT)) / 100).ToDecimal(2);
                    listentry.Add(entry);
                }
                ac = new Api_kucunModel()
                {
                    code = "0",
                    msg = "请求成功",
                    data = new Api_KuCunModelData()
                    {
                        list = listentry
                    }


                };
                return ToJson(ac);
            }
            catch (Exception e)
            {
                Log4NetHelper log = new Log4NetHelper();
                log.Error(e.Message, e);
                ac = new Api_kucunModel()
                {
                    code = "1",
                    msg = "请求失败",
                };
                return ToJson(ac);
                throw;
            }
            
        }
        #endregion





        // 保税相关功能
        #region  //保税业务委托列表
        [Route("api/bs/yewuweituoManage/list")]
        [System.Web.Http.HttpPost]
        public object bsyewuweituoManage([FromBody]dynamic obj)
        {
            int userId = obj.userId;

            int limit = obj.limit;
            int offset = (obj.page - 1) * limit;
            string GBNO = obj.GBNO ?? "";
            string C_GOODSAGENT_NAME = obj.C_GOODSAGENT_NAME;
            string CreatTime = obj.CreatTime;
            string CreatTime1 = obj.CreatTime1;
            string BLNO = obj.BLNO;
            string C_GOODS = obj.C_GOODS;
            string HuoZhu = obj.HuoZhu;
            string ShipName = obj.ShipName;
            string VGNO = obj.VGNO;

            Sys_User loginModel = db.Sys_User.FirstOrDefault(n => n.ID == userId && n.state == "激活");

            var wherelambda = ExtLinq.True<C_TB_HC_GOODSBILL>();
            wherelambda = wherelambda.And(t => t.YuanQuID == loginModel.YuanQuId);
            if (!string.IsNullOrEmpty(GBNO))
            {
                wherelambda = wherelambda.And(t => t.GBNO.Contains(GBNO));
            }
            if (!string.IsNullOrEmpty(C_GOODSAGENT_NAME))
            {
                wherelambda = wherelambda.And(t => t.C_GOODSAGENT_NAME.Contains(C_GOODSAGENT_NAME));
            }
            if (!string.IsNullOrEmpty(CreatTime))
            {
                DateTime CreatTime_date = Convert.ToDateTime(CreatTime + " 00:00:00");
                wherelambda = wherelambda.And(n => n.CreatTime >= CreatTime_date);

            }
            if (!string.IsNullOrEmpty(CreatTime1))
            {
                DateTime CreatTime1_date = Convert.ToDateTime(CreatTime1 + " 23:59:59");
                wherelambda = wherelambda.And(n => n.CreatTime <= CreatTime1_date);

            }
            if (!string.IsNullOrEmpty(BLNO))
            {
                wherelambda = wherelambda.And(t => t.BLNO.Contains(BLNO));
            }
            if (!string.IsNullOrEmpty(C_GOODS))
            {
                wherelambda = wherelambda.And(t => t.C_GOODS.Contains(C_GOODS));
            }
            if (!string.IsNullOrEmpty(HuoZhu))
            {
                wherelambda = wherelambda.And(t => t.HuoZhu.Contains(HuoZhu));
            }
            if (!string.IsNullOrEmpty(ShipName))
            {
                wherelambda = wherelambda.And(t => t.ShipName.Contains(ShipName));
            }
            if (!string.IsNullOrEmpty(VGNO))
            {
                wherelambda = wherelambda.And(t => t.VGNO.Contains(VGNO));
            }
            int count = db.Set<C_TB_HC_GOODSBILL>().Where(wherelambda).OrderByDescending(n => n.CreatTime).Count();
            var list = db.Set<C_TB_HC_GOODSBILL>().Where(wherelambda).OrderByDescending(n => n.CreatTime).Skip(offset).Take(limit).AsQueryable();

            //foreach (var itsms_GoodsBill in list)//计算库存
            //{
            //    C_TB_HC_CONTRACT Model_CONTRACT = db.C_TB_HC_CONTRACT.FirstOrDefault(n => n.Guid == itsms_GoodsBill.CONTRACT_Guid);//查找合同
            //    decimal? KuCun = 0;
            //    decimal? KuCunW = 0;
            //    List<C_TB_HC_CONSIGN> list_CONSIGN = db.C_TB_HC_CONSIGN.Where(n => n.GOODSBILL_ID == itsms_GoodsBill.ID).OrderBy(n => n.ID).ToList();
            //    foreach (var items_Consign in list_CONSIGN)
            //    {
            //        List<C_TB_HS_TALLYBILL> list_TALLYBILL = db.C_TB_HS_TALLYBILL.Where(n => n.CONSIGN_ID == items_Consign.ID).OrderBy(n => n.ID).ToList();
            //        foreach (var items_TALLYBILL in list_TALLYBILL)
            //        {
            //            if (items_TALLYBILL.CODE_OPSTYPE == "进库")
            //            {
            //                KuCun += items_TALLYBILL.AMOUNT;
            //                KuCunW += items_TALLYBILL.WEIGHT;
            //            }
            //            if (items_TALLYBILL.CODE_OPSTYPE == "出库")
            //            {
            //                KuCun -= items_TALLYBILL.AMOUNT;
            //                KuCunW -= items_TALLYBILL.WEIGHT;
            //            }
            //        }

            //    }
            //    decimal? ShiJiJInKu = 0;
            //    decimal? ShiJiChuKu = 0;
            //    GetTallyBllList_fy_hj(Convert.ToInt32(itsms_GoodsBill.ID), out decimal? Fyjehj, out decimal? Fyjehj_Sh, out decimal? Kpjehj_Sh);
            //    itsms_GoodsBill.Fyjehj = Fyjehj.ToDecimal(2);
            //    itsms_GoodsBill.KunCun = KuCun.ToString();
            //    itsms_GoodsBill.KunCunW = KuCunW.ToDecimal(3).ToString();
            //    List<C_TB_JIESUAN> list_JieSuan = db.C_TB_JIESUAN.Where(n => n.GoodsBill_id == itsms_GoodsBill.ID).OrderBy(n => n.ID).ToList();
            //    itsms_GoodsBill.Lkjehj = list_JieSuan.Sum(n => n.LaiKuanJinE).ToDecimal(2);
            //    itsms_GoodsBill.Sryk = (Fyjehj - list_JieSuan.Sum(n => n.LaiKuanJinE)).ToDecimal(2);
            //    List<C_TB_CHENGBEN> list_ChengBen = db.C_TB_CHENGBEN.Where(n => n.GoodsBill_id == itsms_GoodsBill.ID).OrderBy(n => n.ID).ToList();
            //    itsms_GoodsBill.ChengBenJinE = list_ChengBen.Sum(n => n.ChengBenJinE).ToDecimal(2);
            //    itsms_GoodsBill.LiRunZongE = (Fyjehj - list_ChengBen.Sum(n => n.ChengBenJinE)).ToDecimal(2);
            //    if (list_JieSuan.Sum(n => n.KaiPiaoJinE) == 0)
            //    {
            //        itsms_GoodsBill.HuiShouLv = 0;
            //    }
            //    else
            //    {
            //        itsms_GoodsBill.HuiShouLv = (list_JieSuan.Sum(n => n.LaiKuanJinE) / list_JieSuan.Sum(n => n.KaiPiaoJinE)).ToDecimal(2);

            //    }
            //    List<C_TB_HC_CONSIGN> list_KuCunCONSIGN = db.C_TB_HC_CONSIGN.Where(n => n.GOODSBILL_ID == itsms_GoodsBill.ID).OrderBy(n => n.ID).ToList();
            //    foreach (var items_KuCunCONSIGN in list_KuCunCONSIGN)
            //    {
            //        List<C_TB_HS_TALLYBILL> list_KuCunTALLYBILL = db.C_TB_HS_TALLYBILL.Where(n => n.CONSIGN_ID == items_KuCunCONSIGN.ID).OrderBy(n => n.ID).ToList();
            //        ShiJiJInKu += list_KuCunTALLYBILL.Where(n => n.CODE_OPSTYPE == "进库" && n.Type != "清场").Sum(n => n.WEIGHT);
            //        ShiJiChuKu += list_KuCunTALLYBILL.Where(n => n.CODE_OPSTYPE == "出库" && n.Type != "清场").Sum(n => n.WEIGHT);
            //    }
            //    itsms_GoodsBill.ShiJiJInKu = ShiJiJInKu.ToDecimal(3);
            //    itsms_GoodsBill.ShiJiChuKu = ShiJiChuKu.ToDecimal(3);
            //    itsms_GoodsBill.KaiPiaoJineHJ = list_JieSuan.Sum(n => n.KaiPiaoJinE).ToDecimal(2);
            //    itsms_GoodsBill.KaiPiaoShuiHouHJ = Kpjehj_Sh.ToDecimal(2);
            //    itsms_GoodsBill.WeiKaiPiaoJineHJ = (Fyjehj - list_JieSuan.Sum(n => n.KaiPiaoJinE)).ToDecimal(2);
            //    itsms_GoodsBill.WeiKaiPiaoShuiHouHJ = (Fyjehj_Sh - Kpjehj_Sh).ToDecimal(2);
            //    itsms_GoodsBill.YinShouZhangKuanYue = (list_JieSuan.Sum(n => n.LaiKuanJinE) - list_JieSuan.Sum(n => n.KaiPiaoJinE)).ToDecimal(2);
            //    itsms_GoodsBill.LiRunZongE = (Fyjehj - list_ChengBen.Sum(n => n.ChengBenJinE)).ToDecimal(2);
            //    itsms_GoodsBill.YikaiPiaoYinShouYuE = (list_JieSuan.Sum(n => n.KaiPiaoJinE) - list_JieSuan.Sum(n => n.LaiKuanJinE)).ToDecimal(2);
            //    itsms_GoodsBill.WeiKaiPiaoYingShouYuE = ((Fyjehj - list_JieSuan.Sum(n => n.KaiPiaoJinE)) - list_JieSuan.Sum(n => n.YuShouJinE)).ToDecimal(2);
            //}

            int total = count;
            object rows = list;
            Api_kucunModel ac = null;
            ac = new Api_kucunModel()
            {
                code = "0",
                msg = "请求成功",
                data = new Api_KuCunModelData()
                {
                    totalCount = total,
                    list = rows,
                }


            };
            return ToJson(ac);








        }
        #endregion

        #region  // 首页图标
        [Route("api/home/module")]
        [System.Web.Http.HttpPost]

        public object homeModule([FromBody]dynamic obj)
        {
            int userId = obj.userId;
            Sys_User user = db.Sys_User.FirstOrDefault(n => n.ID == userId);
            int roleId = user.roleId.ToInt();
            var sec = new Api_homeModuleSecList()
            {
                title = "常用功能"
            };
            var item = new Api_homeModuleDataList()
            {
                id = 1,
                imgUrl = "../../static/img/Kucun@3x.png",
                text = "票货管理",
                
            };
            List<Api_homeModuleSecList> secs = new List<Api_homeModuleSecList>();
            
            List<Api_homeModuleDataList> items = new List<Api_homeModuleDataList>();
            items.Add(item);
            sec.item = items;
            secs.Add(sec);
            Api_homeModule model = new Api_homeModule()
            {
                code = "0",
                msg = "",
                data = new Api_homeModuleData()
                {
                    list = secs
                }
            };
            return ToJson(model);
        }
        #endregion

        #region  // 新增保税业务委托
        [Route("api/bs/yewuweituo/add")]
        [System.Web.Http.HttpPost]
        public object yewuweituoAdd([FromBody]dynamic obj)
        {
            int userId = Convert.ToInt32(obj.userId);
            Sys_User loginModel = db.Sys_User.FirstOrDefault(n => n.ID == userId && n.state == "激活");
            string GBNO = obj.GBNO;
            string hetonghao = obj.hetonghao;
            string baoshuileixing = obj.baoshuileixing;
            string huoming = obj.huoming;
            string huozhu = obj.huozhu;
            string chuanming = obj.chuanming;
            string yingwenchuanming = obj.yingwenchuanming;
            string hangci = obj.hangci;
            string tidanhao = obj.tidanhao;
            string tidanshu = obj.tidanshu;
            string jingzhong = obj.jingzhong;
            string jiedanriqi = obj.jiedanriqi;
            string jihuadaogangriqi = obj.jihuadaogangriqi;
            string danjia = obj.danjia;
            string huozhi = obj.huozhi;
            string yuanchandi = obj.yuanchandi;
            string liaohao = obj.liaohao;
            string xiangshu = obj.xiangshu;
            string xiangxing = obj.xiangxing;
            string danzheng = obj.danzheng;
            string yewuleixing = obj.yewuleixing;
            string beizhu = obj.beizhu;

            C_TB_HC_GOODSBILL model;
            if (!string.IsNullOrEmpty(GBNO))
            {
                model = db.C_TB_HC_GOODSBILL.FirstOrDefault(n => n.GBNO == GBNO);
            }
            else
            {
                model = new C_TB_HC_GOODSBILL();
            }

            model.ContoractNumber = hetonghao;
            model.BaoShuiLeiXing = baoshuileixing;
            model.C_GOODS = huoming;
            model.HuoZhu = huozhu;
            model.ShipName = chuanming;
            model.YingWenChuanMing = yingwenchuanming;
            model.VGNO = hangci;
            model.BLNO = tidanhao;
            model.PLANWEIGHT = StrToDecimal(tidanshu);
            model.jccj = jingzhong;
            model.JieDanRiQi = obj.jiedanriqi;
            model.JiHuaDaoGangRiQi = obj.jihuadaogangriqi;
            model.DanJia = danjia;
            model.HuoZhi = StrToDecimal(huozhi);
            model.YuanChanDi = yuanchandi;
            model.LiaoHao = liaohao;
            model.XiangShu = xiangshu;
            model.XiangXing = xiangxing;
            model.BoolDanZheng = danzheng;
            model.YeWuLeiXing = yewuleixing;
            model.MARK = beizhu;
            model.SysUserID = userId;


            if (string.IsNullOrEmpty(model.GBNO))           //新建时生成流水号
            {
                string TodayTime = DateTime.Today.ToString("yyyyMMdd");
                C_TB_HC_GOODSBILL Num = db.C_TB_HC_GOODSBILL.OrderByDescending(n => n.CreatTime).ToList().FirstOrDefault();
                if (Num != null)
                {
                    if (!string.IsNullOrEmpty(Num.GBNO))
                    {
                        if (Num.GBNO.Substring(2, 8) == TodayTime)
                        {
                            model.GBNO = "PH" + TodayTime + (Convert.ToInt32(Num.GBNO.Replace("PH" + TodayTime, "")) + 1).ToString("0000");
                        }
                        else
                        {
                            model.GBNO = "PH" + TodayTime + "0001";
                        }
                    }
                }

                else
                {
                    model.GBNO = "PH" + TodayTime + "0001";
                }

            }
            var yuanquid = loginModel.YuanQuId ?? throw new ArgumentNullException("loginModel.YuanquID");
            model.YuanQuID = yuanquid;
            model.CreatPeople = loginModel.userName;
            model.SysUserID = loginModel.ID.ToInt();
            if (model.ID == 0)
            {
                model.CreatTime = DateTime.Now;
                model.State = MainBLL.GoodBill.GoodBillStateEnum.待提交审核.ToString();
                model.KunCun = "0";
                model.KunCunW = "0";
            }

            Api_common save = new Api_common();

            if (!string.IsNullOrEmpty(model.ContoractNumber))
            {
                C_TB_HC_CONTRACT model_ht = db.C_TB_HC_CONTRACT.FirstOrDefault(n => n.ContoractNumber == model.ContoractNumber);
                if (model_ht == null)
                {

                    save.code = "1";
                    save.msg = "找不到对应的合同号";
                    return ToJson(save);
                }
                else
                {
                    model.CONTRACT_Guid = model_ht.Guid;
                }
            }
            db.Set<C_TB_HC_GOODSBILL>().AddOrUpdate(model);
            try
            {


                db.SaveChanges();
                save.code = "0";
                save.msg = "保存成功";
                return ToJson(save);
            }
            catch (Exception ex)
            {
                save.code = "1";
                save.msg = "数据有误";
                return ToJson(save);
            }
        }
        #endregion

        #region  // 保税业务委托删除
        //[Route("api/bs/yewuweituo/del")]
        //[System.Web.Http.HttpPost]
        //public object YewuWeituoDel([FromBody]dynamic obj)
        //{
        //    var item = new Api_homeModuleDataList()
        //    {
        //        id = 1,
        //        imgUrl = "../../static/img/Kucun@3x.png",
        //        text = "票货管理"
        //    };
        //    List<Api_homeModuleDataList> mList = new List<Api_homeModuleDataList>();
        //    mList.Add(item);
        //    Api_homeModule model = new Api_homeModule()
        //    {
        //        code = "0",
        //        msg = "",
        //        data = new Api_homeModuleData()
        //        {
        //            list = mList
        //        }
        //    };
        //    return ToJson(model);
        //}
        #endregion

        #region  // 报关列表
        [Route("api/bs/baoguan/list")]
        [System.Web.Http.HttpPost]
        public object BaoguanList([FromBody]dynamic obj)
        {
            int id = obj.id;
            C_TB_HC_GOODSBILL model_goodsbill = db.C_TB_HC_GOODSBILL.Find(id) ?? new C_TB_HC_GOODSBILL();

            List<BS_BAOGUAN> bgList = db.BS_BAOGUAN.Where(n => n.GoodsBillId == id).ToList();
            Api_kucunModel ac = null;
            ac = new Api_kucunModel()
            {
                code = "0",
                msg = "请求成功",
                data = new Api_KuCunModelData()
                {
                    list = bgList,
                }
            };
            return ToJson(ac);
            ;
        }
        #endregion

        #region 报关申请人
        [Route("api/dic/bs/shenqingren")]
        [System.Web.Http.HttpPost]
        public object dicBsShenqiren([FromBody]dynamic obj)
        {
            int userId = Convert.ToInt32(obj.userId);
            Sys_User loginModel = db.Sys_User.FirstOrDefault(n => n.ID == userId && n.state == "激活");
            List<C_TB_CODE_CUSTOMER> mlist = db.C_TB_CODE_CUSTOMER.Where(n => n.YuanQuID == loginModel.YuanQuId).OrderBy(n => n.ID).ToList();//委托人,收货人
            Api_DicKV_Model a = null;
            if (mlist != null)
            {
                a = new Api_DicKV_Model()
                {
                    code = "0",
                    msg = "请求成功",
                    data = new Api_DicKV_ModelList { }
                };

                foreach (var item in mlist)
                {
                    a.data.list.Add(new Api_DicKV_ModelListKV()
                    {
                        value = item.ID.ToInt(),
                        text = item.Name
                    });
                }
            }
            else
            {
                a = new Api_DicKV_Model()
                {
                    code = "1",
                    msg = "用户id请求错误",
                };
            }
            return ToJson(a);
        }
        #endregion

        #region 报关添加
        [Route("api/bs/baoguan/add")]
        [System.Web.Http.HttpPost]
        public object BaoguanAdd([FromBody]dynamic obj)
        {
            int userId = Convert.ToInt32(obj.userId);
            Sys_User loginModel = db.Sys_User.FirstOrDefault(n => n.ID == userId && n.state == "激活");

            BS_BAOGUAN model;
            if (!string.IsNullOrEmpty((obj.Guid).ToString()))
            {
                string Guid = obj.Guid;
                model = db.BS_BAOGUAN.FirstOrDefault(n => n.Guid == Guid);
            }
            else
            {
                model = new BS_BAOGUAN();
                model.Guid = Guid.NewGuid().ToString();

                model.CreatPeople = loginModel.userName;
                model.CreatTime = DateTime.Now;
                model.GoodsBillId = obj.GoodsBillId;
            }

            
            model.BaoGuanShenQingRen = obj.BaoGuanShenQingRen;
            model.BaoGuanLeiBie = obj.BaoGuanLeiBie;
            model.LiuXiang = obj.LiuXiang;
            model.ChuanGongSi = obj.ChuanGongSi;
            model.ShenPiKaiShiRiQi = obj.ShenPiKaiShiRiQi;
            model.ShenPiJieShuRiQi = obj.ShenPiJieShuRiQi;
            model.MianShiQi = obj.MianShiQi;
            model.MaoZhong = obj.MaoZhong;
            model.JingZhong = obj.JingZhong;
            model.DanJia = obj.DanJia;
            model.JianShu = obj.JianShu;
            model.HuoZhi = obj.HuoZhi;
            model.ZhongXinZuoYeDanHao = obj.ZhongXinZuoYeDanHao;
            model.BaoGuanMiaoShu = obj.BaoGuanMiaoShu;


            Api_common save = new Api_common();
            db.Set<BS_BAOGUAN>().AddOrUpdate(model);
            try
            {


                db.SaveChanges();
                save.code = "0";
                save.msg = "保存成功";
                return ToJson(save);
            }
            catch (Exception ex)
            {
                save.code = "1";
                save.msg = "数据有误";
                return ToJson(save);
            }
        }
        #endregion

        #region  //报关 删除
        [Route("api/bs/baoguan/delete")]
        [System.Web.Http.HttpPost]
        public object bsBaoguanDelete([FromBody]dynamic obj)
        {
            string guid = obj.guid;
            Api_common save = new Api_common();

            BS_BAOGUAN model = db.BS_BAOGUAN.FirstOrDefault(n => n.Guid == guid);
            db.BS_BAOGUAN.Remove(model);
            int c = db.SaveChanges();
            if (c > 0)
            {
                save.code = "0";
                save.msg = "提交成功";
                return ToJson(save);
            }
            else
            {
                if (c == 0)
                {
                    save.code = "1";
                    save.msg = "无更新";
                    return ToJson(save);
                }
                save.code = "2";
                save.msg = "失败";
                return ToJson(save);
            }
        }
        #endregion

        #region 报检添加
        [Route("api/bs/baojian/add")]
        [System.Web.Http.HttpPost]
        public object BaojianAdd([FromBody]dynamic obj)
        {
            int userId = Convert.ToInt32(obj.userId);
            Sys_User loginModel = db.Sys_User.FirstOrDefault(n => n.ID == userId && n.state == "激活");

            BS_BAOJIAN model;
            if (!string.IsNullOrEmpty((obj.Guid).ToString()))
            {
                string Guid = obj.Guid;
                model = db.BS_BAOJIAN.FirstOrDefault(n => n.Guid == Guid);
            }
            else
            {
                model = new BS_BAOJIAN();
                model.Guid = Guid.NewGuid().ToString();

                model.CreatPeople = loginModel.userName;
                model.CreatTime = DateTime.Now;
                model.GoodsBillId = obj.GoodsBillId;
            }


            model.BaoJianShenQingRen = obj.BaoJianShenQingRen;
            model.BaoJianLeiBie = obj.BaoJianLeiBie;
            model.BaoJianRiQi = obj.BaoJianRiQi;
            model.BaoJianMiaoShu = obj.BaoJianMiaoShu;


            Api_common save = new Api_common();
            db.Set<BS_BAOJIAN>().AddOrUpdate(model);
            try
            {


                db.SaveChanges();
                save.code = "0";
                save.msg = "保存成功";
                return ToJson(save);
            }
            catch (Exception ex)
            {
                save.code = "1";
                save.msg = "数据有误";
                return ToJson(save);
            }
        }
        #endregion

        #region  // 报检列表
        [Route("api/bs/baojian/list")]
        [System.Web.Http.HttpPost]
        public object BaojianList([FromBody]dynamic obj)
        {
            int id = obj.id;
            C_TB_HC_GOODSBILL model_goodsbill = db.C_TB_HC_GOODSBILL.Find(id) ?? new C_TB_HC_GOODSBILL();

            List<BS_BAOJIAN> bgList = db.BS_BAOJIAN.Where(n => n.GoodsBillId == id).ToList();
            Api_kucunModel ac = null;
            ac = new Api_kucunModel()
            {
                code = "0",
                msg = "请求成功",
                data = new Api_KuCunModelData()
                {
                    list = bgList,
                }
            };
            return ToJson(ac);
            ;
        }
        #endregion

        #region  //报检 删除
        [Route("api/bs/baojian/delete")]
        [System.Web.Http.HttpPost]
        public object bsBaojianDelete([FromBody]dynamic obj)
        {
            string guid = obj.guid;
            Api_common save = new Api_common();

            BS_BAOJIAN model = db.BS_BAOJIAN.FirstOrDefault(n => n.Guid == guid);
            db.BS_BAOJIAN.Remove(model);
            int c = db.SaveChanges();
            if (c > 0)
            {
                save.code = "0";
                save.msg = "提交成功";
                return ToJson(save);
            }
            else
            {
                if (c == 0)
                {
                    save.code = "1";
                    save.msg = "无更新";
                    return ToJson(save);
                }
                save.code = "2";
                save.msg = "失败";
                return ToJson(save);
            }
        }
        #endregion

        #region  // 仓单列表
        [Route("api/bs/cangdan/list")]
        [System.Web.Http.HttpPost]
        public object CangdanList([FromBody]dynamic obj)
        {
            int id = obj.id;
            C_TB_HC_GOODSBILL model_goodsbill = db.C_TB_HC_GOODSBILL.Find(id) ?? new C_TB_HC_GOODSBILL();

            List<BS_CANGDAN> bgList = db.BS_CANGDAN.Where(n => n.GoodsBillId == id).ToList();
            Api_kucunModel ac = null;
            ac = new Api_kucunModel()
            {
                code = "0",
                msg = "请求成功",
                data = new Api_KuCunModelData()
                {
                    list = bgList,
                }
            };
            return ToJson(ac);
            ;
        }
        #endregion

        #region 仓单添加
        [Route("api/bs/cangdan/add")]
        [System.Web.Http.HttpPost]
        public object CangdanAdd([FromBody]dynamic obj)
        {
            int userId = Convert.ToInt32(obj.userId);
            Sys_User loginModel = db.Sys_User.FirstOrDefault(n => n.ID == userId && n.state == "激活");

            BS_CANGDAN model;
            if (!string.IsNullOrEmpty((obj.Guid).ToString()))
            {
                string Guid = obj.Guid;
                model = db.BS_CANGDAN.FirstOrDefault(n => n.Guid == Guid);
            }
            else
            {
                model = new BS_CANGDAN();
                model.Guid = Guid.NewGuid().ToString();

                model.CreatPeople = loginModel.userName;
                model.CreatTime = DateTime.Now;
                model.GoodsBillId = obj.GoodsBillId;
                C_TB_HC_GOODSBILL goodsBill = db.C_TB_HC_GOODSBILL.FirstOrDefault(n => n.ID == model.GoodsBillId);
                model.TiDanHao = goodsBill.BLNO;
                model.YingWenChuanMing = goodsBill.YingWenChuanMing;
                //model.HuiShouRen = goodsBill.YingWenChuanMing;
                //model.HuiShouRiQi = goodsBill.YingWenChuanMing;
                //model.BoolHuiShou = goodsBill.YingWenChuanMing;

            }


            model.TaiTouRen = obj.TaiTouRen;
            model.MaiTou = obj.MaiTou;
            model.PingMing = obj.PingMing;
            model.MaoZhong = obj.MaoZhong;
            model.MaoZhongDanWei = obj.MaoZhongDanWei;
            model.JingZhong = obj.JingZhong;
            model.JingZhongDanWei = obj.JingZhongDanWei;
            model.JianShu = obj.JianShu;
            model.JianShuDanWei = obj.JianShuDanWei;
            model.CangChuRiQi = obj.CangChuRiQi;
            model.QianFaRiQi = obj.QianFaRiQi;
            model.CangDanBeiZhu = obj.CangDanBeiZhu;
            model.BeiZhu = obj.BeiZhu;

            Api_common save = new Api_common();

            try
            {
                if (string.IsNullOrEmpty(model.CangDanHao))           //新建时生成流水号
                {
                    string TodayTime = DateTime.Today.ToString("yyyyMMdd");
                    BS_CANGDAN Num = db.BS_CANGDAN.OrderByDescending(n => n.CreatTime).ToList().FirstOrDefault();
                    if (Num != null)
                    {
                        if (!string.IsNullOrEmpty(Num.CangDanHao))
                        {
                            if (Num.CangDanHao.Substring(2, 8) == TodayTime)
                            {
                                model.CangDanHao = "YGBS" + TodayTime + (Convert.ToInt32(Num.CangDanHao.Replace("YGBS" + TodayTime, "")) + 1).ToString("0000");
                            }
                            else
                            {
                                model.CangDanHao = "YGBS" + TodayTime + "0001";
                            }
                        }
                    }

                    else
                    {
                        model.CangDanHao = "YGBS" + TodayTime + "0001";
                    }

                }
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            {
                var msg = string.Empty;
                var errors = (from u in ex.EntityValidationErrors select u.ValidationErrors).ToList();
                foreach (var item in errors)
                    msg += item.FirstOrDefault().ErrorMessage;
                save.code = "1";
                save.msg = "数据有误";
                return ToJson(save);
            }
            catch (Exception e)
            {
                save.code = "1";
                save.msg = "数据有误";
                return ToJson(save);
            }

            db.Set<BS_CANGDAN>().AddOrUpdate(model);
            try
            {
                db.SaveChanges();
                save.code = "0";
                save.msg = "保存成功";
                return ToJson(save);
            }
            catch (Exception ex)
            {
                save.code = "1";
                save.msg = "数据有误";
                return ToJson(save);
            }
        }
        #endregion

        #region  //仓单 删除
        [Route("api/bs/cangdan/delete")]
        [System.Web.Http.HttpPost]
        public object bsCangdanDelete([FromBody]dynamic obj)
        {
            string guid = obj.guid;
            Api_common save = new Api_common();

            BS_CANGDAN model = db.BS_CANGDAN.FirstOrDefault(n => n.Guid == guid);
            db.BS_CANGDAN.Remove(model);
            int c = db.SaveChanges();
            if (c > 0)
            {
                save.code = "0";
                save.msg = "提交成功";
                return ToJson(save);
            }
            else
            {
                if (c == 0)
                {
                    save.code = "1";
                    save.msg = "无更新";
                    return ToJson(save);
                }
                save.code = "2";
                save.msg = "失败";
                return ToJson(save);
            }
        }
        #endregion

        #region  // 放贷列表
        [Route("api/bs/fangdai/list")]
        [System.Web.Http.HttpPost]
        public object FangDaiList([FromBody]dynamic obj)
        {
            int id = obj.id;
            C_TB_HC_GOODSBILL model_goodsbill = db.C_TB_HC_GOODSBILL.Find(id) ?? new C_TB_HC_GOODSBILL();

            List<BS_BAOJIAN> bgList = db.BS_BAOJIAN.Where(n => n.GoodsBillId == id).ToList();
            Api_kucunModel ac = null;
            ac = new Api_kucunModel()
            {
                code = "0",
                msg = "请求成功",
                data = new Api_KuCunModelData()
                {
                    list = bgList,
                }
            };
            return ToJson(ac);
            ;
        }
        #endregion

        #region  // 保税 发起仓储指令 列表 / 委托列表
        [Route("api/bs/cczl/list")]
        [System.Web.Http.HttpPost]
        public object bsCczlList([FromBody]dynamic obj)
        {
            int GoodsBillId = obj.GoodsBillId;
            int page = obj.page;
            int limit = obj.limit;
            int offset = (page - 1) * limit;
            var wherelambda = ExtLinq.True<C_TB_HC_CONSIGN>();
            wherelambda = wherelambda.And(t => t.GOODSBILL_ID == GoodsBillId);
            var list = db.Set<C_TB_HC_CONSIGN>().Where(wherelambda).OrderByDescending(n => n.CREATETIME).AsQueryable();
            int total = list.Count();
            object rows = list.Skip(offset).Take(limit).AsQueryable();
            Api_kucunModel ac = null;
            ac = new Api_kucunModel()
            {
                code = "0",
                msg = "请求成功",
                data = new Api_KuCunModelData()
                {
                    list = rows,
                }
            };
            return ToJson(ac);
        }
        #endregion

        #region 新增委托信息
        [Route("api/bs/weituo/add")]
        [System.Web.Http.HttpPost]
        public object WeituoAdd([FromBody]dynamic obj)
        {
            int userId = Convert.ToInt32(obj.userId);
            Sys_User loginModel = db.Sys_User.FirstOrDefault(n => n.ID == userId && n.state == "激活");

            int goodsBillId = obj.GoodsBillId;

            C_TB_HC_GOODSBILL model_goodsBill = db.C_TB_HC_GOODSBILL.Find(goodsBillId);

            C_TB_HC_CONSIGN model;

            Api_common save = new Api_common();
            if (!string.IsNullOrEmpty((obj.id).ToString()))
            {
                int id = obj.id;
                model = db.C_TB_HC_CONSIGN.FirstOrDefault(n => n.ID == id);
            }
            else
            {
                model = new C_TB_HC_CONSIGN();
                model.CREATETIME = DateTime.Now;
            }
            model.YuanQuID = loginModel.YuanQuId;
            model.CREATORNAME = loginModel.userName;
            model.WeiTuoBianHao = obj.WeiTuoBianHao;
            model.ZhiLingXiangMu = obj.ZhiLingXiangMu;
            model.ShenQingRen = obj.ShenQingRen;
            model.XiaDaShiJian = obj.XiaDaShiJian;
            model.ZhiLingShuLiang = obj.ZhiLingShuLiang;
            model.FengPiaoChuanMingHangCi = obj.FengPiaoChuanMingHangCi;
            model.ZuiYeDanHao = obj.ZuiYeDanHao;
            model.BeiZhu = obj.BeiZhu;
            model.GOODSBILL_ID = goodsBillId;
            
            if (model.ZhiLingXiangMu != "出库")
            {
                model.State = "进行中";
            }
            else
            {
                model.State = "待领导审核";
            }
            if (model.ZhiLingXiangMu == "入库" && model_goodsBill.State == "接单审核完")
            {
                save.code = "1";
                save.msg = "数据有误";
                return ToJson(save);
            }
            if (string.IsNullOrEmpty(model.CGNO))           //新建时生成流水号
            {
                string TodayTime = DateTime.Today.ToString("yyyyMMdd");
                C_TB_HC_CONSIGN Num = db.C_TB_HC_CONSIGN.OrderByDescending(n => n.CREATETIME).ToList().FirstOrDefault();
                if (Num != null)
                {
                    if (!string.IsNullOrEmpty(Num.CGNO))
                    {
                        if (Num.CGNO.Substring(2, 8) == TodayTime)
                        {
                            model.CGNO = "WT" + TodayTime + (Convert.ToInt32(Num.CGNO.Replace("WT" + TodayTime, "")) + 1).ToString("0000");
                        }
                        else
                        {
                            model.CGNO = "WT" + TodayTime + "0001";
                        }
                    }
                }

                else
                {
                    model.CGNO = "WT" + TodayTime + "0001";
                }

            }
            db.Set<C_TB_HC_CONSIGN>().AddOrUpdate(model);
            
            try
            {
                db.SaveChanges();
                save.code = "0";
                save.msg = "保存成功";
                return ToJson(save);
            }
            catch (Exception ex)
            {

                save.code = "1";
                save.msg = "数据有误";
                return ToJson(save);
            }
        }
        #endregion


        #region dic 作业单号
        [Route("api/dic/bs/zuoyedanhao")]
        [System.Web.Http.HttpPost]
        public object dicBsZuoyedanhao([FromBody]dynamic obj)
        {
            int userId = Convert.ToInt32(obj.userId);
            int id = obj.GoodsBillId;
            Sys_User loginModel = db.Sys_User.FirstOrDefault(n => n.ID == userId && n.state == "激活");
            List<BS_BAOGUAN> mlist = db.BS_BAOGUAN.Where(n => n.GoodsBillId == id && n.ZhongXinZuoYeDanHao != null).OrderBy(n => n.CreatTime).ToList();//作业单号
            Api_DicKV_Model a = null;
            if (mlist != null)
            {
                a = new Api_DicKV_Model()
                {
                    code = "0",
                    msg = "请求成功",
                    data = new Api_DicKV_ModelList { }
                };

                foreach (var item in mlist)
                {
                    a.data.list.Add(new Api_DicKV_ModelListKV()
                    {
                        value = item.GoodsBillId.ToInt(),
                        text = item.ZhongXinZuoYeDanHao
                    });
                }
            }
            else
            {
                a = new Api_DicKV_Model()
                {
                    code = "1",
                    msg = "用户id请求错误",
                };
            }
            return ToJson(a);
        }
        #endregion

        #region  //保税 委托 删除
        [Route("api/bs/weituo/delete")]
        [System.Web.Http.HttpPost]
        public object bsWeituoDelete([FromBody]dynamic obj)
        {
            int guid = obj.guid;
            Api_common save = new Api_common();

            C_TB_HC_CONSIGN model = db.C_TB_HC_CONSIGN.FirstOrDefault(n => n.ID == guid);
            db.C_TB_HC_CONSIGN.Remove(model);
            int c = db.SaveChanges();
            if (c > 0)
            {
                save.code = "0";
                save.msg = "提交成功";
                return ToJson(save);
            }
            else
            {
                if (c == 0)
                {
                    save.code = "1";
                    save.msg = "无更新";
                    return ToJson(save);
                }
                save.code = "2";
                save.msg = "失败";
                return ToJson(save);
            }
        }
        #endregion


        #region  // 保税 放货指令指令 列表
        [Route("api/bs/fanghuo/list")]
        [System.Web.Http.HttpPost]
        public object bsFanghuoList([FromBody]dynamic obj)
        {
            int GoodsBillId = obj.GoodsBillId;
            int page = obj.page;
            int limit = obj.limit;
            int offset = (page - 1) * limit;
            var wherelambda = ExtLinq.True<BS_FANGHUOZHILING>();
            wherelambda = wherelambda.And(t => t.GoodsBillId == GoodsBillId);
            var list = db.Set<BS_FANGHUOZHILING>().Where(wherelambda).OrderByDescending(n => n.CreatTime).AsQueryable();
            int total = list.Count();
            object rows = list.Skip(offset).Take(limit).AsQueryable();
            Api_kucunModel ac = null;
            ac = new Api_kucunModel()
            {
                code = "0",
                msg = "请求成功",
                data = new Api_KuCunModelData()
                {
                    list = rows,
                }
            };
            return ToJson(ac);
        }
        #endregion

        #region 新增放货指令
        [Route("api/bs/fanghuo/add")]
        [System.Web.Http.HttpPost]
        public object FanghuoAdd([FromBody]dynamic obj)
        {
            int userId = Convert.ToInt32(obj.userId);
            Sys_User loginModel = db.Sys_User.FirstOrDefault(n => n.ID == userId && n.state == "激活");

            int goodsBillId = obj.GoodsBillId;

            C_TB_HC_GOODSBILL model_goodsBill = db.C_TB_HC_GOODSBILL.Find(goodsBillId);

            BS_FANGHUOZHILING model;

            Api_common save = new Api_common();
            if (!string.IsNullOrEmpty((obj.id).ToString()))
            {
                string id = obj.id;
                model = db.BS_FANGHUOZHILING.FirstOrDefault(n => n.Guid == id);
            }
            else
            {
                model = new BS_FANGHUOZHILING();
                model.Guid = Guid.NewGuid().ToString();
                model.CreatTime = DateTime.Now;
            }
            model.FangHuoShengQingRen = obj.FangHuoShengQingRen;
            model.FangHuoShuLiang = obj.FangHuoShuLiang;
            model.FangHuoRiQi = obj.FangHuoRiQi;
            model.ZhiLingWenBen = obj.ZhiLingWenBen;
            model.GoodsBillId = goodsBillId;

           
            db.Set<BS_FANGHUOZHILING>().AddOrUpdate(model);

            try
            {
                db.SaveChanges();
                save.code = "0";
                save.msg = "保存成功";
                return ToJson(save);
            }
            catch (Exception ex)
            {

                save.code = "1";
                save.msg = "数据有误";
                return ToJson(save);
            }
        }
        #endregion

        #region  //保税 放货指令删除
        [Route("api/bs/fanghuo/delete")]
        [System.Web.Http.HttpPost]
        public object bsFanghuoDelete([FromBody]dynamic obj)
        {
            string guid = obj.guid;
            Api_common save = new Api_common();

            BS_FANGHUOZHILING model = db.BS_FANGHUOZHILING.FirstOrDefault(n => n.Guid == guid);
            db.BS_FANGHUOZHILING.Remove(model);
            int c = db.SaveChanges();
            if (c > 0)
            {
                save.code = "0";
                save.msg = "提交成功";
                return ToJson(save);
            }
            else
            {
                if (c == 0)
                {
                    save.code = "1";
                    save.msg = "无更新";
                    return ToJson(save);
                }
                save.code = "2";
                save.msg = "失败";
                return ToJson(save);
            }
        }
        #endregion

        #region  // 保税 发起提货指令 列表
        [Route("api/bs/tihuo/list")]
        [System.Web.Http.HttpPost]
        public object bsTihuoList([FromBody]dynamic obj)
        {
            int GoodsBillId = obj.GoodsBillId;
            int page = obj.page;
            int limit = obj.limit;
            int offset = (page - 1) * limit;
            var wherelambda = ExtLinq.True<BS_FangHuoNeiBuShenPi>();
            wherelambda = wherelambda.And(t => t.GoodsBillId == GoodsBillId);
            var list = db.Set<BS_FangHuoNeiBuShenPi>().Where(wherelambda).OrderByDescending(n => n.CreatTime).AsQueryable();
            int total = list.Count();
            object rows = list.Skip(offset).Take(limit).AsQueryable();
            Api_kucunModel ac = null;
            ac = new Api_kucunModel()
            {
                code = "0",
                msg = "请求成功",
                data = new Api_KuCunModelData()
                {
                    list = rows,
                }
            };
            return ToJson(ac);
        }
        #endregion

        #region 新增放货指令
        [Route("api/bs/tihuo/add")]
        [System.Web.Http.HttpPost]
        public object TihuoAdd([FromBody]dynamic obj)
        {
            int userId = Convert.ToInt32(obj.userId);
            Sys_User loginModel = db.Sys_User.FirstOrDefault(n => n.ID == userId && n.state == "激活");
            int goodsBillId = obj.GoodsBillId;

            C_TB_HC_GOODSBILL model_goodsbill = db.C_TB_HC_GOODSBILL.Find(goodsBillId) ?? new C_TB_HC_GOODSBILL(); ;

            BS_FangHuoNeiBuShenPi model;

            Api_common save = new Api_common();
            if (!string.IsNullOrEmpty((obj.id).ToString()))
            {
                string id = obj.id;
                model = db.BS_FangHuoNeiBuShenPi.FirstOrDefault(n => n.Guid == id);
            }
            else
            {
                model = new BS_FangHuoNeiBuShenPi();
                model.Guid = Guid.NewGuid().ToString();
                model.CreatPeople = loginModel.userName;
                model.CreatTime = DateTime.Now;
                model.State_CangDan = "仓单未确认";
            }
            model.GoodsBillId = goodsBillId;
            model.ChuanMing = model_goodsbill.ShipName;
            model.TiDanHao = model_goodsbill.BLNO;
            model.HuoMing = model_goodsbill.C_GOODS;
            model.TiDanShu = model_goodsbill.PLANWEIGHT.ToString();
            model.CunHuoRen = model_goodsbill.HuoZhu;

            model.HuoMing = obj.HuoMing;
            model.TiHuoRen = obj.TiHuoRen;
            model.FangXingPiCi = obj.FangXingPiCi;
            model.FangXingShuLiang = obj.FangXingShuLiang;


            db.Set<BS_FangHuoNeiBuShenPi>().AddOrUpdate(model);

            try
            {
                db.SaveChanges();
                save.code = "0";
                save.msg = "保存成功";
                return ToJson(save);
            }
            catch (Exception ex)
            {

                save.code = "1";
                save.msg = "数据有误";
                return ToJson(save);
            }
        }
        #endregion

        #region  //保税 提货指令删除
        [Route("api/bs/tihuo/delete")]
        [System.Web.Http.HttpPost]
        public object bsTihuoDelete([FromBody]dynamic obj)
        {
            string guid = obj.guid;
            Api_common save = new Api_common();

            BS_FangHuoNeiBuShenPi model = db.BS_FangHuoNeiBuShenPi.FirstOrDefault(n => n.Guid == guid);
            db.BS_FangHuoNeiBuShenPi.Remove(model);
            int c = db.SaveChanges();
            if (c > 0)
            {
                save.code = "0";
                save.msg = "提交成功";
                return ToJson(save);
            }
            else
            {
                if (c == 0)
                {
                    save.code = "1";
                    save.msg = "无更新";
                    return ToJson(save);
                }
                save.code = "2";
                save.msg = "失败";
                return ToJson(save);
            }
        }
        #endregion


        #region  //保税 业务委托审核
        [Route("api/bs/yewuweituo/shenhe")]
        [System.Web.Http.HttpPost]
        public object bsYewuWeituoShenhe([FromBody]dynamic obj)
        {
            int id = obj.id;
            Api_common save = new Api_common();
            try
            {
                C_TB_HC_GOODSBILL model_GoodsBill = db.C_TB_HC_GOODSBILL.Find(id) ?? new C_TB_HC_GOODSBILL();
                model_GoodsBill.State = "接单审核完";
                db.Set<C_TB_HC_GOODSBILL>().AddOrUpdate(model_GoodsBill);
                db.SaveChanges();
                List<C_TB_HC_CONSIGN> list_CONSIGN = db.C_TB_HC_CONSIGN.Where(n => n.GOODSBILL_ID == model_GoodsBill.ID && n.ZhiLingXiangMu == "入库").OrderBy(n => n.ID).ToList();
                foreach (var items_CONSIGN in list_CONSIGN)
                {
                    C_TB_HC_CONSIGN model_CONSIGN = db.C_TB_HC_CONSIGN.Find(items_CONSIGN.ID) ?? new C_TB_HC_CONSIGN();
                    model_CONSIGN.State = "已完成";
                    db.Set<C_TB_HC_CONSIGN>().AddOrUpdate(model_CONSIGN);
                    db.SaveChanges();
                    List<C_TB_HS_TALLYBILL> list_TALLYBILL = db.C_TB_HS_TALLYBILL.Where(n => n.CONSIGN_ID == model_CONSIGN.ID && n.CODE_OPSTYPE == "进库").OrderBy(n => n.ID).ToList();
                    foreach (var items_TALLYBILL in list_TALLYBILL)
                    {
                        C_TB_HS_TALLYBILL model_TALLYBILL = db.C_TB_HS_TALLYBILL.Find(items_TALLYBILL.ID) ?? new C_TB_HS_TALLYBILL();
                        model_TALLYBILL.State = "已完成";
                        db.Set<C_TB_HS_TALLYBILL>().AddOrUpdate(model_TALLYBILL);
                        db.SaveChanges();
                    }
                }
                save.code = "0";
                save.msg = "提交成功";
                return ToJson(save);
            }
            catch (Exception e)
            {
                save.code = "2";
                save.msg = "失败";
                return ToJson(save);
            }

        }
        #endregion

        #region dic 仓单 品名
        [Route("api/dic/bs/pinming")]
        [System.Web.Http.HttpPost]
        public object dicBsPinming([FromBody]dynamic obj)
        {
            int userId = Convert.ToInt32(obj.userId);
            Sys_User loginModel = db.Sys_User.FirstOrDefault(n => n.ID == userId && n.state == "激活");
            List<BS_C_PINGMING> mlist = db.BS_C_PINGMING.ToList();//作业单号
            Api_DicKV_Model a = null;
            if (mlist != null)
            {
                a = new Api_DicKV_Model()
                {
                    code = "0",
                    msg = "请求成功",
                    data = new Api_DicKV_ModelList { }
                };

                foreach (var item in mlist)
                {
                    a.data.list.Add(new Api_DicKV_ModelListKV()
                    {
                        value = item.Code.ToInt(),
                        text = item.GoodsName
                    });
                }
            }
            else
            {
                a = new Api_DicKV_Model()
                {
                    code = "1",
                    msg = "用户id请求错误",
                };
            }
            return ToJson(a);
        }
        #endregion

        #region dic 仓单 抬头人
        [Route("api/dic/bs/taitouren")]
        [System.Web.Http.HttpPost]
        public object dicTaitouren([FromBody]dynamic obj)
        {
            int userId = Convert.ToInt32(obj.userId);
            Sys_User loginModel = db.Sys_User.FirstOrDefault(n => n.ID == userId && n.state == "激活");
            List<BS_C_TAITOUREN> mlist = db.BS_C_TAITOUREN.ToList();//作业单号
            Api_DicKV_Model a = null;
            if (mlist != null)
            {
                a = new Api_DicKV_Model()
                {
                    code = "0",
                    msg = "请求成功",
                    data = new Api_DicKV_ModelList { }
                };

                foreach (var item in mlist)
                {
                    a.data.list.Add(new Api_DicKV_ModelListKV()
                    {
                        value = item.Code.ToInt(),
                        text = item.GoodsName
                    });
                }
            }
            else
            {
                a = new Api_DicKV_Model()
                {
                    code = "1",
                    msg = "用户id请求错误",
                };
            }
            return ToJson(a);
        }
        #endregion

        #region 新增报关放行
        [Route("api/bs/baoguanfangxing/add")]
        [System.Web.Http.HttpPost]
        public object baoguanFangXingAdd([FromBody]dynamic obj)
        {
            int userId = Convert.ToInt32(obj.userId);
            Sys_User loginModel = db.Sys_User.FirstOrDefault(n => n.ID == userId && n.state == "激活");
            int goodsBillId = obj.GoodsBillId;

            BS_BAOGUAN model;

            Api_common save = new Api_common();
            if (!string.IsNullOrEmpty((obj.id).ToString()))
            {
                string id = obj.id;
                model = db.BS_BAOGUAN.FirstOrDefault(n => n.Guid == id);
            }
            else
            {
                model = new BS_BAOGUAN();
                model.Guid = Guid.NewGuid().ToString();
                model.CreatTime = DateTime.Now;
                model.GoodsBillId = goodsBillId;
                model.CreatPeople = loginModel.userName;
            }
            model.FangXingRiQi = obj.FangHuoShengQingRen;
            model.BaoGuanRiQi = obj.FangHuoShuLiang;
            model.ZhongXinZuoYeDanHao = obj.FangHuoRiQi;
            model.BaoZhengJin = StrToDecimal((obj.BaoZhengJin).ToString());
            model.GuanShui = obj.GuanShui;
            model.ZengZhiShui = StrToDecimal((obj.ZengZhiShui).ToString());
            model.BeiZhu = obj.BeiZhu;
            model.State = "结束";

            db.Set<BS_BAOGUAN>().AddOrUpdate(model);

            try
            {
                db.SaveChanges();
                save.code = "0";
                save.msg = "保存成功";
                return ToJson(save);
            }
            catch (Exception ex)
            {

                save.code = "1";
                save.msg = "数据有误";
                return ToJson(save);
            }
        }
        #endregion

        #region  // 放货指令审核列表
        [Route("api/bs/fanghuozhilingshenhe/list")]
        [System.Web.Http.HttpPost]
        public object FanghuozhilingShenhe([FromBody]dynamic obj)
        {
            int limit = obj.limit;
            int page = obj.page;
            int offset = (page - 1) * limit;
            int userId = Convert.ToInt32(obj.userId);
            Sys_User loginModel = db.Sys_User.FirstOrDefault(n => n.ID == userId && n.state == "激活");
            var wherelambda = ExtLinq.True<BS_FANGHUOZHILING>();
            wherelambda = wherelambda.And(t => t.State == "待领导审核");
            List<BS_FANGHUOZHILING> BS_FANGHUOZHILING = new List<BS_FANGHUOZHILING>();
            List<C_TB_HC_GOODSBILL> list_GOODSBILL = db.C_TB_HC_GOODSBILL.Where(n => n.YuanQuID == loginModel.YuanQuId).OrderBy(n => n.ID).ToList();
            foreach (var items in list_GOODSBILL)
            {
                List<BS_FANGHUOZHILING> list_th = db.BS_FANGHUOZHILING.Where(n => n.GoodsBillId == items.ID).ToList();
                foreach (var items_th in list_th)
                {
                    BS_FANGHUOZHILING.Add(items_th);
                }
            }

            var list = db.BS_FANGHUOZHILING.Where(wherelambda).OrderByDescending(n => n.CreatTime).AsQueryable();
            int total = list.Count();
            object rows = list.Skip(offset).Take(limit).AsQueryable();
            Api_kucunModel ac = null;
            ac = new Api_kucunModel()
            {
                code = "0",
                msg = "请求成功",
                data = new Api_KuCunModelData()
                {
                    list = rows,
                }
            };
            return ToJson(ac);
            ;
        }
        #endregion

        #region  // 放货指令审核状态
        [Route("api/bs/fanghuozhilingshenhe/state")]
        [System.Web.Http.HttpPost]
        public object FanghuozhilingShenheState([FromBody]dynamic obj)
        {
            int userId = Convert.ToInt32(obj.userId);
            string state = obj.state;
            string guid = obj.guid;
            Sys_User loginModel = db.Sys_User.FirstOrDefault(n => n.ID == userId && n.state == "激活");
            Api_common common = new Api_common();
            try
            {
                BS_FANGHUOZHILING model_FangHuoNeiBuShenPi = db.BS_FANGHUOZHILING.Find(guid) ?? new BS_FANGHUOZHILING();
                model_FangHuoNeiBuShenPi.State = state;
                db.Set<BS_FANGHUOZHILING>().AddOrUpdate(model_FangHuoNeiBuShenPi);
                db.SaveChanges();
                common.code = "0";
                common.msg = "请求成功";
                return ToJson(common);
            }
            catch (Exception e)
            {
                common.code = "1";
                common.msg = "数据有误";
                return ToJson(common);
            }
        }
        #endregion

        #region  //保税委托列表
        [Route("api/bs/weituo/list")]
        [System.Web.Http.HttpPost]
        public object bsWeituoList([FromBody]dynamic obj)
        {
            int userId = Convert.ToInt32(obj.userId);
            int goodBillId = Convert.ToInt32(obj.goodBillId);
            int limit = Convert.ToInt32(obj.limit);
            int page = Convert.ToInt32(obj.page);
            int offset = (page - 1) * limit;
            Sys_User loginModel = db.Sys_User.FirstOrDefault(n => n.ID == userId && n.state == "激活");
            var wherelambda = ExtLinq.True<C_TB_HC_CONSIGN>();
            wherelambda = wherelambda.And(t => t.GOODSBILL_ID == goodBillId);
            var list = db.Set<C_TB_HC_CONSIGN>().Where(wherelambda).OrderByDescending(n => n.CREATETIME).AsQueryable();
            int total = list.Count();
            object rows = list.Skip(offset).Take(limit).AsQueryable();

            Api_kucunModel ac = null;
            ac = new Api_kucunModel()
            {
                code = "0",
                msg = "请求成功",
                data = new Api_KuCunModelData()
                {
                    list = rows,
                }
            };
            return ToJson(ac);
        }
        #endregion

        #region  //保税理货列表
        [Route("api/bs/lihuo/list")]
        [System.Web.Http.HttpPost]
        public object bsLihuoList([FromBody]dynamic obj)
        {
            int userId = Convert.ToInt32(obj.userId);
            int consignId = obj.consignId;
            int limit = obj.limit;
            int page = obj.page;
            int offset = (page - 1) * limit;
            List<C_TB_HS_TALLYBILL> list = db.C_TB_HS_TALLYBILL.Where(n => n.CONSIGN_ID == consignId).OrderByDescending(n => n.ID).Skip<C_TB_HS_TALLYBILL>((page - 1) * limit).Take<C_TB_HS_TALLYBILL>(limit).ToList();
            var total = list.Count;
            decimal CODE_SECTION = 0;
            foreach (var items in list)
            {
                //string xiangxi = "";
                //if (!string.IsNullOrEmpty(items.ZuoYeLeiXIng))
                //{
                //    string[] Guid = items.ZuoYeLeiXIng.Split(',');
                //    foreach (var tiems_g in Guid)
                //    {
                //        BS_LAOWUZUOYELEIBIE Model_LAOWUZUOYELEIBIE = db.BS_LAOWUZUOYELEIBIE.FirstOrDefault(n => n.Guid == tiems_g);//查找合同
                //        xiangxi += items.GoodsName + Model_LAOWUZUOYELEIBIE.ZuoYeLeiBieMingCheng + Model_LAOWUZUOYELEIBIE.DanJia + "*" + items.WEIGHT + "=" + Model_LAOWUZUOYELEIBIE.DanJia.ToDecimal() * items.WEIGHT + ";";
                //    }
                //    items.ZuoYeLeiXIng = xiangxi;
                //}
                //CODE_SECTION = Convert.ToDecimal(items.CODE_SECTION);
                //items.CODE_SECTION = db.C_TB_CODE_BOOTH.FirstOrDefault(n => n.ID == CODE_SECTION).BOOTH;
            }
            var rows = list.ToList();

            Api_kucunModel ac = null;
            ac = new Api_kucunModel()
            {
                code = "0",
                msg = "请求成功",
                data = new Api_KuCunModelData()
                {
                    list = rows,
                }
            };
            return ToJson(ac);
        }
        #endregion

        #region 保税获取作业明细
        [Route("api/dic/bs/zuoyemingxi")]
        [System.Web.Http.HttpPost]
        public object dicBsZuoyeMingxi([FromBody]dynamic obj)
        {
            int userId = Convert.ToInt32(obj.userId);
            int consignId = obj.consignId;
            Sys_User user = db.Sys_User.FirstOrDefault(n => n.ID == userId && n.state == "激活");
            C_TB_HC_CONSIGN model_CONSIGN = db.C_TB_HC_CONSIGN.Find(consignId) ?? new C_TB_HC_CONSIGN();
            List<BS_LAOWUZUOYELEIBIE> mlist = db.BS_LAOWUZUOYELEIBIE.Where(n => n.JinChuKu == model_CONSIGN.ZhiLingXiangMu && n.YuanQuId == user.YuanQuId).ToList();
            Api_kucunModel ac = null;
            ac = new Api_kucunModel()
            {
                code = "0",
                msg = "请求成功",
                data = new Api_KuCunModelData()
                {
                    list = mlist,
                }
            };
            return ToJson(ac);
        }
        #endregion

        #region  保税理货单添加 和 修改
        [Route("api/bs/lihuo/save")]
        [System.Web.Http.HttpPost]
        public object bslihuoSave([FromBody]dynamic obj)
        {
            int userId = Convert.ToInt32(obj.userId);
            Sys_User loginModel = db.Sys_User.FirstOrDefault(n => n.ID == userId);
            int consignId = obj.consignId;
            decimal? KuCun = 0;
            decimal? KuCunW = 0;
            decimal? KuCUnX = 0;
            C_TB_HC_CONSIGN model_CONSIGN_sh = db.C_TB_HC_CONSIGN.FirstOrDefault(n => n.ID == consignId) ?? new C_TB_HC_CONSIGN();
            C_TB_HC_GOODSBILL model_GOODSBILL = db.C_TB_HC_GOODSBILL.FirstOrDefault(n => n.ID == model_CONSIGN_sh.GOODSBILL_ID) ?? new C_TB_HC_GOODSBILL();
            List<C_TB_HC_CONSIGN> list_CONSIGN = db.C_TB_HC_CONSIGN.Where(n => n.GOODSBILL_ID == model_GOODSBILL.ID).OrderBy(n => n.ID).ToList();
            foreach (var items_Consign in list_CONSIGN)
            {
                List<C_TB_HS_TALLYBILL> list_TALLYBILL = db.C_TB_HS_TALLYBILL.Where(n => n.CONSIGN_ID == items_Consign.ID).OrderBy(n => n.ID).ToList();
                foreach (var items_TALLYBILL in list_TALLYBILL)
                {
                    if (items_TALLYBILL.CODE_OPSTYPE == "进库")
                    {
                        KuCun += items_TALLYBILL.AMOUNT;
                        KuCunW += items_TALLYBILL.WEIGHT;
                        KuCUnX += items_TALLYBILL.XIANGSHU;
                    }
                    if (items_TALLYBILL.CODE_OPSTYPE == "出库")
                    {
                        KuCun -= items_TALLYBILL.AMOUNT;
                        KuCunW -= items_TALLYBILL.WEIGHT;
                        KuCUnX -= items_TALLYBILL.XIANGSHU;
                    }
                }
            }


            var yuanquid = loginModel.YuanQuId ?? throw new ArgumentNullException("loginModel.Yuan");

            string ID = obj.ID;
            C_TB_HS_TALLYBILL model;
            if (!string.IsNullOrEmpty(ID))
            {
                int id = obj.ID;
                model = db.C_TB_HS_TALLYBILL.FirstOrDefault(n => n.ID == id);
            }
            else
            {
                model = new C_TB_HS_TALLYBILL();
                model.State = "进行中";
                model.Type = "进出库";
            }

            model.YuanQuID = yuanquid;
            model.TALLYMAN = loginModel.userName;
            decimal AorU = model.ID;

            if (model.TBNO == null)
            {
                string TodayTime = DateTime.Today.ToString("yyyyMMdd");
                C_TB_HS_TALLYBILL Num = db.C_TB_HS_TALLYBILL.OrderByDescending(n => n.TBNO)
                    .FirstOrDefault();
                if (Num != null)
                {
                    if (!string.IsNullOrEmpty(Num.TBNO))
                    {
                        if (Num.TBNO.Substring(2, 8) == TodayTime)
                        {
                            model.TBNO = "LH" + TodayTime +
                                         (Convert.ToInt32(Num.TBNO.Replace("LH" + TodayTime, "")) + 1).ToString("0000");
                        }
                        else
                        {
                            model.TBNO = "LH" + TodayTime + "0001";
                        }
                    }
                }

                else
                {
                    model.TBNO = "LH" + TodayTime + "0001";
                }
            }

            if (model.CAOZUO == "其他") //借磅的情况（件数和重量都=0，这样库存就不相加了）
            {
                model.AMOUNT = 0; //件数
                model.WEIGHT = 0; //重量
                model.PIECEWEIGHT = 0; //件重
                model.XIANGSHU = 0; //箱数

            }

            var cgno = model_CONSIGN_sh.CGNO;
            if (model.Type == "清场")
            {
                model.CreatTime = DateTime.Now;
            }
            C_TB_HC_CONSIGN con = db.C_TB_HC_CONSIGN.FirstOrDefault(n => n.CGNO == cgno);
            if (con != null) model.CONSIGN_ID = con.ID;
            if (model.ID == 0)
            {
                model.State = "进行中";
            }


            model.CONSIGN_ID = con.ID;
            model.CGNO = con.CGNO;
            model.TRAINNUM = StrToDecimal(obj.TRAINNUM.ToString());
            model.SIGNDATE = obj.SIGNDATE;
            model.CODE_OPSTYPE = obj.CODE_OPSTYPE;
            model.AMOUNT = StrToDecimal(obj.AMOUNT.ToString());
            model.WEIGHT = StrToDecimal(obj.WEIGHT.ToString());
            model.XIANGSHU = StrToDecimal(obj.XIANGSHU.ToString());
            model.STORAG = obj.STORAG;
            model.CODE_SECTION = obj.CODE_SECTION;
            model.CODE_QUALITY = obj.CODE_QUALITY;
            model.ZuoYeLeiXIng = obj.selectedValueStr;
            model.REMARK = obj.REMARK;

            Api_common save = new Api_common();
            try
            {
                if (model_GOODSBILL.State == "接单审核完" && model.CODE_OPSTYPE == "进库")
                {
                    save.code = "1";
                    save.msg = "业务委托已接单审核完，不可添加进库理货单";
                    return ToJson(save);
                    //return Json("改业务委托已接单审核完，不可添加进库理货单");
                }
                List<C_TB_HC_CONSIGN> list_wth =
                    db.C_TB_HC_CONSIGN.Where(n => n.CGNO == model.CGNO).OrderBy(n => n.ID).ToList();
                if (list_wth.Count == 0)
                {
                    save.code = "101";
                    save.msg = "找不到对应的委托号";
                    return ToJson(save);
                    //return Json("找不到对应的委托号");
                }

                List<C_TB_HC_CONSIGN> list_wth1 = db.C_TB_HC_CONSIGN
                    .Where(n => n.CGNO == model.CGNO && n.State != "已完成").OrderBy(n => n.ID).ToList();
                if (list_wth1.Count == 0)
                {
                    save.code = "102";
                    save.msg = "该委托号流程已结束";
                    return ToJson(save);
                    //return Json("该委托号流程已结束");
                }

                db.C_TB_HS_TALLYBILL.AddOrUpdate(model);
                db.SaveChanges();
                TallyBllController tl = new TallyBllController();
                bool b = tl.EidSTOCKDORMANT(model, AorU);
                C_TB_HC_CONSIGN model_CONSIGN = db.C_TB_HC_CONSIGN.Find(model.CONSIGN_ID) ?? new C_TB_HC_CONSIGN();
                List<C_TB_HS_TALLYBILL> list_TALLYBILL = db.C_TB_HS_TALLYBILL
                    .Where(n => n.CONSIGN_ID == model.CONSIGN_ID).OrderBy(n => n.ID).ToList();
                if (list_TALLYBILL.Count != 0)
                {
                    model_CONSIGN.State = "已生成" + list_TALLYBILL.Count + "条理货单";
                }
                else
                {
                    model_CONSIGN.State = "进行中";
                }

                db.Set<C_TB_HC_CONSIGN>().AddOrUpdate(model_CONSIGN);
                db.SaveChanges();
                if (b)
                {
                    save.code = "0";
                    save.msg = "成功";
                    return ToJson(save);
                    //return Json("成功");
                }
                else
                {
                    db.C_TB_HS_TALLYBILL.Remove(model);
                    db.SaveChanges();
                    //return Json("更新库存失败");
                    save.code = "103";
                    save.msg = "更新库存失败";
                    return ToJson(save);
                }
            }

            catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            {
                var msg = string.Empty;
                var errors = (from u in ex.EntityValidationErrors select u.ValidationErrors).ToList();
                foreach (var item in errors)
                    msg += item.FirstOrDefault().ErrorMessage;

                //return Json(new AjaxResult { state = ResultType.error.ToString(), message = msg });
                save.code = "104";
                save.msg = "数据有误";
                return ToJson(save);
            }
            catch (Exception ex)
            {
                //return Json(new AjaxResult { state = ResultType.error.ToString(), message = ex.ToString() });
                save.code = "105";
                save.msg = "数据有误";
                return ToJson(save);
            }
        }
        #endregion

        #region 保税 获取委托单下面理货单的累计情况
        [Route("api/bs/lihuo/all")]
        [System.Web.Http.HttpPost]
        public object bsLihuoAll([FromBody]dynamic obj)
        {
            int userId = Convert.ToInt32(obj.userId);
            int consignId = obj.consignId;
            Sys_User user = db.Sys_User.FirstOrDefault(n => n.ID == userId && n.state == "激活");
            List<C_TB_HS_TALLYBILL> list = db.C_TB_HS_TALLYBILL.Where(n => n.CONSIGN_ID == consignId).OrderByDescending(n => n.ID).ToList();
            decimal? allin = 0;//累计进库
            decimal? allout = 0;//累计出库
            foreach (var items in list)
            {
                if (items.CODE_OPSTYPE == "进库")
                {
                    allin += items.WEIGHT;
                }
                if (items.CODE_OPSTYPE == "出库")
                {
                    allout += items.WEIGHT;
                }
            }
            ApiBsLihuoAll a = null;
            a = new ApiBsLihuoAll()
            {
                code = "0",
                msg = "请求成功",
                data = new BsLihuoAllData
                {
                    allin = allin,
                    allout = allout,
                    sunyi = allin - allout
                }
            };
            return ToJson(a);
        }
        #endregion

        #region  //保税 委托单下面的指令项目
        [Route("api/bs/weituozl/list")]
        [System.Web.Http.HttpPost]
        public object bsZLXM([FromBody]dynamic obj)
        {
            int userId = Convert.ToInt32(obj.userId);
            int consignId = obj.consignId;
            int limit = obj.limit;
            int page = obj.page;
            int offset = (page - 1) * limit;
            List<BS_ZYLB_TBLL> list = db.BS_ZYLB_TBLL.Where(n => n.ConSignId == consignId).OrderByDescending(n => n.CreatTime).Skip<BS_ZYLB_TBLL>(offset).Take<BS_ZYLB_TBLL>(limit).ToList();
            var total = list.Count;
            //decimal CODE_SECTION = 0;

            //foreach (var items in list)
            //{
            //    string xiangxi = "";
            //    if (!string.IsNullOrEmpty(items.ZuoYeLeiXIng))
            //    {
            //        string[] Guid = items.ZuoYeLeiXIng.Split(',');
            //        foreach (var tiems_g in Guid)
            //        {
            //            BS_LAOWUZUOYELEIBIE Model_LAOWUZUOYELEIBIE = db.BS_LAOWUZUOYELEIBIE.FirstOrDefault(n => n.Guid == tiems_g);//查找合同
            //            xiangxi += items.GoodsName + Model_LAOWUZUOYELEIBIE.ZuoYeLeiBieMingCheng + Model_LAOWUZUOYELEIBIE.DanJia + "*" + items.WEIGHT + "=" + Model_LAOWUZUOYELEIBIE.DanJia.ToDecimal() * items.WEIGHT + ";";
            //        }
            //        items.ZuoYeLeiXIng = xiangxi;
            //    }
            //    CODE_SECTION = Convert.ToDecimal(items.CODE_SECTION);
            //    items.CODE_SECTION = db.C_TB_CODE_BOOTH.FirstOrDefault(n => n.ID == CODE_SECTION).BOOTH;
            //}
            var rows = list.ToList();

            Api_kucunModel ac = null;
            ac = new Api_kucunModel()
            {
                code = "0",
                msg = "请求成功",
                data = new Api_KuCunModelData()
                {
                    list = rows,
                }
            };
            return ToJson(ac);
        }
        #endregion

        #region  // 保税 计费管理列表
        [Route("api/bs/jifei/list")]
        [System.Web.Http.HttpPost]
        public object bsJifeiList([FromBody]dynamic obj)
        {
            int limit = obj.limit;
            int page = obj.page;
            int offset = (page - 1) * limit;
            int userId = Convert.ToInt32(obj.userId);
            Sys_User loginModel = db.Sys_User.FirstOrDefault(n => n.ID == userId && n.state == "激活");


            var wherelambda = ExtLinq.True<C_TB_HC_GOODSBILL>();
            wherelambda = wherelambda.And(t => t.YuanQuID == loginModel.YuanQuId);
            wherelambda = wherelambda.And(t => t.State != "待审核");
            wherelambda = wherelambda.And(t => t.State != "被驳回");
            wherelambda = wherelambda.And(t => t.State_JiFei != "已完成" || t.State_JiFei == null);
            if (!string.IsNullOrEmpty(obj.GBNO))
            {
                string GBNO = obj.GBNO;
                wherelambda = wherelambda.And(t => t.GBNO.Contains(GBNO));
            }
            if (!string.IsNullOrEmpty(obj.C_GOODSAGENT_NAME))
            {
                string C_GOODSAGENT_NAME = obj.C_GOODSAGENT_NAME;
                wherelambda = wherelambda.And(t => t.C_GOODSAGENT_NAME.Contains(C_GOODSAGENT_NAME));
            }
            if (!string.IsNullOrEmpty(obj.CreatTime))
            {
                string CreatTime = obj.CreatTime;
                DateTime CreatTime_date = Convert.ToDateTime(obj.CreatTime + " 00:00:00");
                wherelambda = wherelambda.And(n => n.CreatTime >= CreatTime_date);

            }
            if (!string.IsNullOrEmpty(obj.CreatTime1))
            {
                string CreatTime1 = obj.CreatTime1;
                DateTime CreatTime1_date = Convert.ToDateTime(CreatTime1 + " 23:59:59");
                wherelambda = wherelambda.And(n => n.CreatTime <= CreatTime1_date);

            }
            if (!string.IsNullOrEmpty(obj.BLNO))
            {
                string BLNO = obj.BLNO;
                wherelambda = wherelambda.And(t => t.BLNO.Contains(BLNO));
            }
            if (!string.IsNullOrEmpty(obj.C_GOODS))
            {
                string C_GOODS = obj.C_GOODS;
                wherelambda = wherelambda.And(t => t.C_GOODS.Contains(C_GOODS));
            }
            if (!string.IsNullOrEmpty(obj.HuoZhu))
            {
                string HuoZhu = obj.HuoZhu;
                wherelambda = wherelambda.And(t => t.HuoZhu.Contains(HuoZhu));
            }
            if (!string.IsNullOrEmpty(obj.ShipName))
            {
                string ShipName = obj.ShipName;
                wherelambda = wherelambda.And(t => t.ShipName.Contains(ShipName));
            }
            if (!string.IsNullOrEmpty(obj.VGNO))
            {
                string VGNO = obj.VGNO;
                wherelambda = wherelambda.And(t => t.VGNO.Contains(VGNO));
            }
            int count = db.Set<C_TB_HC_GOODSBILL>().Where(wherelambda).OrderByDescending(n => n.CreatTime).Count();
            var list = db.Set<C_TB_HC_GOODSBILL>().Where(wherelambda).OrderByDescending(n => n.CreatTime).Skip(offset).Take(limit).AsQueryable();

            foreach (var itsms_GoodsBill in list)//计算库存
            {
                C_TB_HC_CONTRACT Model_CONTRACT = db.C_TB_HC_CONTRACT.FirstOrDefault(n => n.Guid == itsms_GoodsBill.CONTRACT_Guid);//查找合同
                decimal? KuCun = 0;
                decimal? KuCunW = 0;
                List<C_TB_HC_CONSIGN> list_CONSIGN = db.C_TB_HC_CONSIGN.Where(n => n.GOODSBILL_ID == itsms_GoodsBill.ID).OrderBy(n => n.ID).ToList();
                foreach (var items_Consign in list_CONSIGN)
                {
                    List<C_TB_HS_TALLYBILL> list_TALLYBILL = db.C_TB_HS_TALLYBILL.Where(n => n.CONSIGN_ID == items_Consign.ID).OrderBy(n => n.ID).ToList();
                    foreach (var items_TALLYBILL in list_TALLYBILL)
                    {
                        if (items_TALLYBILL.CODE_OPSTYPE == "进库")
                        {
                            KuCun += items_TALLYBILL.AMOUNT;
                            KuCunW += items_TALLYBILL.WEIGHT;
                        }
                        if (items_TALLYBILL.CODE_OPSTYPE == "出库")
                        {
                            KuCun -= items_TALLYBILL.AMOUNT;
                            KuCunW -= items_TALLYBILL.WEIGHT;
                        }
                    }

                }
                decimal? ShiJiJInKu = 0;
                decimal? ShiJiChuKu = 0;
                MoneyController moneyController = new MoneyController();
                moneyController.GetTallyBllList_fy_hj(Convert.ToInt32(itsms_GoodsBill.ID), out decimal? Fyjehj, out decimal? Fyjehj_Sh, out decimal? Kpjehj_Sh);
                itsms_GoodsBill.Fyjehj = Fyjehj.ToDecimal(2);
                itsms_GoodsBill.KunCun = KuCun.ToString();
                itsms_GoodsBill.KunCunW = KuCunW.ToDecimal(3).ToString();
                List<C_TB_JIESUAN> list_JieSuan = db.C_TB_JIESUAN.Where(n => n.GoodsBill_id == itsms_GoodsBill.ID).OrderBy(n => n.ID).ToList();
                itsms_GoodsBill.Lkjehj = list_JieSuan.Sum(n => n.LaiKuanJinE).ToDecimal(2);
                itsms_GoodsBill.Sryk = (Fyjehj - list_JieSuan.Sum(n => n.LaiKuanJinE)).ToDecimal(2);
                List<C_TB_CHENGBEN> list_ChengBen = db.C_TB_CHENGBEN.Where(n => n.GoodsBill_id == itsms_GoodsBill.ID).OrderBy(n => n.ID).ToList();
                itsms_GoodsBill.ChengBenJinE = list_ChengBen.Sum(n => n.ChengBenJinE).ToDecimal(2);
                itsms_GoodsBill.LiRunZongE = (Fyjehj - list_ChengBen.Sum(n => n.ChengBenJinE)).ToDecimal(2);
                if (list_JieSuan.Sum(n => n.KaiPiaoJinE) == 0)
                {
                    itsms_GoodsBill.HuiShouLv = 0;
                }
                else
                {
                    itsms_GoodsBill.HuiShouLv = (list_JieSuan.Sum(n => n.LaiKuanJinE) / list_JieSuan.Sum(n => n.KaiPiaoJinE)).ToDecimal(2);

                }
                List<C_TB_HC_CONSIGN> list_KuCunCONSIGN = db.C_TB_HC_CONSIGN.Where(n => n.GOODSBILL_ID == itsms_GoodsBill.ID).OrderBy(n => n.ID).ToList();
                foreach (var items_KuCunCONSIGN in list_KuCunCONSIGN)
                {
                    List<C_TB_HS_TALLYBILL> list_KuCunTALLYBILL = db.C_TB_HS_TALLYBILL.Where(n => n.CONSIGN_ID == items_KuCunCONSIGN.ID).OrderBy(n => n.ID).ToList();
                    ShiJiJInKu += list_KuCunTALLYBILL.Where(n => n.CODE_OPSTYPE == "进库" && n.Type != "清场").Sum(n => n.WEIGHT);
                    ShiJiChuKu += list_KuCunTALLYBILL.Where(n => n.CODE_OPSTYPE == "出库" && n.Type != "清场").Sum(n => n.WEIGHT);
                }
                itsms_GoodsBill.ShiJiJInKu = ShiJiJInKu.ToDecimal(3);
                itsms_GoodsBill.ShiJiChuKu = ShiJiChuKu.ToDecimal(3);
                itsms_GoodsBill.KaiPiaoJineHJ = list_JieSuan.Sum(n => n.KaiPiaoJinE).ToDecimal(2);
                itsms_GoodsBill.KaiPiaoShuiHouHJ = Kpjehj_Sh.ToDecimal(2);
                itsms_GoodsBill.WeiKaiPiaoJineHJ = (Fyjehj - list_JieSuan.Sum(n => n.KaiPiaoJinE)).ToDecimal(2);
                itsms_GoodsBill.WeiKaiPiaoShuiHouHJ = (Fyjehj_Sh - Kpjehj_Sh).ToDecimal(2);
                itsms_GoodsBill.YinShouZhangKuanYue = (list_JieSuan.Sum(n => n.LaiKuanJinE) - list_JieSuan.Sum(n => n.KaiPiaoJinE)).ToDecimal(2);
                itsms_GoodsBill.LiRunZongE = (Fyjehj - list_ChengBen.Sum(n => n.ChengBenJinE)).ToDecimal(2);
                itsms_GoodsBill.YikaiPiaoYinShouYuE = (list_JieSuan.Sum(n => n.KaiPiaoJinE) - list_JieSuan.Sum(n => n.LaiKuanJinE)).ToDecimal(2);
                itsms_GoodsBill.WeiKaiPiaoYingShouYuE = ((Fyjehj - list_JieSuan.Sum(n => n.KaiPiaoJinE)) - list_JieSuan.Sum(n => n.YuShouJinE)).ToDecimal(2);
            }

            int total = count;
            object rows = list;
            Api_kucunModel ac = null;
            ac = new Api_kucunModel()
            {
                code = "0",
                msg = "请求成功",
                data = new Api_KuCunModelData()
                {
                    list = rows,
                }
            };
            return ToJson(ac);


        }
        #endregion

        #region  // 保税 计费审核
        [Route("api/bs/jifei/shenhe")]
        [System.Web.Http.HttpPost]
        public object bsJifeiShenhe([FromBody]dynamic obj)
        {
            int userId = Convert.ToInt32(obj.userId);
            int ID = obj.ID;
            Sys_User loginModel = db.Sys_User.FirstOrDefault(n => n.ID == userId && n.state == "激活");
            MoneyController money = new MoneyController();
            var res = money.shenhe_jifei(ID);
            return formatJson(res.Data);
        }
        #endregion

        public object formatJson(object data)
        {
            return data.ToString().ToJson();
        }

        #region  // 保税 计费管理 二级列表合同
        [Route("api/bs/jifei/erji")]
        [System.Web.Http.HttpPost]
        public object bsJifeiErji([FromBody]dynamic obj)
        {
            int userId = Convert.ToInt32(obj.userId);
            int id = obj.ID;
            Sys_User loginModel = db.Sys_User.FirstOrDefault(n => n.ID == userId && n.state == "激活");
            decimal? shuilv = 0;
            decimal? HuiShouLv = 0;
            List<Stock_Money> list = new List<Stock_Money>();
            int total = 0;
            var rows = list.ToList();
            //C_TB_HS_TALLYBILL Model_TALLYBILL = db.C_TB_HS_TALLYBILL.FirstOrDefault(n => n.ID == id);//查找理货单
            //C_TB_HC_CONSIGN Model_CONSIGN = db.C_TB_HC_CONSIGN.FirstOrDefault(n => n.ID == Model_TALLYBILL.CONSIGN_ID);//查找委托
            C_TB_HC_GOODSBILL Model_GOODSBILL = db.C_TB_HC_GOODSBILL.FirstOrDefault(n => n.ID == id);//查找票货
            Api_kucunModel ac = null;
            if (Model_GOODSBILL.CONTRACT_Guid == null)
            {
                
                ac = new Api_kucunModel()
                {
                    code = "0",
                    msg = "请求成功",
                    data = new Api_KuCunModelData()
                    {
                        list = rows,
                    }
                };
                return ToJson(ac);
            }
            C_TB_HC_CONTRACT Model_CONTRACT = db.C_TB_HC_CONTRACT.FirstOrDefault(n => n.Guid == Model_GOODSBILL.CONTRACT_Guid);//查找合同
            List<C_TB_HC_CONTRACT_DETAILED> list_DETAILED = db.C_TB_HC_CONTRACT_DETAILED.Where(n => n.CONTRACT_Guid == Model_CONTRACT.Guid).ToList();


            foreach (var items in list_DETAILED)
            {
                string KaiPiao_State = "";

                B_TB_KAIPIAOJILU Model_JilU = db.B_TB_KAIPIAOJILU.FirstOrDefault(n => n.GoodsBillId == id && n.FeiMuZhongLei == items.FeiMuZhongLei && n.Type == items.Type);
                if (Model_JilU != null)
                {
                    KaiPiao_State = "已提交";
                }
                else
                {
                    KaiPiao_State = "未提交";
                }


                C_TB_ZHIXINGFEILV model_ZHIXINGFEILV = db.C_TB_ZHIXINGFEILV.FirstOrDefault(n => n.FeiMuZhongLei == items.FeiMuZhongLei && n.Type == items.Type && n.GoodsBillId == id);

                decimal? ShiJiShuiLv = 0;
                if (model_ZHIXINGFEILV != null)
                {
                    if (!string.IsNullOrEmpty(model_ZHIXINGFEILV.FeiLv.ToString()))
                    {
                        ShiJiShuiLv = model_ZHIXINGFEILV.FeiLv;
                    }
                    else
                    {
                        ShiJiShuiLv = items.DanJia;
                    }
                }
                else
                {
                    ShiJiShuiLv = items.DanJia;
                }
                string type = "";
                if (string.IsNullOrEmpty(items.Type))
                {
                    type = "全部";
                }
                else
                {
                    type = items.Type;
                }
                List<C_TB_JIESUAN> list_JieSuan = db.C_TB_JIESUAN.Where(n => n.FeiMuZhongLei == items.FeiMuZhongLei && n.Type == type && n.GoodsBill_id == id).ToList();
                List<C_TB_CHENGBEN> list_ChengBen = db.C_TB_CHENGBEN.Where(n => n.FeiMuZhongLei == items.FeiMuZhongLei && n.Type == type && n.GoodsBill_id == id).ToList();
                if (list_JieSuan.Sum(n => n.KaiPiaoJinE) == 0)
                {
                    HuiShouLv = 0;
                }
                else
                {
                    HuiShouLv = list_JieSuan.Sum(n => n.LaiKuanJinE) / list_JieSuan.Sum(n => n.KaiPiaoJinE);
                }
                if (!string.IsNullOrEmpty(items.ShuiE) && items.ShuiE != null)
                {
                    shuilv = Convert.ToDecimal(items.ShuiE);
                }
                else
                {
                    shuilv = 0;
                }
                if (string.IsNullOrEmpty(items.Type) || items.Type == "null")
                {
                    items.Type = "全部";
                }
                MoneyController money = new MoneyController();
                List<C_TB_SHOUFEI> list_ShouFei = db.C_TB_SHOUFEI.Where(n => n.FeiMuZhongLei == items.FeiMuZhongLei && n.Type == items.Type && n.GoodsBill_id == id).ToList();
                decimal? feiyong = 0;
                decimal? ShuLiang = 0;
                //TimeSpan? ts = DateTime.Now - Model_TALLYBILL.SIGNDATE;//获取收费天数
                if (items.JiLiangDanWei.Contains("*天"))
                {
                    if (items.Type == "进库" || items.Type == "出库")
                    {
                        decimal? FeiYong, JiFeiShuLiang, duicun;
                        
                        money.GetTallyBllList_hqdc(id, out FeiYong, out JiFeiShuLiang, out duicun);
                        feiyong = FeiYong.ToDecimal(2);
                        ShuLiang = JiFeiShuLiang;
                    }
                    else
                    {
                        decimal? FeiYong, JiFeiShuLiang, duicun;
                        money.GetTallyBllList_hqdc_k(id, out FeiYong, out JiFeiShuLiang, out duicun);
                        feiyong = FeiYong.ToDecimal(2);
                        ShuLiang = JiFeiShuLiang;
                    }
                }

                else
                {

                    decimal? FeiYong, shuLiang;
                    money.GetTallyBllList_hqqyfy(id, items.FeiMuZhongLei, items.Type, out FeiYong, out shuLiang);
                    ShuLiang = shuLiang;
                    feiyong = (shuLiang * ShiJiShuiLv).ToDecimal(2);

                }

                Stock_Money model = new Stock_Money()
                {
                    ID = Model_GOODSBILL.ID,
                    HuoMing = items.HuoMing,
                    DanJia = items.DanJia,
                    FMZhongLei = items.FeiMuZhongLei,
                    JiLiangDanWei = items.JiLiangDanWei,
                    BeiZhu = items.BeiZhu,
                    FeiYong = feiyong.ToDecimal(2),
                    MianCunQi = items.MianDuiCunTianShu,
                    ShuLiang = ShuLiang.ToDecimal(3),
                    GoodsBill_id = id,
                    Type = items.Type,
                    YiShou = list_ShouFei.Sum(n => n.JinE).ToDecimal(2),
                    WeiShou = feiyong - list_ShouFei.Sum(n => n.JinE).ToDecimal(2),
                    ShuiHouJinE = (feiyong / Convert.ToDecimal((1 + (Convert.ToDecimal(shuilv) * Convert.ToDecimal(0.01))))).ToDecimal(2),
                    KaiPiaoJinE = list_JieSuan.Sum(n => n.KaiPiaoJinE).ToDecimal(2),
                    KaiPiaoShuiHou = (list_JieSuan.Sum(n => n.KaiPiaoJinE).ToDecimal(2) / Convert.ToDecimal((1 + (Convert.ToDecimal(shuilv) * Convert.ToDecimal(0.01))))).ToDecimal(2),
                    LaiKuanJinE = list_JieSuan.Sum(n => n.LaiKuanJinE).ToDecimal(2),
                    WeiKaiPiaoJinE = (feiyong - list_JieSuan.Sum(n => n.KaiPiaoJinE)).ToDecimal(2),
                    WeiKaiPiaoShuiHou = ((feiyong - list_JieSuan.Sum(n => n.KaiPiaoJinE)) / Convert.ToDecimal((1 + (Convert.ToDecimal(shuilv) * Convert.ToDecimal(0.01))))).ToDecimal(2),
                    YinSHouYuE = (list_JieSuan.Sum(n => n.KaiPiaoJinE) - list_JieSuan.Sum(n => n.LaiKuanJinE)).ToDecimal(2),
                    HuiShouLv = HuiShouLv.ToDecimal(3),
                    WaiFuJinE = list_ChengBen.Sum(n => n.ChengBenJinE),
                    ShiJiShuiLv = ShiJiShuiLv,
                    ChengBenJiFeiYiJu = items.ChengBenJiFeiYiJu,
                    KaiPiao_State = KaiPiao_State

                };
                list.Add(model);

            }
            Stock_Money model_hj = new Stock_Money()//合计
            {
                ID = Model_GOODSBILL.ID,
                HuoMing = "",
                DanJia = null,
                FMZhongLei = "合计",
                JiLiangDanWei = "",
                BeiZhu = "",
                FeiYong = list.Sum(n => n.FeiYong).ToDecimal(3),
                MianCunQi = null,
                ShuLiang = null,
                GoodsBill_id = id,
                Type = "",
                YiShou = list.Sum(n => n.YiShou).ToDecimal(2),
                WeiShou = list.Sum(n => n.WeiShou).ToDecimal(2),
                ShuiHouJinE = list.Sum(n => n.ShuiHouJinE).ToDecimal(2),
                KaiPiaoJinE = list.Sum(n => n.KaiPiaoJinE).ToDecimal(2),
                LaiKuanJinE = list.Sum(n => n.LaiKuanJinE).ToDecimal(2),
                WeiKaiPiaoJinE = list.Sum(n => n.WeiKaiPiaoJinE).ToDecimal(2),
                YinSHouYuE = list.Sum(n => n.YinSHouYuE).ToDecimal(2),
                HuiShouLv = null,
                WeiKaiPiaoShuiHou = list.Sum(n => n.WeiKaiPiaoShuiHou).ToDecimal(2),
                KaiPiaoShuiHou = list.Sum(n => n.KaiPiaoShuiHou).ToDecimal(2),
                WaiFuJinE = list.Sum(n => n.WaiFuJinE).ToDecimal(2),


            };
            list.Add(model_hj);
            total = list.Count();
            rows = list.ToList();
            ac = new Api_kucunModel()
            {
                code = "0",
                msg = "请求成功",
                data = new Api_KuCunModelData()
                {
                    list = rows,
                }
            };
            return ToJson(ac);
        }
        #endregion

        #region  // 保税 计费 修改实际费率
        [Route("api/bs/jifei/change/sjfl")]
        [System.Web.Http.HttpPost]
        public object bsJifeiChangeSjfl([FromBody]dynamic obj)
        {
            int userId = Convert.ToInt32(obj.userId);
            int GoodsBill_id = obj.goodsBillId;
            string Type = obj.Type;
            string FeiMuZhongLei = obj.FeiMuZhongLei;
            int FeiLv = obj.FeiLv;
            Sys_User loginModel = db.Sys_User.FirstOrDefault(n => n.ID == userId && n.state == "激活");
            C_TB_ZHIXINGFEILV model = db.C_TB_ZHIXINGFEILV.FirstOrDefault(n => n.GoodsBillId == GoodsBill_id && n.Type == Type && n.FeiMuZhongLei == FeiMuZhongLei) ?? new C_TB_ZHIXINGFEILV();
            model.FeiLv = FeiLv;
            db.C_TB_ZHIXINGFEILV.AddOrUpdate(model);
            Api_common common = new Api_common();
            try
            {
                db.SaveChanges();
                common.code = "0";
                common.msg = "success";
            }
            catch (Exception e)
            {
                db.SaveChanges();
                common.code = "1";
                common.msg = "数据有误";
                throw;
            }
            return ToJson(common);
        }
        #endregion

        #region  // 保税 计费 收费规则列表
        [Route("api/bs/shoufeigz/list")]
        [System.Web.Http.HttpPost]
        public object bsShoufeigzList([FromBody]dynamic obj)
        {
            int userId = Convert.ToInt32(obj.userId);
            int GoodsBill_id = obj.goodsBillId;
            string Type = obj.Type;
            string FeiMuZhongLei = obj.FeiMuZhongLei;
            if (string.IsNullOrEmpty(Type) || Type == "null")
            {
                Type = "全部";
            }

            List<C_DIC_GUIZE> list = db.C_DIC_GUIZE.Where(n => n.FeiMuZhongLei == FeiMuZhongLei && n.Type == Type && n.GoodsBillId == GoodsBill_id).ToList();
            int total = list.Count();
            object rows = list.AsQueryable();
            Api_kucunModel ac = null;
            ac = new Api_kucunModel()
            {
                code = "0",
                msg = "请求成功",
                data = new Api_KuCunModelData()
                {
                    list = rows,
                }
            };
            return ToJson(ac);
        }
        #endregion

        #region  // 保税 计费 收费规则删除
        [Route("api/bs/shoufeigz/delete")]
        [System.Web.Http.HttpPost]
        public object bsShoufeigzDelete([FromBody]dynamic obj)
        {
            int userId = Convert.ToInt32(obj.userId);
            string guid = obj.guid;
            Api_common save = new Api_common();

            C_DIC_GUIZE model = db.C_DIC_GUIZE.FirstOrDefault(n => n.Guid == guid);
            db.C_DIC_GUIZE.Remove(model);
            int c = db.SaveChanges();
            if (c > 0)
            {
                save.code = "0";
                save.msg = "提交成功";
                return ToJson(save);
            }
            else
            {
                if (c == 0)
                {
                    save.code = "1";
                    save.msg = "无更新";
                    return ToJson(save);
                }
                save.code = "2";
                save.msg = "失败";
                return ToJson(save);
            }

        }
        #endregion

        #region  // 保税 计费 收费规则保存
        [Route("api/bs/shoufeigz/save")]
        [System.Web.Http.HttpPost]
        public object bsShoufeigzSave([FromBody]dynamic obj)
        {
            int userId = Convert.ToInt32(obj.userId);
            string guid = obj.guid;
            C_DIC_GUIZE model = null;
            if (string.IsNullOrEmpty(guid))
            {
                model = new C_DIC_GUIZE();
                model.Guid = Guid.NewGuid().ToString();
            }
            else
            {
                model = db.C_DIC_GUIZE.Find(guid);
            }
            model.GoodsBillId = obj.goodsBillId;
            model.Type = obj.type;
            model.FeiMuZhongLei = obj.FeiMuZhongLei;
            model.Time1 = StrToDecimal(obj.Time1.ToString());
            model.Time2 = StrToDecimal(obj.Time2.ToString());
            model.FeiLv = obj.FeiLv;
            if (!string.IsNullOrEmpty(obj.Time_start.ToString()))
            {
                model.Time_start = obj.Time_start;
            }
            if (!string.IsNullOrEmpty(obj.Time_end.ToString()))
            {
                model.Time_end = obj.Time_end;
            }
            model.Num = StrToDecimal(obj.Num.ToString());


            Api_common save = new Api_common();
            db.C_DIC_GUIZE.AddOrUpdate(model);
            int c = db.SaveChanges();
            if (c > 0)
            {
                save.code = "0";
                save.msg = "提交成功";
                return ToJson(save);
            }
            else
            {
                if (c == 0)
                {
                    save.code = "1";
                    save.msg = "无更新";
                    return ToJson(save);
                }
                save.code = "2";
                save.msg = "失败";
                return ToJson(save);
            }

        }
        #endregion

        #region  // 保税 委托 完工/取消完工
        [Route("api/bs/weituo/finish")]
        [System.Web.Http.HttpPost]
        public object bsWeituoFinish([FromBody]dynamic obj)
        {
            int userId = Convert.ToInt32(obj.userId);
            Sys_User loginModel = db.Sys_User.FirstOrDefault(n => n.ID == userId && n.state == "激活");
            int id = obj.id;
            string state = obj.state;
            C_TB_HC_CONSIGN model_consign = db.C_TB_HC_CONSIGN.Find(id) ?? new C_TB_HC_CONSIGN();
            C_TB_HC_GOODSBILL model_goodsbill = db.C_TB_HC_GOODSBILL.Find(model_consign.GOODSBILL_ID) ?? new C_TB_HC_GOODSBILL();
            List<C_TB_HC_CONSIGN> list_Consign = db.C_TB_HC_CONSIGN.Where(n => n.GOODSBILL_ID == model_goodsbill.ID).OrderByDescending(n => n.ID).ToList();
            Api_common save = new Api_common();
            try
            {
                foreach (var items_consign in list_Consign)
                {
                    List<C_TB_HS_TALLYBILL> list_TallBill = db.C_TB_HS_TALLYBILL.Where(n => n.CONSIGN_ID == items_consign.ID).OrderByDescending(n => n.ID).ToList();
                    foreach (var items in list_TallBill)
                    {
                        items.State = state;
                        items.Shr = loginModel.userName;
                        db.Set<C_TB_HS_TALLYBILL>().AddOrUpdate(items);
                        db.SaveChanges();
                    }
                    items_consign.State = state;
                    db.SaveChanges();
                }
                save.code = "0";
                save.msg = "提交成功";
                return ToJson(save);
            }
            catch (Exception e)
            {
                save.code = "1";
                save.msg = "数据有误";
                return ToJson(save);
            }

        }
        #endregion



    }
}

