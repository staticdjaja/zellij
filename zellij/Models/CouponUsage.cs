using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace zellij.Models
{
    public class CouponUsage
    {
        public int Id { get; set; }
        
        [Required]
        public int CouponId { get; set; }
        
        [Required]
        public string UserId { get; set; } = string.Empty;
        
        public int? OrderId { get; set; }
        
        public DateTime UsedDate { get; set; } = DateTime.Now;
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountAmount { get; set; }
        
        // Navigation properties
        [ForeignKey("CouponId")]
        public virtual Coupon Coupon { get; set; } = default!;
        
        [ForeignKey("UserId")]
        public virtual IdentityUser User { get; set; } = default!;
        
        [ForeignKey("OrderId")]
        public virtual Order? Order { get; set; }
    }
}