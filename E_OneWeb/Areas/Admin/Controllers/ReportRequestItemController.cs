using E_OneWeb.DataAccess.Repository.IRepository;
using E_OneWeb.Models.ViewModels;
using E_OneWeb.Models;
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
              
        public async Task<FileResult> Export(string code)
        {
            var requestheader = await _unitOfWork.RequestItemHeader.GetAllAsync();

            var datalist = (from z in await _unitOfWork.RequestItemDetail.GetAllAsync()
                            select new
                            {
                                id = z.Id,
                                refnumber = requestheader.Where(x=>x.Id == z.IdHeader).Select(i=>i.ReqNumber).FirstOrDefault(),
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
            return File(file, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Export_Data_ItemRequest.xlsx");


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
            cell.SetCellValue("REF NUMBER");
            cell.CellStyle = style;
            cell = rowHeader.CreateCell(2);
            cell.SetCellValue("REQUEST DATE");
            cell.CellStyle = style;
            cell = rowHeader.CreateCell(3);
            cell.SetCellValue("REQUESTER");
            cell.CellStyle = style;
            cell = rowHeader.CreateCell(4);
            cell.SetCellValue("ITEM NAME");
            cell.CellStyle = style;
            cell = rowHeader.CreateCell(5);
            cell.SetCellValue("REASON");
            cell.CellStyle = style;
            cell = rowHeader.CreateCell(6);
            cell.SetCellValue("SPECIFICATION");
            cell.CellStyle = style;
            cell = rowHeader.CreateCell(7);
            cell.SetCellValue("PRICE");
            cell.CellStyle = style;
            cell = rowHeader.CreateCell(8);
            cell.SetCellValue("QTY");
            cell.CellStyle = style;
            cell = rowHeader.CreateCell(9);
            cell.SetCellValue("TOTAL");
            cell.CellStyle = style;
            cell = rowHeader.CreateCell(10);
            cell.SetCellValue("STATUS");
            cell.CellStyle = style;
            cell = rowHeader.CreateCell(11);
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
    }
}
