using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace E_OneWeb.Models
{
    public class Items
    {
        [Key]
        public int Id { get; set; }
        [MaxLength(50)]
        public string? Code { get; set; }

        [Display(Name = "Item Name")]
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }
        public string? Description { get; set; }
        public DateTime? StartDate { get; set; }
        public double? Price { get; set; }
        public int? Qty { get; set; }
        public double? TotalAmount { get; set; }
        public decimal? Percent { get; set; }
        public int? Period { get; set; }
        public double? DepreciationExpense { get; set; }    
        public int? Status { get; set; }
        [MaxLength(255)]
        public string? OriginOfGoods { get; set; }
        [Required]
        public int RoomId { get; set; }
        [ForeignKey("RoomId")]
        public Room Room { get; set; }
        [Required]
        public int CategoryId { get; set; }
        [ForeignKey("CategoryId")]
        public Category Category { get; set; }
        [MaxLength(255)]
        public string? EntryBy { get; set; }
        public DateTime? EntryDate { get; set; }
        public int? Condition { get; set; }


    }
}
