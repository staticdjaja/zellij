using Microsoft.EntityFrameworkCore;
using zellij.Data;
using zellij.Models;

namespace zellij.Repositories
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        public ProductRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Product>> SearchProductsAsync(string? searchTerm, string? origin, string? color, decimal? minPrice, decimal? maxPrice)
        {
            var query = _dbSet.AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(p => p.Name.Contains(searchTerm) ||
                                        p.Description.Contains(searchTerm) ||
                                        p.MarbleType.Contains(searchTerm));
            }

            if (!string.IsNullOrEmpty(origin))
            {
                query = query.Where(p => p.Origin == origin);
            }

            if (!string.IsNullOrEmpty(color))
            {
                query = query.Where(p => p.Color == color);
            }

            if (minPrice.HasValue)
            {
                query = query.Where(p => p.Price >= minPrice);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= maxPrice);
            }

            return await query.Where(p => p.InStock).ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetInStockProductsAsync()
        {
            return await _dbSet.Where(p => p.InStock && p.StockQuantity > 0).ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetProductsByOriginAsync(string origin)
        {
            return await _dbSet.Where(p => p.Origin == origin && p.InStock).ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetProductsByColorAsync(string color)
        {
            return await _dbSet.Where(p => p.Color == color && p.InStock).ToListAsync();
        }

        public async Task<bool> UpdateStockAsync(int productId, int quantity)
        {
            var product = await GetByIdAsync(productId);
            if (product == null) return false;

            product.StockQuantity = quantity;
            product.InStock = quantity > 0;
            product.ModifiedDate = DateTime.UtcNow;

            await UpdateAsync(product);
            return true;
        }

        public async Task<IEnumerable<string>> GetDistinctOriginsAsync()
        {
            return await _dbSet.Where(p => p.InStock)
                              .Select(p => p.Origin)
                              .Distinct()
                              .OrderBy(o => o)
                              .ToListAsync();
        }

        public async Task<IEnumerable<string>> GetDistinctColorsAsync()
        {
            return await _dbSet.Where(p => p.InStock)
                              .Select(p => p.Color)
                              .Distinct()
                              .OrderBy(c => c)
                              .ToListAsync();
        }
    }
}