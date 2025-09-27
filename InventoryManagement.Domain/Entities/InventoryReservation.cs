using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagement.Domain.Entities
{
    /// <summary>
    /// Tracks inventory reservations for orders
    /// </summary>
    [Table("InventoryReservations")]
    public class InventoryReservation
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long ReservationId { get; set; }

        /// <summary>
        /// Foreign key to InventoryItem
        /// </summary>
        public long InventoryId { get; set; }

        /// <summary>
        /// Foreign key to Order
        /// </summary>
        public long OrderId { get; set; }

        /// <summary>
        /// Quantity reserved
        /// </summary>
        public int QuantityReserved { get; set; }

        /// <summary>
        /// Reservation status (Active, Released, Expired)
        /// </summary>
        [Required]
        [StringLength(50)]
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpiresAt { get; set; }
        /// When the reservation was released or fulfilled
        public DateTime? ReleasedAt { get; set; }

        // Navigation properties
        public virtual InventoryItem InventoryItem { get; set; }
        public virtual Order Order { get; set; }
    }
}
