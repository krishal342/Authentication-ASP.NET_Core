using System.Data;

namespace Authentication.Models
{
    public class RefreshToken
    {
        public int Id { get; set; }

        public required string Token { get; set; } 

        public int UserId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime ExpiresAt { get; set; }



        public bool IsExpired => DateTime.UtcNow > ExpiresAt;

        public bool IsActive =>  !IsExpired;
    }
}
