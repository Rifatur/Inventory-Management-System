namespace InventoryManagement.Application.DTOs.Warehouse
{
    public class OperatingHoursDto
    {
        public TimeSlotDto Monday { get; set; }
        public TimeSlotDto Tuesday { get; set; }
        public TimeSlotDto Wednesday { get; set; }
        public TimeSlotDto Thursday { get; set; }
        public TimeSlotDto Friday { get; set; }
        public TimeSlotDto Saturday { get; set; }
        public TimeSlotDto Sunday { get; set; }
        public string TimeZone { get; set; } = "UTC";
    }
}
