using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_OneWeb.Models
{
    public class Article
    {
        [Key]
        public int Id { get; set; }       
        public string Title { get; set; }
        public string Content { get; set; }
        public int? Point { get; set; }
        public Nullable<double> PointDesc { get; set; }
        public Nullable<System.DateTime> PointExepiredDate { get; set; }
        public string? EntryBy { get; set; }
        public string? PhoneNumber { get; set; }
        public Nullable<System.DateTime> EntryDate { get; set; }
        public Int16? Flag { get; set; }

    }
}
