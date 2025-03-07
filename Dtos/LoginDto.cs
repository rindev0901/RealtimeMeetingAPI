using System.ComponentModel.DataAnnotations;

namespace RealtimeMeetingAPI.Dtos
{
    public class LoginDto
    {
        [Required]
        [EmailAddress] // Ensures basic email validation
        [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "Invalid email format.")]
        public string UserName { get; set; }
        [Required]
        [StringLength(20, MinimumLength = 8)]
        public string Password { get; set; }
    }

    public class LoginSocialDto
    {
        public string Provider { get; set; }
        [Required]
        [EmailAddress] // Ensures basic email validation
        [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "Invalid email format.")]
        public string Email { get; set; }
        [Required]
        public string Name { get; set; }
        public string PhotoUrl { get; set; }
    }
}
