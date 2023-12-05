using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_OneWeb.Models.ViewModels
{
    public class RequestItemHeaderVM
    {
        public RequestItemHeader RequestItemHeader { get; set; }
        public RequestItemDetail RequestItemDetail { get; set; }
        public List<RequestItemDetail> AddItemsList { get; set; }
        public string PriceString { get; set; }
        public string TotalAmountString { get; set; }
        public string GrandTotalString { get; set; }
        public IEnumerable<SelectListItem> ListCategory { get; set; }
    }
    public class AddItems
    {
        public string ItemName { get; set; }
        public string Category { get; set; }
    }
}
