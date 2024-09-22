using E_OneWeb.DataAccess.Repository.IRepository;
using E_OneWeb.Models.ViewModels;
using E_OneWeb.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using E_OneWeb.Utility;
using Microsoft.AspNetCore.Authorization;
using System.Data;
using NPOI.SS.Formula.Functions;
using NuGet.Packaging;
using System.Globalization;
using static com.sun.tools.@internal.xjc.reader.xmlschema.bindinfo.BIConversion;
using com.sun.xml.@internal.bind.v2.model.core;

namespace E_OneWeb.Areas.Users.Controllers
{
    [Area("Users")]
    [Authorize(Roles = SD.Role_User)]
    public class RoomReservationController : Controller
    {
        private readonly ILogger<RoomReservationController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _hostEnvironment;

        public RoomReservationController(ILogger<RoomReservationController> logger, IUnitOfWork unitOfWork, IWebHostEnvironment hostEnvironment)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _hostEnvironment = hostEnvironment;
        }
        public IActionResult Index()
        {
            return View();
        }
    
        public async Task<IActionResult> Create(int? id)
        {
            ViewBag.Status = "";
            ViewBag.Reason = "";
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var user = _unitOfWork.ApplicationUser.GetFirstOrDefault(u => u.Id == claim.Value);
            RoomReservationUserVM vm = new RoomReservationUserVM()
            {
                RoomReservationUser = new RoomReservationUser()
            };
          
            vm.RoomReservationUser.UserId = user.Id;
            vm.RoomReservationUser.EntryBy = user.Name;
            vm.RoomReservationUser.EntryDate = DateTime.Now;
            vm.StartDate = DateTime.Now.ToString("HH:mm dd/MM/yyyy");
            vm.EndDate = DateTime.Now.ToString("HH:mm dd/MM/yyyy");
            vm.RoomReservationUser.StartDate = DateTime.Now;
            //ViewBag.TimeNow = DateTime.Now.ToString("HH:mm");
            vm.RoomReservationUser.EndDate = DateTime.Now;
            ViewBag.ClockStart = DateTime.Now.ToString("HH:mm");
            ViewBag.ClockEnd = DateTime.Now.ToString("HH:mm");

            return View(vm);

        }
        [TempData]
        public string StatusMessage { get; set; }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RoomReservationUserVM vm)
        {
            var Gen_4 = _unitOfWork.Genmaster.GetAll().Where(z => z.GENFLAG == 4 && z.GENVALUE == 1).FirstOrDefault();
            var Gen_5 = _unitOfWork.Genmaster.GetAll().Where(z => z.GENFLAG == 5 && z.GENVALUE == 1).FirstOrDefault();

            DateTime orderDate = Convert.ToDateTime(vm.RoomReservationUser.StartDate);
            TimeSpan orderTime = vm.ClockStart;
            DateTime orderDateTimeStart = orderDate + orderTime;

            DateTime orderDateEnd = Convert.ToDateTime(vm.RoomReservationUser.EndDate);
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

            var ValBookingRoom = _unitOfWork.GetRommReservationAdmin.GetAll().Where(z => z.Id == vm.RoomReservationUser.RoomReservationAdmin.Id && 
            ((orderDateTimeStart >= z.BookingStartDate && orderDateTimeStart < z.BookingEndDate) || (orderDateTimeEnd > z.BookingStartDate && orderDateTimeEnd <= z.BookingEndDate) || (orderDateTimeStart <= z.BookingStartDate && orderDateTimeEnd >= z.BookingEndDate)));

            if (ValBookingRoom.Count() > 0)
            {
                ViewBag.Status = "Error";
                ViewBag.Reason = "Ruangan sudah di booking dari tanggal: " + ValBookingRoom.FirstOrDefault().BookingStartDate.Value.ToString("dd/MM/yyyy") + " jam: " + ValBookingRoom.FirstOrDefault().BookingStartDate.Value.ToString("HH:mm") + "-" + ValBookingRoom.FirstOrDefault().BookingEndDate.Value.ToString("HH:mm");
                return View(vm);
            }

            string? dayss = orderDate.ToString("dddd");
            var inx = Array.FindIndex(CultureInfo.CurrentCulture.DateTimeFormat.DayNames, x => x == dayss);
            var BookingFixed = await _unitOfWork.FixedSchedulerRoom.GetAllAsync();
            BookingFixed = BookingFixed.Where(z=>z.Flag == inx && z.RoomId == vm.RoomReservationUser.RoomReservationAdmin.RoomId).ToList();

            var GetValBookingFixed_ = (from z in BookingFixed
                                      select new
                                    {
                                        getdatestart_ = Convert.ToDateTime(orderDate).ToString("yyyy-MM-dd") + " "  + Convert.ToDateTime(z.ValStart_Clock).ToString("HH:mm"),
                                        getdateend_ = Convert.ToDateTime(orderDate).ToString("yyyy-MM-dd") + " " + Convert.ToDateTime(z.ValEnd_Clock).ToString("HH:mm"),
                                        study_ = z.Study,
                                        dosen_ = z.Dosen
                                      }).ToList();

            var GetValBookingFixed = (from z in GetValBookingFixed_
                                       select new
                                      {
                                          getdatestart = Convert.ToDateTime(z.getdatestart_),
                                          getdateend = Convert.ToDateTime(z.getdateend_),
                                          study = z.study_,
                                          dosen = z.dosen_
                                       }).ToList();
            GetValBookingFixed = GetValBookingFixed.Where(x => (orderDateTimeStart >= x.getdatestart && orderDateTimeStart < x.getdateend) || (orderDateTimeEnd > x.getdatestart && orderDateTimeEnd <= x.getdateend) || (orderDateTimeStart <= x.getdatestart && orderDateTimeEnd >= x.getdateend)).ToList();

            if (GetValBookingFixed.Count() > 0)
            {
                ViewBag.Status = "Error";
                ViewBag.Reason = "Ruangan akan digunakan untuk kelas: " + GetValBookingFixed.FirstOrDefault().study + ", dosen: " + GetValBookingFixed.FirstOrDefault().dosen +
                                 " jam: " + GetValBookingFixed.FirstOrDefault().getdatestart.ToString("HH:mm") + "-" + GetValBookingFixed.FirstOrDefault().getdateend.ToString("HH:mm");
                return View(vm);
            }

            //if (orderDateTimeStart >= orderDateTimeEnd)
            //{
            //    ViewBag.Status = "Error";
            //    ViewBag.Reason = "Tanggal Peminjaman harus lebih kecil dari Tanggal Selesai";
            //    return View();
            //}
     
            if (vm.RoomReservationUser.RoomReservationAdmin.Id == 0)
            {
                ViewBag.Status = "Error";
                ViewBag.Reason = "Ruangan harus dipilih";
                return View(vm);
            }

            if (vm.RoomReservationUser.Id == 0)
            {
                string webRootPath = _hostEnvironment.WebRootPath;
                var files = HttpContext.Request.Form.Files;
                if (files.Count > 0)
                {
                    string fileName = Guid.NewGuid().ToString();
                    var uploads = Path.Combine(webRootPath, @"images\files");
                    var extenstion = Path.GetExtension(files[0].FileName);

                    if (vm.RoomReservationUser.RefFile != null)
                    {
                        //this is an edit and we need to remove old image
                        var imagePath = Path.Combine(webRootPath, vm.RoomReservationUser.RefFile.TrimStart('\\'));
                        if (System.IO.File.Exists(imagePath))
                        {
                            System.IO.File.Delete(imagePath);
                        }
                    }
                    using (var filesStreams = new FileStream(Path.Combine(uploads, fileName + extenstion), FileMode.Create))
                    {
                        files[0].CopyTo(filesStreams);
                    }
                    vm.RoomReservationUser.RefFile = fileName + extenstion;
                }

                RoomReservationAdmin roomReservationAdmin = await _unitOfWork.RoomReservationAdmin.GetAsync(vm.RoomReservationUser.RoomReservationAdmin.Id);
                vm.RoomReservationUser.RoomReservationAdmin = roomReservationAdmin;
                vm.RoomReservationUser.StatusId = Gen_4.IDGEN;
                vm.RoomReservationUser.Status = Gen_4.GENNAME;                
                vm.RoomReservationUser.StartDate = orderDateTimeStart;                               
                vm.RoomReservationUser.EndDate = orderDateTimeEnd;
                await _unitOfWork.RoomReservationUser.AddAsync(vm.RoomReservationUser);
                _unitOfWork.Save();

                roomReservationAdmin.BookingId = vm.RoomReservationUser.Id;
                roomReservationAdmin.BookingBy = vm.RoomReservationUser.EntryBy;
                roomReservationAdmin.BookingStartDate = vm.RoomReservationUser.StartDate;
                roomReservationAdmin.BookingEndDate = vm.RoomReservationUser.EndDate;
                roomReservationAdmin.StatusId = Gen_4.IDGEN;
                roomReservationAdmin.Status = Gen_4.GENNAME;
				_unitOfWork.RoomReservationAdmin.Update(roomReservationAdmin);
				_unitOfWork.Save();
                ViewBag.Status = "Success";
                ViewBag.Reason = "Berhasil booking ruangan";

            }
            //var errorval = ModelState.Values.SelectMany(i=>i.Errors);
            //if (ModelState.IsValid)
            //{
            //    if (vm.RoomReservationUser.Id == 0)
            //    {                 


            //        await _unitOfWork.RoomReservationUser.AddAsync(vm.RoomReservationUser);
            //        _unitOfWork.Save();
            //        roomReservationAdmin.BookingId = vm.RoomReservationUser.Id;
            //        roomReservationAdmin.BookingBy = vm.RoomReservationUser.EntryBy;
            //        roomReservationAdmin.BookingDate = DateTime.Now;
            //        ViewBag.Status = "Save Success";

            //    }

            //    _unitOfWork.Save();
            //    //return RedirectToAction(nameof(Index));
            //}
            //TempData["Success"] = "Save successfully";

            //return View(vm);
            return RedirectToAction(nameof(Index));

        }

        public async Task<IActionResult> BookingClassRoom(int? id)
        {
            ViewBag.Status = "";
            ViewBag.Reason = "";
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var user = _unitOfWork.ApplicationUser.GetFirstOrDefault(u => u.Id == claim.Value);
            RoomReservationUserVM vm = new RoomReservationUserVM()
            {
                RoomReservationUser = new RoomReservationUser()

            };

            vm.RoomReservationUser.UserId = user.Id;
            vm.RoomReservationUser.EntryBy = user.Name;
            vm.RoomReservationUser.EntryDate = DateTime.Now;

            return View(vm);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BookingClassRoom(RoomReservationUserVM vm)
        {
            var Gen_4 = _unitOfWork.Genmaster.GetAll().Where(z => z.GENFLAG == 4 && z.GENVALUE == 2).FirstOrDefault();

            //DateTime orderDate = Convert.ToDateTime(vm.StartDateValue);
            //TimeSpan orderTime = vm.ClockStart;
            //DateTime orderDateTimeStart = orderDate + orderTime;

            //DateTime orderDateEnd = Convert.ToDateTime(vm.EndDateValue);
            //TimeSpan orderTimeEnd = vm.ClockEnd;
            //DateTime orderDateTimeEnd = orderDateEnd + orderTimeEnd;
            if (vm.RoomReservationUser.RoomReservationAdmin.Id == 0)
            {
                ViewBag.Status = "Error";
                ViewBag.Reason = "Ruangan harus dipilih";
                return View();
            }

            if (vm.RoomReservationUser.Id == 0)
            {
                string webRootPath = _hostEnvironment.WebRootPath;
                var files = HttpContext.Request.Form.Files;
                if (files.Count > 0)
                {
                    string fileName = Guid.NewGuid().ToString();
                    var uploads = Path.Combine(webRootPath, @"images\files");
                    var extenstion = Path.GetExtension(files[0].FileName);

                    if (vm.RoomReservationUser.RefFile != null)
                    {
                        //this is an edit and we need to remove old image
                        var imagePath = Path.Combine(webRootPath, vm.RoomReservationUser.RefFile.TrimStart('\\'));
                        if (System.IO.File.Exists(imagePath))
                        {
                            System.IO.File.Delete(imagePath);
                        }
                    }
                    using (var filesStreams = new FileStream(Path.Combine(uploads, fileName + extenstion), FileMode.Create))
                    {
                        files[0].CopyTo(filesStreams);
                    }
                    vm.RoomReservationUser.RefFile = fileName + extenstion;
                }

                RoomReservationAdmin roomReservationAdmin = await _unitOfWork.RoomReservationAdmin.GetAsync(vm.RoomReservationUser.RoomReservationAdmin.Id);
                vm.RoomReservationUser.RoomReservationAdmin = roomReservationAdmin;
                vm.RoomReservationUser.StatusId = Gen_4.IDGEN;
                vm.RoomReservationUser.Status = Gen_4.GENNAME;
                vm.RoomReservationUser.StartDate = vm.StartDateValue;
                vm.RoomReservationUser.EndDate = vm.EndDateValue;
                await _unitOfWork.RoomReservationUser.AddAsync(vm.RoomReservationUser);
                _unitOfWork.Save();

                roomReservationAdmin.BookingId = vm.RoomReservationUser.Id;
                roomReservationAdmin.BookingBy = vm.RoomReservationUser.EntryBy;
                roomReservationAdmin.StatusId = Gen_4.IDGEN;
                roomReservationAdmin.Status = Gen_4.GENNAME;
                _unitOfWork.RoomReservationAdmin.Update(roomReservationAdmin);
                _unitOfWork.Save();
                ViewBag.Status = "Success";
                ViewBag.Reason = "Berhasil booking ruang kelas"; ;

            }
           

            return View(vm);

        }

        [HttpGet]
        public async Task<IActionResult> GetAllRoomAndLocation()
        {
            var datalist = (from z in await _unitOfWork.RoomReservationAdmin.GetAllAsync()
                            select new
                            {
                                id = z.Id,
                                flag = z.Flag,
                                name_of_room = z.RoomName,
                                name_of_location = z.LocationName,
                                roomId = z.RoomId,
                                desc = _unitOfWork.Room.Get(z.RoomId).Description,
                            }).Where(u => u.flag == 1).ToList();
            return Json(new { data = datalist });
        }

        [HttpGet]
        public async Task<IActionResult> GetAllRoomAndLocationClassRoom()
        {           
            var datalist = (from z in await _unitOfWork.RoomReservationAdmin.GetAllAsync()
                            select new
                            {
                                id = z.Id,
                                flag = z.Flag,
                                status = z.Status,
                                name_of_room = z.RoomName,
                                name_of_location = z.LocationName,
                                startdate = z.BookingStartDate,
                                enddate = z.BookingEndDate,
                                startdateshow = z.BookingStartDate != null ? z.BookingStartDate.Value.ToString("dd/MM/yyyy") : "",
                                enddatetime = z.BookingEndDate != null ? z.BookingStartDate.Value.ToString("dd/MM/yyyy") : "",
                                clockstart = z.BookingStartDate != null ? z.BookingStartDate.Value.ToString("HH:mm") : "",
                                clockend = z.BookingStartDate != null ? z.BookingEndDate.Value.ToString("HH:mm") : "",
                                bookingdate = z.BookingStartDate,
                                clockstart_clockend = z.BookingEndDate != null ? z.BookingStartDate.Value.ToString("HH:mm") + " - " + z.BookingEndDate.Value.ToString("HH:mm") : "",
                            }).Where(u=> u.flag == 2 && u.status == "Room Available").ToList();
            return Json(new { data = datalist });
        }

        #region API CALLS

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var user = _unitOfWork.ApplicationUser.GetFirstOrDefault(u => u.Id == claim.Value);

            var datalist = (from z in await _unitOfWork.RoomReservationUser.GetAllAsync(includeProperties: "RoomReservationAdmin")
                            select new
                            {
                                id = z.Id,
                                roomname = z.RoomReservationAdmin.RoomName +" (" + z.RoomReservationAdmin.LocationName +")",
                                bookingdate = z.StartDate != null ? z.StartDate.Value.ToString("dd/MM/yyyy") : "",
                                bookingclock = z.StartDate != null ? z.StartDate.Value.ToString("HH:mm") + "-" + Convert.ToDateTime(z.EndDate).ToString("HH:mm") : "",
                                status = z.Status == "Waiting Approval" ? "Menunggu Persetujuan" : z.Status == "Approved" ? "Disetujui" : z.Status == "Rejected" ? "Ditolak" : z.Status,
                                statusid = z.StatusId,
                                description = z.Description,
                                entryby = z.EntryBy,
                                notes = z.Notes
                            }).Where(i => i.entryby == user.ToString()).ToList().OrderByDescending(o => o.id);

            return Json(new { data = datalist });
        }
        [HttpGet]
        public async Task<IActionResult> GetVehicleReservation()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var user = _unitOfWork.ApplicationUser.GetFirstOrDefault(u => u.Id == claim.Value);

            var datalist = (from z in await _unitOfWork.VehicleReservationUser.GetAllAsync(includeProperties: "VehicleReservationAdmin")
                            select new                            {
                                id = z.Id,
                                name = z.VehicleReservationAdmin.ItemName,
                                bookingdate = z.BookingStartDate != null ? z.BookingStartDate.Value.ToString("dd/MM/yyyy") : "",
                                bookingclock = z.BookingStartDate != null ? z.BookingStartDate.Value.ToString("HH:mm") + "-" + z.BookingEndDate.Value.ToString("HH:mm") : "",
                                status = z.Status,
                                statusid = z.StatusId,
                                entryby = z.EntryBy,
                                notes = z.NotesReject,
                                utility = z.Utilities,
                                destination = z.Destination,
                                driver = z.DriverName
                            }).Where(i => i.entryby == user.ToString()).ToList().OrderByDescending(o => o.id);

            return Json(new { data = datalist });
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var objFromDb = await _unitOfWork.Category.GetAsync(id);
            if (objFromDb == null)
            {
                TempData["Error"] = "Gagal Hapus";
                return Json(new { success = false, message = "Gagal Hapus" });
            }
            await _unitOfWork.Category.RemoveAsync(objFromDb);
            _unitOfWork.Save();

            TempData["Success"] = "Berhasil Hapus";
            return Json(new { success = true, message = "Berhasil Hapus" });

        }

        #endregion
        public List<GridBookingRoom> ListBookingRoom { get; set; }
        [HttpGet]
        public async Task<IActionResult> GetBookingListUser(int id)
        {
            try
            {
                RoomReservationAdmin RoomID = await _unitOfWork.RoomReservationAdmin.GetAsync(id);

                List<GridBookingRoom> ListGridBookingRoom = new List<GridBookingRoom>();
                var datalist = (from z in await _unitOfWork.RoomReservationUser.GetAllAsync(includeProperties: "RoomReservationAdmin")
                                select new GridBookingRoom
                                {
                                    //id = z.Id,
                                    id = z.RoomAdminId,
                                    //locationname = z.RoomReservationAdmin.LocationName,
                                    //roomid = z.RoomReservationAdmin.RoomId,
                                    roomname = z.RoomReservationAdmin.RoomName,
                                    booking_date = z.StartDate != null ? z.StartDate.Value.ToString("dddd", new System.Globalization.CultureInfo("id-ID")) + " - " +  z.StartDate.Value.ToString("dd/MM/yyyy") : "",
                                    booking_clock = z.StartDate != null ? z.StartDate.Value.ToString("HH:mm") + "-" + z.EndDate.Value.ToString("HH:mm"): "",
                                    study = z.Study,
                                    dosen = z.Dosen,
                                    prodi = z.Description,
                                    semester = z.ApproveBy,
                                    bookingenddate = z.EndDate
                                }).Where(o => o.id == id && o.bookingenddate >= DateTime.Now).ToList();

                var datalist2 = (from z in await _unitOfWork.FixedSchedulerRoom.GetAllAsync(includeProperties: "Room")
                                 select new GridBookingRoom
                                 {
                                     id = z.RoomId,
                                     roomname = z.RoomName,
                                     booking_date = z.Days,
                                     booking_clock = z.Start_Clock + "-" + z.End_Clock,
                                     study = z.Study,
                                     dosen = z.Dosen,
                                     prodi = z.Prodi,
                                     semester = z.Semester
                                 }).Where(o => o.id == RoomID.RoomId).ToList();
                
                //foreach (var item in datalist)
                //{
                //    GridBookingRoom gridBookingRoom = new GridBookingRoom
                //    {
                //        id = item.id,
                //        roomname = item.roomname,
                //        booking_date = item.booking_date,
                //        booking_clock = item.booking_clock,
                //    };
                //    ListGridBookingRoom.Add(gridBookingRoom);
                //}


                //foreach (var item in datalist2)
                //{
                //    GridBookingRoom gridBookingRoom = new GridBookingRoom
                //    {
                //        id = item.id,
                //        roomname = item.roomname,
                //        booking_date = item.booking_date,
                //        booking_clock = item.booking_clock,
                //    };
                //    ListGridBookingRoom.Add(gridBookingRoom);
                //}
                ListGridBookingRoom.AddRange(datalist);
                ListGridBookingRoom.AddRange(datalist2);
                //var ListGridBookingRoom_ = ListGridBookingRoom.ToList();

                return Json(new { data = ListGridBookingRoom });
            }
            catch (Exception ex)
            {
                string err = ex.Message.ToString();
                return Json(new { data = "" });
         
            }
            

           
        }

        [HttpGet]
        public async Task<IActionResult> GetAllRuangTetap()
        {
            try
            {
                int Number = 1;
                var datalist = (from z in await _unitOfWork.FixedSchedulerRoom.GetAllAsync(includeProperties: "Room")
                                select new
                                {
                                    no = Number++,
                                    id = z.Id,
                                    roomname = z.RoomName,
                                    locationname = z.LocationName,
                                    days = z.Days,
                                    clock = z.Start_Clock + "-" + z.End_Clock,
                                    prodi = z.Prodi,
                                    study = z.Study,
                                    semester = z.Semester,
                                    dosen = z.Dosen
                                }).ToList();

                return Json(new { data = datalist });
            }
            catch (Exception ex)
            {
                string error = ex.Message.ToString();
                return Json(new { data = "" });
            }

        }
    }
}
