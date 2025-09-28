using InventoryManagement.Application.DTOs.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagement.Application.Interfaces
{
    /// <summary>
    /// Interface for integration with external systems
    /// </summary>
    public interface IIntegrationService
    {
        Task<ApiResponse<bool>> SyncProductsFromERPAsync();
        Task<ApiResponse<bool>> PushInventoryLevelsToERPAsync();
        Task<ApiResponse<bool>> ImportOrdersFromEcommerceAsync(string source);
        Task<ApiResponse<bool>> UpdateOrderStatusInExternalSystemAsync(long orderId, string status, string externalSystem);
        Task<ApiResponse<bool>> NotifyWarehouseSystemAsync(long orderId, long warehouseId);
    }
}
