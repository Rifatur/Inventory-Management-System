using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagement.Application.DTOs.Inventory
{
    public class InventoryReservationDto
    {
        public long ReservationId { get; set; }
        public long InventoryId { get; set; }
        public long OrderId { get; set; }
        public string OrderNumber { get; set; }
        public int QuantityReserved { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime? ReleasedAt { get; set; }
    }
}
