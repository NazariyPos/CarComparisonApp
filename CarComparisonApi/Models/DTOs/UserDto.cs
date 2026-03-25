namespace CarComparisonApi.Models.DTOs
{
    /// <summary>
    /// Public user profile data returned by authentication endpoints.
    /// </summary>
    public class UserDto
    {
        public int Id { get; set; }
        public string Login { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? RealName { get; set; }
        public bool IsAdmin { get; set; }
        public string? About { get; set; }
        public string? AvatarUrl { get; set; }
    }

    /// <summary>
    /// Authentication response that contains JWT token and user info.
    /// </summary>
    public class AuthResponse
    {
        public string Token { get; set; } = string.Empty;
        public UserDto User { get; set; } = new();
    }
}
