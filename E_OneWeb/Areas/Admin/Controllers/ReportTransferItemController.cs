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
            cell.SetCellValue("NO");
            cell.CellStyle = style;
            cell = rowHeader.CreateCell(1);
            cell.SetCellValue("CODE");
            cell.CellStyle = style;
            cell = rowHeader.CreateCell(2);
            cell.SetCellValue("ITEM NAME");
            cell.CellStyle = style;
            cell = rowHeader.CreateCell(3);
            cell.SetCellValue("DESCRIPTION");
            cell.CellStyle = style;
            cell = rowHeader.CreateCell(4);
            cell.SetCellValue("TRANSFER DATE");
            cell.CellStyle = style;
            cell = rowHeader.CreateCell(5);
            cell.SetCellValue("PREVIOUS LOCATION");
            cell.CellStyle = style;
            cell = rowHeader.CreateCell(6);
            cell.SetCellValue("CURRENT LOCATION");
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
    }
}
