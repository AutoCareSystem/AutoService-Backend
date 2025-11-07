namespace backend_EAD.DTOs
{
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

    public class UpdateCustomerDTO
    {
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public int LoyaltyPoints { get; set; }
        public string? Address { get; set; }
    }
}
