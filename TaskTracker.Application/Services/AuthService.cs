using Microsoft.Extensions.Configuration;
using TaskTracker.Application.DTOs.Auth;
using TaskTracker.Application.Interfaces;
using TaskTracker.Application.Interfaces.Repositories;
using TaskTracker.Domain.Entities;

namespace TaskTracker.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenService _tokenService;
    private readonly IConfiguration _configuration;

    public AuthService(IUnitOfWork unitOfWork, ITokenService tokenService, IConfiguration configuration)
    {
        _unitOfWork = unitOfWork;
        _tokenService = tokenService;
        _configuration = configuration;
    }

    public async Task<LoginResponseDto> RegisterAsync(RegisterDto registerDto)
    {
        // Check if user already exists
        var existingUser = (await _unitOfWork.Users.FindAsync(u => u.Email == registerDto.Email)).FirstOrDefault();
        if (existingUser != null)
        {
            throw new InvalidOperationException("User with this email already exists");
        }

        // Create new user
        var user = new User
        {
            Email = registerDto.Email,
            FirstName = registerDto.FirstName,
            LastName = registerDto.LastName,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password)
        };

        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        // Generate tokens
        return await GenerateLoginResponse(user);
    }

    public async Task<LoginResponseDto> LoginAsync(LoginDto loginDto)
    {
        // Find user by email
        var user = (await _unitOfWork.Users.FindAsync(u => u.Email == loginDto.Email)).FirstOrDefault();
        if (user == null)
        {
            throw new UnauthorizedAccessException("Invalid email or password");
        }

        // Verify password
        if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid email or password");
        }

        // Generate tokens
        return await GenerateLoginResponse(user);
    }

    public async Task<LoginResponseDto> RefreshTokenAsync(string refreshToken)
    {
        // Find the refresh token
        var tokenEntity = (await _unitOfWork.RefreshTokens.FindAsync(rt => rt.Token == refreshToken))
            .FirstOrDefault();

        if (tokenEntity == null || !tokenEntity.IsActive)
        {
            throw new UnauthorizedAccessException("Invalid or expired refresh token");
        }

        // Get the user
        var user = await _unitOfWork.Users.GetByIdAsync(tokenEntity.UserId);
        if (user == null)
        {
            throw new UnauthorizedAccessException("User not found");
        }

        // Revoke old refresh token
        tokenEntity.RevokedAt = DateTime.UtcNow;
        await _unitOfWork.RefreshTokens.UpdateAsync(tokenEntity);

        // Generate new tokens
        return await GenerateLoginResponse(user);
    }

    public async Task RevokeRefreshTokenAsync(string refreshToken)
    {
        var tokenEntity = (await _unitOfWork.RefreshTokens.FindAsync(rt => rt.Token == refreshToken))
            .FirstOrDefault();

        if (tokenEntity != null && tokenEntity.IsActive)
        {
            tokenEntity.RevokedAt = DateTime.UtcNow;
            await _unitOfWork.RefreshTokens.UpdateAsync(tokenEntity);
            await _unitOfWork.SaveChangesAsync();
        }
    }

    private async Task<LoginResponseDto> GenerateLoginResponse(User user)
    {
        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken();

        var jwtSettings = _configuration.GetSection("JwtSettings");
        var accessTokenExpiresInMinutes = int.Parse(jwtSettings["AccessTokenExpiresInMinutes"] ?? "60");
        var refreshTokenExpiresInDays = int.Parse(jwtSettings["RefreshTokenExpiresInDays"] ?? "7");

        // Store refresh token
        var refreshTokenEntity = new RefreshToken
        {
            UserId = user.Id,
            Token = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(refreshTokenExpiresInDays),
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.RefreshTokens.AddAsync(refreshTokenEntity);
        await _unitOfWork.SaveChangesAsync();

        return new LoginResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(accessTokenExpiresInMinutes),
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName
            }
        };
    }
}
