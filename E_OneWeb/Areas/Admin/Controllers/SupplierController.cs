using E_OneWeb.DataAccess.Repository.IRepository;
using E_OneWeb.Models;
using E_OneWeb.Models.ViewModels;
using E_OneWeb.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace E_OneWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
    public class SupplierController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public SupplierController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<IActionResult> Index()
        {
            SupplierVM supplierVM = new SupplierVM()
            {
                Suppliers = await _unitOfWork.Supplier.GetAllAsync()
            };
            var count = supplierVM.Suppliers.Count();
            return View(supplierVM);
        }

        public async Task<IActionResult> Upsert(int? id)
        {
            Supplier supplier = new Supplier();
            if (id == null)
            {
                //this is for create
                return View(supplier);
            }
            //this is for edit
            supplier = await _unitOfWork.Supplier.GetAsync(id.GetValueOrDefault());
            if (supplier == null)
            {
                return NotFound();
            }
            return View(supplier);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(Supplier supplier)
        {
            if (ModelState.IsValid)
            {
                if (supplier.Id == 0)
                {
                    await _unitOfWork.Supplier.AddAsync(supplier);

                }
                else
                {
                    _unitOfWork.Supplier.Update(supplier);
                }
                _unitOfWork.Save();
                return RedirectToAction(nameof(Index));
            }
            return View(supplier);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var allObj = await _unitOfWork.Supplier.GetAllAsync();
            return Json(new { data = allObj });
        }
        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var objFromDb = await _unitOfWork.Supplier.GetAsync(id);
            if (objFromDb == null)
            {
                TempData["Error"] = "Error deleting Category";
                return Json(new { success = false, message = "Error while deleting" });
            }
            await _unitOfWork.Supplier.RemoveAsync(objFromDb);
            _unitOfWork.Save();

            TempData["Success"] = "Category successfully deleted";
            return Json(new { success = true, message = "Delete Successful" });

        }
    }
}
