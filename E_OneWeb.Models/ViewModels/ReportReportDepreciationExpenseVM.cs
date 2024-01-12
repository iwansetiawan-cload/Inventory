using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_OneWeb.Models.ViewModels
{
    public class ReportReportDepreciationExpenseVM
    {
        public int? SearchMonth { get; set; }
        public int? SearchYear { get; set; }
        public int? SearchCategory { get; set; }
        public DateTime? SearchStartDate { get; set; }
        public DateTime? SearchEndDate { get; set; }
        public IEnumerable<SelectListItem> CategoryList { get; set; }
        public IEnumerable<SelectListItem> MonthList { get; set; }
    }
    public class ExportReportDepreciationExpense
    {
        public int? Number { get; set; }
        public string? Name { get; set; }
        public string? StartDate { get; set; }
        public string? Category { get; set; }
        public decimal? Percent { get; set; }
        public int? Qty { get; set; }
        public string? TotalAmount { get; set; }
        public string? ExpenseAmount { get; set; }
        public string? ExpenseTotalAmount { get; set; }

    }
}
