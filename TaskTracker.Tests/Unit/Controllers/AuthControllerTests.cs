using Microsoft.AspNetCore.Mvc;
using TaskTracker.API.Controllers;
using TaskTracker.Application.DTOs;
using TaskTracker.Application.DTOs.Auth;
using TaskTracker.Application.Interfaces;

namespace TaskTracker.Tests.Unit.Controllers;

/// <summary>
/// Unit tests for AuthController
/// Tests: Register, Login, Refresh, Revoke endpoints with status codes and error handling
/// </summary>
public class AuthControllerTests
{
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _authServiceMock = new Mock<IAuthService>();
        _controller = new AuthController(_authServiceMock.Object);
    }

    [Fact]
    public async Task Register_WithValidData_ShouldReturn201Created()
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

        var response = new LoginResponseDto
        {
            AccessToken = "access-token",
            RefreshToken = "refresh-token",
            User = new UserDto
            {
                Id = Guid.NewGuid(),
                Email = registerDto.Email,
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName
            }
        };

        _authServiceMock.Setup(x => x.RegisterAsync(registerDto))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.Register(registerDto);

        // Assert
        result.Result.Should().BeOfType<ObjectResult>();
        var objectResult = result.Result as ObjectResult;
        objectResult!.StatusCode.Should().Be(201);
        objectResult.Value.Should().BeEquivalentTo(response);
    }

    [Fact]
    public async Task Register_WithExistingEmail_ShouldReturn400BadRequest()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Email = "existing@example.com",
            Password = "Test123!@#",
            ConfirmPassword = "Test123!@#",
            FirstName = "Test",
            LastName = "User"
        };

        _authServiceMock.Setup(x => x.RegisterAsync(registerDto))
            .ThrowsAsync(new InvalidOperationException("User with this email already exists"));

        // Act
        var result = await _controller.Register(registerDto);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result.Result as BadRequestObjectResult;
        badRequestResult!.Value.Should().BeEquivalentTo(new { error = "User with this email already exists" });
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturn200Ok()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Email = "user@example.com",
            Password = "Test123!@#"
        };

        var response = new LoginResponseDto
        {
            AccessToken = "access-token",
            RefreshToken = "refresh-token",
            User = new UserDto
            {
                Id = Guid.NewGuid(),
                Email = loginDto.Email,
                FirstName = "Test",
                LastName = "User"
            }
        };

        _authServiceMock.Setup(x => x.LoginAsync(loginDto))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.Login(loginDto);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(response);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ShouldReturn401Unauthorized()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Email = "user@example.com",
            Password = "WrongPassword"
        };

        _authServiceMock.Setup(x => x.LoginAsync(loginDto))
            .ThrowsAsync(new InvalidOperationException("Invalid email or password"));

        // Act
        var result = await _controller.Login(loginDto);

        // Assert
        result.Result.Should().BeOfType<UnauthorizedObjectResult>();
        var unauthorizedResult = result.Result as UnauthorizedObjectResult;
        unauthorizedResult!.Value.Should().BeEquivalentTo(new { error = "Invalid email or password" });
    }

    [Fact]
    public async Task Refresh_WithValidToken_ShouldReturn200Ok()
    {
        // Arrange
        var refreshDto = new RefreshTokenDto
        {
            RefreshToken = "valid-refresh-token"
        };

        var response = new LoginResponseDto
        {
            AccessToken = "new-access-token",
            RefreshToken = "new-refresh-token",
            User = new UserDto
            {
                Id = Guid.NewGuid(),
                Email = "user@example.com",
                FirstName = "Test",
                LastName = "User"
            }
        };

        _authServiceMock.Setup(x => x.RefreshTokenAsync(refreshDto.RefreshToken))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.RefreshToken(refreshDto);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(response);
    }

    [Fact]
    public async Task Refresh_WithInvalidToken_ShouldReturn401Unauthorized()
    {
        // Arrange
        var refreshDto = new RefreshTokenDto
        {
            RefreshToken = "invalid-token"
        };

        _authServiceMock.Setup(x => x.RefreshTokenAsync(refreshDto.RefreshToken))
            .ThrowsAsync(new InvalidOperationException("Invalid refresh token"));

        // Act
        var result = await _controller.RefreshToken(refreshDto);

        // Assert
        result.Result.Should().BeOfType<UnauthorizedObjectResult>();
        var unauthorizedResult = result.Result as UnauthorizedObjectResult;
        unauthorizedResult!.Value.Should().BeEquivalentTo(new { error = "Invalid refresh token" });
    }

    [Fact]
    public async Task Revoke_WithValidToken_ShouldReturn204NoContent()
    {
        // Arrange
        var revokeDto = new RefreshTokenDto
        {
            RefreshToken = "valid-token"
        };

        _authServiceMock.Setup(x => x.RevokeRefreshTokenAsync(revokeDto.RefreshToken))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.RevokeToken(revokeDto);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Revoke_WithInvalidToken_ShouldReturn400BadRequest()
    {
        // Arrange
        var revokeDto = new RefreshTokenDto
        {
            RefreshToken = "invalid-token"
        };

        _authServiceMock.Setup(x => x.RevokeRefreshTokenAsync(revokeDto.RefreshToken))
            .ThrowsAsync(new InvalidOperationException("Invalid refresh token"));

        // Act
        var result = await _controller.RevokeToken(revokeDto);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult!.Value.Should().BeEquivalentTo(new { error = "Invalid refresh token" });
    }

    [Fact]
    public async Task Register_ShouldCallAuthService()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Email = "test@example.com",
            Password = "Test123!@#",
            ConfirmPassword = "Test123!@#",
            FirstName = "Test",
            LastName = "User"
        };

        var response = new LoginResponseDto
        {
            AccessToken = "token",
            RefreshToken = "refresh",
            User = new UserDto { Id = Guid.NewGuid(), Email = registerDto.Email, FirstName = "Test", LastName = "User" }
        };

        _authServiceMock.Setup(x => x.RegisterAsync(registerDto))
            .ReturnsAsync(response);

        // Act
        await _controller.Register(registerDto);

        // Assert
        _authServiceMock.Verify(x => x.RegisterAsync(registerDto), Times.Once);
    }

    [Fact]
    public async Task Login_ShouldCallAuthService()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Email = "test@example.com",
            Password = "Test123!@#"
        };

        var response = new LoginResponseDto
        {
            AccessToken = "token",
            RefreshToken = "refresh",
            User = new UserDto { Id = Guid.NewGuid(), Email = loginDto.Email, FirstName = "Test", LastName = "User" }
        };

        _authServiceMock.Setup(x => x.LoginAsync(loginDto))
            .ReturnsAsync(response);

        // Act
        await _controller.Login(loginDto);

        // Assert
        _authServiceMock.Verify(x => x.LoginAsync(loginDto), Times.Once);
    }
}
