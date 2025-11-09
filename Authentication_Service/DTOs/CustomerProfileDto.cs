namespace backend_EAD.DTOs
{
    // =====================================================
    // Customer DTOs (for admin/internal use)
    // =====================================================
    public class CustomerDTO
    {
        public string UserID { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Role { get; set; } = "Customer";
        public DateTime CreatedAt { get; set; }
        public int LoyaltyPoints { get; set; }
        public string? Address { get; set; }
    }

    /// <summary>
    /// Update customer DTO - Only allows updating UserName, PhoneNumber, and Address
    /// Email, LoyaltyPoints are restricted for security and business logic
    /// </summary>
    public class UpdateCustomerDTO
    {
        public string? UserName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
    }

    // =====================================================
    // Profile DTOs (for customer self-service)
    // =====================================================

    /// <summary>
    /// Complete customer profile response including user info and vehicles
    /// </summary>
    public class CustomerProfileResponseDto
    {
        public string UserID { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public int LoyaltyPoints { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<VehicleDto> Vehicles { get; set; } = new();
    }

    /// <summary>
    /// Update customer profile (UserName, PhoneNumber, Address only)
    /// Loyalty points are read-only for customers
    /// </summary>
    public class UpdateCustomerProfileDto
    {
        public string? UserName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
    }

    /// <summary>
    /// Vehicle information DTO
    /// </summary>
    public class VehicleDto
    {
        public int VehicleID { get; set; }
        public string Model { get; set; } = string.Empty;
        public string Year { get; set; } = string.Empty;
        public string Vin { get; set; } = string.Empty;
        public string PlateNumber { get; set; } = string.Empty;
        public string? Company { get; set; }
    }

    /// <summary>
    /// Add new vehicle to customer profile
    /// VehicleID is auto-generated, CustomerID comes from authenticated user
    /// </summary>
    public class AddVehicleDto
    {
        public string Model { get; set; } = string.Empty;
        public string Year { get; set; } = string.Empty;
        public string Vin { get; set; } = string.Empty;
        public string PlateNumber { get; set; } = string.Empty;
        public string? Company { get; set; }
    }

    /// <summary>
    /// Add vehicle with CustomerID (for public API)
    /// </summary>
    public class AddVehicleWithCustomerDto
    {
        public string CustomerID { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string Year { get; set; } = string.Empty;
        public string Vin { get; set; } = string.Empty;
        public string PlateNumber { get; set; } = string.Empty;
        public string? Company { get; set; }
    }

    /// <summary>
    /// Update vehicle details
    /// </summary>
    public class UpdateVehicleDto
    {
        public string? Model { get; set; }
        public string? Year { get; set; }
        public string? PlateNumber { get; set; }
        public string? Company { get; set; }
    }
}

// =====================================================
// Customer DTOs (for admin/internal use)
// =====================================================
public class CustomerDTO
{
    public string UserID { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Role { get; set; } = "Customer";
    public DateTime CreatedAt { get; set; }
    public int LoyaltyPoints { get; set; }
    public string? Address { get; set; }
}

/// <summary>
/// Update customer DTO - Only allows updating UserName, PhoneNumber, and Address
/// Email, LoyaltyPoints are restricted for security and business logic
/// </summary>
public class UpdateCustomerDTO
{
    public string? UserName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
}

// =====================================================
// Vehicle DTOs (for public API)
// =====================================================

/// <summary>
/// Vehicle information DTO
/// </summary>
public class VehicleDto
{
    public int VehicleID { get; set; }
    public string Model { get; set; } = string.Empty;
    public string Year { get; set; } = string.Empty;
    public string Vin { get; set; } = string.Empty;
    public string PlateNumber { get; set; } = string.Empty;
    public string? Company { get; set; }
}

/// <summary>
/// Add vehicle with CustomerID (for public API)
/// </summary>
public class AddVehicleWithCustomerDto
{
    public string CustomerID { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string Year { get; set; } = string.Empty;
    public string Vin { get; set; } = string.Empty;
    public string PlateNumber { get; set; } = string.Empty;
    public string? Company { get; set; }
}
