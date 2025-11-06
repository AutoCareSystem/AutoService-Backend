namespace Authentication_Service.DTOs
{
    public class UpdateProfileDto
    {
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Role { get; set; }
    }
}
