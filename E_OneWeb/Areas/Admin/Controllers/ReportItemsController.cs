using E_OneWeb.DataAccess.Repository.IRepository;
using E_OneWeb.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Data;

namespace E_OneWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class ReportItemsController : Controller
    {       
        private readonly IUnitOfWork _unitOfWork;
        public ReportItemsController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            return View();
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
                                price = z.Price != null ? z.Price : 0,
                                totalamount = z.TotalAmount != null ? z.TotalAmount : 0,
                                room = z.Room.Name,
                                category = z.Category.Name
                            }).ToList();

            return Json(new { data = datalist });
        }
        [HttpGet]
        public ActionResult Export(string code)
        {

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

            return View();

        }
    }
}
