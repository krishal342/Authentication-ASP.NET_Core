using System.ComponentModel.DataAnnotations;

namespace Authentication.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required] 
        [MinLength(3)]
        public string FirstName { get; set; } = null!;

        [Required] 
        [MinLength(3)]
        public string LastName { get; set; } = null!;

        [Required]
        public string Email { get; set; } = null!;

        public string Password { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


    }
}
