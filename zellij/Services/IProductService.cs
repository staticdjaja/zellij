using zellij.Models;

namespace zellij.Services
{
    public interface IProductService
    {
        Task<Product?> GetProductAsync(int id);
        Task<IEnumerable<Product>> GetAllProductsAsync();
        Task<IEnumerable<Product>> SearchProductsAsync(string? searchTerm, string? origin, string? color, decimal? minPrice, decimal? maxPrice);
        Task<IEnumerable<Product>> GetInStockProductsAsync();
        Task<Product> CreateProductAsync(Product product);
        Task<Product> UpdateProductAsync(Product product);
        Task<bool> DeleteProductAsync(int id);
        Task<bool> UpdateStockAsync(int productId, int quantity);
        Task<IEnumerable<string>> GetDistinctOriginsAsync();
        Task<IEnumerable<string>> GetDistinctColorsAsync();
        Task<int> GetInStockCountAsync();
        Task<int> GetOutOfStockCountAsync();
    }
}