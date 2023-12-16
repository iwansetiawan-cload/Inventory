using E_OneWeb.DataAccess.Repository.IRepository;
using E_OneWeb.Models;
using E_OneWeb.Models.ViewModels;
using E_OneWeb.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Finance.Implementations;
using System.Data;

namespace E_OneWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
    public class RoomController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public RoomController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            LocationVM locationVM = new LocationVM()
            {
                Locations = _unitOfWork.Location.GetAll()
            };
            var count = locationVM.Locations.Count();
            return View(locationVM);
        }

        public IActionResult Upsert(int? id)
        {
            Location location = new Location();
            if (id == null)
            {
                //this is for create
                return View(location);
            }
            //this is for edit
            location = _unitOfWork.Location.Get(id.GetValueOrDefault());
            if (location == null)
            {
                return NotFound();
            }
            LocationId = location.Id;
            ViewBag.Status = "";
            return View(location);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(Location location)
        {
            if (ModelState.IsValid)
            {
                if (location.Id == 0)
                {
                    _unitOfWork.Location.Add(location);
                    ViewBag.Status = "Save Success";
                }
                else
                {
                    _unitOfWork.Location.Update(location);
                    ViewBag.Status = "Update Success";
                }
                _unitOfWork.Save();
                return RedirectToAction(nameof(Index));
            }
            return View(location);
        }
        public static int LocationId { get; set; }
        public IActionResult UpsertRoom(int? idlocation, int? id)
        {
         
            Room room = new Room();
            LocationId = Convert.ToInt32(idlocation);
            room.IDLocation = LocationId;

            //Location location = new Location();
            //location = _unitOfWork.Location.Get(room.IDLocation);
            //room.Location = location;

            if (id == null)
            {
                //this is for create
                return View(room);
            }
            //this is for edit
            room = _unitOfWork.Room.Get(id.GetValueOrDefault());
            if (room == null)
            {
                return NotFound();
            }
            else
            {
                LocationId = room.IDLocation;

			}
            ViewBag.Status = "";

            return View(room);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpsertRoom(Room room)
        {

            Location location = new Location();
            location = _unitOfWork.Location.Get(LocationId);
            room.Location = location;          

            if (room.Id == 0)
            {
                _unitOfWork.Room.Add(room);
                ViewBag.Status = "Save Success";
            }
            else
            {
                room.IDLocation = room.Location.Id;
				_unitOfWork.Room.Update(room);
                ViewBag.Status = "Update Success";
            }
            _unitOfWork.Save();
            return View(room);
            //return RedirectToAction(nameof(Upsert), new { id = room.IDLocation});


            //return View(room);
        }

        #region API CALLS

        [HttpGet]
        public IActionResult GetAll()
        {
            var allObj = _unitOfWork.Location.GetAll();
            return Json(new { data = allObj });
        }

        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var objFromDb = _unitOfWork.Location.Get(id);
            if (objFromDb == null)
            {
                TempData["Error"] = "Error deleting Category";
                return Json(new { success = false, message = "Error while deleting" });
            }
            _unitOfWork.Location.Remove(objFromDb);
            _unitOfWork.Save();

            TempData["Success"] = "Category successfully deleted";
            return Json(new { success = true, message = "Delete Successful" });

        }

		[HttpGet]
		public IActionResult GetAllRoom()
		{			
			var datalist = (from z in _unitOfWork.Room.GetAll()
							select new
							{
								id = z.Id,
								name = z.Name,
								description = z.Description,
                                idlocation = z.IDLocation
							}).ToList().Where(i=>i.idlocation == LocationId).OrderByDescending(o => o.id);
			return Json(new { data = datalist });
		}

		#endregion
	}
}
