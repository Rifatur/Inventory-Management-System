namespace InventoryManagement.Application.DTOs.Stock
{
    public class DailyMovementTrend
    {
        public DateTime Date { get; set; }
        public int MovementCount { get; set; }
        public int TotalQuantityIn { get; set; }
        public int TotalQuantityOut { get; set; }
        public decimal TotalValue { get; set; }
        public int UniqueProducts { get; set; }
    }
}
