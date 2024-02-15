using E_OneWeb.DataAccess.Repository;
using E_OneWeb.DataAccess.Repository.IRepository;
using E_OneWeb.Models;
using E_OneWeb.Models.ViewModels;
using E_OneWeb.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Diagnostics;
using System.Security.Claims;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace E_OneWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
    public class ClassListController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public ClassListController( IUnitOfWork unitOfWork)
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

            var datalist = (from z in await _unitOfWork.RoomReservationAdmin.GetAllAsync(includeProperties: "Room")
                                            select new
                                            {
                                                id = z.Id,
                                                roomname = z.RoomName,
                                                locationname = z.LocationName,
                                                status = z.Status,
                                                statusid = z.StatusId,
                                                bookingby = z.BookingBy,
                                                flag = z.Flag,
                                                bookingdate = z.BookingStartDate != null ? Convert.ToDateTime(z.BookingStartDate).ToString("dd-MM-yyyy HH:mm") : "",
                                                bookingenddate = z.BookingEndDate != null ? Convert.ToDateTime(z.BookingEndDate).ToString("dd-MM-yyyy HH:mm") : ""
                                            }).Where(u=>u.flag == 2).ToList();

            return Json(new { data = datalist });
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

        public async Task<IActionResult> Upsert(int? id)
        {
            //IEnumerable<Items> ItemsList = await _unitOfWork.Items.GetAllAsync();          
            RoomReservationAdminVM vm = new RoomReservationAdminVM()
            {
                RoomReservationAdmin = new RoomReservationAdmin(),
                RoomList = _unitOfWork.Room.GetAll().Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                })
           
            };
            ViewBag.Status = "";
            if (id == null)
            {
                //this is for create
                vm.RoomReservationAdmin.BookingStartDate = DateTime.Now;
                vm.RoomReservationAdmin.BookingEndDate = DateTime.Now;

                return View(vm);
            }

            vm.RoomReservationAdmin = await _unitOfWork.RoomReservationAdmin.GetAsync(id.GetValueOrDefault());
            if (vm.RoomReservationAdmin == null)
            {
                return NotFound();
            }

          
            return View(vm);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(RoomReservationAdminVM vm)
        {
            //var errorval = ModelState.Values.SelectMany(i => i.Errors);
            //if (ModelState.IsValid)
            //{
                
            //}
            var Gen_4 = _unitOfWork.Genmaster.GetAll().Where(z => z.GENFLAG == 4 && z.GENVALUE == 0).FirstOrDefault();
            RoomReservationAdmin roomReservationAdmin = await _unitOfWork.RoomReservationAdmin.GetAsync(vm.RoomReservationAdmin.Id);
            Room room = _unitOfWork.Room.Get(vm.RoomReservationAdmin.RoomId);
            vm.RoomReservationAdmin.Room = room;

            DateTime orderDate = Convert.ToDateTime(vm.RoomReservationAdmin.BookingStartDate);
            TimeSpan orderTime = vm.ClockStart;
            DateTime orderDateTimeStart = orderDate + orderTime;

            DateTime orderDateEnd = Convert.ToDateTime(vm.RoomReservationAdmin.BookingEndDate);
            TimeSpan orderTimeEnd = vm.ClockEnd;
            DateTime orderDateTimeEnd = orderDateEnd + orderTimeEnd;

            vm.RoomReservationAdmin.StatusId = Gen_4.IDGEN;
            vm.RoomReservationAdmin.Status = Gen_4.GENNAME;
            vm.RoomReservationAdmin.Flag = 2;
            vm.RoomReservationAdmin.BookingStartDate = orderDateTimeStart;
            vm.RoomReservationAdmin.BookingEndDate = orderDateTimeEnd;

            if (vm.RoomReservationAdmin.Id == 0)
            {
                await _unitOfWork.RoomReservationAdmin.AddAsync(vm.RoomReservationAdmin);
                ViewBag.Status = "Save Success";

            }
            else
            {
                _unitOfWork.RoomReservationAdmin.Update(vm.RoomReservationAdmin);
                ViewBag.Status = "Edit Success";
            }

            _unitOfWork.Save();

            //return View(vm);
            return RedirectToAction(nameof(Index));
        }
        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var objFromDb = await _unitOfWork.RoomReservationAdmin.GetAsync(id);
            if (objFromDb == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }

            await _unitOfWork.RoomReservationAdmin.RemoveAsync(objFromDb);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Delete Successful" });

        }
    }
}
