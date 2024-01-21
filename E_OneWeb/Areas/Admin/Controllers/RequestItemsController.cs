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
using static System.Net.Mime.MediaTypeNames;
using System.IO.Pipes;
using Microsoft.AspNetCore.StaticFiles;
using System.Security.Claims;
using static com.sun.tools.@internal.xjc.reader.xmlschema.bindinfo.BIConversion;

namespace E_OneWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee + "," + SD.Role_Unit)]
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
        [Authorize(Roles = SD.Role_Employee + "," + SD.Role_Unit)]
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var user = _unitOfWork.ApplicationUser.GetFirstOrDefault(u => u.Id == claim.Value);
            var rolesUser = _unitOfWork.ApplicationUser.GetAll();

            List<string> UserUnitList = rolesUser.Where(z => z.RolesName == user.RolesName).Select(u => u.UserName).ToList();

            var datalist = (from z in await _unitOfWork.RequestItemHeader.GetAllAsync()
                            select new
                            {
                                id = z.Id,
                                refnumber = z.ReqNumber,
                                requestdate = Convert.ToDateTime(z.RequestDate).ToString("dd-MM-yyyy"),
                                requestby = z.Requester,
                                desc = z.Notes,
                                totalamount = z.TotalAmount.HasValue ? z.TotalAmount.Value.ToString("#,##0") : "",
                                status = z.Status,
                                entryby = z.EntryBy,
                                approveby = z.ApproveBy,
                                approvedate = z.ApproveDate != null ? z.ApproveDate.Value.ToString("dd-MM-yyyy") : ""

                            }).Where(u => UserUnitList.Contains(u.entryby)).OrderByDescending(i => i.id).ToList();

            return Json(new { data = datalist });
        }
        [Authorize(Roles = SD.Role_Employee + "," + SD.Role_Unit)]
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
            if (requestitemvm.RequestItemHeader.RefFile != null)
            {
                ViewBag.fileDownload = "true";
            }
            else
            {
                ViewBag.fileDownload = "";
            }

            
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
                                 status = z.Status,
                                 roomid = z.RoomId,
                                 roomname = z.RoomName
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
                Items.Status = item.status;
                Items.RoomId = item.roomid;
                Items.RoomName = item.roomname;
                additemlist.Add(Items);
            }

            if (requestitemvm.RequestItemHeader == null)
            {
                return NotFound();
            }
            return View(requestitemvm);

        }
        [Authorize(Roles = SD.Role_Employee + "," + SD.Role_Unit)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(RequestItemHeaderVM vm)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var user = _unitOfWork.ApplicationUser.GetFirstOrDefault(u => u.Id == claim.Value);
            vm.RequestItemHeader.EntryBy = user.Name;
            vm.RequestItemHeader.EntryDate = DateTime.Now;

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
                vm.RequestItemHeader.RefFile = fileName  + extenstion;
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
                                       Total = z.Total,
                                       RoomId = z.RoomId,
                                       RoomName = z.RoomName
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
                        Total = item.Total,
                        RoomId = item.RoomId,
                        RoomName = item.RoomName

                    };
                    vm.RequestItemDetail = itemDetail;
                    await _unitOfWork.RequestItemDetail.AddAsync(vm.RequestItemDetail);
                    _unitOfWork.Save();

                }
                //TempData["Success"] = "Save Request items successfully";
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
                                        Total = z.Total,
                                        RoomId = z.RoomId,
                                        RoomName = z.RoomName
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
                        Total = item.Total,
                        RoomId = item.RoomId,
                        RoomName = item.RoomName
                    };
                    vm.RequestItemDetail = itemDetail;
                    await _unitOfWork.RequestItemDetail.AddAsync(vm.RequestItemDetail);
                    _unitOfWork.Save();

                }
                //TempData["Success"] = "Update Request items successfully";
                ViewBag.Status = "Update Success";
               
            }
            return RedirectToAction(nameof(Index));         
        }
        [Authorize(Roles = SD.Role_Employee + "," + SD.Role_Unit)]
        [HttpPost]        
        public JsonResult AddItem(string name,string category, string reason, string price, string qty, string total, string spesifik, int? idroom, string room)
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
            Items.RoomId = idroom;
            Items.RoomName = room;

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
                                status = z.Status,
                            }).ToList();
            return Json(new { data = datalist });

        }

        [HttpGet]
        public async Task<IActionResult> GetAllViewItems(int id)
        {
            var DetailList = await _unitOfWork.RequestItemDetail.GetAllAsync();
            var datalist = (from z in DetailList.Where(z => z.IdHeader == id)
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
                                 status = z.Status,
                             }).ToList();
        
            return Json(new { data = datalist });

        }

        [HttpDelete]
        [Authorize(Roles = SD.Role_Employee + "," + SD.Role_Unit)]
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
               
        public async Task<byte[]?> GetUrlContent(string url)
        {
            using (var client = new HttpClient())
            using (var result = await client.GetAsync(url))
                return result.IsSuccessStatusCode ? await result.Content.ReadAsByteArrayAsync() : null;
        }
      
        private MemoryStream DownloadSinghFile(string filename, string uploadPath)
        { 
            var path = Path.Combine(Directory.GetCurrentDirectory(),uploadPath, filename);
            var memory = new MemoryStream();
            if (System.IO.File.Exists(path))
            { 
                var net = new System.Net.WebClient();
                var data = net.DownloadData(path);
                var content = new System.IO.MemoryStream(data);
                memory = content;
            }
            memory.Position = 0;
            return memory;
        }
        //[HttpGet("Export")]
        public async Task<IActionResult> Export(string id)
        {

            int idheader = id != null ? Convert.ToInt32(id) : 0;
            var objFromDb = await _unitOfWork.RequestItemHeader.GetAsync(idheader);

            string filename = objFromDb.RefFile;
            string filePath = "wwwroot\\images\\products";
            var memory = DownloadSinghFile(filename, filePath);
            var contentType = "APPLICATION/octet-stream";
            return File(memory.ToArray(), contentType, filename);
            //return File(memory.ToArray(), contentType, Path.GetFileName(filePath));

        }


        #region Approve by Admin
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult Approve()
		{
			return View();
		}
        [HttpGet]
        public async Task<IActionResult> GetAllApprove()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var user = _unitOfWork.ApplicationUser.GetFirstOrDefault(u => u.Id == claim.Value);
            var rolesUser = _unitOfWork.ApplicationUser.GetAll();
            List<string> UserUnitList;
            if (user.RolesName == "Employee")
            {
                UserUnitList = rolesUser.Where(z => z.RolesName == "Unit").Select(u => u.UserName).ToList();
            }
            else
            {
                UserUnitList = rolesUser.Where(z => z.RolesName == "Employee").Select(u => u.UserName).ToList();
            }

            var datalist = (from z in await _unitOfWork.RequestItemHeader.GetAllAsync()
                            select new
                            {
                                id = z.Id,
                                refnumber = z.ReqNumber,
                                requestdate = Convert.ToDateTime(z.RequestDate).ToString("dd-MM-yyyy"),
                                requestby = z.Requester,
                                desc = z.Notes,
                                totalamount = z.TotalAmount.HasValue ? z.TotalAmount.Value.ToString("#,##0") : "",
                                status = z.Status,
                                entryby = z.EntryBy

                            }).Where(u => UserUnitList.Contains(u.entryby)).OrderByDescending(i => i.id).ToList();

            return Json(new { data = datalist });
        }
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public async Task<IActionResult> ViewApprove(int? id)
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
			if (requestitemvm.RequestItemHeader.RefFile != null)
			{
				ViewBag.fileDownload = "true";
			}
			else
			{
				ViewBag.fileDownload = "";
			}


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
        public async Task<IActionResult> ApproveHeader(int id)
        {
            try
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                var user = _unitOfWork.ApplicationUser.GetFirstOrDefault(u => u.Id == claim.Value);
                var Gen_6 = _unitOfWork.Genmaster.GetAll().Where(z => z.GENFLAG == 6 && z.GENVALUE == 1).FirstOrDefault();

                RequestItemHeader requestItemHeader = await _unitOfWork.RequestItemHeader.GetAsync(id);
                requestItemHeader.StatusId = Gen_6.IDGEN;
                requestItemHeader.Status = Gen_6.GENNAME;
                requestItemHeader.ApproveBy = user.UserName;
                requestItemHeader.ApproveDate = DateTime.Now;
                requestItemHeader.RejectedBy = null;
                requestItemHeader.RejectedDate = null;

                _unitOfWork.RequestItemHeader.Update(requestItemHeader);

                TempData["Success"] = "Successfully approved";
                return Json(new { success = true, message = "Approved Successful" });
            }
            catch (Exception)
            {
                TempData["Failed"] = "Error approved";
                return Json(new { success = false, message = "Approved Error" });
            }
           

        }
        [HttpPost]
        public async Task<IActionResult> RejectHeader(string note, int idheader)
        {
            try
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                var user = _unitOfWork.ApplicationUser.GetFirstOrDefault(u => u.Id == claim.Value);
                var Gen_6 = _unitOfWork.Genmaster.GetAll().Where(z => z.GENFLAG == 6 && z.GENVALUE == 2).FirstOrDefault();

                RequestItemHeader requestItemHeader = await _unitOfWork.RequestItemHeader.GetAsync(idheader);
                requestItemHeader.StatusId = Gen_6.IDGEN;
                requestItemHeader.Status = Gen_6.GENNAME;
                requestItemHeader.Notes = note;
                requestItemHeader.RejectedBy = user.UserName;
                requestItemHeader.RejectedDate = DateTime.Now;
                requestItemHeader.ApproveBy = null;
                requestItemHeader.ApproveDate = null;
                _unitOfWork.RequestItemHeader.Update(requestItemHeader);

                TempData["Success"] = "Successfully reject";
                return Json(new { success = true, message = "Reject Successful" });
            }
            catch (Exception)
            {
                TempData["Failed"] = "Error reject";
                return Json(new { success = false, message = "Reject Error" });
            }


        }        

        [HttpPost]
        public async Task<IActionResult> ApproveDetail(int id)
        {
            try
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                var user = _unitOfWork.ApplicationUser.GetFirstOrDefault(u => u.Id == claim.Value);
                var Gen_6 = _unitOfWork.Genmaster.GetAll().Where(z => z.GENFLAG == 6 && z.GENVALUE == 1).FirstOrDefault();

                RequestItemDetail requestItemDetail = await _unitOfWork.RequestItemDetail.GetAsync(id);
                requestItemDetail.StatusId = Gen_6.IDGEN;
                requestItemDetail.Status = Gen_6.GENNAME;
                requestItemDetail.ApproveBy = user.UserName;
                requestItemDetail.ApproveDate = DateTime.Now;
                requestItemDetail.RejectedBy = null;
                requestItemDetail.RejectedDate = null;
                _unitOfWork.RequestItemDetail.Update(requestItemDetail);
             
                TempData["Success"] = "Successfully approved";
                return Json(new { success = true, message = "Approved Successful" });
            }
            catch (Exception)
            {
                TempData["Failed"] = "Error approved";
                return Json(new { success = false, message = "Approved Error" });
            }


        }
        [HttpPost]
        public async Task<IActionResult> RejecteDetail(string note, int iddetail)
        {
            try
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                var user = _unitOfWork.ApplicationUser.GetFirstOrDefault(u => u.Id == claim.Value);
                var Gen_6 = _unitOfWork.Genmaster.GetAll().Where(z => z.GENFLAG == 6 && z.GENVALUE == 2).FirstOrDefault();

                RequestItemDetail requestItemDetail = await _unitOfWork.RequestItemDetail.GetAsync(iddetail);
                requestItemDetail.StatusId = Gen_6.IDGEN;
                requestItemDetail.Status = Gen_6.GENNAME;
                requestItemDetail.RejectedBy = user.UserName;
                requestItemDetail.RejectedDate = DateTime.Now;
                requestItemDetail.ApproveBy = null;
                requestItemDetail.ApproveDate = null;

                _unitOfWork.RequestItemDetail.Update(requestItemDetail);

                TempData["Success"] = "Successfully reject";
                return Json(new { success = true, message = "Reject Successful" });
            }
            catch (Exception)
            {
                TempData["Failed"] = "Error reject";
                return Json(new { success = false, message = "Reject Error" });
            }


        }
        #endregion

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
    }
}
