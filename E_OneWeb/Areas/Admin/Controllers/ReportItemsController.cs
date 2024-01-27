using E_OneWeb.DataAccess.Repository.IRepository;
using E_OneWeb.Models;
using E_OneWeb.Models.ViewModels;
using E_OneWeb.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.SqlServer.Server;
using NPOI.HSSF.Record;
using NPOI.SS.Formula.Functions;
using NPOI.XSSF.UserModel;
using NPOI.XWPF.UserModel;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using SixLabors.ImageSharp;
using sun.misc;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Data.Common;
using com.sun.org.apache.xerces.@internal.impl.dv.xs;
//using Document = Aspose.Pdf.Document;
//using Color = Aspose.Pdf.Color;
using iTextSharp.text.pdf;
using iTextSharp.text;
using Document = iTextSharp.text.Document;
using static NPOI.HSSF.Util.HSSFColor;
using Color = SixLabors.ImageSharp.Color;
using iText.Layout.Properties;
using SixLabors.ImageSharp.ColorSpaces;
using NPOI.SS.UserModel;

namespace E_OneWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
    public class ReportItemsController : Controller
    {       
        private readonly IUnitOfWork _unitOfWork;
        private static ReportItemVM staticvm = new ReportItemVM();
        public ReportItemsController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<IActionResult> Index(ReportItemVM vm, string code, string ownership, int? category, int? status, string startdate, string enddate)
        {
            staticvm.SearchCode = code;
            staticvm.SearchOwnership = ownership;
            staticvm.SearchCategory = category;
            staticvm.SearchStatus = status;
            staticvm.SearchStartDate = startdate == null ? Convert.ToDateTime("01-01-2000") : Convert.ToDateTime(startdate);
            staticvm.SearchEndDate = enddate == null ? DateTime.Now : Convert.ToDateTime(enddate);

            vm = staticvm;
            IEnumerable<Category> CatList = await _unitOfWork.Category.GetAllAsync();
            vm.CategoryList = CatList.Select(i => new SelectListItem
            {
                Text = i.Name,
                Value = i.Id.ToString()
            });
            var StatusList = _unitOfWork.Genmaster.GetAll().Where(z => z.GENFLAG == 2).Select(x => new SelectListItem { Value = x.IDGEN.ToString(), Text = x.GENNAME });
            var OwnershipList = _unitOfWork.Genmaster.GetAll().Where(z => z.GENFLAG == 3).Select(x => new SelectListItem { Value = x.IDGEN.ToString(), Text = x.GENNAME });
            ViewBag.StatusList = new SelectList(StatusList, "Value", "Text");
            ViewBag.OwnershipList = new SelectList(OwnershipList, "Value", "Text");           
            return View(vm);
        }

        //[HttpGet]
        public async Task<IActionResult> GetAll()
        {

            ReportItemVM vm = staticvm;
            var datalist = (from z in await _unitOfWork.Items.GetAllAsync(includeProperties: "Category,Room")
                            select new
                            {
                                id = z.Id,
                                code = z.Code,
                                name = z.Name,
                                description = z.Description,
                                categoryid = z.CategoryId,
                                category = z.Category.Name,
                                room = z.Room.Name,
                                qty = z.Qty,
                                ownership = z.OriginOfGoods,
                                price = z.Price.HasValue ? z.Price.Value.ToString("#,##0") : "",
                                totalamount = z.TotalAmount.HasValue ? z.TotalAmount.Value.ToString("#,##0") : "",
                                status = z.Status,
                                startdate = z.StartDate,
                                startdatestring = z.StartDate != null ? z.StartDate.Value.ToString("dd/MM/yyyy") : ""
                            }).ToList();
            if (vm.SearchStartDate != null && vm.SearchEndDate != null)
            {
                datalist = datalist.Where(o => o.startdate >= staticvm.SearchStartDate && o.startdate <= staticvm.SearchEndDate).ToList();
            }
            if (vm.SearchCode != null)
            {
                datalist = datalist.Where(o => o.code == vm.SearchCode).ToList();
            }
            if (vm.SearchOwnership != null)
            {
                datalist = datalist.Where(o => o.ownership == vm.SearchOwnership).ToList();
            }
            if (vm.SearchCategory != null)
            {
                datalist = datalist.Where(o => o.categoryid == vm.SearchCategory).ToList();
            }
            if (vm.SearchStatus != null)
            {
                datalist = datalist.Where(o => o.status == vm.SearchStatus).ToList();
            }

            return Json(new { data = datalist });
        }
    
        public static byte[] CreateFile<T>(List<T> source)
        {
            var workbook = new XSSFWorkbook();
            var sheet = workbook.CreateSheet("Sheet1");
            var rowHeader = sheet.CreateRow(0);

            var properties = typeof(T).GetProperties();

            //header
            var font = workbook.CreateFont();
            font.IsBold = true;
            var style = workbook.CreateCellStyle();
            style.SetFont(font);

            //var colIndex = 0;
            //foreach (var property in properties)
            //{
            //    var cell = rowHeader.CreateCell(colIndex);
            //    cell.SetCellValue(property.Name);
            //    cell.CellStyle = style;
            //    colIndex++;
            //}

            var cell = rowHeader.CreateCell(0);
            cell.SetCellValue("No");
            cell.CellStyle = style;

            cell = rowHeader.CreateCell(1);
            cell.SetCellValue("Kode");
            cell.CellStyle = style;

            cell = rowHeader.CreateCell(2);
            cell.SetCellValue("Nama Harta Tetap");
            cell.CellStyle = style;

            cell = rowHeader.CreateCell(3);
            cell.SetCellValue("Tanggal Perolehan");
            cell.CellStyle = style;

            cell = rowHeader.CreateCell(4);
            cell.SetCellValue("Keterangan");
            cell.CellStyle = style;

            cell = rowHeader.CreateCell(5);
            cell.SetCellValue("Katagori");
            cell.CellStyle = style;

            cell = rowHeader.CreateCell(6);
            cell.SetCellValue("Gedung");
            cell.CellStyle = style;

            cell = rowHeader.CreateCell(7);
            cell.SetCellValue("Ruangan");
            cell.CellStyle = style;

            cell = rowHeader.CreateCell(8);
            cell.SetCellValue("Nilai Perolehan");
            cell.CellStyle = style;

            cell = rowHeader.CreateCell(9);
            cell.SetCellValue("Jumlah");
            cell.CellStyle = style;
                  
            //end header

            //content
            var rowNum = 1;
            foreach (var item in source)
            {
                var rowContent = sheet.CreateRow(rowNum);

                var colContentIndex = 0;
                foreach (var property in properties)
                {
                    var cellContent = rowContent.CreateCell(colContentIndex);
                    var value = property.GetValue(item, null);

                    if (value == null)
                    {
                        cellContent.SetCellValue("");
                    }
                    else if (property.PropertyType == typeof(string))
                    {
                        cellContent.SetCellValue(value.ToString());
                    }
                    else if (property.PropertyType == typeof(int) || property.PropertyType == typeof(int?))
                    {
                        cellContent.SetCellValue(Convert.ToInt32(value));
                    }
                    else if (property.PropertyType == typeof(decimal) || property.PropertyType == typeof(decimal?))
                    {
                        cellContent.SetCellValue(Convert.ToDouble(value));
                    }
                    else if (property.PropertyType == typeof(DateTime) || property.PropertyType == typeof(DateTime?))
                    {
                        var dateValue = (DateTime)value;
                        cellContent.SetCellValue(dateValue.ToString("yyyy-MM-dd"));
                    }
                    else cellContent.SetCellValue(value.ToString());

                    colContentIndex++;
                }

                rowNum++;
            }

            //end content


            var stream = new MemoryStream();
            workbook.Write(stream);
            var content = stream.ToArray();

            return content;
        }
        
        public async Task<FileResult> Export()
        {
            

            var datalist = (from z in await _unitOfWork.Items.GetAllAsync(includeProperties: "Category,Room")
                            select new
                            {
                                number = z.Id,
                                code = z.Code,
                                name = z.Name,
                                description = z.Description,
                                category = z.Category.Name,
                                location = _unitOfWork.Location.Get(z.Room.IDLocation).Name,
                                room = z.Room.Name,                               
                                qty = z.Qty,
                                price = z.Price != null ? z.Price : 0,
                                totalamount = z.TotalAmount != null ? z.TotalAmount : 0,
                                ownership = z.OriginOfGoods,
                                categoryid = z.CategoryId,
                                status = z.Status,
                                startdate = z.StartDate,
                                startdatestring = z.StartDate != null ? z.StartDate.Value.ToString("dd/MM/yyyy") : ""
                            }).ToList();

            if (staticvm.SearchStartDate != null && staticvm.SearchEndDate != null)
            {
                datalist = datalist.Where(o => o.startdate >= staticvm.SearchStartDate && o.startdate <= staticvm.SearchEndDate).ToList();
            }
            if (staticvm.SearchCode != null)
            {
                datalist = datalist.Where(o => o.code == staticvm.SearchCode).ToList();
            }
            if (staticvm.SearchOwnership != null)
            {
                datalist = datalist.Where(o => o.ownership == staticvm.SearchOwnership).ToList();
            }
            if (staticvm.SearchCategory != null)
            {
                datalist = datalist.Where(o => o.categoryid == staticvm.SearchCategory).ToList();
            }
            if (staticvm.SearchStatus != null)
            {
                datalist = datalist.Where(o => o.status == staticvm.SearchStatus).ToList();
            }

            int Number = 1;
            var datalist_ = (from z in datalist
                        select new ExportReportItem
                        {
                            Number = Number++,
                            Code = z.code,
                            Name = z.name,
                            StartDate = z.startdatestring,
                            Description = z.description,
                            Category = z.category,
                            Location = z.location,
                            Room = z.room,                         
                            Price = z.price.HasValue ? z.price.Value.ToString("#,##0") : "",
                            Qty = z.qty,
                            TotalAmount = z.totalamount.HasValue ? z.totalamount.Value.ToString("#,##0") : ""
                        }).ToList();
         

            var file = CreateFile(datalist_);
            return File(file, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Export_Data_Aset.xlsx");
            

        }
        public async Task<FileResult> ExportPDF()
        {

            var datalist = (from z in await _unitOfWork.Items.GetAllAsync(includeProperties: "Category,Room")
                            select new
                            {
                                number = z.Id,
                                code = z.Code,
                                name = z.Name,
                                description = z.Description,
                                category = z.Category.Name,
                                location = _unitOfWork.Location.Get(z.Room.IDLocation).Name,
                                room = z.Room.Name,
                                qty = z.Qty,
                                price = z.Price != null ? z.Price : 0,
                                totalamount = z.TotalAmount != null ? z.TotalAmount : 0,
                                ownership = z.OriginOfGoods,
                                categoryid = z.CategoryId,
                                status = z.Status,
                                startdate = z.StartDate,
                                startdatestring = z.StartDate != null ? z.StartDate.Value.ToString("dd/MM/yyyy") : ""
                            }).ToList();
            if (staticvm.SearchStartDate != null && staticvm.SearchEndDate != null)
            {
                datalist = datalist.Where(o => o.startdate >= staticvm.SearchStartDate && o.startdate <= staticvm.SearchEndDate).ToList();
            }
            if (staticvm.SearchCode != null)
            {
                datalist = datalist.Where(o => o.code == staticvm.SearchCode).ToList();
            }
            if (staticvm.SearchOwnership != null)
            {
                datalist = datalist.Where(o => o.ownership == staticvm.SearchOwnership).ToList();
            }
            if (staticvm.SearchCategory != null)
            {
                datalist = datalist.Where(o => o.categoryid == staticvm.SearchCategory).ToList();
            }
            if (staticvm.SearchStatus != null)
            {
                datalist = datalist.Where(o => o.status == staticvm.SearchStatus).ToList();
            }

            int Number = 1;
            var datalist2 = (from z in datalist
                            select new
                            {
                                number = Number++,
                                code = z.code,
                                name = z.name,
                                description = z.description,
                                category = z.category,
                                location = z.location,
                                room = z.room,
                                price = z.price != null ? z.price.Value.ToString("#,##0") : "0",
                                qty = z.qty != null ? z.qty.Value.ToString("#,##0") : "0",
                                totalamount = z.totalamount != null ? z.totalamount.Value.ToString("#,##0") : "0"
                            }).ToList(); 

            DataTable dttbl = CreateDataTable(datalist2);
            string physicalPath = "wwwroot\\images\\products\\DataAset.pdf";
            ExportDataTableToPdf(dttbl, physicalPath, "Daftar Aset");

            
            byte[] pdfBytes = System.IO.File.ReadAllBytes(physicalPath);
            MemoryStream ms = new MemoryStream(pdfBytes);
            return new FileStreamResult(ms, "application/pdf");
            
            //return File(System.IO.File.ReadAllBytes(@"D:\test.pdf"), "application/pdf");


            //var document = new Document {
            //    PageInfo = new PageInfo { Margin = new MarginInfo(28, 28, 28, 40) }
            //};
            //var pdfpage = document.Pages.Add();
            //Table tabel = new Table 
            //{
            //    ColumnWidths = "25% 25% 25% 25%",
            //    DefaultCellPadding = new MarginInfo(10,5,10,5),
            //    Border = new BorderInfo(BorderSide.All, .5f, Color.Black),
            //    DefaultCellBorder = new BorderInfo(BorderSide.All, .2f, Color.Black),

            //};
            ////CreateDataTable(datalist);

            //tabel.ImportDataTable(CreateDataTable(datalist), true, 0, 0);
            //document.Pages[1].Paragraphs.Add(tabel);

            //using (var streamout = new MemoryStream())
            //{
            //    document.Save(streamout);
            //    return new FileContentResult(streamout.ToArray(), "application/pdf")
            //    { 
            //        FileDownloadName = "Data_Aset.pdf"
            //    };

            //}
            //return null;

        }

        public static DataTable CreateDataTable<T>(IEnumerable<T> entities)
        {
            var dt = new DataTable();

            //creating columns
            foreach (var prop in typeof(T).GetProperties())
            {
                dt.Columns.Add(prop.Name, prop.PropertyType);
            }

            //creating rows
            foreach (var entity in entities)
            {
                var values = GetObjectValues(entity);
                dt.Rows.Add(values);
            }


            return dt;
        }

        public static object[] GetObjectValues<T>(T entity)
        {
            var values = new List<object>();
            foreach (var prop in typeof(T).GetProperties())
            {
                values.Add(prop.GetValue(entity));
            }

            return values.ToArray();
        }

        void ExportDataTableToPdf(DataTable dtblTable, String strPdfPath, string strHeader)
        { 
           
            System.IO.FileStream fs = new FileStream(strPdfPath, FileMode.Create, FileAccess.Write, FileShare.None);
            Document document = new Document();
            document.SetPageSize(iTextSharp.text.PageSize.A4);
            PdfWriter writer = PdfWriter.GetInstance(document, fs);
            document.Open();

            //Report Header
            BaseFont bfntHead = BaseFont.CreateFont(BaseFont.HELVETICA_BOLD, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
            iTextSharp.text.Font fntHead = new iTextSharp.text.Font(bfntHead, 16, 1, BaseColor.DARK_GRAY);
            Paragraph prgHeading = new Paragraph();
            prgHeading.Alignment = Element.ALIGN_CENTER;
            prgHeading.Add(new Chunk(strHeader.ToUpper(), fntHead));
            document.Add(prgHeading);

            //Author
            //Paragraph prgAuthor = new Paragraph();
            //BaseFont btnAuthor = BaseFont.CreateFont(BaseFont.HELVETICA_BOLD, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
            //iTextSharp.text.Font fntAuthor = new iTextSharp.text.Font(btnAuthor, 8, 2, BaseColor.DARK_GRAY);
            //prgAuthor.Alignment = Element.ALIGN_RIGHT;
            ////prgAuthor.Add(new Chunk("Author : Dotnet Mob", fntAuthor));
            //prgAuthor.Add(new Chunk("\nPrint Date : " + DateTime.Now.ToShortDateString(), fntAuthor));
            //document.Add(prgAuthor);

            //Add a line seperation
            //Paragraph p = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, BaseColor.BLACK, Element.ALIGN_LEFT, 1)));
            //document.Add(p);

            //Add line break
            document.Add(new Chunk("\n", fntHead));

            //Write the table
            //PdfPTable table = new PdfPTable(dtblTable.Columns.Count);
            PdfPTable table = new PdfPTable(10);
            table.HorizontalAlignment = 0;
            table.TotalWidth = 520f;
            table.LockedWidth = true;
            float[] widths = new float[] { 10f, 50f, 80f, 50f, 40f, 30f, 30f, 30f, 20f, 30f, };
            table.SetWidths(widths);
            //Table header
            BaseFont btnColumnHeader = BaseFont.CreateFont(BaseFont.HELVETICA_BOLD, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
            iTextSharp.text.Font fntColumnHeader = new  iTextSharp.text.Font(btnColumnHeader, 8, 1, BaseColor.WHITE);
            //for (int i = 0; i < dtblTable.Columns.Count; i++)
            //{
            //    PdfPCell cell = new PdfPCell();
            //    cell.BackgroundColor = BaseColor.GRAY;
            //    cell.AddElement(new Chunk(dtblTable.Columns[i].ColumnName.ToUpper(), fntColumnHeader));
            //    table.AddCell(cell);
            //}
            //PdfPCell cell = new PdfPCell();
            //cell.BackgroundColor = BaseColor.DARK_GRAY;
            //cell.AddElement(new Chunk("No", fntColumnHeader));
            //table.AddCell(cell);
            //PdfPCell cell2 = new PdfPCell();
            //cell2.BackgroundColor = BaseColor.DARK_GRAY;
            //cell2.AddElement(new Chunk("Kode", fntColumnHeader));
            //table.AddCell(cell2);
            //PdfPCell cell3 = new PdfPCell();
            //cell3.BackgroundColor = BaseColor.DARK_GRAY;
            //cell3.AddElement(new Chunk("Nama Harta Tetap", fntColumnHeader));
            //table.AddCell(cell3);
            //PdfPCell cell4 = new PdfPCell();
            //cell4.BackgroundColor = BaseColor.DARK_GRAY;
            //cell4.AddElement(new Chunk("Keterangan", fntColumnHeader));
            //table.AddCell(cell4);
            //PdfPCell cell5 = new PdfPCell();
            //cell5.BackgroundColor = BaseColor.DARK_GRAY;
            //cell5.AddElement(new Chunk("Katagori", fntColumnHeader));
            //table.AddCell(cell5);
            //PdfPCell cell6 = new PdfPCell();
            //cell6.BackgroundColor = BaseColor.DARK_GRAY;
            //cell6.AddElement(new Chunk("Gedung", fntColumnHeader));
            //table.AddCell(cell6);
            //PdfPCell cell7 = new PdfPCell();
            //cell7.BackgroundColor = BaseColor.DARK_GRAY;
            //cell7.AddElement(new Chunk("Ruangan", fntColumnHeader));
            //table.AddCell(cell7);
            //PdfPCell cell8 = new PdfPCell();
            //cell8.BackgroundColor = BaseColor.DARK_GRAY;
            //cell8.AddElement(new Chunk("Nilai Perolehan", fntColumnHeader));
            //table.AddCell(cell8);
            //PdfPCell cell9 = new PdfPCell();
            //cell9.BackgroundColor = BaseColor.DARK_GRAY;
            //cell9.AddElement(new Chunk("Jumlah", fntColumnHeader));
            //table.AddCell(cell9);
            //PdfPCell cell10 = new PdfPCell();
            //cell10.BackgroundColor = BaseColor.DARK_GRAY;
            //cell10.AddElement(new Chunk("Total Nilai", fntColumnHeader));
            //table.AddCell(cell10);

            addCell(table, "No", 1);
            addCell(table, "Kode", 1);
            addCell(table, "Nama Harta Tetap", 1);
            addCell(table, "Keterangan", 1);
            addCell(table, "Katagori", 1);
            addCell(table, "Gedung", 1);
            addCell(table, "Ruangan", 1);
            addCell(table, "Nilai Perolehan", 1);
            addCell(table, "Jumlah", 1);
            addCell(table, "Total Nilai", 1);
            //table Data
            for (int i = 0; i < dtblTable.Rows.Count; i++)
            {
                for (int j = 0; j < dtblTable.Columns.Count; j++)
                {                 
                    BaseFont bfTimes = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, false);
                    iTextSharp.text.Font times = new iTextSharp.text.Font(bfTimes, 6, iTextSharp.text.Font.NORMAL, iTextSharp.text.BaseColor.BLACK);

                    PdfPCell cell = new PdfPCell(new Phrase(dtblTable.Rows[i][j].ToString(), times));
                    cell.Rowspan = 1;
                    cell.HorizontalAlignment = PdfPCell.ALIGN_CENTER;
                    cell.VerticalAlignment = PdfPCell.ALIGN_MIDDLE;
                    table.AddCell(cell);
                    //table.AddCell(dtblTable.Rows[i][j].ToString());
                }
            }

            document.Add(table);
            document.Close();
            writer.Close();
            fs.Close();

        }

        private static void addCell(PdfPTable table, string text, int rowspan)
        {
            BaseFont bfTimes = BaseFont.CreateFont(BaseFont.TIMES_BOLD, BaseFont.CP1252, false);
            iTextSharp.text.Font times = new iTextSharp.text.Font(bfTimes, 7, iTextSharp.text.Font.NORMAL, iTextSharp.text.BaseColor.BLACK);

            PdfPCell cell = new PdfPCell(new Phrase(text, times));
            cell.BackgroundColor = BaseColor.WHITE;
            cell.Rowspan = rowspan;
            cell.HorizontalAlignment = PdfPCell.ALIGN_CENTER;
            cell.VerticalAlignment = PdfPCell.ALIGN_MIDDLE;
            table.AddCell(cell);
        }

    }
}
