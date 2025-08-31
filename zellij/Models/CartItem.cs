using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace zellij.Models
{
    public class CartItem
    {
        public int Id { get; set; }
        
        [Required]
        public string UserId { get; set; } = string.Empty;
        
        [Required]
        public int ProductId { get; set; }
        
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal PriceAtTimeOfAdd { get; set; } // Store price when added to cart
        
        public DateTime AddedDate { get; set; } = DateTime.Now;
        
        // Navigation properties
        [ForeignKey("UserId")]
        public virtual IdentityUser User { get; set; } = default!;
        
        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; } = default!;
        
        // Calculated properties
        [NotMapped]
        public decimal Total => PriceAtTimeOfAdd * Quantity;
    }
}