using Dapper;
using E_OneWeb.DataAccess.Repository.IRepository;
using E_OneWeb.Models;
using E_OneWeb.Models.ViewModels;
using E_OneWeb.Utility;
using ExcelDataReader;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Hosting;
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
using static com.sun.tools.@internal.xjc.reader.xmlschema.bindinfo.BIConversion;

namespace E_OneWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
    public class ItemsController : Controller
    {

        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _hostEnvironment;
        public ItemsController(IUnitOfWork unitOfWork, IWebHostEnvironment hostEnvironment) 
        {
            _unitOfWork = unitOfWork;
            _hostEnvironment = hostEnvironment;
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
           
            if (ListItems.Where(z => z.Code == vm.Items.Code).Count() > 0 && vm.Items.Id == 0)
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
            try
            {
                List<string> sheetNames = new List<string>();
                int valid = 0;
                int invalid = 0;
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                var user = _unitOfWork.ApplicationUser.GetFirstOrDefault(u => u.Id == claim.Value);

                if (fileUpload == null)
                {
                    ViewBag.Message = "File is empty!";
                    //return View();
                    return Json(new { import = false, message = "File is empty!", jmlvalid = valid, jmlinvalid = invalid });
                }

                if (fileUpload.ContentType == "application/vnd.ms-excel" || fileUpload.ContentType == "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                {

                    List<ImportItems> ListImport = _unitOfWork.ImportItems.GetAll().Where(z => z.EntryBy == user.Name).ToList();
                    _unitOfWork.ImportItems.RemoveRange(ListImport);
                    _unitOfWork.Save();

                    string webRootPath = _hostEnvironment.WebRootPath;
                    string fileName = Guid.NewGuid().ToString() + "_" + fileUpload.FileName;
                    //string rootPath = "C:\\Software\\";// _webHostEnvironment.ContentRootPath;
                    var uploads = Path.Combine(webRootPath, @"images\products");
                    string filePath = Path.Combine(uploads, fileName);

                    using (var stream = System.IO.File.Create(filePath))
                    {
                        await fileUpload.CopyToAsync(stream);
                    }
                    DataTableCollection tables = ReadFromExcel(filePath, ref sheetNames);  

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

                            ImportItems importItems = new ImportItems();
                            importItems.Number = i + 1;
                            importItems.Code = newImport.Code;
                            importItems.Name = newImport.Name;
                            importItems.Description = newImport.Description;
                            importItems.StartDate_String = newImport.StartDate_String;
                            //if (!string.IsNullOrEmpty(newImport.StartDate_String))
                            //{
                            //    importItems.StartDate = DateTime.Parse(newImport.StartDate_String);
                            //}                     
                            importItems.Category_String = newImport.Category;
                            importItems.Price_String = newImport.Price_String;
                            importItems.Qty_String = newImport.Qty_String;
                            importItems.Ownership = newImport.OriginOfGoods;
                            importItems.LocationName = newImport.Location;
                            importItems.RoomName = newImport.Room;
                            importItems.Condition_String = newImport.Condition;
                            importItems.Status_String = newImport.Status;
                            importItems.EntryBy = user.Name;
                            importItems.EntryDate = DateTime.Now;
                            importItems.ImportStatus = "Valid";

                            _unitOfWork.ImportItems.Add(importItems);
                            _unitOfWork.Save();
                        }

                        var parameter = new DynamicParameters();
                        parameter.Add("@UserName", user.Name);
                        _unitOfWork.SP_Call.Execute(SD.Proc_Validation_ImportItems, parameter);             

                    }

                    //this is an edit and we need to remove old image            
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }

                }

                return Json(new { import = true, message = "Import Successful", jmlvalid = valid, jmlinvalid = invalid });
            }
            catch (Exception ex)
            {
                string errorstring = ex.Message.ToString();
                return Json(new { import = false, message = "Import Error", jmlvalid = 0, jmlinvalid = 0 });
            }
            
          
        }

        public ActionResult CountImport()
        {
            bool success = true;
            string error = string.Empty;
            int valid = 0;
            int invalid = 0;
            try
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                var user = _unitOfWork.ApplicationUser.GetFirstOrDefault(u => u.Id == claim.Value);
                valid = _unitOfWork.ImportItems.GetAll().Where(z => z.EntryBy == user.Name && z.ImportStatus == "Valid").Count(); 
                invalid = _unitOfWork.ImportItems.GetAll().Where(z => z.EntryBy == user.Name && z.ImportStatus == "Invalid").Count();
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
            }

            return Json(new { success = success, error = error, valid = valid, invalid = invalid });

        }

        [HttpGet]
        public async Task<IActionResult> GetImportData()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var user = _unitOfWork.ApplicationUser.GetFirstOrDefault(u => u.Id == claim.Value);
            //var datalist = (from z in ItemsVM.staticGridView.ToList()
            //                select new
            //                {
            //                    no = z.No,
            //                    code = z.Code,
            //                    name = z.Name,
            //                    validinvalid = z.ValidInvalid,
            //                    remarks = z.Remarks,
            //                    desc = z.Description,
            //                    startdate = z.StartDate.Value.ToString("dd/MM/yyyy"),
            //                    category = z.Category,
            //                    price = z.Price != null ? z.Price.Value.ToString("#,##0") : z.Price_String,
            //                    qty = z.Qty != null ? z.Qty.Value.ToString("#,##0") : z.Qty_String,
            //                    totalamount = z.TotalAmount != null ? z.TotalAmount.Value.ToString("#,##0") : "0",
            //                    ownership = z.OriginOfGoods,
            //                    location = z.Location,
            //                    room = z.Room,
            //                    condition = z.Condition,
            //                    status = z.Status
            //                }).ToList();

            var datalist = (from z in _unitOfWork.ImportItems.GetAll().Where(z=>z.EntryBy == user.Name)
                            select new
                            {
                                no = z.Number,
                                code = z.Code,
                                name = z.Name,
                                validinvalid = z.ImportStatus,
                                remarks = z.ImportRemark,
                                desc = z.Description,
                                startdate = z.StartDate_String,
                                category = z.Category_String,
                                price = z.Price != null ? z.Price.Value.ToString("#,##0") : z.Price_String,
                                qty = z.Qty != null ? z.Qty.Value.ToString("#,##0") : z.Qty_String,
                                totalamount = z.TotalAmount != null ? z.TotalAmount.Value.ToString("#,##0") : "0",
                                ownership = z.Ownership,
                                location = z.LocationName,
                                room = z.RoomName,
                                condition = z.Condition_String,
                                status = z.Status_String
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
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                var user = _unitOfWork.ApplicationUser.GetFirstOrDefault(u => u.Id == claim.Value);

                var parameter = new DynamicParameters();
                parameter.Add("@UserName", user.Name);
                _unitOfWork.SP_Call.Execute(SD.Proc_Process_ImportItems, parameter);

                List<ImportItems> ListImport = _unitOfWork.ImportItems.GetAll().Where(z => z.EntryBy == user.Name).ToList();

                _unitOfWork.ImportItems.RemoveRange(ListImport);
                _unitOfWork.Save();

                TempData["Success"] = "Successfully Process";
                return Json(new { success = true, message = "Process Successful" });

                //var datalist = ItemsVM.staticGridView.Where(z=>z.ValidInvalid == "Valid").ToList();

                //var claimsIdentity = (ClaimsIdentity)User.Identity;
                //var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                //var user = _unitOfWork.ApplicationUser.GetFirstOrDefault(u => u.Id == claim.Value);

                //foreach (var d in datalist)
                //{
                //    Items newItem = new Items();
                //    Category Catogory = await _unitOfWork.Category.GetAsync(d.CategoryId);
                //    Room Rooms = _unitOfWork.Room.Get(d.RoomId);        
                //    newItem.Code = d.Code;
                //    newItem.Name = d.Name;                   
                //    newItem.Description = d.Description;
                //    newItem.StartDate = d.StartDate;
                //    newItem.Price = d.Price;
                //    newItem.Qty = d.Qty;    
                //    newItem.TotalAmount = d.TotalAmount;
                //    newItem.Percent = Catogory.Percent;    
                //    newItem.Period  = Catogory.Period;
                //    newItem.DepreciationExpense = d.DepreciationExpense;
                //    newItem.Status = d.StatusId;
                //    newItem.OriginOfGoods = d.OwnerShipId.ToString();
                //    newItem.RoomId = d.RoomId;
                //    newItem.Room = Rooms;                   
                //    newItem.CategoryId = d.CategoryId;
                //    newItem.Category = Catogory;
                //    newItem.Condition = d.ConditionId;
                //    decimal Nilai_Buku = GetNilaiPenyusutan(d.StartDate, d.TotalAmount, (int)Catogory.Period, Catogory.Percent);
                //    newItem.DepreciationExpense = Math.Round((double)Nilai_Buku);
                //    newItem.EntryBy = user.Name;
                //    newItem.EntryDate = DateTime.Now;
                //    await _unitOfWork.Items.AddAsync(newItem);
                //    _unitOfWork.Save();
                //}

                //if (datalist.Count() == 0)
                //{
                //    TempData["Failed"] = "Data not found";
                //    return Json(new { success = false, message = "Data not found" });
                //}
                //else
                //{
                //    ItemsVM.staticGridView.Clear();
                //    TempData["Success"] = "Successfully Process";
                //    return Json(new { success = true, message = "Process Successful" });
                //}

               
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
