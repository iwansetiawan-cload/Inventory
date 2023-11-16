using E_OneWeb.DataAccess.Repository.IRepository;
using E_OneWeb.Models;
using E_OneWeb.Models.ViewModels;
using E_OneWeb.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace E_OneWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]

    public class RoomController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public RoomController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            RoomVM roomVM = new RoomVM()
            {
                Rooms = _unitOfWork.Room.GetAll()
            };
            var count = roomVM.Rooms.Count();
            return View(roomVM);
        }

        public IActionResult Upsert(int? id)
        {
            Room room = new Room();
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
            return View(room);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(Room room)
        {
            if (ModelState.IsValid)
            {
                if (room.Id == 0)
                {
                    _unitOfWork.Room.Add(room);

                }
                else
                {
                    _unitOfWork.Room.Update(room);
                }
                _unitOfWork.Save();
                return RedirectToAction(nameof(Index));
            }
            return View(room);
        }

        #region API CALLS

        [HttpGet]
        public IActionResult GetAll()
        {
            var allObj = _unitOfWork.Room.GetAll();
            return Json(new { data = allObj });
        }

        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var objFromDb = _unitOfWork.Room.Get(id);
            if (objFromDb == null)
            {
                TempData["Error"] = "Error deleting Category";
                return Json(new { success = false, message = "Error while deleting" });
            }
            _unitOfWork.Room.Remove(objFromDb);
            _unitOfWork.Save();

            TempData["Success"] = "Category successfully deleted";
            return Json(new { success = true, message = "Delete Successful" });

        }

        #endregion
    }
}
