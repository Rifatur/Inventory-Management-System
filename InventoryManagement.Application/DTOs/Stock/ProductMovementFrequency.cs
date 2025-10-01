namespace InventoryManagement.Application.DTOs.Stock
{
    public class ProductMovementFrequency
    {
        public long ProductId { get; set; }
        public string SKU { get; set; }
        public string ProductName { get; set; }
        public int MovementCount { get; set; }
        public int TotalQuantity { get; set; }
        public decimal TotalValue { get; set; }
    }
}
