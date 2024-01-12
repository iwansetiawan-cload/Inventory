using com.sun.xml.@internal.bind.v2.model.core;
using E_OneWeb.DataAccess.Repository.IRepository;
using E_OneWeb.Models;
using E_OneWeb.Models.ViewModels;
using E_OneWeb.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Globalization;

namespace E_OneWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public CategoryController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<IActionResult> Index()
        {
            //CategoryVM categoryVM = new CategoryVM()
            //{
            //    Categories = await _unitOfWork.Category.GetAllAsync()
            //};
            //var count = categoryVM.Categories.Count();
            //int value = Convert.ToInt32("1F600", 16);
            //var emoji = Char.ConvertFromUtf32(value);
            return View();
        }
        public async Task<IActionResult> Upsert(int? id)
        {
            CategoryVM categoryVm = new CategoryVM();
            Category category = new Category();
            if (id == null)
            {
                //this is for create
                return View(category);
            }
            //this is for edit
            category = await _unitOfWork.Category.GetAsync(id.GetValueOrDefault());
            if (category == null)
            {
                return NotFound();
            }
            categoryVm.Id = category.Id;
            categoryVm.Name = category.Name;
            categoryVm.Description = category.Description;  
            categoryVm.Percent_String =  category.Percent.Value.ToString("#,##0.00");
            categoryVm.Period = category.Period;
            categoryVm.Percent = category.Percent.Value;
            return View(categoryVm);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(CategoryVM vm)
        {
            if (ModelState.IsValid)
            {
                Category category = new Category();
                category = await _unitOfWork.Category.GetAsync(vm.Id);
                category.Name = vm.Name;
                category.Description = vm.Description;
                category.Percent = decimal.Parse(vm.Percent_String, CultureInfo.InvariantCulture);
                category.Period = vm.Period;
                if (category.Id == 0)
                {
                     _unitOfWork.Category.AddAsync(category);

                }
                else
                {
                    _unitOfWork.Category.Update(category);
                }
                _unitOfWork.Save();
                return RedirectToAction(nameof(Index));
            }
            return View(vm);
        }

        #region API CALLS

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var datalist = (from z in await _unitOfWork.Category.GetAllAsync()
                            select new
                            {
                                id = z.Id,
                                name = z.Name,
                                persent_ = z.Percent != null ? z.Percent : 0,
                                period = z.Period != null ? z.Period : 0,
                                description = z.Description
                            }).ToList().OrderByDescending(o => o.id);

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
