using InventoryManagement.Application.DTOs.Common;
using InventoryManagement.Application.DTOs.Inventory;
using InventoryManagement.Application.DTOs.Order;
using InventoryManagement.Application.DTOs.Product;
using InventoryManagement.Application.DTOs.Stock;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagement.Application.Interfaces
{
    /// <summary>
    /// Interface for inventory management operations
    /// </summary>
    public interface IInventoryService
    {
        // Product Management
        Task<ApiResponse<ProductDto>> CreateProductAsync(CreateProductDto productDto, string userId);
        Task<ApiResponse<ProductDto>> UpdateProductAsync(UpdateProductDto productDto, string userId);
        Task<ApiResponse<ProductDto>> GetProductByIdAsync(long productId);
        Task<ApiResponse<ProductDto>> GetProductBySKUAsync(string sku);
        Task<ApiResponse<PaginatedResponse<ProductDto>>> GetProductsAsync(InventorySearchDto searchDto);
        Task<ApiResponse<bool>> DeleteProductAsync(long productId, string userId);

        // Inventory Management
        Task<ApiResponse<InventoryItemDto>> GetInventoryItemAsync(long productId, long warehouseId);
        Task<ApiResponse<List<InventoryItemDto>>> GetInventoryByProductAsync(long productId);
        Task<ApiResponse<List<InventoryItemDto>>> GetInventoryByWarehouseAsync(long warehouseId);
        Task<ApiResponse<bool>> UpdateInventoryAsync(UpdateInventoryDto updateDto, string userId);
        Task<ApiResponse<bool>> TransferInventoryAsync(InventoryTransferDto transferDto, string userId);
        Task<ApiResponse<bool>> AdjustInventoryAsync(long productId, long warehouseId, int adjustmentQuantity, string reason, string userId);

        // Stock Monitoring
        Task<ApiResponse<List<LowStockAlertDto>>> GetLowStockItemsAsync(long? warehouseId = null);
        Task<ApiResponse<List<InventoryItemDto>>> GetOutOfStockItemsAsync(long? warehouseId = null);
        Task<ApiResponse<InventorySummaryDto>> GetInventorySummaryAsync();
        Task<ApiResponse<List<StockMovementDto>>> GetStockMovementsAsync(long? productId, long? warehouseId, DateTime? fromDate, DateTime? toDate);

        // Reservation Management
        Task<ApiResponse<bool>> ReserveInventoryAsync(long orderId, List<CreateOrderItemDto> items);
        Task<ApiResponse<bool>> ReleaseReservationAsync(long orderId);
        Task<ApiResponse<List<InventoryReservationDto>>> GetActiveReservationsAsync(long productId);
        Task<ApiResponse<bool>> ProcessExpiredReservationsAsync();
    }
}
