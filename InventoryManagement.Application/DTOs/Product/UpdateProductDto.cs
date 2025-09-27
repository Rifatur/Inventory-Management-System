using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagement.Application.DTOs.Product
{
    /// <summary>
    /// DTO for updating an existing product
    /// </summary>
    public class UpdateProductDto : CreateProductDto
    {
        public long ProductId { get; set; }
        public bool IsActive { get; set; }
    }
}
