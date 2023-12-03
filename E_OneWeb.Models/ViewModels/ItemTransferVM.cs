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
        public string LocationPrevious { get; set; }
        public string LocationCurrent { get; set; }
        public IEnumerable<SelectListItem> ItemList { get; set; }
        public IEnumerable<SelectListItem> RoomList { get; set; }
    }
}
