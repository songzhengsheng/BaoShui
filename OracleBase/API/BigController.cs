using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Transactions;
using System.Web.Http;
using MainBLL.Big;
using NFine.Code;
using OracleBase.Areas.ck.Controllers;
using OracleBase.Models;
using OracleBase.UserDefinModel;

namespace OracleBase.API
{
    public class BigController : ApiController
    {
        private Entities db = new Entities();

        #region 获取园区列表

        /// <summary>
        /// 获取园区列表
        /// </summary>
        /// <returns></returns>
        [System.Web.Http.Route("big/GetYuanQuList")]
        [System.Web.Http.HttpGet]
        public object GetYuanQuList()
        {
            var list = db.Set<C_Dic_YuanQu>().OrderByDescending(n => n.ID).AsQueryable();
            return Json(list);
        }

        #endregion

        #region 获取所有货代公司信息
        /// <summary>
        /// 获取所有货代公司信息
        /// </summary>
        /// <returns></returns>
        [System.Web.Http.Route("big/GetHuoDaiList")]
        [System.Web.Http.HttpGet]
        public object GetHuoDaiList()
        {
            var list = db.Set<C_GOODSAGENT>().OrderByDescending(n => n.ID).AsQueryable();
            return Json(list);
        }


        #endregion

        #region 确认货物信息接口    
        [System.Web.Http.Route("big/AddSure2")]
        [System.Web.Http.HttpPost]
        public object PostAddSure(string Entity)
        {
            B_TB_Sure model = Newtonsoft.Json.JsonConvert.DeserializeObject<B_TB_Sure>(Entity);
            try
            {
                //B_TB_Sure model=new B_TB_Sure();
                //model.NWM = Entity.NWM;
                //model.HUODAI = Entity.HUODAI;
                //model.PIECEWEIGHT = Entity.PIECEWEIGHT;
                //model.VGNO = Entity.VGNO;
                //model.BLNO = Entity.BLNO;
                //model.MARK_GOOGSBILLTYPE = Entity.MARK_GOOGSBILLTYPE;
                //model.MARK = Entity.MARK;
                //model.HETONGHAO = Entity.HETONGHAO;
           
                //model.CreatPeople = Entity.CreatPeople;
                //model.GoodsName = Entity.GoodsName;
                //model.YuanQuID = Entity.YuanQuID;
                //model.ShipName = Entity.ShipName;
                //model.GoodsType = Entity.GoodsType;
                //model.YuanQuName = Entity.YuanQuName;
                //model.ShuLiang = Entity.ShuLiang;
                //model.CreatPeopleID = Entity.CreatPeopleID;


                model.GUID = Guid.NewGuid().ToString();     
                model.CreatTime=DateTime.Now;
                model.State ="未处理";
                db.B_TB_Sure.AddOrUpdate(model);
                int c = db.SaveChanges();
                if (c > 0)
                {
                    return Json(new AjaxResult { state = ResultType.success.ToString(), message = "成功！", data = model.GUID });
                }
                else
                {
                    return Json(new AjaxResult { state = ResultType.error.ToString(), message = "失败！" });
                }
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            {
                var msg = string.Empty;
                var errors = (from u in ex.EntityValidationErrors select u.ValidationErrors).ToList();
                foreach (var item in errors)
                    msg += item.FirstOrDefault().ErrorMessage;
                return Json(new AjaxResult { state = ResultType.error.ToString(), message = msg });
            }
            catch (Exception e)
            {
                return Json(new AjaxResult { state = ResultType.error.ToString(), message = e.Message });
            }
  

        }

        //http://localhost:55763/big/GetMySure2?take=10&skip=0&creatPeopleId=tjid002
        [System.Web.Http.Route("big/GetMySure")]
        public object GetSureList(int take, int skip, string creatPeopleId)
        {

            var wherelambda = ExtLinq.True<B_TB_Sure>();
            wherelambda = wherelambda.And(t => t.CreatPeopleID == creatPeopleId);
            var list = db.Set<B_TB_Sure>().Where(wherelambda).OrderByDescending(n => n.CreatTime).AsQueryable();
            int total = list.Count();
            object rows = list.Skip(skip).Take(take).AsQueryable();
            return Json(new { total = total, rows = rows });

        }
        [System.Web.Http.Route("big/AddSure")]
        [System.Web.Http.HttpPost]
        public object PostAddSure2(SureEntity Entity)
        {
          
            try
            {
               // B_TB_Sure model = Newtonsoft.Json.JsonConvert.DeserializeObject<B_TB_Sure>(Entity);
                B_TB_Sure model = new B_TB_Sure();
                model.NWM = Entity.NWM;
                model.HUODAI = Entity.HUODAI;
                model.PIECEWEIGHT = Entity.PIECEWEIGHT;
                model.VGNO = Entity.VGNO;
                model.BLNO = Entity.BLNO;
                model.MARK_GOOGSBILLTYPE = Entity.MARK_GOOGSBILLTYPE;
                model.MARK = Entity.MARK;
                model.HETONGHAO = Entity.HETONGHAO;
                model.Cdbh = Entity.Cdbh;
                model.CreatPeople = Entity.CreatPeople;
                model.GoodsName = Entity.GoodsName;
                model.YuanQuID = Entity.YuanQuID;
                model.ShipName = Entity.ShipName;
                model.GoodsType = Entity.GoodsType;
                model.YuanQuName = Entity.YuanQuName;
                model.ShuLiang = Entity.ShuLiang;
                model.CreatPeopleID = Entity.CreatPeopleID;


                model.GUID = Guid.NewGuid().ToString();
                model.CreatTime = DateTime.Now;
                model.State = "未处理";
                db.B_TB_Sure.AddOrUpdate(model);
                int c = db.SaveChanges();
                if (c > 0)
                {
                    return Json(new AjaxResult { state = ResultType.success.ToString(), message = "成功！", data = model.GUID });
                }
                else
                {
                    return Json(new AjaxResult { state = ResultType.error.ToString(), message = "失败！" });
                }
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            {
                var msg = string.Empty;
                var errors = (from u in ex.EntityValidationErrors select u.ValidationErrors).ToList();
                foreach (var item in errors)
                    msg += item.FirstOrDefault().ErrorMessage;
                return Json(new AjaxResult { state = ResultType.error.ToString(), message = msg });
            }
            catch (Exception e)
            {
                return Json(new AjaxResult { state = ResultType.error.ToString(), message = e.Message });
            }


        }

        /// <summary>
        /// 编辑仓单
        /// </summary>
        /// <param name="Entity"></param>
        /// <returns></returns>
        [System.Web.Http.Route("big/EditSure")]
        [System.Web.Http.HttpPost]
        public object PostEditSure(B_TB_Sure Entity)
        {

            try
            {
                if (Entity.State!="未处理")
                {
                    return Json(new AjaxResult { state = ResultType.error.ToString(), message = "仓单已审核，不能操作！" });
                }
                Entity.CreatTime=DateTime.Now;
                db.B_TB_Sure.AddOrUpdate(Entity);
                int c = db.SaveChanges();
                if (c > 0)
                {
                    return Json(new AjaxResult { state = ResultType.success.ToString(), message = "成功！" });
                }
                else
                {
                    return Json(new AjaxResult { state = ResultType.error.ToString(), message = "失败！" });
                }
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            {
                var msg = string.Empty;
                var errors = (from u in ex.EntityValidationErrors select u.ValidationErrors).ToList();
                foreach (var item in errors)
                    msg += item.FirstOrDefault().ErrorMessage;
                return Json(new AjaxResult { state = ResultType.error.ToString(), message = msg });
            }
            catch (Exception e)
            {
                return Json(new AjaxResult { state = ResultType.error.ToString(), message = e.Message });
            }


        }

        [System.Web.Http.Route("big/DelSure")]
        [System.Web.Http.HttpPost]
        public object DelSure(dynamic obj)
        {
            try
            {
                string guid = obj.guid;
                B_TB_Sure u = db.B_TB_Sure.Find(guid);
             
                if (u.State != "未处理")
                {
                    return Json(new AjaxResult { state = ResultType.error.ToString(), message = "仓单已审核，不能操作！" });
                }
                db.B_TB_Sure.Attach(u);
                db.B_TB_Sure.Remove(u);
                int c = db.SaveChanges();
                if (c > 0)
                {
                    return Json(new AjaxResult { state = ResultType.success.ToString(), message = "成功！" });
                }
                else
                {
                    return Json(new AjaxResult { state = ResultType.error.ToString(), message = "失败！" });
                }
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            {
                var msg = string.Empty;
                var errors = (from u in ex.EntityValidationErrors select u.ValidationErrors).ToList();
                foreach (var item in errors)
                    msg += item.FirstOrDefault().ErrorMessage;
                return Json(new AjaxResult { state = ResultType.error.ToString(), message = msg });
            }
            catch (Exception e)
            {
                return Json(new AjaxResult { state = ResultType.error.ToString(), message = e.Message });
            }


        }
        #endregion

        #region 货权转移

        [System.Web.Http.Route("big/AddZhaunYi")]
        [System.Web.Http.HttpPost]
        public object PostAddZhaunYi(B_TB_ZhuanYiInfo model)
        {
            try
            {
               model.GUID = Guid.NewGuid().ToString();
               // model.CreatTime = DateTime.Now;
                model.State = "未处理";
                db.B_TB_ZhuanYiInfo.AddOrUpdate(model);
                int c = db.SaveChanges();
                if (c > 0)
                {
                    return Json(new AjaxResult { state = ResultType.success.ToString(), message = "成功！", data = model.GUID });
                }
                else
                {
                    return Json(new AjaxResult { state = ResultType.error.ToString(), message = "失败！" });
                }
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            {
                var msg = string.Empty;
                var errors = (from u in ex.EntityValidationErrors select u.ValidationErrors).ToList();
                foreach (var item in errors)
                    msg += item.FirstOrDefault().ErrorMessage;
                return Json(new AjaxResult { state = ResultType.error.ToString(), message = msg });
            }
            catch (Exception e)
            {
                return Json(new AjaxResult { state = ResultType.error.ToString(), message = e.Message });
            }


        }

        [System.Web.Http.Route("big/DelSure")]
        [System.Web.Http.HttpPost]
        public object DelZhaunYi(dynamic obj)
        {
            try
            {
                string guid = obj.guid;
                B_TB_ZhuanYiInfo u = db.B_TB_ZhuanYiInfo.Find(guid);

                if (u.State != "未处理")
                {
                    return Json(new AjaxResult { state = ResultType.error.ToString(), message = "仓单已审核，不能操作！" });
                }
                db.B_TB_ZhuanYiInfo.Attach(u);
                db.B_TB_ZhuanYiInfo.Remove(u);
                int c = db.SaveChanges();
                if (c > 0)
                {
                    return Json(new AjaxResult { state = ResultType.success.ToString(), message = "成功！" });
                }
                else
                {
                    return Json(new AjaxResult { state = ResultType.error.ToString(), message = "失败！" });
                }
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            {
                var msg = string.Empty;
                var errors = (from u in ex.EntityValidationErrors select u.ValidationErrors).ToList();
                foreach (var item in errors)
                    msg += item.FirstOrDefault().ErrorMessage;
                return Json(new AjaxResult { state = ResultType.error.ToString(), message = msg });
            }
            catch (Exception e)
            {
                return Json(new AjaxResult { state = ResultType.error.ToString(), message = e.Message });
            }


        }

        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("big/GetZhaunYiList")]
        public object GetZhaunYiList(int take, int skip, string creatPeopleId)
        {
            var wherelambda = ExtLinq.True<B_TB_ZhuanYiInfo>();
            wherelambda = wherelambda.And(t => t.CreatPeopleID == creatPeopleId);
            var list = db.Set<B_TB_ZhuanYiInfo>().Where(wherelambda).OrderByDescending(n => n.CreatTime).AsQueryable();
            int total = list.Count();
            object rows = list.Skip(skip).Take(take).AsQueryable();
            return Json(new { total = total, rows = rows });
        }
        #endregion

        #region 出库申请

        [System.Web.Http.Route("big/AddCk")]
        [System.Web.Http.HttpPost]
        public object PostAddCk(B_TB_ChuKuShenQing model)
        {
            try
            {
                model.GUID = Guid.NewGuid().ToString();
                model.CreatTime = DateTime.Now;
                model.State = "未处理";
                db.B_TB_ChuKuShenQing.AddOrUpdate(model);
                int c = db.SaveChanges();
                if (c > 0)
                {
                    return Json(new AjaxResult { state = ResultType.success.ToString(), message = "成功！", data = model.GUID });
                }
                else
                {
                    return Json(new AjaxResult { state = ResultType.error.ToString(), message = "失败！" });
                }
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            {
                var msg = string.Empty;
                var errors = (from u in ex.EntityValidationErrors select u.ValidationErrors).ToList();
                foreach (var item in errors)
                    msg += item.FirstOrDefault().ErrorMessage;
                return Json(new AjaxResult { state = ResultType.error.ToString(), message = msg });
            }
            catch (Exception e)
            {
                return Json(new AjaxResult { state = ResultType.error.ToString(), message = e.Message });
            }


        }
        /// <summary>
        /// 编辑出库
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [System.Web.Http.Route("big/EditCk")]
        [System.Web.Http.HttpPost]
        public object PostEditCk(B_TB_ChuKuShenQing model)
        {         
            try
            {
                B_TB_ChuKuShenQing oldModel = db.B_TB_ChuKuShenQing.Find(model.GUID);
                if (oldModel != null && oldModel.State!= "未处理")
                {
                    return Json(new AjaxResult { state = ResultType.error.ToString(), message = "数据已处理不能修改！" });
                }
                model.CreatTime = DateTime.Now;
                model.State = "未处理";
                db.B_TB_ChuKuShenQing.AddOrUpdate(model);
                int c = db.SaveChanges();
                if (c > 0)
                {
                    return Json(new AjaxResult { state = ResultType.success.ToString(), message = "成功！", data = model.GUID });
                }
                else
                {
                    return Json(new AjaxResult { state = ResultType.error.ToString(), message = "失败！" });
                }
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            {
                var msg = string.Empty;
                var errors = (from u in ex.EntityValidationErrors select u.ValidationErrors).ToList();
                foreach (var item in errors)
                    msg += item.FirstOrDefault().ErrorMessage;
                return Json(new AjaxResult { state = ResultType.error.ToString(), message = msg });
            }
            catch (Exception e)
            {
                return Json(new AjaxResult { state = ResultType.error.ToString(), message = e.Message });
            }
        }

        [System.Web.Http.Route("big/DelCk")]
        [System.Web.Http.HttpPost]
        public object DelCk(dynamic obj)
        {
            try
            {
                string guid = obj.guid;
                B_TB_ChuKuShenQing u = db.B_TB_ChuKuShenQing.Find(guid);

                if (u.State != "未处理")
                {
                    return Json(new AjaxResult { state = ResultType.error.ToString(), message = "仓单已审核，不能操作！" });
                }
                db.B_TB_ChuKuShenQing.Attach(u);
                db.B_TB_ChuKuShenQing.Remove(u);
                int c = db.SaveChanges();
                if (c > 0)
                {
                    return Json(new AjaxResult { state = ResultType.success.ToString(), message = "成功！" });
                }
                else
                {
                    return Json(new AjaxResult { state = ResultType.error.ToString(), message = "失败！" });
                }
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            {
                var msg = string.Empty;
                var errors = (from u in ex.EntityValidationErrors select u.ValidationErrors).ToList();
                foreach (var item in errors)
                    msg += item.FirstOrDefault().ErrorMessage;
                return Json(new AjaxResult { state = ResultType.error.ToString(), message = msg });
            }
            catch (Exception e)
            {
                return Json(new AjaxResult { state = ResultType.error.ToString(), message = e.Message });
            }


        }
        #endregion

        #region 根据货代名称获取票货信息
        /// <summary>
        /// 根据货代名称获取票货信息
        /// </summary>
        /// <param name="take"></param>
        /// <param name="skip"></param>
        /// <param name="C_GOODSAGENT_ID">货代id</param>
        /// <returns></returns>
        /// http://58.241.235.76:8099/big/GoodBillList?take=5&skip=0&C_GOODSAGENT_ID=64&BLNO=&VGNO=&ShipName=
        [HttpGet]
        [System.Web.Http.Route("big/GoodBillList")]
        public object GoodBillList(int take, int skip,string BLNO, string VGNO, string ShipName,string Cgno)
        {
            GoodsBillController gc = new GoodsBillController();
            var wherelambda = ExtLinq.True<C_TB_HC_GOODSBILL>();
            if (!string.IsNullOrWhiteSpace(BLNO))
            {
                wherelambda = wherelambda.And(t => t.BLNO == BLNO);
            }
            if (!string.IsNullOrWhiteSpace(VGNO))
            {
                wherelambda = wherelambda.And(t => t.VGNO == VGNO);
            }
            if (!string.IsNullOrWhiteSpace(ShipName))
            {
                wherelambda = wherelambda.And(t => t.ShipName == ShipName);
            }
            if (!string.IsNullOrWhiteSpace(Cgno))
            {
                C_TB_HC_CONSIGN Model_CONSIGN = db.C_TB_HC_CONSIGN.FirstOrDefault(n => n.CGNO == Cgno);//查找委托
                if (Model_CONSIGN!=null)
                {
                    wherelambda = wherelambda.And(t => t.ID == Model_CONSIGN.GOODSBILL_ID);
                }
                
            }

           // wherelambda = wherelambda.And(t => t.C_GOODSAGENT_ID == C_GOODSAGENT_ID);
            var list = db.Set<C_TB_HC_GOODSBILL>().Where(wherelambda).OrderByDescending(n => n.CreatTime).AsQueryable();
            foreach (var items in list)
            {
                if (!string.IsNullOrWhiteSpace(Cgno))
                {
                    items.Cgno = Cgno;
                }
                items.KunCun = (gc.GetKuCun(items.ID.ToInt()).KunCun.ToDecimal() - items.SuoHuoKunCun.ToDecimal()).ToString();
                items.KunCunW = (gc.GetKuCun(items.ID.ToInt()).KunCunW.ToDecimal() - items.SuoHuoKunCunW.ToDecimal()).ToString();
            }
            int total = list.Count();
            object rows = list.Skip(skip).Take(take).AsQueryable();
            return Json(new { total = total, rows = rows });

        }

        #endregion

        #region 委托单添加
        /// <summary>
        /// 委托单添加
        /// </summary>
        /// <param name="model"></param>
        /// <param name="UserName">大宗平台登录人用户名</param>
        /// <param name="YuanquID">园区id</param>
        /// <returns></returns>
        [Route("big/AddConsign")]
        [HttpPost]
        public object AddConsign(string UserName,int YuanquID,int GOODSBILL_ID,DateTime WeiTuoTime,string PAPERYNO,decimal PLANWEIGHT,string jbr,string Phone,string BeiZhu,string MaiFangName,string MaiFangName2,string Ckdh)
        {

            C_TB_HC_GOODSBILL modelGoodsBill = db.C_TB_HC_GOODSBILL.Find(GOODSBILL_ID);
            if (modelGoodsBill == null)
            {
                return Json(new AjaxResult { state = ResultType.error.ToString(), message = "票货不存在！" });
            }
            C_TB_HC_CONSIGN model=new C_TB_HC_CONSIGN();
            model.GOODSBILL_ID = GOODSBILL_ID;
            model.WeiTuoTime = WeiTuoTime;
            model.PAPERYNO = PAPERYNO;
            model.PLANWEIGHT = PLANWEIGHT;
            model.jbr = jbr;
            model.Phone = Phone;
            model.BeiZhu = BeiZhu;
            model.MaiFangName = MaiFangName;
            model.MaiFangName2 = MaiFangName2;
            model.Ckdh = Ckdh;//出库单号


            model.GoodsBill_Num = modelGoodsBill.GBNO;//票货编码
            model.BLNO = modelGoodsBill.BLNO;//提单号
            model.WeiTuoRen = modelGoodsBill.C_GOODSAGENT_NAME;//委托人
            model.GoodsName = modelGoodsBill.C_GOODS;//货物名称
            model.ShipName = modelGoodsBill.ShipName;
            model.VGNO = modelGoodsBill.VGNO;//航次
            model.GoodsType = modelGoodsBill.GoodsType;//货物类型
            model.HengZhong = "否";//衡重
            string todayTime = DateTime.Today.ToString("yyyyMMdd");
                C_TB_HC_CONSIGN num = db.C_TB_HC_CONSIGN.OrderByDescending(n => n.CREATETIME).ToList().FirstOrDefault();
                if (num != null)
                {
                    if (!string.IsNullOrEmpty(num.CGNO))
                    {
                        if (num.CGNO.Substring(2, 8) == todayTime)
                        {
                            model.CGNO = "WT" + todayTime + (Convert.ToInt32(num.CGNO.Replace("WT" + todayTime, "")) + 1).ToString("0000");
                        }
                        else
                        {
                            model.CGNO = "WT" + todayTime + "0001";
                        }
                    }
                }
                else
                {
                    model.CGNO = "WT" + todayTime + "0001";
                }

                model.CODE_OPERATION = "场-汽";
                model.CREATORNAME = UserName;
                model.CREATETIME = DateTime.Now;
                model.YuanQuID = YuanquID;
                model.State = "待审核";
                model.LaiYuan = "货代";
             db.Set<C_TB_HC_CONSIGN>().AddOrUpdate(model);

            try
            {
                using (var scope = new TransactionScope(TransactionScopeOption.Required))
                {
                    db.SaveChanges();
                    List<C_TB_HC_CONSIGN> listConsign = db.C_TB_HC_CONSIGN.Where(n => n.GOODSBILL_ID == model.GOODSBILL_ID).OrderBy(n => n.ID).ToList();
                    if (listConsign.Count != 0)
                    {
                        modelGoodsBill.State = "已生成" + listConsign.Count + "条委托";

                    }
                    else
                    {
                        modelGoodsBill.State = "进行中";
                    }
                    db.Set<C_TB_HC_GOODSBILL>().AddOrUpdate(modelGoodsBill);
                    int c = db.SaveChanges();
                    if (c > 0)
                    {
                        scope.Complete();
                        return Json(new AjaxResult { state = ResultType.success.ToString(), message = "成功！", data = model.ID });
                    }
                    else
                    {
                        if (c == 0)
                        {
                            return Json(new AjaxResult { state = ResultType.success.ToString(), message = "成功！", data = model.ID });
                        }
                        return Json(new AjaxResult { state = ResultType.error.ToString(), message = "失败！" });
                    }
                }
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            {
                var msg = string.Empty;
                var errors = (from u in ex.EntityValidationErrors select u.ValidationErrors).ToList();
                foreach (var item in errors)
                    msg += item.FirstOrDefault().ErrorMessage;
                Log4NetHelper log = new Log4NetHelper();
                log.Error(msg, ex);
                return Json(msg);
            }
            catch (Exception ex)
            {
                Log4NetHelper log = new Log4NetHelper();
                log.Error(ex.Message, ex);
                return Json(ex.InnerException.InnerException.Message);
            }

        }
        #endregion

        #region 出库实提
        /// <summary>
        /// 出库实提
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [System.Web.Http.Route("big/EditCkSt")]
        [System.Web.Http.HttpPost]
        public bool PostEditCkSt(int id)
        {
            try
            {
                decimal? sl2ok = 0;//实提数量
                B_TB_SUREST model = new B_TB_SUREST();
                C_TB_HC_CONSIGN model_CONSIGN = db.C_TB_HC_CONSIGN.Find(id);
               
                List<C_TB_HS_TALLYBILL> list_TALLYBILL = db.C_TB_HS_TALLYBILL.Where(n => n.CONSIGN_ID == model_CONSIGN.ID).ToList();
                foreach (var items in list_TALLYBILL)
                {
                    sl2ok += items.WEIGHT;
                }
                model.CreatTime = DateTime.Now;
                model.ckdh = model_CONSIGN.Ckdh;
                model.sl2ok = sl2ok;
                model.sl2 = model_CONSIGN.PLANWEIGHT;
                model.remark = model_CONSIGN.BeiZhu;
                model.Guid = Guid.NewGuid().ToString();
                model.WeiTuoId = id;
                db.B_TB_SUREST.AddOrUpdate(model);
                int c = db.SaveChanges();
                bool state = ckSUREST(model.ckdh,model.sl2ok,model.sl2,model.remark);
                if (state == true)
                {
                    return true;
                }
                else
                {
                    return false;
                }
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
                return false;
            }
        }
        private bool ckSUREST(string ckdh, decimal? sl2ok, decimal? sl2, string remark)
        {
            string url = "http://lyg.steel56.com.cn/explatform/exp/thrid/ckInterface/ckDeliver.do";
            //  string guid = "483cc9dc-dcb6-4f41-a83b-36ef71bc03cd";
            url += string.Format("?ckdh={0}&&sl2ok={1}&&sl2={2}&&remark={3}", ckdh, sl2ok, sl2, remark);
            var r = HttpMethods.HttpPost(url);
            BigResult result = JsonHelper.JsonToObject<BigResult>(r);
            if (result.code == 0)
            {
                return true;
            }
            return false;
        }
        #endregion


        #region 货代注册仓单的时候，要在仓库进行锁货
        /// <summary>
        /// 货代注册仓单的时候，要在仓库进行锁货
        /// </summary>
        /// <param name="gbno">票货号</param>
        /// <param name="SuoHuoKunCunW">仓单注册的重量</param>
        /// <param name="SuoHuoKunCun">仓单注册的件数</param>
        /// <returns></returns>
        [Route("big/SuoHuo")]
        [HttpPost]
        public object SuoHuo(string gbno,double SuoHuoKunCunW,double SuoHuoKunCun)
        {
            C_TB_HC_GOODSBILL model = db.C_TB_HC_GOODSBILL.FirstOrDefault(n => n.GBNO == gbno);
            if (model==null)
            {
                return Json(new AjaxResult { state = ResultType.error.ToString(), message = "未找到该票货" });
            }
            try
            {
                GoodsBillController gc=new GoodsBillController();
                int id = model.ID.ToInt();
                C_TB_HC_GOODSBILL entry = gc.GetKuCun(id);
                string stringkunCun = entry.KunCun;
                string stringKunCunW = entry.KunCunW;
                double.TryParse(stringkunCun, out var  kunCun);
               double.TryParse(stringKunCunW, out var kunCunW);
              
                double suoHuoKunCun =model.SuoHuoKunCun.ToDouble()+ SuoHuoKunCun;
                double suoHuoKunCunW =model.SuoHuoKunCunW.ToDouble() + SuoHuoKunCunW;
                if (suoHuoKunCun > kunCun)
                {
                    return Json(new AjaxResult { state = ResultType.error.ToString(), message = "失败！锁定库存件数大于剩余" });
                }
                if (suoHuoKunCunW > kunCunW)
                {
                    return Json(new AjaxResult { state = ResultType.error.ToString(), message = "失败！锁定库存吨数/材积大于剩余" });
                }

                C_TB_HC_GOODSBILL m = db.C_TB_HC_GOODSBILL.FirstOrDefault(n => n.GBNO == model.GBNO);
                if (m != null)
                {
                    m.SuoHuoKunCun = suoHuoKunCun.ToDecimal();
                    m.SuoHuoKunCunW = suoHuoKunCunW.ToDecimal();
                }

                int c = db.SaveChanges();
                if (c > 0)
                {
                    return Json(new AjaxResult { state = ResultType.success.ToString(), message = "成功！" });
                }
                else
                {
                    if (c == 0)
                    {
                        return Json(new AjaxResult { state = ResultType.warning.ToString(), message = "无更新" });
                    }
                    return Json(new AjaxResult { state = ResultType.error.ToString(), message = "失败！" });
                }
            }
            catch (Exception e)
            {
                return Json(new AjaxResult { state = ResultType.error.ToString(), message = e.Message });

            }

        }
        #endregion

    }
}