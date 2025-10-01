using InventoryManagement.Domain.Entities;

namespace InventoryManagement.Infrastructure.Specifications
{
    public class OrdersByCustomerSpecification : BaseSpecification<Order>
    {
        public OrdersByCustomerSpecification(string customerId, int pageNumber, int pageSize)
            : base(o => o.CustomerId == customerId)
        {
            AddInclude(o => o.OrderItems);
            ApplyOrderByDescending(o => o.OrderDate);
            ApplyPaging((pageNumber - 1) * pageSize, pageSize);
        }
    }
}
