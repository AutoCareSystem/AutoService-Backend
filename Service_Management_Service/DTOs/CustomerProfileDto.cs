namespace Service_Management_Service.DTOs
{
    public class CustomerDTO
    {
        public string UserID { get; set; } = null!;
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string Role { get; set; } = "User";
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
