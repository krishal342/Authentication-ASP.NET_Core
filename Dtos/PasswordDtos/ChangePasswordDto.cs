using System.ComponentModel.DataAnnotations;

namespace Authentication.Dtos.PasswordDtos
{
    public class ChangePasswordDto
    {
        [Required(ErrorMessage = "Password is required!")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters long!")]
        public string CurrentPassword { get; set; } = null!;

        [Required(ErrorMessage = "Password is required!")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters long!")]
        public string NewPassword { get; set; } = null!;
    }
}
