using System.ComponentModel.DataAnnotations;

namespace CarComparisonApi.Models
{
    /// <summary>
    /// Represents a user review for a trim.
    /// </summary>
    public class Review
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }
        public int TrimId { get; set; }
        public Trim? Trim { get; set; }

        [Required]
        [StringLength(2000)]
        public string Content { get; set; } = string.Empty;

        [Range(1, 10)]
        public int Rating { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
