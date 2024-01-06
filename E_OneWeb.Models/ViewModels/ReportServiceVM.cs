using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_OneWeb.Models.ViewModels
{
    public class ReportServiceVM
    {
        public string? SearchItem { get; set; }
        public int? SearchItemId { get; set; }
        public string? SearchLocationItem { get; set; }
        public int? SearchMonth { get; set; }
        public int? SearchYear { get; set; }
        public int? SearchStatus { get; set; }
        public IEnumerable<SelectListItem> MonthList { get; set; }
        public IEnumerable<SelectListItem> SearchStatusList { get; set; }
    }
    public class ExportReportServiceItem
    {
        public int? Number { get; set; }
        public string? Code { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? ServiceDate { get; set; }
        public string? ServiceEndDate { get; set; }
        public string? Technician { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? RequestBy { get; set; }
        public string? CostOfRepair { get; set; }

    }
}
