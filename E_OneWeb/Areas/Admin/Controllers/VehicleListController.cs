using E_OneWeb.DataAccess.Repository.IRepository;
using E_OneWeb.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using E_OneWeb.Models;
using E_OneWeb.Models.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis;
using System.Data;

namespace E_OneWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
    public class VehicleListController : Controller
    {
        private readonly ILogger<RoomListController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public VehicleListController(ILogger<RoomListController> logger, IUnitOfWork unitOfWork)
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
         
            var VehicleReservationAdminList = (from z in await _unitOfWork.VehicleReservationAdmin.GetAllAsync(includeProperties: "Items")
                                       select new
                                       {
                                           id = z.Id,
                                           itemId = z.ItemId,
                                           status = z.Status,
                                           statusid = z.StatusId,
                                           flag = z.Flag
                                       }).Where(z=>z.flag != null).ToList();

            var datalist = (from z in await _unitOfWork.Items.GetAllAsync(includeProperties: "Category,Room")
                            select new
                            {
                                id = z.Id,
                                code = z.Code,
                                name = z.Name,
                                description = z.Description,                               
                                room = z.Room.Name,
                                location = _unitOfWork.Location.Get(z.Room.IDLocation).Name,
                                categoriid = z.CategoryId,
                                category = z.Category.Name,
                                idreservation = VehicleReservationAdminList.Where(i => i.itemId == z.Id).Count() > 0 ? 1 : 0
                            }).Where(x=>x.categoriid == 5 || x.categoriid == 6).ToList();         


            return Json(new { data = datalist });
        }
        [HttpPost]
        public async Task<IActionResult> Lock([FromBody] string id)
        {

            var Gen_5 = _unitOfWork.Genmaster.GetAll().Where(z => z.GENFLAG == 5 && z.GENVALUE == 0).FirstOrDefault();
            var RoomList = _unitOfWork.Room.GetAll(includeProperties: "Location").ToList();

            int idBooking = id != null ? Convert.ToInt32(id) : 0;
            var VehicleReservationAdminList = await _unitOfWork.VehicleReservationAdmin.GetAllAsync();
            VehicleReservationAdmin VehicleReservationAdmin = VehicleReservationAdminList.Where(z => z.ItemId == idBooking).FirstOrDefault();

            if (VehicleReservationAdmin == null)
            {
                var ListItems = await _unitOfWork.Items.GetAllAsync();

                VehicleReservationAdminVM vm = new VehicleReservationAdminVM()
                {
                    VehicleReservationAdmin = new VehicleReservationAdmin()

                };

                vm.VehicleReservationAdmin.Items = ListItems.Where(z => z.Id == idBooking).FirstOrDefault();                
                vm.VehicleReservationAdmin.ItemId = vm.VehicleReservationAdmin.Items.Id;
                vm.VehicleReservationAdmin.ItemName = vm.VehicleReservationAdmin.Items.Name;
                vm.VehicleReservationAdmin.RoomName = vm.VehicleReservationAdmin.Items.Room.Name;
                vm.VehicleReservationAdmin.LocationName = vm.VehicleReservationAdmin.Items.Room.Location.Name;
                vm.VehicleReservationAdmin.StatusId = Gen_5.IDGEN;
                vm.VehicleReservationAdmin.Status = Gen_5.GENNAME;
                vm.VehicleReservationAdmin.Flag = (int)Gen_5.GENVALUE;

                _unitOfWork.VehicleReservationAdmin.AddAsync(vm.VehicleReservationAdmin);
                _unitOfWork.Save();
            }
            else
            {
                VehicleReservationAdmin.StatusId = Gen_5.IDGEN;
                VehicleReservationAdmin.Status = Gen_5.GENNAME;
                VehicleReservationAdmin.Flag = (int)Gen_5.GENVALUE;
                _unitOfWork.VehicleReservationAdmin.Update(VehicleReservationAdmin);
                _unitOfWork.Save();
            }

            return Json(new { success = true, message = "Unlock Successful." });
        }

        [HttpPost]
        public async Task<IActionResult> Unlock([FromBody] string id)
        {

            int idBooking = id != null ? Convert.ToInt32(id) : 0;
            var VehicleReservationList = await _unitOfWork.VehicleReservationAdmin.GetAllAsync();
            VehicleReservationAdmin VehicleReservationAdmin = VehicleReservationList.Where(z => z.ItemId == idBooking).FirstOrDefault();
            VehicleReservationAdmin.StatusId = null;
            VehicleReservationAdmin.Status = null;
            VehicleReservationAdmin.Flag = null;
            VehicleReservationAdmin.BookingBy = null;
            VehicleReservationAdmin.BookingStartDate = null;
            VehicleReservationAdmin.BookingEndDate = null;
            VehicleReservationAdmin.BookingId = null;
            _unitOfWork.VehicleReservationAdmin.Update(VehicleReservationAdmin);
            _unitOfWork.Save();

            return Json(new { success = true, message = "Lock Successful." });
        }

    }
}
