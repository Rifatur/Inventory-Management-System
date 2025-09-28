using InventoryManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagement.Domain.Interfaces.Repositories
{
    /// <summary>
    /// Warehouse-specific repository interface
    /// </summary>
    public interface IWarehouseRepository : IRepository<Warehouse>
    {
        Task<Warehouse> GetWarehouseByCodeAsync(string warehouseCode);
        Task<IReadOnlyList<Warehouse>> GetActiveWarehousesAsync();
        Task<IReadOnlyList<Warehouse>> GetWarehousesByTypeAsync(string warehouseType);
        Task<Warehouse> GetNearestWarehouseAsync(string postalCode);
        Task<Dictionary<int, decimal>> GetWarehouseUtilizationAsync();
    }
}
