using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_OneWeb.Models.ViewModels
{
    public class GENMASTERVM
    {
        public GENMASTER GENMASTER { get; set; }
        public IEnumerable<SelectListItem> GENMASTERLIST { get; set; }
    }
}
