using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_OneWeb.Models.ViewModels
{
    public class RoomReservationUserVM
    {
        public RoomReservationUser RoomReservationUser { get; set; }
        public string StatusMessage { get; set; }
        public TimeSpan ClockStart { get; set; }
        public TimeSpan ClockEnd { get; set; }
        public string? StartDate { get; set; }
        public string? EndDate { get; set; }
        public Personal? Personal { get; set; }
    }
}
