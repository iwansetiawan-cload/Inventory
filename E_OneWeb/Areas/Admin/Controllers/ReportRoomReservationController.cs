using E_OneWeb.DataAccess.Repository.IRepository;
using E_OneWeb.Models.ViewModels;
using E_OneWeb.Models;
using E_OneWeb.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NPOI.XSSF.UserModel;
using iTextSharp.text.pdf;
using iTextSharp.text;
using System.Data;

namespace E_OneWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
    public class ReportRoomReservationController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private static ReportRoomReservationVM staticvm = new ReportRoomReservationVM();
        public ReportRoomReservationController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<IActionResult> Index(ReportRoomReservationVM vm)
        {
            DateTime now = DateTime.Now;
            DateTime dtnow = new DateTime(now.Year, now.Month, 1);
            vm.SearchStartDate = vm.SearchStartDate == null ? dtnow : vm.SearchStartDate;
            vm.SearchEndDate = vm.SearchEndDate == null ? DateTime.Now : vm.SearchEndDate;
            staticvm = vm;
            return View(vm);
        }     
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {

            var datalist = (from z in await _unitOfWork.RoomReservationUser.GetAllAsync(includeProperties: "RoomReservationAdmin")
                            select new
                            {
                                id = z.Id,
                                room = z.RoomReservationAdmin.RoomName + " (" + z.RoomReservationAdmin.LocationName + ")",
                                startdate = z.StartDate.Value.ToString("dd/MM/yyyy HH:mm"),
                                enddate = z.EndDate.Value.ToString("dd/MM/yyyy HH:mm"),
                                startdate_ = z.StartDate,
                                enddate_ = z.EndDate,
                                description = z.Description,
                                entryby = z.EntryBy,
                                roomid = z.RoomReservationAdmin.RoomId
                            }).ToList();

            if (staticvm.SearchStartDate != null && staticvm.SearchEndDate != null)
            {
                datalist = datalist.Where(o => o.startdate_ >= staticvm.SearchStartDate && o.enddate_ <= staticvm.SearchEndDate).ToList();
            }
            if (staticvm.SearchRoomId != null)
            {
                datalist = datalist.Where(o => o.roomid == staticvm.SearchRoomId).ToList();
            }

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
        public async Task<FileResult> Export()
        {


            var datalist = (from z in await _unitOfWork.RoomReservationUser.GetAllAsync(includeProperties: "RoomReservationAdmin")
                            select new
                            {
                                id = z.Id,
                                room = z.RoomReservationAdmin.RoomName + " (" + z.RoomReservationAdmin.LocationName + ")",
                                startdate = z.StartDate.Value.ToString("dd/MM/yyyy HH:mm"),
                                enddate = z.EndDate.Value.ToString("dd/MM/yyyy HH:mm"),
                                startdate_ = z.StartDate,
                                enddate_ = z.EndDate,
                                description = z.Description,
                                entryby = z.EntryBy,
                                study = z.Study,
                                teacher = z.Dosen,
                                roomid = z.RoomReservationAdmin.RoomId
                            }).ToList();

            if (staticvm.SearchStartDate != null && staticvm.SearchEndDate != null)
            {
                datalist = datalist.Where(o => o.startdate_ >= staticvm.SearchStartDate && o.enddate_ <= staticvm.SearchEndDate).ToList();
            }
            if (staticvm.SearchRoomId != null)
            {
                datalist = datalist.Where(o => o.roomid == staticvm.SearchRoomId).ToList();
            }

            int Number = 1;
            var datalist_ = (from z in datalist
                             select new ExportReportRoomReservation
                             {
                                 Number = Number++,
                                 RoomName = z.room,
                                 StartDate = z.startdate,
                                 EndDate = z.enddate,
                                 Description = z.description,
                                 EntryBy = z.entryby,
                                 Study = z.study,
                                 Teacher = z.teacher
                             }).ToList();
            var file = CreateFile(datalist_);
            return File(file, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Data_Peminjaman_Ruangan.xlsx");

        }
        public static byte[] CreateFile<T>(List<T> source)
        {
            var workbook = new XSSFWorkbook();
            var sheet = workbook.CreateSheet("Sheet1");
            var rowHeader = sheet.CreateRow(2);

            var properties = typeof(T).GetProperties();

            //header
            var font = workbook.CreateFont();
            font.IsBold = true;
            var style = workbook.CreateCellStyle();
            style.SetFont(font);

            var rowHeaderName = sheet.CreateRow(0);
            var cell_ = rowHeaderName.CreateCell(0);
            cell_.SetCellValue("DATA PEMINJAMAN RUANGAN");
            cell_.CellStyle = style;

            var cell = rowHeader.CreateCell(0);
            cell.SetCellValue("No");
            cell.CellStyle = style;

            cell = rowHeader.CreateCell(1);
            cell.SetCellValue("Nama Ruangan");
            cell.CellStyle = style;

            cell = rowHeader.CreateCell(2);
            cell.SetCellValue("Tanggal Mulai Pinjam");
            cell.CellStyle = style;

            cell = rowHeader.CreateCell(3);
            cell.SetCellValue("Tanggal Selesai Pinjam");
            cell.CellStyle = style;

            cell = rowHeader.CreateCell(4);
            cell.SetCellValue("Keterangan");
            cell.CellStyle = style;

            cell = rowHeader.CreateCell(5);
            cell.SetCellValue("Peminjam");
            cell.CellStyle = style;

            cell = rowHeader.CreateCell(6);
            cell.SetCellValue("Mata Kuliah");
            cell.CellStyle = style;

            cell = rowHeader.CreateCell(7);
            cell.SetCellValue("Dosen");
            cell.CellStyle = style;

            //end header

            //content
            var rowNum = 3;
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

            var datalist = (from z in await _unitOfWork.RoomReservationUser.GetAllAsync(includeProperties: "RoomReservationAdmin")
                            select new
                            {
                                id = z.Id,
                                room = z.RoomReservationAdmin.RoomName + " (" + z.RoomReservationAdmin.LocationName + ")",
                                startdate = z.StartDate.Value.ToString("dd/MM/yyyy HH:mm"),
                                enddate = z.EndDate.Value.ToString("dd/MM/yyyy HH:mm"),
                                startdate_ = z.StartDate,
                                enddate_ = z.EndDate,
                                description = z.Description,
                                entryby = z.EntryBy,
                                study = z.Study,
                                teacher = z.Dosen,
                                roomid = z.RoomReservationAdmin.RoomId
                            }).ToList();

            if (staticvm.SearchStartDate != null && staticvm.SearchEndDate != null)
            {
                datalist = datalist.Where(o => o.startdate_ >= staticvm.SearchStartDate && o.enddate_ <= staticvm.SearchEndDate).ToList();
            }
            if (staticvm.SearchRoomId != null)
            {
                datalist = datalist.Where(o => o.roomid == staticvm.SearchRoomId).ToList();
            }

            int Number = 1;
            var datalist_ = (from z in datalist
                             select new 
                             {
                                 Number = Number++,
                                 RoomName = z.room,
                                 StartDate = z.startdate,
                                 EndDate = z.enddate,
                                 Description = z.description,
                                 EntryBy = z.entryby,
                                 Study = z.study,
                                 Teacher = z.teacher
                             }).ToList();

            DataTable dttbl = CreateDataTable(datalist_);
            string physicalPath = "wwwroot\\images\\products\\DaftarPeminjamanRuangan.pdf";
            ExportDataTableToPdf(dttbl, physicalPath, "Daftar Peminjaman Ruangan");


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
            PdfPTable table = new PdfPTable(8);
            table.HorizontalAlignment = 0;
            table.TotalWidth = 520f;
            table.LockedWidth = true;
            float[] widths = new float[] { 20f, 80f, 30f, 30f, 50f, 50f, 50f, 50f};
            table.SetWidths(widths);
            //Table header
            BaseFont btnColumnHeader = BaseFont.CreateFont(BaseFont.HELVETICA_BOLD, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
            iTextSharp.text.Font fntColumnHeader = new iTextSharp.text.Font(btnColumnHeader, 8, 1, BaseColor.WHITE);

            addCell(table, "No", 1);
            addCell(table, "Nama Ruangan", 1);
            addCell(table, "Tanggal Pinjam", 1);
            addCell(table, "Tanggal Selesai", 1);
            addCell(table, "Keterangan", 1);
            addCell(table, "Peminjam", 1);
            addCell(table, "Mata Kuliah", 1);
            addCell(table, "Dosen", 1);

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
