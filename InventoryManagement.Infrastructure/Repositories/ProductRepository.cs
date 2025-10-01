using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.Interfaces.Repositories;
using InventoryManagement.Infrastructure.Data;
using InventoryManagement.Infrastructure.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Infrastructure.Repositories
{
    /// <summary>
    /// Product repository implementation with custom methods
    /// </summary>
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        public ProductRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Product> GetProductWithInventoryAsync(long productId)
        {
            return await _context.Products
                .Include(p => p.InventoryItems)
                    .ThenInclude(i => i.Warehouse)
                .FirstOrDefaultAsync(p => p.ProductId == productId);
        }

        public async Task<Product> GetProductBySKUAsync(string sku)
        {
            return await _context.Products
                .Include(p => p.InventoryItems)
                .FirstOrDefaultAsync(p => p.SKU == sku);
        }

        public async Task<IReadOnlyList<Product>> GetProductsByCategoryAsync(string category)
        {
            return await _context.Products
                .Where(p => p.Category == category && p.IsActive)
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<Product>> GetLowStockProductsAsync(long threshold)
        {
            return await _context.Products
                .Include(p => p.InventoryItems)
                .Where(p => p.IsActive &&
                       p.InventoryItems.Sum(i => i.QuantityOnHand) <= threshold)
                .ToListAsync();
        }

        public async Task<bool> IsSkuUniqueAsync(string sku, long? excludeProductId = null)
        {
            var query = _context.Products.Where(p => p.SKU == sku);

            if (excludeProductId.HasValue)
            {
                query = query.Where(p => p.ProductId != excludeProductId.Value);
            }

            return !await query.AnyAsync();
        }

        public async Task<Dictionary<string, int>> GetProductCountByCategoryAsync()
        {
            return await _context.Products
                .Where(p => p.IsActive)
                .GroupBy(p => p.Category)
                .Select(g => new { Category = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Category, x => x.Count);
        }
    }
}
