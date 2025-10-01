using InventoryManagement.Application.DTOs.Stock;
using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.longerfaces.Repositories;
using InventoryManagement.Infrastructure.Data;
using InventoryManagement.Infrastructure.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Infrastructure.Repositories
{
    /// <summary>
    /// Repository implementation for stock movement operations
    /// </summary>
    public class StockMovementRepository : Repository<StockMovement>, IStockMovementRepository
    {
        public StockMovementRepository(ApplicationDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Gets all stock movements for a specific product
        /// </summary>
        /// <param name="productId">The product ID to filter by</param>
        /// <param name="fromDate">Optional start date filter</param>
        /// <returns>List of stock movements for the product</returns>
        public async Task<IReadOnlyList<StockMovement>> GetMovementsByProductAsync(long productId, DateTime? fromDate = null)
        {
            var query = _context.StockMovements
                .Include(sm => sm.Product)
                .Include(sm => sm.FromWarehouse)
                .Include(sm => sm.ToWarehouse)
                .Where(sm => sm.ProductId == productId);

            if (fromDate.HasValue)
            {
                query = query.Where(sm => sm.CreatedAt >= fromDate.Value);
            }

            return await query
                .OrderByDescending(sm => sm.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Gets all stock movements for a specific warehouse
        /// </summary>
        /// <param name="warehouseId">The warehouse ID to filter by</param>
        /// <param name="fromDate">Optional start date filter</param>
        /// <returns>List of stock movements involving the warehouse</returns>
        public async Task<IReadOnlyList<StockMovement>> GetMovementsByWarehouseAsync(long warehouseId, DateTime? fromDate = null)
        {
            var query = _context.StockMovements
                .Include(sm => sm.Product)
                .Include(sm => sm.FromWarehouse)
                .Include(sm => sm.ToWarehouse)
                .Where(sm => sm.FromWarehouseId == warehouseId || sm.ToWarehouseId == warehouseId);

            if (fromDate.HasValue)
            {
                query = query.Where(sm => sm.CreatedAt >= fromDate.Value);
            }

            return await query
                .OrderByDescending(sm => sm.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Gets all stock movements of a specific type
        /// </summary>
        /// <param name="movementType">The movement type to filter by</param>
        /// <param name="fromDate">Optional start date filter</param>
        /// <returns>List of stock movements of the specified type</returns>
        public async Task<IReadOnlyList<StockMovement>> GetMovementsByTypeAsync(string movementType, DateTime? fromDate = null)
        {
            var query = _context.StockMovements
                .Include(sm => sm.Product)
                .Include(sm => sm.FromWarehouse)
                .Include(sm => sm.ToWarehouse)
                .Where(sm => sm.MovementType == movementType);

            if (fromDate.HasValue)
            {
                query = query.Where(sm => sm.CreatedAt >= fromDate.Value);
            }

            return await query
                .OrderByDescending(sm => sm.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Gets a summary of stock movements grouped by type within a date range
        /// </summary>
        /// <param name="fromDate">Start date for the summary</param>
        /// <param name="toDate">End date for the summary</param>
        /// <returns>Dictionary with movement type as key and count as value</returns>
        public async Task<Dictionary<string, int>> GetMovementSummaryAsync(DateTime fromDate, DateTime toDate)
        {
            var summary = await _context.StockMovements
                .Where(sm => sm.CreatedAt >= fromDate && sm.CreatedAt <= toDate)
                .GroupBy(sm => sm.MovementType)
                .Select(g => new
                {
                    MovementType = g.Key,
                    Count = g.Count()
                })
                .ToDictionaryAsync(x => x.MovementType, x => x.Count);

            return summary;
        }

        /// <summary>
        /// Calculates the total value of all stock movements within a date range
        /// </summary>
        /// <param name="fromDate">Start date for calculation</param>
        /// <param name="toDate">End date for calculation</param>
        /// <returns>Total value of stock movements</returns>
        public async Task<decimal> GetTotalMovementValueAsync(DateTime fromDate, DateTime toDate)
        {
            var totalValue = await _context.StockMovements
                .Where(sm => sm.CreatedAt >= fromDate && sm.CreatedAt <= toDate)
                .SumAsync(sm => sm.TotalCost);

            return totalValue;
        }

        // Additional useful methods for stock movement analysis

        /// <summary>
        /// Gets stock movements with advanced filtering
        /// </summary>
        public async Task<IReadOnlyList<StockMovement>> GetMovementsAsync(StockMovementFilter filter)
        {
            var query = _context.StockMovements
                .Include(sm => sm.Product)
                .Include(sm => sm.FromWarehouse)
                .Include(sm => sm.ToWarehouse)
                .AsQueryable();

            // Apply filters
            if (filter.ProductId.HasValue)
            {
                query = query.Where(sm => sm.ProductId == filter.ProductId.Value);
            }

            if (filter.FromWarehouseId.HasValue)
            {
                query = query.Where(sm => sm.FromWarehouseId == filter.FromWarehouseId.Value);
            }

            if (filter.ToWarehouseId.HasValue)
            {
                query = query.Where(sm => sm.ToWarehouseId == filter.ToWarehouseId.Value);
            }

            if (!string.IsNullOrWhiteSpace(filter.MovementType))
            {
                query = query.Where(sm => sm.MovementType == filter.MovementType);
            }

            if (filter.FromDate.HasValue)
            {
                query = query.Where(sm => sm.CreatedAt >= filter.FromDate.Value);
            }

            if (filter.ToDate.HasValue)
            {
                query = query.Where(sm => sm.CreatedAt <= filter.ToDate.Value);
            }

            if (!string.IsNullOrWhiteSpace(filter.ReferenceNumber))
            {
                query = query.Where(sm => sm.ReferenceNumber.Contains(filter.ReferenceNumber));
            }

            if (!string.IsNullOrWhiteSpace(filter.CreatedBy))
            {
                query = query.Where(sm => sm.CreatedBy == filter.CreatedBy);
            }

            // Apply sorting
            query = filter.SortBy?.ToLower() switch
            {
                "date" => filter.SortDescending ?
                    query.OrderByDescending(sm => sm.CreatedAt) :
                    query.OrderBy(sm => sm.CreatedAt),
                "quantity" => filter.SortDescending ?
                    query.OrderByDescending(sm => sm.Quantity) :
                    query.OrderBy(sm => sm.Quantity),
                "value" => filter.SortDescending ?
                    query.OrderByDescending(sm => sm.TotalCost) :
                    query.OrderBy(sm => sm.TotalCost),
                _ => query.OrderByDescending(sm => sm.CreatedAt)
            };

            // Apply paging
            if (filter.PageNumber > 0 && filter.PageSize > 0)
            {
                query = query
                    .Skip((filter.PageNumber - 1) * filter.PageSize)
                    .Take(filter.PageSize);
            }

            return await query.ToListAsync();
        }

        /// <summary>
        /// Gets movement statistics for a product
        /// </summary>
        public async Task<StockMovementStatistics> GetMovementStatisticsAsync(int productId, DateTime fromDate, DateTime toDate)
        {
            var movements = await _context.StockMovements
                .Where(sm => sm.ProductId == productId &&
                           sm.CreatedAt >= fromDate &&
                           sm.CreatedAt <= toDate)
                .ToListAsync();

            var statistics = new StockMovementStatistics
            {
                ProductId = productId,
                FromDate = fromDate,
                ToDate = toDate,
                TotalMovements = movements.Count,
                TotalQuantityIn = movements.Where(m => m.Quantity > 0).Sum(m => m.Quantity),
                TotalQuantityOut = Math.Abs(movements.Where(m => m.Quantity < 0).Sum(m => m.Quantity)),
                TotalValue = movements.Sum(m => m.TotalCost),
                AverageMovementValue = movements.Any() ? movements.Average(m => m.TotalCost) : 0,
                MovementsByType = movements
                    .GroupBy(m => m.MovementType)
                    .ToDictionary(g => g.Key, g => g.Count())
            };

            return statistics;
        }

        /// <summary>
        /// Gets top products by movement frequency
        /// </summary>
        public async Task<IReadOnlyList<ProductMovementFrequency>> GetTopMovingProductsAsync(
            DateTime fromDate,
            DateTime toDate,
            int topCount = 10)
        {
            var topProducts = await _context.StockMovements
                .Where(sm => sm.CreatedAt >= fromDate && sm.CreatedAt <= toDate)
                .GroupBy(sm => new { sm.ProductId, sm.Product.SKU, sm.Product.Name })
                .Select(g => new ProductMovementFrequency
                {
                    ProductId = g.Key.ProductId,
                    SKU = g.Key.SKU,
                    ProductName = g.Key.Name,
                    MovementCount = g.Count(),
                    TotalQuantity = g.Sum(sm => Math.Abs(sm.Quantity)),
                    TotalValue = g.Sum(sm => sm.TotalCost)
                })
                .OrderByDescending(p => p.MovementCount)
                .Take(topCount)
                .ToListAsync();

            return topProducts;
        }

        /// <summary>
        /// Gets warehouse transfer summary
        /// </summary>
        public async Task<IReadOnlyList<WarehouseTransferSummary>> GetWarehouseTransferSummaryAsync(
            DateTime fromDate,
            DateTime toDate)
        {
            var transfers = await _context.StockMovements
                .Where(sm => sm.MovementType == "Transfer" &&
                           sm.FromWarehouseId.HasValue &&
                           sm.ToWarehouseId.HasValue &&
                           sm.CreatedAt >= fromDate &&
                           sm.CreatedAt <= toDate)
                .GroupBy(sm => new
                {
                    FromWarehouseId = sm.FromWarehouseId.Value,
                    FromWarehouseName = sm.FromWarehouse.Name,
                    ToWarehouseId = sm.ToWarehouseId.Value,
                    ToWarehouseName = sm.ToWarehouse.Name
                })
                .Select(g => new WarehouseTransferSummary
                {
                    FromWarehouseId = g.Key.FromWarehouseId,
                    FromWarehouseName = g.Key.FromWarehouseName,
                    ToWarehouseId = g.Key.ToWarehouseId,
                    ToWarehouseName = g.Key.ToWarehouseName,
                    TransferCount = g.Count(),
                    TotalQuantity = g.Sum(sm => sm.Quantity),
                    TotalValue = g.Sum(sm => sm.TotalCost)
                })
                .ToListAsync();

            return transfers;
        }

        /// <summary>
        /// Gets daily movement trends
        /// </summary>
        public async Task<IReadOnlyList<DailyMovementTrend>> GetDailyMovementTrendsAsync(
            DateTime fromDate,
            DateTime toDate)
        {
            var trends = await _context.StockMovements
                .Where(sm => sm.CreatedAt >= fromDate && sm.CreatedAt <= toDate)
                .GroupBy(sm => sm.CreatedAt.Date)
                .Select(g => new DailyMovementTrend
                {
                    Date = g.Key,
                    MovementCount = g.Count(),
                    TotalQuantityIn = g.Where(sm => sm.Quantity > 0).Sum(sm => sm.Quantity),
                    TotalQuantityOut = g.Where(sm => sm.Quantity < 0).Sum(sm => Math.Abs(sm.Quantity)),
                    TotalValue = g.Sum(sm => sm.TotalCost),
                    UniqueProducts = g.Select(sm => sm.ProductId).Distinct().Count()
                })
                .OrderBy(t => t.Date)
                .ToListAsync();

            return trends;
        }

        Task<Dictionary<string, long>> IStockMovementRepository.GetMovementSummaryAsync(DateTime fromDate, DateTime toDate)
        {
            throw new NotImplementedException();
        }
    }
}
