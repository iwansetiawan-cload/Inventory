using Dapper;
using E_OneWeb.DataAccess.Repository;
using E_OneWeb.DataAccess.Repository.IRepository;
using E_OneWeb.Models;
using E_OneWeb.Models.ViewModels;
using E_OneWeb.Utility;
using ExcelDataReader;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Hosting;
using NPOI.SS.Formula.Functions;
using NPOI.XSSF.UserModel;
using System.Data;
using System.Diagnostics;
using System.Security.Claims;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace E_OneWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
    public class ClassListController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _hostEnvironment;
        public ClassListController( IUnitOfWork unitOfWork, IWebHostEnvironment hostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _hostEnvironment = hostEnvironment;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]   
        public async Task<IActionResult> GetAll()
        {

            var datalist = (from z in await _unitOfWork.RoomReservationAdmin.GetAllAsync(includeProperties: "Room")
                                            select new
                                            {
                                                id = z.Id,
                                                roomname = z.RoomName,
                                                locationname = z.LocationName,
                                                status = z.Status,
                                                statusid = z.StatusId,
                                                bookingby = z.BookingBy,
                                                flag = z.Flag,
                                                bookingdate = z.BookingStartDate != null ? Convert.ToDateTime(z.BookingStartDate).ToString("dd-MM-yyyy") : "",
                                                bookingenddate = z.BookingEndDate != null ? Convert.ToDateTime(z.BookingStartDate).ToString("HH:mm") + " - " + Convert.ToDateTime(z.BookingEndDate).ToString("HH:mm") : ""
                                            }).Where(u=>u.flag == 2).ToList();

            return Json(new { data = datalist });
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
                                name_of_location = z.Location.Name,
                                name_of_room_and_location = z.Name + " (" + z.Location.Name + ")"
                            }).ToList().OrderByDescending(o => o.id);
            return Json(new { data = datalist });
        }

        public async Task<IActionResult> Upsert(int? id)
        {
            //IEnumerable<Items> ItemsList = await _unitOfWork.Items.GetAllAsync();          
            RoomReservationAdminVM vm = new RoomReservationAdminVM()
            {
                RoomReservationAdmin = new RoomReservationAdmin(),
                RoomList = _unitOfWork.Room.GetAll().Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                })
           
            };
            ViewBag.Status = "";
            if (id == null)
            {
                //this is for create
                vm.RoomReservationAdmin.BookingStartDate = DateTime.Now;
                vm.RoomReservationAdmin.BookingEndDate = DateTime.Now;

                return View(vm);
            }

            vm.RoomReservationAdmin = await _unitOfWork.RoomReservationAdmin.GetAsync(id.GetValueOrDefault());
            if (vm.RoomReservationAdmin == null)
            {
                return NotFound();
            }

          
            return View(vm);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(RoomReservationAdminVM vm)
        {
            //var errorval = ModelState.Values.SelectMany(i => i.Errors);
            //if (ModelState.IsValid)
            //{
                
            //}
            var Gen_4 = _unitOfWork.Genmaster.GetAll().Where(z => z.GENFLAG == 4 && z.GENVALUE == 0).FirstOrDefault();
            RoomReservationAdmin roomReservationAdmin = await _unitOfWork.RoomReservationAdmin.GetAsync(vm.RoomReservationAdmin.Id);
            Room room = _unitOfWork.Room.Get(vm.RoomReservationAdmin.RoomId);
            vm.RoomReservationAdmin.Room = room;

            DateTime orderDate = Convert.ToDateTime(vm.RoomReservationAdmin.BookingStartDate);
            TimeSpan orderTime = vm.ClockStart;
            DateTime orderDateTimeStart = orderDate + orderTime;

            DateTime orderDateEnd = Convert.ToDateTime(vm.RoomReservationAdmin.BookingStartDate);
            TimeSpan orderTimeEnd = vm.ClockEnd;
            DateTime orderDateTimeEnd = orderDateEnd + orderTimeEnd;

            vm.RoomReservationAdmin.StatusId = Gen_4.IDGEN;
            vm.RoomReservationAdmin.Status = Gen_4.GENNAME;
            vm.RoomReservationAdmin.Flag = 2;
            vm.RoomReservationAdmin.BookingStartDate = orderDateTimeStart;
            vm.RoomReservationAdmin.BookingEndDate = orderDateTimeEnd;

            if (vm.RoomReservationAdmin.Id == 0)
            {
                await _unitOfWork.RoomReservationAdmin.AddAsync(vm.RoomReservationAdmin);
                ViewBag.Status = "Save Success";

            }
            else
            {
                _unitOfWork.RoomReservationAdmin.Update(vm.RoomReservationAdmin);
                ViewBag.Status = "Edit Success";
            }

            _unitOfWork.Save();

            //return View(vm);
            return RedirectToAction(nameof(Index));
        }
        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var objFromDb = await _unitOfWork.RoomReservationAdmin.GetAsync(id);
            if (objFromDb == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }

            await _unitOfWork.RoomReservationAdmin.RemoveAsync(objFromDb);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Delete Successful" });

        }

        #region Import Data
        public async Task<IActionResult> ImportData()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var user = _unitOfWork.ApplicationUser.GetFirstOrDefault(u => u.Id == claim.Value);
            List<ImportBookingClass> ListImport = _unitOfWork.ImportBookingClass.GetAll().Where(z => z.EntryBy == user.Name).ToList();
            _unitOfWork.ImportBookingClass.RemoveRange(ListImport);
            _unitOfWork.Save();
            return View();
        }
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

                    List<ImportBookingClass> ListImport = _unitOfWork.ImportBookingClass.GetAll().Where(z => z.EntryBy == user.Name).ToList();
                    _unitOfWork.ImportBookingClass.RemoveRange(ListImport);
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
                            ImportBookingClass newImport = new ImportBookingClass();
                            newImport.LocationName = dt.Rows[i][1].ToString();
                            newImport.RoomName = dt.Rows[i][2].ToString();
                            newImport.StartDate = dt.Rows[i][3].ToString();
                            newImport.EndDate = dt.Rows[i][4].ToString();
                            newImport.EntryBy = user.Name;
                            newImport.EntryDate = DateTime.Now;
                            newImport.ImportStatus = "Valid";
                            newImport.Number = i + 1;                                                  

                            _unitOfWork.ImportBookingClass.Add(newImport);
                            _unitOfWork.Save();
                        }

                        var parameter = new DynamicParameters();
                        parameter.Add("@UserName", user.Name);
                        _unitOfWork.SP_Call.Execute(SD.Proc_Validation_ImportBookingRoom, parameter);

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
                valid = _unitOfWork.ImportBookingClass.GetAll().Where(z => z.EntryBy == user.Name && z.ImportStatus == "Valid").Count();
                invalid = _unitOfWork.ImportBookingClass.GetAll().Where(z => z.EntryBy == user.Name && z.ImportStatus == "Invalid").Count();
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

            var datalist = (from z in _unitOfWork.ImportBookingClass.GetAll().Where(z => z.EntryBy == user.Name)
                            select new
                            {
                                no = z.Number,
                                validinvalid = z.ImportStatus,
                                remarks = z.ImportRemark,                                
                                location = z.LocationName,
                                room = z.RoomName,
                                startdate = z.StartDate,
                                enddate = z.EndDate
                            }).ToList();

            return Json(new { data = datalist });
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
                _unitOfWork.SP_Call.Execute(SD.Proc_Process_BookingRoom, parameter);

                List<ImportBookingClass> ListImport = _unitOfWork.ImportBookingClass.GetAll().Where(z => z.EntryBy == user.Name).ToList();

                _unitOfWork.ImportBookingClass.RemoveRange(ListImport);
                _unitOfWork.Save();

                TempData["Success"] = "Successfully Process";
                return Json(new { success = true, message = "Process Successful" });

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
            cell.SetCellValue("Nama Gedung");
            cell.CellStyle = style;

            cell = rowHeader.CreateCell(2);
            cell.SetCellValue("Nama Ruangan");
            cell.CellStyle = style;

            cell = rowHeader.CreateCell(3);
            cell.SetCellValue("Mulai Format(dd/MM/yyyy HH:mm)");
            cell.CellStyle = style;

            cell = rowHeader.CreateCell(4);
            cell.SetCellValue("Sampai Format(dd/MM/yyyy HH:mm)");
            cell.CellStyle = style;

            var stream = new MemoryStream();
            workbook.Write(stream);
            var content = stream.ToArray();

            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Template_ImportBookingClass.xlsx");
        }
        #endregion
    }
}
