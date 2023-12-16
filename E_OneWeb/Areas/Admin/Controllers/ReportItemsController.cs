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
using NPOI.XSSF.UserModel;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Data;
using System.Diagnostics;

namespace E_OneWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
    public class ReportItemsController : Controller
    {       
        private readonly IUnitOfWork _unitOfWork;
        public ReportItemsController(IUnitOfWork unitOfWork)
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

            var colIndex = 0;
            foreach (var property in properties)
            {
                var cell = rowHeader.CreateCell(colIndex);
                cell.SetCellValue(property.Name);
                cell.CellStyle = style;
                colIndex++;
            }
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

            if (code != null)
            {
                products = products.Where(z => z.code == code).ToList();
            }

            var file = CreateFile(products);
            return File(file, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Export_data.xlsx");
            //string con = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            //SqlCommand cmd = new SqlCommand();
            //DataTable dt = new DataTable();

            //#region Filter Data
            //string filter = "where TYPE_STATUS_DAPEMMASTER= '1' "; // export hanya untuk status dapemmaster yg active
            //if (!string.IsNullOrEmpty(AnnuityProduct))
            //{
            //    if (string.IsNullOrEmpty(filter))
            //    {
            //        filter = " where IDANNUITYTYPE = @AnnuityProduct ";
            //    }
            //    else
            //    {
            //        filter = filter + " and IDANNUITYTYPE = @AnnuityProduct ";
            //    }

            //    cmd.Parameters.AddWithValue("@AnnuityProduct", AnnuityProduct);
            //}

            //if (!string.IsNullOrEmpty(PaymentPlanDate))
            //{
            //    string[] splitdt = PaymentPlanDate.Split('/');
            //    string searchPaymentPlanDt = splitdt[1] + "/" + splitdt[0] + "/" + splitdt[2];
            //    if (string.IsNullOrEmpty(filter))
            //    {
            //        filter = " where PAYMENTPLAN = @PaymentPlanDate ";
            //    }
            //    else
            //    {
            //        filter = filter + " and PAYMENTPLAN = @PaymentPlanDate ";
            //    }

            //    cmd.Parameters.AddWithValue("@PaymentPlanDate", searchPaymentPlanDt);
            //}

            //if (!string.IsNullOrEmpty(MemberInstanceID))
            //{
            //    if (string.IsNullOrEmpty(filter))
            //    {
            //        filter = " where MEMBERINSTANCE_ID = @MemberInstanceID ";
            //    }
            //    else
            //    {
            //        filter = filter + " and MEMBERINSTANCE_ID = @MemberInstanceID ";
            //    }

            //    cmd.Parameters.AddWithValue("@MemberInstanceID", MemberInstanceID);
            //}
            //#endregion

            //using (SqlConnection conn = new SqlConnection(con))
            //{
            //    conn.Open();
            //    cmd.Connection = conn;
            //    cmd.CommandTimeout = 1600;
            //    SqlDataAdapter da = new SqlDataAdapter(cmd);

            //    cmd.CommandText = "select ROW_NUMBER() OVER (ORDER BY DAPEMID) AS NUMBER,* from VW_RETURN_DAPEMLIST " + filter + " order by BANKNAME asc";
            //    da.Fill(dt);

            //    var data = dt.AsEnumerable().Select(x => new 
            //    {
            //        NUMBER = x.Field<long>("NUMBER"),
            //        ID = x.Field<long>("DAPEMID"),
            //        AMOUNT_STRING = x.Field<double?>("AMOUNT").HasValue ? x.Field<double?>("AMOUNT").Value.ToString("#,##0") : "",
            //        AMOUNT = x.Field<double?>("AMOUNT").HasValue ? x.Field<double?>("AMOUNT").Value : 0,
            //        MEMBERINSTANCE_ID = x.Field<string>("MEMBERINSTANCE_ID"),
            //        FULLNAME = x.Field<string>("FULLNAME"),
            //        BENEFICIARY = x.Field<string>("FAMILYNAME"),
            //        ANNUITY_PRODUCT = x.Field<string>("ANNUITYTYPE"),
            //        BANK_ACCOUNT_NO = x.Field<string>("BANKACCOUNTNO"),
            //        BANK_ACCOUNT_NAME = x.Field<string>("BANKACCOUNTNAME"),
            //        IDBANK = x.Field<int?>("IDBANK"),
            //        IDBANKHEADER = x.Field<int?>("IDBANKHEADER"),
            //        BANK_BRANCH = x.Field<string>("BANK_BRANCH"),
            //        BANK_GROUP = x.Field<string>("BANK_GROUP"),
            //        BANKPAYMENT = x.Field<string>("BANKNAME"),
            //        STATUS = x.Field<string>("STATUS_VALIDATION"),
            //        DAPEMID = x.Field<long?>("DAPEMID"),
            //        PAYMENTPLANRETUR = x.Field<DateTime?>("PAYMENTPLANRETUR").HasValue ? x.Field<DateTime?>("PAYMENTPLANRETUR").Value.ToString("dd/MM/yyyy") : "",
            //        RETURNDATE = x.Field<DateTime?>("RETURNDATE").HasValue ? x.Field<DateTime?>("RETURNDATE").Value.ToString("dd/MM/yyyy") : "",
            //        PERIODDAPEM = x.Field<DateTime?>("DAPEMDATE").HasValue ? x.Field<DateTime?>("DAPEMDATE").Value.ToString("MMM yyyy") : "",
            //        POLICYNO = x.Field<string>("POLICYNO")
            //    }).ToList();

            //    conn.Close();
            //    ExcelPackage Exc = new ExcelPackage();
            //    ExcelWorksheet worksheet = Exc.Workbook.Worksheets.Add("Sheet 1");
            //    worksheet.Cells[1, 1].Value = "NO";
            //    worksheet.Cells[1, 2].Value = "MEMBER INSTANCE ID";
            //    worksheet.Cells[1, 3].Value = "POLICY NO";
            //    worksheet.Cells[1, 4].Value = "RETUR DATE";
            //    worksheet.Cells[1, 5].Value = "PERIOD";
            //    worksheet.Cells[1, 6].Value = "MEMBER NAMA";
            //    worksheet.Cells[1, 7].Value = "AMOUNT";
            //    worksheet.Cells[1, 8].Value = "BANK ACCOUNT NO";
            //    worksheet.Cells[1, 9].Value = "BANK ACCOUNT NAME";
            //    worksheet.Cells[1, 10].Value = "BANK NAME";
            //    worksheet.Cells[1, 11].Value = "BANK BRANCH";
            //    worksheet.Cells[1, 12].Value = "PAYMENT PLAN RETUR (dd/MM/yyyy)";
            //    worksheet.Cells[1, 13].Value = "DAPEM CODE";

            //    worksheet.Cells["A" + 1 + ":M" + 1].Style.Font.Bold = true;
            //    worksheet.Cells[1, 8].Style.Fill.PatternType = ExcelFillStyle.Solid;
            //    worksheet.Cells[1, 8].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Yellow);
            //    worksheet.Cells[1, 9].Style.Fill.PatternType = ExcelFillStyle.Solid;
            //    worksheet.Cells[1, 9].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Yellow);
            //    worksheet.Cells[1, 10].Style.Fill.PatternType = ExcelFillStyle.Solid;
            //    worksheet.Cells[1, 10].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Yellow);
            //    worksheet.Cells[1, 11].Style.Fill.PatternType = ExcelFillStyle.Solid;
            //    worksheet.Cells[1, 11].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Yellow);
            //    worksheet.Cells[1, 12].Style.Fill.PatternType = ExcelFillStyle.Solid;
            //    worksheet.Cells[1, 12].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Yellow);

            //    /* content row */
            //    int number = 1;
            //    int detailRow = 2;
            //    foreach (var item in data)
            //    {

            //        worksheet.Cells["A" + detailRow.ToString()].Value = number;
            //        worksheet.Cells["B" + detailRow.ToString()].Value = item.MEMBERINSTANCE_ID;
            //        worksheet.Cells["C" + detailRow.ToString()].Value = item.POLICYNO;
            //        worksheet.Cells["D" + detailRow.ToString()].Value = item.RETURNDATE;
            //        worksheet.Cells["E" + detailRow.ToString()].Value = item.PERIODDAPEM;
            //        worksheet.Cells["F" + detailRow.ToString()].Value = item.FULLNAME;
            //        worksheet.Cells["G" + detailRow.ToString()].Value = item.AMOUNT;
            //        worksheet.Cells["H" + detailRow.ToString()].Value = item.BANK_ACCOUNT_NO;
            //        worksheet.Cells["I" + detailRow.ToString()].Value = item.BANK_ACCOUNT_NAME;
            //        worksheet.Cells["J" + detailRow.ToString()].Value = item.BANKPAYMENT;
            //        worksheet.Cells["K" + detailRow.ToString()].Value = item.BANK_BRANCH;
            //        worksheet.Cells["L" + detailRow.ToString()].Value = item.PAYMENTPLANRETUR;
            //        worksheet.Cells["M" + detailRow.ToString()].Value = item.DAPEMID;

            //        detailRow++;
            //        number++;

            //    }
            //    worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();



            //    byte[] bin = Exc.GetAsByteArray();
            //    Response.Buffer = true;
            //    Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            //    Response.AddHeader("content-length", bin.Length.ToString());
            //    Response.AddHeader("content-disposition", "attachment; filename=\"ExportDataReturDapemlist_" + DateTime.Now.ToString("yyyyMMdd:hh.mm") + ".xlsx\"");
            //    Response.OutputStream.Write(bin, 0, bin.Length);
            //    Response.Flush();
            //    Response.SuppressContent = true;
            //}

            //return View();

        }
    }
}
