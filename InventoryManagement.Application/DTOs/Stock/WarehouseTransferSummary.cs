namespace InventoryManagement.Application.DTOs.Stock
{
    public class WarehouseTransferSummary
    {
        public long FromWarehouseId { get; set; }
        public string FromWarehouseName { get; set; }
        public long ToWarehouseId { get; set; }
        public string ToWarehouseName { get; set; }
        public int TransferCount { get; set; }
        public int TotalQuantity { get; set; }
        public decimal TotalValue { get; set; }
    }
}
