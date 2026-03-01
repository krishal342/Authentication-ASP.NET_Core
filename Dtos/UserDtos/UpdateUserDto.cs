using System.ComponentModel.DataAnnotations;

namespace Authentication.Dtos.UserDtos
{
    public class UpdateUserDto
    {
        [MinLength(3, ErrorMessage = "First Name must be at least 3 characters long!")]
        public string? FirstName { get; set; }

        [MinLength(3, ErrorMessage = "Last Name must be at least 3 characters long!")]
        public string? LastName { get; set; }

        public string? Email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; } = null!;

    }
}
