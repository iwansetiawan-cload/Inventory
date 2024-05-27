using E_OneWeb.DataAccess.Repository.IRepository;
using E_OneWeb.Models;
using E_OneWeb.Models.ViewModels;
using E_OneWeb.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sun.misc;
using System.Security.Claims;
using static com.sun.tools.@internal.xjc.reader.xmlschema.bindinfo.BIConversion;

namespace E_OneWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
    public class VehicleBookingController : Controller
    {
        private readonly ILogger<VehicleBookingController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public VehicleBookingController(ILogger<VehicleBookingController> logger, IUnitOfWork unitOfWork)
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
        public async Task<IActionResult> GetOrderList(string status)
        {
            List<GridVehicleReservationAdmin> Addlist = new List<GridVehicleReservationAdmin>();
            DateTime dtnow = DateTime.Now;
            var datalistadmin = (from z in await _unitOfWork.VehicleReservationAdmin.GetAllAsync(includeProperties: "Items")
                                    select new GridVehicleReservationAdmin
                                    {
                                        id = z.Id,
                                        code = z.Items.Code,
                                        name = z.ItemName,
                                        idadmin = z.Id,
                                        locationname = z.LocationName,
                                        startdate = z.BookingStartDate,
                                        enddate = z.BookingEndDate,
                                        bookingdate = z.BookingStartDate != null ? z.BookingStartDate.Value.ToString("dd/MM/yyyy") : "",
                                        bookingclock = z.BookingStartDate != null ? z.BookingStartDate.Value.ToString("HH:mm") + "-" + z.BookingEndDate.Value.ToString("HH:mm") : "",
                                        status = z.Status,
                                        statusid = z.StatusId,
                                        bookingid = z.BookingId,
                                        bookingby = z.BookingBy, 
                                        flag = z.Flag,
                                        driver = ""
                                    }).Where(x=>x.flag != null).ToList();

            var datalistuser = (from z in await _unitOfWork.VehicleReservationUser.GetAllAsync(includeProperties: "VehicleReservationAdmin")
                            select new GridVehicleReservationAdmin
                            {
                                id = z.Id,
                                code = z.VehicleReservationAdmin.Items.Code,
                                name = z.VehicleReservationAdmin.ItemName,
                                idadmin = z.VehicleReservationAdmin.Id,
                                locationname = z.VehicleReservationAdmin.LocationName,
                                startdate = z.BookingStartDate,
                                enddate = z.BookingEndDate,
                                bookingdate = z.BookingStartDate != null ? z.BookingStartDate.Value.ToString("dd/MM/yyyy") : "",
                                bookingclock = z.BookingStartDate != null ? z.BookingStartDate.Value.ToString("HH:mm") + "-" + z.BookingEndDate.Value.ToString("HH:mm") : "",
                                status = z.Status,
                                statusid = z.StatusId,
                                bookingid = z.Id,
                                bookingby = z.EntryBy,
                                flag = z.Flag,
                                driver = z.DriverName
                            }).ToList();

            //Addlist.AddRange(datalistadmin);
            Addlist.AddRange(datalistuser);
            switch (status)
            {
                case "available":
                    Addlist = datalistadmin.ToList();
                    break;
                case "waiting_approval":
                    Addlist = Addlist.Where(o => o.flag == 1).ToList();
                    break;
                case "room_in_use":
                    Addlist = Addlist.Where(o => o.flag == 2 && o.startdate <= DateTime.Now && o.enddate >= DateTime.Now).ToList();
                    break;
                default:
                    Addlist = Addlist.Where(o => o.flag == 2 || o.flag == 3).ToList();
                    break;
            }
            return Json(new { data = Addlist });
        }

        public async Task<IActionResult> ViewPersonal(int id)
        {
            try
            {
                VehicleReservationUser vehicleReservationUser = await _unitOfWork.VehicleReservationUser.GetAsync(id);

                VehicleReservationAdmin vehicleReservationAdmin = await _unitOfWork.VehicleReservationAdmin.GetAsync(vehicleReservationUser.BookingId);

                VehicleReservationUserVM vm = new VehicleReservationUserVM()
                {
                    VehicleReservationUser = new VehicleReservationUser()

                };

                vm.VehicleReservationUser.VehicleReservationAdmin = vehicleReservationAdmin;
                vm.VehicleReservationUser = vehicleReservationUser;
                vm.StartDate = vehicleReservationUser.BookingStartDate.Value.ToString("HH:mm");
                vm.EndDate = vehicleReservationUser.BookingEndDate.Value.ToString("HH:mm");

                Personal personal = _unitOfWork.Personal.GetAll().Where(z => z.UserId == vehicleReservationUser.UserId).FirstOrDefault();
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
        [HttpPost]
        public async Task<IActionResult> Approve(int id)
        {
            try
            {
				var Gen_5 = _unitOfWork.Genmaster.GetAll().Where(z => z.GENFLAG == 5 && z.GENVALUE == 2).FirstOrDefault();
				int? IdGen5 = Gen_5 != null ? Convert.ToInt32(Gen_5.IDGEN) : 0;
				var claimsIdentity = (ClaimsIdentity)User.Identity;
				var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
				var user = _unitOfWork.ApplicationUser.GetFirstOrDefault(u => u.Id == claim.Value);
				#region Update User
				VehicleReservationUser vehicleReservationUser = await _unitOfWork.VehicleReservationUser.GetAsync(id);

				vehicleReservationUser.StatusId = IdGen5;
				vehicleReservationUser.Status = Gen_5.GENNAME;
				vehicleReservationUser.Flag = (int)Gen_5.GENVALUE;
				vehicleReservationUser.ApproveBy = user.Name;
				vehicleReservationUser.ApproveDate = DateTime.Now;
				vehicleReservationUser.RejectedBy = null;
				vehicleReservationUser.RejectedDate = null;
				vehicleReservationUser.NotesReject = null;
				_unitOfWork.VehicleReservationUser.Update(vehicleReservationUser);
				#endregion


				_unitOfWork.Save();

				TempData["Success"] = "Pinjam kendaraan berhasil disetujui";
				return Json(new { success = true, message = "Berhasil Disetujui" });
			}
            catch (Exception)
            {
				TempData["Failed"] = "Error Approved";
				return Json(new { success = false, message = "Gagal Disetujui" });
			}
            

        }
        [HttpPost]
        public async Task<IActionResult> Rejected(string note, int id)
        {
            try
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                var user = _unitOfWork.ApplicationUser.GetFirstOrDefault(u => u.Id == claim.Value);

                var Gen_5 = _unitOfWork.Genmaster.GetAll().Where(z => z.GENFLAG == 5 && z.GENVALUE == 3).FirstOrDefault();
                int? IdGen4 = Gen_5 != null ? Convert.ToInt32(Gen_5.IDGEN) : 0;

                #region Update User
                VehicleReservationUser vehicleReservationUser = await _unitOfWork.VehicleReservationUser.GetAsync(id);

                vehicleReservationUser.StatusId = IdGen4;
                vehicleReservationUser.Status = Gen_5.GENNAME;
                vehicleReservationUser.Flag = (int)Gen_5.GENVALUE;
                vehicleReservationUser.NotesReject = note;
                vehicleReservationUser.RejectedBy = user.UserName;
                vehicleReservationUser.RejectedDate = DateTime.Now;
                vehicleReservationUser.ApproveBy = null;
                vehicleReservationUser.ApproveDate = null;
                _unitOfWork.VehicleReservationUser.Update(vehicleReservationUser);
                #endregion             

                TempData["Success"] = "Successfully reject";
                return Json(new { success = true, message = "Berhasil Ditolak" });
            }
            catch (Exception)
            {
                TempData["Failed"] = "Error reject";
                return Json(new { success = false, message = "Gagal Ditolak" });
            }


        }
    }
}
