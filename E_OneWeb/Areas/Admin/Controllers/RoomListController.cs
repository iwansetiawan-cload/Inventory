﻿using E_OneWeb.DataAccess.Repository.IRepository;
using E_OneWeb.Models;
using E_OneWeb.Models.ViewModels;
using E_OneWeb.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis;
using System.Data;

namespace E_OneWeb.Areas.Admin.Controllers
{
	[Area("Admin")]
    [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
    public class RoomListController : Controller
    {
		private readonly ILogger<RoomListController> _logger;
		private readonly IUnitOfWork _unitOfWork;

		public RoomListController(ILogger<RoomListController> logger, IUnitOfWork unitOfWork)
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
            //var RoomReservation = _unitOfWork.RoomReservationAdmin.GetAllAsync();

			var RoomReservationList = (from z in await _unitOfWork.RoomReservationAdmin.GetAllAsync(includeProperties: "Room")
							select new
							{
								id = z.Id,
								idroom = z.RoomId,
								status = z.Status,
								statusid = z.StatusId,
								flag = z.Flag
							}).ToList();


			var datalist = (from z in _unitOfWork.Room.GetAll(includeProperties: "Location")
							select new
							{
								id = z.Id,
								roomname = z.Name,							
								locationname = z.Location.Name,
								description = z.Description,
                                idreservation = RoomReservationList.Where(i=>i.idroom == z.Id && i.flag > 0).Count() > 0 ? 1 : 0
							}).ToList();
			

			return Json(new { data = datalist });
		}
        [HttpPost]
        public async Task<IActionResult> Lock([FromBody] string id)
        {
			
			var Gen_4 = _unitOfWork.Genmaster.GetAll().Where(z => z.GENFLAG == 4 && z.GENVALUE == 0).FirstOrDefault();
			var RoomList = _unitOfWork.Room.GetAll(includeProperties: "Location").ToList();

			int idRoom = id != null ? Convert.ToInt32(id) : 0;
			var RoomReservationist = await _unitOfWork.RoomReservationAdmin.GetAllAsync();
			RoomReservationAdmin RoomReservationAdmin = RoomReservationist.Where(z => z.RoomId == idRoom).FirstOrDefault();

			if (RoomReservationAdmin == null)
			{
				
				RoomReservationAdminVM vm = new RoomReservationAdminVM()
				{
					RoomReservationAdmin = new RoomReservationAdmin()

				};

				vm.RoomReservationAdmin.Room = RoomList.Where(z => z.Id == idRoom).FirstOrDefault();
				vm.RoomReservationAdmin.RoomName = vm.RoomReservationAdmin.Room.Name;
				vm.RoomReservationAdmin.LocationName = vm.RoomReservationAdmin.Room.Location.Name;

				vm.RoomReservationAdmin.StatusId = Gen_4.IDGEN;
				vm.RoomReservationAdmin.Status = Gen_4.GENNAME;
				vm.RoomReservationAdmin.Flag = 1;

				_unitOfWork.RoomReservationAdmin.AddAsync(vm.RoomReservationAdmin);
				_unitOfWork.Save();
			}
			else
			{
				RoomReservationAdmin.Room = RoomList.Where(z => z.Id == idRoom).FirstOrDefault();
				RoomReservationAdmin.RoomName = RoomReservationAdmin.Room.Name;
				RoomReservationAdmin.LocationName = RoomReservationAdmin.Room.Location.Name;
				RoomReservationAdmin.StatusId = Gen_4.IDGEN;
				RoomReservationAdmin.Status = Gen_4.GENNAME;
				RoomReservationAdmin.Flag = 1;
				_unitOfWork.RoomReservationAdmin.Update(RoomReservationAdmin);
				_unitOfWork.Save();
			}			

			return Json(new { success = true, message = "Operation Successful." });
        }

		[HttpPost]
		public async Task<IActionResult> Unlock([FromBody] string id)
		{

			int idRoom = id != null ? Convert.ToInt32(id) : 0;
			var RoomReservationist = await _unitOfWork.RoomReservationAdmin.GetAllAsync();
			RoomReservationAdmin RoomReservationAdmin = RoomReservationist.Where(z => z.RoomId == idRoom).FirstOrDefault();
			RoomReservationAdmin.StatusId = null;
			RoomReservationAdmin.Status = null;
			RoomReservationAdmin.Flag = null;
			RoomReservationAdmin.BookingBy = null;
			RoomReservationAdmin.BookingStartDate = null;
            RoomReservationAdmin.BookingEndDate = null;
            RoomReservationAdmin.BookingId = null;
			_unitOfWork.RoomReservationAdmin.Update(RoomReservationAdmin);
			_unitOfWork.Save();		

			return Json(new { success = true, message = "Operation Successful." });
		}

	}
}
