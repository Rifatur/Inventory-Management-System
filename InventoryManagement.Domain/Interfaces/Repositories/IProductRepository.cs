using InventoryManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagement.Domain.Interfaces.Repositories
{
    /// <summary>
    /// Product-specific repository interface
    /// </summary>
    public interface IProductRepository : IRepository<Product>
    {
        Task<Product> GetProductWithInventoryAsync(int productId);
        Task<Product> GetProductBySKUAsync(string sku);
        Task<IReadOnlyList<Product>> GetProductsByCategoryAsync(string category);
        Task<IReadOnlyList<Product>> GetLowStockProductsAsync(int threshold);
        Task<bool> IsSkuUniqueAsync(string sku, int? excludeProductId = null);
        Task<Dictionary<string, int>> GetProductCountByCategoryAsync();
    }
}
