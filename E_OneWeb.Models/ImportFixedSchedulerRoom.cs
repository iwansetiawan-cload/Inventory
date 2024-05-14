using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_OneWeb.Models
{
    public class ImportFixedSchedulerRoom
    {
        [Key]
        public Int64 Id { get; set; }
        public int Number { get; set; }
        public string? LocationName { get; set; }
        public string? RoomName { get; set; }
        public int? RoomId { get; set; }
        public string? Days { get; set; }
        public string? ValDays { get; set; }
        public string? Start_Clock { get; set; }
        public string? End_Clock { get; set; }
        public DateTime? ValStart_Clock { get; set; }
        public DateTime? ValEnd_Clock { get; set; }
        public string? Prodi { get; set; }
        public string? Study { get; set; }
        public string? Semester { get; set; }
        public string? Dosen { get; set; }
        public string? ImportStatus { get; set; }
        public string? ImportRemark { get; set; }
        public int? Flag { get; set; }
        public string? UserKey { get; set; }
        [MaxLength(255)]
        public string? EntryBy { get; set; }
        public DateTime? EntryDate { get; set; }
    }
}
