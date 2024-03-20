using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_OneWeb.Models.ViewModels
{
    public class ItemsVM
    {
        public Items Items { get; set; }
        public string? Percent_String { get; set; }
        public string PriceString { get; set; }
        public string TotalAmountString { get; set; }
        public string DepreciationExpenseString { get; set; }
        public IEnumerable<SelectListItem> CategoryList { get; set; }
        public IEnumerable<SelectListItem> RoomList { get; set; }
        public IEnumerable<SelectListItem> LocationList { get; set; }
        public string File { get; set; }
        public string Valid { get; set; }
        public string Invalid { get; set; }
        public string? Code_Before { get; set; }
        public static List<ImportData> staticGridView { get; set; }
    }
    public class ImportData
    { 
        public string No { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? StartDate_String { get; set; }
        public DateTime? StartDate { get; set; }
        public string Category { get; set; }
        public int CategoryId { get; set; }
        public double? Price { get; set; }
        public string? Price_String { get; set; }
        public int? Qty { get; set; }
        public string? Qty_String { get; set; }
        public double? TotalAmount { get; set; }
        public decimal? Percent { get; set; }
        public int? Period { get; set; }
        public double? DepreciationExpense { get; set; }
        public string? OriginOfGoods { get; set; }
        public int? OwnerShipId { get; set; }
        public string? Location { get; set; }
        public string? Room { get; set; }
        public int RoomId { get; set; }
        public string? EntryBy { get; set; }
        public DateTime? EntryDate { get; set; }
        public string? Condition { get; set; }
        public int? ConditionId { get; set; }
        public string? Status { get; set; }
        public int? StatusId { get; set; }
        public string ValidInvalid { get; set; }
        public string Remarks { get; set; }
    }
}
