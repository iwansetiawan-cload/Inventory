using E_OneWeb.DataAccess.Repository.IRepository;
using E_OneWeb.Models.ViewModels;
using E_OneWeb.Models;
using E_OneWeb.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data;
using System.Diagnostics;

namespace E_OneWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class PurchaseOrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public PurchaseOrderController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {

            var datalist = (from z in await _unitOfWork.PurchaseOrderHeader.GetAllAsync(includeProperties: "Supplier")
                            select new
                            {
                                id = z.Id,
                                purchaseorderno = z.PurchaseOrderNo,
                                transactiondate = Convert.ToDateTime(z.TransactionDate).ToString("dd/MM/yyyy"),
                                requester = z.Requester,
                                supplier = z.Supplier.Name
                            }).ToList();

            return Json(new { data = datalist });
        }
        [HttpGet]
        public async Task<IActionResult> GetAllItems()
        {

            var datalist = (from z in await _unitOfWork.Items.GetAllAsync(includeProperties: "Category,Room")
                            select new
                            {
                                id = z.Id,
                                code = z.Code,
                                name = z.Name,
                                description = z.Description,
                                price = z.Price != null ? z.Price : 0,
                                qty = z.Qty != null ? z.Qty : 0,
                                totalamount = z.TotalAmount != null ? z.TotalAmount : 0,
                                room = z.Room.Name,
                                category = z.Category.Name
                            }).ToList();

            return Json(new { data = datalist });
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

        //public ActionResult Import()
        //{
        //    PurchaseOrderHeaderVM vm = new PurchaseOrderHeaderVM();
        //    ViewBag.Status = "";
        //    ViewBag.Reason = "";

        //    return View(vm);
        //}       
        public async Task<IActionResult> AddOrderDetail(int? id)
        {
            //PurchaseOrderHeaderVM vm = new PurchaseOrderHeaderVM();
            //return View(vm);
            IEnumerable<Category> CatList = await _unitOfWork.Category.GetAllAsync();
            ItemsVM itemsVM = new ItemsVM()
            {
                Items = new Items(),
                CategoryList = CatList.Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                }),
                RoomList = _unitOfWork.Room.GetAll().Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                })
            };

            if (id == null)
            {
                itemsVM.Items.StartDate = Convert.ToDateTime("01-01-2000");
                //this is for create
                return View(itemsVM);
            }

            itemsVM.Items = await _unitOfWork.Items.GetAsync(id.GetValueOrDefault());
            if (itemsVM.Items == null)
            {
                return NotFound();
            }

            ViewBag.Status = "";
            return View(itemsVM);
        }

        public ActionResult LookupProduct(int id)
        {
            PurchaseOrderHeaderVM vm = new PurchaseOrderHeaderVM();

            return PartialView(vm);
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var objFromDb = await _unitOfWork.PurchaseOrderHeader.GetAsync(id);
            if (objFromDb == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }

            await _unitOfWork.PurchaseOrderHeader.RemoveAsync(objFromDb);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Delete Successful" });

        }

        [HttpPost]
        public IActionResult AddItems([FromBody] int id)
        {
          
            return Json(new { success = true, message = "Operation Successful." });
        }
    }
}
