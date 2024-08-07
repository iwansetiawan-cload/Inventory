﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_OneWeb.Models
{
    public class RequestItemHeader
    {
        [Key]
        public int Id { get; set; }
        [Display(Name = "Ref Number")]
        [Required]
        [MaxLength(100)]
        public string ReqNumber { get; set; }
        public string? Notes { get; set; }        
        public string? Requester { get; set; }
        [Display(Name = "Request Date")]
        public DateTime? RequestDate { get; set; }
        public string? RefFile { get; set; }
        public double? TotalAmount { get; set; }
        [MaxLength(255)]
        public string? EntryBy { get; set; }
        public DateTime? EntryDate { get; set; }
		public string? Status { get; set; }
        public int? StatusId { get; set; }
        public string? ApproveBy { get; set; }
        public DateTime? ApproveDate { get; set; }
        public string? RejectedBy { get; set; }
        public DateTime? RejectedDate { get; set; }
    }
}
