using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_OneWeb.Models.ViewModels
{
    public class CategoryVM
    {
        //public Category Category { get; set; }    
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public decimal? Percent { get; set; }
        public string? Percent_String { get; set; }
        public int? Period { get; set; }
        //public IEnumerable<Category> Categories { get; set; }
    }
}
