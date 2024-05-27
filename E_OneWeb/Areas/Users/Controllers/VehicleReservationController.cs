using E_OneWeb.Models.ViewModels;
using E_OneWeb.Models;
using E_OneWeb.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using E_OneWeb.DataAccess.Repository.IRepository;
using javax.print.attribute.standard;
using System.Globalization;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace E_OneWeb.Areas.Users.Controllers
{
    [Area("Users")]
    [Authorize(Roles = SD.Role_User)]
    public class VehicleReservationController : Controller
    {
        private readonly ILogger<VehicleReservationController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _hostEnvironment;

        public VehicleReservationController(ILogger<VehicleReservationController> logger, IUnitOfWork unitOfWork, IWebHostEnvironment hostEnvironment)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _hostEnvironment = hostEnvironment;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> GetAllItems()
        {     
            var datalist = (from z in await _unitOfWork.VehicleReservationAdmin.GetAllAsync(includeProperties: "Items")
                                 select new 
                                 {
                                     id = z.Id,
                                     code = z.Items.Code,
                                     name = z.ItemName,
                                     desc = z.Items.Description,
                                     roomname = z.RoomName,
                                     flag = z.Flag,
                                 }).Where(x => x.flag == 0).ToList();


            return Json(new { data = datalist });
        }
        [HttpGet]
        public async Task<IActionResult> GetBookingListUser(int id)
        {
            try
            {               
                var datalist = (from z in await _unitOfWork.VehicleReservationUser.GetAllAsync(includeProperties: "VehicleReservationAdmin")
                                select new 
                                {
                                    id = z.Id,
                                    bookingid = z.BookingId,
                                    itemname = z.VehicleReservationAdmin.ItemName,                           
                                    booking_date = z.BookingStartDate != null ? z.BookingStartDate.Value.ToString("dd/MM/yyyy") : "",
                                    booking_clock = z.BookingStartDate != null ? z.BookingStartDate.Value.ToString("HH:mm") + "-" + z.BookingEndDate.Value.ToString("HH:mm") : "",
                                    bookingenddate = z.BookingEndDate,
                                    utility = z.Utilities,
                                    destination = z.Destination,
                                    driver = z.DriverName
                                }).Where(o => o.bookingid == id && o.bookingenddate >= DateTime.Now).ToList();


                return Json(new { data = datalist });
            }
            catch (Exception ex)
            {
                string err = ex.Message.ToString();
                return Json(new { data = "" });

            }



        }
        public async Task<IActionResult> Create()
        {
            ViewBag.Status = "";
            ViewBag.Reason = "";
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var user = _unitOfWork.ApplicationUser.GetFirstOrDefault(u => u.Id == claim.Value);

            var listDrivers = _unitOfWork.Driver.GetAll().Select(x => new SelectListItem { Value = x.Id.ToString(), Text = x.Name });
            //ViewBag.listDrivers = new SelectList(listDrivers, "Value", "Text");
            VehicleReservationUserVM vm = new VehicleReservationUserVM()
            {
                VehicleReservationUser = new VehicleReservationUser(),
                ListDriver = listDrivers.Select(i => new SelectListItem
                {
                    Text = i.Text,
                    Value = i.Value
                }),
            };

            vm.VehicleReservationUser.UserId = user.Id;
            vm.VehicleReservationUser.EntryBy = user.Name;
            vm.VehicleReservationUser.EntryDate = DateTime.Now;
            vm.StartDate = DateTime.Now.ToString("HH:mm dd/MM/yyyy");
            vm.EndDate = DateTime.Now.ToString("HH:mm dd/MM/yyyy");
            vm.VehicleReservationUser.BookingStartDate = DateTime.Now;
            vm.VehicleReservationUser.BookingEndDate = DateTime.Now;
            vm.VehicleReservationUser.Phone = user.PhoneNumber;
			ViewBag.ClockStart = DateTime.Now.ToString("HH:mm");
            ViewBag.ClockEnd = DateTime.Now.ToString("HH:mm");

            return View(vm);

        }
        public string StatusMessage { get; set; }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(VehicleReservationUserVM vm)
        {
            var Gen_5 = _unitOfWork.Genmaster.GetAll().Where(z => z.GENFLAG == 5 && z.GENVALUE == 1).FirstOrDefault();
            var listDrivers = _unitOfWork.Driver.GetAll().Select(x => new SelectListItem { Value = x.Id.ToString(), Text = x.Name });
            vm.ListDriver = listDrivers;

            DateTime orderDate = Convert.ToDateTime(vm.VehicleReservationUser.BookingStartDate);
            TimeSpan orderTime = vm.ClockStart;
            DateTime orderDateTimeStart = orderDate + orderTime;

            DateTime orderDateEnd = Convert.ToDateTime(vm.VehicleReservationUser.BookingStartDate);
            TimeSpan orderTimeEnd = vm.ClockEnd;
            DateTime orderDateTimeEnd = orderDate + orderTimeEnd;

            ViewBag.ClockStart = orderDateTimeStart.ToString("HH:mm");
            ViewBag.ClockEnd = orderDateTimeEnd.ToString("HH:mm");
           
            if (orderDateTimeEnd < DateTime.Now)
            {
                ViewBag.Status = "Error";
                ViewBag.Reason = "Tanggal dan jam peminjaman harus lebih besar dari waktu saat ini";
                return View(vm);
            }
          

            if (vm.VehicleReservationUser.VehicleReservationAdmin.Id == 0)
            {
                ViewBag.Status = "Error";
                ViewBag.Reason = "Kendaraan harus dipilih";
                return View(vm);
            }          

            if (vm.VehicleReservationUser.Id == 0)
            {
                VehicleReservationAdmin vehicleReservationAdmin = await _unitOfWork.VehicleReservationAdmin.GetAsync(vm.VehicleReservationUser.VehicleReservationAdmin.Id);
                vm.VehicleReservationUser.VehicleReservationAdmin = vehicleReservationAdmin;
                var dataBooking = (from z in await _unitOfWork.VehicleReservationUser.GetAllAsync(includeProperties: "VehicleReservationAdmin")
                                   select new
                                   {
                                       id = z.Id,
                                       bookingid = z.BookingId,
                                       bookingstartdate = z.BookingStartDate,
                                       bookingenddate = z.BookingEndDate
                                   }).Where(o => o.bookingid == vehicleReservationAdmin.Id && o.bookingenddate >= DateTime.Now
                                   && ((orderDateTimeStart >= o.bookingstartdate && orderDateTimeStart < o.bookingenddate) || (orderDateTimeEnd > o.bookingstartdate && orderDateTimeEnd <= o.bookingenddate) || (orderDateTimeStart <= o.bookingstartdate && orderDateTimeEnd >= o.bookingenddate))).ToList();

                if (dataBooking.Count() > 0)
                {
                    ViewBag.Status = "Error";
                    ViewBag.Reason = "Kendaraan sudah di pinjam dari tanggal: " + dataBooking.FirstOrDefault().bookingstartdate.Value.ToString("dd/MM/yyyy") + " jam: " + dataBooking.FirstOrDefault().bookingstartdate.Value.ToString("HH:mm") + "-" + dataBooking.FirstOrDefault().bookingenddate.Value.ToString("HH:mm");
                    return View(vm);
                }
                if (vm.FlagDriver == "1")
                {
                    ViewBag.flagdrivers = "1";
                    vm.VehicleReservationUser.DriverName = listDrivers.Where(z => z.Value == vm.VehicleReservationUser.DriverId.ToString()).FirstOrDefault().Text;
                    var valDriver = (from z in await _unitOfWork.VehicleReservationUser.GetAllAsync(includeProperties: "VehicleReservationAdmin")
                                     select new
                                     {
                                         id = z.Id,
                                         bookingid = z.BookingId,
                                         bookingstartdate = z.BookingStartDate,
                                         bookingenddate = z.BookingEndDate,
                                         driverid = z.DriverId,
                                     }).Where(o => o.driverid == vm.VehicleReservationUser.DriverId && o.bookingenddate >= DateTime.Now
                                     && ((orderDateTimeStart >= o.bookingstartdate && orderDateTimeStart < o.bookingenddate) || (orderDateTimeEnd > o.bookingstartdate && orderDateTimeEnd <= o.bookingenddate) || (orderDateTimeStart <= o.bookingstartdate && orderDateTimeEnd >= o.bookingenddate))).ToList();
                    if (valDriver.Count() > 0)
                    {
                        ViewBag.Status = "Error";
                        ViewBag.Reason = "Supir sedang bertugas pada tanggal: " + valDriver.FirstOrDefault().bookingstartdate.Value.ToString("dd/MM/yyyy") + " jam: " + valDriver.FirstOrDefault().bookingstartdate.Value.ToString("HH:mm") + "-" + valDriver.FirstOrDefault().bookingenddate.Value.ToString("HH:mm");
                        return View(vm);
                    }

                }
                else
                {
                    ViewBag.flagdrivers = "0";
                    vm.VehicleReservationUser.DriverId = null;
                }


				vm.VehicleReservationUser.StatusId = Gen_5.IDGEN;
                vm.VehicleReservationUser.Status = Gen_5.GENNAME;
                vm.VehicleReservationUser.Flag = (int)Gen_5.GENVALUE;
                vm.VehicleReservationUser.BookingStartDate = orderDateTimeStart;
                vm.VehicleReservationUser.BookingEndDate = orderDateTimeEnd;              
				
                await _unitOfWork.VehicleReservationUser.AddAsync(vm.VehicleReservationUser);
                _unitOfWork.Save();

                //vehicleReservationAdmin.BookingId = vm.VehicleReservationUser.Id;
                //vehicleReservationAdmin.BookingBy = vm.VehicleReservationUser.EntryBy;
                //vehicleReservationAdmin.BookingStartDate = vm.VehicleReservationUser.BookingStartDate;
                //vehicleReservationAdmin.BookingEndDate = vm.VehicleReservationUser.BookingEndDate;
                //vehicleReservationAdmin.StatusId = Gen_5.IDGEN;
                //vehicleReservationAdmin.Status = Gen_5.GENNAME;
                //vehicleReservationAdmin.Flag = (int)Gen_5.GENVALUE;
                //_unitOfWork.VehicleReservationAdmin.Update(vehicleReservationAdmin);
                //_unitOfWork.Save();
                ViewBag.Status = "Success";
                ViewBag.Reason = "Berhasil peminjaman kendaraan";

            }

            //TempData["Success"] = "Save successfully";
            return RedirectToAction("Index", "RoomReservation", new { area = "Users" });
            //return View(vm);

        }
        
    }
}
