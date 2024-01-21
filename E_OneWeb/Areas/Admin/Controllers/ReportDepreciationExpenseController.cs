using E_OneWeb.DataAccess.Repository.IRepository;
using E_OneWeb.Models;
using E_OneWeb.Models.ViewModels;
using E_OneWeb.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NPOI.XSSF.UserModel;
using static com.sun.tools.@internal.xjc.reader.xmlschema.bindinfo.BIConversion;

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
            vm.SearchYear = vm.SearchYear == null ? DateTime.Now.Year : vm.SearchYear;
            vm.SearchMonth = vm.SearchMonth == null ? DateTime.Now.Month : vm.SearchMonth;
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
                                expenseamount = ((decimal)z.TotalAmount * z.Percent) / 100,
                                expensetotalamount = (decimal)z.TotalAmount - (((decimal)z.TotalAmount * z.Percent) / 100),
                                startdate_calculate = z.Period != null ? z.StartDate.Value.AddYears((int)z.Period) : z.StartDate,
                            }).ToList();

            if (staticvm.SearchStartDate != null && staticvm.SearchEndDate != null)
            {
                datalist = datalist.Where(o => o.startdate >= staticvm.SearchStartDate && o.startdate <= staticvm.SearchEndDate).ToList();
            }
            if (staticvm.SearchCategory != null)
            {
                datalist = datalist.Where(o => o.categoryid == staticvm.SearchCategory).ToList();
            }

            if (staticvm.SearchYear != null && staticvm.SearchMonth != null)
            {
                DateTime DtPeriode = new DateTime((int)staticvm.SearchYear, (int)staticvm.SearchMonth, 1);
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
                                     ExpenseAmount = z.startdate_calculate <= DtPeriode ? z.expenseamount.Value.ToString("#,##0") : "0",
                                     ExpenseTotalAmount = z.startdate_calculate <= DtPeriode ? z.expensetotalamount.Value.ToString("#,##0") : z.totalamount.Value.ToString("#,##0")
                                 }).ToList();


                var file = CreateFile(datalist_);
                return File(file, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Report_Depreciation_Expense.xlsx");
            }
            else
            {
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
                                     ExpenseAmount = "0",
                                     ExpenseTotalAmount = z.totalamount.Value.ToString("#,##0")
                                 }).ToList();


                var file = CreateFile(datalist_);
                return File(file, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Report_Depreciation_Expense.xlsx");
            }

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
