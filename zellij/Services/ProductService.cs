using zellij.Models;
using zellij.Repositories;

namespace zellij.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;

        public ProductService(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<Product?> GetProductAsync(int id)
        {
            return await _productRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Product>> GetAllProductsAsync()
        {
            return await _productRepository.GetAllAsync();
        }

        public async Task<IEnumerable<Product>> SearchProductsAsync(string? searchTerm, string? origin, string? color, decimal? minPrice, decimal? maxPrice)
        {
            return await _productRepository.SearchProductsAsync(searchTerm, origin, color, minPrice, maxPrice);
        }

        public async Task<IEnumerable<Product>> GetInStockProductsAsync()
        {
            return await _productRepository.GetInStockProductsAsync();
        }

        public async Task<Product> CreateProductAsync(Product product)
        {
            product.CreatedDate = DateTime.UtcNow;
            return await _productRepository.AddAsync(product);
        }

        public async Task<Product> UpdateProductAsync(Product product)
        {
            product.ModifiedDate = DateTime.UtcNow;
            return await _productRepository.UpdateAsync(product);
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            return await _productRepository.DeleteAsync(id);
        }

        public async Task<bool> UpdateStockAsync(int productId, int quantity)
        {
            return await _productRepository.UpdateStockAsync(productId, quantity);
        }

        public async Task<IEnumerable<string>> GetDistinctOriginsAsync()
        {
            return await _productRepository.GetDistinctOriginsAsync();
        }

        public async Task<IEnumerable<string>> GetDistinctColorsAsync()
        {
            return await _productRepository.GetDistinctColorsAsync();
        }

        public async Task<int> GetInStockCountAsync()
        {
            return await _productRepository.CountAsync(p => p.InStock && p.StockQuantity > 0);
        }

        public async Task<int> GetOutOfStockCountAsync()
        {
            return await _productRepository.CountAsync(p => !p.InStock || p.StockQuantity == 0);
        }
    }
}