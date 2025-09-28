using InventoryManagement.Application.DTOs.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagement.Application.Interfaces
{
    /// <summary>
    /// Interface for reporting operations
    /// </summary>
    public interface IReportingService
    {
        Task<ApiResponse<byte[]>> GenerateInventoryReportAsync(DateTime asOfDate, int? warehouseId = null);
        Task<ApiResponse<byte[]>> GenerateStockMovementReportAsync(DateTime fromDate, DateTime toDate, int? warehouseId = null);
        Task<ApiResponse<byte[]>> GenerateLowStockReportAsync();
        Task<ApiResponse<object>> GetInventoryValuationAsync(DateTime asOfDate);
        Task<ApiResponse<object>> GetOrderAnalyticsAsync(DateTime fromDate, DateTime toDate);
    }
}
