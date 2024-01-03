using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_OneWeb.Models.ViewModels
{
    public class ReportTransferItemVM
    {
        public string? SearchItem { get; set; }
        public int? SearchItemId { get; set; }
        public string? SearchLocationItem { get; set; }
        public int? SearchMonth { get; set; }
        public int? SearchYear { get; set; }
        public string? SearchRoomName { get; set; }
        public int? SearchRoomId { get; set; }
        public IEnumerable<SelectListItem> MonthList { get; set; }
    }
    public class ExportReportTransferItem
    {
        public int? Number { get; set; }
        public string? Code { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? TransferDate { get; set; }
        public string? PreviousLocation { get; set; }
        public string? CurrentLocation { get; set; }

    }
}
