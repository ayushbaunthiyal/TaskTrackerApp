using TaskTracker.Application.DTOs;
using TaskTracker.Application.Interfaces;
using TaskTracker.Application.Interfaces.Repositories;
using TaskTracker.Application.Interfaces.Services;
using TaskTracker.Application.Services;
using TaskTracker.Domain.Entities;
using TaskTracker.Tests.Helpers;
using TaskStatus = TaskTracker.Domain.Enums.TaskStatus;
using TaskPriority = TaskTracker.Domain.Enums.TaskPriority;

namespace TaskTracker.Tests.Unit.Services;

/// <summary>
/// Unit tests for TaskService
/// Tests: CRUD operations, Ownership validation, Filtering, Pagination
/// </summary>
public class TaskServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IAuditService> _auditServiceMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<ITaskRepository> _taskRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly TaskService _taskService;
    private readonly Guid _currentUserId;

    public TaskServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _auditServiceMock = new Mock<IAuditService>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _taskRepositoryMock = new Mock<ITaskRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();

        _currentUserId = Guid.NewGuid();
        _currentUserServiceMock.Setup(x => x.UserId).Returns(_currentUserId);
        _currentUserServiceMock.Setup(x => x.Email).Returns("current@example.com");
        _currentUserServiceMock.Setup(x => x.IsAuthenticated).Returns(true);

        _unitOfWorkMock.Setup(u => u.Tasks).Returns(_taskRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.Users).Returns(_userRepositoryMock.Object);

        _taskService = new TaskService(
            _unitOfWorkMock.Object,
            _auditServiceMock.Object,
            _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task CreateTaskAsync_WithValidData_ShouldCreateTask()
    {
        // Arrange
        var user = TestDataBuilder.CreateUser(id: _currentUserId);
        var createDto = new CreateTaskDto
        {
            Title = "New Task",
            Description = "Task Description",
            Status = TaskStatus.Pending,
            Priority = TaskPriority.High,
            DueDate = DateTime.UtcNow.AddDays(7),
            Tags = new[] { "urgent" }
        };

        _userRepositoryMock.Setup(x => x.GetByIdAsync(_currentUserId))
            .ReturnsAsync(user);

        // Act
        var result = await _taskService.CreateTaskAsync(createDto);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be(createDto.Title);
        result.Description.Should().Be(createDto.Description);
        result.Status.Should().Be(createDto.Status);
        result.Priority.Should().Be(createDto.Priority);
        result.UserId.Should().Be(_currentUserId);

        _taskRepositoryMock.Verify(x => x.AddAsync(It.Is<TaskItem>(t =>
            t.UserId == _currentUserId &&
            t.Title == createDto.Title &&
            t.Description == createDto.Description
        )), Times.Once);

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        _auditServiceMock.Verify(x => x.LogActionAsync(
            _currentUserId,
            "Created",
            "TaskItem",
            It.IsAny<string>(),
            It.IsAny<string>()
        ), Times.Once);
    }

    [Fact]
    public async Task CreateTaskAsync_WithNonExistentUser_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var createDto = new CreateTaskDto
        {
            Title = "New Task",
            Description = "Description",
            Status = TaskStatus.Pending,
            Priority = TaskPriority.Medium
        };

        _userRepositoryMock.Setup(x => x.GetByIdAsync(_currentUserId))
            .ReturnsAsync((User?)null);

        // Act & Assert
        await _taskService.Invoking(s => s.CreateTaskAsync(createDto))
            .Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"User with ID {_currentUserId} not found");
    }

    [Fact]
    public async Task GetTaskByIdAsync_WithExistingTask_ShouldReturnTask()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var task = TestDataBuilder.CreateTask(_currentUserId, taskId);

        _taskRepositoryMock.Setup(x => x.GetByIdAsync(taskId))
            .ReturnsAsync(task);

        // Act
        var result = await _taskService.GetTaskByIdAsync(taskId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(taskId);
        result.UserId.Should().Be(_currentUserId);
    }

    [Fact]
    public async Task GetTaskByIdAsync_WithNonExistentTask_ShouldReturnNull()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        _taskRepositoryMock.Setup(x => x.GetByIdAsync(taskId))
            .ReturnsAsync((TaskItem?)null);

        // Act
        var result = await _taskService.GetTaskByIdAsync(taskId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateTaskAsync_WithOwnTask_ShouldUpdateSuccessfully()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var existingTask = TestDataBuilder.CreateTask(_currentUserId, taskId, title: "Old Title");
        var updateDto = new UpdateTaskDto
        {
            Title = "Updated Title",
            Description = "Updated Description",
            Status = TaskStatus.InProgress,
            Priority = TaskPriority.High,
            DueDate = DateTime.UtcNow.AddDays(14),
            Tags = new[] { "updated" }
        };

        _taskRepositoryMock.Setup(x => x.GetByIdAsync(taskId))
            .ReturnsAsync(existingTask);

        // Act
        var result = await _taskService.UpdateTaskAsync(taskId, updateDto);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be(updateDto.Title);
        result.Description.Should().Be(updateDto.Description);
        result.Status.Should().Be(updateDto.Status);

        _taskRepositoryMock.Verify(x => x.UpdateAsync(It.Is<TaskItem>(t =>
            t.Id == taskId &&
            t.Title == updateDto.Title
        )), Times.Once);

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        _auditServiceMock.Verify(x => x.LogActionAsync(
            _currentUserId,
            "Updated",
            "TaskItem",
            taskId.ToString(),
            It.IsAny<string>()
        ), Times.Once);
    }

    [Fact]
    public async Task UpdateTaskAsync_WithOtherUsersTask_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var otherUserId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var task = TestDataBuilder.CreateTask(otherUserId, taskId);
        var updateDto = new UpdateTaskDto
        {
            Title = "Hacked Title",
            Description = "Description",
            Status = TaskStatus.Completed,
            Priority = TaskPriority.Low
        };

        _taskRepositoryMock.Setup(x => x.GetByIdAsync(taskId))
            .ReturnsAsync(task);

        // Act & Assert
        await _taskService.Invoking(s => s.UpdateTaskAsync(taskId, updateDto))
            .Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("You can only modify your own tasks");

        _taskRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<TaskItem>()), Times.Never);
    }

    [Fact]
    public async Task UpdateTaskAsync_WithNonExistentTask_ShouldThrowKeyNotFoundException()
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

        _taskRepositoryMock.Setup(x => x.GetByIdAsync(taskId))
            .ReturnsAsync((TaskItem?)null);

        // Act & Assert
        await _taskService.Invoking(s => s.UpdateTaskAsync(taskId, updateDto))
            .Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Task with ID {taskId} not found");
    }

    [Fact]
    public async Task DeleteTaskAsync_WithOwnTask_ShouldDeleteSuccessfully()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var task = TestDataBuilder.CreateTask(_currentUserId, taskId);

        _taskRepositoryMock.Setup(x => x.GetByIdAsync(taskId))
            .ReturnsAsync(task);

        // Act
        var result = await _taskService.DeleteTaskAsync(taskId);

        // Assert
        result.Should().BeTrue();
        task.IsDeleted.Should().BeTrue();

        _taskRepositoryMock.Verify(x => x.UpdateAsync(It.Is<TaskItem>(t =>
            t.Id == taskId && t.IsDeleted
        )), Times.Once);

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        _auditServiceMock.Verify(x => x.LogActionAsync(
            _currentUserId,
            "Deleted",
            "TaskItem",
            taskId.ToString(),
            It.IsAny<string>()
        ), Times.Once);
    }

    [Fact]
    public async Task DeleteTaskAsync_WithOtherUsersTask_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var otherUserId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var task = TestDataBuilder.CreateTask(otherUserId, taskId);

        _taskRepositoryMock.Setup(x => x.GetByIdAsync(taskId))
            .ReturnsAsync(task);

        // Act & Assert
        await _taskService.Invoking(s => s.DeleteTaskAsync(taskId))
            .Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("You can only modify your own tasks");

        _taskRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<TaskItem>()), Times.Never);
    }

    [Fact]
    public async Task DeleteTaskAsync_WithNonExistentTask_ShouldReturnFalse()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        _taskRepositoryMock.Setup(x => x.GetByIdAsync(taskId))
            .ReturnsAsync((TaskItem?)null);

        // Act
        var result = await _taskService.DeleteTaskAsync(taskId);

        // Assert
        result.Should().BeFalse();
        _taskRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<TaskItem>()), Times.Never);
    }

    [Fact]
    public async Task GetFilteredTasksAsync_WithValidFilter_ShouldReturnPaginatedResults()
    {
        // Arrange
        var tasks = new List<TaskItem>
        {
            TestDataBuilder.CreateTask(_currentUserId, title: "Task 1"),
            TestDataBuilder.CreateTask(_currentUserId, title: "Task 2")
        };

        var filter = new TaskFilterDto
        {
            PageNumber = 1,
            PageSize = 10,
            SearchTerm = "Task"
        };

        _taskRepositoryMock.Setup(x => x.GetFilteredTasksAsync(
            It.IsAny<string>(),
            It.IsAny<int?>(),
            It.IsAny<int?>(),
            It.IsAny<string>(),
            It.IsAny<DateTime?>(),
            It.IsAny<DateTime?>(),
            It.IsAny<string>(),
            It.IsAny<bool>(),
            It.IsAny<int>(),
            It.IsAny<int>()
        )).ReturnsAsync((tasks, tasks.Count));

        // Act
        var result = await _taskService.GetFilteredTasksAsync(filter);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task GetFilteredTasksAsync_ShouldEnforceMaximumPageSize()
    {
        // Arrange
        var filter = new TaskFilterDto
        {
            PageNumber = 1,
            PageSize = 500 // Exceeds maximum of 100
        };

        _taskRepositoryMock.Setup(x => x.GetFilteredTasksAsync(
            It.IsAny<string>(),
            It.IsAny<int?>(),
            It.IsAny<int?>(),
            It.IsAny<string>(),
            It.IsAny<DateTime?>(),
            It.IsAny<DateTime?>(),
            It.IsAny<string>(),
            It.IsAny<bool>(),
            It.IsAny<int>(),
            100 // Should be capped at 100
        )).ReturnsAsync((new List<TaskItem>(), 0));

        // Act
        var result = await _taskService.GetFilteredTasksAsync(filter);

        // Assert
        _taskRepositoryMock.Verify(x => x.GetFilteredTasksAsync(
            It.IsAny<string>(),
            It.IsAny<int?>(),
            It.IsAny<int?>(),
            It.IsAny<string>(),
            It.IsAny<DateTime?>(),
            It.IsAny<DateTime?>(),
            It.IsAny<string>(),
            It.IsAny<bool>(),
            It.IsAny<int>(),
            100 // Verify page size was capped
        ), Times.Once);
    }
}
