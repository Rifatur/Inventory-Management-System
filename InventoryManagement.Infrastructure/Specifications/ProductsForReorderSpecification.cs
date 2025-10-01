using InventoryManagement.Domain.Entities;

namespace InventoryManagement.Infrastructure.Specifications
{
    public class ProductsForReorderSpecification : BaseSpecification<Product>
    {
        public ProductsForReorderSpecification()
            : base(p => p.IsActive)
        {
            AddInclude(p => p.InventoryItems);
            AddCriteria(p => p.InventoryItems.Sum(i => i.QuantityOnHand) <= p.ReorderLevel);
            ApplyOrderBy(p => p.ReorderLevel);
        }
    }
}
