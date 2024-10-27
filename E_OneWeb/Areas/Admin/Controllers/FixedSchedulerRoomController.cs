using Dapper;
using E_OneWeb.DataAccess.Repository.IRepository;
using E_OneWeb.Models;
using E_OneWeb.Utility;
using ExcelDataReader;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NPOI.XSSF.UserModel;
using System.Data;
using System.Security.Claims;
using NPOI.SS.Formula.Functions;
using sun.misc;
using E_OneWeb.Models.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq;

namespace E_OneWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
    public class FixedSchedulerRoomController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _hostEnvironment;
        public FixedSchedulerRoomController(IUnitOfWork unitOfWork, IWebHostEnvironment hostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _hostEnvironment = hostEnvironment;
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
            try
            {
                int Number = 1;
                var datalist = (from z in await _unitOfWork.FixedSchedulerRoom.GetAllAsync(includeProperties: "Room")
                                select new
                                {
                                    no = Number++,
                                    id = z.Id,
                                    roomname = z.RoomName,
                                    locationname = z.LocationName,
                                    days = z.Days,
                                    clock = z.Start_Clock + "-" + z.End_Clock,
                                    prodi = z.Prodi,
                                    study = z.Study,
                                    semester = z.Semester,
                                    dosen = z.Dosen
                                }).ToList();

                return Json(new { data = datalist });
            }
            catch (Exception ex)
            {
                string error = ex.Message.ToString();
                return Json(new { data = "" });
            }

        }


        #region Import Data
        public async Task<IActionResult> ImportData()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var user = _unitOfWork.ApplicationUser.GetFirstOrDefault(u => u.Id == claim.Value);
            List<ImportFixedSchedulerRoom> ListImport = _unitOfWork.ImportFixedSchedulerRoom.GetAll().Where(z => z.EntryBy == user.Name).ToList();
            if (ListImport.Count > 0)
            {
                _unitOfWork.ImportFixedSchedulerRoom.RemoveRange(ListImport);
                _unitOfWork.Save();
            }            
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

                    List<ImportFixedSchedulerRoom> ListImport = _unitOfWork.ImportFixedSchedulerRoom.GetAll().Where(z => z.EntryBy == user.Name).ToList();
                    _unitOfWork.ImportFixedSchedulerRoom.RemoveRange(ListImport);
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

                    string valMonth = DateTime.Now.Month.ToString();
					string valyear = DateTime.Now.Year.ToString();
					string dtNow = DateTime.Now.ToString("yyyy-MM-dd ");					

					foreach (DataTable dt in tables)
                    {
                        int RowNumber = dt.Rows.Count;

                        for (int i = 0; i < RowNumber; i++)
                        {
							var GetSchedulerRoom = await _unitOfWork.FixedSchedulerRoom.GetAllAsync();
							List<ImportFixedSchedulerRoom> GetImport = _unitOfWork.ImportFixedSchedulerRoom.GetAll().Where(z => z.EntryBy == user.Name).ToList();
							ImportFixedSchedulerRoom newImport = new ImportFixedSchedulerRoom();
                            newImport.LocationName = dt.Rows[i][1].ToString();
                            newImport.RoomName = dt.Rows[i][2].ToString();
                            newImport.Days = dt.Rows[i][3].ToString().ToLower().Replace(" ","");      
                            newImport.Start_Clock = dt.Rows[i][4].ToString();
                            if (newImport.Start_Clock != null)
                            {
                                string valdays = dtNow + newImport.Start_Clock;
                                newImport.ValStart_Clock = DateTime.Parse(valdays);
                            }
                            newImport.End_Clock = dt.Rows[i][5].ToString();
							if (newImport.End_Clock != null)
							{
								string valdays = dtNow + newImport.End_Clock;
								newImport.ValEnd_Clock = DateTime.Parse(valdays);
							}
							newImport.Prodi = dt.Rows[i][6].ToString();
                            newImport.Study = dt.Rows[i][7].ToString();
                            newImport.Semester = dt.Rows[i][8].ToString();
                            newImport.Dosen = dt.Rows[i][9].ToString();
                            newImport.EntryBy = user.Name;
                            newImport.EntryDate = DateTime.Now;                            
                            newImport.Number = i + 1;

							GetSchedulerRoom = GetSchedulerRoom.Where(z => ((newImport.ValStart_Clock >= z.ValStart_Clock && newImport.ValStart_Clock < z.ValEnd_Clock)
							|| (newImport.ValEnd_Clock > z.ValStart_Clock && newImport.ValEnd_Clock <= z.ValEnd_Clock)
							|| (newImport.ValStart_Clock <= z.ValStart_Clock && newImport.ValEnd_Clock >= z.ValEnd_Clock))
							&& z.LocationName == newImport.LocationName && z.RoomName == newImport.RoomName && z.Days == newImport.Days).ToList();

                            if (GetSchedulerRoom.Count() > 0)
                            {
								newImport.ImportStatus = "Invalid";
								newImport.ImportRemark = "Jam sudah digunakan";
							}
                            else 
                            {
								GetImport = GetImport.Where(z => ((newImport.ValStart_Clock >= z.ValStart_Clock && newImport.ValStart_Clock < z.ValEnd_Clock)
							    || (newImport.ValEnd_Clock > z.ValStart_Clock && newImport.ValEnd_Clock <= z.ValEnd_Clock)
							    || (newImport.ValStart_Clock <= z.ValStart_Clock && newImport.ValEnd_Clock >= z.ValEnd_Clock))
							    && z.LocationName == newImport.LocationName && z.RoomName == newImport.RoomName && z.Days == newImport.Days).ToList();

								if (GetImport.Count > 0)
								{
									newImport.ImportStatus = "Invalid";
									newImport.ImportRemark = "Jam sudah digunakan";
								}
								else
								{
									newImport.ImportStatus = "Valid";
								}
							}				

							_unitOfWork.ImportFixedSchedulerRoom.Add(newImport);
                            _unitOfWork.Save();
                        }

                        var parameter = new DynamicParameters();
                        parameter.Add("@UserName", user.Name);
                        _unitOfWork.SP_Call.Execute(SD.Proc_Validation_ImportFixedSchedulerRoom, parameter);

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
                valid = _unitOfWork.ImportFixedSchedulerRoom.GetAll().Where(z => z.EntryBy == user.Name && z.ImportStatus == "Valid").Count();
                invalid = _unitOfWork.ImportFixedSchedulerRoom.GetAll().Where(z => z.EntryBy == user.Name && z.ImportStatus == "Invalid").Count();
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

            var datalist = (from z in _unitOfWork.ImportFixedSchedulerRoom.GetAll().Where(z => z.EntryBy == user.Name)
                            select new
                            {
                                no = z.Number,
                                validinvalid = z.ImportStatus,
                                remarks = z.ImportRemark,
                                location = z.LocationName,
                                room = z.RoomName,
                                days = z.Days,
                                startclock = z.Start_Clock,
                                endclock = z.End_Clock,
                                prodi = z.Prodi,
                                study = z.Study,
                                semester = z.Semester,
                                dosen = z.Dosen
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
                _unitOfWork.SP_Call.Execute(SD.Proc_Process_ImportFixedSchedulerRoom, parameter);

                List<ImportFixedSchedulerRoom> ListImport = _unitOfWork.ImportFixedSchedulerRoom.GetAll().Where(z => z.EntryBy == user.Name).ToList();

                _unitOfWork.ImportFixedSchedulerRoom.RemoveRange(ListImport);
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
            cell.SetCellValue("Hari");
            cell.CellStyle = style;

            cell = rowHeader.CreateCell(4);
            cell.SetCellValue("Jam Mulai Format(HH:mm)");
            cell.CellStyle = style;

            cell = rowHeader.CreateCell(5);
            cell.SetCellValue("Jam Selesai Format(HH:mm)");
            cell.CellStyle = style;

            cell = rowHeader.CreateCell(6);
            cell.SetCellValue("Prodi");
            cell.CellStyle = style;

            cell = rowHeader.CreateCell(7);
            cell.SetCellValue("Mata Kuliah");
            cell.CellStyle = style;

            cell = rowHeader.CreateCell(8);
            cell.SetCellValue("Semester");
            cell.CellStyle = style;

            cell = rowHeader.CreateCell(9);
            cell.SetCellValue("Dosen UTS/UAS");
            cell.CellStyle = style;

            var stream = new MemoryStream();
            workbook.Write(stream);
            var content = stream.ToArray();

            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Template_DataRuangKelasTetap.xlsx");
        }
        #endregion

        [HttpDelete]
        public async Task<IActionResult> DeleteAll()
        {
            var objFromDb = await _unitOfWork.FixedSchedulerRoom.GetAllAsync();
            if (objFromDb == null)
            {
                TempData["Error"] = "Gagal hapus Daftar Ruang Kelas Tetap";
                return Json(new { success = false, message = "Error while deleting" });
            }
            await _unitOfWork.FixedSchedulerRoom.RemoveRangeAsync(objFromDb);
            _unitOfWork.Save();

            TempData["Success"] = "Daftar Ruang Kelas Tetap berhasil hapus";
            return Json(new { success = true, message = "Delete Successful" });

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
        public async Task<IActionResult> Upsert(int? id)
        {
            var ListHari = _unitOfWork.Genmaster.GetAll().Where(z => z.GENFLAG == 7).Select(x => new SelectListItem { Value = x.GENVALUE.ToString(), Text = x.GENNAME });
            ViewBag.ListHari = new SelectList(ListHari, "Value", "Text");
            
            ViewBag.Status = "";
            FixedSchedulerRoomVM vm = new FixedSchedulerRoomVM();
            vm.FixedSchedulerRoom = await _unitOfWork.FixedSchedulerRoom.GetAsync(id.GetValueOrDefault());
            if (vm.FixedSchedulerRoom != null)
            {
                ViewBag.ClockStart = vm.FixedSchedulerRoom.ValStart_Clock.Value.ToString("HH:mm");
                ViewBag.ClockEnd = vm.FixedSchedulerRoom.ValEnd_Clock.Value.ToString("HH:mm");
                ViewBag.FlagSave = "1";
            }
            else
            {
                ViewBag.ClockStart = DateTime.Now.ToString("HH:mm");
                ViewBag.ClockEnd = DateTime.Now.ToString("HH:mm");
                ViewBag.FlagSave = "0";
            }
            return View(vm);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(FixedSchedulerRoomVM vm)
        {

            var ListHari = _unitOfWork.Genmaster.GetAll().Where(z => z.GENFLAG == 7).Select(x => new SelectListItem { Value = x.GENVALUE.ToString(), Text = x.GENNAME });
            ViewBag.ListHari = new SelectList(ListHari, "Value", "Text", vm.FixedSchedulerRoom.Flag.ToString());
            ViewBag.ClockStart = vm.ClockStart.ToString();
            ViewBag.ClockEnd = vm.ClockEnd.ToString();
            ModelState.Clear();

            string dtNow = DateTime.Now.ToString("yyyy-MM-dd ");
            if (vm.ClockStart.ToString() != null)
            {
                string valdays = dtNow + vm.ClockStart;
                vm.FixedSchedulerRoom.ValStart_Clock = DateTime.Parse(valdays);
                vm.FixedSchedulerRoom.Start_Clock = vm.FixedSchedulerRoom.ValStart_Clock.Value.ToString("HH:mm");
            }

            if (vm.ClockEnd.ToString() != null)
            {
                string valdays = dtNow + vm.ClockEnd;
                vm.FixedSchedulerRoom.ValEnd_Clock = DateTime.Parse(valdays);
                vm.FixedSchedulerRoom.End_Clock = vm.FixedSchedulerRoom.ValEnd_Clock.Value.ToString("HH:mm");
            }
            if (vm.FixedSchedulerRoom.ValEnd_Clock < vm.FixedSchedulerRoom.ValStart_Clock)
            {
                ModelState.AddModelError("ClockStart", "Jam mulai harus lebih besar dari jam selesai");           
            }

            if (vm.FixedSchedulerRoom.RoomId == 0)
            {
                ModelState.AddModelError("RoomId", "Ruangan harus diisi");
            }
            if (vm.FixedSchedulerRoom.Id == 0)
            {
                bool valClock = false;
                var BookingFixed = await _unitOfWork.FixedSchedulerRoom.GetAllAsync(z => z.RoomId == vm.FixedSchedulerRoom.RoomId && z.Flag == vm.FixedSchedulerRoom.Flag);
                foreach (var bookingFixed in BookingFixed)
                {
                    string Startvaldays = dtNow + bookingFixed.Start_Clock;
                    string Endtvaldays = dtNow + bookingFixed.End_Clock;
                    if (vm.FixedSchedulerRoom.ValStart_Clock >= DateTime.Parse(Startvaldays) && vm.FixedSchedulerRoom.ValEnd_Clock <= DateTime.Parse(Endtvaldays))
                    {
                        valClock = true;
                    }
                    if (vm.FixedSchedulerRoom.ValStart_Clock >= DateTime.Parse(Startvaldays) && vm.FixedSchedulerRoom.ValStart_Clock <= DateTime.Parse(Endtvaldays) && vm.FixedSchedulerRoom.ValEnd_Clock <= DateTime.Parse(Endtvaldays))
                    {
                        valClock = true;
                    }
                    if (vm.FixedSchedulerRoom.ValStart_Clock <= DateTime.Parse(Startvaldays) && vm.FixedSchedulerRoom.ValEnd_Clock > DateTime.Parse(Startvaldays))
                    {
                        valClock = true;
                    }
                    if (vm.FixedSchedulerRoom.ValStart_Clock < DateTime.Parse(Endtvaldays) && vm.FixedSchedulerRoom.ValEnd_Clock >= DateTime.Parse(Endtvaldays))
                    {
                        valClock = true;
                    }
                }
                if (valClock)
                {
                    ModelState.AddModelError("RoomId", "Ruangan sudah dijadwalkan, cek hari dan jam input");
                }
            }

            var errorList = ModelState.Values.SelectMany(x => x.Errors).ToList();        

            if (errorList.Count() == 0)
            {       
               
                vm.FixedSchedulerRoom.Days = _unitOfWork.Genmaster.GetAll().Where(z => z.GENFLAG == 7 && z.GENVALUE == vm.FixedSchedulerRoom.Flag).Select(x => x.GENCODE).FirstOrDefault();
                if (vm.FixedSchedulerRoom.Id == 0)
                {                   
                    var claimsIdentity = (ClaimsIdentity)User.Identity;
                    var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                    var user = _unitOfWork.ApplicationUser.GetFirstOrDefault(u => u.Id == claim.Value);
                    
                    vm.FixedSchedulerRoom.EntryBy = user.Name;
                    vm.FixedSchedulerRoom.EntryDate = DateTime.Now;
                    await _unitOfWork.FixedSchedulerRoom.AddAsync(vm.FixedSchedulerRoom);
                    ViewBag.Status = "Save Success";
                    ViewBag.FlagSave = "0";
                    _unitOfWork.Save();
                }
                else
                {
                    vm.FixedSchedulerRoom.Room = _unitOfWork.Room.Get(vm.FixedSchedulerRoom.RoomId);
                    _unitOfWork.FixedSchedulerRoom.Update(vm.FixedSchedulerRoom);
                    ViewBag.Status = "Update Success";
                    ViewBag.FlagSave = "1";
                }
                _unitOfWork.Save();
            }
            else
                ViewBag.Status = "Error";

            return View(vm);
        }

       
        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var objFromDb = await _unitOfWork.FixedSchedulerRoom.GetAsync(id);
            if (objFromDb == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }

            await _unitOfWork.FixedSchedulerRoom.RemoveAsync(objFromDb);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Delete Successful" });

        }
    }
}
