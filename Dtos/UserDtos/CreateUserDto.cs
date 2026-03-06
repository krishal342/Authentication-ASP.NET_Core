
using System.ComponentModel.DataAnnotations;

namespace Authentication.Dtos.UserDtos
{
    public class CreateUserDto
    {
        [Required(ErrorMessage = "First Name is required!")]
        [MaxLength(30, ErrorMessage = "First Name is maximum 30 character.")]
        public string FirstName { get; set; } = null!;

        [Required(ErrorMessage = "Last Name is required!")]
        [MaxLength(30, ErrorMessage = "Last Name is maximum 30 character.")]
        public string LastName { get; set; } = null!;

        [Required(ErrorMessage = "Email is required!")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Password is required!")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters long!")]
        //[RegularExpression(@"^(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&]).+$",
        //ErrorMessage = "Password must contain at least one uppercase letter, one number, and one special character!")]
        public string Password { get; set; } = null!;
    }
}
