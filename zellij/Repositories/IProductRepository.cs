using zellij.Models;

namespace zellij.Repositories
{
    public interface IProductRepository : IRepository<Product>
    {
        Task<IEnumerable<Product>> SearchProductsAsync(string? searchTerm, string? origin, string? color, decimal? minPrice, decimal? maxPrice);
        Task<IEnumerable<Product>> GetInStockProductsAsync();
        Task<IEnumerable<Product>> GetProductsByOriginAsync(string origin);
        Task<IEnumerable<Product>> GetProductsByColorAsync(string color);
        Task<bool> UpdateStockAsync(int productId, int quantity);
        Task<IEnumerable<string>> GetDistinctOriginsAsync();
        Task<IEnumerable<string>> GetDistinctColorsAsync();
    }
}