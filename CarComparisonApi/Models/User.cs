using System.ComponentModel.DataAnnotations;

namespace CarComparisonApi.Models
{
    /// <summary>
    /// Represents an application user.
    /// </summary>
    public class User
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Login { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;

        public bool IsAdmin { get; set; } = false;

        [StringLength(100)]
        public string? RealName { get; set; }

        [StringLength(500)]
        public string? About { get; set; }

        public string? AvatarUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLogin { get; set; }

        public List<Review> Reviews { get; set; } = new();
        public List<Favorite> Favorites { get; set; } = new();
    }
}
