using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_OneWeb.Models.ViewModels
{
    public class ItemsVM
    {
        public Items Items { get; set; }
        public string PriceString { get; set; }
        public string TotalAmountString { get; set; }
        public string DepreciationExpenseString { get; set; }
        public IEnumerable<SelectListItem> CategoryList { get; set; }
        public IEnumerable<SelectListItem> RoomList { get; set; }
        public IEnumerable<SelectListItem> LocationList { get; set; }
    }
}
