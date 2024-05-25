using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_OneWeb.Models
{
    public class VehicleReservationAdmin
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int ItemId { get; set; }
        [ForeignKey("ItemId")]
        public Items Items { get; set; }
        public string? ItemName { get; set; }
        public string? RoomName { get; set; }
        public string? LocationName { get; set; }
        public int? StatusId { get; set; }
        public string? Status { get; set; }
        public int? Flag { get; set; }
        public int? BookingId { get; set; }
        [MaxLength(255)]
        public string? BookingBy { get; set; }
        public DateTime? BookingStartDate { get; set; }
        public DateTime? BookingEndDate { get; set; }
    }
}
