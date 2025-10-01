namespace InventoryManagement.Application.DTOs.Stock
{
    public class StockMovementStatistics
    {
        public long ProductId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int TotalMovements { get; set; }
        public int TotalQuantityIn { get; set; }
        public int TotalQuantityOut { get; set; }
        public decimal TotalValue { get; set; }
        public decimal AverageMovementValue { get; set; }
        public Dictionary<string, int> MovementsByType { get; set; }
    }
}
