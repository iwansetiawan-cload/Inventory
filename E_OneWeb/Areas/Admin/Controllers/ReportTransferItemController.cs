using E_OneWeb.DataAccess.Repository.IRepository;
using E_OneWeb.Models.ViewModels;
using E_OneWeb.Models;
using E_OneWeb.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NPOI.XSSF.UserModel;
using sun.misc;
using static com.sun.tools.@internal.xjc.reader.xmlschema.bindinfo.BIConversion;
using System.Data;
using iTextSharp.text.pdf;
using iTextSharp.text;

namespace E_OneWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
    public class ReportTransferItemController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private static ReportTransferItemVM staticvm = new ReportTransferItemVM();
        public ReportTransferItemController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<IActionResult> Index(ReportTransferItemVM vm)
        {          
            staticvm = vm;
            vm.MonthList = MonthItems.Where(x => x != "").Select((m, i) => new SelectListItem
            {
                Value = (i + 1).ToString(),
                Text = m
            }).ToList();
            ViewBag.SearchMonthList = new SelectList(vm.MonthList, "Value", "Text");
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
           
            var datalist = (from z in await _unitOfWork.ItemTransfer.GetAllAsync(includeProperties: "Items")
                            select new
                            {
                                id = z.Id,
                                itemid = z.ItemId,
                                code = z.Items.Code,
                                name = z.Items.Name,
                                description = z.Description,
                                previousroom = z.PreviousRoom + " (" + z.PreviousLocation + ")",
                                currentroom = z.CurrentRoom + " (" + z.CurrentLocation + ")",
                                transferdate = z.TransferDate != null ? z.TransferDate.Value.ToString("dd/MM/yyyy") : "",
                                month = z.TransferDate != null ? z.TransferDate.Value.Month : 0,
                                year = z.TransferDate != null ?  z.TransferDate.Value.Year : 0,
                                currentroomid = z.CurrentLocationId
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
            if (staticvm.SearchRoomId != null)
            {
                datalist = datalist.Where(o => o.currentroomid == staticvm.SearchRoomId).ToList();
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
        [HttpGet]
        public IActionResult GetAllRoomAndLocation()
        {
            var datalist = (from z in _unitOfWork.Room.GetAll(includeProperties: "Location")
                            select new
                            {
                                id = z.Id,
                                name_of_room = z.Name,
                                description = z.Description,
                                name_of_location = z.Location.Name,
                                name_of_room_and_location = z.Name + " (" + z.Location.Name + ")"
                            }).ToList().OrderByDescending(o => o.id);
            return Json(new { data = datalist });
        }

        #region Export Data Excel
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
            cell.SetCellValue("Tanggal Transfer");
            cell.CellStyle = style;
            cell = rowHeader.CreateCell(5);
            cell.SetCellValue("Dari Lokasi");
            cell.CellStyle = style;
            cell = rowHeader.CreateCell(6);
            cell.SetCellValue("Ke Lokasi");
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

        public async Task<FileResult> Export(string code)
        {

            var datalist = (from z in await _unitOfWork.ItemTransfer.GetAllAsync(includeProperties: "Items")
                            select new
                            {
                                id = z.Id,
                                itemid = z.ItemId,
                                code = z.Items.Code,
                                name = z.Items.Name,
                                description = z.Description,
                                previousroom = z.PreviousRoom + " (" + z.PreviousLocation + ")",
                                currentroom = z.CurrentRoom + " (" + z.CurrentLocation + ")",
                                transferdate = z.TransferDate != null ? z.TransferDate.Value.ToString("dd/MM/yyyy") : "",
                                month = z.TransferDate != null ? z.TransferDate.Value.Month : 0,
                                year = z.TransferDate != null ? z.TransferDate.Value.Year : 0,
                                currentroomid = z.CurrentLocationId
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
            if (staticvm.SearchRoomId != null)
            {
                datalist = datalist.Where(o => o.currentroomid == staticvm.SearchRoomId).ToList();
            }
            int Number = 1;
            var datalist_ = (from z in datalist
                             select new ExportReportTransferItem
                             {
                                 Number = Number++,
                                 Code = z.code,
                                 Name = z.name,
                                 Description = z.description,
                                 TransferDate = z.transferdate,
                                 PreviousLocation = z.previousroom,
                                 CurrentLocation = z.currentroom,

                             }).ToList();


            var file = CreateFile(datalist_);
            return File(file, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Export_Data_ItemTransfer.xlsx");


        }
        #endregion

        #region Export Data PDF
        public async Task<FileResult> ExportPDF()
        {

            var datalist = (from z in await _unitOfWork.ItemTransfer.GetAllAsync(includeProperties: "Items")
                            select new
                            {
                                id = z.Id,
                                itemid = z.ItemId,
                                code = z.Items.Code,
                                name = z.Items.Name,
                                description = z.Description,
                                previousroom = z.PreviousRoom + " (" + z.PreviousLocation + ")",
                                currentroom = z.CurrentRoom + " (" + z.CurrentLocation + ")",
                                transferdate = z.TransferDate != null ? z.TransferDate.Value.ToString("dd/MM/yyyy") : "",
                                month = z.TransferDate != null ? z.TransferDate.Value.Month : 0,
                                year = z.TransferDate != null ? z.TransferDate.Value.Year : 0,
                                currentroomid = z.CurrentLocationId
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
            if (staticvm.SearchRoomId != null)
            {
                datalist = datalist.Where(o => o.currentroomid == staticvm.SearchRoomId).ToList();
            }

            int Number = 1;        
            var datalist_ = (from z in datalist
                             select new 
                             {
                                 Number = Number++,
                                 Code = z.code,
                                 Name = z.name,
                                 Description = z.description,
                                 TransferDate = z.transferdate,
                                 PreviousLocation = z.previousroom,
                                 CurrentLocation = z.currentroom,

                             }).ToList();

            DataTable dttbl = CreateDataTable(datalist_);
            string physicalPath = "wwwroot\\images\\products\\DaftarPemindahanAset.pdf";
            ExportDataTableToPdf(dttbl, physicalPath, "Daftar Pemindahan Aset");


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
            PdfPTable table = new PdfPTable(7);
            table.HorizontalAlignment = 0;
            table.TotalWidth = 520f;
            table.LockedWidth = true;
            float[] widths = new float[] { 20f, 30f, 80f, 50f, 30f, 60f, 60f };
            table.SetWidths(widths);
            //Table header
            BaseFont btnColumnHeader = BaseFont.CreateFont(BaseFont.HELVETICA_BOLD, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
            iTextSharp.text.Font fntColumnHeader = new iTextSharp.text.Font(btnColumnHeader, 8, 1, BaseColor.WHITE);          

            addCell(table, "No", 1);
            addCell(table, "Kode", 1);
            addCell(table, "Nama Aset", 1);
            addCell(table, "Keterangan", 1);
            addCell(table, "Tanggal Transfer", 1);
            addCell(table, "Dari Lokasi", 1);
            addCell(table, "Ke Lokasi", 1);
           
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
