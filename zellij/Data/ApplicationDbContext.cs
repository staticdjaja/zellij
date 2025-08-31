using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using zellij.Models;

namespace zellij.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<UserAddress> UserAddresses { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Coupon> Coupons { get; set; }
        public DbSet<CouponUsage> CouponUsages { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure relationships
            builder.Entity<UserAddress>()
                .HasOne(ua => ua.User)
                .WithMany()
                .HasForeignKey(ua => ua.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<CartItem>()
                .HasOne(ci => ci.User)
                .WithMany()
                .HasForeignKey(ci => ci.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<CartItem>()
                .HasOne(ci => ci.Product)
                .WithMany()
                .HasForeignKey(ci => ci.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany()
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Order>()
                .HasOne(o => o.ShippingAddress)
                .WithMany()
                .HasForeignKey(o => o.ShippingAddressId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Order>()
                .HasOne(o => o.BillingAddress)
                .WithMany()
                .HasForeignKey(o => o.BillingAddressId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<OrderItem>()
                .HasOne(oi => oi.Product)
                .WithMany()
                .HasForeignKey(oi => oi.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<CouponUsage>()
                .HasOne(cu => cu.Coupon)
                .WithMany(c => c.CouponUsages)
                .HasForeignKey(cu => cu.CouponId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<CouponUsage>()
                .HasOne(cu => cu.User)
                .WithMany()
                .HasForeignKey(cu => cu.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Ensure only one default address per user
            builder.Entity<UserAddress>()
                .HasIndex(ua => new { ua.UserId, ua.IsDefault })
                .HasFilter("[IsDefault] = 1");

            // Ensure unique coupon codes
            builder.Entity<Coupon>()
                .HasIndex(c => c.Code)
                .IsUnique();

            // Ensure unique cart items per user/product combination
            builder.Entity<CartItem>()
                .HasIndex(ci => new { ci.UserId, ci.ProductId })
                .IsUnique();

            // Seed some Moroccan marble products
            builder.Entity<Product>().HasData(
                new Product
                {
                    Id = 1,
                    Name = "Moroccan Carrara White",
                    Description = "Beautiful white marble from the Atlas Mountains with elegant veining, perfect for luxury interiors.",
                    Price = 85.99m,
                    ImageUrl = "/images/products/carrara-white.jpg",
                    MarbleType = "Carrara",
                    Origin = "Atlas Mountains",
                    Color = "White",
                    Finish = "Polished",
                    Dimensions = "24x24",
                    InStock = true,
                    StockQuantity = 50,
                    CreatedDate = DateTime.Now
                },
                new Product
                {
                    Id = 2,
                    Name = "Verde Atlas Marble",
                    Description = "Stunning green marble with natural patterns, sourced from the Rif Mountains region.",
                    Price = 120.50m,
                    ImageUrl = "/images/products/verde-atlas.jpg",
                    MarbleType = "Verde",
                    Origin = "Rif Mountains",
                    Color = "Green",
                    Finish = "Honed",
                    Dimensions = "18x18",
                    InStock = true,
                    StockQuantity = 35,
                    CreatedDate = DateTime.Now
                },
                new Product
                {
                    Id = 3,
                    Name = "Noir Sahara Black",
                    Description = "Dramatic black marble with gold veining from the Sahara region, ideal for countertops.",
                    Price = 95.75m,
                    ImageUrl = "/images/products/noir-sahara.jpg",
                    MarbleType = "Noir",
                    Origin = "Sahara",
                    Color = "Black",
                    Finish = "Polished",
                    Dimensions = "30x30",
                    InStock = true,
                    StockQuantity = 25,
                    CreatedDate = DateTime.Now
                },
                new Product
                {
                    Id = 4,
                    Name = "Rose Atlas Pink",
                    Description = "Elegant pink marble with soft patterns from the Atlas Mountains, perfect for bathrooms.",
                    Price = 110.25m,
                    ImageUrl = "/images/products/rose-atlas.jpg",
                    MarbleType = "Rose",
                    Origin = "Atlas Mountains",
                    Color = "Pink",
                    Finish = "Tumbled",
                    Dimensions = "12x12",
                    InStock = true,
                    StockQuantity = 40,
                    CreatedDate = DateTime.Now
                },
                new Product
                {
                    Id = 5,
                    Name = "Beige Fez Marble",
                    Description = "Classic beige marble with subtle veining from the Fez region, versatile for any application.",
                    Price = 78.90m,
                    ImageUrl = "/images/products/beige-fez.jpg",
                    MarbleType = "Beige",
                    Origin = "Fez",
                    Color = "Beige",
                    Finish = "Honed",
                    Dimensions = "20x20",
                    InStock = true,
                    StockQuantity = 60,
                    CreatedDate = DateTime.Now
                }
            );

            // Seed some coupons
            builder.Entity<Coupon>().HasData(
                new Coupon
                {
                    Id = 1,
                    Code = "WELCOME10",
                    Description = "Welcome discount for new customers - 10% off",
                    DiscountPercentage = 10.00m,
                    MinimumOrderAmount = 100.00m,
                    ValidFrom = DateTime.Now.AddDays(-30),
                    ValidUntil = DateTime.Now.AddDays(365),
                    UsageLimit = null,
                    IsActive = true,
                    RequireEmailConfirmation = true,
                    CreatedDate = DateTime.Now
                },
                new Coupon
                {
                    Id = 2,
                    Code = "SUMMER15",
                    Description = "Summer sale - 15% off orders over $500",
                    DiscountPercentage = 15.00m,
                    MinimumOrderAmount = 500.00m,
                    ValidFrom = DateTime.Now.AddDays(-10),
                    ValidUntil = DateTime.Now.AddDays(90),
                    UsageLimit = 100,
                    IsActive = true,
                    RequireEmailConfirmation = true,
                    CreatedDate = DateTime.Now
                },
                new Coupon
                {
                    Id = 3,
                    Code = "LUXURY20",
                    Description = "Luxury collection - 20% off premium marble",
                    DiscountPercentage = 20.00m,
                    MinimumOrderAmount = 1000.00m,
                    ValidFrom = DateTime.Now,
                    ValidUntil = DateTime.Now.AddDays(60),
                    UsageLimit = 50,
                    IsActive = true,
                    RequireEmailConfirmation = true,
                    CreatedDate = DateTime.Now
                }
            );
        }
    }
}