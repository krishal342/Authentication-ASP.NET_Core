using System.ComponentModel.DataAnnotations;

namespace Authentication.Models
{
    public class User
    {
        public int Id { get; set; }

        public required string FirstName { get; set; }

        public required string LastName { get; set; } 

        public required string Email { get; set; } 

        public required string Password { get; set; } 

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


    }
}
