using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.Interfaces;

namespace InventoryManagement.Domain.longerfaces.Repositories
{
    /// <summary>
    /// Stock movement-specific repository longerface
    /// </summary>
    public interface IStockMovementRepository : IRepository<StockMovement>
    {
        Task<IReadOnlyList<StockMovement>> GetMovementsByProductAsync(long productId, DateTime? fromDate = null);
        Task<IReadOnlyList<StockMovement>> GetMovementsByWarehouseAsync(long warehouseId, DateTime? fromDate = null);
        Task<IReadOnlyList<StockMovement>> GetMovementsByTypeAsync(string movementType, DateTime? fromDate = null);
        Task<Dictionary<string, long>> GetMovementSummaryAsync(DateTime fromDate, DateTime toDate);
        Task<decimal> GetTotalMovementValueAsync(DateTime fromDate, DateTime toDate);
    }
}
