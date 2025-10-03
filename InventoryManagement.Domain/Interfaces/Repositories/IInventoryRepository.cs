using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.Interfaces;

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
        Task<int> GetTotalQuantityAsync(long productId);
        Task<bool> CheckAvailabilityAsync(long productId, long quantity);
        Task<bool> ReserveStockAsync(long productId, long warehouseId, int quantity, long orderId);
        Task<bool> ReleaseStockAsync(long orderId);
        Task<Dictionary<long, int>> GetStockLevelsAsync(List<long> productIds);
    }
}
