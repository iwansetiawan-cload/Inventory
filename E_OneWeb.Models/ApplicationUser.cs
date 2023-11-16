using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_OneWeb.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        public string Name { get; set; }
        public string? Gender { get; set; }
        public string? CardNumber { get; set; }
        public string? City { get; set; }
        public string? PostalCode { get; set; }
        [NotMapped]
        public string Role { get; set; }

    }
}
