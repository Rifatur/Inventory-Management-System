namespace InventoryManagement.Application.DTOs.Warehouse
{
    public class CapacityTrend
    {
        public DateTime Date { get; set; }
        public decimal UtilizationPercentage { get; set; }
        public int ItemCount { get; set; }
    }
}
