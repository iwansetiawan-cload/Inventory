using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_OneWeb.Models
{
    public class PurchaseOrderDetail
    {
        [Key]
        public Int64 Id { get; set; }
        [MaxLength(20)]
        public string? PurchaseOrderNo { get; set; }
        [Required]
        public int ItemsId { get; set; }
        [ForeignKey("ItemsId")]
        public Items Items { get; set; }
        [Required]
        public Int64 HeaderId { get; set; }
        [ForeignKey("HeaderId")]
        public PurchaseOrderHeader PurchaseOrderHeader { get; set; }

        [Required]
        [MaxLength(100)]
        public string ItemName { get; set; }
        public double Quantity { get; set; }
        public double? ItemCost { get; set; }
        public double? ItemDiscount { get; set; }
        public double? DiscountAmount { get; set; }
        public double? NettPrice { get; set; }
        public double Total { get; set; }   
        public double? TaxAmount { get; set; }
        public double? DPP { get; set; }
        public double? BuyPrice { get; set; }
    }
}
