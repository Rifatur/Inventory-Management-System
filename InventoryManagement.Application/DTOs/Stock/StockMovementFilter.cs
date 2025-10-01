namespace InventoryManagement.Application.DTOs.Stock
{
    public class StockMovementFilter
    {
        public long? ProductId { get; set; }
        public long? FromWarehouseId { get; set; }
        public long? ToWarehouseId { get; set; }
        public string MovementType { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string ReferenceNumber { get; set; }
        public string CreatedBy { get; set; }
        public string SortBy { get; set; } = "date";
        public bool SortDescending { get; set; } = true;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 50;
    }
}
