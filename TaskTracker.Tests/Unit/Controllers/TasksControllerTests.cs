using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TaskTracker.API.Controllers;
using TaskTracker.Application.DTOs;
using TaskTracker.Application.Interfaces.Services;
using TaskStatus = TaskTracker.Domain.Enums.TaskStatus;
using TaskPriority = TaskTracker.Domain.Enums.TaskPriority;

namespace TaskTracker.Tests.Unit.Controllers;

/// <summary>
/// Unit tests for TasksController
/// Tests: CRUD endpoints, Authorization, Ownership validation, Error handling
/// </summary>
public class TasksControllerTests
{
    private readonly Mock<ITaskService> _taskServiceMock;
    private readonly Mock<ILogger<TasksController>> _loggerMock;
    private readonly TasksController _controller;

    public TasksControllerTests()
    {
        _taskServiceMock = new Mock<ITaskService>();
        _loggerMock = new Mock<ILogger<TasksController>>();
        _controller = new TasksController(_taskServiceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetTasks_WithValidFilter_ShouldReturn200Ok()
    {
        // Arrange
        var filter = new TaskFilterDto { PageNumber = 1, PageSize = 10 };
        var response = new PaginatedResponse<TaskDto>
        {
            Items = new List<TaskDto>
            {
                new TaskDto { Id = Guid.NewGuid(), Title = "Task 1", Description = "Description 1", Status = TaskStatus.Pending, Priority = TaskPriority.Medium, UserId = Guid.NewGuid() }
            },
            TotalCount = 1,
            PageNumber = 1,
            PageSize = 10
        };

        _taskServiceMock.Setup(x => x.GetFilteredTasksAsync(It.IsAny<TaskFilterDto>()))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.GetTasks(null, null, null, null, null, null, "CreatedAt", true, 1, 10);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(response);
    }

    [Fact]
    public async Task GetTaskById_WithExistingTask_ShouldReturn200Ok()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var taskDto = new TaskDto
        {
            Id = taskId,
            Title = "Test Task",
            Description = "Description",
            Status = TaskStatus.Pending,
            Priority = TaskPriority.Medium,
            UserId = Guid.NewGuid()
        };

        _taskServiceMock.Setup(x => x.GetTaskByIdAsync(taskId))
            .ReturnsAsync(taskDto);

        // Act
        var result = await _controller.GetTaskById(taskId);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(taskDto);
    }

    [Fact]
    public async Task GetTaskById_WithNonExistentTask_ShouldReturn404NotFound()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        _taskServiceMock.Setup(x => x.GetTaskByIdAsync(taskId))
            .ReturnsAsync((TaskDto?)null);

        // Act
        var result = await _controller.GetTaskById(taskId);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task CreateTask_WithValidData_ShouldReturn201Created()
    {
        // Arrange
        var createDto = new CreateTaskDto
        {
            Title = "New Task",
            Description = "Description",
            Status = TaskStatus.Pending,
            Priority = TaskPriority.High
        };

        var createdTask = new TaskDto
        {
            Id = Guid.NewGuid(),
            Title = createDto.Title,
            Description = createDto.Description,
            Status = createDto.Status,
            Priority = createDto.Priority,
            UserId = Guid.NewGuid()
        };

        _taskServiceMock.Setup(x => x.CreateTaskAsync(createDto))
            .ReturnsAsync(createdTask);

        // Act
        var result = await _controller.CreateTask(createDto);

        // Assert
        result.Result.Should().BeOfType<CreatedAtActionResult>();
        var createdResult = result.Result as CreatedAtActionResult;
        createdResult!.ActionName.Should().Be(nameof(TasksController.GetTaskById));
        createdResult.RouteValues!["id"].Should().Be(createdTask.Id);
        createdResult.Value.Should().BeEquivalentTo(createdTask);
    }

    [Fact]
    public async Task UpdateTask_WithOwnTask_ShouldReturn200Ok()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var updateDto = new UpdateTaskDto
        {
            Title = "Updated Task",
            Description = "Updated Description",
            Status = TaskStatus.InProgress,
            Priority = TaskPriority.High
        };

        var updatedTask = new TaskDto
        {
            Id = taskId,
            Title = updateDto.Title,
            Description = updateDto.Description,
            Status = updateDto.Status,
            Priority = updateDto.Priority,
            UserId = Guid.NewGuid()
        };

        _taskServiceMock.Setup(x => x.UpdateTaskAsync(taskId, updateDto))
            .ReturnsAsync(updatedTask);

        // Act
        var result = await _controller.UpdateTask(taskId, updateDto);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(updatedTask);
    }

    [Fact]
    public async Task UpdateTask_WithOtherUsersTask_ShouldReturn403Forbidden()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var updateDto = new UpdateTaskDto
        {
            Title = "Hacked Title",
            Description = "Description",
            Status = TaskStatus.Completed,
            Priority = TaskPriority.Low
        };

        _taskServiceMock.Setup(x => x.UpdateTaskAsync(taskId, updateDto))
            .ThrowsAsync(new UnauthorizedAccessException("You can only modify your own tasks"));

        // Act
        var result = await _controller.UpdateTask(taskId, updateDto);

        // Assert
        result.Result.Should().BeOfType<ObjectResult>();
        var objectResult = result.Result as ObjectResult;
        objectResult!.StatusCode.Should().Be(403);
        objectResult.Value.Should().BeEquivalentTo(new { error = "You can only modify your own tasks" });
    }

    [Fact]
    public async Task UpdateTask_WithNonExistentTask_ShouldReturn404NotFound()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var updateDto = new UpdateTaskDto
        {
            Title = "Title",
            Description = "Description",
            Status = TaskStatus.Completed,
            Priority = TaskPriority.Low
        };

        _taskServiceMock.Setup(x => x.UpdateTaskAsync(taskId, updateDto))
            .ThrowsAsync(new KeyNotFoundException($"Task with ID {taskId} not found"));

        // Act
        var result = await _controller.UpdateTask(taskId, updateDto);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = result.Result as NotFoundObjectResult;
        notFoundResult!.Value.Should().BeEquivalentTo(new { error = $"Task with ID {taskId} not found" });
    }

    [Fact]
    public async Task DeleteTask_WithOwnTask_ShouldReturn204NoContent()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        _taskServiceMock.Setup(x => x.DeleteTaskAsync(taskId))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteTask(taskId);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task DeleteTask_WithOtherUsersTask_ShouldReturn403Forbidden()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        _taskServiceMock.Setup(x => x.DeleteTaskAsync(taskId))
            .ThrowsAsync(new UnauthorizedAccessException("You can only modify your own tasks"));

        // Act
        var result = await _controller.DeleteTask(taskId);

        // Assert
        result.Should().BeOfType<ObjectResult>();
        var objectResult = result as ObjectResult;
        objectResult!.StatusCode.Should().Be(403);
        objectResult.Value.Should().BeEquivalentTo(new { error = "You can only modify your own tasks" });
    }

    [Fact]
    public async Task DeleteTask_WithNonExistentTask_ShouldReturn404NotFound()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        _taskServiceMock.Setup(x => x.DeleteTaskAsync(taskId))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.DeleteTask(taskId);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = result as NotFoundObjectResult;
        notFoundResult!.Value.Should().BeEquivalentTo(new { error = $"Task with ID {taskId} not found" });
    }

    [Fact]
    public async Task CreateTask_ShouldCallTaskService()
    {
        // Arrange
        var createDto = new CreateTaskDto
        {
            Title = "Task",
            Description = "Description",
            Status = TaskStatus.Pending,
            Priority = TaskPriority.Medium
        };

        var createdTask = new TaskDto
        {
            Id = Guid.NewGuid(),
            Title = createDto.Title,
            Description = createDto.Description,
            Status = createDto.Status,
            Priority = createDto.Priority,
            UserId = Guid.NewGuid()
        };

        _taskServiceMock.Setup(x => x.CreateTaskAsync(createDto))
            .ReturnsAsync(createdTask);

        // Act
        await _controller.CreateTask(createDto);

        // Assert
        _taskServiceMock.Verify(x => x.CreateTaskAsync(createDto), Times.Once);
    }

    [Fact]
    public async Task GetTasks_ShouldCallTaskService()
    {
        // Arrange
        var response = new PaginatedResponse<TaskDto>
        {
            Items = new List<TaskDto>(),
            TotalCount = 0,
            PageNumber = 1,
            PageSize = 10
        };

        _taskServiceMock.Setup(x => x.GetFilteredTasksAsync(It.IsAny<TaskFilterDto>()))
            .ReturnsAsync(response);

        // Act
        await _controller.GetTasks(null, null, null, null, null, null, "CreatedAt", true, 1, 10);

        // Assert
        _taskServiceMock.Verify(x => x.GetFilteredTasksAsync(It.Is<TaskFilterDto>(f => 
            f.PageNumber == 1 && 
            f.PageSize == 10 && 
            f.SortBy == "CreatedAt" && 
            f.SortDescending == true)), Times.Once);
    }
}
