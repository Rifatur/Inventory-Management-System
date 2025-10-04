namespace InventoryManagement.Application.DTOs.Warehouse
{
    public class TimeSlotDto
    {
        public bool IsOpen { get; set; }
        public string OpenTime { get; set; } // Format: "HH:mm"
        public string CloseTime { get; set; } // Format: "HH:mm"
    }
}
