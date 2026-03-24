using System.ComponentModel.DataAnnotations;

namespace CarComparisonApi.Models.DTOs
{
    public class RegisterRequest
    {
        [Required]
        [StringLength(20, MinimumLength = 3)]
        [RegularExpression("^[a-zA-Z0-9_]+$",
            ErrorMessage = "Логін може містити тільки латинські літери, цифри та знак підкреслення")]
        public string Login { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(8)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$",
            ErrorMessage = "Пароль має містити принаймні одну велику літеру, одну малу літеру та одну цифру")]
        public string Password { get; set; } = string.Empty;

        public string? RealName { get; set; }
    }
}