namespace Authentication.Models
{
    public class Otp
    {
        public int Id { get; set; }

        public int OTP { get; set; }

        public int UserId { get; set; }

        public required string Token { get; set; }

        public bool IsUsed { get; set; } = false;

        public DateTime CreatedAt { get; set; }

        public DateTime ExpiresAt { get; set; }
    }
}
