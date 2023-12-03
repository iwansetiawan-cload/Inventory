using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_OneWeb.Models
{
    public class ItemService
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int ItemId { get; set; }
        [ForeignKey("ItemId")]
        public Items Items { get; set; }
        [Display(Name = "Service Date")]
        public DateTime? ServiceDate { get; set; }
        [Display(Name = "Service End Date")]
        public DateTime? ServiceEndDate { get; set; }
        [Display(Name = "Repair Description")]
        public string? RepairDescription { get; set; }
        public string? Technician { get; set; }
        [Display(Name = "Phone Number")]
        [MaxLength(20)]
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
		[Display(Name = "Request By")]
		[MaxLength(255)]
        public string? RequestBy { get; set; }
		[Display(Name = "Cost Of Repair")]
		public double? CostOfRepair { get; set; }
        public int? Status { get; set; }
        [MaxLength(255)]
        public string? EntryBy { get; set; }
        public DateTime? EntryDate { get; set; }	
		public int? RoomId { get; set; }
		public int? LocationId { get; set; }
	}
}
