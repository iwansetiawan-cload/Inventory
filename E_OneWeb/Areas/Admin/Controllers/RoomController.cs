using E_OneWeb.DataAccess.Repository.IRepository;
using E_OneWeb.Models;
using E_OneWeb.Models.ViewModels;
using E_OneWeb.Utility;
using iTextSharp.text.pdf;
using iTextSharp.text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Finance.Implementations;
using System.Data;

namespace E_OneWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
    public class RoomController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public RoomController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            LocationVM locationVM = new LocationVM()
            {
                Locations = _unitOfWork.Location.GetAll()
            };
            var count = locationVM.Locations.Count();
            return View(locationVM);
        }

        public IActionResult Upsert(int? id)
        {
            Location location = new Location();
            if (id == null)
            {
                //this is for create
                return View(location);
            }
            //this is for edit
            location = _unitOfWork.Location.Get(id.GetValueOrDefault());
            if (location == null)
            {
                return NotFound();
            }
            LocationId = location.Id;
            ViewBag.Status = "";
            return View(location);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(Location location)
        {
            if (ModelState.IsValid)
            {
                if (location.Id == 0)
                {
                    _unitOfWork.Location.Add(location);
                    ViewBag.Status = "Save Success";
                }
                else
                {
                    _unitOfWork.Location.Update(location);
                    ViewBag.Status = "Update Success";
                }
                _unitOfWork.Save();
                return RedirectToAction(nameof(Index));
            }
            return View(location);
        }
        public static int LocationId { get; set; }
        public IActionResult UpsertRoom(int? idlocation, int? id)
        {
         
            Room room = new Room();
            LocationId = Convert.ToInt32(idlocation);
            room.IDLocation = LocationId;

            //Location location = new Location();
            //location = _unitOfWork.Location.Get(room.IDLocation);
            //room.Location = location;

            if (id == null)
            {
                //this is for create
                return View(room);
            }
            //this is for edit
            room = _unitOfWork.Room.Get(id.GetValueOrDefault());
            if (room == null)
            {
                return NotFound();
            }
            else
            {
                LocationId = room.IDLocation;

			}
            ViewBag.Status = "";

            return View(room);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpsertRoom(Room room)
        {

            Location location = new Location();
            location = _unitOfWork.Location.Get(LocationId);
            room.Location = location;          

            if (room.Id == 0)
            {
                _unitOfWork.Room.Add(room);
                ViewBag.Status = "Save Success";
            }
            else
            {
                room.IDLocation = room.Location.Id;
				_unitOfWork.Room.Update(room);
                ViewBag.Status = "Update Success";
            }
            _unitOfWork.Save();
            return View(room);
            //return RedirectToAction(nameof(Upsert), new { id = room.IDLocation});


            //return View(room);
        }

        #region API CALLS

        [HttpGet]
        public IActionResult GetAll()
        {
            var allObj = _unitOfWork.Location.GetAll();
            return Json(new { data = allObj });
        }

        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var objFromDb = _unitOfWork.Location.Get(id);
            if (objFromDb == null)
            {
                TempData["Error"] = "Error deleting Category";
                return Json(new { success = false, message = "Error while deleting" });
            }
            _unitOfWork.Location.Remove(objFromDb);
            _unitOfWork.Save();

            TempData["Success"] = "Category successfully deleted";
            return Json(new { success = true, message = "Delete Successful" });

        }

		[HttpGet]
		public IActionResult GetAllRoom()
		{			
			var datalist = (from z in _unitOfWork.Room.GetAll()
							select new
							{
								id = z.Id,
								name = z.Name,
								description = z.Description,
                                idlocation = z.IDLocation
							}).ToList().Where(i=>i.idlocation == LocationId).OrderByDescending(o => o.id);
			return Json(new { data = datalist });
		}

        #endregion

        public async Task<IActionResult> ViewItemList(int? id)
        {
            Room room = new Room();
            ViewBag.RoomId = id;
            room = _unitOfWork.Room.Get(id.GetValueOrDefault());
            if (room == null)
            {
                return NotFound();
            }          
            return View(room);

        }

        [HttpGet]
        public async Task<IActionResult> GetItemList(int? id)
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
                                roomid = z.RoomId,
                                category = z.Category.Name,
                                startdate = z.StartDate.Value != null ? z.StartDate.Value.ToString("dd/MM/yyyy") : ""
                            }).Where(i=>i.roomid == id).ToList().OrderByDescending(o => o.id);

            return Json(new { data = datalist });
        }
        [HttpDelete]
        public IActionResult DeleteRoom(int id)
        {
            var objFromDb = _unitOfWork.Room.Get(id);
            if (objFromDb == null)
            {
                TempData["Error"] = "Error deleting Category";
                return Json(new { success = false, message = "Error while deleting" });
            }
            _unitOfWork.Room.Remove(objFromDb);
            _unitOfWork.Save();

            TempData["Success"] = "Category successfully deleted";
            return Json(new { success = true, message = "Delete Successful" });

        }

        public async Task<FileResult> Export(int? id)
        {


            var datalist = (from z in await _unitOfWork.Items.GetAllAsync(includeProperties: "Category,Room")
                            select new
                            {
                                number = z.Id,
                                code = z.Code,
                                name = z.Name,
                                description = z.Description,
                                category = z.Category.Name,
                                location = _unitOfWork.Location.Get(z.Room.IDLocation).Name,
                                room = z.Room.Name,
                                roomid = z.RoomId,
                                qty = z.Qty,
                                price = z.Price != null ? z.Price : 0,
                                totalamount = z.TotalAmount != null ? z.TotalAmount : 0,
                                ownership = z.OriginOfGoods,
                                categoryid = z.CategoryId,
                                status = z.Status,
                                startdate = z.StartDate,
                                startdatestring = z.StartDate != null ? z.StartDate.Value.ToString("dd/MM/yyyy") : ""
                            }).ToList();

            if (id != null)
            {
                datalist = datalist.Where(o => o.roomid == id).ToList();
            }

            int Number = 1;
            var datalist_ = (from z in datalist
                             select new ExportReportItem
                             {
                                 Number = Number++,
                                 Code = z.code,
                                 Name = z.name,
                                 StartDate = z.startdatestring,
                                 Description = z.description,
                                 Category = z.category,
                                 Location = z.location,
                                 Room = z.room,
                                 Price = z.price.HasValue ? z.price.Value.ToString("#,##0") : "",
                                 Qty = z.qty,
                                 TotalAmount = z.totalamount.HasValue ? z.totalamount.Value.ToString("#,##0") : ""
                             }).ToList();

            double? totalNilai = datalist_.Sum(o => Convert.ToDouble(o.TotalAmount));

            var file = CreateFile(datalist_, totalNilai);
            return File(file, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Export_Data_Aset_" + DateTime.Now.ToString("yyyyMMdd:hh.mm") + ".xlsx");


        }
        public async Task<FileResult> ExportPDF(int? id)
        {

            var datalist = (from z in await _unitOfWork.Items.GetAllAsync(includeProperties: "Category,Room")
                            select new
                            {
                                number = z.Id,
                                code = z.Code,
                                name = z.Name,
                                description = z.Description,
                                category = z.Category.Name,
                                location = _unitOfWork.Location.Get(z.Room.IDLocation).Name,
                                room = z.Room.Name,
                                roomid = z.RoomId,
                                qty = z.Qty,
                                price = z.Price != null ? z.Price : 0,
                                totalamount = z.TotalAmount != null ? z.TotalAmount : 0,
                                ownership = z.OriginOfGoods,
                                categoryid = z.CategoryId,
                                status = z.Status,
                                startdate = z.StartDate,
                                startdatestring = z.StartDate != null ? z.StartDate.Value.ToString("dd/MM/yyyy") : ""
                            }).ToList();
            if (id != null)
            {
                datalist = datalist.Where(o => o.roomid == id).ToList();
            }

            int Number = 1;
            var datalist2 = (from z in datalist
                             select new
                             {
                                 number = Number++,
                                 code = z.code,
                                 name = z.name,
                                 description = z.description,
                                 category = z.category,
                                 location = z.location,
                                 room = z.room,
                                 price = z.price != null ? z.price.Value.ToString("#,##0") : "0",
                                 qty = z.qty != null ? z.qty.Value.ToString("#,##0") : "0",
                                 totalamount = z.totalamount != null ? z.totalamount.Value.ToString("#,##0") : "0"
                             }).ToList();
            double? totalNilai = datalist2.Sum(o => Convert.ToDouble(o.totalamount));

            DataTable dttbl = CreateDataTable(datalist2);
            string physicalPath = "wwwroot\\images\\products\\DaftarAset.pdf";
            ExportDataTableToPdf(dttbl, physicalPath, "Daftar Aset", totalNilai);


            byte[] pdfBytes = System.IO.File.ReadAllBytes(physicalPath);
            MemoryStream ms = new MemoryStream(pdfBytes);
            return new FileStreamResult(ms, "application/pdf");



        }

        void ExportDataTableToPdf(DataTable dtblTable, String strPdfPath, string strHeader, double? totalNilai)
        {

            System.IO.FileStream fs = new FileStream(strPdfPath, FileMode.Create, FileAccess.Write, FileShare.None);
            Document document = new Document();
            document.SetPageSize(iTextSharp.text.PageSize.A4);
            PdfWriter writer = PdfWriter.GetInstance(document, fs);
            document.Open();
            PdfPCell cell = null;

            //Report Header
            BaseFont bfntHead = BaseFont.CreateFont(BaseFont.HELVETICA_BOLD, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
            iTextSharp.text.Font fntHead = new iTextSharp.text.Font(bfntHead, 14, 1, BaseColor.DARK_GRAY);
            Paragraph prgHeading = new Paragraph();
            prgHeading.Alignment = Element.ALIGN_CENTER;
            prgHeading.Add(new Chunk(strHeader.ToUpper(), fntHead));
            document.Add(prgHeading);                      

            //Add line break
            document.Add(new Chunk("\n", fntHead));

            PdfPTable table = new PdfPTable(10);
            table.HorizontalAlignment = 0;
            table.TotalWidth = 520f;
            table.LockedWidth = true;
            float[] widths = new float[] { 20f, 50f, 80f, 50f, 40f, 30f, 30f, 30f, 20f, 30f, };
            table.SetWidths(widths);
            //Table header
            BaseFont btnColumnHeader = BaseFont.CreateFont(BaseFont.HELVETICA_BOLD, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
            iTextSharp.text.Font fntColumnHeader = new iTextSharp.text.Font(btnColumnHeader, 8, 1, BaseColor.WHITE);           

            addCell(table, "No", 1);
            addCell(table, "Kode", 1);
            addCell(table, "Nama Harta Tetap", 1);
            addCell(table, "Keterangan", 1);
            addCell(table, "Katagori", 1);
            addCell(table, "Gedung", 1);
            addCell(table, "Ruangan", 1);
            addCell(table, "Nilai Perolehan", 1);
            addCell(table, "Jumlah", 1);
            addCell(table, "Total Nilai", 1);
            //table Data
            for (int i = 0; i < dtblTable.Rows.Count + 1; i++)
            {
                BaseFont bfTimes = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, false);
                iTextSharp.text.Font times = new iTextSharp.text.Font(bfTimes, 6, iTextSharp.text.Font.NORMAL, iTextSharp.text.BaseColor.BLACK);

                for (int j = 0; j < dtblTable.Columns.Count; j++)
                {     

                    if (dtblTable.Rows.Count > i)
                    {
                        cell = new PdfPCell(new Phrase(dtblTable.Rows[i][j].ToString(), times));
                        cell.Rowspan = 1;
                        cell.HorizontalAlignment = PdfPCell.ALIGN_CENTER;
                        cell.VerticalAlignment = PdfPCell.ALIGN_MIDDLE;
                        table.AddCell(cell);
                    }
                    else
                    {
                        if (j == 0)
                        {
                            cell = new PdfPCell(new Phrase("", times));
                            cell.Colspan = 7;
                            cell.Border = 0;
                            cell.HorizontalAlignment = PdfPCell.ALIGN_CENTER;
                            cell.VerticalAlignment = PdfPCell.ALIGN_MIDDLE;
                            table.AddCell(cell);

                        }
                        else if (j == 1)
                        {
                            cell = new PdfPCell(new Phrase("TOTAL :", times));
                            cell.Rowspan = 1;
                            cell.Colspan = 2;
                            cell.HorizontalAlignment = PdfPCell.ALIGN_CENTER;
                            cell.VerticalAlignment = PdfPCell.ALIGN_MIDDLE;
                            table.AddCell(cell);

                        }
                        else if (j == 2)
                        {
                            cell = new PdfPCell(new Phrase(totalNilai.Value.ToString("#,##0"), times));
                            cell.Rowspan = 1;
                            cell.HorizontalAlignment = PdfPCell.ALIGN_CENTER;
                            cell.VerticalAlignment = PdfPCell.ALIGN_MIDDLE;
                            table.AddCell(cell);
                        }
                        else
                        {
                            cell = new PdfPCell(new Phrase("", times));
                            cell.Border = 0;
                            cell.HorizontalAlignment = PdfPCell.ALIGN_CENTER;
                            cell.VerticalAlignment = PdfPCell.ALIGN_MIDDLE;
                            table.AddCell(cell);
                        }
                    }
                }


            }

            document.Add(table);
            document.Close();
            writer.Close();
            fs.Close();

        }
        public static byte[] CreateFile<T>(List<T> source, double? totalNilai)
        {
           

            var workbook = new XSSFWorkbook();
            var sheet = workbook.CreateSheet("Sheet1");
            //sheet.ShiftRows(18, 20, 1);
            var rowHeaderName = sheet.CreateRow(0);
            var rowHeader = sheet.CreateRow(2);
            //sheet.AutoSizeColumn(2);

            var properties = typeof(T).GetProperties();

            //header
            var font = workbook.CreateFont();
            font.IsBold = true;
            var style = workbook.CreateCellStyle();
            style.SetFont(font);      

            var cellHeader = rowHeaderName.CreateCell(0);
            cellHeader.SetCellValue("DAFTAR ASET");
            cellHeader.CellStyle = style;
            //sheet.AddMergedRegion(CellRangeAddress.ValueOf("A1:K1"));       

            var cell = rowHeader.CreateCell(0);
            cell.SetCellValue("No");
            cell.CellStyle = style;

            cell = rowHeader.CreateCell(1);
            cell.SetCellValue("Kode");
            cell.CellStyle = style;

            cell = rowHeader.CreateCell(2);
            cell.SetCellValue("Nama Harta Tetap");
            cell.CellStyle = style;

            cell = rowHeader.CreateCell(3);
            cell.SetCellValue("Tanggal Perolehan");
            cell.CellStyle = style;

            cell = rowHeader.CreateCell(4);
            cell.SetCellValue("Keterangan");
            cell.CellStyle = style;

            cell = rowHeader.CreateCell(5);
            cell.SetCellValue("Katagori");
            cell.CellStyle = style;

            cell = rowHeader.CreateCell(6);
            cell.SetCellValue("Gedung");
            cell.CellStyle = style;

            cell = rowHeader.CreateCell(7);
            cell.SetCellValue("Ruangan");
            cell.CellStyle = style;

            cell = rowHeader.CreateCell(8);
            cell.SetCellValue("Nilai Perolehan");
            cell.CellStyle = style;

            cell = rowHeader.CreateCell(9);
            cell.SetCellValue("Jumlah");
            cell.CellStyle = style;

            cell = rowHeader.CreateCell(10);
            cell.SetCellValue("Total Nilai");
            cell.CellStyle = style;

            //end header

            //content
            var rowNum = 3;
            foreach (var item in source)
            {
                var rowContent = sheet.CreateRow(rowNum);

                var colContentIndex = 0;
                foreach (var property in properties)
                {
                    var cellContent = rowContent.CreateCell(colContentIndex);
                    var value = property.GetValue(item, null);

                    if (value == null)
                    {
                        cellContent.SetCellValue("");
                    }
                    else if (property.PropertyType == typeof(string))
                    {
                        cellContent.SetCellValue(value.ToString());
                    }
                    else if (property.PropertyType == typeof(int) || property.PropertyType == typeof(int?))
                    {
                        cellContent.SetCellValue(Convert.ToInt32(value));
                    }
                    else if (property.PropertyType == typeof(decimal) || property.PropertyType == typeof(decimal?))
                    {
                        cellContent.SetCellValue(Convert.ToDouble(value));
                    }
                    else if (property.PropertyType == typeof(DateTime) || property.PropertyType == typeof(DateTime?))
                    {
                        var dateValue = (DateTime)value;
                        cellContent.SetCellValue(dateValue.ToString("yyyy-MM-dd"));
                    }
                    else cellContent.SetCellValue(value.ToString());

                    colContentIndex++;
                }

                rowNum++;
            }

            //end content

            #region Footer
            var rowFooter = sheet.CreateRow(rowNum);
            cell = rowFooter.CreateCell(9);
            cell.SetCellValue("Total : ");
            cell.CellStyle = style;

            cell = rowFooter.CreateCell(10);
            cell.SetCellValue("" + totalNilai.Value.ToString("#,##0") + "");
            cell.CellStyle = style;
        
            #endregion


            var stream = new MemoryStream();
            workbook.Write(stream);
            var content = stream.ToArray();

            return content;
        }
        public static DataTable CreateDataTable<T>(IEnumerable<T> entities)
        {
            var dt = new DataTable();

            //creating columns
            foreach (var prop in typeof(T).GetProperties())
            {
                dt.Columns.Add(prop.Name, prop.PropertyType);
            }

            //creating rows
            foreach (var entity in entities)
            {
                var values = GetObjectValues(entity);
                dt.Rows.Add(values);
            }


            return dt;
        }
        public static object[] GetObjectValues<T>(T entity)
        {
            var values = new List<object>();
            foreach (var prop in typeof(T).GetProperties())
            {
                values.Add(prop.GetValue(entity));
            }

            return values.ToArray();
        }
        private static void addCell(PdfPTable table, string text, int rowspan)
        {
            BaseFont bfTimes = BaseFont.CreateFont(BaseFont.TIMES_BOLD, BaseFont.CP1252, false);
            iTextSharp.text.Font times = new iTextSharp.text.Font(bfTimes, 7, iTextSharp.text.Font.NORMAL, iTextSharp.text.BaseColor.BLACK);

            PdfPCell cell = new PdfPCell(new Phrase(text, times));
            cell.BackgroundColor = BaseColor.WHITE;
            cell.Rowspan = rowspan;
            cell.HorizontalAlignment = PdfPCell.ALIGN_CENTER;
            cell.VerticalAlignment = PdfPCell.ALIGN_MIDDLE;
            table.AddCell(cell);
        }
    }
}
