using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace zellij.Models
{
    public class OrderItem
    {
        public int Id { get; set; }

        [Required]
        public int OrderId { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        [StringLength(100)]
        public string ProductName { get; set; } = string.Empty; // Store product name at time of order

        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; } // Store price at time of order

        [Column(TypeName = "decimal(18,2)")]
        public decimal Total { get; set; }

        [StringLength(255)]
        public string? ProductImageUrl { get; set; } // Store image URL at time of order

        [StringLength(500)]
        public string? ProductDescription { get; set; } // Store description at time of order

        // Navigation properties
        [ForeignKey("OrderId")]
        public virtual Order Order { get; set; } = default!;

        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; } = default!;
    }
}