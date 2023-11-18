using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_OneWeb.Models
{
    public class GENMASTER
    {
        [Key]
        public int IDGEN { get; set;}
        [MaxLength(255)]
        public string? GENCODE { get; set; }
        [MaxLength(255)]
        public string? GENNAME { get; set; }
        public Nullable<double> GENVALUE { get; set; }
        public int GENFLAG { get; set; }
        public string? ENTRYBY { get; set; }
        public Nullable<System.DateTime> ENTRYDATE { get; set; }
    }
}
