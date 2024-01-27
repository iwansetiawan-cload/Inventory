using E_OneWeb.DataAccess.Repository.IRepository;
using E_OneWeb.Models.ViewModels;
using E_OneWeb.Models;
using E_OneWeb.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NPOI.XSSF.UserModel;
using System.Data;
using iTextSharp.text.pdf;
using iTextSharp.text;

namespace E_OneWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
    public class ReportServiceItemController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private static ReportServiceVM staticvm = new ReportServiceVM();
        public ReportServiceItemController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<IActionResult> Index(ReportServiceVM vm)
        {           
            staticvm = vm;
            vm.MonthList = MonthItems.Where(x => x != "").Select((m, i) => new SelectListItem
            {
                Value = (i + 1).ToString(),
                Text = m
            }).ToList();
            ViewBag.SearchMonthList = new SelectList(vm.MonthList, "Value", "Text");
            var StatusList = _unitOfWork.Genmaster.GetAll().Where(z => z.GENFLAG == 6).Select(x => new SelectListItem { Value = x.IDGEN.ToString(), Text = x.GENNAME });
            ViewBag.SearchStatusList = new SelectList(StatusList, "Value", "Text");
            return View(vm);
        }
        public static IEnumerable<String> MonthItems
        {
            get
            {
                return new System.Globalization.DateTimeFormatInfo().MonthNames;
            }
        }
  
        public async Task<IActionResult> GetAll()
        {

            var datalist = (from z in await _unitOfWork.ItemService.GetAllAsync(includeProperties: "Items")
                            select new
                            {
                                id = z.Id,
                                itemid = z.ItemId,
                                code = z.Items.Code,
                                name = z.Items.Name,
                                description = z.RepairDescription,
                                servicedate = z.ServiceDate != null ? z.ServiceDate.Value.ToString("dd/MM/yyyy") : "",
                                serviceenddate = z.ServiceEndDate != null ? z.ServiceEndDate.Value.ToString("dd/MM/yyyy") : "",
                                requestby = z.RequestBy ,
                                cost = z.CostOfRepair.HasValue ? z.CostOfRepair.Value.ToString("#,##0") : "",
                                status = z.Status,
                                statusid = z.StatusId,
                                month = z.ServiceDate != null ? z.ServiceDate.Value.Month : 0,
                                year = z.ServiceDate != null ? z.ServiceDate.Value.Year : 0,
                            }).ToList();

            if (staticvm.SearchItemId != null)
            {
                datalist = datalist.Where(o => o.itemid == staticvm.SearchItemId).ToList();
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
        [HttpGet]
        public async Task<IActionResult> GetAllItems()
        {

            var datalist = (from z in await _unitOfWork.Items.GetAllAsync(includeProperties: "Category,Room")
                            select new
                            {
                                id = z.Id,
                                name = z.Name,
                                category = z.Category.Name,
                                location = _unitOfWork.Location.Get(z.Room.IDLocation).Name,
                                room = z.Room.Name,
                                roomid = z.Room.Id,
                                name_of_room_and_location_ = z.Room.Name + " (" + _unitOfWork.Location.Get(z.Room.IDLocation).Name + ")"
                            }).ToList();

            return Json(new { data = datalist });
        }

        #region Export Data Excel
        public async Task<FileResult> Export(string code)
        {

            var datalist = (from z in await _unitOfWork.ItemService.GetAllAsync(includeProperties: "Items")
                            select new
                            {
                                id = z.Id,
                                itemid = z.ItemId,
                                code = z.Items.Code,
                                name = z.Items.Name,
                                description = z.RepairDescription,
                                servicedate = z.ServiceDate != null ? z.ServiceDate.Value.ToString("dd/MM/yyyy") : "",
                                serviceenddate = z.ServiceEndDate != null ? z.ServiceEndDate.Value.ToString("dd/MM/yyyy") : "",
                                requestby = z.RequestBy,
                                cost = z.CostOfRepair.HasValue ? z.CostOfRepair.Value.ToString("#,##0") : "",
                                status = z.Status,
                                statusid = z.StatusId,
                                month = z.ServiceDate != null ? z.ServiceDate.Value.Month : 0,
                                year = z.ServiceDate != null ? z.ServiceDate.Value.Year : 0,
                                technician = z.Technician,
                                phone = z.PhoneNumber,
                                address = z.Address
                            }).ToList();

            if (staticvm.SearchItemId != null)
            {
                datalist = datalist.Where(o => o.itemid == staticvm.SearchItemId).ToList();
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
                             select new ExportReportServiceItem
                             {
                                 Number = Number++,
                                 Code = z.code,
                                 Name = z.name,
                                 Description = z.description,
                                 ServiceDate = z.servicedate,
                                 ServiceEndDate = z.serviceenddate,
                                 Technician = z.technician,
                                 PhoneNumber = z.phone,
                                 Address = z.address,
                                 RequestBy = z.requestby,
                                 CostOfRepair = z.cost
                             }).ToList();


            var file = CreateFile(datalist_);
            return File(file, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Export_Data_ItemService.xlsx");


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
            cell.SetCellValue("Kode");
            cell.CellStyle = style;
            cell = rowHeader.CreateCell(2);
            cell.SetCellValue("Nama Aset");
            cell.CellStyle = style;
            cell = rowHeader.CreateCell(3);
            cell.SetCellValue("Keterangan");
            cell.CellStyle = style;
            cell = rowHeader.CreateCell(4);
            cell.SetCellValue("Tanggal Servis");
            cell.CellStyle = style;
            cell = rowHeader.CreateCell(5);
            cell.SetCellValue("Tanggal Selesai");
            cell.CellStyle = style;
            cell = rowHeader.CreateCell(6);
            cell.SetCellValue("Teknisi");
            cell.CellStyle = style;
            cell = rowHeader.CreateCell(7);
            cell.SetCellValue("No.Telepon");
            cell.CellStyle = style;
            cell = rowHeader.CreateCell(8);
            cell.SetCellValue("Alamat");
            cell.CellStyle = style;
            cell = rowHeader.CreateCell(9);
            cell.SetCellValue("Permintaan");
            cell.CellStyle = style;
            cell = rowHeader.CreateCell(10);
            cell.SetCellValue("Biaya Servis");
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

            var datalist = (from z in await _unitOfWork.ItemService.GetAllAsync(includeProperties: "Items")
                            select new
                            {
                                id = z.Id,
                                itemid = z.ItemId,
                                code = z.Items.Code,
                                name = z.Items.Name,
                                description = z.RepairDescription,
                                servicedate = z.ServiceDate != null ? z.ServiceDate.Value.ToString("dd/MM/yyyy") : "",
                                serviceenddate = z.ServiceEndDate != null ? z.ServiceEndDate.Value.ToString("dd/MM/yyyy") : "",
                                requestby = z.RequestBy,
                                cost = z.CostOfRepair.HasValue ? z.CostOfRepair.Value.ToString("#,##0") : "",
                                status = z.Status,
                                statusid = z.StatusId,
                                month = z.ServiceDate != null ? z.ServiceDate.Value.Month : 0,
                                year = z.ServiceDate != null ? z.ServiceDate.Value.Year : 0,
                                technician = z.Technician,
                                phone = z.PhoneNumber,
                                address = z.Address
                            }).ToList();

            if (staticvm.SearchItemId != null)
            {
                datalist = datalist.Where(o => o.itemid == staticvm.SearchItemId).ToList();
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
                                 Code = z.code,
                                 Name = z.name,
                                 Description = z.description,
                                 ServiceDate = z.servicedate,
                                 ServiceEndDate = z.serviceenddate,
                                 Technician = z.technician,
                                 PhoneNumber = z.phone,
                                 Address = z.address,
                                 RequestBy = z.requestby,
                                 CostOfRepair = z.cost 
                             }).ToList();

            DataTable dttbl = CreateDataTable(datalist_);
            string physicalPath = "wwwroot\\images\\products\\DaftarServisAset.pdf";
            ExportDataTableToPdf(dttbl, physicalPath, "Daftar Servis Aset");


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
            PdfPTable table = new PdfPTable(11);
            table.HorizontalAlignment = 0;
            table.TotalWidth = 520f;
            table.LockedWidth = true;
            float[] widths = new float[] { 20f, 30f, 80f, 50f, 30f, 30f, 50f, 50f, 60f, 40f, 40f };
            table.SetWidths(widths);
            //Table header
            BaseFont btnColumnHeader = BaseFont.CreateFont(BaseFont.HELVETICA_BOLD, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
            iTextSharp.text.Font fntColumnHeader = new iTextSharp.text.Font(btnColumnHeader, 8, 1, BaseColor.WHITE);

            addCell(table, "No", 1);
            addCell(table, "Kode", 1);
            addCell(table, "Nama Aset", 1);
            addCell(table, "Keterangan", 1);
            addCell(table, "Tanggal Servis", 1);
            addCell(table, "Tanggal Selesai", 1);
            addCell(table, "Teknisi", 1);
            addCell(table, "No. Telepon", 1);
            addCell(table, "Alamat", 1);
            addCell(table, "Permintaan", 1);
            addCell(table, "Biaya Servis", 1);

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
