using E_OneWeb.DataAccess.Repository.IRepository;
using E_OneWeb.Models;
using E_OneWeb.Models.ViewModels;
using E_OneWeb.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace E_OneWeb.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
	public class DriversController : Controller
	{
		private readonly IUnitOfWork _unitOfWork;
		public DriversController(IUnitOfWork unitOfWork)
		{
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
        public IActionResult Upsert(int? id)
        {
            Drivers driver = new Drivers();
            if (id == null)
            {
                //this is for create
                return View(driver);
            }
            //this is for edit
            driver =  _unitOfWork.Driver.Get(id.GetValueOrDefault());
            if (driver == null)
            {
                return NotFound();
            }
            return View(driver);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(Drivers driver)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    //Drivers driver = new Drivers();
                    //if (vm.Id == 0)
                    //{
                    //    driver.Name = vm.Name;
                    //}
                    //else
                    //{
                    //    driver = _unitOfWork.Driver.Get(vm.Id);
                    //    driver.Name = vm.Name;
                    //}             

                    if (driver.Id == 0)
                    {
                        _unitOfWork.Driver.Add(driver);
                    }
                    else
                    {
                        _unitOfWork.Driver.Update(driver);
                    }
                    _unitOfWork.Save();
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                string err = ex.Message.ToString();
            }

            return View(driver);
        }

        #region API CALLS

        [HttpGet]
        public IActionResult GetAll()
        {
            var datalist = (from z in _unitOfWork.Driver.GetAll()
                            select new
                            {
                                id = z.Id,
                                name = z.Name,
                                address = z.Address,
                                phone = z.PhoneNumber
                            }).ToList();

            return Json(new { data = datalist });
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var objFromDb = _unitOfWork.Driver.Get(id);
            if (objFromDb == null)
            {
                TempData["Error"] = "Hapus data supir gagal";
                return Json(new { success = false, message = "Hapus Gagal" });
            }
            _unitOfWork.Driver.Remove(objFromDb);
            _unitOfWork.Save();

            TempData["Success"] = "Hapus data supir berhasil";
            return Json(new { success = true, message = "Hapus Berhasil" });

        }

        #endregion
    }
}
