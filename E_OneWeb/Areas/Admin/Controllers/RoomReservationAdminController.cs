using E_OneWeb.Areas.Users.Controllers;
using E_OneWeb.DataAccess.Repository.IRepository;
using E_OneWeb.Models;
using E_OneWeb.Models.ViewModels;
using E_OneWeb.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data;
using System.Security.Claims;

namespace E_OneWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
    public class RoomReservationAdminController : Controller
    {
        private readonly ILogger<RoomReservationAdminController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public RoomReservationAdminController(ILogger<RoomReservationAdminController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
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
                                bookingdate = z.BookingStartDate != null ? Convert.ToDateTime(z.BookingStartDate).ToString("dd-MM-yyyy HH:mm") : "",
                                bookingenddate = z.BookingEndDate != null ? Convert.ToDateTime(z.BookingEndDate).ToString("dd-MM-yyyy HH:mm") : ""
                            }).ToList().OrderByDescending(o => o.id);

            return Json(new { data = datalist });
        }

        #region API CALLS
        [HttpGet]
        public async Task<IActionResult> GetOrderList(string status)
        {
            //var claimsIdentity = (ClaimsIdentity)User.Identity;
            //var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            //IEnumerable<RoomReservationAdmin> orderHeaderList;

            //if (User.IsInRole(SD.Role_Admin) || User.IsInRole(SD.Role_Employee))
            //{
            //    orderHeaderList = _unitOfWork.RoomReservationAdmin.GetAll(includeProperties: "ApplicationUser");
            //}
            //else
            //{
            //    orderHeaderList = _unitOfWork.OrderHeader.GetAll(
            //                            u => u.ApplicationUserId == claim.Value,
            //                            includeProperties: "ApplicationUser");
            //}
            var RoomReservationUserlist = (from z in await _unitOfWork.RoomReservationUser.GetAllAsync(includeProperties: "RoomReservationAdmin")
                            select new
                            {
                                id = z.Id,
                                roomname = z.RoomReservationAdmin.RoomName + " (" + z.RoomReservationAdmin.LocationName + ")",
                                startdate = Convert.ToDateTime(z.StartDate).ToString("dd-MM-yyyy HH:mm"),
                                enddate = Convert.ToDateTime(z.EndDate).ToString("dd-MM-yyyy HH:mm"),
                                status = z.Status,
                                statusid = z.StatusId,
                                description = z.Description,
                                entryby = z.EntryBy,
                                userid = z.UserId
                            }).ToList();

            var datalist = (from z in await _unitOfWork.RoomReservationAdmin.GetAllAsync(includeProperties: "Room")
                            select new
                            {
                                id = z.Id,
                                roomname = z.RoomName,
                                locationname = z.LocationName,
                                status = z.Status,
                                statusid = z.StatusId,
                                bookingby = z.BookingBy,
                                startdate = RoomReservationUserlist.Where(i => i.id == z.BookingId).Count() > 0 ? RoomReservationUserlist.Where(i => i.id == z.BookingId).Select(i => i.startdate).FirstOrDefault() : "",
                                enddate = RoomReservationUserlist.Where(i => i.id == z.BookingId).Count() > 0 ? RoomReservationUserlist.Where(i=>i.id == z.BookingId).Select(i=> i.enddate).FirstOrDefault() : "",
                                flag = z.Flag,
                                userid = RoomReservationUserlist.Where(i => i.id == z.BookingId).Count() > 0 ? RoomReservationUserlist.Where(i => i.id == z.BookingId).Select(i => i.userid).FirstOrDefault() : ""
                            }).Where(o=>o.flag != null).ToList();

            switch (status)
            {
                case "available":
                    datalist = datalist.Where(o => o.flag == 0).ToList();
                    break;
                case "waiting_approval":
                    datalist = datalist.Where(o => o.flag == 1).ToList();
                    break;
                case "room_in_use":
                    datalist = datalist.Where(o => o.flag == 2).ToList();
                    break;                
                default:
                    break;
            }

            
            return Json(new { data = datalist });
        }

        [HttpPost]
        public async Task<IActionResult> Approve(int id)
        {
            var Gen_4 = _unitOfWork.Genmaster.GetAll().Where(z => z.GENFLAG == 4 && z.GENVALUE == 2).FirstOrDefault();
            var Gen_5 = _unitOfWork.Genmaster.GetAll().Where(z => z.GENFLAG == 5 && z.GENVALUE == 2).FirstOrDefault();

            int? IdGen4 = Gen_4 != null ? Convert.ToInt32(Gen_4.IDGEN) : 0;
            int? IdGen5 = Gen_5 != null ? Convert.ToInt32(Gen_5.IDGEN) : 0;

            #region Update Admin       
            RoomReservationAdmin roomReservationAdmin = await _unitOfWork.RoomReservationAdmin.GetAsync(id);  
            roomReservationAdmin.StatusId = IdGen4;
            roomReservationAdmin.Status = Gen_4.GENNAME;
            roomReservationAdmin.Flag = Convert.ToInt32(Gen_4.GENVALUE);
            _unitOfWork.RoomReservationAdmin.Update(roomReservationAdmin);
            #endregion

            #region Update User
            var RoomReservationUserList = await _unitOfWork.RoomReservationUser.GetAllAsync(includeProperties: "RoomReservationAdmin");
            RoomReservationUser RoomReservationUser = RoomReservationUserList.Where(z => z.RoomAdminId == roomReservationAdmin.Id).FirstOrDefault();
            RoomReservationUser.StatusId = IdGen5;
            RoomReservationUser.Status = Gen_5.GENNAME;
            _unitOfWork.RoomReservationUser.Update(RoomReservationUser);
            #endregion
            
            _unitOfWork.Save();

            TempData["Success"] = "Room Reservation successfully approved";
            return Json(new { success = true, message = "Approved Successful" });

        }

        public async Task<IActionResult> ViewPersonal(string id)
        {
            try
            {
                Personal personal = _unitOfWork.Personal.GetAll().Where(z => z.UserId == id).FirstOrDefault();
                if (personal != null)
                {
                    ViewBag.img = personal.Photo;
                    return View(personal);
                }
                else
                {
                    ViewBag.img = "";
                    return View(personal);
                }                
               
            }
            catch (Exception ex)
            {
                ViewBag.img = "";
                return View();
            }
          

        }

        #endregion
    }

}
