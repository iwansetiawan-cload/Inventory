using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_OneWeb.Models.ViewModels
{
    public class ReportItemVM
    {
        public string? SearchCode { get; set; }
        public string? SearchOwnership { get; set; }
        public int? SearchStatus { get; set; }
        public int? SearchCategory { get; set; }
        public DateTime? SearchStartDate { get; set; }
        public DateTime? SearchEndDate { get; set; }
        public IEnumerable<SelectListItem> CategoryList { get; set; }
        public IEnumerable<SelectListItem> RoomList { get; set; }
        public IEnumerable<SelectListItem> LocationList { get; set; }

    }
    public class ExportReportItem
    {
        public int? Number { get; set;}
        public string? Code { get; set;}
        public string? Name { get; set; }
        public string? StartDate { get; set; }
        public string? Description { get; set; }
        public string? Category { get; set; }
        public string? Location { get; set; }
        public string? Room { get; set; }
        public string? Price { get; set; }
        public int? Qty { get; set; }
        public string? TotalAmount { get; set; }

    }
}
