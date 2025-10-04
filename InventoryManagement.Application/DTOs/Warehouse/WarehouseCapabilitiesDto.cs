namespace InventoryManagement.Application.DTOs.Warehouse
{
    public class WarehouseCapabilitiesDto
    {
        public bool HasColdStorage { get; set; }
        public bool HasHazmatStorage { get; set; }
        public bool HasLoadingDock { get; set; }
        public int NumberOfLoadingDocks { get; set; }
        public bool Has24HourAccess { get; set; }
        public bool HasSecuritySystem { get; set; }
        public bool HasClimateControl { get; set; }
        public bool HasRackingSystem { get; set; }
        public bool HasForklifts { get; set; }
        public int NumberOfForklifts { get; set; }
        public bool HasPackingStation { get; set; }
        public bool HasShippingIntegration { get; set; }
    }
}
