using E_OneWeb.DataAccess.Repository.IRepository;
using E_OneWeb.Models.ViewModels;
using E_OneWeb.Models;
using E_OneWeb.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data;

namespace E_OneWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class RequestItemsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public RequestItemsController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> Upsert(int? id)
        {
            IEnumerable<Supplier> SupplierList = await _unitOfWork.Supplier.GetAllAsync();

            PurchaseOrderHeaderVM purchaseOrderHeaderVM = new PurchaseOrderHeaderVM()
            {
                PurchaseOrderHeader = new PurchaseOrderHeader(),
                SupplierList = SupplierList.Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                })

            };
            purchaseOrderHeaderVM.PurchaseOrderHeader.TransactionDate = DateTime.Today;
            if (id == null)
            {
                //this is for create
                return View(purchaseOrderHeaderVM);
            }

            purchaseOrderHeaderVM.PurchaseOrderHeader = await _unitOfWork.PurchaseOrderHeader.GetAsync(id.GetValueOrDefault());
            if (purchaseOrderHeaderVM.PurchaseOrderHeader == null)
            {
                return NotFound();
            }

            ViewBag.Status = "";

            return View(purchaseOrderHeaderVM);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(PurchaseOrderHeaderVM vm)
        {
            IEnumerable<Supplier> SupplierList = await _unitOfWork.Supplier.GetAllAsync();
            vm.SupplierList = SupplierList.Select(i => new SelectListItem
            {
                Text = i.Name,
                Value = i.Id.ToString()
            });

            if (vm.PurchaseOrderHeader.Id == 0)
            {
                await _unitOfWork.PurchaseOrderHeader.AddAsync(vm.PurchaseOrderHeader);
                ViewBag.Status = "Save Success";

            }
            else
            {
                _unitOfWork.PurchaseOrderHeader.Update(vm.PurchaseOrderHeader);
                ViewBag.Status = "Edit Success";
            }
            _unitOfWork.Save();

            return View(vm);
        }
    }
}
