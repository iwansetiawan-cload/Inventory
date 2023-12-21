using E_OneWeb.DataAccess.Repository.IRepository;
using E_OneWeb.Models.ViewModels;
using E_OneWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using E_OneWeb.Utility;
using Microsoft.AspNetCore.Authorization;
using System.Data;
using Microsoft.Extensions.Hosting;

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

            var ValBookingRoom = _unitOfWork.GetRommReservationAdmin.GetAll().Where(z => z.Id == vm.RoomReservationUser.RoomAdminId && z.BookingEndDate > vm.RoomReservationUser.EndDate);

            if (ValBookingRoom != null)
            {
                StatusMessage = "Ruangan sudah di booking";
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
                vm.RoomReservationUser.StatusId = Gen_5.IDGEN;
                vm.RoomReservationUser.Status = Gen_5.GENNAME;
                DateTime orderDate = Convert.ToDateTime(vm.RoomReservationUser.StartDate);
                TimeSpan orderTime = vm.ClockStart;
                DateTime orderDateTime = orderDate + orderTime;
                vm.RoomReservationUser.StartDate = orderDateTime;

                DateTime orderDateEnd = Convert.ToDateTime(vm.RoomReservationUser.EndDate);
                TimeSpan orderTimeEnd = vm.ClockEnd;
                DateTime orderDateTimeEnd = orderDateEnd + orderTimeEnd;                
                vm.RoomReservationUser.EndDate = orderDateTimeEnd;
                await _unitOfWork.RoomReservationUser.AddAsync(vm.RoomReservationUser);
                _unitOfWork.Save();

                roomReservationAdmin.BookingId = vm.RoomReservationUser.Id;
                roomReservationAdmin.BookingBy = vm.RoomReservationUser.EntryBy;
                roomReservationAdmin.BookingStartDate = vm.RoomReservationUser.StartDate;
                roomReservationAdmin.BookingEndDate = vm.RoomReservationUser.EndDate;
                roomReservationAdmin.StatusId = Gen_4.IDGEN;
                roomReservationAdmin.Status = Gen_4.GENNAME;
				roomReservationAdmin.Flag = Gen_4.GENCODE != null ? Convert.ToInt32(Gen_4.GENCODE) : 0;
				_unitOfWork.RoomReservationAdmin.Update(roomReservationAdmin);
				_unitOfWork.Save();
				ViewBag.Status = "Save Success";

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
            TempData["Success"] = "Save successfully";

            return RedirectToAction(nameof(Index));

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
                                name_of_location = z.LocationName
                            }).Where(i => i.flag == 0).ToList();
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
                                startdate = Convert.ToDateTime(z.StartDate).ToString("dd/MM/yyyy"),
                                enddate = Convert.ToDateTime(z.EndDate).ToString("dd/MM/yyyy"),
                                status = z.Status,
                                statusid = z.StatusId,
                                description = z.Description,
                                entryby = z.EntryBy,
                            }).Where(i => i.entryby == user.ToString()).ToList().OrderByDescending(o => o.id);

            return Json(new { data = datalist });
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var objFromDb = await _unitOfWork.Category.GetAsync(id);
            if (objFromDb == null)
            {
                TempData["Error"] = "Error deleting Category";
                return Json(new { success = false, message = "Error while deleting" });
            }
            await _unitOfWork.Category.RemoveAsync(objFromDb);
            _unitOfWork.Save();

            TempData["Success"] = "Category successfully deleted";
            return Json(new { success = true, message = "Delete Successful" });

        }

        #endregion
    }
}
