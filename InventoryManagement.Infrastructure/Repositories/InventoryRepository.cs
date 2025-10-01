using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.longerfaces.Repositories;
using InventoryManagement.Infrastructure.Data;
using InventoryManagement.Infrastructure.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Infrastructure.Repositories
{
    /// <summary>
    /// Inventory repository implementation with business-specific methods
    /// </summary>
    public class InventoryRepository : Repository<InventoryItem>, IInventoryRepository
    {
        public InventoryRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<InventoryItem> GetInventoryAsync(long productId, long warehouseId)
        {
            return await _context.InventoryItems
                .Include(i => i.Product)
                .Include(i => i.Warehouse)
                .FirstOrDefaultAsync(i => i.ProductId == productId &&
                                        i.WarehouseId == warehouseId);
        }

        public async Task<IReadOnlyList<InventoryItem>> GetInventoryByProductAsync(long productId)
        {
            return await _context.InventoryItems
                .Include(i => i.Warehouse)
                .Where(i => i.ProductId == productId)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<InventoryItem>> GetInventoryByWarehouseAsync(long warehouseId)
        {
            return await _context.InventoryItems
                .Include(i => i.Product)
                .Where(i => i.WarehouseId == warehouseId)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<InventoryItem>> GetLowStockItemsAsync(long? warehouseId = null)
        {
            var query = _context.InventoryItems
                .Include(i => i.Product)
                .Include(i => i.Warehouse)
                .Where(i => i.Product.IsActive &&
                           i.QuantityOnHand <= i.Product.ReorderLevel);

            if (warehouseId.HasValue)
            {
                query = query.Where(i => i.WarehouseId == warehouseId.Value);
            }

            return await query.ToListAsync();
        }

        public async Task<long> GetTotalQuantityAsync(long productId)
        {
            return await _context.InventoryItems
                .Where(i => i.ProductId == productId)
                .SumAsync(i => i.QuantityOnHand);
        }

        public async Task<bool> CheckAvailabilityAsync(long productId, long quantity)
        {
            var totalAvailable = await _context.InventoryItems
                .Where(i => i.ProductId == productId)
                .SumAsync(i => i.QuantityAvailable);

            return totalAvailable >= quantity;
        }

        public async Task<bool> ReserveStockAsync(long productId, long warehouseId, int quantity, long orderId)
        {
            var inventory = await GetInventoryAsync(productId, warehouseId);

            if (inventory == null || inventory.QuantityAvailable < quantity)
            {
                return false;
            }

            // Create reservation
            var reservation = new InventoryReservation
            {
                InventoryId = inventory.InventoryId,
                OrderId = orderId,
                QuantityReserved = quantity,
                Status = "Active",
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddHours(24)
            };

            _context.InventoryReservations.Add(reservation);

            // Update inventory
            inventory.QuantityReserved += quantity;
            inventory.LastUpdated = DateTime.UtcNow;

            return true;
        }

        public async Task<bool> ReleaseStockAsync(long orderId)
        {
            var reservations = await _context.InventoryReservations
                .Include(r => r.InventoryItem)
                .Where(r => r.OrderId == orderId && r.Status == "Active")
                .ToListAsync();

            foreach (var reservation in reservations)
            {
                reservation.InventoryItem.QuantityReserved -= reservation.QuantityReserved;
                reservation.InventoryItem.LastUpdated = DateTime.UtcNow;
                reservation.Status = "Released";
                reservation.ReleasedAt = DateTime.UtcNow;
            }

            return reservations.Any();
        }

        public async Task<Dictionary<long, int>> GetStockLevelsAsync(List<long> productIds)
        {
            return await _context.InventoryItems
                .Where(i => productIds.Contains(i.ProductId))
                .GroupBy(i => i.ProductId)
                .Select(g => new { ProductId = g.Key, Total = g.Sum(i => i.QuantityOnHand) })
                .ToDictionaryAsync(x => x.ProductId, x => x.Total);
        }
    }
}
