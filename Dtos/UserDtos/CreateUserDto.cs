
using System.ComponentModel.DataAnnotations;

namespace Authentication.Dtos.UserDtos
{
    public class CreateUserDto
    {
        [Required(ErrorMessage = "First Name is required!")]
        [MinLength(3, ErrorMessage = "First Name must be at least 3 characters long!")]
        public string FirstName { get; set; } = null!;

        [Required(ErrorMessage = "Last Name is required!")]
        [MinLength(3, ErrorMessage = "Last Name must be at least 3 characters long!")]
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
