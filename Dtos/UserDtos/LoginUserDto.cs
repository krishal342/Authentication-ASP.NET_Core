using System.ComponentModel.DataAnnotations;

namespace Authentication.Dtos.UserDtos
{
    public record LoginUserDto
    {
        [Required(ErrorMessage = "Email is required!")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Password is required!")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters long!")]
        public string Password { get; set; } = null!;
    }
}
