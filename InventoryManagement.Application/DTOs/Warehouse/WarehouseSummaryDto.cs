namespace InventoryManagement.Application.DTOs.Warehouse
{
    public class WarehouseSummaryDto : WarehouseDto
    {
        // Inventory statistics
        public int TotalProducts { get; set; }
        public int TotalQuantity { get; set; }
        public decimal TotalValue { get; set; }
        public int TotalReserved { get; set; }

        // Movement statistics
        public int InboundMovements { get; set; }
        public int OutboundMovements { get; set; }
        public decimal TotalMovementValue { get; set; }

        // Product information
        public List<ProductStockInfo> TopProductsByQuantity { get; set; }
        public List<ProductStockInfo> LowStockItems { get; set; }

        // Performance metrics
        public decimal AveragePickTime { get; set; }
        public decimal OrderFulfillmentRate { get; set; }
        public int PendingOrders { get; set; }

        public DateTime LastUpdated { get; set; }
    }
}
