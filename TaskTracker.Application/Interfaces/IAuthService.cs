using TaskTracker.Application.DTOs;
using TaskTracker.Application.DTOs.Auth;

namespace TaskTracker.Application.Interfaces;

public interface IAuthService
{
    Task<LoginResponseDto> RegisterAsync(RegisterDto registerDto);
    Task<LoginResponseDto> LoginAsync(LoginDto loginDto);
    Task<LoginResponseDto> RefreshTokenAsync(string refreshToken);
    Task RevokeRefreshTokenAsync(string refreshToken);
    Task ChangePasswordAsync(Guid userId, ChangePasswordDto changePasswordDto);
}
