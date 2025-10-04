namespace InventoryManagement.Application.DTOs.Warehouse
{
    public class WarehouseDto
    {
        public long WarehouseId { get; set; }

        public string WarehouseCode { get; set; }

        public string Name { get; set; }

        public string WarehouseType { get; set; }

        public string Address { get; set; }

        public string City { get; set; }

        public string State { get; set; }

        public string Country { get; set; }

        public string PostalCode { get; set; }

        public string Phone { get; set; }

        public string ManagerEmail { get; set; }

        public int MaxCapacity { get; set; }

        public decimal CurrentUtilization { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public string UpdatedBy { get; set; }

        // Calculated properties
        public decimal UtilizationPercentage => MaxCapacity > 0
            ? Math.Round((CurrentUtilization / MaxCapacity) * 100, 2)
            : 0;

        public int AvailableCapacity => MaxCapacity - (int)(MaxCapacity * (CurrentUtilization / 100));

        public string Status => CurrentUtilization switch
        {
            >= 90 => "Critical",
            >= 75 => "High",
            >= 50 => "Medium",
            _ => "Low"
        };
    }
}
