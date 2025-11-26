using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using TaskTracker.Domain.Entities;
using TaskTracker.Infrastructure.Services;
using TaskTracker.Tests.Helpers;

namespace TaskTracker.Tests.Unit.Services;

/// <summary>
/// Unit tests for TokenService
/// Tests: Access Token Generation, Refresh Token Generation, Token Validation
/// </summary>
public class TokenServiceTests
{
    private readonly TokenService _tokenService;
    private readonly string _testSecret = "VGhpc0lzQVZlcnlTZWNyZXRLZXlGb3JUZXN0aW5nUHVycG9zZXNPbmx5MTIzNDU2Nzg5MA==";
    private readonly string _testIssuer = "TestIssuer";
    private readonly string _testAudience = "TestAudience";

    public TokenServiceTests()
    {
        var configurationMock = new Mock<IConfiguration>();
        var jwtSection = new Mock<IConfigurationSection>();
        
        jwtSection.Setup(x => x["Secret"]).Returns(_testSecret);
        jwtSection.Setup(x => x["Issuer"]).Returns(_testIssuer);
        jwtSection.Setup(x => x["Audience"]).Returns(_testAudience);
        jwtSection.Setup(x => x["AccessTokenExpirationMinutes"]).Returns("60");
        
        configurationMock.Setup(x => x.GetSection("JwtSettings")).Returns(jwtSection.Object);

        _tokenService = new TokenService(configurationMock.Object);
    }

    [Fact]
    public void GenerateAccessToken_WithValidUser_ShouldReturnValidJwt()
    {
        // Arrange
        var user = TestDataBuilder.CreateUser(
            id: Guid.NewGuid(),
            email: "test@example.com",
            firstName: "Test",
            lastName: "User");

        // Act
        var token = _tokenService.GenerateAccessToken(user);

        // Assert
        token.Should().NotBeNullOrEmpty();

        // Decode and verify token
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        jwtToken.Subject.Should().Be(user.Id.ToString());
        jwtToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Email && c.Value == user.Email);
        jwtToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Jti);
        jwtToken.Issuer.Should().Be(_testIssuer);
        jwtToken.Audiences.Should().Contain(_testAudience);
        jwtToken.ValidTo.Should().BeCloseTo(DateTime.UtcNow.AddMinutes(60), TimeSpan.FromMinutes(1));
    }

    [Fact]
    public void GenerateAccessToken_ShouldIncludeUserClaims()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = TestDataBuilder.CreateUser(
            id: userId,
            email: "user@example.com");

        // Act
        var token = _tokenService.GenerateAccessToken(user);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        var claims = jwtToken.Claims.ToList();
        claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == userId.ToString());
        claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Email && c.Value == "user@example.com");
    }

    [Fact]
    public void GenerateRefreshToken_ShouldReturnUniqueToken()
    {
        // Act
        var token1 = _tokenService.GenerateRefreshToken();
        var token2 = _tokenService.GenerateRefreshToken();

        // Assert
        token1.Should().NotBeNullOrEmpty();
        token2.Should().NotBeNullOrEmpty();
        token1.Should().NotBe(token2);
        token1.Length.Should().BeGreaterThan(40); // Base64 encoded random bytes
    }

    [Fact]
    public void ValidateAccessToken_WithValidToken_ShouldReturnUserId()
    {
        // Arrange
        var user = TestDataBuilder.CreateUser();
        var token = _tokenService.GenerateAccessToken(user);

        // Act
        var userId = _tokenService.ValidateAccessToken(token);

        // Assert
        userId.Should().NotBeNull();
        userId.Should().Be(user.Id);
    }

    [Fact]
    public void ValidateAccessToken_WithInvalidToken_ShouldReturnNull()
    {
        // Arrange
        var invalidToken = "invalid.token.here";

        // Act
        var userId = _tokenService.ValidateAccessToken(invalidToken);

        // Assert
        userId.Should().BeNull();
    }

    [Fact]
    public void ValidateAccessToken_WithEmptyToken_ShouldReturnNull()
    {
        // Arrange
        var emptyToken = string.Empty;

        // Act
        var userId = _tokenService.ValidateAccessToken(emptyToken);

        // Assert
        userId.Should().BeNull();
    }

    [Fact]
    public void GenerateAccessToken_MultipleCalls_ShouldHaveUniqueJti()
    {
        // Arrange
        var user = TestDataBuilder.CreateUser();

        // Act
        var token1 = _tokenService.GenerateAccessToken(user);
        var token2 = _tokenService.GenerateAccessToken(user);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken1 = handler.ReadJwtToken(token1);
        var jwtToken2 = handler.ReadJwtToken(token2);

        var jti1 = jwtToken1.Claims.First(c => c.Type == JwtRegisteredClaimNames.Jti).Value;
        var jti2 = jwtToken2.Claims.First(c => c.Type == JwtRegisteredClaimNames.Jti).Value;

        jti1.Should().NotBe(jti2);
    }
}
