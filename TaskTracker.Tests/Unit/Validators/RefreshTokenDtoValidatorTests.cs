using FluentValidation.TestHelper;
using TaskTracker.Application.DTOs.Auth;
using TaskTracker.Application.Validators;

namespace TaskTracker.Tests.Unit.Validators;

/// <summary>
/// Unit tests for RefreshTokenDtoValidator
/// Tests: RefreshToken validation rules
/// </summary>
public class RefreshTokenDtoValidatorTests
{
    private readonly RefreshTokenDtoValidator _validator;

    public RefreshTokenDtoValidatorTests()
    {
        _validator = new RefreshTokenDtoValidator();
    }

    [Fact]
    public void Validate_WithValidToken_ShouldNotHaveErrors()
    {
        // Arrange
        var dto = new RefreshTokenDto
        {
            RefreshToken = "valid-refresh-token-value"
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_WithEmptyToken_ShouldHaveError(string token)
    {
        // Arrange
        var dto = new RefreshTokenDto
        {
            RefreshToken = token!
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.RefreshToken)
            .WithErrorMessage("Refresh token is required");
    }

    [Fact]
    public void Validate_WithWhitespaceToken_ShouldHaveError()
    {
        // Arrange
        var dto = new RefreshTokenDto
        {
            RefreshToken = "   "
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.RefreshToken)
            .WithErrorMessage("Refresh token is required");
    }
}
