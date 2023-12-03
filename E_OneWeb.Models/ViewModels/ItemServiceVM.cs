using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_OneWeb.Models.ViewModels
{
    public class ItemServiceVM
    {
        public ItemService ItemService { get; set; }
        public string? CostOfRepairString { get; set; }
        public string name_of_room_and_location { get; set; }
    }
}
