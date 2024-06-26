﻿using E_OneWeb.DataAccess.Repository.IRepository;
using E_OneWeb.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using E_OneWeb.Models;
using E_OneWeb.Models.ViewModels;
using System.Drawing.Drawing2D;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using static com.sun.tools.@internal.xjc.reader.xmlschema.bindinfo.BIConversion;

namespace E_OneWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee + "," + SD.Role_Unit)]
    public class ServiceItemsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<IdentityUser> _userManager;
        public ServiceItemsController(IUnitOfWork unitOfWork,RoleManager<IdentityRole> roleManager, UserManager<IdentityUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _roleManager = roleManager;
            _userManager = userManager;
        }
        [Authorize(Roles = SD.Role_Employee + "," + SD.Role_Unit)]
        public IActionResult Index()
        {
            return View();
        }
        [Authorize(Roles = SD.Role_Employee + "," + SD.Role_Unit)]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var user = _unitOfWork.ApplicationUser.GetFirstOrDefault(u => u.Id == claim.Value);

            //var RoleList = _roleManager.Roles.ToList();

            var rolesUser = _unitOfWork.ApplicationUser.GetAll();
            //var RoleUserList = _userManager.Users.ToList();

            List<string> UserUnitList = rolesUser.Where(z => z.RolesName == user.RolesName).Select(u=>u.UserName).ToList();


            var datalist = (from z in await _unitOfWork.ItemService.GetAllAsync(includeProperties: "Items")
                            select new
                            {
                                id = z.Id,
                                itemname = z.Items.Name,
                                location = _unitOfWork.Room.Get((int)z.RoomId).Name + " (" + _unitOfWork.Location.Get((int)z.LocationId).Name + ")",
                                servicedate = Convert.ToDateTime(z.ServiceDate).ToString("dd-MM-yyyy"),
                                serviceenddate = Convert.ToDateTime(z.ServiceEndDate).ToString("dd-MM-yyyy"),
                                desc = z.RepairDescription,
                                requestby = z.RequestBy,
                                costofrepair = z.CostOfRepair.HasValue ? z.CostOfRepair.Value.ToString("#,##0") : "",
                                status = z.Status,
                                notes = z.Notes,
                                entryby = z.EntryBy,
                                approveby = z.ApproveBy,
                                approvedate = z.ApproveDate != null ? z.ApproveDate.Value.ToString("dd-MM-yyyy") : ""

							}).Where(u=> UserUnitList.Contains(u.entryby)).OrderByDescending(i => i.id).ToList();

           
            return Json(new { data = datalist });
        }
        [Authorize(Roles = SD.Role_Employee + "," + SD.Role_Unit)]
        public async Task<IActionResult> Upsert(int? id)
        {
            ViewBag.Status = "";
            IEnumerable<Items> ItemsList = await _unitOfWork.Items.GetAllAsync();
            ItemServiceVM itemservicevm = new ItemServiceVM()
            {
                ItemService = new ItemService()

            };
            if (id == null)
            {
                //this is for create
                itemservicevm.ItemService.ServiceDate = DateTime.Now;
                itemservicevm.ItemService.ServiceEndDate = DateTime.Now;          

                return View(itemservicevm);
            }

            itemservicevm.ItemService = await _unitOfWork.ItemService.GetAsync(id.GetValueOrDefault());
            itemservicevm.name_of_room_and_location = _unitOfWork.Room.Get((int)itemservicevm.ItemService.RoomId).Name + " (" + _unitOfWork.Location.Get((int)itemservicevm.ItemService.LocationId).Name + ")";
		
			if (itemservicevm.ItemService == null)
            {
                return NotFound();
            }
            return View(itemservicevm); 

        }
        [Authorize(Roles = SD.Role_Employee + "," + SD.Role_Unit)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(ItemServiceVM vm)
        {
           
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var user = _unitOfWork.ApplicationUser.GetFirstOrDefault(u => u.Id == claim.Value);
            var Gen_6 = _unitOfWork.Genmaster.GetAll().Where(z => z.GENFLAG == 6 && z.GENVALUE == 0).FirstOrDefault();
            vm.ItemService.StatusId = Gen_6.IDGEN;
            vm.ItemService.Status = Gen_6.GENNAME;
            vm.ItemService.EntryBy = user.UserName;
            vm.ItemService.EntryDate = DateTime.Now;

            IEnumerable<Items> itemList = await _unitOfWork.Items.GetAllAsync();
			var Items = itemList.Where(z => z.Id == vm.ItemService.Items.Id).FirstOrDefault();
			var Rooms = _unitOfWork.Room.Get(Items.RoomId);
            var Locations = _unitOfWork.Location.Get(Rooms.IDLocation);
            vm.ItemService.LocationId = Locations.Id;
			vm.ItemService.RoomId = Items.RoomId;
			vm.ItemService.Items = Items;
            vm.name_of_room_and_location = Rooms.Name + " (" + Locations.Name + ")";
            if (vm.ItemService.Id == 0)
            {
                _unitOfWork.ItemService.AddAsync(vm.ItemService);
                ViewBag.Status = "Save Success";

            }
            else
            {
                _unitOfWork.ItemService.Update(vm.ItemService);
                ViewBag.Status = "Update Success";
            }
            _unitOfWork.Save();       
            return View(vm);
        }
        [Authorize(Roles = SD.Role_Employee + "," + SD.Role_Unit)]
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
                                name_of_room_and_location_ = z.Room.Name + " (" + _unitOfWork.Location.Get(z.Room.IDLocation).Name + ")"
                            }).ToList();

            return Json(new { data = datalist });
        }
        [Authorize(Roles = SD.Role_Employee + "," + SD.Role_Unit)]
        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var objFromDb = await _unitOfWork.ItemService.GetAsync(id);
            if (objFromDb == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }
            await _unitOfWork.ItemService.RemoveAsync(objFromDb);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Delete Successful" });

        }

        #region Approve by Admin and 
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult Approve()
        {
            return View();
        }
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
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


            var Gen_6 = _unitOfWork.Genmaster.GetAll().Where(z => z.GENFLAG == 6).ToList();

            var datalist = (from z in await _unitOfWork.ItemService.GetAllAsync(includeProperties: "Items")
                            select new
                            {
                                id = z.Id,
                                itemname = z.Items.Name,
                                location = _unitOfWork.Room.Get((int)z.RoomId).Name + " (" + _unitOfWork.Location.Get((int)z.LocationId).Name + ")",
                                servicedate = Convert.ToDateTime(z.ServiceDate).ToString("dd-MM-yyyy"),
                                serviceenddate = Convert.ToDateTime(z.ServiceEndDate).ToString("dd-MM-yyyy"),
                                desc = z.RepairDescription,
                                requestby = z.RequestBy,
                                costofrepair = z.CostOfRepair.HasValue ? z.CostOfRepair.Value.ToString("#,##0") : "",
                                status = Gen_6.Where(i => i.IDGEN == z.StatusId).Select(i => i.GENNAME).FirstOrDefault(),
                                notes = z.Notes,
                                entryby = z.EntryBy

                            }).Where(u => UserUnitList.Contains(u.entryby)).OrderByDescending(i => i.id).ToList();

            return Json(new { data = datalist });
        }
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public async Task<IActionResult> ViewApprove(int? id)
        {

            ViewBag.Status = "";
            IEnumerable<Items> ItemsList = await _unitOfWork.Items.GetAllAsync();
            ItemServiceVM itemservicevm = new ItemServiceVM()
            {
                ItemService = new ItemService()

            };
            if (id == null)
            {
                //this is for create
                itemservicevm.ItemService.ServiceDate = DateTime.Now;
                itemservicevm.ItemService.ServiceEndDate = DateTime.Now;

                return View(itemservicevm);
            }

            itemservicevm.ItemService = await _unitOfWork.ItemService.GetAsync(id.GetValueOrDefault());
            itemservicevm.name_of_room_and_location = _unitOfWork.Room.Get((int)itemservicevm.ItemService.RoomId).Name + " (" + _unitOfWork.Location.Get((int)itemservicevm.ItemService.LocationId).Name + ")";

            if (itemservicevm.ItemService == null)
            {
                return NotFound();
            }
            return View(itemservicevm);

        }
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        [HttpPost]
        public async Task<IActionResult> Approved(int id)
        {
            try
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                var user = _unitOfWork.ApplicationUser.GetFirstOrDefault(u => u.Id == claim.Value);

                var Gen_6 = _unitOfWork.Genmaster.GetAll().Where(z => z.GENFLAG == 6 && z.GENVALUE == 1).FirstOrDefault();

                ItemService itemService = await _unitOfWork.ItemService.GetAsync(id);
                itemService.StatusId = Gen_6.IDGEN;
                itemService.Status = Gen_6.GENNAME;
                itemService.Notes = "";
                itemService.ApproveBy = user.UserName;
                itemService.ApproveDate = DateTime.Now;
                itemService.RejectedBy = null;
                itemService.RejectedDate = null;

                _unitOfWork.ItemService.Update(itemService);

                TempData["Success"] = "Successfully approved";
                return Json(new { success = true, message = "Approved Successful" });
            }
            catch (Exception)
            {
                TempData["Failed"] = "Error approved";
                return Json(new { success = false, message = "Approved Error" });
            }


        }
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        [HttpPost]
        public async Task<IActionResult> Rejected(string note, int id)
        {
            try
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                var user = _unitOfWork.ApplicationUser.GetFirstOrDefault(u => u.Id == claim.Value);
                var Gen_6 = _unitOfWork.Genmaster.GetAll().Where(z => z.GENFLAG == 6 && z.GENVALUE == 2).FirstOrDefault();

                ItemService itemService = await _unitOfWork.ItemService.GetAsync(id);
                itemService.StatusId = Gen_6.IDGEN;
                itemService.Status = Gen_6.GENNAME;
                itemService.Notes = note;
                itemService.RejectedBy = user.UserName;
                itemService.RejectedDate = DateTime.Now;
                itemService.ApproveBy = null;
                itemService.ApproveDate = null;
                _unitOfWork.ItemService.Update(itemService);

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
        public async Task<IActionResult> GetServiceItem(int? id, int? serviceId)
        {

            var datalist = (from z in await _unitOfWork.ItemService.GetAllAsync(includeProperties: "Items")
                            select new
                            {
                                id = z.Id,
                                itemid = z.ItemId,
                                servicedate = Convert.ToDateTime(z.ServiceDate).ToString("dd-MM-yyyy"),
                                serviceenddate = Convert.ToDateTime(z.ServiceEndDate).ToString("dd-MM-yyyy"),
                                desc = z.RepairDescription,
                                tecnician = z.Technician,
                                phone = z.PhoneNumber,
                                requestby = z.RequestBy,
                                costofrepair = z.CostOfRepair.HasValue ? z.CostOfRepair.Value.ToString("#,##0") : ""

                            }).Where(o => o.itemid == id && o.id != serviceId).OrderByDescending(i => i.id).ToList();

            return Json(new { data = datalist });
        }

        public async Task<IActionResult> Create(int? id)
        {
            ItemServiceVM itemservicevm = new ItemServiceVM()
            {
                ItemService = new ItemService()

            };

            itemservicevm.ItemService.Items = await _unitOfWork.Items.GetAsync(id.GetValueOrDefault());
            
            Room room = _unitOfWork.Room.Get(itemservicevm.ItemService.Items.RoomId);
            Location location = _unitOfWork.Location.Get(room.IDLocation);
            ViewBag.Status = "";
            itemservicevm.name_of_room_and_location = room.Name + " (" + location.Name + ")";
            //this is for create
            itemservicevm.ItemService.ServiceDate = DateTime.Now;
            itemservicevm.ItemService.ServiceEndDate = DateTime.Now;

            if (itemservicevm.ItemService == null)
            {
                return NotFound();
            }
            return View(itemservicevm);

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ItemServiceVM vm)
        {

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var user = _unitOfWork.ApplicationUser.GetFirstOrDefault(u => u.Id == claim.Value);
            var Gen_6 = _unitOfWork.Genmaster.GetAll().Where(z => z.GENFLAG == 6 && z.GENVALUE == 0).FirstOrDefault();
            vm.ItemService.StatusId = Gen_6.IDGEN;
            vm.ItemService.Status = Gen_6.GENNAME;
            vm.ItemService.EntryBy = user.UserName;
            vm.ItemService.EntryDate = DateTime.Now;

            IEnumerable<Items> itemList = await _unitOfWork.Items.GetAllAsync();
            var Items = itemList.Where(z => z.Id == vm.ItemService.Items.Id).FirstOrDefault();
            var Rooms = _unitOfWork.Room.Get(Items.RoomId);
            var Locations = _unitOfWork.Location.Get(Rooms.IDLocation);
            vm.ItemService.LocationId = Locations.Id;
            vm.ItemService.RoomId = Items.RoomId;
            vm.ItemService.Items = Items;
            vm.name_of_room_and_location = Rooms.Name + " (" + Locations.Name + ")";
            if (vm.ItemService.Id == 0)
            {
                _unitOfWork.ItemService.AddAsync(vm.ItemService);
                ViewBag.Status = "Save Success";

            }
      
            _unitOfWork.Save();
            return View(vm);
        }
    }
}
