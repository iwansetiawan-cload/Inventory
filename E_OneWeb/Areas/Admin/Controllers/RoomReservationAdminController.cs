using E_OneWeb.Areas.Users.Controllers;
using E_OneWeb.DataAccess.Repository.IRepository;
using E_OneWeb.Models;
using E_OneWeb.Models.ViewModels;
using E_OneWeb.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NPOI.SS.Formula.Functions;
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
        public async Task<IActionResult> Index()
        {
            var countNotice = (from z in await _unitOfWork.VehicleReservationUser.GetAllAsync()
                               select new GridVehicleReservationAdmin
                               {
                                   id = z.Id,
                                   flag = z.Flag
                               }).Where(i => i.flag == 1).Count();

            if (countNotice > 0)
            {
                HttpContext.Session.SetInt32(SD.ssNotice, countNotice);
            }
            else
            {
                HttpContext.Session.SetString(SD.ssNotice, "o");
            }
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
            DateTime dtnow = DateTime.Now;
            var Getavailablelist = (from z in await _unitOfWork.RoomReservationUser.GetAllAsync(includeProperties: "RoomReservationAdmin")
                                              select new
                                              {
                                                  id = z.Id,
                                                  idadmin = z.RoomReservationAdmin.Id,
                                                  locationname = z.RoomReservationAdmin.LocationName,
                                                  startdate = z.StartDate,
                                                  enddate = z.EndDate,
                                                  status = z.Status,
                                                  statusid = z.StatusId,
                                                  bookingby = z.EntryBy,
                                                  flag = z.RoomReservationAdmin.Flag,
                                                  entrydate = z.EntryDate
                                              }).Where(o=> (o.startdate <= dtnow && o.enddate >= dtnow && o.statusid == 11) || (o.flag == 2 && o.statusid == 11)).ToList();

            List<int> listIdAdmin = Getavailablelist.Select(o => o.idadmin).ToList();

            if (status == "available")
            {
                var RoomReservationAdminlist = (from z in await _unitOfWork.RoomReservationAdmin.GetAllAsync(includeProperties: "Room")
                                                select new
                                                {
                                                    id = z.Id,
                                                    bookingid = z.BookingId,
                                                    roomname = z.RoomName,
                                                    locationname = z.LocationName,
                                                    status = "Ruangan Tersedia",
                                                    statusid = z.StatusId,
                                                    bookingby = z.BookingBy,
                                                    startdate = z.Flag == 2 ? Convert.ToDateTime(z.BookingStartDate).ToString("dd-MM-yyyy HH:mm") : "",
                                                    enddate = z.Flag == 2 ? Convert.ToDateTime(z.BookingEndDate).ToString("dd-MM-yyyy HH:mm") : "",
                                                    flag = 0,
                                                    flagval = z.Flag,
                                                    userid = "",
                                                    entrydate = ""
                                                }).Where(o => !listIdAdmin.Contains(o.id) && (o.flagval == 1 || o.flagval == 2)).ToList();
                return Json(new { data = RoomReservationAdminlist });
            }
             else if (status == "room_in_use")
            {
                var RoomReservationAdminlist = (from z in await _unitOfWork.RoomReservationAdmin.GetAllAsync(includeProperties: "Room")
                                                select new
                                                {
                                                    id = z.Id,
                                                    bookingid = z.BookingId,
                                                    roomname = z.RoomName,
                                                    locationname = z.LocationName,
                                                    status = "Ruangan Digunakan",
                                                    statusid = z.StatusId,
                                                    bookingby = Getavailablelist.Where(o=>o.idadmin == z.Id).Select(i => i.bookingby).FirstOrDefault(),
                                                    startdate = Convert.ToDateTime(Getavailablelist.Where(o => o.idadmin == z.Id).Select(i => i.startdate).FirstOrDefault()).ToString("dd-MM-yyyy HH:mm"),
                                                    enddate = Convert.ToDateTime(Getavailablelist.Where(o => o.idadmin == z.Id).Select(i => i.enddate).FirstOrDefault()).ToString("dd-MM-yyyy HH:mm"),
                                                    enddate_ = Getavailablelist.Where(o => o.idadmin == z.Id).Select(i => i.enddate).FirstOrDefault(),
                                                    flag = 1,
                                                    flagval = z.Flag,
                                                    userid = Getavailablelist.Where(o => o.idadmin == z.Id).Select(i => i.id).FirstOrDefault(),
                                                    entrydate = Getavailablelist.Where(o => o.idadmin == z.Id).Select(i => i.entrydate).FirstOrDefault(),
                                                }).Where(o => listIdAdmin.Contains(o.id) && o.enddate_ >= DateTime.Now).ToList();
                return Json(new { data = RoomReservationAdminlist });
              
            }
            else if (status == "waiting_approval")
            {
                var RoomReservationUserlist = (from z in await _unitOfWork.RoomReservationUser.GetAllAsync(includeProperties: "RoomReservationAdmin")
                                               select new
                                               {
                                                   id = z.Id,
                                                   bookingid = z.Id,
                                                   roomname = z.RoomReservationAdmin.RoomName,
                                                   locationname = z.RoomReservationAdmin.LocationName,
                                                   startdate = Convert.ToDateTime(z.StartDate).ToString("dd-MM-yyyy HH:mm"),
                                                   enddate = Convert.ToDateTime(z.EndDate).ToString("dd-MM-yyyy HH:mm"),
                                                   enddate_ = z.EndDate,
                                                   status = "Menunggu Persetujuan",
                                                   statusid = z.StatusId,
                                                   bookingby = z.EntryBy,
                                                   flag = 2,//_unitOfWork.Genmaster.GetAll().Where(i=>i.IDGEN == z.StatusId).Select(o=>o.GENVALUE).FirstOrDefault(),
                                                   flagval = z.RoomReservationAdmin.Flag,
                                                   userid = z.UserId,
                                                   entrydate = Convert.ToDateTime(z.EntryDate).ToString("dd-MM-yyyy HH:mm")
                                               }).Where(v=>v.statusid == 10 && v.enddate_ >= DateTime.Now ).ToList();
                return Json(new { data = RoomReservationUserlist });
            }
            else
            {          
                var RoomReservationUserlist = (from z in await _unitOfWork.RoomReservationUser.GetAllAsync(includeProperties: "RoomReservationAdmin")
                                               select new
                                               {
                                                   id = z.Id,
                                                   bookingid = z.Id,
                                                   roomname = z.RoomReservationAdmin.RoomName,
                                                   locationname = z.RoomReservationAdmin.LocationName,
                                                   startdate = Convert.ToDateTime(z.StartDate).ToString("dd-MM-yyyy HH:mm"),
                                                   enddate = Convert.ToDateTime(z.EndDate).ToString("dd-MM-yyyy HH:mm"),
                                                   status = z.StatusId == 11 ? "Disetujui" : "Ditolak",
                                                   statusid = z.StatusId,
                                                   bookingby = z.EntryBy,
                                                   flag = 3,
                                                   flagval = z.RoomReservationAdmin.Flag,
                                                   userid = z.UserId,
                                                   entrydate = Convert.ToDateTime(z.EntryDate).ToString("dd-MM-yyyy HH:mm")
                                               }).Where(v => v.statusid == 11 || v.statusid == 18).ToList();
                return Json(new { data = RoomReservationUserlist });
            }
           

            

            //switch (status)
            //{
            //    case "available":
            //        datalist = datalist.Where(o => o.flag == 0).ToList();
            //        break;
            //    case "waiting_approval":
            //        datalist = datalist.Where(o => o.flag == 1).ToList();
            //        break;
            //    case "room_in_use":
            //        datalist = datalist.Where(o => o.flag == 2).ToList();
            //        break;                
            //    default:
            //        break;
            //}

            
            //return Json(new { data = datalist });
        }

        [HttpPost]
        public async Task<IActionResult> Approve(int id)
        {
            var Gen_4 = _unitOfWork.Genmaster.GetAll().Where(z => z.GENFLAG == 4 && z.GENVALUE == 2).FirstOrDefault();
            int? IdGen4 = Gen_4 != null ? Convert.ToInt32(Gen_4.IDGEN) : 0;

            #region Update User
            RoomReservationUser RoomReservationUser = await _unitOfWork.RoomReservationUser.GetAsync(id);

            RoomReservationUser.StatusId = IdGen4;
            RoomReservationUser.Status = Gen_4.GENNAME;
            _unitOfWork.RoomReservationUser.Update(RoomReservationUser);
            #endregion

            #region Update Admin       
            RoomReservationAdmin roomReservationAdmin = await _unitOfWork.RoomReservationAdmin.GetAsync(RoomReservationUser.RoomAdminId);  
            roomReservationAdmin.StatusId = IdGen4;
            roomReservationAdmin.Status = Gen_4.GENNAME;
            _unitOfWork.RoomReservationAdmin.Update(roomReservationAdmin);
            #endregion

            
            _unitOfWork.Save();

            TempData["Success"] = "Room Reservation successfully approved";
            return Json(new { success = true, message = "Approved Successful" });

        }
        [HttpPost]
        public async Task<IActionResult> Rejected(string note, int id)
        {
            try
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                var user = _unitOfWork.ApplicationUser.GetFirstOrDefault(u => u.Id == claim.Value);

                var Gen_4 = _unitOfWork.Genmaster.GetAll().Where(z => z.GENFLAG == 4 && z.GENVALUE == 3).FirstOrDefault();
                int? IdGen4 = Gen_4 != null ? Convert.ToInt32(Gen_4.IDGEN) : 0;

                #region Update User
                RoomReservationUser RoomReservationUser = await _unitOfWork.RoomReservationUser.GetAsync(id);

                RoomReservationUser.StatusId = IdGen4;
                RoomReservationUser.Status = Gen_4.GENNAME;
                RoomReservationUser.Notes = note;
                RoomReservationUser.RejectedBy = user.UserName;
                RoomReservationUser.RejectedDate = DateTime.Now;
                _unitOfWork.RoomReservationUser.Update(RoomReservationUser);
                #endregion

                #region Update Admin       
                RoomReservationAdmin roomReservationAdmin = await _unitOfWork.RoomReservationAdmin.GetAsync(RoomReservationUser.RoomAdminId);
                roomReservationAdmin.StatusId = IdGen4;
                roomReservationAdmin.Status = Gen_4.GENNAME;
                _unitOfWork.RoomReservationAdmin.Update(roomReservationAdmin);
                #endregion

                TempData["Success"] = "Successfully reject";
                return Json(new { success = true, message = "Reject Successful" });
            }
            catch (Exception)
            {
                TempData["Failed"] = "Error reject";
                return Json(new { success = false, message = "Reject Error" });
            }


        }

        public async Task<IActionResult> ViewPersonal(int id)
        {
            try
            {
                RoomReservationUser roomReservationUser = await _unitOfWork.RoomReservationUser.GetAsync(id);
                if (roomReservationUser.RefFile != null)
                {
                    ViewBag.fileDownload = "true";
                }
                else
                {
                    ViewBag.fileDownload = "";
                }
                RoomReservationAdmin roomReservationAdmin = await _unitOfWork.RoomReservationAdmin.GetAsync(roomReservationUser.RoomAdminId);

                RoomReservationUserVM vm = new RoomReservationUserVM()
                {
                    RoomReservationUser = new RoomReservationUser()

                };

                vm.RoomReservationUser.RoomReservationAdmin = roomReservationAdmin;
                vm.RoomReservationUser = roomReservationUser;
                vm.StartDate = roomReservationUser.StartDate.Value.ToString("HH:mm");
                vm.EndDate = roomReservationUser.EndDate.Value.ToString("HH:mm");

                Personal personal = _unitOfWork.Personal.GetAll().Where(z => z.UserId == roomReservationUser.UserId).FirstOrDefault();
                vm.Personal = personal;

                if (personal.Photo != null)
                {
                    ViewBag.img = personal.Photo;
                    return View(vm);
                }
                else
                {
                    ViewBag.img = "";
                    return View(vm);
                }                
               
            }
            catch (Exception ex)
            {
                ViewBag.img = "";
                return View();
            }
          

        }

        #endregion

        public async Task<IActionResult> Export(string id)
        {

            int idheader = id != null ? Convert.ToInt32(id) : 0;
            var objFromDb = await _unitOfWork.RoomReservationUser.GetAsync(idheader);

            string filename = objFromDb.RefFile;
            string filePath = "wwwroot\\images\\files";
            var memory = DownloadSinghFile(filename, filePath);
            var contentType = "APPLICATION/octet-stream";
            return File(memory.ToArray(), contentType, filename);
            //return File(memory.ToArray(), contentType, Path.GetFileName(filePath));

        }
        private MemoryStream DownloadSinghFile(string filename, string uploadPath)
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), uploadPath, filename);
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
    }

}
