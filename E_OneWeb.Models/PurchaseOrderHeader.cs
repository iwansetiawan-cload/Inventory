using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_OneWeb.Models
{
    public class PurchaseOrderHeader
    {
        [Key]
        public Int64 Id { get; set; }
        [Required]
        public int SupplierId { get; set; }
        [ForeignKey("SupplierId")]
        public Supplier Supplier { get; set; }
        [Display(Name = "Purchase Order No")]
        [MaxLength(20)]
        public string? PurchaseOrderNo { get; set; }
        public DateTime TransactionDate { get; set; }
        [MaxLength(255)]
        public string Requester { get; set; }
        public string? Notes { get; set; }
        public Boolean IsActive { get; set; }
        [MaxLength(50)]
        public string? TaxId { get; set; }
        public double? TaxAmount { get; set; }
        public double? ShipmentFee { get; set; }
        public double? HandlingFee { get; set; }
        public double? DownPayment { get; set; }
        public double? DownPaymentPaid { get; set; } 
        public int? PercentageDiscount { get; set; }
        public double? FinalDiscount { get; set; }
        public double? SubTotal { get; set; }
        public double? Total { get; set; }
        public double? DPP { get; set; }
        public double? GrandTotal { get; set; }

        public string? ApprovedBy { get; set; }        
        public DateTime? DeletedDate { get; set; }
        [MaxLength(50)]
        public string? DeletedBy { get; set; }
        [MaxLength(50)]
        public string? StatusTerima { get; set; }
        [MaxLength(255)]
        public string? EntryBy { get; set; }
        public DateTime? EntryDate { get; set; }
    }
}
