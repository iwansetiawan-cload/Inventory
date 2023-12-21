using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_OneWeb.Models
{
    public class RoomReservationUser
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int RoomAdminId { get; set; }
        [ForeignKey("RoomAdminId")]
        public RoomReservationAdmin RoomReservationAdmin { get; set; }       
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Description { get; set; }
        public string? ApproveBy { get; set; }
        public string? Study { get; set; }
        public string? Dosen { get; set; }
        public int? StatusId { get; set; }
        public string? Status { get; set; }
        [Required]
        [MaxLength(450)]
        public string UserId { get; set; }
        [MaxLength(255)]
        public string? EntryBy { get; set; }
        public DateTime? EntryDate { get; set; }
        public string? RefFile { get; set; }
    }
}
