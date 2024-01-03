using E_OneWeb.DataAccess.Repository.IRepository;
using E_OneWeb.Models;
using E_OneWeb.Models.ViewModels;
using E_OneWeb.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.SqlServer.Server;
using NPOI.HSSF.Record;
using NPOI.SS.Formula.Functions;
using NPOI.XSSF.UserModel;
using NPOI.XWPF.UserModel;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using SixLabors.ImageSharp;
using sun.misc;
using System.Data;
using System.Diagnostics;

namespace E_OneWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
    public class ReportItemsController : Controller
    {       
        private readonly IUnitOfWork _unitOfWork;
        private static ReportItemVM staticvm = new ReportItemVM();
        public ReportItemsController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<IActionResult> Index(ReportItemVM vm, string code, string ownership, int? category, int? status)
        {
            staticvm.SearchCode = code;
            staticvm.SearchOwnership = ownership;
            staticvm.SearchCategory = category;
            staticvm.SearchStatus = status;
            vm = staticvm;
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

        //[HttpGet]
        public async Task<IActionResult> GetAll()
        {

            ReportItemVM vm = staticvm;
            var datalist = (from z in await _unitOfWork.Items.GetAllAsync(includeProperties: "Category,Room")
                            select new
                            {
                                id = z.Id,
                                code = z.Code,
                                name = z.Name,
                                description = z.Description,
                                categoryid = z.CategoryId,
                                category = z.Category.Name,
                                room = z.Room.Name,
                                qty = z.Qty,
                                ownership = z.OriginOfGoods,
                                price = z.Price.HasValue ? z.Price.Value.ToString("#,##0") : "",
                                totalamount = z.TotalAmount.HasValue ? z.TotalAmount.Value.ToString("#,##0") : "",
                                status = z.Status
                            }).ToList();
            if (vm.SearchCode != null)
            {
                datalist = datalist.Where(o => o.code == vm.SearchCode).ToList();
            }
            if (vm.SearchOwnership != null)
            {
                datalist = datalist.Where(o => o.ownership == vm.SearchOwnership).ToList();
            }
            if (vm.SearchCategory != null)
            {
                datalist = datalist.Where(o => o.categoryid == vm.SearchCategory).ToList();
            }
            if (vm.SearchStatus != null)
            {
                datalist = datalist.Where(o => o.status == vm.SearchStatus).ToList();
            }

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

            //var colIndex = 0;
            //foreach (var property in properties)
            //{
            //    var cell = rowHeader.CreateCell(colIndex);
            //    cell.SetCellValue(property.Name);
            //    cell.CellStyle = style;
            //    colIndex++;
            //}

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
            cell.SetCellValue("LOCATION");
            cell.CellStyle = style;

            cell = rowHeader.CreateCell(6);
            cell.SetCellValue("ROOM");
            cell.CellStyle = style;

            cell = rowHeader.CreateCell(7);
            cell.SetCellValue("PRICE");
            cell.CellStyle = style;

            cell = rowHeader.CreateCell(8);
            cell.SetCellValue("QTY");
            cell.CellStyle = style;

            cell = rowHeader.CreateCell(9);
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
        
        public async Task<FileResult> Export()
        {
            

            var datalist = (from z in await _unitOfWork.Items.GetAllAsync(includeProperties: "Category,Room")
                            select new
                            {
                                number = z.Id,
                                code = z.Code,
                                name = z.Name,
                                description = z.Description,
                                category = z.Category.Name,
                                location = _unitOfWork.Location.Get(z.Room.IDLocation).Name,
                                room = z.Room.Name,                               
                                qty = z.Qty,
                                price = z.Price != null ? z.Price : 0,
                                totalamount = z.TotalAmount != null ? z.TotalAmount : 0,
                                ownership = z.OriginOfGoods,
                                categoryid = z.CategoryId,
                                status = z.Status
                            }).ToList();

            if (staticvm.SearchCode != null)
            {
                datalist = datalist.Where(o => o.code == staticvm.SearchCode).ToList();
            }
            if (staticvm.SearchOwnership != null)
            {
                datalist = datalist.Where(o => o.ownership == staticvm.SearchOwnership).ToList();
            }
            if (staticvm.SearchCategory != null)
            {
                datalist = datalist.Where(o => o.categoryid == staticvm.SearchCategory).ToList();
            }
            if (staticvm.SearchStatus != null)
            {
                datalist = datalist.Where(o => o.status == staticvm.SearchStatus).ToList();
            }

            int Number = 1;
            var datalist_ = (from z in datalist
                        select new ExportReportItem
                        {
                            Number = Number++,
                            Code = z.code,
                            Name = z.name,
                            Description = z.description,
                            Category = z.category,
                            Location = z.location,
                            Room = z.room,                         
                            Price = z.price.HasValue ? z.price.Value.ToString("#,##0") : "",
                            Qty = z.qty,
                            TotalAmount = z.totalamount.HasValue ? z.totalamount.Value.ToString("#,##0") : ""
                        }).ToList();
         

            var file = CreateFile(datalist_);
            return File(file, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Export_data_item.xlsx");
            

        }
    }
}
