using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_OneWeb.Models.ViewModels
{
    public class RoomReservationAdminVM
    {
        public RoomReservationAdmin RoomReservationAdmin { get; set; }
        public IEnumerable<SelectListItem> RoomList { get; set; }
        public TimeSpan ClockStart { get; set; }
        public TimeSpan ClockEnd { get; set; }
        public string RoomAndLocation { get; set; }
        public string? File { get; set; }
        public string? Valid { get; set; }
        public string? Invalid { get; set; }
    }
}
