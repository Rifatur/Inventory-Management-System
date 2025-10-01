using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.Interfaces.Repositories;
using InventoryManagement.Infrastructure.Data;
using InventoryManagement.Infrastructure.Repositories.Base;

namespace InventoryManagement.Infrastructure.Repositories
{
    public class WarehouseRepository : Repository<Warehouse>, IWarehouseRepository
    {
        public WarehouseRepository(ApplicationDbContext context) : base(context)
        {
        }

        public Task<IReadOnlyList<Warehouse>> GetActiveWarehousesAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Warehouse> GetNearestWarehouseAsync(string postalCode)
        {
            throw new NotImplementedException();
        }

        public Task<Warehouse> GetWarehouseByCodeAsync(string warehouseCode)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<Warehouse>> GetWarehousesByTypeAsync(string warehouseType)
        {
            throw new NotImplementedException();
        }

        public Task<Dictionary<int, decimal>> GetWarehouseUtilizationAsync()
        {
            throw new NotImplementedException();
        }
    }
}
