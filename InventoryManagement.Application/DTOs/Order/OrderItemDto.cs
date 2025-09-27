using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagement.Application.DTOs.Order
{
    public class OrderItemDto
    {
        public long OrderItemId { get; set; }
        public long ProductId { get; set; }
        public long OrderId { get; set; }
        public string SKU { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal LineTotal { get; set; }
        public string FulfillmentStatus { get; set; }
        public long? FulfilledFromWarehouseId { get; set; }
        public string FulfilledFromWarehouseName { get; set; }
    }
}
