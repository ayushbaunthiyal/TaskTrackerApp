using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using TaskTracker.Infrastructure.Services;

namespace TaskTracker.Tests.Unit.Services;

/// <summary>
/// Unit tests for CurrentUserService
/// Tests: JWT Claims extraction, Authentication status
/// </summary>
public class CurrentUserServiceTests
{
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;

    public CurrentUserServiceTests()
    {
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
    }

    [Fact]
    public void UserId_WithAuthenticatedUser_ShouldReturnUserId()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Email, "test@example.com")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = principal };

        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        var service = new CurrentUserService(_httpContextAccessorMock.Object);

        // Act
        var result = service.UserId;

        // Assert
        result.Should().Be(userId);
    }

    [Fact]
    public void Email_WithAuthenticatedUser_ShouldReturnEmail()
    {
        // Arrange
        var email = "test@example.com";
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Email, email)
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = principal };

        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        var service = new CurrentUserService(_httpContextAccessorMock.Object);

        // Act
        var result = service.Email;

        // Assert
        result.Should().Be(email);
    }

    [Fact]
    public void IsAuthenticated_WithAuthenticatedUser_ShouldReturnTrue()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Email, "test@example.com")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = principal };

        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        var service = new CurrentUserService(_httpContextAccessorMock.Object);

        // Act
        var result = service.IsAuthenticated;

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsAuthenticated_WithUnauthenticatedUser_ShouldReturnFalse()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        var service = new CurrentUserService(_httpContextAccessorMock.Object);

        // Act
        var result = service.IsAuthenticated;

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void UserId_WithNoHttpContext_ShouldReturnEmptyGuid()
    {
        // Arrange
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext?)null);

        var service = new CurrentUserService(_httpContextAccessorMock.Object);

        // Act & Assert
        Assert.Throws<UnauthorizedAccessException>(() => service.UserId)
            .Message.Should().Be("User is not authenticated");
    }

    [Fact]
    public void Email_WithNoHttpContext_ShouldReturnEmptyString()
    {
        // Arrange
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext?)null);

        var service = new CurrentUserService(_httpContextAccessorMock.Object);

        // Act & Assert
        Assert.Throws<UnauthorizedAccessException>(() => service.Email)
            .Message.Should().Be("User email not found");
    }

    [Fact]
    public void UserId_WithMissingNameIdentifierClaim_ShouldReturnEmptyGuid()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Email, "test@example.com")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = principal };

        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        var service = new CurrentUserService(_httpContextAccessorMock.Object);

        // Act & Assert
        Assert.Throws<UnauthorizedAccessException>(() => service.UserId)
            .Message.Should().Be("User is not authenticated");
    }

    [Fact]
    public void Email_WithMissingEmailClaim_ShouldReturnEmptyString()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = principal };

        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        var service = new CurrentUserService(_httpContextAccessorMock.Object);

        // Act & Assert
        Assert.Throws<UnauthorizedAccessException>(() => service.Email)
            .Message.Should().Be("User email not found");
    }
}
