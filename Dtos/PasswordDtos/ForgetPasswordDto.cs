using System.ComponentModel.DataAnnotations;

namespace Authentication.Dtos.PasswordDtos
{
    public class ForgetPasswordDto
    {
        [Required (ErrorMessage = "Email is required!")]
        public string Email { get; set; } = null!;
    }
}
