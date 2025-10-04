using System.ComponentModel.DataAnnotations;

namespace InventoryManagement.Application.DTOs.Warehouse
{
    public class CreateWarehouseDto
    {
        [Required(ErrorMessage = "Warehouse code is required")]
        [StringLength(20, ErrorMessage = "Warehouse code cannot exceed 20 characters")]
        [RegularExpression(@"^[A-Z0-9\-]+$", ErrorMessage = "Warehouse code must contain only uppercase letters, numbers, and hyphens")]
        public string WarehouseCode { get; set; }

        [Required(ErrorMessage = "Warehouse name is required")]
        [StringLength(100, ErrorMessage = "Warehouse name cannot exceed 100 characters")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Warehouse type is required")]
        [StringLength(50, ErrorMessage = "Warehouse type cannot exceed 50 characters")]
        [RegularExpression(@"^(Main|Regional|Distribution|Outlet|Temporary)$",
            ErrorMessage = "Warehouse type must be one of: Main, Regional, Distribution, Outlet, Temporary")]
        public string WarehouseType { get; set; }

        [Required(ErrorMessage = "Address is required")]
        [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
        public string Address { get; set; }

        [Required(ErrorMessage = "City is required")]
        [StringLength(100, ErrorMessage = "City cannot exceed 100 characters")]
        public string City { get; set; }

        [Required(ErrorMessage = "State/Province is required")]
        [StringLength(50, ErrorMessage = "State/Province cannot exceed 50 characters")]
        public string State { get; set; }

        [Required(ErrorMessage = "Country is required")]
        [StringLength(2, MinimumLength = 2, ErrorMessage = "Country must be a 2-letter ISO code")]
        [RegularExpression(@"^[A-Z]{2}$", ErrorMessage = "Country must be a valid 2-letter ISO code")]
        public string Country { get; set; }

        [Required(ErrorMessage = "Postal code is required")]
        [StringLength(20, ErrorMessage = "Postal code cannot exceed 20 characters")]
        public string PostalCode { get; set; }

        [Phone(ErrorMessage = "Invalid phone number format")]
        [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
        public string Phone { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email address")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
        public string ManagerEmail { get; set; }

        [Required(ErrorMessage = "Maximum capacity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Maximum capacity must be greater than 0")]
        public int MaxCapacity { get; set; }

        // Optional fields for initial setup
        [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
        public string Notes { get; set; }

        // Operating hours (optional)
        public OperatingHoursDto OperatingHours { get; set; }

        // Warehouse capabilities (optional)
        public WarehouseCapabilitiesDto Capabilities { get; set; }
    }
}
