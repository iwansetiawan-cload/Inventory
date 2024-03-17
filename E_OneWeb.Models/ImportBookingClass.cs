using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_OneWeb.Models
{
    public class ImportBookingClass
    {
        [Key]
        public Int64 Id { get; set; }
        public int Number { get; set; }
        public string? LocationName { get; set; }
        public string? RoomName { get; set; }
        public int? RoomId { get; set; }
        public string? StartDate { get; set; }
        public string? EndDate { get; set; }
        public DateTime? BookingStartDate { get; set; }
        public DateTime? BookingEndDate { get; set; }
        public string? ImportStatus { get; set; }
        public string? ImportRemark { get; set; }
        public string? UserKey { get; set; }
        public string? EntryBy { get; set; }
        public DateTime? EntryDate { get; set; }
    }
}
