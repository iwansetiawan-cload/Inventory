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

namespace E_OneWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
    public class ReportRoomAndLocationController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private static ReportRoomAndLocationVM staticvm = new ReportRoomAndLocationVM();
        public ReportRoomAndLocationController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index(ReportRoomAndLocationVM vm)
        {
            staticvm = vm;
            return View(vm);

        }
        [HttpGet]
        public IActionResult GetAll()
        {

            var datalist = (from z in  _unitOfWork.Room.GetAll(includeProperties: "Location")
                            select new
                            {
                                id = z.Id,
                                name = z.Name,
                                description = z.Description,
                                location = z.Location.Name,
                                room_and_location = z.Name + " (" + z.Location.Name + ")",
                                locationid = z.Location.Id
                            }).ToList();

            if (staticvm.SearchLocationId != null)
            {
                datalist = datalist.Where(o => o.locationid == staticvm.SearchLocationId).ToList();
            }      

            return Json(new { data = datalist });
        }

        [HttpGet]
        public IActionResult GetAllLocation()
        {
            var datalist = (from z in _unitOfWork.Location.GetAll()
                            select new
                            {
                                id = z.Id,
                                name = z.Name,
                                description = z.Description
                            }).ToList();
            return Json(new { data = datalist });
        }
        public async Task<FileResult> Export(string code)
        {

            var datalist = (from z in _unitOfWork.Room.GetAll(includeProperties: "Location")
                            select new
                            {
                                id = z.Id,
                                name = z.Name,
                                description = z.Description,
                                location = z.Location.Name,
                                room_and_location = z.Name + " (" + z.Location.Name + ")",
                                locationid = z.Location.Id
                            }).ToList();

            if (staticvm.SearchLocationId != null)
            {
                datalist = datalist.Where(o => o.locationid == staticvm.SearchLocationId).ToList();
            }

            int Number = 1;
            var datalist_ = (from z in datalist
                             select new ExportReportRoomAndLocation
                             {
                                 Number = Number++,
                                 RoomName = z.name,
                                 Description = z.description,
                                 LocationName = z.location
                             }).ToList();


            var file = CreateFile(datalist_);
            return File(file, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Export_Data_Ruangan.xlsx");


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
            cell_.SetCellValue("DAFTAR GEDUNG DAN RUANGAN");
            cell_.CellStyle = style;

            var cell = rowHeader.CreateCell(0);
            cell.SetCellValue("No");
            cell.CellStyle = style;

            cell = rowHeader.CreateCell(1);
            cell.SetCellValue("Nama Ruangan");
            cell.CellStyle = style;

            cell = rowHeader.CreateCell(2);
            cell.SetCellValue("keterangan");
            cell.CellStyle = style;

            cell = rowHeader.CreateCell(3);
            cell.SetCellValue("Lokasi/ Gedung");
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
