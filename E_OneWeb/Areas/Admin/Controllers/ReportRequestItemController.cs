using E_OneWeb.DataAccess.Repository.IRepository;
using E_OneWeb.Models.ViewModels;
using E_OneWeb.Models;
using E_OneWeb.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NPOI.XSSF.UserModel;
using static com.sun.tools.@internal.xjc.reader.xmlschema.bindinfo.BIConversion;
using iTextSharp.text.pdf;
using iTextSharp.text;
using System.Data;

namespace E_OneWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
    public class ReportRequestItemController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private static ReportRequestItemVM staticvm = new ReportRequestItemVM();
        public ReportRequestItemController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<IActionResult> Index(ReportRequestItemVM vm)
        {
            staticvm = vm;
            vm.MonthList = MonthItems.Where(x => x != "").Select((m, i) => new SelectListItem
            {
                Value = (i + 1).ToString(),
                Text = m
            }).ToList();
            ViewBag.SearchMonthList = new SelectList(vm.MonthList, "Value", "Text");
            var StatusList = _unitOfWork.Genmaster.GetAll().Where(z => z.GENFLAG == 6).Select(x => new SelectListItem { Value = x.IDGEN.ToString(), Text = x.GENNAME });
            ViewBag.StatusList = new SelectList(StatusList, "Value", "Text");
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

            var datalist = (from z in await _unitOfWork.RequestItemHeader.GetAllAsync()
                            select new
                            {
                                id = z.Id,
                                refnumber = z.ReqNumber,
                                requestdate = z.RequestDate != null ? z.RequestDate.Value.ToString("dd/MM/yyyy") : "",
                                requestby = z.Requester,
                                status = z.Status,
                                statusid = z.StatusId,
                                month = z.RequestDate != null ? z.RequestDate.Value.Month : 0,
                                year = z.RequestDate != null ? z.RequestDate.Value.Year : 0,
                                totalamount = z.TotalAmount.HasValue ? z.TotalAmount.Value.ToString("#,##0") : ""
                            }).ToList();

            if (staticvm.SearchRefNumber != null)
            {
                datalist = datalist.Where(o => o.refnumber.ToLower().Contains(staticvm.SearchRefNumber.ToLower()) ).ToList();
            }
            if (staticvm.SearchMonth != null)
            {
                datalist = datalist.Where(o => o.month == staticvm.SearchMonth).ToList();
            }
            if (staticvm.SearchYear != null)
            {
                datalist = datalist.Where(o => o.year == staticvm.SearchYear).ToList();
            }
            if (staticvm.SearchStatus != null)
            {
                datalist = datalist.Where(o => o.statusid == staticvm.SearchStatus).ToList();
            }

            return Json(new { data = datalist });
        }

        #region Export Data Excel
        public async Task<FileResult> Export(string code)
        {
            var requestheader = await _unitOfWork.RequestItemHeader.GetAllAsync();

            var datalist = (from z in await _unitOfWork.RequestItemDetail.GetAllAsync()
                            select new
                            {
                                id = z.Id,
                                refnumber = requestheader.Where(x => x.Id == z.IdHeader).Select(i => i.ReqNumber).FirstOrDefault(),
                                requestdate = requestheader.Where(x => x.Id == z.IdHeader).Select(i => i.RequestDate).FirstOrDefault() != null
                                            ? requestheader.Where(x => x.Id == z.IdHeader).Select(i => i.RequestDate.Value.ToString("dd/MM/yyyy")).FirstOrDefault() : "",
                                requestby = requestheader.Where(x => x.Id == z.IdHeader).Select(i => i.Requester).FirstOrDefault(),
                                month = requestheader.Where(x => x.Id == z.IdHeader).Select(i => i.RequestDate).FirstOrDefault() != null
                                        ? requestheader.Where(x => x.Id == z.IdHeader).Select(i => i.RequestDate.Value.Month).FirstOrDefault() : 0,
                                year = requestheader.Where(x => x.Id == z.IdHeader).Select(i => i.RequestDate).FirstOrDefault() != null
                                        ? requestheader.Where(x => x.Id == z.IdHeader).Select(i => i.RequestDate.Value.Year).FirstOrDefault() : 0,
                                totalamount = requestheader.Where(x => x.Id == z.IdHeader).Select(i => i.TotalAmount.Value.ToString("#,##0")).FirstOrDefault(),
                                status = z.Status,
                                statusid = z.StatusId,
                                name = z.Name,
                                reason = z.Reason,
                                spesifik = z.Specification,
                                price = z.Price.HasValue ? z.Price.Value.ToString("#,##0") : "",
                                qty = z.Qty.HasValue ? z.Qty.Value.ToString("#,##0") : "",
                                total = z.Total.HasValue ? z.Total.Value.ToString("#,##0") : "",
                            }).ToList();


            if (staticvm.SearchRefNumber != null)
            {
                datalist = datalist.Where(o => o.refnumber.ToLower().Contains(staticvm.SearchRefNumber.ToLower())).ToList();
            }
            if (staticvm.SearchMonth != null)
            {
                datalist = datalist.Where(o => o.month == staticvm.SearchMonth).ToList();
            }
            if (staticvm.SearchYear != null)
            {
                datalist = datalist.Where(o => o.year == staticvm.SearchYear).ToList();
            }
            if (staticvm.SearchStatus != null)
            {
                datalist = datalist.Where(o => o.statusid == staticvm.SearchStatus).ToList();
            }

            int Number = 1;
            var datalist_ = (from z in datalist
                             select new ExportReportRequestItem
                             {
                                 Number = Number++,
                                 RefNumber = z.refnumber,
                                 RequestDate = z.requestdate,
                                 Requester = z.requestby,
                                 Name = z.name,
                                 Reason = z.reason,
                                 Specification = z.spesifik,
                                 Price = z.price,
                                 Qty = z.qty,
                                 Total = z.total,
                                 Status = z.status,
                                 TotalAmount = z.totalamount
                             }).ToList();


            var file = CreateFile(datalist_);
            return File(file, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "DaftarPermintaanAset.xlsx");


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

            var cell = rowHeader.CreateCell(0);
            cell.SetCellValue("No");
            cell.CellStyle = style;
            cell = rowHeader.CreateCell(1);
            cell.SetCellValue("No.Referensi");
            cell.CellStyle = style;
            cell = rowHeader.CreateCell(2);
            cell.SetCellValue("Tanggal");
            cell.CellStyle = style;
            cell = rowHeader.CreateCell(3);
            cell.SetCellValue("Permintaan");
            cell.CellStyle = style;
            cell = rowHeader.CreateCell(4);
            cell.SetCellValue("Nama Aset");
            cell.CellStyle = style;
            cell = rowHeader.CreateCell(5);
            cell.SetCellValue("Catatan");
            cell.CellStyle = style;
            cell = rowHeader.CreateCell(6);
            cell.SetCellValue("Spesifikasi");
            cell.CellStyle = style;
            cell = rowHeader.CreateCell(7);
            cell.SetCellValue("Nilai");
            cell.CellStyle = style;
            cell = rowHeader.CreateCell(8);
            cell.SetCellValue("Jumlah");
            cell.CellStyle = style;
            cell = rowHeader.CreateCell(9);
            cell.SetCellValue("Total Nilai");
            cell.CellStyle = style;
            cell = rowHeader.CreateCell(10);
            cell.SetCellValue("Status");
            cell.CellStyle = style;
            cell = rowHeader.CreateCell(11);
            cell.SetCellValue("Total Keseluruhan");
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
        #endregion

        #region Export Data PDF
        public async Task<FileResult> ExportPDF()
        {

            var requestheader = await _unitOfWork.RequestItemHeader.GetAllAsync();

            var datalist = (from z in await _unitOfWork.RequestItemDetail.GetAllAsync()
                            select new
                            {
                                id = z.Id,
                                refnumber = requestheader.Where(x => x.Id == z.IdHeader).Select(i => i.ReqNumber).FirstOrDefault(),
                                requestdate = requestheader.Where(x => x.Id == z.IdHeader).Select(i => i.RequestDate).FirstOrDefault() != null
                                            ? requestheader.Where(x => x.Id == z.IdHeader).Select(i => i.RequestDate.Value.ToString("dd/MM/yyyy")).FirstOrDefault() : "",
                                requestby = requestheader.Where(x => x.Id == z.IdHeader).Select(i => i.Requester).FirstOrDefault(),
                                month = requestheader.Where(x => x.Id == z.IdHeader).Select(i => i.RequestDate).FirstOrDefault() != null
                                        ? requestheader.Where(x => x.Id == z.IdHeader).Select(i => i.RequestDate.Value.Month).FirstOrDefault() : 0,
                                year = requestheader.Where(x => x.Id == z.IdHeader).Select(i => i.RequestDate).FirstOrDefault() != null
                                        ? requestheader.Where(x => x.Id == z.IdHeader).Select(i => i.RequestDate.Value.Year).FirstOrDefault() : 0,
                                totalamount = requestheader.Where(x => x.Id == z.IdHeader).Select(i => i.TotalAmount.Value.ToString("#,##0")).FirstOrDefault(),
                                status = z.Status,
                                statusid = z.StatusId,
                                name = z.Name,
                                reason = z.Reason,
                                spesifik = z.Specification,
                                price = z.Price.HasValue ? z.Price.Value.ToString("#,##0") : "",
                                qty = z.Qty.HasValue ? z.Qty.Value.ToString("#,##0") : "",
                                total = z.Total.HasValue ? z.Total.Value.ToString("#,##0") : "",
                            }).ToList();


            if (staticvm.SearchRefNumber != null)
            {
                datalist = datalist.Where(o => o.refnumber.ToLower().Contains(staticvm.SearchRefNumber.ToLower())).ToList();
            }
            if (staticvm.SearchMonth != null)
            {
                datalist = datalist.Where(o => o.month == staticvm.SearchMonth).ToList();
            }
            if (staticvm.SearchYear != null)
            {
                datalist = datalist.Where(o => o.year == staticvm.SearchYear).ToList();
            }
            if (staticvm.SearchStatus != null)
            {
                datalist = datalist.Where(o => o.statusid == staticvm.SearchStatus).ToList();
            }

            int Number = 1;
            var datalist_ = (from z in datalist
                             select new
                             {
                                 Number = Number++,
                                 RefNumber = z.refnumber,
                                 RequestDate = z.requestdate,
                                 Requester = z.requestby,
                                 Name = z.name,
                                 Reason = z.reason,
                                 Specification = z.spesifik,
                                 Price = z.price,
                                 Qty = z.qty,
                                 Total = z.total,
                                 Status = z.status,
                                 TotalAmount = z.totalamount
                             }).ToList();

            DataTable dttbl = CreateDataTable(datalist_);
            string physicalPath = "wwwroot\\images\\products\\DaftarPermintaanAset.pdf";
            ExportDataTableToPdf(dttbl, physicalPath, "Daftar Permintaan Aset");


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
            PdfPTable table = new PdfPTable(12);
            table.HorizontalAlignment = 0;
            table.TotalWidth = 520f;
            table.LockedWidth = true;
            float[] widths = new float[] { 20f, 30f, 30f, 40f, 60f, 60f, 60f, 30f, 30f, 30f, 30f, 40f };
            table.SetWidths(widths);
            //Table header
            BaseFont btnColumnHeader = BaseFont.CreateFont(BaseFont.HELVETICA_BOLD, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
            iTextSharp.text.Font fntColumnHeader = new iTextSharp.text.Font(btnColumnHeader, 8, 1, BaseColor.WHITE);

            addCell(table, "No", 1);
            addCell(table, "No. Ref", 1);
            addCell(table, "Tanggal", 1);
            addCell(table, "Permintaan", 1);
            addCell(table, "Nama Aset", 1);
            addCell(table, "Catatan", 1);
            addCell(table, "Spesifikasi", 1);
            addCell(table, "Nilai", 1);
            addCell(table, "Jumlah", 1);
            addCell(table, "Total Nilai", 1);
            addCell(table, "Status", 1);
            addCell(table, "Total Keseluruhan", 1);

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
