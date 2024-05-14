using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_OneWeb.Models
{
    public class FixedSchedulerRoom
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int RoomId { get; set; }
        [ForeignKey("RoomId")]
        public Room Room { get; set; }
        public string RoomName { get; set; }
        public string? LocationName { get; set; }
        public string? Days { get; set; }
        public string? Start_Clock { get; set; }
        public string? End_Clock { get; set; }
        public DateTime? ValStart_Clock { get; set; }
        public DateTime? ValEnd_Clock { get; set; }
        public string? Prodi { get; set; }
        public string? Study { get; set; }
        public string? Semester { get; set; }
        public string? Dosen { get; set; }
        public int? Flag { get; set; }
        [MaxLength(255)]
        public string? EntryBy { get; set; }
        public DateTime? EntryDate { get; set; }
    }
}
