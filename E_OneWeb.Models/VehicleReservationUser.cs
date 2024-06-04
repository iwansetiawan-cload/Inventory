using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_OneWeb.Models
{
    public class VehicleReservationUser
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int BookingId { get; set; }
        [ForeignKey("BookingId")]
        public VehicleReservationAdmin VehicleReservationAdmin { get; set; }
        public DateTime? BookingStartDate { get; set; }
        public DateTime? BookingEndDate { get; set; }
        [Required(ErrorMessage = "Tujuan harus diisi")]
        public string Destination { get; set; }
        [Required(ErrorMessage ="Keperluan harus diisi")]
		public string Utilities { get; set; }
        [RegularExpression(@"\d+(\.\d{1,2})?", ErrorMessage = "No Telepon harus numeric")]
        public string? Phone { get; set; }
		public int? DriverId { get; set; }
		public string? DriverName { get; set; }
		[Required]
        [MaxLength(450)]
        public string UserId { get; set; }        
        public int? Flag { get; set; }
        public int? StatusId { get; set; }
        public string? Status { get; set; }
        public string? ApproveBy { get; set; }
        public DateTime? ApproveDate { get; set; }
        public string? RejectedBy { get; set; }
        public DateTime? RejectedDate { get; set; }
        public string? NotesReject { get; set; }
        [MaxLength(255)]
        public string? EntryBy { get; set; }
        public DateTime? EntryDate { get; set; }     

    }
}
