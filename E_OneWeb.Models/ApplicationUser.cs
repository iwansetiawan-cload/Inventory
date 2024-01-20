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
        public string FullName { get; set; }
        public string? Gender { get; set; }
        public string? NIM { get; set; }
        public string? Fakultas { get; set; }
        public string? Prodi { get; set; }
        [NotMapped]
        public string Role { get; set; }

    }
}
