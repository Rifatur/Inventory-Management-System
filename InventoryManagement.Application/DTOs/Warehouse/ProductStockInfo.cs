namespace InventoryManagement.Application.DTOs.Warehouse
{
    public class ProductStockInfo
    {
        public long ProductId { get; set; }
        public string SKU { get; set; }
        public string ProductName { get; set; }
        public int QuantityOnHand { get; set; }
        public int QuantityReserved { get; set; }
        public int QuantityAvailable => QuantityOnHand - QuantityReserved;
        public decimal Value { get; set; }
        public int? ReorderLevel { get; set; }
        public int? ReorderQuantity { get; set; }
    }
}
