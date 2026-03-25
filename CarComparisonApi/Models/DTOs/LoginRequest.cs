using System.ComponentModel.DataAnnotations;

namespace CarComparisonApi.Models.DTOs
{
    /// <summary>
    /// Request payload for user login.
    /// </summary>
    public class LoginRequest
    {
        [Required]
        public string LoginOrEmail { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }
}
