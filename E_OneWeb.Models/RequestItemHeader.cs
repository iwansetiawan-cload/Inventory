using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_OneWeb.Models
{
    public class RequestItemHeader
    {
        [Key]
        public int Id { get; set; }
        [Display(Name = "Item Name")]
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }
        public string? Description { get; set; }
        [Display(Name = "Ref Number")]
        public string? RefNumber { get; set; }
        public string? Requester { get; set; }
        [Display(Name = "Request Date")]
        public DateTime? RequestDate { get; set; }
        public string? RefFile { get; set; }
        public double? TotalAmount { get; set; }
        [MaxLength(255)]
        public string? EntryBy { get; set; }
        public DateTime? EntryDate { get; set; }
    }
}
