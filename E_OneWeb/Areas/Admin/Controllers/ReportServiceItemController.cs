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
    public class ReportServiceItemController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public ReportServiceItemController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<IActionResult> Index(ItemsVM vm)
        {
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
                                category = z.Category.Name,
                                room = z.Room.Name,
                                qty = z.Qty,
                                price = z.Price != null ? z.Price : 0,
                                totalamount = z.TotalAmount != null ? z.TotalAmount : 0
                            }).ToList();

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
            cell.SetCellValue("CATEGORY");
            cell.CellStyle = style;
            cell = rowHeader.CreateCell(5);
            cell.SetCellValue("ROOM NAME");
            cell.CellStyle = style;
            cell = rowHeader.CreateCell(6);
            cell.SetCellValue("QTY");
            cell.CellStyle = style;
            cell = rowHeader.CreateCell(7);
            cell.SetCellValue("PRICE");
            cell.CellStyle = style;
            cell = rowHeader.CreateCell(8);
            cell.SetCellValue("TOTAL AMOUNT");
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
        [HttpGet("Export")]
        public async Task<FileResult> Export(string code)
        {

            var products = (from z in await _unitOfWork.Items.GetAllAsync(includeProperties: "Category,Room")
                            select new
                            {
                                number = z.Id,
                                code = z.Code,
                                name = z.Name,
                                description = z.Description,
                                category = z.Category.Name,
                                room = z.Room.Name,
                                qty = z.Qty,
                                price = z.Price != null ? z.Price : 0,
                                totalamount = z.TotalAmount != null ? z.TotalAmount : 0

                            }).ToList();

            if (code != null)
            {
                products = products.Where(z => z.code == code).ToList();
            }
            int Number = 1;
            products = (from z in products
                        select new
                        {
                            number = Number++,
                            code = z.code,
                            name = z.name,
                            description = z.description,
                            category = z.category,
                            room = z.room,
                            qty = z.qty,
                            price = z.price != null ? z.price : 0,
                            totalamount = z.totalamount != null ? z.totalamount : 0

                        }).ToList();


            var file = CreateFile(products);
            return File(file, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Export_data_item.xlsx");


        }
    }
}
