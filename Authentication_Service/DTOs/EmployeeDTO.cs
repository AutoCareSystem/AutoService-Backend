namespace backend_EAD.DTOs
{
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
        public string Position { get; set; } = "Staff";
        public string EmpNo { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }

    public class UpdateEmployeeDTO
    {
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Position { get; set; }
        public string? EmpNo { get; set; }
        public bool IsActive { get; set; }
    }
}
