using E_OneWeb.DataAccess.Repository.IRepository;
using E_OneWeb.Models.ViewModels;
using E_OneWeb.Models;
using E_OneWeb.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NPOI.XSSF.UserModel;

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
    }
}
