using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_OneWeb.Models.ViewModels
{
    public class ReportRoomReservationVM
    {
        public string? SearchRoomName { get; set; }
        public int? SearchRoomId { get; set; }
        public DateTime? SearchStartDate { get; set; }
        public DateTime? SearchEndDate { get; set; }
        public IEnumerable<SelectListItem> CategoryList { get; set; }
        public IEnumerable<SelectListItem> MonthList { get; set; }
    }
    public class ExportReportRoomReservation
    {
        public int? Number { get; set; }
        public string? RoomName { get; set; }
        public string? StartDate { get; set; }
        public string? EndDate { get; set; }
        public string? Description { get; set; }
        public string? EntryBy { get; set; }
        public string? Study { get; set; }
        public string? Teacher { get; set; }
    }
}
