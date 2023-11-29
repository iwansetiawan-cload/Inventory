using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_OneWeb.Models.ViewModels
{
    public class ItemTransferVM
    {
        public ItemTransfer ItemTransfer { get; set; }
        public string name_of_room_and_location { get; set; }
        public IEnumerable<SelectListItem> ItemList { get; set; }
        public IEnumerable<SelectListItem> RoomList { get; set; }
    }
}
