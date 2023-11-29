using E_OneWeb.DataAccess.Repository;
using E_OneWeb.DataAccess.Repository.IRepository;
using E_OneWeb.Models.ViewModels;
using E_OneWeb.Models;
using E_OneWeb.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data;
using System.Security.Claims;
using System.Diagnostics;
using System.Linq;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace E_OneWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class ItemTransferController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public ItemTransferController(IUnitOfWork unitOfWork)
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
            IEnumerable<Room> RoomList = _unitOfWork.Room.GetAll();
            var datalist = (from z in await _unitOfWork.ItemTransfer.GetAllAsync(includeProperties: "Items")
                            select new
                            {
                                id = z.Id,
                                itemname = z.Items.Name,
                                previouslocation = z.PreviousLocation,
								currentlocation = z.CurrentLocation,
								desc = z.Description,
                                stranferdate = Convert.ToDateTime( z.TransferDate).ToString("dd-MM-yyyy"),
                                entrydate = z.EntryDate
                            }).OrderByDescending(i=>i.entrydate).ToList();

            return Json(new { data = datalist });
        }
        public static int? transferId { get; set; }    
        [HttpGet]
        public async Task<IActionResult> GetProduct()
        {
            var datalist = (from z in await _unitOfWork.Items.GetAllAsync(includeProperties: "Category,Room")
                            select new
                            {
                                id = z.Id,
                                name = z.Name,
                                room = z.Room.Name,
                                category = z.Category.Name,
                                description = z.Description,
                                idtranfer = transferId
                            }).ToList();

            return Json(new { data = datalist });
        }
        [HttpGet] 
        public async Task<IActionResult> GetItem(int? id)
        {
            ItemTransferVM itemsVM = new ItemTransferVM();

            //var Items = await _unitOfWork.Items.GetAsync(id.GetValueOrDefault());
            //ViewBag.ItemsName = Items.Name;

            var datalist = (from z in await _unitOfWork.Items.GetAllAsync(includeProperties: "Category,Room")
                            select new
                            {
                                id = z.Id,
                                code = z.Code,
                                name = z.Name,
                                description = z.Description,
                                price = z.Price != null ? z.Price : 0,
                                qty = z.Qty != null ? z.Qty : 0,
                                totalamount = z.TotalAmount != null ? z.TotalAmount : 0,
                                room = z.Room.Name,
                                category = z.Category.Name
                            }).Where(z => z.id == id.GetValueOrDefault()).FirstOrDefault();
            ViewBag.ItemsName = datalist.name;
            //return Json(new { data = datalist });
			return RedirectToAction("Upsert", new { id = transferId, itemId = id });

		}

        public ActionResult LookupItems()
        {
         
            return PartialView();
        }        

        public async Task<IActionResult> Upsert(int? id, int? itemId)
        {
            //itemId = id;
            var ListLocation = _unitOfWork.Room.GetAll().Select(x => new SelectListItem { Value = x.Id.ToString(), Text = x.Name }).ToList();
            ViewBag.PreviousLocationList = new SelectList(ListLocation, "Value", "Text");
            //ViewBag.CurrentLocationList = new SelectList(ListLocation, "Value", "Text");
            IEnumerable<Items> ItemsList = await _unitOfWork.Items.GetAllAsync();
            ItemTransferVM itemsVM = new ItemTransferVM()
            {
                ItemTransfer = new ItemTransfer() 
               
            };
            if (id == null)
            {
                //this is for create
                itemsVM.ItemTransfer.TransferDate = DateTime.Now;
                if (itemId != null)
                {
                    itemsVM.ItemTransfer.Items = ItemsList.Where(z => z.Id == itemId).FirstOrDefault();
                    itemsVM.ItemTransfer.PreviousLocationId = itemsVM.ItemTransfer.Items.Room.Id;
                    itemsVM.ItemTransfer.PreviousLocation = itemsVM.ItemTransfer.Items.Room.Name;
                }
                  
              
                return View(itemsVM);
            }

            itemsVM.ItemTransfer = await _unitOfWork.ItemTransfer.GetAsync(id.GetValueOrDefault());
            ViewBag.PreviousLocationList = new SelectList(ListLocation, "Value", "Text", itemsVM.ItemTransfer.PreviousLocationId);
            Room LocationName = _unitOfWork.Room.GetAll(includeProperties: "Location").Where(z=>z.Id == itemsVM.ItemTransfer.CurrentLocationId).FirstOrDefault();
            itemsVM.name_of_room_and_location = itemsVM.ItemTransfer.CurrentLocation + " (" + LocationName.Location.Name + ")";
            //ViewBag.CurrentLocationList = new SelectList(ListLocation, "Value", "Text", itemsVM.ItemTransfer.CurrentLocationId);
            //if (itemId != null)
            //{
            //    ViewBag.ItemsName = itemsVM.ItemList.FirstOrDefault().Text;
            //}
            //itemsVM.ItemTransfer.Items.Name = itemsVM.ItemList.FirstOrDefault().Text;
            if (itemsVM.ItemTransfer == null)
            {
                return NotFound();
            }
          
            ViewBag.Status = "";
            //var ListLocation = db.GENMASTERs.Where(x => x.GENFLAG == 102).Select(x => new SelectListItem { Value = x.GENNAME, Text = x.GENNAME }).ToList();
           
            return View(itemsVM);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(ItemTransferVM vm)
        {
            var ListLocation = _unitOfWork.Room.GetAll().Select(x => new SelectListItem { Value = x.Id.ToString(), Text = x.Name }).ToList();  
            ViewBag.PreviousLocationList = new SelectList(ListLocation, "Value", "Text", vm.ItemTransfer.PreviousLocationId);
            //ViewBag.CurrentLocationList = new SelectList(ListLocation, "Value", "Text", vm.ItemTransfer.CurrentLocationId);
           
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var user = _unitOfWork.ApplicationUser.GetFirstOrDefault(u => u.Id == claim.Value);

            vm.ItemTransfer.EntryBy = user.Name;
            vm.ItemTransfer.EntryDate = DateTime.Now;
            vm.ItemTransfer.PreviousLocation = ListLocation.Where(z => z.Value == vm.ItemTransfer.PreviousLocationId.ToString()).Select(x => x.Text).FirstOrDefault();
            vm.ItemTransfer.CurrentLocation = ListLocation.Where(z => z.Value == vm.ItemTransfer.CurrentLocationId.ToString()).Select(x => x.Text).FirstOrDefault();
            vm.ItemTransfer.ItemId = vm.ItemTransfer.Items.Id;

            IEnumerable <Items> itemList = await _unitOfWork.Items.GetAllAsync();
            vm.ItemTransfer.Items = itemList.Where(z => z.Id == vm.ItemTransfer.Items.Id).FirstOrDefault();
            //vm.ItemList = itemList.Select(i => new SelectListItem
            //{
            //    Text = i.Name,
            //    Value = i.Id.ToString()
            //});


            if (vm.ItemTransfer.Id == 0)
            {
                await _unitOfWork.ItemTransfer.AddAsync(vm.ItemTransfer);
                ViewBag.Status = "Save Success";

            }
            else
            {
                _unitOfWork.ItemTransfer.Update(vm.ItemTransfer);
                ViewBag.Status = "Edit Success";
            }

            ItemsVM itemsVM = new ItemsVM();
            itemsVM.Items = await _unitOfWork.Items.GetAsync(vm.ItemTransfer.ItemId);
            itemsVM.Items.RoomId = vm.ItemTransfer.CurrentLocationId;
            if (itemsVM.Items.Id != 0)
            {
                _unitOfWork.Items.Update(itemsVM.Items);

            }         

            _unitOfWork.Save();
           
            return View(vm);
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var objFromDb = await _unitOfWork.ItemTransfer.GetAsync(id);
            if (objFromDb == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }

            await _unitOfWork.ItemTransfer.RemoveAsync(objFromDb);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Delete Successful" });

        }

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
