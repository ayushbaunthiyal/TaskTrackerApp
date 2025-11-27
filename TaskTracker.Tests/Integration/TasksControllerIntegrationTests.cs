using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using TaskTracker.Application.DTOs;
using TaskTracker.Application.DTOs.Auth;
using Xunit;
using TaskStatus = TaskTracker.Domain.Enums.TaskStatus;
using TaskPriority = TaskTracker.Domain.Enums.TaskPriority;

namespace TaskTracker.Tests.Integration;

public class TasksControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public TasksControllerIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<string> GetAccessTokenAsync()
    {
        var registerDto = new RegisterDto
        {
            Email = $"task_test_{Guid.NewGuid()}@example.com",
            Password = "Test123!@#",
            ConfirmPassword = "Test123!@#",
            FirstName = "Task",
            LastName = "Test"
        };

        var response = await _client.PostAsJsonAsync("/api/auth/register", registerDto);
        var authResponse = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
        return authResponse!.AccessToken;
    }

    [Fact]
    public async Task CreateTask_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var token = await GetAccessTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createDto = new CreateTaskDto
        {
            Title = "Integration Test Task",
            Description = "This is a test task",
            Status = TaskStatus.Pending,
            Priority = TaskPriority.Medium,
            DueDate = DateTime.UtcNow.AddDays(7),
            Tags = new[] { "test", "integration" }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/tasks", createDto);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        
        var result = await response.Content.ReadFromJsonAsync<TaskDto>();
        Assert.NotNull(result);
        Assert.Equal(createDto.Title, result.Title);
        Assert.Equal(createDto.Description, result.Description);
        Assert.Equal(createDto.Status, result.Status);
        Assert.Equal(createDto.Priority, result.Priority);
        Assert.NotEqual(Guid.Empty, result.Id);
    }

    [Fact]
    public async Task CreateTask_WithoutAuth_ReturnsUnauthorized()
    {
        // Arrange
        var createDto = new CreateTaskDto
        {
            Title = "Unauthorized Task",
            Status = TaskStatus.Pending,
            Priority = TaskPriority.Low
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/tasks", createDto);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetTaskById_ExistingTask_ReturnsTask()
    {
        // Arrange
        var token = await GetAccessTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Create a task first
        var createDto = new CreateTaskDto
        {
            Title = "Get Test Task",
            Status = TaskStatus.Pending,
            Priority = TaskPriority.High
        };
        var createResponse = await _client.PostAsJsonAsync("/api/tasks", createDto);
        var createdTask = await createResponse.Content.ReadFromJsonAsync<TaskDto>();

        // Act
        var response = await _client.GetAsync($"/api/tasks/{createdTask!.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var result = await response.Content.ReadFromJsonAsync<TaskDto>();
        Assert.NotNull(result);
        Assert.Equal(createdTask.Id, result.Id);
        Assert.Equal(createDto.Title, result.Title);
    }

    [Fact]
    public async Task GetTaskById_NonExistentTask_ReturnsNotFound()
    {
        // Arrange
        var token = await GetAccessTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync($"/api/tasks/{Guid.NewGuid()}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateTask_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var token = await GetAccessTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Create a task
        var createDto = new CreateTaskDto
        {
            Title = "Original Title",
            Status = TaskStatus.Pending,
            Priority = TaskPriority.Low
        };
        var createResponse = await _client.PostAsJsonAsync("/api/tasks", createDto);
        var createdTask = await createResponse.Content.ReadFromJsonAsync<TaskDto>();

        // Act - Update the task
        var updateDto = new UpdateTaskDto
        {
            Title = "Updated Title",
            Description = "Updated description",
            Status = TaskStatus.InProgress,
            Priority = TaskPriority.High,
            Tags = new[] { "updated" }
        };
        var response = await _client.PutAsJsonAsync($"/api/tasks/{createdTask!.Id}", updateDto);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var result = await response.Content.ReadFromJsonAsync<TaskDto>();
        Assert.NotNull(result);
        Assert.Equal(updateDto.Title, result.Title);
        Assert.Equal(updateDto.Description, result.Description);
        Assert.Equal(updateDto.Status, result.Status);
        Assert.Equal(updateDto.Priority, result.Priority);
    }

    [Fact]
    public async Task DeleteTask_ExistingTask_ReturnsNoContent()
    {
        // Arrange
        var token = await GetAccessTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Create a task
        var createDto = new CreateTaskDto
        {
            Title = "Task to Delete",
            Status = TaskStatus.Pending,
            Priority = TaskPriority.Low
        };
        var createResponse = await _client.PostAsJsonAsync("/api/tasks", createDto);
        var createdTask = await createResponse.Content.ReadFromJsonAsync<TaskDto>();

        // Act
        var response = await _client.DeleteAsync($"/api/tasks/{createdTask!.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Verify task is deleted
        var getResponse = await _client.GetAsync($"/api/tasks/{createdTask.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task GetFilteredTasks_WithFilters_ReturnsFilteredResults()
    {
        // Arrange
        var token = await GetAccessTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Create multiple tasks
        await _client.PostAsJsonAsync("/api/tasks", new CreateTaskDto
        {
            Title = "High Priority Task",
            Status = TaskStatus.Pending,
            Priority = TaskPriority.High
        });

        await _client.PostAsJsonAsync("/api/tasks", new CreateTaskDto
        {
            Title = "Medium Priority Task",
            Status = TaskStatus.InProgress,
            Priority = TaskPriority.Medium
        });

        // Act - Filter by priority
        var response = await _client.GetAsync("/api/tasks?Priority=High&PageNumber=1&PageSize=10");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var result = await response.Content.ReadFromJsonAsync<PaginatedResponse<TaskDto>>();
        Assert.NotNull(result);
        Assert.True(result.Items.Count > 0);
        Assert.All(result.Items, task => Assert.Equal(TaskPriority.High, task.Priority));
    }

    [Fact]
    public async Task CompleteTask_ChangesStatusToCompleted()
    {
        // Arrange
        var token = await GetAccessTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Create a task
        var createDto = new CreateTaskDto
        {
            Title = "Task to Complete",
            Status = TaskStatus.InProgress,
            Priority = TaskPriority.Medium
        };
        var createResponse = await _client.PostAsJsonAsync("/api/tasks", createDto);
        var createdTask = await createResponse.Content.ReadFromJsonAsync<TaskDto>();

        // Act - Mark as completed
        var updateDto = new UpdateTaskDto
        {
            Title = createdTask!.Title,
            Status = TaskStatus.Completed,
            Priority = createdTask.Priority
        };
        var response = await _client.PutAsJsonAsync($"/api/tasks/{createdTask.Id}", updateDto);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var result = await response.Content.ReadFromJsonAsync<TaskDto>();
        Assert.NotNull(result);
        Assert.Equal(TaskStatus.Completed, result.Status);
    }

    [Fact]
    public async Task GetTasks_Pagination_ReturnsCorrectPage()
    {
        // Arrange
        var token = await GetAccessTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Create 5 tasks
        for (int i = 1; i <= 5; i++)
        {
            await _client.PostAsJsonAsync("/api/tasks", new CreateTaskDto
            {
                Title = $"Pagination Test Task {i}",
                Status = TaskStatus.Pending,
                Priority = TaskPriority.Low
            });
        }

        // Act - Get page 1 with 2 items
        var response = await _client.GetAsync("/api/tasks?PageNumber=1&PageSize=2");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var result = await response.Content.ReadFromJsonAsync<PaginatedResponse<TaskDto>>();
        Assert.NotNull(result);
        Assert.Equal(2, result.Items.Count);
        Assert.Equal(1, result.PageNumber);
        Assert.Equal(2, result.PageSize);
        Assert.True(result.TotalCount >= 5);
    }
}
