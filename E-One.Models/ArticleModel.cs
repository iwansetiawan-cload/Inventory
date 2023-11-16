using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_OneWeb.Models
{
    public class ArticleModel
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]  
        public string Content { get; set; }
        public int Point { get; set; }
        public double PointDescription { get; set; }
        public DateTime? PointExepiredDate { get; set; }
        public string EntryBy { get; set; }
        public DateTime? EntryDate { get; set; }
    }
}
