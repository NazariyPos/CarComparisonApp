using CarComparisonApi.Models;
using CarComparisonApi.Models.DTOs;

namespace CarComparisonApi.Services
{
    public interface IAuthService
    {
        Task<AuthResponse> RegisterAsync(RegisterRequest request);
        Task<AuthResponse> LoginAsync(LoginRequest request);
        Task<User?> GetUserByIdAsync(int id);
    }
}
