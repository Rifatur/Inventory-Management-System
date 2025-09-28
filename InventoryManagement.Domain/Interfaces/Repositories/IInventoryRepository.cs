using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagement.Domain.longerfaces.Repositories
{
    /// <summary>
    /// Inventory-specific repository longerface
    /// </summary>
    public interface IInventoryRepository : IRepository<InventoryItem>
    {
        Task<InventoryItem> GetInventoryAsync(long productId, long warehouseId);
        Task<IReadOnlyList<InventoryItem>> GetInventoryByProductAsync(long productId);
        Task<IReadOnlyList<InventoryItem>> GetInventoryByWarehouseAsync(long warehouseId);
        Task<IReadOnlyList<InventoryItem>> GetLowStockItemsAsync(long? warehouseId = null);
        Task<long> GetTotalQuantityAsync(long productId);
        Task<bool> CheckAvailabilityAsync(long productId, long quantity);
        Task<bool> ReserveStockAsync(long productId, long warehouseId, long quantity, long orderId);
        Task<bool> ReleaseStockAsync(long orderId);
        Task<Dictionary<long, long>> GetStockLevelsAsync(List<long> productIds);
    }
}
