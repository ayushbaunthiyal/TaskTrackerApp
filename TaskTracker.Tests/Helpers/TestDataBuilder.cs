using TaskTracker.Domain.Entities;

namespace TaskTracker.Tests.Helpers;

/// <summary>
/// Test data builder for creating test entities with proper defaults
/// </summary>
public static class TestDataBuilder
{
    public static User CreateUser(
        Guid? id = null,
        string email = "test@example.com",
        string firstName = "Test",
        string lastName = "User",
        string? passwordHash = null)
    {
        return new User
        {
            Id = id ?? Guid.NewGuid(),
            Email = email,
            PasswordHash = passwordHash ?? BCrypt.Net.BCrypt.HashPassword("Test123!@#"),
            FirstName = firstName,
            LastName = lastName,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsDeleted = false
        };
    }

    public static TaskItem CreateTask(
        Guid userId,
        Guid? id = null,
        string title = "Test Task",
        string description = "Test Description",
        Domain.Enums.TaskStatus status = Domain.Enums.TaskStatus.Pending,
        Domain.Enums.TaskPriority priority = Domain.Enums.TaskPriority.Medium)
    {
        return new TaskItem
        {
            Id = id ?? Guid.NewGuid(),
            UserId = userId,
            Title = title,
            Description = description,
            Status = status,
            Priority = priority,
            DueDate = DateTime.UtcNow.AddDays(7),
            Tags = new[] { "test" },
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsDeleted = false
        };
    }

    public static RefreshToken CreateRefreshToken(
        Guid userId,
        string token = "test-refresh-token",
        DateTime? expiresAt = null,
        DateTime? revokedAt = null)
    {
        return new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = token,
            ExpiresAt = expiresAt ?? DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow,
            RevokedAt = revokedAt
        };
    }
}
