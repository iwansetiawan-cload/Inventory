using E_OneWeb.DataAccess.Repository.IRepository;
using E_OneWeb.Models.ViewModels;
using E_OneWeb.Models;
using E_OneWeb.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data;
using System.Drawing;
using Microsoft.Extensions.Hosting;
using System.Net;
using System.Runtime.CompilerServices;
using System.IO;
using System.Security.Policy;
using OfficeOpenXml;

namespace E_OneWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class RequestItemsController : Controller
    {
        public static List<RequestItemDetail> additemlist = new List<RequestItemDetail>();
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _hostEnvironment;    
        //public static string FlagDeleteItem { get; set; }
        public RequestItemsController(IUnitOfWork unitOfWork, IWebHostEnvironment hostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _hostEnvironment = hostEnvironment;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {

            var datalist = (from z in await _unitOfWork.RequestItemHeader.GetAllAsync()
                            select new
                            {
                                id = z.Id,
                                refnumber = z.ReqNumber,
                                requestdate = Convert.ToDateTime(z.RequestDate).ToString("dd-MM-yyyy"),
                                requestby = z.Requester,
                                desc = z.Description,
                                totalamount = z.TotalAmount.HasValue ? z.TotalAmount.Value.ToString("#,##0") : ""

                            }).OrderByDescending(i => i.id).ToList();

            return Json(new { data = datalist });
        }
        public async Task<IActionResult> Upsert(int? id)
        {           

            additemlist = new List<RequestItemDetail>();
            ViewBag.Status = "";
            IEnumerable<Category> CatList = await _unitOfWork.Category.GetAllAsync();
            var listcategory = CatList.Select(x => new SelectListItem { Value = x.Name, Text = x.Name });
            ViewBag.CategoryList = new SelectList(listcategory, "Value", "Text");

            RequestItemHeaderVM requestitemvm = new RequestItemHeaderVM()
            {
                RequestItemHeader = new RequestItemHeader()

            };
            if (id == null)
            {
                //this is for create
                requestitemvm.RequestItemHeader.RequestDate = DateTime.Now;
                requestitemvm.ListCategory = listcategory;

                return View(requestitemvm);
            }

            requestitemvm.RequestItemHeader = await _unitOfWork.RequestItemHeader.GetAsync(id.GetValueOrDefault());
            var DetailList = await _unitOfWork.RequestItemDetail.GetAllAsync();
            var datalist_ = (from z in DetailList.Where(z => z.IdHeader == id)
                             select new
                             {
                                 id = z.Id,
                                 idheader = z.IdHeader,
                                 itemname = z.Name,
                                 category = z.Category,
                                 reason = z.Reason,
                                 spesifik = z.Specification,
                                 price = z.Price.HasValue ? z.Price.Value.ToString("#,##0") : "",
                                 qty = z.Qty,
                                 total = z.Total.HasValue ? z.Total.Value.ToString("#,##0") : "",
                             }).ToList();

            foreach (var item in datalist_)
            {
                RequestItemDetail Items = new RequestItemDetail();
                Items.Id = item.id;
                Items.IdHeader = item.idheader;
                Items.Name = item.itemname;
                Items.Category = item.category;
                Items.Reason = item.reason;
                Items.Specification = item.spesifik;
                Items.Price = Convert.ToDouble(item.price);
                Items.Qty = Convert.ToInt32(item.qty);
                Items.Total = Convert.ToDouble(item.total);
                additemlist.Add(Items);
            }

            if (requestitemvm.RequestItemHeader == null)
            {
                return NotFound();
            }
            return View(requestitemvm);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(RequestItemHeaderVM vm)
        {           

            IEnumerable<Category> CatList = await _unitOfWork.Category.GetAllAsync();
            vm.ListCategory = CatList.Select(x => new SelectListItem { Value = x.Name, Text = x.Name });
            ViewBag.CategoryList = new SelectList(vm.ListCategory, "Value", "Text");

            string webRootPath = _hostEnvironment.WebRootPath;
            var files = HttpContext.Request.Form.Files;
            if (files.Count > 0)
            {
                string fileName = Guid.NewGuid().ToString();
                var uploads = Path.Combine(webRootPath, @"images\products");
                var extenstion = Path.GetExtension(files[0].FileName);

                if (vm.RequestItemHeader.RefFile != null)
                {
                    //this is an edit and we need to remove old image
                    var imagePath = Path.Combine(webRootPath, vm.RequestItemHeader.RefFile.TrimStart('\\'));
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }
                using (var filesStreams = new FileStream(Path.Combine(uploads, fileName + extenstion), FileMode.Create))
                {
                    files[0].CopyTo(filesStreams);
                }
                vm.RequestItemHeader.RefFile = @"\images\products\" + fileName + extenstion;
            }
          
            if (vm.RequestItemHeader.Id == 0)
            {
                
                await _unitOfWork.RequestItemHeader.AddAsync(vm.RequestItemHeader);
                ViewBag.Status = "Save Success";
                _unitOfWork.Save();
                var AddItemsList = (from z in additemlist
                                   select new RequestItemDetail
                                   {
                                       Id = z.Id,
                                       IdHeader = vm.RequestItemHeader.Id,
                                       Name = z.Name,
                                       Category = z.Category,
                                       Reason = z.Reason,
                                       Price = z.Price,
                                       Qty = z.Qty,
                                       Total = z.Total
                                   }).ToList();
                vm.AddItemsList = AddItemsList;
                foreach (var item in AddItemsList)
                {

                    RequestItemDetail itemDetail = new RequestItemDetail()
                    {
                        IdHeader = item.IdHeader,
                        Name = item.Name,
                        Category = item.Category,
                        Reason = item.Reason,
                        Specification = item.Specification,
                        Price = item.Price,
                        Qty = item.Qty,
                        Total = item.Total
                    
                    };
                    vm.RequestItemDetail = itemDetail;
                    await _unitOfWork.RequestItemDetail.AddAsync(vm.RequestItemDetail);
                    _unitOfWork.Save();

                }
               
            }
            else
            {               
                _unitOfWork.RequestItemHeader.Update(vm.RequestItemHeader);
               

                var objFromDetail = await _unitOfWork.RequestItemDetail.GetAllAsync();
                objFromDetail = objFromDetail.Where(z => z.IdHeader == vm.RequestItemHeader.Id).ToList();
                await _unitOfWork.RequestItemDetail.RemoveRangeAsync(objFromDetail);
                _unitOfWork.Save();

                var AddItemsList = (from z in additemlist
                                    select new RequestItemDetail
                                    {
                                        IdHeader = vm.RequestItemHeader.Id,
                                        Name = z.Name,
                                        Category = z.Category,
                                        Reason = z.Reason,
                                        Specification = z.Specification,
                                        Price = z.Price,
                                        Qty = z.Qty,
                                        Total = z.Total
                                    }).ToList();
                vm.AddItemsList = AddItemsList;
                foreach (var item in AddItemsList)
                {

                    RequestItemDetail itemDetail = new RequestItemDetail()
                    {
                        IdHeader = item.IdHeader,
                        Name = item.Name,
                        Category = item.Category,
                        Reason = item.Reason,
                        Specification = item.Specification,
                        Price = item.Price,
                        Qty = item.Qty,
                        Total = item.Total

                    };
                    vm.RequestItemDetail = itemDetail;
                    await _unitOfWork.RequestItemDetail.AddAsync(vm.RequestItemDetail);
                    _unitOfWork.Save();

                }
                ViewBag.Status = "Edit Success";
               
            }
            return RedirectToAction(nameof(Index));         
        }     
       
        [HttpPost]
        public JsonResult AddItem(string name,string category, string reason, string price, string qty, string total, string spesifik)
        {

            RequestItemDetail Items = new RequestItemDetail();
            Items.IdHeader = 0;
            Items.Name = name;
            Items.Category = category;
            Items.Reason = reason;
            Items.Specification = spesifik;
            Items.Price = Convert.ToDouble(price);
            Items.Qty = Convert.ToInt32(qty);
            Items.Total = Convert.ToDouble(total);

            additemlist.Add(Items);
            double? totalamount = additemlist.Sum(z=>z.Total);
            var res = new
            {
                result = "true",
                messageerrors = "",
                grandtotal = totalamount
            };
            return Json(res);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllItems(int id)
        {          
            var datalist = (from z in additemlist
                            select new
                            {
                                id = z.Id,
                                itemname = z.Name,
                                category = z.Category,
                                reason = z.Reason,
                                spesifik = z.Specification,
                                price = z.Price.HasValue ? z.Price.Value.ToString("#,##0") : "",
                                qty = z.Qty,
                                total = z.Total.HasValue ? z.Total.Value.ToString("#,##0") : "",
                            }).ToList();
            return Json(new { data = datalist });

        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var objFromDb = await _unitOfWork.RequestItemHeader.GetAsync(id);
            var objFromDetail = await _unitOfWork.RequestItemDetail.GetAllAsync();
            objFromDetail = objFromDetail.Where(z => z.IdHeader == objFromDb.Id).ToList();
            if (objFromDb == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }
            await _unitOfWork.RequestItemDetail.RemoveRangeAsync(objFromDetail);
            await _unitOfWork.RequestItemHeader.RemoveAsync(objFromDb);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Delete Successful" });

        }

        [HttpDelete]
        public async Task<IActionResult> DeleteItem(int id)
        {
            if (id > 0)
            {
                var objFromDb = await _unitOfWork.RequestItemDetail.GetAsync(id);
                if (objFromDb == null)
                {
                    return Json(new { success = false, message = "Error while deleting" });
                }
                await _unitOfWork.RequestItemDetail.RemoveAsync(objFromDb);
                _unitOfWork.Save();
            }
           
            additemlist = additemlist.Where(z => z.Id != id).ToList();
            double? totalamount = additemlist.Sum(z => z.Total);
            return Json(new { success = true, message = "Delete Successful", grandtotal = totalamount });

        }

        //public async void downloadFile(int id)
        //{
        //	var objFromDb = await _unitOfWork.RequestItemHeader.GetAsync(id);
        //	string file = System.IO.Path.GetFileName(objFromDb.RefFile);
        //	WebClient cln = new WebClient();
        //	//cln.DownloadFile(objFromDb.RefFile, file);
        //	var net = new System.Net.WebClient();
        //	var data = net.DownloadData(objFromDb.RefFile);
        //	var content = new System.IO.MemoryStream(data);
        //	var contentType = "APPLICATION/octet-stream";
        //	var fileName = "something.bin";
        //	return File(content, contentType, fileName);

        //	if (System.IO.File.Exists(file))
        //	{
        //		File(System.IO.File.OpenRead(file), "application/octet-stream", Path.GetFileName(file));
        //	}
        //}

        //[HttpGet("downloadFile")]
        public async Task<IActionResult> downloadFile(int id)
		{
			var objFromDb = await _unitOfWork.RequestItemHeader.GetAsync(id);
			//var net = new System.Net.WebClient();

           objFromDb.RefFile = "D:\\My File\\Core 2022\\Github\\Inventory\\E_OneWeb\\wwwroot\\images\\products\\31caadd9-12e4-48ac-8c95-6f7897fcc7c8.docx";
            //var data = net.DownloadData(objFromDb.RefFile);
            //var content = new System.IO.MemoryStream(data);
            //var contentType = "APPLICATION/octet-stream";
            //var fileName = "something.bin";
            //return File(content, contentType, fileName);
            //using (var client = new HttpClient())
            //using (var result = await client.GetAsync(objFromDb.RefFile))
            //{
            //    return result.IsSuccessStatusCode ? await result.Content.ReadAsByteArrayAsync() : null;
            //}
            string filePath = "tess";
            var filePathWithName = objFromDb.RefFile.Replace("\\", "/");
            var result = await GetUrlContent(filePathWithName);
            if (objFromDb != null)
            {
                return File(result, "APPLICATION/octet-stream", Path.GetFileName(filePath));
            }
            return Ok("file is not exist");

        }
        public async Task<byte[]?> GetUrlContent(string url)
        {
            using (var client = new HttpClient())
            using (var result = await client.GetAsync(url))
                return result.IsSuccessStatusCode ? await result.Content.ReadAsByteArrayAsync() : null;
        }
        //public ActionResult Export()
        //{

        //    ExcelPackage Exc = new ExcelPackage();
        //    ExcelWorksheet worksheet = Exc.Workbook.Worksheets.Add("Sheet 1");
        //    worksheet.Cells[1, 1, 1, 13].Style.Font.Bold = true;
        //    worksheet.Cells[1, 1].Value = "NO";
        //    worksheet.Cells[1, 2].Value = "MEMBER INSTANCE ID";
        //    worksheet.Cells[1, 3].Value = "MEMBER NAMA";
        //    worksheet.Cells[1, 4].Value = "TGL LAHIR";
        //    worksheet.Cells[1, 5].Value = "TMT PESERTA";
        //    worksheet.Cells[1, 6].Value = "TMT PENSIUN";
        //    worksheet.Cells[1, 7].Value = "AMOUNT";
        //    worksheet.Cells[1, 8].Value = "BANK ACCOUNT NO";
        //    worksheet.Cells[1, 9].Value = "BANK ACCOUNT NAME";
        //    worksheet.Cells[1, 10].Value = "BANK NAME";
        //    worksheet.Cells[1, 11].Value = "BANK BRANCH";
        //    worksheet.Cells[1, 12].Value = "INSTANSI";
        //    worksheet.Cells[1, 13].Value = "DAPEM CODE";

        //    worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

        //    byte[] bin = Exc.GetAsByteArray();
        //    Response.Buffer = true;
        //    Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        //    Response.AddHeader("content-length", bin.Length.ToString());
        //    Response.AddHeader("content-disposition", "attachment; filename=\"ExportApprovalDapemlist_" + DateTime.Now.ToString("yyyyMMdd:hh.mm") + ".xlsx\"");
        //    Response.OutputStream.Write(bin, 0, bin.Length);
        //    Response.Flush();
        //    Response.SuppressContent = true;


        //    return View();

        //}
    }
}
