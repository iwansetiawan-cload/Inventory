using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace E_OneWeb.Models
{
    public class Supplier
    {
        [Key]
        public int Id { get; set; }
        public string? Code { get; set; }
        [Display(Name = "Supplier Name")]
        [Required]
        [MaxLength(225)]
        public string Name { get; set; }
        public string? Address { get; set; }
        [Display(Name = "Phone Number")]
        [MaxLength(25)]
        public string? Phone { get; set; }
    }
}
