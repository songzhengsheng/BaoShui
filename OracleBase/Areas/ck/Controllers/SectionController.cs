using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Main.HelpClass;
using MainBLL.SysModel;
using MainBLL.TallyBLL;
using Newtonsoft.Json;
using NFine.Code;
using NFine.Code.Select;
using OracleBase.HelpClass;
using OracleBase.Models;

namespace OracleBase.Areas.ck.Controllers
{
    /// <summary>
    /// Section
    /// </summary>
    [SignLoginAuthorize]
    public class SectionController : Controller
    {
        private Entities db = new Entities();

        // GET: ck/Section
        public ActionResult SectionIndex()
        {
            return View();
        }

        public ActionResult Section()
        {
            return View();
        }

        [System.Web.Http.HttpGet]
        public object GetYuanQuList(int limit, int offset, string SECTION)
        {
            OperatorModel loginModel = OperatorProvider.Provider.GetCurrent();
            var wherelambda = ExtLinq.True<C_TB_CODE_SECTION>();
            if (!string.IsNullOrEmpty(loginModel.YuanquID.ToString()))
            {
                wherelambda = wherelambda.And(t => t.CODE_COMPANY == loginModel.YuanquID);
            }
            if (!string.IsNullOrEmpty(SECTION))
            {
                wherelambda = wherelambda.And(t => t.SECTION.Contains(SECTION));
            }

            var list = db.Set<C_TB_CODE_SECTION>().Where(wherelambda).OrderByDescending(n => n.ID).AsQueryable();
            int total = list.Count();
            object rows = list.Skip(offset).Take(limit).AsQueryable();
            return Json(new {total = total, rows = rows}, JsonRequestBehavior.AllowGet);

        }

        public ActionResult AddSection(int id)
        {

            List<SelectModel> smList = new List<SelectModel>();
            SelectModel sm = new SelectModel()
            {
                Text = "是",
                Value = "是"
            };
            smList.Add(sm);
            sm = new SelectModel()
            {
                Text = "否",
                Value = "否"
            };
            smList.Add(sm);
            List<SelectListItem> emplist = SelectHelp.CreateSelect<SelectModel>(smList, "Value", "Text", null);
            ViewData["MARK_FORBID"] = new SelectList(emplist, "Value", "Text", "是");
            C_TB_CODE_SECTION model = db.C_TB_CODE_SECTION.Find(id) ?? new C_TB_CODE_SECTION();
            return View(model);
        }

        [HttpPost]
        public ActionResult AddSection(C_TB_CODE_SECTION model)
        {
            OperatorModel loginModel = OperatorProvider.Provider.GetCurrent();
            var yuanquid = loginModel.YuanquID ?? throw new ArgumentNullException("loginModel.YuanquID");
            C_Dic_YuanQu yq = db.C_Dic_YuanQu.Find(yuanquid);
            if (yq != null)
            {
                model.CODE_COMPANY_Name = yq.YuanQuName;
                model.CODE_COMPANY = yq.ID;
            }
            else
            {
                return Json(new AjaxResult {state = ResultType.error.ToString(), message = "失败！园区不存在"}.ToJson());
            }

            try
            {
                db.C_TB_CODE_SECTION.AddOrUpdate(model);
                int c = db.SaveChanges();
                if (c > 0)
                {
                    return Json(new AjaxResult {state = ResultType.success.ToString(), message = "成功！"}.ToJson());
                }

                return Json(new AjaxResult {state = ResultType.error.ToString(), message = "失败！"}.ToJson());

            }
            catch (Exception e)
            {
                return Json(new AjaxResult {state = ResultType.error.ToString(), message = e.Message}.ToJson());


            }


        }
        [HttpPost]
        public JsonResult DelSectionById(int id)
        {
            EFHelpler<C_TB_CODE_BOOTH> ef = new EFHelpler<C_TB_CODE_BOOTH>();
            try
            {
                C_TB_CODE_SECTION u = new C_TB_CODE_SECTION() { ID = id };
                db.C_TB_CODE_SECTION.Attach(u);
                db.C_TB_CODE_SECTION.Remove(u);
                List<C_TB_CODE_STORAGE> storages = db.C_TB_CODE_STORAGE.Where(n => n.SECTION_ID == id).ToList();
                foreach (C_TB_CODE_STORAGE st in storages)
                {
                    List<C_TB_CODE_BOOTH> BOOTHs = db.C_TB_CODE_BOOTH.Where(n => n.Storage_ID == st.ID).ToList();
              
                    ef.delete(BOOTHs.ToArray());
                }
                EFHelpler<C_TB_CODE_STORAGE> efC_TB_CODE_STORAGE = new EFHelpler<C_TB_CODE_STORAGE>();
                efC_TB_CODE_STORAGE.delete(storages.ToArray());
                db.SaveChanges();
                return Json(new AjaxResult { state = ResultType.success.ToString(), message = "成功" }.ToJson());
            }
            catch (Exception e)
            {
                return Json(new AjaxResult { state = ResultType.error.ToString(), message = "失败！" }.ToJson());
            }
        }
        [HttpPost]
        public JsonResult DelSectionList(string datalist)
        {
            List<C_TB_CODE_SECTION> list = JsonConvert.DeserializeObject<List<C_TB_CODE_SECTION>>(datalist);
            EFHelpler<C_TB_CODE_SECTION> efse = new EFHelpler<C_TB_CODE_SECTION>();
            EFHelpler<C_TB_CODE_STORAGE> efst = new EFHelpler<C_TB_CODE_STORAGE>();
            EFHelpler<C_TB_CODE_BOOTH> efboot = new EFHelpler<C_TB_CODE_BOOTH>();
            try
            {
                foreach (C_TB_CODE_SECTION se in list)
                {
                    List<C_TB_CODE_STORAGE> storages = db.C_TB_CODE_STORAGE.Where(n => n.SECTION_ID == se.ID).ToList();
                    foreach (C_TB_CODE_STORAGE st in storages)
                    {
                        List<C_TB_CODE_BOOTH> BOOTHs = db.C_TB_CODE_BOOTH.Where(n => n.Storage_ID == st.ID).ToList();
                        efboot.delete(BOOTHs.ToArray());
                    }
                    efst.delete(storages.ToArray());
                }
                efse.delete(list.ToArray());
                return Json(new AjaxResult { state = ResultType.success.ToString(), message = "成功！" }.ToJson());

            }
            catch (Exception e)
            {
                return Json(new AjaxResult { state = ResultType.error.ToString(), message = "失败！" }.ToJson());
            }
         
        }

        public ActionResult Storage(int id)
        {
            ViewBag.SECTION_ID = id;
            return View();
        }

        [System.Web.Http.HttpGet]
        public object GetStorageList(int limit, int offset, int SECTION_ID)
        {
            var list = db.Set<C_TB_CODE_STORAGE>().Where(n => n.SECTION_ID == SECTION_ID).OrderByDescending(n => n.ID)
                .AsQueryable();
            int total = list.Count();
            object rows = list.Skip(offset).Take(limit).AsQueryable();
            return Json(new {total = total, rows = rows}, JsonRequestBehavior.AllowGet);

        }

        public ActionResult AddStorage(int id, int SECTION_ID)
        {
            C_TB_CODE_STORAGE model = db.C_TB_CODE_STORAGE.Find(id) ?? new C_TB_CODE_STORAGE()
            {
                SECTION_ID = SECTION_ID
            };
            return View(model);
        }

        [HttpPost]
        public ActionResult AddStorage(C_TB_CODE_STORAGE model)
        {
            try
            {
                db.C_TB_CODE_STORAGE.AddOrUpdate(model);
                int c = db.SaveChanges();
                if (c > 0)
                {
                    return Json(new AjaxResult {state = ResultType.success.ToString(), message = "成功！"}.ToJson());
                }

                return Json(new AjaxResult {state = ResultType.error.ToString(), message = "失败！"}.ToJson());

            }
            catch (Exception e)
            {
                return Json(new AjaxResult {state = ResultType.error.ToString(), message = e.Message}.ToJson());
            }

        }
        [HttpPost]
        public JsonResult DelStoragehById(int id)
        {
            EFHelpler<C_TB_CODE_STORAGE> efst = new EFHelpler<C_TB_CODE_STORAGE>();
            EFHelpler<C_TB_CODE_BOOTH> efBOOTH = new EFHelpler<C_TB_CODE_BOOTH>();
            try
            {
                C_TB_CODE_STORAGE booth = new C_TB_CODE_STORAGE { ID = id };
                List<C_TB_CODE_BOOTH> BOOTHs = db.C_TB_CODE_BOOTH.Where(n => n.Storage_ID == booth.ID).ToList();
                efBOOTH.delete(BOOTHs.ToArray());
                efst.delete(booth);
                return Json(new AjaxResult { state = ResultType.success.ToString(), message = "成功" }.ToJson());
            }
            catch (Exception e)
            {
                return Json(new AjaxResult { state = ResultType.error.ToString(), message = e.Message }.ToJson());
            }
        }
        [HttpPost]
        public JsonResult DelStorageList(string datalist)
        {
            List<C_TB_CODE_STORAGE> storages = JsonConvert.DeserializeObject<List<C_TB_CODE_STORAGE>>(datalist);
            EFHelpler<C_TB_CODE_STORAGE> efst = new EFHelpler<C_TB_CODE_STORAGE>();
            EFHelpler<C_TB_CODE_BOOTH> efboot = new EFHelpler<C_TB_CODE_BOOTH>();
        
            try
            {
                foreach (C_TB_CODE_STORAGE st in storages)
                {
                    List<C_TB_CODE_BOOTH> BOOTHs = db.C_TB_CODE_BOOTH.Where(n => n.Storage_ID == st.ID).ToList();
                    efboot.delete(BOOTHs.ToArray());
                }
                efst.delete(storages.ToArray());
                return Json(new AjaxResult { state = ResultType.success.ToString(), message = "成功！" }.ToJson());
            }
            catch (Exception e)
            {
                return Json(new AjaxResult { state = ResultType.error.ToString(), message = e.Message }.ToJson());
            }

        }

        #region 位
        public ActionResult Booth(int Storage_ID)
        {
            ViewBag.Storage_ID = Storage_ID;
            return View();
        }
        [System.Web.Http.HttpGet]
        public object GetBoothListByStorage_ID(int limit, int offset, int Storage_ID)
        {
            var list = db.Set<C_TB_CODE_BOOTH>().Where(n => n.Storage_ID == Storage_ID).OrderByDescending(n => n.ID)
                .AsQueryable();
            int total = list.Count();
            object rows = list.Skip(offset).Take(limit).AsQueryable();
            return Json(new { total = total, rows = rows }, JsonRequestBehavior.AllowGet);

        }
        public ActionResult AddBooth(int id, int Storage_ID)
        {
           
            C_TB_CODE_BOOTH model = db.C_TB_CODE_BOOTH.Find(id) ?? new C_TB_CODE_BOOTH()
            {
                Storage_ID = Storage_ID
                
            };
            return View(model);
        }
        [HttpPost]
        public ActionResult AddBooth(C_TB_CODE_BOOTH model)
        {
            try
            {
                OperatorModel loginModel = OperatorProvider.Provider.GetCurrent();
                model.CompanyId = loginModel.YuanquID;
                db.C_TB_CODE_BOOTH.AddOrUpdate(model);
                int c = db.SaveChanges();
                if (c > 0)
                {
                    return Json(new AjaxResult { state = ResultType.success.ToString(), message = "成功！" }.ToJson());
                }

                return Json(new AjaxResult { state = ResultType.error.ToString(), message = "失败！" }.ToJson());

            }
            catch (Exception e)
            {
                return Json(new AjaxResult { state = ResultType.error.ToString(), message = e.Message }.ToJson());
            }

        }

        [HttpPost]
        public JsonResult DelBoothById(int id)
        {
            EFHelpler<C_TB_CODE_BOOTH> ef = new EFHelpler<C_TB_CODE_BOOTH>();
            try
            {
                C_TB_CODE_BOOTH booth = new C_TB_CODE_BOOTH { ID = id };
                ef.delete(booth);
                return Json(new AjaxResult { state = ResultType.success.ToString(), message = "成功" }.ToJson());
            }
            catch (Exception e)
            {
                return Json(new AjaxResult { state = ResultType.error.ToString(), message = e.Message }.ToJson());
            }
        }
        [HttpPost]
        public JsonResult DelBoothList(string datalist)
        {
            List<C_TB_CODE_BOOTH> list = JsonConvert.DeserializeObject<List<C_TB_CODE_BOOTH>>(datalist);
            EFHelpler<C_TB_CODE_BOOTH> efboot = new EFHelpler<C_TB_CODE_BOOTH>();
            try
            {
                efboot.delete(list.ToArray());
                return Json(new AjaxResult { state = ResultType.success.ToString(), message = "成功！" }.ToJson());
            }
            catch (Exception e)
            {
                return Json(new AjaxResult { state = ResultType.error.ToString(), message = e.Message }.ToJson());
            }

        }


        #endregion


        #region 设置库存平面图

        public ActionResult SectionPic()
        {
            OperatorModel loginModel = OperatorProvider.Provider.GetCurrent();
            decimal? YuanQu_id = loginModel.YuanquID;
            C_Dic_YuanQu YuanQuModel = db.C_Dic_YuanQu.Find(YuanQu_id);
            if (!string.IsNullOrEmpty(YuanQuModel.PicPach))
            {
                ViewBag.PicPach = YuanQuModel.PicPach;

            }
            else
            {
                ViewBag.PicPach = "demo_picture.jpg";

            }
            return View();
        }
        [HttpGet]
        public string GetBoothList()
        {
            List<C_TB_CODE_BOOTH> booths = new List<C_TB_CODE_BOOTH>();
            OperatorModel loginModel = OperatorProvider.Provider.GetCurrent();
            if (!string.IsNullOrEmpty(loginModel.YuanquID.ToString()))
            {
                booths = db.C_TB_CODE_BOOTH.Where(n => n.color != null && n.CompanyId == loginModel.YuanquID).ToList();
            }
            else
            {
                booths = db.C_TB_CODE_BOOTH.Where(n => n.color != null && n.CompanyId == 0).ToList();
            }
            List<Box> boxs = booths.Select(x => new Box { id = x.ID, text = x.BOOTH, color = x.color, height = x.height, width=x.width, pageX=x.pageX, pageY=x.pageY }).ToList();
            JsonHelper jshelp = new JsonHelper();
            string boothstring= jshelp.ListToJSON(boxs);
            return boothstring;


        }
        public ActionResult CreatBox()
        {
            OperatorModel loginModel = OperatorProvider.Provider.GetCurrent();
            //OperatorModel loginModel = OperatorProvider.Provider.GetCurrent();
            //var yuanquid = loginModel.YuanquID ?? throw new ArgumentNullException("loginModel.YuanquID");
            List<C_TB_CODE_SECTION> listSECTION = db.C_TB_CODE_SECTION.Where(n=>n.CODE_COMPANY== loginModel.YuanquID).ToList();
            List<SelectListItem> emplist = SelectHelp.CreateSelect<C_TB_CODE_SECTION>(listSECTION, "SECTION", "ID", null);
            ViewBag.qu = new SelectList(emplist, "Value", "Text", "");
            return View();
        }
        /// <summary>
        /// 根据区id获取下面的场
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public string GetSTORAGEBySECTIONId(int id)
        {
            List<C_TB_CODE_STORAGE> sections = db.C_TB_CODE_STORAGE.Where(n => n.SECTION_ID == id).ToList();
            List<Storage> stb = sections.Select(x => new Storage { ID = x.ID, StorageName = x.STORAGEName }).ToList();
            JsonHelper jshelp = new JsonHelper();
            return jshelp.List2JSON(stb);

        }
        [HttpGet]
        public string GetBoothByChangId(int id)
        {

            List<C_TB_CODE_BOOTH> sections = db.C_TB_CODE_BOOTH.Where(n => n.Storage_ID == id).ToList();
            List<Booth> stb = sections.Select(x => new Booth { ID = x.ID, BoothName = x.BOOTH }).ToList();
            JsonHelper jshelp = new JsonHelper();
            return jshelp.List2JSON(stb);

        }

        [HttpPost]
        public JsonResult SaveSectionPic(string d)
        {

            Box box =  JsonHelper.JsonDeserialize<Box>(d);
            C_TB_CODE_BOOTH boothModel = db.C_TB_CODE_BOOTH.Find(box.id);
            if (boothModel!=null)
            {
                boothModel.color = box.color;
                boothModel.height = box.height;
                boothModel.width = box.width;
                boothModel.pageX = box.pageX;
                boothModel.pageY = box.pageY;
                try
                {
                    db.SaveChanges();
                    return Json(new AjaxResult { state = ResultType.success.ToString(), message = "成功" }.ToJson());
                }
                catch (Exception e)
                {
                    return Json(new AjaxResult { state = ResultType.error.ToString(), message = e.Message }.ToJson());
                }
               
            }
            return Json(new AjaxResult { state = ResultType.error.ToString(), message = "失败" }.ToJson());
        }

       /// <summary>
       /// 删除位
       /// </summary>
       /// <param name="id"></param>
       /// <returns></returns>
        [HttpPost]
        public JsonResult DelSectionPic(int id)
        {
            C_TB_CODE_BOOTH boothModel = db.C_TB_CODE_BOOTH.Find(id);
            if (boothModel != null)
            {
                boothModel.color = null;
                boothModel.height = null;
                boothModel.width = null;
                boothModel.pageX = null;
                boothModel.pageY = null;
                try
                {
                    db.SaveChanges();
                    return Json(new AjaxResult { state = ResultType.success.ToString(), message = "成功" }.ToJson());
                }
                catch (Exception e)
                {
                    return Json(new AjaxResult { state = ResultType.error.ToString(), message = e.Message }.ToJson());
                }

            }
            return Json(new AjaxResult { state = ResultType.error.ToString(), message = "失败" }.ToJson());
        }


        public ActionResult TallyBllIndex(int STORAG)
        {
            ViewBag.STORAG = STORAG;
            return View();
        }

        [System.Web.Http.HttpGet]
        public object GetTallyBllList(int limit, int offset, string STORAG)
        {
            var wherelambda = ExtLinq.True<C_TB_HS_TALLYBILL>();
            if (!string.IsNullOrEmpty(STORAG))
            {
                wherelambda = wherelambda.And(t => t.STORAG== STORAG);
            }

            var list = db.Set<C_TB_HS_TALLYBILL>().Where(wherelambda).OrderByDescending(n => n.ID).AsQueryable();
            int total = list.Count();
            object rows = list.Skip(offset).Take(limit).AsQueryable();
            return Json(new { total = total, rows = rows }, JsonRequestBehavior.AllowGet);

        }

        #endregion


    }
}