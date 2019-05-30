using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Migrations;
using System.IO;
using System.Linq;
using System.Text;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using Main.HelpClass;
using MainBLL.Money;
using MainBLL.ToOut;
using NFine.Code;
using NFine.Code.Excel;
using NPOI.HSSF.UserModel;
using NPOI.HSSF.Util;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using OracleBase.HelpClass;
using OracleBase.HelpClass.Sys;
using OracleBase.Models;
using OracleBase.ServiceReference1;

namespace OracleBase.Areas.ck.Controllers
{
    [SignLoginAuthorize]
    public class ToOutController : Controller
    {
        private Entities db = new Entities();

        #region 其他
        // GET: /ck/ToOut
        public ActionResult Index()
        {
            return View();
        }

        public void WorkNumToExcel()
        {
            OperatorModel loginModel = OperatorProvider.Provider.GetCurrent();
            var yuanquid = loginModel.YuanquID ?? throw new ArgumentNullException("loginModel.YuanquID");

            List<Stock_Month> Stocklist = new List<Stock_Month>();
            DateTime starTime = Convert.ToDateTime(Request["starTime"]);
            DateTime endTime = Convert.ToDateTime(Request["endTime"]);
            int days = (endTime - starTime).Days;
            List<C_TB_HS_STOCKDORMANT> list = db.C_TB_HS_STOCKDORMANT.Where(n => n.LastEidTime <= endTime && yuanquid == yuanquid).ToList();
            for (int i = 0; i <= days; i++)
            {

                starTime = starTime.AddDays(1);

                list = db.C_TB_HS_STOCKDORMANT.Where(n => n.LastEidTime <= starTime).ToList();

                if (list.Count == 0)
                {
                    Stock_Month model = new Stock_Month()
                    {
                        ID = i,
                        Time = starTime.AddDays(-1),
                        Goodname = "",
                        InAMOUNT = "0",
                        InWEIGHT = "0",
                        OutAMOUNT = "0",
                        OutWEIGHT = "0",
                        AllAMOUNT = "0",
                        AllWEIGHT = "0",
                    };
                    Stocklist.Add(model);
                }
                else
                {
                    Array goodsname = list.Select(n => n.GoodsName).Distinct().ToArray();

                    foreach (var item in goodsname)
                    {
                        decimal? InAMOUNT, InWEIGHT, OutAMOUNT, OutWEIGHT, AllAMOUNT, AllWEIGHT;
                        string goodname = null;
                        DateTime Time = starTime.AddDays(-1);
                        if (item == null)
                        {
                            InAMOUNT = 0;
                            InWEIGHT = 0;
                            OutAMOUNT = 0;
                            OutWEIGHT = 0;
                            AllAMOUNT = 0;
                            AllWEIGHT = 0;
                        }
                        else
                        {
                            goodname = item.ToString();
                            InAMOUNT = list.Where(n => n.LastEidTime != null && (n.GoodsName == item.ToString() && n.REMARK == "进库" && n.LastEidTime.Value.Date == Time.Date)).Sum(n => n.AMOUNT);
                            InWEIGHT = list.Where(n => n.LastEidTime != null && (n.GoodsName == item.ToString() && n.REMARK == "进库" && n.LastEidTime.Value.Date == Time.Date)).Sum(n => n.WEIGHT);

                            OutAMOUNT = list.Where(n => n.LastEidTime != null && (n.GoodsName == item.ToString() && n.REMARK == "出库" && n.LastEidTime.Value.Date == Time.Date)).Sum(n => n.AMOUNT);
                            OutWEIGHT = list.Where(n => n.LastEidTime != null && (n.GoodsName == item.ToString() && n.REMARK == "出库" && n.LastEidTime.Value.Date == Time.Date)).Sum(n => n.WEIGHT);

                            AllAMOUNT = list.Where(n => n.LastEidTime != null && (n.GoodsName == item.ToString() && n.LastEidTime.Value.Date <= Time.Date)).Sum(n => n.AMOUNT);
                            AllWEIGHT = list.Where(n => n.LastEidTime != null && (n.GoodsName == item.ToString() && n.LastEidTime.Value.Date <= Time.Date)).Sum(n => n.WEIGHT);
                        }


                        Stock_Month model = new Stock_Month()
                        {
                            ID = i,
                            Time = Time,
                            Goodname = goodname,
                            InAMOUNT = InAMOUNT.ToString(),
                            InWEIGHT = InWEIGHT.ToString(),
                            OutAMOUNT = OutAMOUNT.ToString(),
                            OutWEIGHT = OutWEIGHT.ToString(),
                            AllAMOUNT = AllAMOUNT.ToString(),
                            AllWEIGHT = AllWEIGHT.ToString(),
                        };
                        Stocklist.Add(model);
                    }
                }

            }
            var filepath = PMonthCountExportToExcel(Stocklist, "作业量");
            FileDownHelper.DownLoad(filepath);

        }
        public string PMonthCountExportToExcel(List<Stock_Month> Monthlist, string sheetName)
        {
            var book = new NPOI.HSSF.UserModel.HSSFWorkbook();

            Array Id = Monthlist.Select(n => n.ID).Distinct().ToArray();

            foreach (int id in Id)
            {
                var list = Monthlist.Where(n => n.ID == id).ToList();
                string sheetname = list.FirstOrDefault()?.Time.ToString("MM.dd");
                var sheet = book.CreateSheet(sheetname);

                //表格格式
                HSSFCellStyle boderstyle = (HSSFCellStyle)book.CreateCellStyle();
                boderstyle.BorderTop = BorderStyle.Thin;
                boderstyle.BorderBottom = BorderStyle.Thin;
                boderstyle.BorderLeft = BorderStyle.Thin;
                boderstyle.BorderRight = BorderStyle.Thin;
                boderstyle.VerticalAlignment = VerticalAlignment.Center;
                boderstyle.Alignment = HorizontalAlignment.Center;//单元格水平居中
                                                                  //设置列宽
                                                                  //for (int i = 0; i < 8; i++)
                                                                  //    sheet.AutoSizeColumn(i);
                sheet.SetColumnWidth(0, 20 * 150);
                sheet.SetColumnWidth(1, 20 * 150);
                sheet.SetColumnWidth(2, 20 * 150);
                sheet.SetColumnWidth(3, 20 * 150);
                sheet.SetColumnWidth(4, 20 * 150);
                sheet.SetColumnWidth(5, 20 * 150);
                sheet.SetColumnWidth(6, 20 * 150);
                sheet.SetColumnWidth(7, 20 * 150);
                //创建行
                var row0 = sheet.CreateRow(0);
                for (int i = 0; i < 8; i++)
                {
                    ICell cell = row0.CreateCell(i);  //在第二行中创建单元格
                    cell.CellStyle = boderstyle;
                }
                var row1 = sheet.CreateRow(1);
                for (int i = 0; i < 8; i++)
                {
                    ICell cell = row1.CreateCell(i);  //在第二行中创建单元格
                    cell.CellStyle = boderstyle;
                }
                var row2 = sheet.CreateRow(2);
                for (int i = 0; i < 8; i++)
                {
                    ICell cell = row2.CreateCell(i);  //在第二行中创建单元格
                    cell.CellStyle = boderstyle;

                }

                //实例化字体和样式
                HSSFFont font = (HSSFFont)book.CreateFont();
                //表头格式
                HSSFCellStyle style = (HSSFCellStyle)book.CreateCellStyle();
                font.Color = HSSFColor.Black.Index;
                font.FontHeightInPoints = 20;
                //创建表头
                row0.Cells[0].CellStyle = style;
                row0.Cells[0].CellStyle.SetFont(font);
                row0.Cells[0].CellStyle.Alignment = HorizontalAlignment.Center;
                row0.GetCell(0).SetCellValue("作 业 量");


                //表格中的单元格合并
                sheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, 7));
                sheet.AddMergedRegion(new CellRangeAddress(1, 1, 1, 2));
                sheet.AddMergedRegion(new CellRangeAddress(1, 1, 3, 4));
                sheet.AddMergedRegion(new CellRangeAddress(1, 1, 5, 6));
                //添字段名称
                row1.Cells[0].SetCellValue("货名");
                row1.Cells[1].SetCellValue("进库");
                row1.Cells[3].SetCellValue("出库");
                row1.Cells[5].SetCellValue("总库存");
                row1.Cells[7].SetCellValue("备注");

                row2.Cells[1].SetCellValue("支/件");
                row2.Cells[2].SetCellValue("数量/吨/方");
                row2.Cells[3].SetCellValue("支/件");
                row2.Cells[4].SetCellValue("数量/吨/方");
                row2.Cells[5].SetCellValue("支/件");
                row2.Cells[6].SetCellValue("数量/吨/方");
                // 赋值
                for (int i = 0; i < list.Count; i++)
                {
                    int row = i + 3;
                    var newRrow = sheet.CreateRow(row);
                    for (int j = 0; j < 8; j++)
                    {
                        ICell cell = newRrow.CreateCell(j);  //在第二行中创建单元格
                        cell.CellStyle = boderstyle;
                    }
                    newRrow.Cells[0].SetCellValue(list[i].Goodname);
                    newRrow.Cells[1].SetCellValue(list[i].InAMOUNT);
                    newRrow.Cells[2].SetCellValue(list[i].InWEIGHT);
                    newRrow.Cells[3].SetCellValue(list[i].OutAMOUNT);
                    newRrow.Cells[4].SetCellValue(list[i].OutWEIGHT);
                    newRrow.Cells[5].SetCellValue(list[i].AllAMOUNT);
                    newRrow.Cells[6].SetCellValue(list[i].AllWEIGHT);
                }

            }




            string filePath = string.Format("/ExportFile/{1}-{0:yyyyMMddHHmmssfff}.xls", DateTime.Now, sheetName);

            using (var ms = new MemoryStream())
            {
                book.Write(ms);
                try
                {
                    var folder = Server.MapPath("/ExportFile");
                    var path = Server.MapPath(filePath);
                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);

                    }
                    using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write))
                    {
                        var data = ms.ToArray();
                        fs.Write(data, 0, data.Length);
                        fs.Flush();
                    }
                }
                catch (Exception ex)
                {

                    throw ex;
                }

            }
            return filePath;
        }
        public JsonResult WorkNumToExcel_year(string year)
        {
            if (string.IsNullOrEmpty(year))
            {
                return Json(new AjaxResult { state = ResultType.error.ToString(), message = "请选择年份！" }.ToJson());
            }

            List<C_GOODS> list_Goods = db.C_GOODS.OrderBy(n => n.ID).ToList();

            string text1 = Server.MapPath("..//QueryOut//" + "年报表" + ".xls");

            StreamWriter writer1 = new StreamWriter(text1, false);
            StreamWriter writer2 = writer1;
            writer2.WriteLine("	<?xml version=\"1.0\"?>	");
            writer2.WriteLine("	<?mso-application progid=\"Excel.Sheet\"?>	");
            writer2.WriteLine("	<Workbook xmlns=\"urn:schemas-microsoft-com:office:spreadsheet\"	");
            writer2.WriteLine("	 xmlns:o=\"urn:schemas-microsoft-com:office:office\"	");
            writer2.WriteLine("	 xmlns:x=\"urn:schemas-microsoft-com:office:excel\"	");
            writer2.WriteLine("	 xmlns:dt=\"uuid:C2F41010-65B3-11d1-A29F-00AA00C14882\"	");
            writer2.WriteLine("	 xmlns:ss=\"urn:schemas-microsoft-com:office:spreadsheet\"	");
            writer2.WriteLine("	 xmlns:html=\"http://www.w3.org/TR/REC-html40\">	");
            writer2.WriteLine("	 <DocumentProperties xmlns=\"urn:schemas-microsoft-com:office:office\">	");
            writer2.WriteLine("	  <Author>qqq</Author>	");
            writer2.WriteLine("	  <LastAuthor>kaifa</LastAuthor>	");
            writer2.WriteLine("	  <Revision>1</Revision>	");
            writer2.WriteLine("	  <Created>2015-06-25T03:02:05Z</Created>	");
            writer2.WriteLine("	  <LastSaved>2018-11-30T06:22:11Z</LastSaved>	");
            writer2.WriteLine("	  <Version>16.00</Version>	");
            writer2.WriteLine("	 </DocumentProperties>	");
            writer2.WriteLine("	 <CustomDocumentProperties xmlns=\"urn:schemas-microsoft-com:office:office\">	");
            writer2.WriteLine("	  <KSOProductBuildVer dt:dt=\"string\">2052-11.1.0.7989</KSOProductBuildVer>	");
            writer2.WriteLine("	  <KSORubyTemplateID dt:dt=\"string\">11</KSORubyTemplateID>	");
            writer2.WriteLine("	 </CustomDocumentProperties>	");
            writer2.WriteLine("	 <OfficeDocumentSettings xmlns=\"urn:schemas-microsoft-com:office:office\">	");
            writer2.WriteLine("	  <AllowPNG/>	");
            writer2.WriteLine("	 </OfficeDocumentSettings>	");
            writer2.WriteLine("	 <ExcelWorkbook xmlns=\"urn:schemas-microsoft-com:office:excel\">	");
            writer2.WriteLine("	  <WindowHeight>9765</WindowHeight>	");
            writer2.WriteLine("	  <WindowWidth>23445</WindowWidth>	");
            writer2.WriteLine("	  <WindowTopX>32760</WindowTopX>	");
            writer2.WriteLine("	  <WindowTopY>32760</WindowTopY>	");
            writer2.WriteLine("	  <ProtectStructure>False</ProtectStructure>	");
            writer2.WriteLine("	  <ProtectWindows>False</ProtectWindows>	");
            writer2.WriteLine("	  <DisplayInkNotes>False</DisplayInkNotes>	");
            writer2.WriteLine("	 </ExcelWorkbook>	");
            writer2.WriteLine("	 <Styles>	");
            writer2.WriteLine("	  <Style ss:ID=\"Default\" ss:Name=\"Normal\">	");
            writer2.WriteLine("	   <Alignment ss:Vertical=\"Center\"/>	");
            writer2.WriteLine("	   <Borders/>	");
            writer2.WriteLine("	   <Font ss:FontName=\"宋体\" x:CharSet=\"134\" ss:Size=\"12\"/>	");
            writer2.WriteLine("	   <Interior/>	");
            writer2.WriteLine("	   <NumberFormat/>	");
            writer2.WriteLine("	   <Protection/>	");
            writer2.WriteLine("	  </Style>	");
            writer2.WriteLine("	  <Style ss:ID=\"s64\">	");
            writer2.WriteLine("	   <Alignment ss:Horizontal=\"Center\" ss:Vertical=\"Center\"/>	");
            writer2.WriteLine("	   <Font ss:FontName=\"宋体\" x:CharSet=\"134\" ss:Size=\"12\" ss:Bold=\"1\"/>	");
            writer2.WriteLine("	  </Style>	");
            writer2.WriteLine("	  <Style ss:ID=\"s65\">	");
            writer2.WriteLine("	   <Alignment ss:Horizontal=\"Center\" ss:Vertical=\"Center\"/>	");
            writer2.WriteLine("	  </Style>	");
            writer2.WriteLine("	  <Style ss:ID=\"s66\">	");
            writer2.WriteLine("	   <Interior/>	");
            writer2.WriteLine("	  </Style>	");
            writer2.WriteLine("	  <Style ss:ID=\"s69\">	");
            writer2.WriteLine("	   <Alignment ss:Horizontal=\"Center\" ss:Vertical=\"Center\"/>	");
            writer2.WriteLine("	   <Borders>	");
            writer2.WriteLine("	    <Border ss:Position=\"Bottom\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Left\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Right\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Top\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	   </Borders>	");
            writer2.WriteLine("	   <Font ss:FontName=\"宋体\" x:CharSet=\"134\" ss:Size=\"12\" ss:Bold=\"1\"/>	");
            writer2.WriteLine("	  </Style>	");
            writer2.WriteLine("	  <Style ss:ID=\"s70\">	");
            writer2.WriteLine("	   <Alignment ss:Horizontal=\"Center\" ss:Vertical=\"Center\"/>	");
            writer2.WriteLine("	   <Font ss:FontName=\"宋体\" x:CharSet=\"134\" ss:Size=\"12\" ss:Bold=\"1\"/>	");
            writer2.WriteLine("	   <Interior/>	");
            writer2.WriteLine("	  </Style>	");
            writer2.WriteLine("	  <Style ss:ID=\"s71\">	");
            writer2.WriteLine("	   <Alignment ss:Horizontal=\"Center\" ss:Vertical=\"Center\"/>	");
            writer2.WriteLine("	   <Borders>	");
            writer2.WriteLine("	    <Border ss:Position=\"Bottom\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Left\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Right\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Top\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	   </Borders>	");
            writer2.WriteLine("	   <Font ss:FontName=\"宋体\" x:CharSet=\"134\" ss:Size=\"16\" ss:Bold=\"1\"/>	");
            writer2.WriteLine("	  </Style>	");
            writer2.WriteLine("	  <Style ss:ID=\"s72\">	");
            writer2.WriteLine("	   <Alignment ss:Horizontal=\"Center\" ss:Vertical=\"Center\"/>	");
            writer2.WriteLine("	   <Borders>	");
            writer2.WriteLine("	    <Border ss:Position=\"Bottom\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Right\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Top\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	   </Borders>	");
            writer2.WriteLine("	   <Font ss:FontName=\"宋体\" x:CharSet=\"134\" ss:Size=\"12\" ss:Color=\"#339966\"/>	");
            writer2.WriteLine("	  </Style>	");
            writer2.WriteLine("	  <Style ss:ID=\"s73\">	");
            writer2.WriteLine("	   <Alignment ss:Horizontal=\"Center\" ss:Vertical=\"Center\"/>	");
            writer2.WriteLine("	   <Borders>	");
            writer2.WriteLine("	    <Border ss:Position=\"Bottom\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Left\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Right\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Top\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	   </Borders>	");
            writer2.WriteLine("	   <Font ss:FontName=\"宋体\" x:CharSet=\"134\" ss:Size=\"12\" ss:Color=\"#0000FF\"/>	");
            writer2.WriteLine("	  </Style>	");
            writer2.WriteLine("	  <Style ss:ID=\"s74\">	");
            writer2.WriteLine("	   <Alignment ss:Horizontal=\"Center\" ss:Vertical=\"Center\"/>	");
            writer2.WriteLine("	   <Borders>	");
            writer2.WriteLine("	    <Border ss:Position=\"Bottom\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Left\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Right\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Top\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	   </Borders>	");
            writer2.WriteLine("	  </Style>	");
            writer2.WriteLine("	  <Style ss:ID=\"s75\">	");
            writer2.WriteLine("	   <Alignment ss:Horizontal=\"Center\" ss:Vertical=\"Center\"/>	");
            writer2.WriteLine("	   <Borders>	");
            writer2.WriteLine("	    <Border ss:Position=\"Bottom\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Left\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Right\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Top\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	   </Borders>	");
            writer2.WriteLine("	   <Font ss:FontName=\"宋体\" x:CharSet=\"134\" ss:Size=\"12\" ss:Color=\"#339966\"/>	");
            writer2.WriteLine("	  </Style>	");
            writer2.WriteLine("	  <Style ss:ID=\"s76\">	");
            writer2.WriteLine("	   <Alignment ss:Horizontal=\"Center\" ss:Vertical=\"Center\"/>	");
            writer2.WriteLine("	   <Borders>	");
            writer2.WriteLine("	    <Border ss:Position=\"Bottom\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Left\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Right\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Top\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	   </Borders>	");
            writer2.WriteLine("	   <Font ss:FontName=\"宋体\" x:CharSet=\"134\" ss:Size=\"16\" ss:Bold=\"1\"/>	");
            writer2.WriteLine("	   <Interior ss:Color=\"#DDEBF7\" ss:Pattern=\"Solid\"/>	");
            writer2.WriteLine("	  </Style>	");
            writer2.WriteLine("	  <Style ss:ID=\"s77\">	");
            writer2.WriteLine("	   <Alignment ss:Horizontal=\"Center\" ss:Vertical=\"Center\"/>	");
            writer2.WriteLine("	   <Borders>	");
            writer2.WriteLine("	    <Border ss:Position=\"Bottom\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Right\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Top\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	   </Borders>	");
            writer2.WriteLine("	   <Font ss:FontName=\"宋体\" x:CharSet=\"134\" ss:Size=\"12\" ss:Color=\"#339966\"/>	");
            writer2.WriteLine("	   <Interior ss:Color=\"#DDEBF7\" ss:Pattern=\"Solid\"/>	");
            writer2.WriteLine("	  </Style>	");
            writer2.WriteLine("	  <Style ss:ID=\"s78\">	");
            writer2.WriteLine("	   <Alignment ss:Horizontal=\"Center\" ss:Vertical=\"Center\"/>	");
            writer2.WriteLine("	   <Borders>	");
            writer2.WriteLine("	    <Border ss:Position=\"Bottom\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Left\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Right\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Top\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	   </Borders>	");
            writer2.WriteLine("	   <Font ss:FontName=\"宋体\" x:CharSet=\"134\" ss:Size=\"12\" ss:Color=\"#0000FF\"/>	");
            writer2.WriteLine("	   <Interior ss:Color=\"#DDEBF7\" ss:Pattern=\"Solid\"/>	");
            writer2.WriteLine("	  </Style>	");
            writer2.WriteLine("	  <Style ss:ID=\"s79\">	");
            writer2.WriteLine("	   <Alignment ss:Horizontal=\"Center\" ss:Vertical=\"Center\"/>	");
            writer2.WriteLine("	   <Borders>	");
            writer2.WriteLine("	    <Border ss:Position=\"Bottom\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Left\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Right\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Top\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	   </Borders>	");
            writer2.WriteLine("	   <Interior ss:Color=\"#DDEBF7\" ss:Pattern=\"Solid\"/>	");
            writer2.WriteLine("	  </Style>	");
            writer2.WriteLine("	  <Style ss:ID=\"s80\">	");
            writer2.WriteLine("	   <Alignment ss:Horizontal=\"Center\" ss:Vertical=\"Center\"/>	");
            writer2.WriteLine("	   <Borders>	");
            writer2.WriteLine("	    <Border ss:Position=\"Bottom\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Left\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Right\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Top\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	   </Borders>	");
            writer2.WriteLine("	   <Font ss:FontName=\"宋体\" x:CharSet=\"134\" ss:Size=\"12\" ss:Color=\"#339966\"/>	");
            writer2.WriteLine("	   <Interior ss:Color=\"#DDEBF7\" ss:Pattern=\"Solid\"/>	");
            writer2.WriteLine("	  </Style>	");
            writer2.WriteLine("	  <Style ss:ID=\"s81\">	");
            writer2.WriteLine("	   <Alignment ss:Horizontal=\"Center\" ss:Vertical=\"Center\"/>	");
            writer2.WriteLine("	   <Borders>	");
            writer2.WriteLine("	    <Border ss:Position=\"Bottom\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Left\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Right\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Top\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	   </Borders>	");
            writer2.WriteLine("	   <Font ss:FontName=\"宋体\" x:CharSet=\"134\" ss:Size=\"14\" ss:Bold=\"1\"/>	");
            writer2.WriteLine("	  </Style>	");
            writer2.WriteLine("	  <Style ss:ID=\"s82\">	");
            writer2.WriteLine("	   <Alignment ss:Horizontal=\"Center\" ss:Vertical=\"Center\"/>	");
            writer2.WriteLine("	   <Borders>	");
            writer2.WriteLine("	    <Border ss:Position=\"Bottom\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Left\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Right\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Top\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	   </Borders>	");
            writer2.WriteLine("	   <Font ss:FontName=\"宋体\" x:CharSet=\"134\" ss:Size=\"14\" ss:Bold=\"1\"/>	");
            writer2.WriteLine("	   <Interior ss:Color=\"#FCE4D6\" ss:Pattern=\"Solid\"/>	");
            writer2.WriteLine("	  </Style>	");
            writer2.WriteLine("	  <Style ss:ID=\"s83\">	");
            writer2.WriteLine("	   <Alignment ss:Horizontal=\"Center\" ss:Vertical=\"Center\"/>	");
            writer2.WriteLine("	   <Borders>	");
            writer2.WriteLine("	    <Border ss:Position=\"Bottom\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Left\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Right\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Top\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	   </Borders>	");
            writer2.WriteLine("	   <Font ss:FontName=\"宋体\" x:CharSet=\"134\" ss:Size=\"12\" ss:Color=\"#339966\"/>	");
            writer2.WriteLine("	   <Interior ss:Color=\"#FCE4D6\" ss:Pattern=\"Solid\"/>	");
            writer2.WriteLine("	  </Style>	");
            writer2.WriteLine("	  <Style ss:ID=\"s84\">	");
            writer2.WriteLine("	   <Alignment ss:Horizontal=\"Center\" ss:Vertical=\"Center\"/>	");
            writer2.WriteLine("	   <Borders>	");
            writer2.WriteLine("	    <Border ss:Position=\"Bottom\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Left\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Right\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Top\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	   </Borders>	");
            writer2.WriteLine("	   <Font ss:FontName=\"宋体\" x:CharSet=\"134\" ss:Size=\"12\" ss:Color=\"#0000FF\"/>	");
            writer2.WriteLine("	   <Interior ss:Color=\"#FCE4D6\" ss:Pattern=\"Solid\"/>	");
            writer2.WriteLine("	  </Style>	");
            writer2.WriteLine("	  <Style ss:ID=\"s85\">	");
            writer2.WriteLine("	   <Alignment ss:Horizontal=\"Center\" ss:Vertical=\"Center\"/>	");
            writer2.WriteLine("	   <Borders>	");
            writer2.WriteLine("	    <Border ss:Position=\"Bottom\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Left\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Right\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Top\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	   </Borders>	");
            writer2.WriteLine("	   <Interior ss:Color=\"#FCE4D6\" ss:Pattern=\"Solid\"/>	");
            writer2.WriteLine("	  </Style>	");
            writer2.WriteLine("	  <Style ss:ID=\"s86\">	");
            writer2.WriteLine("	   <Alignment ss:Horizontal=\"Center\" ss:Vertical=\"Center\"/>	");
            writer2.WriteLine("	   <Borders>	");
            writer2.WriteLine("	    <Border ss:Position=\"Bottom\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Left\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Right\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Top\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	   </Borders>	");
            writer2.WriteLine("	   <Font ss:FontName=\"宋体\" x:CharSet=\"134\" ss:Size=\"12\" ss:Color=\"#339966\"/>	");
            writer2.WriteLine("	   <Interior/>	");
            writer2.WriteLine("	  </Style>	");
            writer2.WriteLine("	  <Style ss:ID=\"s87\">	");
            writer2.WriteLine("	   <Alignment ss:Horizontal=\"Center\" ss:Vertical=\"Center\"/>	");
            writer2.WriteLine("	   <Borders>	");
            writer2.WriteLine("	    <Border ss:Position=\"Bottom\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Left\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Right\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Top\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	   </Borders>	");
            writer2.WriteLine("	   <Font ss:FontName=\"宋体\" x:CharSet=\"134\" ss:Size=\"12\" ss:Color=\"#0000FF\"/>	");
            writer2.WriteLine("	   <Interior/>	");
            writer2.WriteLine("	  </Style>	");
            writer2.WriteLine("	  <Style ss:ID=\"s88\">	");
            writer2.WriteLine("	   <Alignment ss:Horizontal=\"Center\" ss:Vertical=\"Center\"/>	");
            writer2.WriteLine("	   <Borders>	");
            writer2.WriteLine("	    <Border ss:Position=\"Bottom\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Left\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Right\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Top\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	   </Borders>	");
            writer2.WriteLine("	   <Interior/>	");
            writer2.WriteLine("	  </Style>	");
            writer2.WriteLine("	  <Style ss:ID=\"s89\">	");
            writer2.WriteLine("	   <Alignment ss:Horizontal=\"Center\" ss:Vertical=\"Center\"/>	");
            writer2.WriteLine("	   <Borders>	");
            writer2.WriteLine("	    <Border ss:Position=\"Bottom\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Right\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Top\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	   </Borders>	");
            writer2.WriteLine("	   <Font ss:FontName=\"宋体\" x:CharSet=\"134\" ss:Size=\"12\" ss:Color=\"#339966\"/>	");
            writer2.WriteLine("	   <Interior ss:Color=\"#DDEBF7\" ss:Pattern=\"Solid\"/>	");
            writer2.WriteLine("	   <NumberFormat ss:Format=\"0.000_ \"/>	");
            writer2.WriteLine("	  </Style>	");
            writer2.WriteLine("	  <Style ss:ID=\"s90\">	");
            writer2.WriteLine("	   <Borders>	");
            writer2.WriteLine("	    <Border ss:Position=\"Bottom\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Left\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Right\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Top\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	   </Borders>	");
            writer2.WriteLine("	  </Style>	");
            writer2.WriteLine("	  <Style ss:ID=\"s91\">	");
            writer2.WriteLine("	   <Alignment ss:Horizontal=\"Center\" ss:Vertical=\"Center\"/>	");
            writer2.WriteLine("	   <Font ss:FontName=\"宋体\" x:CharSet=\"134\" ss:Size=\"12\" ss:Color=\"#339966\"/>	");
            writer2.WriteLine("	  </Style>	");
            writer2.WriteLine("	  <Style ss:ID=\"s92\">	");
            writer2.WriteLine("	   <Alignment ss:Horizontal=\"Center\" ss:Vertical=\"Center\"/>	");
            writer2.WriteLine("	   <Font ss:FontName=\"宋体\" x:CharSet=\"134\" ss:Size=\"12\" ss:Color=\"#0000FF\"/>	");
            writer2.WriteLine("	  </Style>	");
            writer2.WriteLine("	  <Style ss:ID=\"s93\">	");
            writer2.WriteLine("	   <Borders>	");
            writer2.WriteLine("	    <Border ss:Position=\"Bottom\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Left\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Right\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Top\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	   </Borders>	");
            writer2.WriteLine("	   <Interior ss:Color=\"#FCE4D6\" ss:Pattern=\"Solid\"/>	");
            writer2.WriteLine("	  </Style>	");
            writer2.WriteLine("	  <Style ss:ID=\"s94\">	");
            writer2.WriteLine("	   <Alignment ss:Horizontal=\"Center\" ss:Vertical=\"Center\"/>	");
            writer2.WriteLine("	   <Borders>	");
            writer2.WriteLine("	    <Border ss:Position=\"Bottom\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Left\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Right\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	    <Border ss:Position=\"Top\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>	");
            writer2.WriteLine("	   </Borders>	");
            writer2.WriteLine("	   <Font ss:FontName=\"宋体\" x:CharSet=\"134\" ss:Size=\"12\" ss:Bold=\"1\"/>	");
            writer2.WriteLine("	   <Interior/>	");
            writer2.WriteLine("	  </Style>	");
            writer2.WriteLine("	  <Style ss:ID=\"s95\">	");
            writer2.WriteLine("	   <Interior ss:Color=\"#FCE4D6\" ss:Pattern=\"Solid\"/>	");
            writer2.WriteLine("	  </Style>	");
            writer2.WriteLine("	  <Style ss:ID=\"s96\">	");
            writer2.WriteLine("	   <NumberFormat ss:Format=\"Short Date\"/>	");
            writer2.WriteLine("	  </Style>	");
            writer2.WriteLine("	  <Style ss:ID=\"s97\">	");
            writer2.WriteLine("	   <Alignment ss:Horizontal=\"Center\" ss:Vertical=\"Center\"/>	");
            writer2.WriteLine("	   <Borders/>	");
            writer2.WriteLine("	   <Font ss:FontName=\"宋体\" x:CharSet=\"134\" ss:Size=\"28\"/>	");
            writer2.WriteLine("	  </Style>	");
            writer2.WriteLine("	 </Styles>	");
            writer2.WriteLine("	 <Worksheet ss:Name=\"Sheet1\">	");
            writer2.WriteLine("	  <Table ss:ExpandedColumnCount=\"25\" ss:ExpandedRowCount=\"19\" x:FullColumns=\"1\"	");
            writer2.WriteLine("	   x:FullRows=\"1\" ss:DefaultColumnWidth=\"54\" ss:DefaultRowHeight=\"14.25\">	");
            writer2.WriteLine("	   <Column ss:StyleID=\"s65\" ss:AutoFitWidth=\"0\" ss:Width=\"75.75\"/>	");
            writer2.WriteLine("	   <Column ss:AutoFitWidth=\"0\" ss:Width=\"69\"/>	");
            writer2.WriteLine("	   <Column ss:AutoFitWidth=\"0\" ss:Width=\"62.25\"/>	");
            writer2.WriteLine("	   <Column ss:AutoFitWidth=\"0\" ss:Width=\"66\"/>	");
            writer2.WriteLine("	   <Column ss:StyleID=\"s66\" ss:AutoFitWidth=\"0\"/>	");
            writer2.WriteLine("	   <Column ss:AutoFitWidth=\"0\" ss:Width=\"62.25\" ss:Span=\"2\"/>	");
            writer2.WriteLine("	   <Column ss:Index=\"9\" ss:StyleID=\"s66\" ss:AutoFitWidth=\"0\"/>	");
            writer2.WriteLine("	   <Column ss:AutoFitWidth=\"0\" ss:Width=\"62.25\" ss:Span=\"2\"/>	");
            writer2.WriteLine("	   <Column ss:Index=\"13\" ss:StyleID=\"s66\" ss:AutoFitWidth=\"0\"/>	");
            writer2.WriteLine("	   <Column ss:Width=\"62.25\" ss:Span=\"1\"/>	");
            writer2.WriteLine("	   <Column ss:Index=\"16\" ss:Width=\"69\"/>	");
            writer2.WriteLine("	   <Column ss:StyleID=\"s66\" ss:AutoFitWidth=\"0\"/>	");
            writer2.WriteLine("	   <Column ss:Width=\"69\"/>	");
            writer2.WriteLine("	   <Column ss:Width=\"62.25\"/>	");
            writer2.WriteLine("	   <Column ss:Width=\"69\"/>	");
            writer2.WriteLine("	   <Column ss:StyleID=\"s66\" ss:AutoFitWidth=\"0\"/>	");
            writer2.WriteLine("	   <Column ss:Width=\"62.25\" ss:Span=\"2\"/>	");
            writer2.WriteLine("	   <Row>	");
            writer2.WriteLine("	    <Cell ss:Index=\"23\" ss:StyleID=\"s66\"/>	");
            writer2.WriteLine("	    <Cell ss:StyleID=\"s66\"/>	");
            writer2.WriteLine("	    <Cell ss:StyleID=\"s66\"/>	");
            writer2.WriteLine("	   </Row>	");
            writer2.WriteLine("	   <Row ss:Height=\"35.25\">	");
            writer2.WriteLine("	    <Cell ss:Index=\"2\" ss:MergeAcross=\"22\" ss:StyleID=\"s97\"><Data ss:Type=\"String\">" + year + "</Data></Cell>	");
            writer2.WriteLine("	   </Row>	");
            writer2.WriteLine("	   <Row ss:AutoFitHeight=\"0\" ss:Height=\"15.9375\" ss:StyleID=\"s64\">	");
            writer2.WriteLine("	    <Cell ss:Index=\"2\" ss:MergeAcross=\"2\" ss:StyleID=\"s69\"><Data ss:Type=\"String\">1月</Data></Cell>	");
            writer2.WriteLine("	    <Cell ss:StyleID=\"s70\"/>	");
            writer2.WriteLine("	    <Cell ss:MergeAcross=\"2\" ss:StyleID=\"s69\"><Data ss:Type=\"String\">2月</Data></Cell>	");
            writer2.WriteLine("	    <Cell ss:StyleID=\"s70\"/>	");
            writer2.WriteLine("	    <Cell ss:MergeAcross=\"2\" ss:StyleID=\"s69\"><Data ss:Type=\"String\">3月</Data></Cell>	");
            writer2.WriteLine("	    <Cell ss:StyleID=\"s70\"/>	");
            writer2.WriteLine("	    <Cell ss:MergeAcross=\"2\" ss:StyleID=\"s69\"><Data ss:Type=\"String\">4月</Data></Cell>	");
            writer2.WriteLine("	    <Cell ss:StyleID=\"s70\"/>	");
            writer2.WriteLine("	    <Cell ss:MergeAcross=\"2\" ss:StyleID=\"s69\"><Data ss:Type=\"String\">5月</Data></Cell>	");
            writer2.WriteLine("	    <Cell ss:StyleID=\"s70\"/>	");
            writer2.WriteLine("	    <Cell ss:MergeAcross=\"2\" ss:StyleID=\"s69\"><Data ss:Type=\"String\">6月</Data></Cell>	");
            writer2.WriteLine("	   </Row>	");
            writer2.WriteLine("	   <Row ss:StyleID=\"s64\">	");
            writer2.WriteLine("	    <Cell ss:Index=\"2\" ss:StyleID=\"s69\"><Data ss:Type=\"String\">进库</Data></Cell>	");
            writer2.WriteLine("	    <Cell ss:StyleID=\"s69\"><Data ss:Type=\"String\">出库</Data></Cell>	");
            writer2.WriteLine("	    <Cell ss:StyleID=\"s69\"><Data ss:Type=\"String\">库存</Data></Cell>	");
            writer2.WriteLine("	    <Cell ss:StyleID=\"s70\"/>	");
            writer2.WriteLine("	    <Cell ss:StyleID=\"s69\"><Data ss:Type=\"String\">进库</Data></Cell>	");
            writer2.WriteLine("	    <Cell ss:StyleID=\"s69\"><Data ss:Type=\"String\">出库</Data></Cell>	");
            writer2.WriteLine("	    <Cell ss:StyleID=\"s69\"><Data ss:Type=\"String\">库存</Data></Cell>	");
            writer2.WriteLine("	    <Cell ss:StyleID=\"s70\"/>	");
            writer2.WriteLine("	    <Cell ss:StyleID=\"s69\"><Data ss:Type=\"String\">进库</Data></Cell>	");
            writer2.WriteLine("	    <Cell ss:StyleID=\"s69\"><Data ss:Type=\"String\">出库</Data></Cell>	");
            writer2.WriteLine("	    <Cell ss:StyleID=\"s69\"><Data ss:Type=\"String\">库存</Data></Cell>	");
            writer2.WriteLine("	    <Cell ss:StyleID=\"s70\"/>	");
            writer2.WriteLine("	    <Cell ss:StyleID=\"s69\"><Data ss:Type=\"String\">进库</Data></Cell>	");
            writer2.WriteLine("	    <Cell ss:StyleID=\"s69\"><Data ss:Type=\"String\">出库</Data></Cell>	");
            writer2.WriteLine("	    <Cell ss:StyleID=\"s69\"><Data ss:Type=\"String\">库存</Data></Cell>	");
            writer2.WriteLine("	    <Cell ss:StyleID=\"s70\"/>	");
            writer2.WriteLine("	    <Cell ss:StyleID=\"s69\"><Data ss:Type=\"String\">进库</Data></Cell>	");
            writer2.WriteLine("	    <Cell ss:StyleID=\"s69\"><Data ss:Type=\"String\">出库</Data></Cell>	");
            writer2.WriteLine("	    <Cell ss:StyleID=\"s69\"><Data ss:Type=\"String\">库存</Data></Cell>	");
            writer2.WriteLine("	    <Cell ss:StyleID=\"s70\"/>	");
            writer2.WriteLine("	    <Cell ss:StyleID=\"s69\"><Data ss:Type=\"String\">进库</Data></Cell>	");
            writer2.WriteLine("	    <Cell ss:StyleID=\"s69\"><Data ss:Type=\"String\">出库</Data></Cell>	");
            writer2.WriteLine("	    <Cell ss:StyleID=\"s69\"><Data ss:Type=\"String\">库存</Data></Cell>	");
            writer2.WriteLine("	   </Row>	");

            OperatorModel loginModel = OperatorProvider.Provider.GetCurrent();
            var yuanquid = loginModel.YuanquID ?? throw new ArgumentNullException("loginModel.YuanquID");

            for (int i = 0; i < list_Goods.Count; i++)
            {
                List<Stock_Month> Stocklist = new List<Stock_Month>();
                for (int i_six = 0; i_six < 6; i_six++)
                {
                    int days = DateTime.DaysInMonth(Convert.ToInt32(year), i_six + 1);
                    DateTime starTime = Convert.ToDateTime(year + "-" + (i_six + 1).ToString() + "-" + "01 00:00:00");
                    DateTime endTime = Convert.ToDateTime(year + "-" + (i_six + 1).ToString() + "-" + days.ToString() + " 23:59:59");
                    List<C_TB_HS_STOCKDORMANT> list = db.C_TB_HS_STOCKDORMANT.Where(n => n.LastEidTime >= starTime && n.LastEidTime <= endTime && n.YuanQuID == yuanquid).ToList();
                    if (list.Count == 0)
                    {
                        Stock_Month model = new Stock_Month()
                        {
                            ID = i,
                            Time = starTime.AddDays(-1),
                            Goodname = "",
                            InAMOUNT = "0",
                            InWEIGHT = "0",
                            OutAMOUNT = "0",
                            OutWEIGHT = "0",
                            AllAMOUNT = "0",
                            AllWEIGHT = "0",
                        };
                        Stocklist.Add(model);
                    }
                    else
                    {
                        //Array goodsname = list.Select(n => n.GoodsName).Distinct().ToArray();
                        decimal? InAMOUNT, InWEIGHT, OutAMOUNT, OutWEIGHT, AllAMOUNT, AllWEIGHT;
                        string goodname = list_Goods[i].GoodsName.ToString();
                        InAMOUNT = list.Where(n => n.LastEidTime != null && (n.GoodsName == goodname.ToString() && n.REMARK == "进库" && n.LastEidTime.Value.Date >= starTime && n.LastEidTime.Value.Date <= endTime)).Sum(n => n.AMOUNT);
                        InWEIGHT = list.Where(n => n.LastEidTime != null && (n.GoodsName == goodname.ToString() && n.REMARK == "进库" && n.LastEidTime.Value.Date >= starTime && n.LastEidTime.Value.Date <= endTime)).Sum(n => n.WEIGHT);
                        OutAMOUNT = list.Where(n => n.LastEidTime != null && (n.GoodsName == goodname.ToString() && n.REMARK == "出库" && n.LastEidTime.Value.Date >= starTime && n.LastEidTime.Value.Date <= endTime)).Sum(n => n.AMOUNT);
                        OutWEIGHT = list.Where(n => n.LastEidTime != null && (n.GoodsName == goodname.ToString() && n.REMARK == "出库" && n.LastEidTime.Value.Date >= starTime && n.LastEidTime.Value.Date <= endTime)).Sum(n => n.WEIGHT);
                        AllAMOUNT = list.Where(n => n.LastEidTime != null && (n.GoodsName == goodname.ToString() && n.LastEidTime.Value.Date <= endTime)).Sum(n => n.AMOUNT);
                        AllWEIGHT = list.Where(n => n.LastEidTime != null && (n.GoodsName == goodname.ToString() && n.LastEidTime.Value.Date <= endTime)).Sum(n => n.WEIGHT);
                        Stock_Month model = new Stock_Month()
                        {
                            ID = i,
                            Goodname = goodname,
                            InAMOUNT = InAMOUNT.ToString(),
                            InWEIGHT = InWEIGHT.ToString(),
                            OutAMOUNT = OutAMOUNT.ToString(),
                            OutWEIGHT = OutWEIGHT.ToString(),
                            AllAMOUNT = AllAMOUNT.ToString(),
                            AllWEIGHT = AllWEIGHT.ToString(),
                        };
                        Stocklist.Add(model);

                    }


                }
                writer2.WriteLine("	   <Row ss:Height=\"20.25\">	");
                writer2.WriteLine("	    <Cell ss:StyleID=\"s71\"><Data ss:Type=\"String\">" + list_Goods[i].GoodsName + "</Data></Cell>	");
                int num = -2;
                for (int i_Socket = 0; i_Socket < Stocklist.Count; i_Socket++)
                {
                    num += 4;
                    writer2.WriteLine("	    <Cell ss:Index=\"" + num + "\" ss:StyleID=\"s76\"><Data ss:Type=\"Number\">" + Stocklist[i_Socket].InWEIGHT + "</Data></Cell>	");
                    writer2.WriteLine("	    <Cell ss:StyleID=\"s77\"><Data ss:Type=\"Number\">" + Stocklist[i_Socket].OutWEIGHT + "</Data></Cell>	");
                    writer2.WriteLine("	    <Cell ss:StyleID=\"s78\"><Data ss:Type=\"Number\">" + Stocklist[i_Socket].AllWEIGHT + "</Data></Cell>	");
                }

                writer2.WriteLine("	   </Row>	");
            }

            writer2.WriteLine("	   <Row ss:Index=\"11\">	");
            writer2.WriteLine("	    <Cell ss:StyleID=\"s64\"/>	");
            writer2.WriteLine("	    <Cell ss:MergeAcross=\"2\" ss:StyleID=\"s69\"><Data ss:Type=\"String\">7月</Data></Cell>	");
            writer2.WriteLine("	    <Cell ss:Index=\"6\" ss:MergeAcross=\"2\" ss:StyleID=\"s69\"><Data ss:Type=\"String\">8月</Data></Cell>	");
            writer2.WriteLine("	    <Cell ss:Index=\"10\" ss:MergeAcross=\"2\" ss:StyleID=\"s69\"><Data ss:Type=\"String\">9月</Data></Cell>	");
            writer2.WriteLine("	    <Cell ss:Index=\"14\" ss:MergeAcross=\"2\" ss:StyleID=\"s69\"><Data ss:Type=\"String\">10月</Data></Cell>	");
            writer2.WriteLine("	    <Cell ss:Index=\"18\" ss:MergeAcross=\"2\" ss:StyleID=\"s69\"><Data ss:Type=\"String\">11月</Data></Cell>	");
            writer2.WriteLine("	    <Cell ss:Index=\"22\" ss:MergeAcross=\"2\" ss:StyleID=\"s69\"><Data ss:Type=\"String\">12月</Data></Cell>	");
            writer2.WriteLine("	   </Row>	");
            writer2.WriteLine("	   <Row>	");
            writer2.WriteLine("	    <Cell ss:StyleID=\"s64\"/>	");
            writer2.WriteLine("	    <Cell ss:StyleID=\"s69\"><Data ss:Type=\"String\">进库</Data></Cell>	");
            writer2.WriteLine("	    <Cell ss:StyleID=\"s69\"><Data ss:Type=\"String\">出库</Data></Cell>	");
            writer2.WriteLine("	    <Cell ss:StyleID=\"s69\"><Data ss:Type=\"String\">库存</Data></Cell>	");
            writer2.WriteLine("	    <Cell ss:Index=\"6\" ss:StyleID=\"s69\"><Data ss:Type=\"String\">进库</Data></Cell>	");
            writer2.WriteLine("	    <Cell ss:StyleID=\"s69\"><Data ss:Type=\"String\">出库</Data></Cell>	");
            writer2.WriteLine("	    <Cell ss:StyleID=\"s69\"><Data ss:Type=\"String\">库存</Data></Cell>	");
            writer2.WriteLine("	    <Cell ss:Index=\"10\" ss:StyleID=\"s69\"><Data ss:Type=\"String\">进库</Data></Cell>	");
            writer2.WriteLine("	    <Cell ss:StyleID=\"s69\"><Data ss:Type=\"String\">出库</Data></Cell>	");
            writer2.WriteLine("	    <Cell ss:StyleID=\"s69\"><Data ss:Type=\"String\">库存</Data></Cell>	");
            writer2.WriteLine("	    <Cell ss:Index=\"14\" ss:StyleID=\"s69\"><Data ss:Type=\"String\">进库</Data></Cell>	");
            writer2.WriteLine("	    <Cell ss:StyleID=\"s69\"><Data ss:Type=\"String\">出库</Data></Cell>	");
            writer2.WriteLine("	    <Cell ss:StyleID=\"s69\"><Data ss:Type=\"String\">库存</Data></Cell>	");
            writer2.WriteLine("	    <Cell ss:Index=\"18\" ss:StyleID=\"s69\"><Data ss:Type=\"String\">进库</Data></Cell>	");
            writer2.WriteLine("	    <Cell ss:StyleID=\"s69\"><Data ss:Type=\"String\">出库</Data></Cell>	");
            writer2.WriteLine("	    <Cell ss:StyleID=\"s69\"><Data ss:Type=\"String\">库存</Data></Cell>	");
            writer2.WriteLine("	    <Cell ss:Index=\"22\" ss:StyleID=\"s69\"><Data ss:Type=\"String\">进库</Data></Cell>	");
            writer2.WriteLine("	    <Cell ss:StyleID=\"s94\"><Data ss:Type=\"String\">出库</Data></Cell>	");
            writer2.WriteLine("	    <Cell ss:StyleID=\"s94\"><Data ss:Type=\"String\">库存</Data></Cell>	");
            writer2.WriteLine("	    <Cell ss:StyleID=\"s66\"/>	");
            writer2.WriteLine("	   </Row>	");
            for (int i = 0; i < list_Goods.Count; i++)
            {
                List<Stock_Month> Stocklist = new List<Stock_Month>();
                for (int i_six = 0; i_six < 6; i_six++)
                {
                    int days = DateTime.DaysInMonth(Convert.ToInt32(year), i_six + 7);
                    DateTime starTime = Convert.ToDateTime(year + "-" + (i_six + 7).ToString() + "-" + "01 00:00:00");
                    DateTime endTime = Convert.ToDateTime(year + "-" + (i_six + 7).ToString() + "-" + days.ToString() + " 23:59:59");
                    List<C_TB_HS_STOCKDORMANT> list = db.C_TB_HS_STOCKDORMANT.Where(n => n.LastEidTime >= starTime && n.LastEidTime <= endTime && n.YuanQuID == yuanquid).ToList();
                    if (list.Count == 0)
                    {
                        Stock_Month model = new Stock_Month()
                        {
                            ID = i,
                            Time = starTime.AddDays(-1),
                            Goodname = "",
                            InAMOUNT = "0",
                            InWEIGHT = "0",
                            OutAMOUNT = "0",
                            OutWEIGHT = "0",
                            AllAMOUNT = "0",
                            AllWEIGHT = "0",
                        };
                        Stocklist.Add(model);
                    }
                    else
                    {
                        //Array goodsname = list.Select(n => n.GoodsName).Distinct().ToArray();
                        decimal? InAMOUNT, InWEIGHT, OutAMOUNT, OutWEIGHT, AllAMOUNT, AllWEIGHT;
                        string goodname = list_Goods[i].GoodsName.ToString();
                        InAMOUNT = list.Where(n => n.LastEidTime != null && (n.GoodsName == goodname.ToString() && n.REMARK == "进库" && n.LastEidTime.Value.Date >= starTime && n.LastEidTime.Value.Date <= endTime)).Sum(n => n.AMOUNT);
                        InWEIGHT = list.Where(n => n.LastEidTime != null && (n.GoodsName == goodname.ToString() && n.REMARK == "进库" && n.LastEidTime.Value.Date >= starTime && n.LastEidTime.Value.Date <= endTime)).Sum(n => n.WEIGHT);
                        OutAMOUNT = list.Where(n => n.LastEidTime != null && (n.GoodsName == goodname.ToString() && n.REMARK == "出库" && n.LastEidTime.Value.Date >= starTime && n.LastEidTime.Value.Date <= endTime)).Sum(n => n.AMOUNT);
                        OutWEIGHT = list.Where(n => n.LastEidTime != null && (n.GoodsName == goodname.ToString() && n.REMARK == "出库" && n.LastEidTime.Value.Date >= starTime && n.LastEidTime.Value.Date <= endTime)).Sum(n => n.WEIGHT);
                        AllAMOUNT = list.Where(n => n.LastEidTime != null && (n.GoodsName == goodname.ToString() && n.LastEidTime.Value.Date <= endTime)).Sum(n => n.AMOUNT);
                        AllWEIGHT = list.Where(n => n.LastEidTime != null && (n.GoodsName == goodname.ToString() && n.LastEidTime.Value.Date <= endTime)).Sum(n => n.WEIGHT);
                        Stock_Month model = new Stock_Month()
                        {
                            ID = i,
                            Goodname = goodname,
                            InAMOUNT = InAMOUNT.ToString(),
                            InWEIGHT = InWEIGHT.ToString(),
                            OutAMOUNT = OutAMOUNT.ToString(),
                            OutWEIGHT = OutWEIGHT.ToString(),
                            AllAMOUNT = AllAMOUNT.ToString(),
                            AllWEIGHT = AllWEIGHT.ToString(),
                        };
                        Stocklist.Add(model);

                    }


                }
                writer2.WriteLine("	   <Row ss:Height=\"20.25\">	");
                writer2.WriteLine("	    <Cell ss:StyleID=\"s71\"><Data ss:Type=\"String\">" + list_Goods[i].GoodsName + "</Data></Cell>	");
                int num = -2;
                for (int i_Socket = 0; i_Socket < Stocklist.Count; i_Socket++)
                {
                    num += 4;
                    writer2.WriteLine("	    <Cell ss:Index=\"" + num + "\" ss:StyleID=\"s76\"><Data ss:Type=\"Number\">" + Stocklist[i_Socket].InWEIGHT + "</Data></Cell>	");
                    writer2.WriteLine("	    <Cell ss:StyleID=\"s77\"><Data ss:Type=\"Number\">" + Stocklist[i_Socket].OutWEIGHT + "</Data></Cell>	");
                    writer2.WriteLine("	    <Cell ss:StyleID=\"s78\"><Data ss:Type=\"Number\">" + Stocklist[i_Socket].AllWEIGHT + "</Data></Cell>	");
                }

                writer2.WriteLine("	   </Row>	");
            }



            writer2.WriteLine("	   <Row ss:Index=\"19\" ss:AutoFitHeight=\"0\" ss:Height=\"36\">	");
            writer2.WriteLine("	    <Cell ss:Index=\"19\"><Data ss:Type=\"String\">制表日期：</Data></Cell>	");
            writer2.WriteLine("	    <Cell ss:StyleID=\"s96\"><Data ss:Type=\"String\">" + DateTime.Today + "</Data></Cell>	");
            writer2.WriteLine("	   </Row>	");
            writer2.WriteLine("	  </Table>	");
            writer2.WriteLine("	  <WorksheetOptions xmlns=\"urn:schemas-microsoft-com:office:excel\">	");
            writer2.WriteLine("	   <PageSetup>	");
            writer2.WriteLine("	    <Header x:Margin=\"0.51\"/>	");
            writer2.WriteLine("	    <Footer x:Margin=\"0.51\"/>	");
            writer2.WriteLine("	   </PageSetup>	");
            writer2.WriteLine("	   <Print>	");
            writer2.WriteLine("	    <ValidPrinterInfo/>	");
            writer2.WriteLine("	    <PaperSizeIndex>9</PaperSizeIndex>	");
            writer2.WriteLine("	    <VerticalResolution>0</VerticalResolution>	");
            writer2.WriteLine("	   </Print>	");
            writer2.WriteLine("	   <Zoom>77</Zoom>	");
            writer2.WriteLine("	   <PageBreakZoom>100</PageBreakZoom>	");
            writer2.WriteLine("	   <Selected/>	");
            writer2.WriteLine("	   <FreezePanes/>	");
            writer2.WriteLine("	   <FrozenNoSplit/>	");
            writer2.WriteLine("	   <SplitVertical>1</SplitVertical>	");
            writer2.WriteLine("	   <LeftColumnRightPane>1</LeftColumnRightPane>	");
            writer2.WriteLine("	   <ActivePane>1</ActivePane>	");
            writer2.WriteLine("	   <Panes>	");
            writer2.WriteLine("	    <Pane>	");
            writer2.WriteLine("	     <Number>3</Number>	");
            writer2.WriteLine("	    </Pane>	");
            writer2.WriteLine("	    <Pane>	");
            writer2.WriteLine("	     <Number>1</Number>	");
            writer2.WriteLine("	     <ActiveRow>8</ActiveRow>	");
            writer2.WriteLine("	     <ActiveCol>0</ActiveCol>	");
            writer2.WriteLine("	    </Pane>	");
            writer2.WriteLine("	   </Panes>	");
            writer2.WriteLine("	   <ProtectObjects>False</ProtectObjects>	");
            writer2.WriteLine("	   <ProtectScenarios>False</ProtectScenarios>	");
            writer2.WriteLine("	  </WorksheetOptions>	");
            writer2.WriteLine("	 </Worksheet>	");
            writer2.WriteLine("	</Workbook>	");
            writer2 = null;
            writer1.Close();

            // MyComm.RegisterScript(UpdatePanel2, this, "window.open('../QueryOut/处罚信息表(" + sUserName + ").xls','','')");
            string filePath = string.Format("/QueryOut//" + "年报表" + ".xls", DateTime.Now, "年报表");
            return Json(new AjaxResult { state = ResultType.success.ToString(), message = filePath }.ToJson());
        }
        public JsonResult WorkNumToExcel_ribao(string date, string date1)
        {
            OperatorModel loginModel = OperatorProvider.Provider.GetCurrent();

            if (string.IsNullOrEmpty(date))
            {
                return Json(new AjaxResult { state = ResultType.error.ToString(), message = "请选择日期！" }.ToJson());
            }

            C_Dic_YuanQu model_yuanqu = db.C_Dic_YuanQu.FirstOrDefault(n => n.ID == loginModel.YuanquID);

            string text1 = Server.MapPath("..//QueryOut//" + "新路代日报表" + ".xls");

            StreamWriter writer1 = new StreamWriter(text1, false);
            StreamWriter writer2 = writer1;
            writer2.WriteLine("	<?xml version=\"1.0\"?>	");
            writer2.WriteLine("	<?mso-application progid=\"Excel.Sheet\"?>	");
            writer2.WriteLine("	<Workbook xmlns=\"urn:schemas-microsoft-com:office:spreadsheet\"	");
            writer2.WriteLine("	 xmlns:o=\"urn:schemas-microsoft-com:office:office\"	");
            writer2.WriteLine("	 xmlns:x=\"urn:schemas-microsoft-com:office:excel\"	");
            writer2.WriteLine("	 xmlns:dt=\"uuid:C2F41010-65B3-11d1-A29F-00AA00C14882\"	");
            writer2.WriteLine("	 xmlns:ss=\"urn:schemas-microsoft-com:office:spreadsheet\"	");
            writer2.WriteLine("	 xmlns:html=\"http://www.w3.org/TR/REC-html40\">	");
            writer2.WriteLine("	 <DocumentProperties xmlns=\"urn:schemas-microsoft-com:office:office\">	");
            writer2.WriteLine("	  <Author>kaifa</Author>	");
            writer2.WriteLine("	  <LastAuthor>kaifa</LastAuthor>	");
            writer2.WriteLine("	  <Revision>1</Revision>	");
            writer2.WriteLine("	  <LastPrinted>2014-06-13T06:19:53Z</LastPrinted>	");
            writer2.WriteLine("	  <Created>1996-12-17T01:32:42Z</Created>	");
            writer2.WriteLine("	  <LastSaved>2018-11-28T01:52:54Z</LastSaved>	");
            writer2.WriteLine("	  <Version>16.00</Version>	");
            writer2.WriteLine("	 </DocumentProperties>	");
            writer2.WriteLine("	 <CustomDocumentProperties xmlns=\"urn:schemas-microsoft-com:office:office\">	");
            writer2.WriteLine("	  <KSOProductBuildVer dt:dt=\"string\">2052-11.1.0.7989</KSOProductBuildVer>	");
            writer2.WriteLine("	  <KSOReadingLayout dt:dt=\"boolean\">0</KSOReadingLayout>	");
            writer2.WriteLine("	 </CustomDocumentProperties>	");
            writer2.WriteLine("	 <OfficeDocumentSettings xmlns=\"urn:schemas-microsoft-com:office:office\">	");
            writer2.WriteLine("	  <AllowPNG/>	");
            writer2.WriteLine("	 </OfficeDocumentSettings>	");
            writer2.WriteLine("	 <ExcelWorkbook xmlns=\"urn:schemas-microsoft-com:office:excel\">	");
            writer2.WriteLine("	  <WindowHeight>9765</WindowHeight>	");
            writer2.WriteLine("	  <WindowWidth>23445</WindowWidth>	");
            writer2.WriteLine("	  <WindowTopX>32760</WindowTopX>	");
            writer2.WriteLine("	  <WindowTopY>32760</WindowTopY>	");
            writer2.WriteLine("	  <TabRatio>873</TabRatio>	");
            writer2.WriteLine("	  <ProtectStructure>False</ProtectStructure>	");
            writer2.WriteLine("	  <ProtectWindows>False</ProtectWindows>	");
            writer2.WriteLine("	  <DisplayInkNotes>False</DisplayInkNotes>	");
            writer2.WriteLine("	 </ExcelWorkbook>	");
            writer2.WriteLine("	 <Styles>	");
            writer2.WriteLine("	  <Style ss:ID=\"Default\" ss:Name=\"Normal\">	");
            writer2.WriteLine("	   <Alignment ss:Vertical=\"Bottom\"/>	");
            writer2.WriteLine("	   <Borders/>	");
            writer2.WriteLine("	   <Font ss:FontName=\"宋体\" x:CharSet=\"134\" ss:Size=\"12\"/>	");
            writer2.WriteLine("	   <Interior/>	");
            writer2.WriteLine("	   <NumberFormat/>	");
            writer2.WriteLine("	   <Protection/>	");
            writer2.WriteLine("	  </Style>	");
            writer2.WriteLine("	  <Style ss:ID=\"s64\">	");
            writer2.WriteLine("	   <Alignment ss:Vertical=\"Bottom\" ss:WrapText=\"1\"/>	");
            writer2.WriteLine("	   <Font ss:FontName=\"宋体\" x:CharSet=\"134\" ss:Size=\"12\"/>	");
            writer2.WriteLine("	  </Style>	");
            writer2.WriteLine("	  <Style ss:ID=\"s65\">	");
            writer2.WriteLine("	   <Font ss:FontName=\"宋体\" x:CharSet=\"134\" ss:Size=\"12\"/>	");
            writer2.WriteLine("	  </Style>	");
            writer2.WriteLine("	  <Style ss:ID=\"s66\">	");
            writer2.WriteLine("	   <Font ss:FontName=\"宋体\" x:CharSet=\"134\" ss:Size=\"12\" ss:Color=\"#0000FF\"/>	");
            writer2.WriteLine("	  </Style>	");
            writer2.WriteLine("	  <Style ss:ID=\"s68\">	");
            writer2.WriteLine("	   <Font ss:FontName=\"宋体\" x:CharSet=\"134\" ss:Size=\"12\" ss:Color=\"#800080\"/>	");
            writer2.WriteLine("	  </Style>	");
            writer2.WriteLine("	  <Style ss:ID=\"s71\">	");
            writer2.WriteLine("	   <Alignment ss:Horizontal=\"Center\" ss:Vertical=\"Bottom\" ss:WrapText=\"1\"/>	");
            writer2.WriteLine("	   <Font ss:FontName=\"宋体\" x:CharSet=\"134\" ss:Size=\"12\"/>	");
            writer2.WriteLine("	  </Style>	");
            writer2.WriteLine("	  <Style ss:ID=\"s73\">	");
            writer2.WriteLine("	   <Font ss:FontName=\"宋体\" x:CharSet=\"134\" ss:Size=\"11\"/>	");
            writer2.WriteLine("	  </Style>	");
            writer2.WriteLine("	  <Style ss:ID=\"s80\">	");
            writer2.WriteLine("	   <Alignment ss:Horizontal=\"Center\" ss:Vertical=\"Bottom\"/>	");
            writer2.WriteLine("	   <Font ss:FontName=\"宋体\" x:CharSet=\"134\" ss:Size=\"12\"/>	");
            writer2.WriteLine("	  </Style>	");
            writer2.WriteLine("	  <Style ss:ID=\"s81\">	");
            writer2.WriteLine("	   <Font ss:FontName=\"宋体\" x:CharSet=\"134\" ss:Size=\"12\" ss:Color=\"#339966\"/>	");
            writer2.WriteLine("	  </Style>	");
            writer2.WriteLine("	  <Style ss:ID=\"s87\">	");
            writer2.WriteLine("	   <Font ss:FontName=\"宋体\" x:CharSet=\"134\" ss:Size=\"12\"/>	");
            writer2.WriteLine("	   <Interior/>	");
            writer2.WriteLine("	  </Style>	");
            writer2.WriteLine("	  <Style ss:ID=\"s117\">	");
            writer2.WriteLine("	   <Font ss:FontName=\"宋体\" x:CharSet=\"134\" ss:Size=\"12\" ss:Color=\"#800080\"/>	");
            writer2.WriteLine("	   <Interior/>	");
            writer2.WriteLine("	  </Style>	");
            writer2.WriteLine("	  <Style ss:ID=\"s118\">	");
            writer2.WriteLine("	   <Font ss:FontName=\"宋体\" x:CharSet=\"134\" ss:Size=\"12\" ss:Color=\"#008E40\"	");
            writer2.WriteLine("	    ss:Bold=\"1\"/>	");
            writer2.WriteLine("	  </Style>	");
            writer2.WriteLine("	  <Style ss:ID=\"s121\">	");
            writer2.WriteLine("	   <Alignment ss:Horizontal=\"Center\" ss:Vertical=\"Bottom\"/>	");
            writer2.WriteLine("	   <Font ss:FontName=\"宋体\" x:CharSet=\"134\" ss:Size=\"12\"/>	");
            writer2.WriteLine("	   <Interior/>	");
            writer2.WriteLine("	  </Style>	");
            writer2.WriteLine("	  <Style ss:ID=\"s122\">	");
            writer2.WriteLine("	   <Alignment ss:Horizontal=\"Right\" ss:Vertical=\"Bottom\"/>	");
            writer2.WriteLine("	   <Font ss:FontName=\"宋体\" x:CharSet=\"134\" ss:Size=\"12\" ss:Color=\"#C65911\"/>	");
            writer2.WriteLine("	   <Interior/>	");
            writer2.WriteLine("	  </Style>	");
            writer2.WriteLine("	  <Style ss:ID=\"s124\">	");
            writer2.WriteLine("	   <Alignment ss:Horizontal=\"Center\" ss:Vertical=\"Bottom\" ss:WrapText=\"1\"/>	");
            writer2.WriteLine("	   <Font ss:FontName=\"宋体\" x:CharSet=\"134\" ss:Size=\"12\" ss:Color=\"#C65911\"/>	");
            writer2.WriteLine("	   <Interior/>	");
            writer2.WriteLine("	  </Style>	");
            writer2.WriteLine("	  <Style ss:ID=\"s126\">	");
            writer2.WriteLine("	   <Font ss:FontName=\"宋体\" x:CharSet=\"134\" ss:Size=\"12\" ss:Color=\"#0000FF\"/>	");
            writer2.WriteLine("	  </Style>	");
            writer2.WriteLine("	  <Style ss:ID=\"s127\">	");
            writer2.WriteLine("	   <Font ss:FontName=\"宋体\" x:CharSet=\"134\" ss:Size=\"12\" ss:Color=\"#C65911\"/>	");
            writer2.WriteLine("	   <Interior/>	");
            writer2.WriteLine("	  </Style>	");
            writer2.WriteLine("	  <Style ss:ID=\"s130\">	");
            writer2.WriteLine("	   <Alignment ss:Horizontal=\"Center\" ss:Vertical=\"Bottom\" ss:WrapText=\"1\"/>	");
            writer2.WriteLine("	   <Font ss:FontName=\"宋体\" x:CharSet=\"134\" ss:Size=\"12\" ss:Color=\"#0000FF\"/>	");
            writer2.WriteLine("	  </Style>	");
            writer2.WriteLine("	  <Style ss:ID=\"s131\">	");
            writer2.WriteLine("	   <Alignment ss:Horizontal=\"Center\" ss:Vertical=\"Bottom\" ss:WrapText=\"1\"/>	");
            writer2.WriteLine("	   <Font ss:FontName=\"宋体\" x:CharSet=\"134\" ss:Size=\"12\" ss:Color=\"#339966\"/>	");
            writer2.WriteLine("	  </Style>	");
            writer2.WriteLine("	  <Style ss:ID=\"s132\">	");
            writer2.WriteLine("	   <Alignment ss:Horizontal=\"Center\" ss:Vertical=\"Bottom\" ss:WrapText=\"1\"/>	");
            writer2.WriteLine("	   <Font ss:FontName=\"宋体\" x:CharSet=\"134\" ss:Size=\"12\" ss:Color=\"#800080\"/>	");
            writer2.WriteLine("	  </Style>	");
            writer2.WriteLine("	 </Styles>	");
            TimeSpan? ts_Mdc = Convert.ToDateTime(date1) - Convert.ToDateTime(date);
            int Days = ts_Mdc.Value.Days;
            if (Days < 0)
            {
                writer2 = null;
                writer1.Close();
                return Json(new AjaxResult { state = ResultType.error.ToString(), message = "结束时间不得早于开始时间！" }.ToJson());
            }
            for (int i_date = 0; i_date <= Days; i_date++)
            {
                DateTime starttime = Convert.ToDateTime(date + " 00:00:00").AddDays(i_date);
                DateTime endtime = Convert.ToDateTime(date + " 23:59:59").AddDays(i_date);
                writer2.WriteLine("	 <Worksheet ss:Name=\"" + starttime.ToDateString() + "\">	");
                writer2.WriteLine("	  <Table ss:ExpandedColumnCount=\"202\" ss:ExpandedRowCount=\"44\" x:FullColumns=\"1\"	");
                writer2.WriteLine("	   x:FullRows=\"1\" ss:DefaultColumnWidth=\"54\" ss:DefaultRowHeight=\"14.25\">	");
                writer2.WriteLine("	   <Column ss:StyleID=\"s65\" ss:AutoFitWidth=\"0\" ss:Width=\"75\"/>	");
                writer2.WriteLine("	   <Column ss:StyleID=\"s65\" ss:AutoFitWidth=\"0\" ss:Width=\"52.5\"/>	");
                writer2.WriteLine("	   <Column ss:StyleID=\"s65\" ss:AutoFitWidth=\"0\" ss:Width=\"58.5\"/>	");
                writer2.WriteLine("	   <Column ss:StyleID=\"s65\" ss:AutoFitWidth=\"0\" ss:Width=\"41.25\"/>	");
                writer2.WriteLine("	   <Column ss:StyleID=\"s65\" ss:AutoFitWidth=\"0\" ss:Width=\"44.25\"/>	");
                writer2.WriteLine("	   <Column ss:StyleID=\"s65\" ss:AutoFitWidth=\"0\" ss:Width=\"58.5\"/>	");
                writer2.WriteLine("	   <Column ss:StyleID=\"s65\" ss:AutoFitWidth=\"0\" ss:Width=\"43.5\"/>	");
                writer2.WriteLine("	   <Column ss:StyleID=\"s65\" ss:AutoFitWidth=\"0\" ss:Width=\"60\"/>	");
                writer2.WriteLine("	   <Column ss:StyleID=\"s126\" ss:AutoFitWidth=\"0\" ss:Width=\"31.5\"/>	");
                writer2.WriteLine("	   <Column ss:StyleID=\"s126\" ss:AutoFitWidth=\"0\" ss:Width=\"52.5\" ss:Span=\"1\"/>	");
                writer2.WriteLine("	   <Column ss:Index=\"12\" ss:StyleID=\"s81\" ss:AutoFitWidth=\"0\" ss:Width=\"33.75\"/>	");
                writer2.WriteLine("	   <Column ss:StyleID=\"s81\" ss:AutoFitWidth=\"0\" ss:Width=\"45.75\"/>	");
                writer2.WriteLine("	   <Column ss:StyleID=\"s81\" ss:AutoFitWidth=\"0\" ss:Width=\"56.25\"/>	");
                writer2.WriteLine("	   <Column ss:StyleID=\"s68\" ss:AutoFitWidth=\"0\" ss:Width=\"57.75\"/>	");
                writer2.WriteLine("	   <Column ss:StyleID=\"s68\" ss:AutoFitWidth=\"0\" ss:Width=\"87.75\"/>	");
                writer2.WriteLine("	   <Column ss:StyleID=\"s65\" ss:AutoFitWidth=\"0\" ss:Width=\"60.75\"/>	");
                writer2.WriteLine("	   <Column ss:StyleID=\"s65\" ss:AutoFitWidth=\"0\" ss:Width=\"52.5\"/>	");
                writer2.WriteLine("	   <Column ss:StyleID=\"s127\" ss:AutoFitWidth=\"0\" ss:Width=\"79.5\"/>	");
                writer2.WriteLine("	   <Column ss:StyleID=\"s127\" ss:AutoFitWidth=\"0\" ss:Width=\"74.25\"/>	");
                writer2.WriteLine("	   <Column ss:AutoFitWidth=\"0\" ss:Width=\"45.75\" ss:Span=\"181\"/>	");
                writer2.WriteLine("	   <Row ss:AutoFitHeight=\"0\" ss:Height=\"33\">	");
                writer2.WriteLine("	    <Cell ss:MergeAcross=\"19\" ss:StyleID=\"s80\"><Data ss:Type=\"String\">" + model_yuanqu.YuanQuName + starttime.Year + "年" + starttime.Month + "月" + starttime.Day + "日" + "</Data></Cell>	");
                writer2.WriteLine("	   </Row>	");
                writer2.WriteLine("	   <Row ss:AutoFitHeight=\"0\" ss:Height=\"21.9375\" ss:StyleID=\"s64\">	");
                writer2.WriteLine("	    <Cell ss:MergeDown=\"1\" ss:StyleID=\"s71\"><Data ss:Type=\"String\">船名</Data></Cell>	");
                writer2.WriteLine("	    <Cell ss:MergeDown=\"1\" ss:StyleID=\"s71\"><Data ss:Type=\"String\">提单号</Data></Cell>	");
                writer2.WriteLine("	    <Cell ss:MergeDown=\"1\" ss:StyleID=\"s71\"><Data ss:Type=\"String\">货代</Data></Cell>	");
                writer2.WriteLine("	    <Cell ss:MergeDown=\"1\" ss:StyleID=\"s71\"><Data ss:Type=\"String\">货种</Data></Cell>	");
                writer2.WriteLine("	    <Cell ss:MergeDown=\"1\" ss:StyleID=\"s71\"><Data ss:Type=\"String\">提单支数</Data></Cell>	");
                writer2.WriteLine("	    <Cell ss:MergeDown=\"1\" ss:StyleID=\"s71\"><Data ss:Type=\"String\">提单材积</Data></Cell>	");
                writer2.WriteLine("	    <Cell ss:MergeDown=\"1\" ss:StyleID=\"s71\"><Data ss:Type=\"String\">检测支数</Data></Cell>	");
                writer2.WriteLine("	    <Cell ss:MergeDown=\"1\" ss:StyleID=\"s71\"><Data ss:Type=\"String\">检测材积</Data></Cell>	");
                writer2.WriteLine("	    <Cell ss:MergeAcross=\"2\" ss:StyleID=\"s130\"><Data ss:Type=\"String\">进库</Data></Cell>	");
                writer2.WriteLine("	    <Cell ss:MergeAcross=\"2\" ss:StyleID=\"s131\"><Data ss:Type=\"String\">出库</Data></Cell>	");
                writer2.WriteLine("	    <Cell ss:MergeAcross=\"1\" ss:StyleID=\"s132\"><Data ss:Type=\"String\">总库存</Data></Cell>	");
                writer2.WriteLine("	    <Cell ss:StyleID=\"s71\"><Data ss:Type=\"String\">月进库</Data></Cell>	");
                writer2.WriteLine("	    <Cell ss:StyleID=\"s71\"><Data ss:Type=\"String\">月出库</Data></Cell>	");
                writer2.WriteLine("	    <Cell ss:StyleID=\"s124\"><Data ss:Type=\"String\">累计进库</Data></Cell>	");
                writer2.WriteLine("	    <Cell ss:StyleID=\"s124\"><Data ss:Type=\"String\">累计出库</Data></Cell>	");
                writer2.WriteLine("	   </Row>	");
                writer2.WriteLine("	   <Row>	");
                writer2.WriteLine("	    <Cell ss:Index=\"9\"><Data ss:Type=\"String\">车</Data></Cell>	");
                writer2.WriteLine("	    <Cell><Data ss:Type=\"String\">支数/件</Data></Cell>	");
                writer2.WriteLine("	    <Cell><Data ss:Type=\"String\">数量</Data></Cell>	");
                writer2.WriteLine("	    <Cell><Data ss:Type=\"String\">车</Data></Cell>	");
                writer2.WriteLine("	    <Cell><Data ss:Type=\"String\">支数/件</Data></Cell>	");
                writer2.WriteLine("	    <Cell><Data ss:Type=\"String\">数量</Data></Cell>	");
                writer2.WriteLine("	    <Cell><Data ss:Type=\"String\">支数/件</Data></Cell>	");
                writer2.WriteLine("	    <Cell><Data ss:Type=\"String\">数量</Data></Cell>	");
                writer2.WriteLine("	    <Cell ss:StyleID=\"s80\"><Data ss:Type=\"String\">数量</Data></Cell>	");
                writer2.WriteLine("	    <Cell ss:StyleID=\"s80\"><Data ss:Type=\"String\">数量</Data></Cell>	");
                writer2.WriteLine("	    <Cell ss:StyleID=\"s121\"><Data ss:Type=\"String\">数量</Data></Cell>	");
                writer2.WriteLine("	    <Cell ss:StyleID=\"s121\"><Data ss:Type=\"String\">数量</Data></Cell>	");
                writer2.WriteLine("	   </Row>	");

                List<C_GOODS> list_Goods = db.C_GOODS.OrderBy(n => n.ID).ToList();
                foreach (var items_Goods in list_Goods)
                {
                    writer2.WriteLine("	   <Row>	");
                    writer2.WriteLine("	    <Cell ss:StyleID=\"s118\"><Data ss:Type=\"String\">" + items_Goods.GoodsName + "</Data></Cell>	");
                    writer2.WriteLine("	    <Cell ss:Index=\"9\" ss:StyleID=\"s66\"/>	");
                    writer2.WriteLine("	    <Cell ss:StyleID=\"s66\"/>	");
                    writer2.WriteLine("	    <Cell ss:StyleID=\"s66\"/>	");
                    writer2.WriteLine("	    <Cell ss:Index=\"19\" ss:StyleID=\"s122\"/>	");
                    writer2.WriteLine("	    <Cell ss:StyleID=\"s122\"/>	");
                    writer2.WriteLine("	   </Row>	");
                    List<C_TB_HC_GOODSBILL> list_GOODSBILL = db.C_TB_HC_GOODSBILL.Where(n => n.C_GOODS == items_Goods.GoodsName&&n.YuanQuID==loginModel.YuanquID).OrderBy(n => n.ID).ToList();
                    foreach (var items_GoodBill in list_GOODSBILL)
                    {
                        decimal? InCar = 0;
                        decimal? InN = 0;
                        decimal? InW = 0;
                        decimal? OutCar = 0;
                        decimal? OutN = 0;
                        decimal? OutW = 0;
                        decimal? zkuj = 0;
                        decimal? zkus = 0;
                        decimal? yuein = 0;
                        decimal? yueout = 0;
                        decimal? allin = 0;
                        decimal? allout = 0;
                        List<C_TB_HC_CONSIGN> list_CONSIGN = db.C_TB_HC_CONSIGN.Where(n => n.GOODSBILL_ID == items_GoodBill.ID).OrderBy(n => n.ID).ToList();

                        writer2.WriteLine("	   <Row>	");
                        writer2.WriteLine("	    <Cell><Data ss:Type=\"String\">" + items_GoodBill.ShipName + "</Data></Cell>	");
                        writer2.WriteLine("	    <Cell><Data ss:Type=\"String\">" + items_GoodBill.BLNO + "</Data></Cell>	");
                        writer2.WriteLine("	    <Cell ss:Index=\"3\"><Data ss:Type=\"String\">" + items_GoodBill.C_GOODSAGENT_NAME + "</Data></Cell>	");
                        writer2.WriteLine("	    <Cell><Data ss:Type=\"String\">" + items_GoodBill.C_GOODS + "</Data></Cell>	");
                        writer2.WriteLine("	    <Cell><Data ss:Type=\"Number\">" + items_GoodBill.PLANAMOUNT + "</Data></Cell>	");
                        writer2.WriteLine("	    <Cell><Data ss:Type=\"Number\">" + items_GoodBill.PLANWEIGHT + "</Data></Cell>	");
                        writer2.WriteLine("	    <Cell><Data ss:Type=\"Number\">" + items_GoodBill.jcjs + "</Data></Cell>	");
                        writer2.WriteLine("	    <Cell><Data ss:Type=\"Number\">" + items_GoodBill.jccj + "</Data></Cell>	");

                        foreach (var item_Consign in list_CONSIGN)
                        {



                            int days = DateTime.DaysInMonth(Convert.ToInt32(starttime.Year), starttime.Month);
                            DateTime yuestart = Convert.ToDateTime(starttime.Year + "-" + starttime.Month + "-" + "01" + " 00:00:00");//得到月的第一天
                            List<C_TB_HS_TALLYBILL> list_TALLYBILL = db.C_TB_HS_TALLYBILL.Where(n => n.CONSIGN_ID == item_Consign.ID).OrderBy(n => n.ID).ToList();
                            List<C_TB_HS_TALLYBILL> list_TALLYBILL_1 = db.C_TB_HS_TALLYBILL.Where(n => n.CONSIGN_ID == item_Consign.ID && n.SIGNDATE >= starttime && n.SIGNDATE <= endtime).OrderBy(n => n.ID).ToList();

                            zkuj += list_TALLYBILL.Where(n => n.SIGNDATE <= endtime && (n.CODE_OPSTYPE == "进库")).Sum(n => n.AMOUNT);
                            zkuj -= list_TALLYBILL.Where(n => n.SIGNDATE <= endtime && (n.CODE_OPSTYPE == "出库")).Sum(n => n.AMOUNT);
                            zkus += list_TALLYBILL.Where(n => n.SIGNDATE <= endtime && (n.CODE_OPSTYPE == "进库")).Sum(n => n.WEIGHT);
                            zkus -= list_TALLYBILL.Where(n => n.SIGNDATE <= endtime && (n.CODE_OPSTYPE == "出库")).Sum(n => n.WEIGHT);
                            yuein += list_TALLYBILL.Where(n => n.SIGNDATE != null && (n.CODE_OPSTYPE == "进库" && n.SIGNDATE >= yuestart && n.SIGNDATE <= endtime)).Sum(n => n.WEIGHT);
                            yueout += list_TALLYBILL.Where(n => n.SIGNDATE != null && (n.CODE_OPSTYPE == "出库" && n.SIGNDATE >= yuestart && n.SIGNDATE <= endtime)).Sum(n => n.WEIGHT);
                            allin += list_TALLYBILL.Where(n => n.SIGNDATE <= endtime && (n.CODE_OPSTYPE == "进库")).Sum(n => n.WEIGHT);
                            allout += list_TALLYBILL.Where(n => n.SIGNDATE <= endtime && (n.CODE_OPSTYPE == "出库")).Sum(n => n.WEIGHT);

                            foreach (var item_TallBill in list_TALLYBILL_1)
                            {
                                if (item_TallBill.CODE_OPSTYPE == "进库")
                                {

                                    InN += item_TallBill.AMOUNT;
                                    InW += item_TallBill.WEIGHT;
                                }
                                if (item_TallBill.CODE_OPSTYPE == "出库")
                                {
                                    OutN += item_TallBill.AMOUNT;
                                    OutW += item_TallBill.WEIGHT;
                                }
                                if (item_TallBill.CODE_OPSTYPE == "进库")
                                {
                                    InCar += item_TallBill.TRAINNUM;
                                }
                                if (item_TallBill.CODE_OPSTYPE == "出库")
                                {
                                    OutCar += item_TallBill.TRAINNUM;
                                }
                            }

                        }
                        writer2.WriteLine("	    <Cell><Data ss:Type=\"Number\">" + InCar + "</Data></Cell>	");
                        writer2.WriteLine("	    <Cell><Data ss:Type=\"Number\">" + InN + "</Data></Cell>	");
                        writer2.WriteLine("	    <Cell><Data ss:Type=\"Number\">" + InW + "</Data></Cell>	");
                        writer2.WriteLine("	    <Cell><Data ss:Type=\"Number\">" + OutCar + "</Data></Cell>	");
                        writer2.WriteLine("	    <Cell><Data ss:Type=\"Number\">" + OutN + "</Data></Cell>	");
                        writer2.WriteLine("	    <Cell><Data ss:Type=\"Number\">" + OutW + "</Data></Cell>	");
                        writer2.WriteLine("	    <Cell ss:Index=\"15\"><Data ss:Type=\"Number\">" + zkuj + "</Data></Cell>	");
                        writer2.WriteLine("	    <Cell><Data ss:Type=\"Number\">" + zkus + "</Data></Cell>	");
                        writer2.WriteLine("	    <Cell><Data ss:Type=\"Number\">" + yuein + "</Data></Cell>	");
                        writer2.WriteLine("	    <Cell><Data ss:Type=\"Number\">" + yueout + "</Data></Cell>	");
                        writer2.WriteLine("	    <Cell ss:StyleID=\"s122\"><Data ss:Type=\"Number\">" + allin + "</Data></Cell>	");
                        writer2.WriteLine("	    <Cell ss:StyleID=\"s122\"><Data ss:Type=\"Number\">" + allout + "</Data></Cell>	");
                        writer2.WriteLine("	   </Row>	");

                    }




                }


                writer2.WriteLine("	  </Table>	");
                writer2.WriteLine("	  <WorksheetOptions xmlns=\"urn:schemas-microsoft-com:office:excel\">	");
                writer2.WriteLine("	   <Print>	");
                writer2.WriteLine("	    <ValidPrinterInfo/>	");
                writer2.WriteLine("	    <PaperSizeIndex>9</PaperSizeIndex>	");
                writer2.WriteLine("	    <VerticalResolution>0</VerticalResolution>	");
                writer2.WriteLine("	   </Print>	");
                writer2.WriteLine("	   <PageBreakZoom>60</PageBreakZoom>	");
                writer2.WriteLine("	   <Selected/>	");
                writer2.WriteLine("	   <FreezePanes/>	");
                writer2.WriteLine("	   <FrozenNoSplit/>	");
                writer2.WriteLine("	   <SplitHorizontal>3</SplitHorizontal>	");
                writer2.WriteLine("	   <TopRowBottomPane>3</TopRowBottomPane>	");
                writer2.WriteLine("	   <SplitVertical>8</SplitVertical>	");
                writer2.WriteLine("	   <LeftColumnRightPane>8</LeftColumnRightPane>	");
                writer2.WriteLine("	   <ActivePane>0</ActivePane>	");
                writer2.WriteLine("	   <Panes>	");
                writer2.WriteLine("	    <Pane>	");
                writer2.WriteLine("	     <Number>3</Number>	");
                writer2.WriteLine("	    </Pane>	");
                writer2.WriteLine("	    <Pane>	");
                writer2.WriteLine("	     <Number>1</Number>	");
                writer2.WriteLine("	     <ActiveCol>0</ActiveCol>	");
                writer2.WriteLine("	    </Pane>	");
                writer2.WriteLine("	    <Pane>	");
                writer2.WriteLine("	     <Number>2</Number>	");
                writer2.WriteLine("	     <ActiveRow>0</ActiveRow>	");
                writer2.WriteLine("	    </Pane>	");
                writer2.WriteLine("	    <Pane>	");
                writer2.WriteLine("	     <Number>0</Number>	");
                writer2.WriteLine("	     <ActiveRow>10</ActiveRow>	");
                writer2.WriteLine("	     <ActiveCol>9</ActiveCol>	");
                writer2.WriteLine("	    </Pane>	");
                writer2.WriteLine("	   </Panes>	");
                writer2.WriteLine("	   <ProtectObjects>False</ProtectObjects>	");
                writer2.WriteLine("	   <ProtectScenarios>False</ProtectScenarios>	");
                writer2.WriteLine("	  </WorksheetOptions>	");
                writer2.WriteLine("	 </Worksheet>	");

            }




            writer2.WriteLine("	</Workbook>	");

            writer2 = null;
            writer1.Close();

            // MyComm.RegisterScript(UpdatePanel2, this, "window.open('../QueryOut/新路代日报表(" + sUserName + ").xls','','')");
            string filePath = string.Format("/QueryOut//" + "新路代日报表" + ".xls", DateTime.Now, "新路代日报表");
            return Json(new AjaxResult { state = ResultType.success.ToString(), message = filePath }.ToJson());
        }


        #endregion


        #region 客户分析
        /// <summary>
        /// 客户分析
        /// </summary>
        /// <returns></returns>
        public ActionResult Kehufenxi()
        {
            return View();
        }
        [HttpGet]
        public JsonResult GetEchatData(DateTime? starTime,DateTime? endTime)
        {
            try
            {
                //1从票货表中获取所有的货代信息（客户），2从委托表中获取所理货单数据3从理货单中计算数据
                OperatorModel loginModel = OperatorProvider.Provider.GetCurrent();
                var yuanquid = loginModel.YuanquID ?? throw new ArgumentNullException("loginModel.YuanquID");
                var wherelambda = ExtLinq.True<C_TB_HC_GOODSBILL>();
                wherelambda = wherelambda.And(t => t.YuanQuID == yuanquid);
                if (starTime != null)
                {
                    wherelambda = wherelambda.And(t => t.CreatTime >= starTime);
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
                return Json(new { data = listentry }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Log4NetHelper log = new Log4NetHelper();
                log.Error(e.Message, e);
                throw;
            }


        }

        public ActionResult Kehufenxi2()
        {
            return View();
        }
        [HttpGet]
        public JsonResult GetKehufenxi2(DateTime? starTime, DateTime? endTime)
        {
            try
            {
                //1从票货表中获取所有的货代信息（客户），2从委托表中获取所理货单数据3从理货单中计算数据
                OperatorModel loginModel = OperatorProvider.Provider.GetCurrent();
                var yuanquid = loginModel.YuanquID ?? throw new ArgumentNullException("loginModel.YuanquID");
                var wherelambda = ExtLinq.True<C_TB_HC_GOODSBILL>();
                wherelambda = wherelambda.And(t => t.YuanQuID == yuanquid);
                if (starTime != null)
                {
                    wherelambda = wherelambda.And(t => t.CreatTime >= starTime);
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
                return Json(new { data = listentry }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Log4NetHelper log = new Log4NetHelper();
                log.Error(e.Message, e);
                throw;
            }


        }
        public ActionResult Kehufenxi3()
        {
            return View();
        }
        [HttpGet]
        public JsonResult GetKehufenxi3(DateTime? starTime, DateTime? endTime)
        {
            try
            {
                //1从票货表中获取所有的货代信息（客户），2从委托表中获取所理货单数据3从理货单中计算数据
                OperatorModel loginModel = OperatorProvider.Provider.GetCurrent();
                var yuanquid = loginModel.YuanquID ?? throw new ArgumentNullException("loginModel.YuanquID");
                var wherelambda = ExtLinq.True<C_TB_HC_GOODSBILL>();
                wherelambda = wherelambda.And(t => t.YuanQuID == yuanquid);
                if (starTime != null)
                {
                    wherelambda = wherelambda.And(t => t.CreatTime >= starTime);
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
                return Json(new { data = listentry }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Log4NetHelper log = new Log4NetHelper();
                log.Error(e.Message, e);
                throw;
            }


        }
        #endregion

        #region 货种分析

        /// <summary>
        /// 货种分析
        /// </summary>
        /// <returns></returns>
        public ActionResult HuoZhongFenXi()
        {
            return View();
        }
        [HttpGet]
        public JsonResult GetHuoZhongFenXi(DateTime? starTime, DateTime? endTime)
        {
            try
            {
                OperatorModel loginModel = OperatorProvider.Provider.GetCurrent();
                var yuanquid = loginModel.YuanquID ?? throw new ArgumentNullException("loginModel.YuanquID");
                var wherelambda = ExtLinq.True<C_TB_HS_TALLYBILL>();
                wherelambda = wherelambda.And(t => t.YuanQuID == yuanquid);
                if (starTime != null)
                {
                    wherelambda = wherelambda.And(t => t.SIGNDATE >= starTime);
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


                return Json(new { data = listentry }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Log4NetHelper log = new Log4NetHelper();
                log.Error(e.Message, e);
                throw;
            }
        }

        public ActionResult HuoZhongFenXi2()
        {
            return View();
        }
        #endregion
        #region 开票同比

        /// <summary>
        /// 开票同比
        /// </summary>
        /// <returns></returns>
        public ActionResult KaiPiaoTongBi()
        {
            return View();
        }
        [HttpGet]
        public JsonResult GetKaiPiaoTongBi()
        {
            DateTime starTime = DateTime.Now.AddMonths(1 - DateTime.Now.Month).AddDays(1 - DateTime.Now.Day).Date;//今年的第一月的第一天
            DateTime endTime = DateTime.Now.AddMonths(1 - DateTime.Now.Month).AddDays(1 - DateTime.Now.Day).Date.AddMonths(1).AddSeconds(-1);//今年的第一月的最后一天
            OperatorModel loginModel = OperatorProvider.Provider.GetCurrent();
            List<HuanBiEntry> listentry = new List<HuanBiEntry>();
            Array GoodsBillIDArray = db.C_TB_HC_GOODSBILL.Where(n=>n.YuanQuID== loginModel.YuanquID).Select(n => n.ID).ToArray();
            List<decimal?> goodsagentid = new List<decimal?>();
            foreach (var items in GoodsBillIDArray)
            {
                goodsagentid.Add(items.ToDecimal());
            }//获取该园区所有的GoodsBillId

            List<C_TB_JIESUAN> JIESUAN_List = db.C_TB_JIESUAN.Where(n=> goodsagentid.Contains(n.GoodsBill_id)).ToList();//获取该园区所有的结算信息
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
                return Json(new { data = listentry }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Log4NetHelper log = new Log4NetHelper();
                log.Error(e.Message, e);
                throw;
            }
        }


        #endregion
        #region
        /// <summary>
        /// 作业量同比
        /// </summary>
        /// <returns></returns>
        public ActionResult ZuoYeLiangTongBi()
        {
            return View();
        }
        [HttpGet]
        public JsonResult GetZuoYeLiangTongBi()
        {
            DateTime starTime = DateTime.Now.AddMonths(1 - DateTime.Now.Month).AddDays(1 - DateTime.Now.Day).Date;//今年的第一月的第一天
            DateTime endTime = DateTime.Now.AddMonths(1 - DateTime.Now.Month).AddDays(1 - DateTime.Now.Day).Date.AddMonths(1).AddSeconds(-1);//今年的第一月的最后一天
            OperatorModel loginModel = OperatorProvider.Provider.GetCurrent();
            List<HuanBiEntry> listentry = new List<HuanBiEntry>();
            Array GoodsBillIDArray = db.C_TB_HC_GOODSBILL.Where(n => n.YuanQuID == loginModel.YuanquID).Select(n => n.ID).ToArray();
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
                    entry.TongBi = ((List_JiSuan.Where(n => n.SIGNDATE >= starTime.AddYears(-1).AddMonths(i) && n.SIGNDATE <= endTime.AddYears(-1).AddMonths(i)).Sum(n => n.WEIGHT)- List_JiSuan.Where(n => n.SIGNDATE >= starTime.AddMonths(i) && n.SIGNDATE <= endTime.AddMonths(i)).Sum(n => n.WEIGHT))/100).ToDecimal(2);
                    listentry.Add(entry);
                }
                return Json(new { data = listentry }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Log4NetHelper log = new Log4NetHelper();
                log.Error(e.Message, e);
                throw;
            }
        }


#endregion
        #region 开票详细
        public ActionResult KaiPiaoMingXi()
        {
            return View();
        }

        [System.Web.Http.HttpGet]
        public object GetKaiPiaoMingXi(int limit, int offset, DateTime? CreatTime, DateTime? EndTime)
        {

            List<KaiPiaoMingXientry> KaiPiaoList = new List<KaiPiaoMingXientry>();

            OperatorModel loginModel = OperatorProvider.Provider.GetCurrent();
            var yuanquid = loginModel.YuanquID ?? throw new ArgumentNullException("loginModel.YuanquID");
            var wherelambda = ExtLinq.True<C_TB_JIESUAN>();
            if (CreatTime!=null)
            {
                wherelambda = wherelambda.And(t => t.KaiPiaoRiQi >= CreatTime);
            }
            if (EndTime != null)
            {
                wherelambda = wherelambda.And(t => t.KaiPiaoRiQi <= EndTime);
            }
           
            List<C_TB_JIESUAN> jiesuanList = db.C_TB_JIESUAN.Where(wherelambda).ToList();
            DataTableController dc = new DataTableController();
            foreach (var j in jiesuanList)
            {
                decimal? ChengBenFeiLv = 0;
                int GoodsBill_id = j.GoodsBill_id.ToInt();
                C_TB_HC_GOODSBILL piaoHuoEntry = db.C_TB_HC_GOODSBILL.Find(GoodsBill_id);
                var piaohuoYuanquId = piaoHuoEntry.YuanQuID;
                if (yuanquid != piaohuoYuanquId)
                {
                    continue;
                }
                List<Stock_Money> listStock_Money = new List<Stock_Money>();
                listStock_Money = dc.Get_FeiMuXX(GoodsBill_id, piaoHuoEntry.CONTRACT_Guid);
                Stock_Money smEntry =
              listStock_Money.FirstOrDefault(n => n.FMZhongLei == j.FeiMuZhongLei && n.Type == j.Type);
               C_TB_CHENGBEN ChengBen = db.C_TB_CHENGBEN.FirstOrDefault(n => n.FeiMuZhongLei == j.FeiMuZhongLei && n.Type == j.Type && n.GoodsBill_id == piaoHuoEntry.ID);
                if (ChengBen!=null)
                {
                    ChengBenFeiLv = ChengBen.ChengBenFeiLv;
                }
                KaiPiaoMingXientry entry = new KaiPiaoMingXientry()
                {
                    KaiPiaoDanWei = j.KaiPianDanWei,
                    VGNO = piaoHuoEntry.VGNO,
                    BLNO = piaoHuoEntry.BLNO,
                    KaiPiaoJinE = smEntry.KaiPiaoShuiHou,
                    ChengBenJinE = ((smEntry.ShuLiang * ChengBenFeiLv) / Convert.ToDecimal((1 + (Convert.ToDecimal(smEntry.ShuiLv) * Convert.ToDecimal(0.01))))).ToDecimal(2),
                    LiRun= smEntry.ShuiHouJinE- ((smEntry.ShuLiang * ChengBenFeiLv) / Convert.ToDecimal((1 + (Convert.ToDecimal(smEntry.ShuiLv) * Convert.ToDecimal(0.01))))).ToDecimal(2),
                    FeiYongLeiXing = j.FeiMuZhongLei,
                    FuKuanDanWei = j.LaiKuanDanWei,
                    ChuanMing = piaoHuoEntry.ShipName,
                    FeiLv = smEntry.ShiJiShuiLv,
                    ShuLiang = smEntry.ShuLiang,
                    FeiYong1 = smEntry.FeiYong,
                    FeiYong2 = smEntry.ShuiHouJinE,
                    LaiKuanJinE = smEntry.LaiKuanJinE,
                    HuoZhong = smEntry.HuoMing,
                    KaiPiaoYinSHouFuKuanYuE= smEntry.KaiPiaoJinE- smEntry.LaiKuanJinE,

                };
                KaiPiaoList.Add(entry);
            }
            int total = KaiPiaoList.Count;
            object rows = KaiPiaoList.OrderBy(n=>n.RiQi).Skip(offset).Take(limit).AsQueryable();
            return Json(new { total = total, rows = rows }, JsonRequestBehavior.AllowGet);

        }
        #endregion

        #region 电子口岸

        private string codeuser = "FICCGS";
        private string password = "987654";
        //private string GsCode = "6377";
        /// <summary>
        /// 获取BR客户标准元数据
        /// </summary>
        public ActionResult QueryClient()
        {
 
         return View();

        }
        /// <summary>
        /// 生成xml数据的方法
        /// </summary>
        /// <param name="IVNO">发票编码</param>
        /// <param name="IVDISPLAY">发票描述</param>
        /// <param name="CODECOMPANY">公司编码 需要对照BR元数据</param>
        /// <param name="CODECLIENT">委托人编码 需要对照BR元数据</param>
        /// <param name="TOTAL">合计金额</param>
        /// <param name="REMARK">备注</param>
        /// <param name="CODEINOUT">进出口编码  需要对照BR元数据</param>
        /// <param name="CODETRADE">内外贸编码  需要对照BR元数据</param>
        /// <param name="VGNO">船舶编码  需要将船舶数据插入到EAS表中</param>
        /// <param name="VESSEL">船名</param>
        /// <param name="VOYAGE">航次</param>
        /// <param name="CREATETIME">创建时间--!>20100928</param>
        /// <param name="CREATOR">创建人 与EPORT平台用户进行对照</param>
        /// <param name="CREATORNAME">创建人名称</param>

        /// <param name="PAYER">付款人</param>
        /// <param name="PAPERNO">机打发票号码（国税票可不填、其他必填）</param>

        /// <param name="CODEBILLTYPE">数据来源类型</param>
        /// <param name="TYR">托运人</param>
        /// <param name="SHR">收货人</param>
        /// <param name="FZ">发站</param>
        /// <param name="DZ">到站</param>
        /// <param name="YFZ">原发站</param>
        /// <param name="ZDZ">终到站</param>
        /// <param name="CZ">车种</param>
        /// <param name="FJ">附记</param>
        /// <param name="VEHICLES">车数</param>
        /// <param name="DCLC">调车里程</param>
        /// <param name="TOTALAMOUNT">件数</param>
        /// <param name="PLPB">铁路篷布</param>
        /// <param name="ZBPB">自备篷布</param>
        /// <param name="TOTALWEIGHT">计费重量</param>
        /// <param name="QRZL">托运人确认重量-</param>
        /// <param name="UPPERTOTAL">金额大写</param>
        /// <param name="DRAWER">开票人</param>
        /// <param name="CHECKER">复核人</param>
        /// <param name="CODE_IVTEMPLATE">发票模板种类</param>
        /// <param name="CODE_BLINE">FI行业编码[固定值 7]</param>
        /// <param name="CODE_DEPARTMENT">FI部门编码[取iport系统部门代码需要手动对应] </param>
        /// <param name="PERIOD">发票期间，取年，YYYY</param>
        /// <param name="DRAWTIME">开票日期 yyyy-mm-dd</param>
        /// <param name="QYD">起运地</param>
        /// <param name="MDD">目的地</param>
        /// <param name="FUZHU">附注</param>
        /// <param name="DLGRQ">到离港日期</param>
        /// <param name="ZHG">装货港</param>
        /// <param name="XHG">卸货港</param>
        /// <param name="YZM">（国税票可不填、其他必填）地税发票验证码，4位随机数</param>
        /// <param name="INVOICETYPE">发票状态， 1 正常票 2红冲票（负数发票） 3作废票（正常票被作废）</param>
        /// <param name="DEPOSER">发票作废人</param>
        /// <param name="DEPOSETIME">发票作废时间，yyyy-mm-dd</param>
        /// <param name="IVNO_HC">红冲原票IVNO编码，当为红冲票时，必填</param>
        /// <param name="BZ">票面显示备注</param>
        /// <param name="FPDM">机打发票代码（国税票可不填、其他必填）</param>
        /// <param name="YFPDM">红冲原票代码</param>
        /// <param name="YFPHM">红冲原票号码</param>
        /// <param name="FPDM_ZZ">纸质发票代码（国税票可不填、其他必填）</param>
        /// <param name="PAPERNO_ZZ">纸质发票票号（国税票可不填、其他必填）</param>
        /// <param name="MARK_DS">地税新票标志,0非地税新票,1地税新票</param>
        /// <param name="CODE_PAYER">纸面付款人编码，一般与CODE_CLIENT值一致</param>
        /// <param name="CODE_NOTETYPE">单据类型 1国税 2地税</param>
        /// <param name="CODE_NOTETEMPLATE">票据模板 3国税普票 4国税专票 13地税票</param>
        /// <param name="list"></param>
        /// <returns></returns>
        public string CreateXML(string IVNO,string IVDISPLAY,string CODECOMPANY,string CODECLIENT,
            double TOTAL,string REMARK,string CODEINOUT,string CODETRADE,string VGNO,string VESSEL,string VOYAGE,
            string CREATETIME,string CREATOR, string CREATORNAME, string PAYER,string PAPERNO, string CODEBILLTYPE, string TYR,string SHR,string FZ,string DZ,string YFZ,
            string ZDZ,string CZ,string FJ,string VEHICLES,string DCLC, string TOTALAMOUNT, string PLPB, string ZBPB, string TOTALWEIGHT, string QRZL,
            string UPPERTOTAL, string DRAWER, string CHECKER, string CODE_IVTEMPLATE, string CODE_BLINE, string CODE_DEPARTMENT, string PERIOD, string DRAWTIME, string QYD, string MDD
            , string FUZHU, string DLGRQ, string ZHG, string XHG, string YZM, string INVOICETYPE, string DEPOSER, string DEPOSETIME, string IVNO_HC, string BZ
            , string FPDM, string YFPDM, string YFPHM, string FPDM_ZZ, string PAPERNO_ZZ, string MARK_DS, string CODE_PAYER, string CODE_NOTETYPE, string CODE_NOTETEMPLATE,List<FiITEM> list)
        {
            StringBuilder x = new StringBuilder("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            x.Append("<RBILL>");
            x.Append("<BILLHEAD>");
            x.Append("<IVNO>"+IVNO+"</IVNO>");
            x.Append("<IVDISPLAY>"+IVDISPLAY+"</IVDISPLAY>");
            x.Append("<CODECOMPANY>"+CODECOMPANY+"</CODECOMPANY>");
            x.Append("<CODECLIENT>"+CODECLIENT+"</CODECLIENT>");
            x.Append("<TOTAL>"+TOTAL+"</TOTAL>");
            x.Append("<REMARK>"+REMARK+"</REMARK>");
            x.Append("<CODEINOUT>"+CODEINOUT+"</CODEINOUT>");
            x.Append("<CODETRADE>"+CODETRADE+"</CODETRADE>");
            //x.Append("<VGNO> "+ VGNO + "</VGNO>");
            //x.Append("<VESSEL>"+ VESSEL + " </VESSEL>");
            //x.Append("<VOYAGE>"+ VOYAGE + " </VOYAGE>");
            x.Append("<CREATETIME>"+CREATETIME+"</CREATETIME>");
            x.Append("<CREATOR>"+CREATOR+"</CREATOR>");
            x.Append("<CREATORNAME>"+CREATORNAME+"</CREATORNAME>");
            x.Append("<PAYER>"+PAYER+"</PAYER>");

            //x.Append("<PAPERNO> "+ PAPERNO + "</PAPERNO>");
            x.Append("<CODEBILLTYPE>"+CODEBILLTYPE+"</CODEBILLTYPE>");
            //x.Append("<TYR>"+ TYR + "</TYR>");
            //x.Append("<SHR>"+ SHR + "</SHR>");
            //x.Append("<FZ>"+ FZ + "</FZ>");
            //x.Append("<DZ>"+ DZ + "</DZ>");
            //x.Append("<YFZ>"+ YFZ + "</YFZ>");
            //x.Append("<ZDZ>"+ ZDZ + "</ZDZ>");
            //x.Append("<CZ>"+ CZ + "</CZ>");
            //x.Append("<FJ>"+ FJ + "</FJ>");
            //x.Append("<VEHICLES>"+ VEHICLES + "</VEHICLES>");
            //x.Append("<DCLC>" + DCLC + "</DCLC>");
            //x.Append("<TOTALAMOUNT>" + TOTALAMOUNT + "</TOTALAMOUNT>");
            //x.Append("<PLPB>" + PLPB + "</PLPB>");
            //x.Append("<ZBPB>" + ZBPB + "</ZBPB>");
            //x.Append("<TOTALWEIGHT>" + TOTALWEIGHT + "</TOTALWEIGHT>");
            //x.Append("<QRZL>" + QRZL + "</QRZL>");
            //x.Append("<UPPERTOTAL>" + UPPERTOTAL + "</UPPERTOTAL>");
            //x.Append("<DRAWER>" + "开票人" + "</DRAWER>  ");
            //x.Append("<CHECKER>" + CHECKER + "</CHECKER >  ");
            //x.Append("<CODE_IVTEMPLATE>" + CODE_IVTEMPLATE + "</CODE_IVTEMPLATE>");
            x.Append("<CODE_BLINE>"+CODE_BLINE+"</CODE_BLINE>");
            x.Append("<CODE_DEPARTMENT>"+CODE_DEPARTMENT+"</CODE_DEPARTMENT>");
            x.Append("<PERIOD>"+PERIOD+"</PERIOD>");
            x.Append("<DRAWTIME>"+DRAWTIME+"</DRAWTIME>");
            //x.Append("<QYD>" + QYD + "</QYD>");
            //x.Append("<MDD>" + MDD + "</MDD>");
            //x.Append("<FUZHU>" + FUZHU + "</FUZHU>");
            //x.Append("<DLGRQ>" + DLGRQ + "</DLGRQ>");
            //x.Append("<ZHG>" + ZHG + "</ZHG>");
            //x.Append("<XHG>" + XHG + "</XHG>");
            //x.Append("<YZM>" + YZM + "</YZM>");
            x.Append("<INVOICETYPE>"+INVOICETYPE+"</INVOICETYPE>");
            //x.Append("<DEPOSER>" + DEPOSER + "</DEPOSER>");
            //x.Append("<DEPOSETIME>" + DEPOSETIME + "</DEPOSETIME>");
            //x.Append("<IVNO_HC>" + IVNO_HC + "</IVNO_HC>");
            //x.Append("<BZ>" + BZ + "</BZ>");
            //x.Append("<FPDM>" + FPDM + "</FPDM>");
            //x.Append("<YFPDM>" + YFPDM + "</YFPDM>");
            //x.Append("<YFPHM>" + YFPHM + "</YFPHM>");
            //x.Append("<FPDM_ZZ>	" + FPDM_ZZ + "</FPDM_ZZ>");
            //x.Append("<PAPERNO_ZZ>" + PAPERNO_ZZ + "</PAPERNO_ZZ>");
            //x.Append("<MARK_DS> " + MARK_DS + "</MARK_DS>");
            x.Append("<CODE_PAYER>"+CODE_PAYER+"</CODE_PAYER >");
            x.Append("<CODE_NOTETYPE>"+CODE_NOTETYPE+"</CODE_NOTETYPE >");
            x.Append("<CODE_NOTETEMPLATE>"+CODE_NOTETEMPLATE+"</CODE_NOTETEMPLATE>");
            x.Append("</BILLHEAD>");
            x.Append("<BILLITEMS>");
         
            foreach (var l in list)
            {
                x.Append("<ITEM>");
                x.Append("<SERIAL>"+l.SERIAL+"</SERIAL>");
                x.Append("<CODECHARGETYPE>"+l.CODECHARGETYPE+"</CODECHARGETYPE>");
                x.Append("<CODECARGO>"+l.CODECARGO+"</CODECARGO>");
                x.Append("<CARGO></CARGO>");
                //x.Append("<CODEPACK>" + l.CODEPACK + "</CODEPACK>");
                //x.Append("<SUMMARY>" + l.SUMMARY + "</SUMMARY>");

                x.Append("<QUANTITY>"+l.QUANTITY+"</QUANTITY>");
                x.Append("<RATE>"+l.RATE+"</RATE>");
                x.Append("<CODE_MEASURE>"+l.CODE_MEASURE+"</CODE_MEASURE>");
                x.Append("<PRICE>"+l.PRICE+"</PRICE>");
                x.Append("<ZKE>"+l.ZKE+"</ZKE>");
                x.Append("<SFKC>"+l.SFKC+"</SFKC>");
                //x.Append("<TDH>" + l.TDH + "</TDH>");
                //x.Append("<FZ>" + l.FZ + "</FZ>");
                x.Append("</ITEM>");
     
            }


            x.Append("</BILLITEMS>");
            x.Append("</RBILL>");
           
            return x.ToString();

        }
        /// <summary>
        /// 添加应收单。
        /// </summary>
        /// <param name="codeuser">用户编码。</param>
        /// <param name="password">登陆口令。</param>
        /// <param name="xmlData">应收单XML数据</param>
        /// <returns>是否成功（0/失败,1/成功）</returns>
        public string AppendRBill(string xmlData)
        {
            ServiceRBillSoap s = new ServiceRBillSoapClient();
            return  s.AppendRBill(codeuser, password, xmlData);
        }
        /// <summary>
        /// 删除应收单。
        /// </summary>
        /// <param name="codeuser">用户编码。</param>
        /// <param name="password">登陆口令。</param>
        /// <param name="codecompany">公司编码。</param>
        /// <param name="ivno">发票编码。</param>
        /// <returns>是否成功（0/失败,1/成功）</returns>
        public string DeleteRBill(string codecompany, string ivno)
        {
            ServiceRBillSoap s = new ServiceRBillSoapClient();
            return s.DeleteRBill(codeuser, password, codecompany, ivno);
        }

        /// <summary>
        /// 查询票据状态。
        /// </summary>
        /// <param name="codeuser">用户编码。</param>
        /// <param name="password">登陆口令。</param>
        /// <param name="codecompany">当前发票对应FI组织机构编码。</param>
        /// <param name="ivno">当前发票编码。</param>
        /// <returns>发票状态（是否存在，打印提交，打印，作废，红冲，一体化，财务接受，发票代码，发票号码）</returns>
        public string IVIsStatus(string codecompany, string ivno)
        {
            ServiceRBillSoap s = new ServiceRBillSoapClient();
            return s.IVIsStatus(codeuser, password, codecompany, ivno);
        }

        #endregion

        public ActionResult KaiPiaoPage()
        {
            return View();
        }
        [System.Web.Http.HttpGet]
        public object GetKaiPiao(int limit, int offset, DateTime? CreatTime, DateTime? EndTime)
        {

       // var KaiPiaoList = new List<B_TB_KAIPIAO>();

            OperatorModel loginModel = OperatorProvider.Provider.GetCurrent();
            var yuanquid = loginModel.YuanquID ?? throw new ArgumentNullException("loginModel.YuanquID");
            var wherelambda = ExtLinq.True<B_TB_KAIPIAO>();
            wherelambda = wherelambda.And(t => t.YuanQuID== yuanquid);
            if (CreatTime != null)
            {
                wherelambda = wherelambda.And(t => t.AddTime >= CreatTime);
            }
            if (EndTime != null)
            {
                wherelambda = wherelambda.And(t => t.AddTime <= EndTime);
            }

           var  KaiPiaoList = db.B_TB_KAIPIAO.Where(wherelambda).AsQueryable();
          
            int total = KaiPiaoList.Count();
            object rows = KaiPiaoList.OrderBy(n => n.AddTime).Skip(offset).Take(limit).AsQueryable();
            return Json(new { total = total, rows = rows }, JsonRequestBehavior.AllowGet);

        }

        [OperationLog(OperationLogAttribute.Operatetype.删除, OperationLogAttribute.ImportantLevel.危险操作, "删除应收发票")]
        [HttpPost]
        public ActionResult DelKaiPiao(string guid)
        {
          B_TB_KAIPIAO entry = db.B_TB_KAIPIAO.Find(guid);
          if (entry != null)
          {
              string r= DeleteRBill(entry.CODECOMPANY,entry.IVNO);
              if (r=="1")
              {
                  using (var scope = new TransactionScope(TransactionScopeOption.Required))
                  {
                      db.B_TB_KAIPIAO.Remove(entry);
                      List<B_TB_KAIPIAO_XIANGXI> xiangxiList =
                          db.B_TB_KAIPIAO_XIANGXI.Where(n => n.GoodsBillId == entry.IVNO).ToList();
                      EFHelpler<B_TB_KAIPIAO_XIANGXI> efHelpler=new EFHelpler<B_TB_KAIPIAO_XIANGXI>();
                      efHelpler.delete(xiangxiList.ToArray());
                        C_TB_HC_GOODSBILL goodEntry = db.C_TB_HC_GOODSBILL.FirstOrDefault(n => n.GBNO == entry.IVNO);
                      if (goodEntry != null) goodEntry.State_FaPiao = null;
                      int c = db.SaveChanges();
                      if (c > 0)
                      {
                          scope.Complete();
                          return Json(new AjaxResult { state = ResultType.success.ToString(), message = "成功！" });
                      }
                      else
                      {
                          return Json(new AjaxResult { state = ResultType.error.ToString(), message = r });
                      }
                     
                  }
          
              }
           
          }
          return Json(new AjaxResult { state = ResultType.error.ToString(), message ="错误" });
        }

        [HttpPost]
        public ActionResult ChaXunKaiPiao(string guid)
        {
            B_TB_KAIPIAO entry = db.B_TB_KAIPIAO.Find(guid);
            if (entry != null)
            {
                string r = IVIsStatus(entry.CODECOMPANY, entry.IVNO);
                string[] rs = r.Split(',');
                string state=null;
                switch (rs[0])
                {
                    case "1":
                        state += "[是否存在/是],";
                        break;
                    case "0":
                        state += "[是否存在/否],";
                        break;
                }
                switch (rs[1])
                {
                    case "1":
                        state += "[打印提交/是],";
                        break;
                    case "0":
                        state += "[打印提交/否],";
                        break;
                }
                switch (rs[2])
                {
                    case "1":
                        state += "[打印/是],";
                        break;
                    case "0":
                        state += "[打印/否],";
                        break;
                }
                switch (rs[3])
                {
                    case "1":
                        state += "[作废/是],";
                        break;
                    case "0":
                        state += "[作废/否],";
                        break;
                }
                switch (rs[4])
                {
                    case "1":
                        state += "[红冲/是],";
                        break;
                    case "0":
                        state += "[红冲/否],";
                        break;
                }
                switch (rs[5])
                {
                    case "1":
                        state += "[一体化/是],";
                        break;
                    case "0":
                        state += "[一体化/否],";
                        break;
                }
                switch (rs[6])
                {
                    case "1":
                        state += "[财务接受/是],";
                        break;
                    case "0":
                        state += "[财务接受/否],";
                        break;
                }
                switch (rs[7])
                {
                    case "1":
                        state += "[发票代码/是],";
                        break;
                    case "0":
                        state += "[发票代码/否],";
                        break;
                }
                switch (rs[8])
                {
                    case "1":
                        state += "[发票号码/是],";
                        break;
                    case "0":
                        state += "[发票号码/否],";
                        break;
                }
              

                entry.State =state;
                int c = db.SaveChanges();
                if (c > 0)
                {
                   
                    return Json(new AjaxResult { state = ResultType.success.ToString(), message = "成功！" });
                }
                else
                {
                    return Json(new AjaxResult { state = ResultType.error.ToString(), message = r });
                }

            }
            return Json(new AjaxResult { state = ResultType.error.ToString(), message = "错误" });
        }
    }

}