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
                                              }).Where(o=>o.startdate <= dtnow && o.enddate >= dtnow && o.statusid == 11).ToList();

            List<int> listIdAdmin = Getavailablelist.Select(o => o.idadmin).ToList();

            if (status == "available")
            {
                var RoomReservationAdminlist = (from z in await _unitOfWork.RoomReservationAdmin.GetAllAsync(includeProperties: "Room")
                                                select new
                                                {
                                                    id = z.Id,
                                                    roomname = z.RoomName,
                                                    locationname = z.LocationName,
                                                    status = "Room available",
                                                    statusid = z.StatusId,
                                                    bookingby = z.BookingBy,
                                                    startdate = "",
                                                    enddate = "",
                                                    flag = 0,
                                                    userid = ""
                                                }).Where(o => !listIdAdmin.Contains(o.id)).ToList();
                return Json(new { data = RoomReservationAdminlist });
            }
             else if (status == "room_in_use")
            {
                var RoomReservationAdminlist = (from z in await _unitOfWork.RoomReservationAdmin.GetAllAsync(includeProperties: "Room")
                                                select new
                                                {
                                                    id = z.Id,
                                                    roomname = z.RoomName,
                                                    locationname = z.LocationName,
                                                    status = "Room in use",
                                                    statusid = z.StatusId,
                                                    bookingby = Getavailablelist.Where(o=>o.idadmin == z.Id).Select(i => i.bookingby).FirstOrDefault(),
                                                    startdate = Convert.ToDateTime(Getavailablelist.Where(o => o.idadmin == z.Id).Select(i => i.startdate).FirstOrDefault()).ToString("dd-MM-yyyy HH:mm"),
                                                    enddate = Convert.ToDateTime(Getavailablelist.Where(o => o.idadmin == z.Id).Select(i => i.enddate).FirstOrDefault()).ToString("dd-MM-yyyy HH:mm"),
                                                    flag = 1,
                                                    userid = Getavailablelist.Where(o => o.idadmin == z.Id).Select(i => i.id).FirstOrDefault()
                                                }).Where(o => listIdAdmin.Contains(o.id)).ToList();
                return Json(new { data = RoomReservationAdminlist });
                //var RoomReservationUserlist = (from z in await _unitOfWork.RoomReservationUser.GetAllAsync(includeProperties: "RoomReservationAdmin")
                //                               select new
                //                               {
                //                                   id = z.Id,
                //                                   roomname = z.RoomReservationAdmin.RoomName,
                //                                   locationname = z.RoomReservationAdmin.LocationName,
                //                                   startdate = Convert.ToDateTime(z.StartDate).ToString("dd-MM-yyyy HH:mm"),
                //                                   enddate = Convert.ToDateTime(z.EndDate).ToString("dd-MM-yyyy HH:mm"),
                //                                   status = z.Status,
                //                                   statusid = z.StatusId,
                //                                   bookingby = z.EntryBy,
                //                                   flag = _unitOfWork.Genmaster.GetAll().Where(i => i.IDGEN == z.StatusId).Select(o => o.GENVALUE).FirstOrDefault(),
                //                                   userid = z.UserId
                //                               }).Where(v => v.flag == 2).ToList();
                //return Json(new { data = RoomReservationUserlist });
            }
            else if (status == "waiting_approval")
            {
                var RoomReservationUserlist = (from z in await _unitOfWork.RoomReservationUser.GetAllAsync(includeProperties: "RoomReservationAdmin")
                                               select new
                                               {
                                                   id = z.Id,
                                                   roomname = z.RoomReservationAdmin.RoomName,
                                                   locationname = z.RoomReservationAdmin.LocationName,
                                                   startdate = Convert.ToDateTime(z.StartDate).ToString("dd-MM-yyyy HH:mm"),
                                                   enddate = Convert.ToDateTime(z.EndDate).ToString("dd-MM-yyyy HH:mm"),
                                                   status = z.Status,
                                                   statusid = z.StatusId,
                                                   bookingby = z.EntryBy,
                                                   flag = 2,//_unitOfWork.Genmaster.GetAll().Where(i=>i.IDGEN == z.StatusId).Select(o=>o.GENVALUE).FirstOrDefault(),
                                                   userid = z.UserId
                                               }).Where(v=>v.statusid == 10).ToList();
                return Json(new { data = RoomReservationUserlist });
            }           
            else
            {
                var RoomReservationUserlist = (from z in await _unitOfWork.RoomReservationUser.GetAllAsync(includeProperties: "RoomReservationAdmin")
                                               select new
                                               {
                                                   id = z.Id,
                                                   roomname = z.RoomReservationAdmin.RoomName,
                                                   locationname = z.RoomReservationAdmin.LocationName,
                                                   startdate = Convert.ToDateTime(z.StartDate).ToString("dd-MM-yyyy HH:mm"),
                                                   enddate = Convert.ToDateTime(z.EndDate).ToString("dd-MM-yyyy HH:mm"),
                                                   status = z.Status,
                                                   statusid = z.StatusId,
                                                   bookingby = z.EntryBy,
                                                   flag = 3,
                                                   userid = z.UserId
                                               }).Where(v => v.statusid == 11).ToList();
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
            var Gen_5 = _unitOfWork.Genmaster.GetAll().Where(z => z.GENFLAG == 5 && z.GENVALUE == 2).FirstOrDefault();

            int? IdGen4 = Gen_4 != null ? Convert.ToInt32(Gen_4.IDGEN) : 0;
            int? IdGen5 = Gen_5 != null ? Convert.ToInt32(Gen_5.IDGEN) : 0;

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
            roomReservationAdmin.Flag = Convert.ToInt32(Gen_4.GENVALUE);
            _unitOfWork.RoomReservationAdmin.Update(roomReservationAdmin);
            #endregion

            
            _unitOfWork.Save();

            TempData["Success"] = "Room Reservation successfully approved";
            return Json(new { success = true, message = "Approved Successful" });

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
