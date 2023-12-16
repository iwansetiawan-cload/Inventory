using E_OneWeb.DataAccess.Repository.IRepository;
using E_OneWeb.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using E_OneWeb.Models;
using E_OneWeb.Models.ViewModels;
using System.Drawing.Drawing2D;
using System.Security.Claims;

namespace E_OneWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
    public class ServiceItemsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public ServiceItemsController(IUnitOfWork unitOfWork)
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
                                costofrepair = z.CostOfRepair.HasValue ? z.CostOfRepair.Value.ToString("#,##0") : ""

							}).OrderByDescending(i => i.id).ToList();

            return Json(new { data = datalist });
        }
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(ItemServiceVM vm)
        {
           
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var user = _unitOfWork.ApplicationUser.GetFirstOrDefault(u => u.Id == claim.Value);
            vm.ItemService.Status = 0;
            vm.ItemService.EntryBy = user.Name;
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
    }
}
