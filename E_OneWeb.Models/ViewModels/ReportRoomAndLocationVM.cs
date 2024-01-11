using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_OneWeb.Models.ViewModels
{
    public class ReportRoomAndLocationVM
    {
        public string? SearchLocation { get; set; }
        public int? SearchLocationId{ get; set; }
        public string? SearchDesc { get; set; }
    }
    public class ExportReportRoomAndLocation
    {
        public int? Number { get; set; }
        public string? RoomName { get; set; }
        public string? Description { get; set; }
        public string? LocationName { get; set; }
    }
}
