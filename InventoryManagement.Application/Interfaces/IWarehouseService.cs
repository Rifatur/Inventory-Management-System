using InventoryManagement.Application.DTOs.Common;
using InventoryManagement.Application.DTOs.Warehouse;

namespace InventoryManagement.Application.Interfaces
{
    /// <summary>
    /// Interface for warehouse management operations
    /// </summary>
    public interface IWarehouseService
    {
        Task<ApiResponse<WarehouseDto>> CreateWarehouseAsync(CreateWarehouseDto warehouseDto);
        Task<ApiResponse<WarehouseDto>> UpdateWarehouseAsync(UpdateWarehouseDto warehouseDto);
        Task<ApiResponse<WarehouseDto>> GetWarehouseByIdAsync(long warehouseId);
        Task<ApiResponse<List<WarehouseDto>>> GetAllWarehousesAsync(bool activeOnly = true);
        Task<ApiResponse<DTOs.Warehouse.WarehouseSummaryDto>> GetWarehouseSummaryAsync(long warehouseId);
        Task<ApiResponse<bool>> DeactivateWarehouseAsync(long warehouseId, string userId);
    }
}
