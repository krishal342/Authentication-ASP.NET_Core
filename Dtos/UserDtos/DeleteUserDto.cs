using System.ComponentModel.DataAnnotations;

namespace Authentication.Dtos.UserDtos
{
    public class DeleteUserDto
    {
        [Required(ErrorMessage = "Password is required!")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters long!")]
        public string Password { get; set; } = null!;
    }
}
