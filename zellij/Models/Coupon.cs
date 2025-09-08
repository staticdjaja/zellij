using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace zellij.Models
{
    public class Coupon
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Coupon Code")]
        public string Code { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(5,2)")]
        [Display(Name = "Discount Percentage")]
        [Range(0, 100)]
        public decimal DiscountPercentage { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Minimum Order Amount")]
        public decimal? MinimumOrderAmount { get; set; }

        [Required]
        [Display(Name = "Valid From")]
        public DateTime ValidFrom { get; set; }

        [Required]
        [Display(Name = "Valid Until")]
        public DateTime ValidUntil { get; set; }

        [Display(Name = "Usage Limit")]
        public int? UsageLimit { get; set; }

        [Display(Name = "Times Used")]
        public int TimesUsed { get; set; } = 0;

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Email Confirmation Required")]
        public bool RequireEmailConfirmation { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Navigation property
        public virtual ICollection<CouponUsage> CouponUsages { get; set; } = new List<CouponUsage>();

        // Helper properties
        public bool IsValid => IsActive && DateTime.Now >= ValidFrom && DateTime.Now <= ValidUntil &&
                              (UsageLimit == null || TimesUsed < UsageLimit);
    }
}