using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace zellij.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [StringLength(255)]
        public string ImageUrl { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string MarbleType { get; set; } = string.Empty; // e.g., "Carrara", "Verde", "Noir", etc.

        [Required]
        [StringLength(50)]
        public string Origin { get; set; } = string.Empty; // Moroccan regions like "Agadir", "Fez", "Meknes"

        [StringLength(50)]
        public string Color { get; set; } = string.Empty;

        [StringLength(100)]
        public string Finish { get; set; } = string.Empty; // e.g., "Polished", "Honed", "Tumbled"

        [StringLength(50)]
        public string Dimensions { get; set; } = string.Empty; // e.g., "12x12", "24x24"

        public bool InStock { get; set; } = true;

        public int StockQuantity { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime? ModifiedDate { get; set; }
    }
}