using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_OneWeb.Models
{
    public class Personal
    {
        [Key]
        public int Id { get; set; }
        [MaxLength(450)]
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string? FullName { get; set; }
        public string Gender { get; set; }
        public string PhoneNumber { get; set; }
        public string? NIM { get; set; }
        public string? Prodi { get; set; }
        public string? Fakultas { get; set; }
        public string? Photo { get; set;}
        public DateTime? EntryDate { get; set; }

    }
}
