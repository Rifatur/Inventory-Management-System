using InventoryManagement.Domain.Entities;

namespace InventoryManagement.Infrastructure.Specifications
{
    public class ProductsWithInventorySpecification : BaseSpecification<Product>
    {
        public ProductsWithInventorySpecification(long? warehouseId = null)
            : base(p => p.IsActive)
        {
            AddInclude(p => p.InventoryItems);
            AddInclude("InventoryItems.Warehouse");

            if (warehouseId.HasValue)
            {
                AddCriteria(p => p.InventoryItems.Any(i => i.WarehouseId == warehouseId.Value));
            }
        }
    }
}
