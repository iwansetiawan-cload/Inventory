using E_OneWeb.DataAccess.Repository.IRepository;
using E_OneWeb.Models;
using E_OneWeb.Models.ViewModels;
using E_OneWeb.Utility;
using ExcelDataReader;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NPOI.SS.Formula.Functions;
using NPOI.XSSF.UserModel;
using org.omg.CORBA.DynAnyPackage;
using sun.misc;
using System.ComponentModel;
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

            ModelState.Clear();

            var ListItems = await _unitOfWork.Items.GetAllAsync();
           
            if (ListItems.Where(z => z.Code == vm.Items.Code).Count() > 0)
            {
                ModelState.AddModelError("Code", "Kode sudah digunakan");
            }

            if (vm.Items.Condition == null)
            {
                ModelState.AddModelError("Condition", "Kondisi harus diisi");
            }

            if (vm.Items.OriginOfGoods == null)
            {
                ModelState.AddModelError("OriginOfGoods", "Kepemilikan harus diisi");
            }

            if (vm.Items.Status == null)
            {
                ModelState.AddModelError("Status", "Status harus diisi");
            }

            var errorList = ModelState.Values.SelectMany(x => x.Errors).ToList();

            if (errorList.Count() == 0)
            {
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

        public async Task<IActionResult> ImportData()
        { 
            return View();
        }       
        public DataTable dtImport { get; set; }
        public List<ImportData> ListImportData = new List<ImportData>();
      
        [HttpPost]
        public async Task<IActionResult> ImportData(IFormFile fileUpload)
        {
            List<string> sheetNames = new List<string>();
            int valid = 0;
            int invalid = 0;
            if (fileUpload == null)
            {
                ViewBag.Message = "File is empty!";
                //return View();
                return Json(new { import = false, message = "File is empty!", jmlvalid = valid, jmlinvalid = invalid });
            }
           
            if (fileUpload.ContentType == "application/vnd.ms-excel" || fileUpload.ContentType == "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            {
                string fileName = Guid.NewGuid().ToString() + "_" + fileUpload.FileName;
                string rootPath = "D:\\My File\\";// _webHostEnvironment.ContentRootPath;
                string filePath = Path.Combine(rootPath, fileName);
                using (var stream = System.IO.File.Create(filePath))
                {
                    await fileUpload.CopyToAsync(stream);
                }

                DataTableCollection tables = ReadFromExcel(filePath, ref sheetNames);
                //dtImport = new DataTable();
               
                var ListItems = await _unitOfWork.Items.GetAllAsync();

                ItemsVM.staticGridView = new List<ImportData>();

                var ConditionList = _unitOfWork.Genmaster.GetAll().Where(z => z.GENFLAG == 1).ToList();
                var StatusList = _unitOfWork.Genmaster.GetAll().Where(z => z.GENFLAG == 2).ToList();
                var OwnershipList = _unitOfWork.Genmaster.GetAll().Where(z => z.GENFLAG == 3).ToList();
                           

                foreach (DataTable dt in tables)
                {

                    int RowNumber = dt.Rows.Count;

                    for (int i = 0; i < RowNumber; i++)
                    {
                        ImportData newImport = new ImportData();
                        newImport.No = dt.Rows[i][0].ToString();
                        newImport.Code = dt.Rows[i][1].ToString();
                        newImport.Name = dt.Rows[i][2].ToString();
                        newImport.Description = dt.Rows[i][3].ToString();
                        newImport.StartDate_String = dt.Rows[i][4].ToString();
                        newImport.Category = dt.Rows[i][5].ToString();
                        newImport.Price_String = dt.Rows[i][6].ToString();
                        newImport.Qty_String = dt.Rows[i][7].ToString();
                        newImport.OriginOfGoods = dt.Rows[i][8].ToString();
                        newImport.Location = dt.Rows[i][9].ToString();
                        newImport.Room = dt.Rows[i][10].ToString();
                        newImport.Condition = dt.Rows[i][11].ToString();
                        newImport.Status = dt.Rows[i][12].ToString();

                        #region Validation
                        if (ListItems.Where(z => z.Code == newImport.Code).Count() > 0)
                        {
                            newImport.Remarks = "Kode sudah digunakan. ";
                        }

                        if (string.IsNullOrEmpty(newImport.StartDate_String))
                        {

                            newImport.Remarks = newImport.Remarks + "Tanggal perolehan harus diisi. ";
                        }
                        else
                        {
                            newImport.StartDate = DateTime.Parse(newImport.StartDate_String);
                        }

                        if (string.IsNullOrEmpty(newImport.Category))
                        {

                            newImport.Remarks = newImport.Remarks + "Katagori harus diisi. ";
                        }
                        else
                        {
                            var category = await _unitOfWork.Category.GetAllAsync();
                            int categoryid = category.Where(z => z.Name == newImport.Category).Select(i => i.Id).FirstOrDefault();

                            if (categoryid > 0)
                            {
                                newImport.CategoryId = categoryid;
                            }
                            else
                            {
                                newImport.Remarks = newImport.Remarks + "Nama katagori tidak ditemukan. ";
                            }

                        }

                        if (string.IsNullOrEmpty(newImport.Price_String))
                        {

                            newImport.Remarks = newImport.Remarks + "Nilai aset harus diisi. ";
                        }
                        else
                        {
                            double number;
                            if (Double.TryParse(newImport.Price_String, out number))
                            {
                                newImport.Price = double.Parse(newImport.Price_String);
                            }
                            else
                            {
                                newImport.Remarks = newImport.Remarks + "Format nilai aset harus numerik. ";
                            }
                        }
                        if (string.IsNullOrEmpty(newImport.Qty_String))
                        {

                            newImport.Remarks = newImport.Remarks + "Jumlah aset harus diisi. ";
                        }
                        else
                        {
                            int number;
                            if (int.TryParse(newImport.Qty_String, out number))
                            {
                                newImport.Qty = int.Parse(newImport.Qty_String);
                            }
                            else
                            {
                                newImport.Remarks = newImport.Remarks + "Format nilai aset harus numerik. ";
                            }
                        }

                        if (newImport.Price != null && newImport.Qty != null)
                        {
                            newImport.TotalAmount = newImport.Price * newImport.Qty;
                        }

                        if (string.IsNullOrEmpty(newImport.OriginOfGoods))
                        {

                            newImport.Remarks = newImport.Remarks + "Kepemilikan harus diisi. ";
                        }
                        else
                        {
                            int OwnerShipId = OwnershipList.Where(z => z.GENNAME == newImport.OriginOfGoods).Select(i => i.IDGEN).FirstOrDefault();

                            if (OwnerShipId > 0)
                            {
                                newImport.OwnerShipId = OwnerShipId;
                            }
                            else
                            {
                                newImport.Remarks = newImport.Remarks + "Nama kepemilikan tidak ditemukan. ";
                            }

                        }

                        if (string.IsNullOrEmpty(newImport.Location))
                        {

                            newImport.Remarks = newImport.Remarks + "Nama gedung harus diisi. ";
                        }
                        else
                        {
                            int locationid = _unitOfWork.Location.GetAll().Where(z => z.Name == newImport.Location).Select(o => o.Id).FirstOrDefault();

                            if (locationid == 0)
                            {
                                newImport.Remarks = newImport.Remarks + "Nama gedung tidak ditemukan. ";
                            }

                            if (string.IsNullOrEmpty(newImport.Room))
                            {

                                newImport.Remarks = newImport.Remarks + "Nama ruangan harus diisi. ";
                            }
                            else
                            {
                                int roomid = _unitOfWork.Room.GetAll().Where(z => z.Name == newImport.Room && z.IDLocation == locationid).Select(o => o.Id).FirstOrDefault();
                                if (roomid > 0)
                                {
                                    newImport.RoomId = roomid;
                                }
                                else
                                {
                                    newImport.Remarks = newImport.Remarks + "Nama ruangan tidak ditemukan. ";
                                }
                            }

                        }

                        if (string.IsNullOrEmpty(newImport.Condition))
                        {

                            newImport.Remarks = newImport.Remarks + "Kondisi harus diisi. ";
                        }
                        else
                        {
                            int ConditionId = ConditionList.Where(z => z.GENNAME == newImport.Condition).Select(i => i.IDGEN).FirstOrDefault();
                            if (ConditionId > 0)
                            {
                                newImport.ConditionId = ConditionId;
                            }
                            else
                            {
                                newImport.Remarks = newImport.Remarks + "Nama Kondisi tidak ditemukan. ";
                            }

                        }

                        if (string.IsNullOrEmpty(newImport.Status))
                        {

                            newImport.Remarks = newImport.Remarks + "Status harus diisi. ";
                        }
                        else
                        {
                            int StatusId = StatusList.Where(z => z.GENNAME == newImport.Status).Select(i => i.IDGEN).FirstOrDefault();
                            if (StatusId > 0)
                            {
                                newImport.StatusId = StatusId;
                            }
                            else
                            {
                                newImport.Remarks = newImport.Remarks + "Nama status tidak ditemukan. ";
                            }

                        }

                        #endregion

                        if (string.IsNullOrEmpty(newImport.Remarks))
                        {
                            newImport.ValidInvalid = "Valid";
                            valid++;
                        }
                        else
                        {
                            newImport.ValidInvalid = "Invalid";
                            invalid++;
                        }

                        ItemsVM.staticGridView.Add(newImport);
                    }
                                        
                    //if (dtImport.Rows.Count > 0)
                    //{
                    //    DataTable dtTemp = ToDataTable(newImport);
                    //    dtImport.Rows.Add(dtTemp.Rows[0].ItemArray);
                    //}
                    //else
                    //{
                    //    dtImport = ToDataTable(newImport);
                    //}
                   
                }

                //this is an edit and we need to remove old image            
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }


            }
          
            return Json(new { import = true, message = "Import Successful", jmlvalid = valid, jmlinvalid = invalid });
          
        }

        [HttpGet]
        public async Task<IActionResult> GetImportData()
        {

            var datalist = (from z in ItemsVM.staticGridView.ToList()
                            select new
                            {
                                no = z.No,
                                code = z.Code,
                                name = z.Name,
                                validinvalid = z.ValidInvalid,
                                remarks = z.Remarks,
                                desc = z.Description,
                                startdate = z.StartDate.Value.ToString("dd/MM/yyyy"),
                                category = z.Category,
                                price = z.Price != null ? z.Price.Value.ToString("#,##0") : z.Price_String,
                                qty = z.Qty != null ? z.Qty.Value.ToString("#,##0") : z.Qty_String,
                                totalamount = z.TotalAmount != null ? z.TotalAmount.Value.ToString("#,##0") : "0",
                                ownership = z.OriginOfGoods,
                                location = z.Location,
                                room = z.Room,
                                condition = z.Condition,
                                status = z.Status
                            }).ToList();

            return Json(new { data = datalist });
        }
        //public DataTable ToDataTable(object Item)
        //{
        //    DataTable dataTable = new DataTable();
        //    PropertyDescriptorCollection propertyDescriptorCollection =
        //        TypeDescriptor.GetProperties(Item.GetType());
        //    for (int i = 0; i < propertyDescriptorCollection.Count; i++)
        //    {
        //        PropertyDescriptor propertyDescriptor = propertyDescriptorCollection[i];
        //        Type type = propertyDescriptor.PropertyType;

        //        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
        //            type = Nullable.GetUnderlyingType(type);


        //        dataTable.Columns.Add(propertyDescriptor.Name, type);
        //    }
        //    object[] values = new object[propertyDescriptorCollection.Count];

        //    for (int i = 0; i < values.Length; i++)
        //    {
        //        values[i] = propertyDescriptorCollection[i].GetValue(Item);
        //    }

        //    dataTable.Rows.Add(values);

        //    return dataTable;
        //}
        DataTableCollection ReadFromExcel(string filePath, ref List<string> sheetNames)
        {
            try
            {
                DataTableCollection tableCollection = null;

                using (var stream = System.IO.File.Open(filePath, FileMode.Open, FileAccess.Read))
                {
                    System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

                    using (IExcelDataReader reader = ExcelReaderFactory.CreateReader(stream))
                    {
                        DataSet result = reader.AsDataSet(new ExcelDataSetConfiguration()
                        {
                            ConfigureDataTable = (_) => new ExcelDataTableConfiguration() { UseHeaderRow = true }
                        });

                        tableCollection = result.Tables;

                        foreach (DataTable table in tableCollection)
                        {
                            sheetNames.Add(table.TableName);
                        }
                    }
                }

                return tableCollection;
            }
            catch (Exception)
            {
                return null;
            }
        }

        [HttpPost]
        public async Task<IActionResult> ProcessImport()
        {
            try
            {               
                var datalist = ItemsVM.staticGridView.Where(z=>z.ValidInvalid == "Valid").ToList();

                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                var user = _unitOfWork.ApplicationUser.GetFirstOrDefault(u => u.Id == claim.Value);

                foreach (var d in datalist)
                {
                    Items newItem = new Items();
                    Category Catogory = await _unitOfWork.Category.GetAsync(d.CategoryId);
                    Room Rooms = _unitOfWork.Room.Get(d.RoomId);        
                    newItem.Code = d.Code;
                    newItem.Name = d.Name;                   
                    newItem.Description = d.Description;
                    newItem.StartDate = d.StartDate;
                    newItem.Price = d.Price;
                    newItem.Qty = d.Qty;    
                    newItem.TotalAmount = d.TotalAmount;
                    newItem.Percent = Catogory.Percent;    
                    newItem.Period  = Catogory.Period;
                    newItem.DepreciationExpense = d.DepreciationExpense;
                    newItem.Status = d.StatusId;
                    newItem.OriginOfGoods = d.OwnerShipId.ToString();
                    newItem.RoomId = d.RoomId;
                    newItem.Room = Rooms;                   
                    newItem.CategoryId = d.CategoryId;
                    newItem.Category = Catogory;
                    newItem.Condition = d.ConditionId;
                    decimal Nilai_Buku = GetNilaiPenyusutan(d.StartDate, d.TotalAmount, (int)Catogory.Period, Catogory.Percent);
                    newItem.DepreciationExpense = Math.Round((double)Nilai_Buku);
                    newItem.EntryBy = user.Name;
                    newItem.EntryDate = DateTime.Now;
                    await _unitOfWork.Items.AddAsync(newItem);
                    _unitOfWork.Save();
                }

                if (datalist.Count() == 0)
                {
                    TempData["Failed"] = "Data not found";
                    return Json(new { success = false, message = "Data not found" });
                }
                else
                {
                    ItemsVM.staticGridView.Clear();
                    TempData["Success"] = "Successfully Process";
                    return Json(new { success = true, message = "Process Successful" });
                }

               
            }
            catch (Exception)
            {
                TempData["Failed"] = "Error Process";
                return Json(new { success = false, message = "Process Error" });
            }


        }
        public async Task<IActionResult> GetTemplate()
        {
            var workbook = new XSSFWorkbook();
            var sheet = workbook.CreateSheet("Sheet1");
            var rowHeader = sheet.CreateRow(0);

            var properties = typeof(T).GetProperties();

            //header
            var font = workbook.CreateFont();
            font.IsBold = true;
            var style = workbook.CreateCellStyle();
            style.SetFont(font);          

            var cell = rowHeader.CreateCell(0);
            cell.SetCellValue("No");
            cell.CellStyle = style;

            cell = rowHeader.CreateCell(1);
            cell.SetCellValue("Kode");
            cell.CellStyle = style;

            cell = rowHeader.CreateCell(2);
            cell.SetCellValue("Nama Aset");
            cell.CellStyle = style;

            cell = rowHeader.CreateCell(3);
            cell.SetCellValue("Keterangan");
            cell.CellStyle = style;

            cell = rowHeader.CreateCell(4);
            cell.SetCellValue("Tgl. Perolehan");
            cell.CellStyle = style;

            cell = rowHeader.CreateCell(5);
            cell.SetCellValue("Katagori");
            cell.CellStyle = style;

            cell = rowHeader.CreateCell(6);
            cell.SetCellValue("Nilai Aset");
            cell.CellStyle = style;

            cell = rowHeader.CreateCell(7);
            cell.SetCellValue("Jumlah");
            cell.CellStyle = style;

            cell = rowHeader.CreateCell(8);
            cell.SetCellValue("Kepemilikan");
            cell.CellStyle = style;

            cell = rowHeader.CreateCell(9);
            cell.SetCellValue("Nama Gedung");
            cell.CellStyle = style;

            cell = rowHeader.CreateCell(10);
            cell.SetCellValue("Nama Ruangan");
            cell.CellStyle = style;

            cell = rowHeader.CreateCell(11);
            cell.SetCellValue("Kondisi");
            cell.CellStyle = style;

            cell = rowHeader.CreateCell(12);
            cell.SetCellValue("Status");
            cell.CellStyle = style;

            var stream = new MemoryStream();
            workbook.Write(stream);
            var content = stream.ToArray();

            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Template_ImportAset.xlsx");
        }
    }
}
