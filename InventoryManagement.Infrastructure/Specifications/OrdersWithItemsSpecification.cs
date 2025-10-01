using InventoryManagement.Domain.Entities;

namespace InventoryManagement.Infrastructure.Specifications
{
    public class OrdersWithItemsSpecification : BaseSpecification<Order>
    {
        public OrdersWithItemsSpecification(long orderId)
            : base(o => o.OrderId == orderId)
        {
            AddInclude(o => o.OrderItems);
            AddInclude("OrderItems.Product");
            AddInclude(o => o.AssignedWarehouse);
        }
    }
}
