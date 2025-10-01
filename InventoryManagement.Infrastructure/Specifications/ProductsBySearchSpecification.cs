using InventoryManagement.Domain.Entities;

namespace InventoryManagement.Infrastructure.Specifications
{
    public class ProductsBySearchSpecification : BaseSpecification<Product>
    {
        public ProductsBySearchSpecification(string searchTerm, string category,
            bool? lowStockOnly, int pageNumber, int pageSize)
            : base(p => p.IsActive)
        {
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                AddCriteria(p => p.SKU.Contains(searchTerm) ||
                               p.Name.Contains(searchTerm) ||
                               p.Description.Contains(searchTerm));
            }

            if (!string.IsNullOrWhiteSpace(category))
            {
                AddCriteria(p => p.Category == category);
            }

            if (lowStockOnly == true)
            {
                AddInclude(p => p.InventoryItems);
                AddCriteria(p => p.InventoryItems.Sum(i => i.QuantityOnHand) <= p.ReorderLevel);
            }

            ApplyOrderBy(p => p.Name);
            ApplyPaging((pageNumber - 1) * pageSize, pageSize);
        }
    }
}
