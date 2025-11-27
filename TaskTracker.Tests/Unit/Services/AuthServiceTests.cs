using Microsoft.Extensions.Configuration;
using TaskTracker.Application.DTOs;
using TaskTracker.Application.DTOs.Auth;
using TaskTracker.Application.Interfaces;
using TaskTracker.Application.Interfaces.Repositories;
using TaskTracker.Application.Interfaces.Services;
using TaskTracker.Application.Services;
using TaskTracker.Domain.Entities;
using TaskTracker.Tests.Helpers;

namespace TaskTracker.Tests.Unit.Services;

/// <summary>
/// Unit tests for AuthService
/// Tests: Registration, Login, Token Refresh, Token Revocation
/// </summary>
public class AuthServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _tokenServiceMock = new Mock<ITokenService>();
        _configurationMock = new Mock<IConfiguration>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();

        _unitOfWorkMock.Setup(u => u.Users).Returns(_userRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.RefreshTokens).Returns(_refreshTokenRepositoryMock.Object);

        // Setup configuration
        var jwtSection = new Mock<IConfigurationSection>();
        jwtSection.Setup(x => x["RefreshTokenExpirationDays"]).Returns("7");
        _configurationMock.Setup(x => x.GetSection("JwtSettings")).Returns(jwtSection.Object);

        _authService = new AuthService(
            _unitOfWorkMock.Object,
            _tokenServiceMock.Object,
            _configurationMock.Object);
    }

    [Fact]
    public async Task RegisterAsync_WithValidData_ShouldCreateUserAndReturnTokens()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Email = "newuser@example.com",
            Password = "Test123!@#",
            ConfirmPassword = "Test123!@#",
            FirstName = "New",
            LastName = "User"
        };

        _userRepositoryMock.Setup(x => x.GetByEmailAsync(registerDto.Email))
            .ReturnsAsync((User?)null);

        _tokenServiceMock.Setup(x => x.GenerateAccessToken(It.IsAny<User>()))
            .Returns("access-token");

        _tokenServiceMock.Setup(x => x.GenerateRefreshToken())
            .Returns("refresh-token");

        // Act
        var result = await _authService.RegisterAsync(registerDto);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().Be("access-token");
        result.RefreshToken.Should().Be("refresh-token");
        result.User.Email.Should().Be(registerDto.Email);
        result.User.FirstName.Should().Be(registerDto.FirstName);
        result.User.LastName.Should().Be(registerDto.LastName);

        _userRepositoryMock.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Once);
        _refreshTokenRepositoryMock.Verify(x => x.AddAsync(It.IsAny<RefreshToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Exactly(2));
    }

    [Fact]
    public async Task RegisterAsync_WithExistingEmail_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var existingUser = TestDataBuilder.CreateUser(email: "existing@example.com");
        var registerDto = new RegisterDto
        {
            Email = "existing@example.com",
            Password = "Test123!@#",
            ConfirmPassword = "Test123!@#",
            FirstName = "Test",
            LastName = "User"
        };

        _userRepositoryMock.Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>()))
            .ReturnsAsync(new List<User> { existingUser });

        // Act & Assert
        await _authService.Invoking(s => s.RegisterAsync(registerDto))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("User with this email already exists");

        _userRepositoryMock.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ShouldReturnTokens()
    {
        // Arrange
        var user = TestDataBuilder.CreateUser(email: "user@example.com");
        var loginDto = new LoginDto
        {
            Email = "user@example.com",
            Password = "Test123!@#"
        };

        _userRepositoryMock.Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>()))
            .ReturnsAsync(new List<User> { user });

        _tokenServiceMock.Setup(x => x.GenerateAccessToken(user))
            .Returns("access-token");

        _tokenServiceMock.Setup(x => x.GenerateRefreshToken())
            .Returns("refresh-token");

        // Act
        var result = await _authService.LoginAsync(loginDto);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().Be("access-token");
        result.RefreshToken.Should().Be("refresh-token");
        result.User.Email.Should().Be(user.Email);

        _refreshTokenRepositoryMock.Verify(x => x.AddAsync(It.IsAny<RefreshToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_WithInvalidEmail_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Email = "nonexistent@example.com",
            Password = "Test123!@#"
        };

        _userRepositoryMock.Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>()))
            .ReturnsAsync(new List<User>());

        // Act & Assert
        await _authService.Invoking(s => s.LoginAsync(loginDto))
            .Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Invalid email or password");
    }

    [Fact]
    public async Task LoginAsync_WithInvalidPassword_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var user = TestDataBuilder.CreateUser(email: "user@example.com");
        var loginDto = new LoginDto
        {
            Email = "user@example.com",
            Password = "WrongPassword123"
        };

        _userRepositoryMock.Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>()))
            .ReturnsAsync(new List<User> { user });

        // Act & Assert
        await _authService.Invoking(s => s.LoginAsync(loginDto))
            .Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Invalid email or password");
    }

    [Fact]
    public async Task RefreshTokenAsync_WithValidToken_ShouldReturnNewTokens()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = TestDataBuilder.CreateUser(id: userId);
        var refreshTokenString = "valid-refresh-token";
        var refreshTokenEntity = TestDataBuilder.CreateRefreshToken(userId, refreshTokenString);

        _refreshTokenRepositoryMock.Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<RefreshToken, bool>>>()))
            .ReturnsAsync(new List<RefreshToken> { refreshTokenEntity });

        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(user);

        _tokenServiceMock.Setup(x => x.GenerateAccessToken(user))
            .Returns("new-access-token");

        _tokenServiceMock.Setup(x => x.GenerateRefreshToken())
            .Returns("new-refresh-token");

        // Act
        var result = await _authService.RefreshTokenAsync(refreshTokenString);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().Be("new-access-token");
        result.RefreshToken.Should().Be("new-refresh-token");

        _refreshTokenRepositoryMock.Verify(x => x.UpdateAsync(It.Is<RefreshToken>(rt => rt.RevokedAt != null)), Times.Once);
        _refreshTokenRepositoryMock.Verify(x => x.AddAsync(It.IsAny<RefreshToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Exactly(2));
    }

    [Fact]
    public async Task RefreshTokenAsync_WithInvalidToken_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var refreshDto = new RefreshTokenDto { RefreshToken = "invalid-token" };

        _refreshTokenRepositoryMock.Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<RefreshToken, bool>>>()))
            .ReturnsAsync(new List<RefreshToken>());

        // Act & Assert
        await _authService.Invoking(s => s.RefreshTokenAsync(refreshDto.RefreshToken))
            .Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Invalid or expired refresh token");
    }

    [Fact]
    public async Task RefreshTokenAsync_WithRevokedToken_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var refreshToken = TestDataBuilder.CreateRefreshToken(
            userId, 
            "revoked-token", 
            revokedAt: DateTime.UtcNow.AddHours(-1));

        var refreshDto = new RefreshTokenDto { RefreshToken = "revoked-token" };

        _refreshTokenRepositoryMock.Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<RefreshToken, bool>>>()))
            .ReturnsAsync(new List<RefreshToken> { refreshToken });

        // Act & Assert
        await _authService.Invoking(s => s.RefreshTokenAsync(refreshDto.RefreshToken))
            .Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Invalid or expired refresh token");
    }

    [Fact]
    public async Task RefreshTokenAsync_WithExpiredToken_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var refreshToken = TestDataBuilder.CreateRefreshToken(
            userId, 
            "expired-token", 
            expiresAt: DateTime.UtcNow.AddHours(-1));

        var refreshDto = new RefreshTokenDto { RefreshToken = "expired-token" };

        _refreshTokenRepositoryMock.Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<RefreshToken, bool>>>()))
            .ReturnsAsync(new List<RefreshToken> { refreshToken });

        // Act & Assert
        await _authService.Invoking(s => s.RefreshTokenAsync(refreshDto.RefreshToken))
            .Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Invalid or expired refresh token");
    }

    [Fact]
    public async Task RevokeRefreshTokenAsync_WithValidToken_ShouldRevokeToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var refreshToken = TestDataBuilder.CreateRefreshToken(userId, "valid-token");

        var revokeDto = new RefreshTokenDto { RefreshToken = "valid-token" };

        _refreshTokenRepositoryMock.Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<RefreshToken, bool>>>()))
            .ReturnsAsync(new List<RefreshToken> { refreshToken });

        // Act
        await _authService.RevokeRefreshTokenAsync(revokeDto.RefreshToken);

        // Assert
        refreshToken.RevokedAt.Should().NotBeNull();
        refreshToken.RevokedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        _refreshTokenRepositoryMock.Verify(x => x.UpdateAsync(refreshToken), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task RevokeRefreshTokenAsync_WithInvalidToken_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var revokeDto = new RefreshTokenDto { RefreshToken = "invalid-token" };

        _refreshTokenRepositoryMock.Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<RefreshToken, bool>>>()))
            .ReturnsAsync(new List<RefreshToken>());

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _authService.RevokeRefreshTokenAsync(revokeDto.RefreshToken)
        );
    }
}

