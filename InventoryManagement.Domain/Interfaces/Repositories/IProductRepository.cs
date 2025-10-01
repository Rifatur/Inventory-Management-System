using InventoryManagement.Domain.Entities;

namespace InventoryManagement.Domain.Interfaces.Repositories
{
    /// <summary>
    /// Product-specific repository interface
    /// </summary>
    public interface IProductRepository : IRepository<Product>
    {
        Task<Product> GetProductWithInventoryAsync(long productId);
        Task<Product> GetProductBySKUAsync(string sku);
        Task<IReadOnlyList<Product>> GetProductsByCategoryAsync(string category);
        Task<IReadOnlyList<Product>> GetLowStockProductsAsync(long threshold);
        Task<bool> IsSkuUniqueAsync(string sku, long? excludeProductId = null);
        Task<Dictionary<string, int>> GetProductCountByCategoryAsync();
    }
}
