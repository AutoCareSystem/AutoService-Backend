namespace backend_EAD.DTOs;

public class EmployeeDTO
{
    public string UserID { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Role { get; set; } = "Employee";
    public DateTime CreatedAt { get; set; }
    public string? Position { get; set; }
    public string? EmpNo { get; set; }
    public bool IsActive { get; set; }
}

public class CreateEmployeeDTO
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? Position { get; set; }
    // EmpNo is removed from input - it will be auto-generated
    // IsActive is removed from input - defaults to true
}

/// <summary>
/// Update employee DTO - Only allows updating UserName and PhoneNumber
/// Email, Position, EmpNo, and IsActive are restricted for security and business logic
/// </summary>
public class UpdateEmployeeDTO
{
    public string? UserName { get; set; }
    public string? PhoneNumber { get; set; }
}
