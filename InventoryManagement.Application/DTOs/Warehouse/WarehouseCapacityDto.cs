namespace InventoryManagement.Application.DTOs.Warehouse
{
    public class WarehouseCapacityDto
    {
        public long WarehouseId { get; set; }
        public string WarehouseCode { get; set; }
        public string WarehouseName { get; set; }
        public string WarehouseType { get; set; }
        public int MaxCapacity { get; set; }
        public decimal CurrentUtilization { get; set; }
        public int AvailableCapacity { get; set; }
        public decimal UtilizationPercentage { get; set; }
        public string Status { get; set; } // Low, Medium, High, Critical
        public List<CapacityTrend> UtilizationTrend { get; set; }
    }
}
