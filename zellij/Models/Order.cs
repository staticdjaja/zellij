using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace zellij.Models
{
    public class Order
    {
        public int Id { get; set; }
        
        [Required]
        public string UserId { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string OrderNumber { get; set; } = string.Empty;
        
        [Required]
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal SubTotal { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountAmount { get; set; } = 0;
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal ShippingCost { get; set; } = 0;
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal Tax { get; set; } = 0;
        
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Total { get; set; }
        
        public int? CouponId { get; set; }
        
        [Required]
        public int ShippingAddressId { get; set; }
        
        public int? BillingAddressId { get; set; }
        
        [StringLength(1000)]
        public string? Notes { get; set; }
        
        public DateTime OrderDate { get; set; } = DateTime.Now;
        
        public DateTime? ShippedDate { get; set; }
        
        public DateTime? DeliveredDate { get; set; }
        
        [StringLength(100)]
        public string? TrackingNumber { get; set; }
        
        // Navigation properties
        [ForeignKey("UserId")]
        public virtual IdentityUser User { get; set; } = default!;
        
        [ForeignKey("CouponId")]
        public virtual Coupon? Coupon { get; set; }
        
        [ForeignKey("ShippingAddressId")]
        public virtual UserAddress ShippingAddress { get; set; } = default!;
        
        [ForeignKey("BillingAddressId")]
        public virtual UserAddress? BillingAddress { get; set; }
        
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
    
    public enum OrderStatus
    {
        Pending = 0,
        Confirmed = 1,
        Processing = 2,
        Shipped = 3,
        Delivered = 4,
        Cancelled = 5,
        Refunded = 6
    }
}