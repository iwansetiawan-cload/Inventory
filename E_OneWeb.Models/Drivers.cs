using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace E_OneWeb.Models
{
    public class Drivers
	{
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string? Address { get; set; }
        [RegularExpression(@"\d+(\.\d{1,2})?", ErrorMessage = "No Telepon harus angka")]
        public string? PhoneNumber { get; set; }
    }
}
