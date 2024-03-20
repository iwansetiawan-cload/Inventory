using E_OneWeb.DataAccess.Repository.IRepository;
using E_OneWeb.Models;
using E_OneWeb.Models.ViewModels;
using E_OneWeb.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Finance.Implementations;
using System.Data;
using static com.sun.tools.@internal.xjc.reader.xmlschema.bindinfo.BIConversion;
using NPOI.XSSF.UserModel;
using Microsoft.AspNetCore.Mvc.Rendering;
using iTextSharp.text.pdf;
using iTextSharp.text;

namespace E_OneWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
    public class ReportDepreciationExpenseController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private static ReportReportDepreciationExpenseVM staticvm = new ReportReportDepreciationExpenseVM();
        public ReportDepreciationExpenseController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<IActionResult> Index(ReportReportDepreciationExpenseVM vm)
        {
            vm.SearchStartDate = vm.SearchStartDate == null ? Convert.ToDateTime("01-01-2000") : vm.SearchStartDate;
            vm.SearchEndDate = vm.SearchEndDate == null ? DateTime.Now : vm.SearchEndDate;
            vm.SearchCalculateDate = vm.SearchCalculateDate == null ? DateTime.Now : vm.SearchCalculateDate;
            //vm.SearchYear = vm.SearchYear == null ? DateTime.Now.Year : vm.SearchYear;
            //vm.SearchMonth = vm.SearchMonth == null ? DateTime.Now.Month : vm.SearchMonth;
            IEnumerable<Category> CatList = await _unitOfWork.Category.GetAllAsync();
            vm.CategoryList = CatList.Select(i => new SelectListItem
            {
                Text = i.Name,
                Value = i.Id.ToString()
            });
            vm.MonthList = MonthItems.Where(x => x != "").Select((m, i) => new SelectListItem
            {
                Value = (i + 1).ToString(),
                Text = m
            }).ToList();
            ViewBag.SearchMonthList = new SelectList(vm.MonthList, "Value", "Text");
            staticvm = vm;
            return View(vm);
        }
        public static IEnumerable<String> MonthItems
        {
            get
            {
                return new System.Globalization.DateTimeFormatInfo().MonthNames;
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {

            var datalist = (from z in await _unitOfWork.Items.GetAllAsync(includeProperties: "Category,Room")
                            select new
                            {
                                id = z.Id,
                                code = z.Code,
                                name = z.Name,
                                description = z.Description,
                                startdate = z.StartDate,
                                startdatestring = z.StartDate != null ? z.StartDate.Value.ToString("dd/MM/yyyy") : "",
                                categoryid = z.CategoryId,
                                category = z.Category.Name,
                                persent = z.Percent,
                                period = z.Period,
                                qty = z.Qty,
                                ownership = z.OriginOfGoods,
                                price = z.Price.HasValue ? z.Price.Value.ToString("#,##0") : "",
                                totalamount = z.TotalAmount.HasValue ? z.TotalAmount.Value.ToString("#,##0") : "",
                                status = z.Status
                            }).ToList();
            if (staticvm.SearchStartDate != null && staticvm.SearchEndDate != null)
            {
                datalist = datalist.Where(o => o.startdate >= staticvm.SearchStartDate && o.startdate <= staticvm.SearchEndDate).ToList();
            }          
            if (staticvm.SearchCategory != null)
            {
                datalist = datalist.Where(o => o.categoryid == staticvm.SearchCategory).ToList();
            }
           
            return Json(new { data = datalist });
        }
        
        private decimal GetNilaiPenyusutan(DateTime? StartDate, double? TotalAmount, int? Period, decimal? Persen, DateTime DtPeriode)
        {
            decimal Result = 0;
            try
            {
                DateTime dtStartDate = StartDate.Value.AddYears((int)Period);
                int? GetDays = (int)CalculateInMonth365(StartDate.Value, DtPeriode);
                int? GetPeriodDays = (int)CalculateInMonth365(StartDate.Value, dtStartDate);

                decimal? Penyusutan_perHari = ((decimal)TotalAmount * Persen / 100) / 12;
                decimal? Pembagian_perhari = (decimal)GetDays / (decimal)GetPeriodDays;
                decimal? Nilai_penyusutan = Penyusutan_perHari * 12 * Period * Pembagian_perhari;
                Result = (decimal)Nilai_penyusutan;
                if (Result < 0)
                {
                    Result = 0;
                }
                if (Result > (decimal)TotalAmount)
                {
                    Result = (decimal)TotalAmount;
                }
            }
            catch (Exception ex)
            {
                string err = ex.Message.ToString();
                return Result;
            }

            return Result;
        }

        private decimal GetNilaiBuku(DateTime? StartDate, double? TotalAmount, int? Period, decimal? Persen, DateTime DtPeriode)
        {
            decimal Result = 1;
            try
            {
                DateTime dtStartDate = StartDate.Value.AddYears((int)Period);
                int? GetDays = (int)CalculateInMonth365(StartDate.Value, DtPeriode);
                int? GetPeriodDays = (int)CalculateInMonth365(StartDate.Value, dtStartDate);

                decimal? Penyusutan_perHari = ((decimal)TotalAmount * Persen / 100) / 12;
                decimal? Pembagian_perhari = (decimal)GetDays / (decimal)GetPeriodDays;
                decimal? Nilai_penyusutan = Penyusutan_perHari * 12 * Period * Pembagian_perhari;
                decimal Nilai_buku = (decimal)TotalAmount - (decimal)Nilai_penyusutan;
                Result = Nilai_buku;
                if (Result < 0)
                {
                    Result = 1;
                }
            }
            catch (Exception ex)
            {
                string err = ex.Message.ToString();
                return Result;
            }

            return Result;
        }
        public double CalculateInMonth365(DateTime tgl2, DateTime tgl1)
        {
            double Result = 0;
            Result = (tgl1 - tgl2).TotalDays;
            return Result;
        }

        #region Export Data Excel
        public async Task<FileResult> Export()
        {


            var datalist = (from z in await _unitOfWork.Items.GetAllAsync(includeProperties: "Category,Room")
                            select new
                            {
                                id = z.Id,
                                code = z.Code,
                                name = z.Name,
                                description = z.Description,
                                startdate = z.StartDate,
                                startdatestring = z.StartDate != null ? z.StartDate.Value.ToString("dd/MM/yyyy") : "",
                                categoryid = z.CategoryId,
                                category = z.Category.Name,
                                persent = z.Percent,
                                period = z.Period,
                                qty = z.Qty,
                                ownership = z.OriginOfGoods,
                                price = z.Price,
                                totalamount = z.TotalAmount,
                                pricestring = z.Price.HasValue ? z.Price.Value.ToString("#,##0") : "",
                                totalamountstring = z.TotalAmount.HasValue ? z.TotalAmount.Value.ToString("#,##0") : "",
                                status = z.Status,
                                //expenseamount = ((decimal)z.TotalAmount * z.Percent) / 100,
                                //expensetotalamount = (decimal)z.TotalAmount - (((decimal)z.TotalAmount * z.Percent) / 100),
                                //startdate_calculate = z.Period != null ? z.StartDate.Value.AddYears((int)z.Period) : z.StartDate,
                            }).ToList();

            if (staticvm.SearchStartDate != null && staticvm.SearchEndDate != null)
            {
                datalist = datalist.Where(o => o.startdate >= staticvm.SearchStartDate && o.startdate <= staticvm.SearchEndDate).ToList();
            }
            if (staticvm.SearchCategory != null)
            {
                datalist = datalist.Where(o => o.categoryid == staticvm.SearchCategory).ToList();
            }

            //DateTime DtPeriode = new DateTime((int)staticvm.SearchYear, (int)staticvm.SearchMonth, 1);
            DateTime DtPeriode = staticvm.SearchCalculateDate == null ? DateTime.Now : (DateTime)staticvm.SearchCalculateDate;
            int Number = 1;
            var datalist_ = (from z in datalist
                             select new ExportReportDepreciationExpense
                             {
                                 Number = Number++,
                                 Name = z.name,
                                 StartDate = z.startdatestring,
                                 Category = z.category,
                                 Percent = z.persent,
                                 Qty = z.qty,
                                 TotalAmount = z.totalamountstring,
                                 //ExpenseAmount = z.startdate_calculate <= DtPeriode ? z.expenseamount.Value.ToString("#,##0") : "0",
                                 ExpenseAmount = GetNilaiPenyusutan(z.startdate, z.totalamount, z.period, z.persent, DtPeriode).ToString("#,##0"),
                                 //ExpenseTotalAmount = z.startdate_calculate <= DtPeriode ? z.expensetotalamount.Value.ToString("#,##0") : z.totalamount.Value.ToString("#,##0")
                                 ExpenseTotalAmount = GetNilaiBuku(z.startdate, z.totalamount, z.period, z.persent, DtPeriode).ToString("#,##0")
                             }).ToList();

            double? totalPerolehan = datalist_.Sum(o => Convert.ToDouble(o.TotalAmount));
            double? totalPenyusutann = datalist_.Sum(o => Convert.ToDouble(o.ExpenseAmount));

            var file = CreateFile(datalist_, totalPerolehan, totalPenyusutann);
            return File(file, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Export_Data_Penyusutan_Aset.xlsx");

        }
        public static byte[] CreateFile<T>(List<T> source, double? totalPerolehan,double? totalPenyusutann)
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

            var cell = rowHeader.CreateCell(0);
            cell.SetCellValue("No");
            cell.CellStyle = style;

            cell = rowHeader.CreateCell(1);
            cell.SetCellValue("Nama Harta Tetap");
            cell.CellStyle = style;

            cell = rowHeader.CreateCell(2);
            cell.SetCellValue("Tanggal Perolehan");
            cell.CellStyle = style;

            cell = rowHeader.CreateCell(3);
            cell.SetCellValue("Katagori");
            cell.CellStyle = style;

            cell = rowHeader.CreateCell(4);
            cell.SetCellValue("Persen");
            cell.CellStyle = style;

            cell = rowHeader.CreateCell(5);
            cell.SetCellValue("Jumlah");
            cell.CellStyle = style;

            cell = rowHeader.CreateCell(6);
            cell.SetCellValue("Nilai Perolehan");
            cell.CellStyle = style;

            cell = rowHeader.CreateCell(7);
            cell.SetCellValue("Nilai Penyusutan");
            cell.CellStyle = style;

            cell = rowHeader.CreateCell(8);
            cell.SetCellValue("Nilai Buku");           
            cell.CellStyle = style;

            //end header

            //content
            var rowNum = 1;
            double? TotalPerolehan = 0;   

            foreach (var item in source)
            {
                TotalPerolehan = TotalPerolehan ;
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

            #region Footer
            var rowFooter = sheet.CreateRow(rowNum + 1);
            cell = rowFooter.CreateCell(5);
            cell.SetCellValue("Total : ");
            cell.CellStyle = style;

            cell = rowFooter.CreateCell(6);
            cell.SetCellValue("" + totalPerolehan.Value.ToString("#,##0") + "");
            cell.CellStyle = style;

            cell = rowFooter.CreateCell(7);
            cell.SetCellValue("" + totalPenyusutann.Value.ToString("#,##0") + "");
            cell.CellStyle = style;
            #endregion


            var stream = new MemoryStream();
            workbook.Write(stream);
            var content = stream.ToArray();

            return content;
        }
        #endregion

        #region Export Data PDF
        public async Task<FileResult> ExportPDF()
        {

            var datalist = (from z in await _unitOfWork.Items.GetAllAsync(includeProperties: "Category,Room")
                            select new
                            {
                                id = z.Id,
                                code = z.Code,
                                name = z.Name,
                                description = z.Description,
                                startdate = z.StartDate,
                                startdatestring = z.StartDate != null ? z.StartDate.Value.ToString("dd/MM/yyyy") : "",
                                categoryid = z.CategoryId,
                                category = z.Category.Name,
                                persent = z.Percent,
                                period = z.Period,
                                qty = z.Qty,
                                ownership = z.OriginOfGoods,
                                price = z.Price,
                                totalamount = z.TotalAmount,
                                pricestring = z.Price.HasValue ? z.Price.Value.ToString("#,##0") : "",
                                totalamountstring = z.TotalAmount.HasValue ? z.TotalAmount.Value.ToString("#,##0") : "",
                                status = z.Status
                            }).ToList();

            if (staticvm.SearchStartDate != null && staticvm.SearchEndDate != null)
            {
                datalist = datalist.Where(o => o.startdate >= staticvm.SearchStartDate && o.startdate <= staticvm.SearchEndDate).ToList();
            }
            if (staticvm.SearchCategory != null)
            {
                datalist = datalist.Where(o => o.categoryid == staticvm.SearchCategory).ToList();
            }

            DateTime DtPeriode = staticvm.SearchCalculateDate == null ? DateTime.Now : (DateTime)staticvm.SearchCalculateDate;
            int Number = 1;
            var datalist_ = (from z in datalist
                             select new 
                             {
                                 Number = Number++,
                                 Name = z.name,
                                 StartDate = z.startdatestring,
                                 Category = z.category,
                                 Percent = z.persent != null ? z.persent.Value.ToString("#,##0.00") : "0",
                                 Qty = z.qty != null ? z.qty.Value.ToString("#,##0") : "0",
                                 TotalAmount = z.totalamountstring,
                                 ExpenseAmount = GetNilaiPenyusutan(z.startdate, z.totalamount, z.period, z.persent, DtPeriode).ToString("#,##0"),
                                 ExpenseTotalAmount = GetNilaiBuku(z.startdate, z.totalamount, z.period, z.persent, DtPeriode).ToString("#,##0")
                             }).ToList();

            DataTable dttbl = CreateDataTable(datalist_);
            string physicalPath = "wwwroot\\images\\products\\DaftarPenyusutanAset.pdf";
            ExportDataTableToPdf(dttbl, physicalPath, "Daftar Nilai Penyusutan Aset");


            byte[] pdfBytes = System.IO.File.ReadAllBytes(physicalPath);
            MemoryStream ms = new MemoryStream(pdfBytes);
            return new FileStreamResult(ms, "application/pdf");

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
            iTextSharp.text.Font fntHead = new iTextSharp.text.Font(bfntHead, 14, 1, BaseColor.DARK_GRAY);
            Paragraph prgHeading = new Paragraph();
            prgHeading.Alignment = Element.ALIGN_CENTER;
            prgHeading.Add(new Chunk(strHeader.ToUpper(), fntHead));
            document.Add(prgHeading);

            //Add line break
            document.Add(new Chunk("\n", fntHead));

            //Write the table         
            PdfPTable table = new PdfPTable(9);
            table.HorizontalAlignment = 0;
            table.TotalWidth = 520f;
            table.LockedWidth = true;
            float[] widths = new float[] { 20f, 80f, 30f, 30f, 30f, 30f, 50f, 50f, 50f };
            table.SetWidths(widths);
            //Table header
            BaseFont btnColumnHeader = BaseFont.CreateFont(BaseFont.HELVETICA_BOLD, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
            iTextSharp.text.Font fntColumnHeader = new iTextSharp.text.Font(btnColumnHeader, 8, 1, BaseColor.WHITE);

            addCell(table, "No", 1);
            addCell(table, "Nama Harta Tetap", 1);
            addCell(table, "Tanggal Perolehan", 1);
            addCell(table, "Katagori", 1);
            addCell(table, "Persen", 1);
            addCell(table, "Jumlah", 1);
            addCell(table, "Nilai Perolehan", 1);
            addCell(table, "Nilai Penyusutan", 1);
            addCell(table, "Nilai Buku", 1);          

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
        #endregion

    }
}
