namespace backend_EAD.Models
{
    public class RefreshToken
    {
        public int Id { get; set; }
        public required string Token { get; set; }
        public required string UserId { get; set; }
        public DateTime Expires { get; set; }
        public bool Revoked { get; set; }

        // Navigation property
        public AppUser? User { get; set; }
    }

}