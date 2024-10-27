using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_OneWeb.Models.ViewModels
{
    public class FixedSchedulerRoomVM
    {
        public FixedSchedulerRoom FixedSchedulerRoom { get; set; }
        public string? File { get; set; }
        public string? Valid { get; set; }
        public string? Invalid { get; set; }
        public TimeSpan ClockStart { get; set; }
        public TimeSpan ClockEnd { get; set; }
    }
}
