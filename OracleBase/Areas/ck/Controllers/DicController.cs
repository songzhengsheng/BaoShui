using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Migrations;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;
using Main.HelpClass;
using MainBLL.ToOut;
using Newtonsoft.Json;
using NFine.Code;
using NFine.Code.Datatable;
using NFine.Code.Select;

using OracleBase.HelpClass;
using OracleBase.Models;
using OracleBase.ServiceReference1;

namespace OracleBase.Areas.ck.Controllers
{
    [SignLoginAuthorize]
    public class DicController : Controller
    {
        private Entities db = new Entities();
        //private string codeuser = "FICCGS";
        //private string password = "987654";
        //private string GsCode = "6377";

        #region 园区

        public ActionResult YuanQuList()
        {
            return View();
        }

        public ActionResult Add_Dic_YaunQu(int id)
        {
            C_Dic_YuanQu model = db.C_Dic_YuanQu.Find(id) ?? new C_Dic_YuanQu();
            return View(model);
        }
        [HttpPost]
        public JsonResult Del_Dic_YaunQu(string datalist)
        {
            List<C_Dic_YuanQu> list = JsonConvert.DeserializeObject<List<C_Dic_YuanQu>>(datalist);

            EFHelpler<C_Dic_YuanQu> ef = new EFHelpler<C_Dic_YuanQu>();
            ef.delete(list.ToArray());
            return Json(new AjaxResult { state = ResultType.success.ToString(), message = "成功！" }.ToJson());

        }
        [HttpPost]
        public JsonResult Del_Dic_C_GOODSAGENT(string datalist)
        {
            List<C_GOODSAGENT> list = JsonConvert.DeserializeObject<List<C_GOODSAGENT>>(datalist);
            EFHelpler<C_GOODSAGENT> ef = new EFHelpler<C_GOODSAGENT>();
            ef.delete(list.ToArray());
            return Json(new AjaxResult { state = ResultType.success.ToString(), message = "成功！" }.ToJson());

        }
        #endregion

        #region 货物
        public ActionResult GoodsList()
        {
            return View();
        }
        [HttpGet]
        public object GetGoodsList(int limit, int offset, string GoodsName)
        {
            var wherelambda = ExtLinq.True<C_GOODS>();
            if (!string.IsNullOrEmpty(GoodsName))
            {
                wherelambda = wherelambda.And(t => t.GoodsName.Contains(GoodsName));
            }
            var list = db.Set<C_GOODS>().Where(wherelambda).OrderByDescending(n => n.ID).AsQueryable();
            int total = list.Count();
            object rows = list.Skip(offset).Take(limit).AsQueryable();
            return Json(new { total = total, rows = rows }, JsonRequestBehavior.AllowGet);

        }
       

        public ActionResult Add_C_Goods(int id)
        {
           C_GOODS model = db.C_GOODS.Find(id) ?? new C_GOODS();
            return View(model);
        }
        [HttpPost]
        public JsonResult Del_Dic_Goods(string datalist)
        {
            List<C_GOODS> list = JsonConvert.DeserializeObject<List<C_GOODS>>(datalist);

            EFHelpler<C_GOODS> ef = new EFHelpler<C_GOODS>();
            ef.delete(list.ToArray());
            return Json(new AjaxResult { state = ResultType.success.ToString(), message = "成功！" }.ToJson());

        }

        public ActionResult QueryCargoPage()
        {
            return View();
        }
        [HttpGet]
        public object QueryCargo(int limit, int offset, string Name)
        {
            OperatorModel loginModel = OperatorProvider.Provider.GetCurrent();
            C_Dic_YuanQu model_YuanQu = db.C_Dic_YuanQu.Find(loginModel.YuanquID) ?? new C_Dic_YuanQu();
            ServiceRBillSoap s = new ServiceRBillSoapClient();

            DataTable dt = s.QueryCargo(model_YuanQu.codeuser, model_YuanQu.password).Tables[0];
            DataView dv = dt.DefaultView;
            dv.RowFilter = "NAME LIKE '%" + Name + "%'";

            DataTable newTable1 = dv.ToTable();

            int total = newTable1.Rows.Count;
            newTable1 = newTable1.AsEnumerable().Skip(offset).Take(limit).CopyToDataTable<DataRow>();

            List<FiKeHuData> list = ExtendMethod.ToDataList<FiKeHuData>(newTable1);

            object rows = list.AsQueryable();
            return Json(new { total = total, rows = rows }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult AddFICargoToGoods(string datalist)
        {
            try
            {
                var loginModel = OperatorProvider.Provider.GetCurrent();
                List<FiKeHuData> list = JsonConvert.DeserializeObject<List<FiKeHuData>>(datalist);
                List<C_GOODS> cList = new List<C_GOODS>();
                foreach (var l in list)
                {
                    C_GOODS entry = new C_GOODS()
                    {
                        Code = l.CODE,
                        GoodsName = l.NAME,
                        Sjm = l.LOGOGRAM,
                        FullName = l.FULLNAME,
                        creater = loginModel.UserName,
                        YuanQuID = loginModel.YuanquID,
                        creatTime = DateTime.Now
                    };
                    cList.Add(entry);
                }
                EFHelpler<C_GOODS> ef = new EFHelpler<C_GOODS>();
                ef.add(cList.ToArray());
                return Json(new AjaxResult { state = ResultType.success.ToString(), message = "成功！" }.ToJson());
            }
            catch (Exception e)
            {
                return Json(new AjaxResult { state = ResultType.error.ToString(), message = e.Message }.ToJson());
                throw;
            }


        }
        #endregion

        #region 费目管理
        public ActionResult C_ChargePage()
        {
            return View();
        }
        [HttpGet]
        public object GetC_ChargeList(int limit, int offset, string Name)
        {
            var wherelambda = ExtLinq.True<C_Charge>();
            if (!string.IsNullOrEmpty(Name))
            {
                wherelambda = wherelambda.And(t => t.Name.Contains(Name));
            }
            var list = db.Set<C_Charge>().Where(wherelambda).OrderByDescending(n => n.creatTime).AsQueryable();
            int total = list.Count();
            object rows = list.Skip(offset).Take(limit).AsQueryable();
            return Json(new { total = total, rows = rows }, JsonRequestBehavior.AllowGet);

        }
        public ActionResult QueryChargePage()
        {
            return View();
        }
        [HttpGet]
        public object QueryCharge(int limit, int offset, string Name)
        {
            OperatorModel loginModel = OperatorProvider.Provider.GetCurrent();
            C_Dic_YuanQu model_YuanQu = db.C_Dic_YuanQu.Find(loginModel.YuanquID) ?? new C_Dic_YuanQu();
            ServiceRBillSoap s = new ServiceRBillSoapClient();

            DataTable dt = s.QueryChargeCompany(model_YuanQu.codeuser, model_YuanQu.password, model_YuanQu.CODECOMPANY).Tables[0];
            DataView dv = dt.DefaultView;
            dv.RowFilter = "NAME LIKE '%" + Name + "%'";

            DataTable newTable1 = dv.ToTable();

            int total = newTable1.Rows.Count;
            newTable1 = newTable1.AsEnumerable().Skip(offset).Take(limit).CopyToDataTable<DataRow>();

            List<FiKeHuData> list = ExtendMethod.ToDataList<FiKeHuData>(newTable1);

            object rows = list.AsQueryable();
            return Json(new { total = total, rows = rows }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult AddCharge(string datalist)
        {
            try
            {
                var loginModel = OperatorProvider.Provider.GetCurrent();
                List<FiKeHuData> list = JsonConvert.DeserializeObject<List<FiKeHuData>>(datalist);
                List<C_Charge> cList = new List<C_Charge>();
                foreach (var l in list)
                {
                    C_Charge entry = new C_Charge()
                    {
                        CODE = l.CODE,
                        Name = l.NAME,
                        LOGOGRAM = l.LOGOGRAM,
                        FULLNAME = l.FULLNAME,
                        INVOICENAME = l.INVOICENAME,
                        creater = loginModel.UserName,
                        YuanQuID = loginModel.YuanquID,
                        creatTime = DateTime.Now,
                        GUID = Guid.NewGuid().ToString()
                    };
                    cList.Add(entry);
                }
                EFHelpler<C_Charge> ef = new EFHelpler<C_Charge>();
                ef.add(cList.ToArray());
                return Json(new AjaxResult { state = ResultType.success.ToString(), message = "成功！" }.ToJson());
            }
            catch (Exception e)
            {
                return Json(new AjaxResult { state = ResultType.error.ToString(), message = e.Message }.ToJson());
                throw;
            }


        }

        [HttpPost]
        public JsonResult DelCharge(string datalist)
        {
            List<C_Charge> list = JsonConvert.DeserializeObject<List<C_Charge>>(datalist);

            EFHelpler<C_Charge> ef = new EFHelpler<C_Charge>();
            ef.delete(list.ToArray());
            return Json(new AjaxResult { state = ResultType.success.ToString(), message = "成功！" }.ToJson());

        }
        #endregion

        //计量单位
        public ActionResult JiLiangList()
        {
            return View();
        }
        [HttpGet]
        public object GetJiLiangList(int limit, int offset, string Name)
        {
            var wherelambda = ExtLinq.True<C_DIC_JILIANG>();
            if (!string.IsNullOrEmpty(Name))
            {
                wherelambda = wherelambda.And(t => t.Name.Contains(Name));
            }
            var list = db.Set<C_DIC_JILIANG>().Where(wherelambda).OrderByDescending(n => n.ID).AsQueryable();
            int total = list.Count();
            object rows = list.Skip(offset).Take(limit).AsQueryable();
            return Json(new { total = total, rows = rows }, JsonRequestBehavior.AllowGet);

        }
        [HttpPost]
        public JsonResult Del_Dic_JiLiang(string datalist)
        {
            List<C_DIC_JILIANG> list = JsonConvert.DeserializeObject<List<C_DIC_JILIANG>>(datalist);

            EFHelpler<C_DIC_JILIANG> ef = new EFHelpler<C_DIC_JILIANG>();
            ef.delete(list.ToArray());
            return Json(new AjaxResult { state = ResultType.success.ToString(), message = "成功！" }.ToJson());

        }
        public ActionResult Add_C_JiLiang(int id)
        {
            var loginModel = OperatorProvider.Provider.GetCurrent();
            C_DIC_JILIANG model = db.C_DIC_JILIANG.Find(id) ?? new C_DIC_JILIANG();
            ViewBag.creater = loginModel.UserName;
            ViewBag.creatTime = DateTime.Now;
            return View(model);
        }
        //进出口
        public ActionResult InOutList()
        {
            return View();
        }
        [HttpGet]
        public object GetInOutList(int limit, int offset, string Name)
        {
            var wherelambda = ExtLinq.True<C_TB_CODE_INOUT>();
            if (!string.IsNullOrEmpty(Name))
            {
                wherelambda = wherelambda.And(t => t.Name.Contains(Name));
            }
            var list = db.Set<C_TB_CODE_INOUT>().Where(wherelambda).OrderByDescending(n => n.ID).AsQueryable();
            int total = list.Count();
            object rows = list.Skip(offset).Take(limit).AsQueryable();
            return Json(new { total = total, rows = rows }, JsonRequestBehavior.AllowGet);

        }
        [HttpPost]
        public JsonResult Del_Dic_InOut(string datalist)
        {
            List<C_TB_CODE_INOUT> list = JsonConvert.DeserializeObject<List<C_TB_CODE_INOUT>>(datalist);

            EFHelpler<C_TB_CODE_INOUT> ef = new EFHelpler<C_TB_CODE_INOUT>();
            ef.delete(list.ToArray());
            return Json(new AjaxResult { state = ResultType.success.ToString(), message = "成功！" }.ToJson());

        }
        public ActionResult Add_C_InOut(int id)
        {
            C_TB_CODE_INOUT model = db.C_TB_CODE_INOUT.Find(id) ?? new C_TB_CODE_INOUT();
            return View(model);
        }
        //内外贸
        public ActionResult TRADEList()
        {
            return View();
        }
        [HttpGet]
        public object GetTRADEList(int limit, int offset, string Name)
        {
            var wherelambda = ExtLinq.True<C_TB_CODE_TRADE>();
            if (!string.IsNullOrEmpty(Name))
            {
                wherelambda = wherelambda.And(t => t.Name.Contains(Name));
            }
            var list = db.Set<C_TB_CODE_TRADE>().Where(wherelambda).OrderByDescending(n => n.ID).AsQueryable();
            int total = list.Count();
            object rows = list.Skip(offset).Take(limit).AsQueryable();
            return Json(new { total = total, rows = rows }, JsonRequestBehavior.AllowGet);

        }
        [HttpPost]
        public JsonResult Del_Dic_TRADE(string datalist)
        {
            List<C_TB_CODE_TRADE> list = JsonConvert.DeserializeObject<List<C_TB_CODE_TRADE>>(datalist);

            EFHelpler<C_TB_CODE_TRADE> ef = new EFHelpler<C_TB_CODE_TRADE>();
            ef.delete(list.ToArray());
            return Json(new AjaxResult { state = ResultType.success.ToString(), message = "成功！" }.ToJson());

        }
        public ActionResult Add_C_TRADE(int id)
        {
            C_TB_CODE_TRADE model = db.C_TB_CODE_TRADE.Find(id) ?? new C_TB_CODE_TRADE();
            return View(model);
        }
        //包装
        public ActionResult PACKList()
        {
            return View();
        }
        [HttpGet]
        public object GetPACKList(int limit, int offset, string Name)
        {
            var wherelambda = ExtLinq.True<C_TB_CODE_PACK>();
            if (!string.IsNullOrEmpty(Name))
            {
                wherelambda = wherelambda.And(t => t.Name.Contains(Name));
            }
            var list = db.Set<C_TB_CODE_PACK>().Where(wherelambda).OrderByDescending(n => n.ID).AsQueryable();
            int total = list.Count();
            object rows = list.Skip(offset).Take(limit).AsQueryable();
            return Json(new { total = total, rows = rows }, JsonRequestBehavior.AllowGet);

        }
        [HttpPost]
        public JsonResult Del_Dic_PACK(string datalist)
        {
            List<C_TB_CODE_PACK> list = JsonConvert.DeserializeObject<List<C_TB_CODE_PACK>>(datalist);

            EFHelpler<C_TB_CODE_PACK> ef = new EFHelpler<C_TB_CODE_PACK>();
            ef.delete(list.ToArray());
            return Json(new AjaxResult { state = ResultType.success.ToString(), message = "成功！" }.ToJson());

        }
        public ActionResult Add_C_PACK(int id)
        {
            C_TB_CODE_PACK model = db.C_TB_CODE_PACK.Find(id) ?? new C_TB_CODE_PACK();
            return View(model);
        }
        //航次
        public ActionResult VOYAGEList()
        {
            return View();
        }
        [HttpGet]
        public object GetVOYAGEList(int limit, int offset, string Name)
        {
            var wherelambda = ExtLinq.True<C_TB_CODE_VOYAGE>();
            if (!string.IsNullOrEmpty(Name))
            {
                wherelambda = wherelambda.And(t => t.Name.Contains(Name));
            }
            var list = db.Set<C_TB_CODE_VOYAGE>().Where(wherelambda).OrderByDescending(n => n.ID).AsQueryable();
            int total = list.Count();
            object rows = list.Skip(offset).Take(limit).AsQueryable();
            return Json(new { total = total, rows = rows }, JsonRequestBehavior.AllowGet);

        }
        [HttpPost]
        public JsonResult Del_Dic_VOYAGE(string datalist)
        {
            List<C_TB_CODE_VOYAGE> list = JsonConvert.DeserializeObject<List<C_TB_CODE_VOYAGE>>(datalist);

            EFHelpler<C_TB_CODE_VOYAGE> ef = new EFHelpler<C_TB_CODE_VOYAGE>();
            ef.delete(list.ToArray());
            return Json(new AjaxResult { state = ResultType.success.ToString(), message = "成功！" }.ToJson());

        }
        public ActionResult Add_C_VOYAGE(int id)
        {
            C_TB_CODE_VOYAGE model = db.C_TB_CODE_VOYAGE.Find(id) ?? new C_TB_CODE_VOYAGE();
            return View(model);
        }

        /// <summary>
        /// 操作类型字典表
        /// </summary>
        /// <returns></returns>
        public ActionResult CaoZuoList()
        {
            return View();
        }
        [HttpGet]
        public object GetCaoZuoListList(int limit, int offset, string NAME)
        {
            var wherelambda = ExtLinq.True<C_TB_CODE_CAOZUO>();
            if (!string.IsNullOrEmpty(NAME))
            {
                wherelambda = wherelambda.And(t => t.NAME.Contains(NAME));
            }
            var list = db.Set<C_TB_CODE_CAOZUO>().Where(wherelambda).OrderByDescending(n => n.ID).AsQueryable();
            int total = list.Count();
            object rows = list.Skip(offset).Take(limit).AsQueryable();
            return Json(new { total = total, rows = rows }, JsonRequestBehavior.AllowGet);

        }

        /// <summary>
        /// 作业方式
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult AddCaoZuo(int id)
        {
            C_TB_CODE_CAOZUO model=db.C_TB_CODE_CAOZUO.Find(id)??new C_TB_CODE_CAOZUO();
            return View(model);
        }
        [HttpPost]
        public JsonResult Del_CODE_CAOZUO(string datalist)
        {
            List<C_TB_CODE_CAOZUO> list = JsonConvert.DeserializeObject<List<C_TB_CODE_CAOZUO>>(datalist);
            EFHelpler<C_TB_CODE_CAOZUO> ef = new EFHelpler<C_TB_CODE_CAOZUO>();
            try
            {
                ef.delete(list.ToArray());
                return Json(new AjaxResult { state = ResultType.success.ToString(), message = "成功！" }.ToJson());

            }
            catch (Exception e)
            {
                return Json(new AjaxResult { state = ResultType.success.ToString(), message =e.Message }.ToJson());

            }


        }
        //客户
        public ActionResult CUSTOMERList()
        {
            return View();
        }
        [HttpGet]
        public object GetCUSTOMERList(int limit, int offset, string Name)
        {
            OperatorModel loginModel = OperatorProvider.Provider.GetCurrent();
            var wherelambda = ExtLinq.True<C_TB_CODE_CUSTOMER>();
            if (!string.IsNullOrEmpty(Name))
            {
                wherelambda = wherelambda.And(t => t.Name.Contains(Name));
            }
            wherelambda = wherelambda.And(t => t.YuanQuID==loginModel.YuanquID);
            var list = db.Set<C_TB_CODE_CUSTOMER>().Where(wherelambda).OrderByDescending(n => n.ID).AsQueryable();
            int total = list.Count();
            object rows = list.Skip(offset).Take(limit).AsQueryable();
            return Json(new { total = total, rows = rows }, JsonRequestBehavior.AllowGet);

        }


        public ActionResult QueryClientPage()
        {
            return View();
        }
   
        [HttpGet]
        public object QueryClient(int limit, int offset,string Name)
        {
            OperatorModel loginModel = OperatorProvider.Provider.GetCurrent();
            C_Dic_YuanQu model_YuanQu = db.C_Dic_YuanQu.Find(loginModel.YuanquID) ?? new C_Dic_YuanQu();
            ServiceRBillSoap s = new ServiceRBillSoapClient();

            DataTable dt = s.QueryClient(model_YuanQu.codeuser, model_YuanQu.password).Tables[0];
            DataView dv = dt.DefaultView;
            dv.RowFilter = "NAME LIKE '%" + Name + "%'";

            DataTable newTable1 = dv.ToTable();

            int total = newTable1.Rows.Count;
            newTable1 = newTable1.AsEnumerable().Skip(offset).Take(limit).CopyToDataTable<DataRow>();
       
            List<FiKeHuData> list= ExtendMethod.ToDataList<FiKeHuData>(newTable1);

            object rows = list.AsQueryable();
            return Json(new { total = total, rows = rows }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult AddFIKeHuToCUSTOMER(string datalist)
        {
            try
            {
                var loginModel = OperatorProvider.Provider.GetCurrent();
                List<FiKeHuData> list = JsonConvert.DeserializeObject<List<FiKeHuData>>(datalist);
                List<C_TB_CODE_CUSTOMER> CUSTOMERList = new List<C_TB_CODE_CUSTOMER>();
                foreach (var l in list)
                {
                    C_TB_CODE_CUSTOMER entry = new C_TB_CODE_CUSTOMER()
                    {
                        Code = l.CODE,
                        Name = l.NAME,
                        Sjm = l.LOGOGRAM,
                        ReMark = l.FULLNAME,
                        creater = loginModel.UserName,
                        YuanQuID = loginModel.YuanquID,
                        creatTime = DateTime.Now
                    };
                    CUSTOMERList.Add(entry);
                }
                EFHelpler<C_TB_CODE_CUSTOMER> ef = new EFHelpler<C_TB_CODE_CUSTOMER>();
                ef.add(CUSTOMERList.ToArray());
                return Json(new AjaxResult { state = ResultType.success.ToString(), message = "成功！" }.ToJson());
            }
            catch (Exception e)
            {
                return Json(new AjaxResult { state = ResultType.error.ToString(), message = e.Message }.ToJson());
                throw;
            }


        }
        [HttpPost]
        public JsonResult Del_Dic_CUSTOMER(string datalist)
        {
            List<C_TB_CODE_CUSTOMER> list = JsonConvert.DeserializeObject<List<C_TB_CODE_CUSTOMER>>(datalist);

            EFHelpler<C_TB_CODE_CUSTOMER> ef = new EFHelpler<C_TB_CODE_CUSTOMER>();
            ef.delete(list.ToArray());
            return Json(new AjaxResult { state = ResultType.success.ToString(), message = "成功！" }.ToJson());

        }
        public ActionResult Add_C_CUSTOMER(int id)
        {
            C_TB_CODE_CUSTOMER model = db.C_TB_CODE_CUSTOMER.Find(id) ?? new C_TB_CODE_CUSTOMER();
            return View(model);
        }
        [HttpPost]
        public ActionResult Add_C_CUSTOMER(C_TB_CODE_CUSTOMER model)
        {
            var loginModel = OperatorProvider.Provider.GetCurrent();
            model.YuanQuID = loginModel.YuanquID;
            model.creater = loginModel.UserName;
            model.creatTime = DateTime.Now;
            db.C_TB_CODE_CUSTOMER.AddOrUpdate(model);
          int c=  db.SaveChanges();
            if (c>0)
            {
                return Json(new AjaxResult { state = ResultType.success.ToString(), message = "成功！" }.ToJson());
            }
            return Json(new AjaxResult { state = ResultType.error.ToString(), message = "失败！" }.ToJson());
       
        }

        public ActionResult UpLoadProcess(string id, string name, string type, string lastModifiedDate, int size, HttpPostedFileBase file)
        {
            try
            {
                string localPath = Path.Combine(HttpRuntime.AppDomainAppPath, "Upload");
                if (Request.Files.Count == 0)
                {
                    return Json(new { jsonrpc = 2.0, error = new { code = 102, message = "保存失败" }, id = "id" });
                }

                string ex = Path.GetExtension(file.FileName);
                var filePathName = Guid.NewGuid().ToString("N") + ex;
                if (!System.IO.Directory.Exists(localPath))
                {
                    System.IO.Directory.CreateDirectory(localPath);
                }
                file.SaveAs(Path.Combine(localPath, filePathName));

                return Json(new
                {
                    jsonrpc = "2.0",
                    id = id,
                    filePath = filePathName
                });
            }
            catch (Exception exception)
            {
                Log4NetHelper log = new Log4NetHelper();
                log.Info(exception.Message, exception);
                throw;
            }


        }
        //货物类型
        public ActionResult GoodsTypeList()
        {
            return View();
        }
        [HttpGet]
        public object GetGoodsTypeList(int limit, int offset, string GoodsName)
        {
            var wherelambda = ExtLinq.True<C_GOODSTYPE>();
            if (!string.IsNullOrEmpty(GoodsName))
            {
                wherelambda = wherelambda.And(t => t.GoodsName.Contains(GoodsName));
            }
            var list = db.Set<C_GOODSTYPE>().Where(wherelambda).OrderByDescending(n => n.ID).AsQueryable();
            int total = list.Count();
            object rows = list.Skip(offset).Take(limit).AsQueryable();
            return Json(new { total = total, rows = rows }, JsonRequestBehavior.AllowGet);

        }


        public ActionResult Add_C_GoodsType(int id)
        {
            C_GOODSTYPE model = db.C_GOODSTYPE.Find(id) ?? new C_GOODSTYPE();
            return View(model);
        }
        [HttpPost]
        public JsonResult Del_Dic_GoodsType(string datalist)
        {
            List<C_GOODSTYPE> list = JsonConvert.DeserializeObject<List<C_GOODSTYPE>>(datalist);

            EFHelpler<C_GOODSTYPE> ef = new EFHelpler<C_GOODSTYPE>();
            ef.delete(list.ToArray());
            return Json(new AjaxResult { state = ResultType.success.ToString(), message = "成功！" }.ToJson());

        }
        //车队
        public ActionResult CarTeamList()
        {
            return View();
        }
        [HttpGet]
        public object GetCarTeamList(int limit, int offset, string GoodsName)
        {
            var wherelambda = ExtLinq.True<C_CARTEAM>();
            if (!string.IsNullOrEmpty(GoodsName))
            {
                wherelambda = wherelambda.And(t => t.GoodsName.Contains(GoodsName));
            }
            var list = db.Set<C_CARTEAM>().Where(wherelambda).OrderByDescending(n => n.ID).AsQueryable();
            int total = list.Count();
            object rows = list.Skip(offset).Take(limit).AsQueryable();
            return Json(new { total = total, rows = rows }, JsonRequestBehavior.AllowGet);

        }
        //抬头人
        public ActionResult TaiTouRenList()
        {
            return View();
        }
        [HttpGet]
        public object GetTaiTouRenList(int limit, int offset, string GoodsName)
        {
            var wherelambda = ExtLinq.True<BS_C_TAITOUREN>();
            if (!string.IsNullOrEmpty(GoodsName))
            {
                wherelambda = wherelambda.And(t => t.GoodsName.Contains(GoodsName));
            }
            var list = db.Set<BS_C_TAITOUREN>().Where(wherelambda).OrderByDescending(n => n.Guid).AsQueryable();
            int total = list.Count();
            object rows = list.Skip(offset).Take(limit).AsQueryable();
            return Json(new { total = total, rows = rows }, JsonRequestBehavior.AllowGet);

        }
        //英文品名
        public ActionResult BsPinMingList()
        {
            return View();
        }
        [HttpGet]
        public object GetBsPinMingList(int limit, int offset, string GoodsName)
        {
            var wherelambda = ExtLinq.True<BS_C_PINGMING>();
            if (!string.IsNullOrEmpty(GoodsName))
            {
                wherelambda = wherelambda.And(t => t.GoodsName.Contains(GoodsName));
            }
            var list = db.Set<BS_C_PINGMING>().Where(wherelambda).OrderByDescending(n => n.Guid).AsQueryable();
            int total = list.Count();
            object rows = list.Skip(offset).Take(limit).AsQueryable();
            return Json(new { total = total, rows = rows }, JsonRequestBehavior.AllowGet);

        }
        public ActionResult Add_C_CarTeamType(int id)
        {
            C_CARTEAM model = db.C_CARTEAM.Find(id) ?? new C_CARTEAM();
            return View(model);
        }
        [HttpPost]
        public ActionResult Add_C_CarTeamType(C_CARTEAM model)
        {
            var loginModel = OperatorProvider.Provider.GetCurrent();
            model.YuanQuID = loginModel.YuanquID;
            db.C_CARTEAM.AddOrUpdate(model);
  
            int c = db.SaveChanges();
            if (c > 0)
            {
                return Json(new AjaxResult { state = ResultType.success.ToString(), message = "成功！" }.ToJson());
            }
            return Json(new AjaxResult { state = ResultType.error.ToString(), message = "失败！" }.ToJson());

        }
        public ActionResult Add_C_TaiTouRen(string id)
        {
            BS_C_TAITOUREN model = db.BS_C_TAITOUREN.Find(id) ?? new BS_C_TAITOUREN();
            return View(model);
        }
        [HttpPost]
        public ActionResult Add_C_TaiTouRen(BS_C_TAITOUREN model)
        {
            var loginModel = OperatorProvider.Provider.GetCurrent();
            model.YuanQuID = loginModel.YuanquID;
            db.BS_C_TAITOUREN.AddOrUpdate(model);
            if (model.Guid==null)
            {
                model.Guid = Guid.NewGuid().ToString();
            }
            int c = db.SaveChanges();
            if (c > 0)
            {
                return Json(new AjaxResult { state = ResultType.success.ToString(), message = "成功！" }.ToJson());
            }
            return Json(new AjaxResult { state = ResultType.error.ToString(), message = "失败！" }.ToJson());

        }
        public ActionResult Add_C_BsPinMing(string id)
        {
            BS_C_PINGMING model = db.BS_C_PINGMING.Find(id) ?? new BS_C_PINGMING();
            return View(model);
        }
        [HttpPost]
        public ActionResult Add_C_BsPinMing(BS_C_PINGMING model)
        {
            var loginModel = OperatorProvider.Provider.GetCurrent();
            model.YuanQuID = loginModel.YuanquID;
            db.BS_C_PINGMING.AddOrUpdate(model);
            if (model.Guid == null)
            {
                model.Guid = Guid.NewGuid().ToString();
            }
            int c = db.SaveChanges();
            if (c > 0)
            {
                return Json(new AjaxResult { state = ResultType.success.ToString(), message = "成功！" }.ToJson());
            }
            return Json(new AjaxResult { state = ResultType.error.ToString(), message = "失败！" }.ToJson());

        }
        [HttpPost]
        public JsonResult Del_Dic_CarTeam(string datalist)
        {
            List<C_CARTEAM> list = JsonConvert.DeserializeObject<List<C_CARTEAM>>(datalist);

            EFHelpler<C_CARTEAM> ef = new EFHelpler<C_CARTEAM>();
            ef.delete(list.ToArray());
            return Json(new AjaxResult { state = ResultType.success.ToString(), message = "成功！" }.ToJson());

        }
        //百分比提醒设置
        public ActionResult PercentList()
        {
            return View();
        }
        //劳务作业类别
        public ActionResult LWZYList()
        {
            return View();
        }
        [HttpGet]
        public object GetLWZYList(int limit, int offset, string ZuoYeLeiBieMingCheng)
        {
            var loginModel = OperatorProvider.Provider.GetCurrent();
            var wherelambda = ExtLinq.True<BS_LAOWUZUOYELEIBIE>();
            wherelambda = wherelambda.And(t=>t.YuanQuId == loginModel.YuanquID);
            if (!string.IsNullOrEmpty(ZuoYeLeiBieMingCheng))
            {
                wherelambda = wherelambda.And(t => t.ZuoYeLeiBieMingCheng.Contains(ZuoYeLeiBieMingCheng));
            }
            var list = db.Set<BS_LAOWUZUOYELEIBIE>().Where(wherelambda).OrderByDescending(n => n.CreatTime).AsQueryable();
            int total = list.Count();
            object rows = list.Skip(offset).Take(limit).AsQueryable();
            return Json(new { total = total, rows = rows }, JsonRequestBehavior.AllowGet);

        }
        public ActionResult Add_C_LAOWUZUOYELEIBIE(string id)
        {
            BS_LAOWUZUOYELEIBIE model = db.BS_LAOWUZUOYELEIBIE.Find(id) ?? new BS_LAOWUZUOYELEIBIE();
            List<C_TB_CODE_CUSTOMER> list_CUSTOMER = db.C_TB_CODE_CUSTOMER.OrderBy(n => n.ID).ToList();//委托人,收货人
            List<SelectListItem> emplist_CUSTOMER = SelectHelp.CreateSelect<C_TB_CODE_CUSTOMER>(list_CUSTOMER, "Name", "Name", null);
            ViewData["CUSTOMER_List"] = new SelectList(emplist_CUSTOMER, "Value", "Text", "是");
            return View(model);
        }
        [HttpPost]
        public ActionResult Add_C_LAOWUZUOYELEIBIE(BS_LAOWUZUOYELEIBIE model)
        {
            var loginModel = OperatorProvider.Provider.GetCurrent();
            db.BS_LAOWUZUOYELEIBIE.AddOrUpdate(model);
            if (model.Guid == null)
            {
                model.Guid = Guid.NewGuid().ToString();
                model.CreatPeople = loginModel.UserName;
                model.CreatTime = DateTime.Now;
               
            }
            model.YuanQuId = loginModel.YuanquID;
            model.ShuLiang = 0;
            model.JinE = 0;
            int c = db.SaveChanges();
            if (c > 0)
            {
                return Json(new AjaxResult { state = ResultType.success.ToString(), message = "成功！" }.ToJson());
            }
            return Json(new AjaxResult { state = ResultType.error.ToString(), message = "失败！" }.ToJson());

        }
    }
}