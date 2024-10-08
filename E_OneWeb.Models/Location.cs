﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace E_OneWeb.Models
{
    public class Location
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [MaxLength(225)]
        public string Name { get; set; }
        public string? Description { get; set; }
    }
}
