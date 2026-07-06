using System.ComponentModel.DataAnnotations;

namespace Authentication.Dtos.PasswordDtos
{
    public class ResetPasswordDto
    {
        [Required(ErrorMessage = "Password is required!")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters long!")]
        //[RegularExpression(@"^(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&]).+$",
        //ErrorMessage = "Password must contain at least one uppercase letter, one number, and one special character!")]
        public string NewPassword { get; set; } = null!;
    }
}
