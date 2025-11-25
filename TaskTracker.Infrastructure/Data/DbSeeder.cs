using Microsoft.EntityFrameworkCore;
using TaskTracker.Domain.Entities;

namespace TaskTracker.Infrastructure.Data;

public class DbSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        // Check if database already has data
        if (await context.Users.AnyAsync())
        {
            return; // Database has been seeded
        }

        // Seed Users
        var users = new List<User>
        {
            new User
            {
                Id = Guid.NewGuid(),
                Email = "john.doe@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false
            },
            new User
            {
                Id = Guid.NewGuid(),
                Email = "jane.smith@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false
            },
            new User
            {
                Id = Guid.NewGuid(),
                Email = "bob.wilson@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false
            }
        };

        await context.Users.AddRangeAsync(users);
        await context.SaveChangesAsync();

        // Seed Tasks
        var tasks = new List<TaskItem>
        {
            new TaskItem
            {
                Id = Guid.NewGuid(),
                UserId = users[0].Id,
                Title = "Complete project documentation",
                Description = "Write comprehensive documentation for the Task Tracker API",
                Status = Domain.Enums.TaskStatus.InProgress,
                Priority = Domain.Enums.TaskPriority.High,
                DueDate = DateTime.UtcNow.AddDays(7),
                Tags = new[] { "documentation", "api", "priority" },
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                UpdatedAt = DateTime.UtcNow.AddDays(-2),
                IsDeleted = false
            },
            new TaskItem
            {
                Id = Guid.NewGuid(),
                UserId = users[0].Id,
                Title = "Fix authentication bug",
                Description = "Resolve the issue with JWT token expiration",
                Status = Domain.Enums.TaskStatus.Pending,
                Priority = Domain.Enums.TaskPriority.Critical,
                DueDate = DateTime.UtcNow.AddDays(2),
                Tags = new[] { "bug", "authentication", "urgent" },
                CreatedAt = DateTime.UtcNow.AddDays(-3),
                UpdatedAt = DateTime.UtcNow.AddDays(-3),
                IsDeleted = false
            },
            new TaskItem
            {
                Id = Guid.NewGuid(),
                UserId = users[1].Id,
                Title = "Design new landing page",
                Description = "Create mockups for the new landing page design",
                Status = Domain.Enums.TaskStatus.Completed,
                Priority = Domain.Enums.TaskPriority.Medium,
                DueDate = DateTime.UtcNow.AddDays(-1),
                Tags = new[] { "design", "ui", "frontend" },
                CreatedAt = DateTime.UtcNow.AddDays(-10),
                UpdatedAt = DateTime.UtcNow.AddDays(-1),
                IsDeleted = false
            },
            new TaskItem
            {
                Id = Guid.NewGuid(),
                UserId = users[1].Id,
                Title = "Update database schema",
                Description = "Add new columns for user preferences",
                Status = Domain.Enums.TaskStatus.InProgress,
                Priority = Domain.Enums.TaskPriority.Medium,
                DueDate = DateTime.UtcNow.AddDays(5),
                Tags = new[] { "database", "schema", "backend" },
                CreatedAt = DateTime.UtcNow.AddDays(-4),
                UpdatedAt = DateTime.UtcNow.AddDays(-1),
                IsDeleted = false
            },
            new TaskItem
            {
                Id = Guid.NewGuid(),
                UserId = users[2].Id,
                Title = "Setup CI/CD pipeline",
                Description = "Configure automated deployment pipeline",
                Status = Domain.Enums.TaskStatus.Pending,
                Priority = Domain.Enums.TaskPriority.High,
                DueDate = DateTime.UtcNow.AddDays(10),
                Tags = new[] { "devops", "ci-cd", "automation" },
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                UpdatedAt = DateTime.UtcNow.AddDays(-2),
                IsDeleted = false
            },
            new TaskItem
            {
                Id = Guid.NewGuid(),
                UserId = users[2].Id,
                Title = "Code review for PR #123",
                Description = "Review the pull request for the new feature",
                Status = Domain.Enums.TaskStatus.Completed,
                Priority = Domain.Enums.TaskPriority.Low,
                DueDate = DateTime.UtcNow.AddDays(-2),
                Tags = new[] { "review", "code-quality" },
                CreatedAt = DateTime.UtcNow.AddDays(-6),
                UpdatedAt = DateTime.UtcNow.AddDays(-3),
                IsDeleted = false
            },
            new TaskItem
            {
                Id = Guid.NewGuid(),
                UserId = users[0].Id,
                Title = "Implement rate limiting",
                Description = "Add rate limiting middleware to the API",
                Status = Domain.Enums.TaskStatus.Pending,
                Priority = Domain.Enums.TaskPriority.Medium,
                DueDate = DateTime.UtcNow.AddDays(14),
                Tags = new[] { "security", "api", "performance" },
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow.AddDays(-1),
                IsDeleted = false
            },
            new TaskItem
            {
                Id = Guid.NewGuid(),
                UserId = users[1].Id,
                Title = "Performance optimization",
                Description = "Optimize database queries for better performance",
                Status = Domain.Enums.TaskStatus.InProgress,
                Priority = Domain.Enums.TaskPriority.High,
                DueDate = DateTime.UtcNow.AddDays(8),
                Tags = new[] { "performance", "optimization", "database" },
                CreatedAt = DateTime.UtcNow.AddDays(-7),
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false
            },
            new TaskItem
            {
                Id = Guid.NewGuid(),
                UserId = users[2].Id,
                Title = "Write unit tests",
                Description = "Increase test coverage to 80%",
                Status = Domain.Enums.TaskStatus.Pending,
                Priority = Domain.Enums.TaskPriority.Medium,
                DueDate = DateTime.UtcNow.AddDays(12),
                Tags = new[] { "testing", "quality", "coverage" },
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow.AddDays(-1),
                IsDeleted = false
            },
            new TaskItem
            {
                Id = Guid.NewGuid(),
                UserId = users[0].Id,
                Title = "Update dependencies",
                Description = "Update all NuGet packages to latest versions",
                Status = Domain.Enums.TaskStatus.Cancelled,
                Priority = Domain.Enums.TaskPriority.Low,
                DueDate = DateTime.UtcNow.AddDays(20),
                Tags = new[] { "maintenance", "dependencies" },
                CreatedAt = DateTime.UtcNow.AddDays(-8),
                UpdatedAt = DateTime.UtcNow.AddDays(-4),
                IsDeleted = false
            }
        };

        await context.Tasks.AddRangeAsync(tasks);
        await context.SaveChangesAsync();

        // Seed some Attachments
        var attachments = new List<Attachment>
        {
            new Attachment
            {
                Id = Guid.NewGuid(),
                TaskId = tasks[0].Id,
                FileName = "api-documentation.pdf",
                FileSize = 1024000,
                FilePath = "/uploads/api-documentation.pdf",
                UploadedAt = DateTime.UtcNow.AddDays(-3),
                CreatedAt = DateTime.UtcNow.AddDays(-3),
                UpdatedAt = DateTime.UtcNow.AddDays(-3),
                IsDeleted = false
            },
            new Attachment
            {
                Id = Guid.NewGuid(),
                TaskId = tasks[2].Id,
                FileName = "landing-page-mockup.png",
                FileSize = 2048000,
                FilePath = "/uploads/landing-page-mockup.png",
                UploadedAt = DateTime.UtcNow.AddDays(-5),
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                UpdatedAt = DateTime.UtcNow.AddDays(-5),
                IsDeleted = false
            },
            new Attachment
            {
                Id = Guid.NewGuid(),
                TaskId = tasks[7].Id,
                FileName = "performance-report.xlsx",
                FileSize = 512000,
                FilePath = "/uploads/performance-report.xlsx",
                UploadedAt = DateTime.UtcNow.AddDays(-2),
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                UpdatedAt = DateTime.UtcNow.AddDays(-2),
                IsDeleted = false
            }
        };

        await context.Attachments.AddRangeAsync(attachments);
        await context.SaveChangesAsync();
    }
}
