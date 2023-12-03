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
    public class ItemTransfer
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int ItemId { get; set; }
        [ForeignKey("ItemId")]
        public Items Items { get; set; }             
        public string? Description { get; set; }
        [Display(Name = "Transfer Date")]
        public DateTime? TransferDate { get; set; }
        public int PreviousLocationId { get; set; }
        public string PreviousRoom { get; set; }
        public string PreviousLocation { get; set; }
        public int CurrentLocationId { get; set; }
        public string CurrentRoom { get; set; }
        public string CurrentLocation { get; set; }
        [MaxLength(255)]
        public string? EntryBy { get; set; }
        public DateTime? EntryDate { get; set; }
        
        
    }
}
