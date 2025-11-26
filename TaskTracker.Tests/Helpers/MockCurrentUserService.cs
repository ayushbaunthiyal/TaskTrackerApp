using TaskTracker.Application.Interfaces;

namespace TaskTracker.Tests.Helpers;

/// <summary>
/// Mock implementation of ICurrentUserService for testing
/// </summary>
public class MockCurrentUserService : ICurrentUserService
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public bool IsAuthenticated { get; set; } = true;

    public MockCurrentUserService(Guid userId, string email = "test@example.com")
    {
        UserId = userId;
        Email = email;
    }

    public static MockCurrentUserService CreateDefault()
    {
        return new MockCurrentUserService(Guid.NewGuid(), "test@example.com");
    }
}
