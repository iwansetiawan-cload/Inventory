using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_OneWeb.Models.ViewModels
{
    public class ReportRequestItemVM
    {
        public string? SearchRefNumber { get; set; }
        public int? SearchMonth { get; set; }
        public int? SearchYear { get; set; }
        public int? SearchStatus { get; set; }
        public IEnumerable<SelectListItem> MonthList { get; set; }
        public IEnumerable<SelectListItem> SearchStatusList { get; set; }
    }
    public class ExportReportRequestItem
    {
        public int? Number { get; set; }
        public string? RefNumber { get; set; }
        public string? RequestDate { get; set; }
        public string? Requester { get; set; }   
        public string? Name { get; set;}
        public string? Reason { get; set; }
        public string? Specification { get; set; }
        public string? Price { get; set; }
        public string? Qty { get; set; }
        public string? Total { get; set;}
        public string? Status { get; set; }
        public string? TotalAmount { get; set; }

    }
}
