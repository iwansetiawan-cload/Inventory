using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_OneWeb.Models
{
    public class ImportItems
    {
        [Key]
        public Int64 Id { get; set; }
        public int Number { get; set; }
        public string? Code { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? StartDate_String { get; set; }
        public DateTime? StartDate { get; set; }
        public string? Category_String { get; set; }
        public int? CategoryId { get; set; }
        public string? Price_String { get; set; }
        public double? Price { get; set; }
        public string? Qty_String { get; set; }
        public int? Qty { get; set; }
        public double? TotalAmount { get; set; }
        public double? DepreciationExpense { get; set; }
        public string? Ownership { get; set; }
        public int? OwnershipId { get; set; }
        public string? LocationName { get; set; }
        public string? RoomName { get; set; }
        public int? RoomId { get; set; }
        public string? Status_String { get; set; }
        public int? Status { get; set; }
        public string? Condition_String { get; set; }
        public int? Condition { get; set; }
        public string? ImportStatus { get; set; }
        public string? ImportRemark { get; set; }
        public string? UserKey { get; set;}
        public string? EntryBy { get; set; }
        public DateTime? EntryDate { get; set; }

    }
}
