using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_OneWeb.Models.ViewModels
{
    public class PurchaseOrderHeaderVM
    {
        public PurchaseOrderHeader PurchaseOrderHeader { get; set; }
        public IEnumerable<SelectListItem> SupplierList { get; set; }
    }
}
