using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_OneWeb.Models
{
    public class RequestItemDetail
    {
        [Key]
        public int Id { get; set; }
        public int IdHeader { get; set; }
        [Display(Name = "Item Name")]
        [Required]
        [MaxLength(225)]
        public string Name { get; set; }
        public string? Category { get; set; }
        public string? Specification { get; set; }
        public string? Reason { get; set; }
        public double? Price { get; set; }
        public int? Qty { get; set; }
        public double? Total { get; set; }       
        public int? StatusId { get; set; }
        public string? Status { get; set; }
        public string? Notes { get; set; }
        public int? RoomId { get; set; }
        public string? RoomName { get; set; }
    }
}
