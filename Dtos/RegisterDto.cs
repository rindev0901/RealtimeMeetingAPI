using System.ComponentModel.DataAnnotations;

namespace RealtimeMeetingAPI.Dtos
{
    public class RegisterDto
    {
        [Required]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [StringLength(20, MinimumLength = 6)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [StringLength(20, MinimumLength = 8)]
        public string Password { get; set; } = string.Empty;
    }
}
