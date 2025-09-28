using InventoryManagement.Application.DTOs.Common;
using InventoryManagement.Application.DTOs.Inventory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagement.Application.Interfaces
{
    /// <summary>
    /// Interface for warehouse management operations
    /// </summary>
    public interface IWarehouseService
    {
        //Task<ApiResponse<WarehouseDto>> CreateWarehouseAsync(CreateWarehouseDto warehouseDto);
        //Task<ApiResponse<WarehouseDto>> UpdateWarehouseAsync(UpdateWarehouseDto warehouseDto);
        //Task<ApiResponse<WarehouseDto>> GetWarehouseByIdAsync(int warehouseId);
        //Task<ApiResponse<List<WarehouseDto>>> GetAllWarehousesAsync(bool activeOnly = true);
        Task<ApiResponse<WarehouseSummaryDto>> GetWarehouseSummaryAsync(int warehouseId);
        Task<ApiResponse<bool>> DeactivateWarehouseAsync(int warehouseId, string userId);
    }
}
