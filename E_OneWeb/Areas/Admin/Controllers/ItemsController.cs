using E_OneWeb.DataAccess.Repository.IRepository;
using E_OneWeb.Models;
using E_OneWeb.Models.ViewModels;
using E_OneWeb.Utility;
using MathNet.Numerics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using sun.misc;
using sun.nio.cs.ext;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Security.Claims;

namespace E_OneWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
    public class ItemsController : Controller
    {

        private readonly IUnitOfWork _unitOfWork;
        public ItemsController(IUnitOfWork unitOfWork) 
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {            
            return View();
        }
        //public static int? ItemID { get; set; }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {

            var datalist = (from z in await _unitOfWork.Items.GetAllAsync(includeProperties: "Category,Room")
                            select new
                            {
                                id = z.Id,
                                code = z.Code,
                                name = z.Name,
                                description = z.Description,
                                price = z.Price != null ? z.Price.Value.ToString("#,##0") : "",
                                qty = z.Qty != null ? z.Qty : 0,
                                totalamount = z.TotalAmount != null ? z.TotalAmount : 0,
                                room = z.Room.Name,
                                category = z.Category.Name
                            }).ToList().OrderByDescending(o=>o.id);

            return Json(new { data = datalist });
        }

        public async Task<IActionResult> Upsert(int? id)
        {
            var ConditionList = _unitOfWork.Genmaster.GetAll().Where(z=>z.GENFLAG == 1).Select(x => new SelectListItem { Value = x.IDGEN.ToString(), Text = x.GENNAME });
            ViewBag.ConditionList = new SelectList(ConditionList, "Value", "Text");

            var StatusList = _unitOfWork.Genmaster.GetAll().Where(z => z.GENFLAG == 2).Select(x => new SelectListItem { Value = x.IDGEN.ToString(), Text = x.GENNAME });
            ViewBag.StatusList = new SelectList(StatusList, "Value", "Text");
            
            var OwnershipList = _unitOfWork.Genmaster.GetAll().Where(z => z.GENFLAG == 3).Select(x => new SelectListItem { Value = x.IDGEN.ToString(), Text = x.GENNAME });
            ViewBag.OwnershipList = new SelectList(OwnershipList, "Value", "Text");

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
                }),
                LocationList = _unitOfWork.Location.GetAll().Select(i => new SelectListItem
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
            itemsVM.Percent_String = itemsVM.Items.Percent.Value.ToString("#,##0.00");
            //itemsVM.TotalAmountString = itemsVM.Items.TotalAmount;
            ViewBag.Status = "";
            return View(itemsVM);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(ItemsVM vm)
        {

            var ConditionList = _unitOfWork.Genmaster.GetAll().Where(z => z.GENFLAG == 1).Select(x => new SelectListItem { Value = x.IDGEN.ToString(), Text = x.GENNAME });
            var StatusList = _unitOfWork.Genmaster.GetAll().Where(z => z.GENFLAG == 2).Select(x => new SelectListItem { Value = x.IDGEN.ToString(), Text = x.GENNAME });
            var OwnershipList = _unitOfWork.Genmaster.GetAll().Where(z => z.GENFLAG == 3).Select(x => new SelectListItem { Value = x.IDGEN.ToString(), Text = x.GENNAME });
            IEnumerable<Category> CatList = await _unitOfWork.Category.GetAllAsync();
            vm.CategoryList = CatList.Select(i => new SelectListItem
            {
                Text = i.Name,
                Value = i.Id.ToString()
            });

            vm.RoomList = _unitOfWork.Room.GetAll().Select(i => new SelectListItem
            {
                Text = i.Name,
                Value = i.Id.ToString()
            });


            if (vm.Items.CategoryId > 0 && vm.Items.RoomId > 0)
            {

                #region Variable
                decimal? Percent = Convert.ToDecimal(vm.Percent_String);
                vm.Items.Percent = Percent;
                int PeriodExpence = vm.Items.Period != null ? vm.Items.Period.Value : 0;
                #endregion

                #region Hitung Nilai Buku
                decimal Nilai_Buku = GetNilaiPenyusutan(vm.Items.StartDate, vm.Items.TotalAmount, PeriodExpence, Percent);
                vm.Items.DepreciationExpense = Math.Round((double)Nilai_Buku);
                ViewBag.DepreciationExpense = Nilai_Buku.ToString("#,##0");
                #endregion

                vm.Items.Room = _unitOfWork.Room.Get(vm.Items.RoomId);
                if (vm.Items.Id == 0)
                {
                    var claimsIdentity = (ClaimsIdentity)User.Identity;
                    var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                    var user = _unitOfWork.ApplicationUser.GetFirstOrDefault(u => u.Id == claim.Value);

                    vm.Items.EntryBy = user.Name;
                    vm.Items.EntryDate = DateTime.Now;
                    await _unitOfWork.Items.AddAsync(vm.Items);
                    ViewBag.Status = "Save Success";
                    _unitOfWork.Save();
                    //vm.Items.Code = "ITM-" + vm.Items.Id.ToString();
                    _unitOfWork.Items.Update(vm.Items);
                }
                else
                {                    
                    _unitOfWork.Items.Update(vm.Items);
                    ViewBag.Status = "Update Success";
                }
                _unitOfWork.Save();
              
            }
            ViewBag.ConditionList = new SelectList(ConditionList, "Value", "Text", vm.Items.Condition);
            ViewBag.StatusList = new SelectList(StatusList, "Value", "Text", vm.Items.Status);
            ViewBag.OwnershipList = new SelectList(OwnershipList, "Value", "Text", vm.Items.OriginOfGoods);
            return View(vm);
        }

        private decimal GetNilaiPenyusutan(DateTime? StartDate, double? TotalAmount, int Period, decimal? Persen)
        {
            decimal Result = 0;
            try
            {                
                DateTime dtStartDate = StartDate.Value.AddYears(Period);
                int? GetDays = (int)CalculateInMonth365(StartDate.Value, DateTime.Now);
                int? GetPeriodDays = (int)CalculateInMonth365(StartDate.Value, dtStartDate);

                decimal? Penyusutan_perHari = ((decimal)TotalAmount * Persen / 100) / 12;
                decimal? Pembagian_perhari = (decimal)GetDays / (decimal)GetPeriodDays;
                decimal? Nilai_penyusutan = Penyusutan_perHari * 12 * Period * Pembagian_perhari;
                decimal Nilai_buku = (decimal)TotalAmount - (decimal)Nilai_penyusutan;
                Result = Nilai_buku;
            }
            catch (Exception ex)
            {
                string err = ex.Message.ToString();
                return Result;                            
            }
        
            return Result;
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var objFromDb = await _unitOfWork.Items.GetAsync(id);
            if (objFromDb == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }

            await _unitOfWork.Items.RemoveAsync(objFromDb);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Delete Successful" });

        }

        [HttpGet]
        public async Task<IActionResult> GetTransferItem(int? id)
        {         
            var datalist = (from z in await _unitOfWork.ItemTransfer.GetAllAsync()
                            select new
                            {
                                id = z.Id,
                                itemid = z.ItemId,
                                previouslocation = z.PreviousLocation, //_unitOfWork.Location.Get(_unitOfWork.Room.Get(z.PreviousLocationId).IDLocation).Name,
                                previousroom = z.PreviousRoom,
                                currentlocation = z.CurrentLocation,//_unitOfWork.Location.Get(_unitOfWork.Room.Get(z.CurrentLocationId).IDLocation).Name,
                                currentroom = z.CurrentRoom,
                                desc = z.Description,
                                stranferdate = Convert.ToDateTime(z.TransferDate).ToString("dd-MM-yyyy"),
                                entrydate = z.EntryDate
                            }).Where(o=>o.itemid == id).OrderByDescending(i => i.entrydate).ToList();

            return Json(new { data = datalist });
        }
        [HttpGet]
        public async Task<IActionResult> GetServiceItem(int? id)
        {
            
            var datalist = (from z in await _unitOfWork.ItemService.GetAllAsync(includeProperties: "Items")
                            select new
                            {
                                id = z.Id,
                                itemid = z.ItemId,
                                servicedate = Convert.ToDateTime(z.ServiceDate).ToString("dd-MM-yyyy"),
                                serviceenddate = Convert.ToDateTime(z.ServiceEndDate).ToString("dd-MM-yyyy"),
                                desc = z.RepairDescription,
                                tecnician = z.Technician,
                                phone = z.PhoneNumber,
                                requestby = z.RequestBy,
                                costofrepair = z.CostOfRepair.HasValue ? z.CostOfRepair.Value.ToString("#,##0") : ""

                            }).Where(o => o.itemid == id).OrderByDescending(i => i.id).ToList();

            return Json(new { data = datalist });
        }
        [HttpPost]
        public async Task<JsonResult> GetDateCategory(int? id)
        {

            IEnumerable<Category> CatList = await _unitOfWork.Category.GetAllAsync();
            var res = CatList.Where(z => z.Id == id).Select(i => new { period_ = i.Period , persent_ = i.Percent.Value.ToString("#,##0.00") }).FirstOrDefault();    
            return Json(res);
        }

        [HttpGet]
        public IActionResult GetAllRoomAndLocation()
        {
            var datalist = (from z in _unitOfWork.Room.GetAll(includeProperties: "Location")
                            select new
                            {
                                id = z.Id,
                                name_of_room = z.Name,
                                description = z.Description,
                                name_of_location = z.Location.Name
                            }).ToList().OrderByDescending(o => o.id);
            return Json(new { data = datalist });
        }

        public int MonthDiff(DateTime tgl1, DateTime tgl2)
        {
            int m1;
            int m2;
            if (tgl1 < tgl2)
            {
                m1 = (tgl2.Month - tgl1.Month);//for years
                m2 = (tgl2.Year - tgl1.Year) * 12; //for months
            }
            else
            {
                m1 = (tgl1.Month - tgl2.Month);//for years
                m2 = (tgl1.Year - tgl2.Year) * 12; //for months
            }

            return m1 + m2;

        }
        public double CalculateInMonth365(DateTime tgl2, DateTime tgl1)
        {
            double Result = 0;
            Result = (tgl1 - tgl2).TotalDays ;
            return Result;
        }
    }
}
