using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_OneWeb.Models.ViewModels
{
    public class VehicleReservationAdminVM
    {
        public VehicleReservationAdmin VehicleReservationAdmin { get; set; }
        public IEnumerable<SelectListItem> ItemList { get; set; } 
    }
    public class GridVehicleReservationAdmin
    {
        public int? id { get; set; }
        public int? idadmin { get; set; }
        public string? code { get; set; }
        public string? name { get; set; }
        public string? locationname { get; set; }
        public DateTime? startdate { get; set; }
        public DateTime? enddate { get; set; }
        public string? bookingdate { get; set; }
        public string? bookingclock { get; set; }
        public string? status { get; set; }
        public int? statusid { get; set; }
        public int? bookingid { get; set; }
        public string? bookingby { get; set; }
        public int? flag { get; set; }
        public string? driver { get; set; }
        public int? flagEdit { get; set; }
    }
}
