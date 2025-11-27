using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace TaskTracker.Tests.Integration;

public class HealthCheckIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public HealthCheckIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task HealthCheck_ReturnsHealthyStatus()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        Assert.NotEmpty(content);
        
        // Should contain status and checks
        var json = await response.Content.ReadFromJsonAsync<HealthCheckResponse>();
        Assert.NotNull(json);
        Assert.Equal("Healthy", json.Status);
        Assert.NotNull(json.Checks);
        Assert.NotEmpty(json.Checks);
    }

    [Fact]
    public async Task HealthCheck_ContainsDatabaseCheck()
    {
        // Act
        var response = await _client.GetAsync("/health");
        var json = await response.Content.ReadFromJsonAsync<HealthCheckResponse>();

        // Assert
        Assert.NotNull(json);
        Assert.Contains(json.Checks, c => c.Name.Contains("database", StringComparison.OrdinalIgnoreCase));
        
        var dbCheck = json.Checks.First(c => c.Name.Contains("database", StringComparison.OrdinalIgnoreCase));
        Assert.Equal("Healthy", dbCheck.Status);
    }

    [Fact]
    public async Task HealthCheck_ContainsMemoryCheck()
    {
        // Act
        var response = await _client.GetAsync("/health");
        var json = await response.Content.ReadFromJsonAsync<HealthCheckResponse>();

        // Assert
        Assert.NotNull(json);
        Assert.Contains(json.Checks, c => c.Name.Contains("memory", StringComparison.OrdinalIgnoreCase));
        
        var memCheck = json.Checks.First(c => c.Name.Contains("memory", StringComparison.OrdinalIgnoreCase));
        Assert.Equal("Healthy", memCheck.Status);
    }

    [Fact]
    public async Task DatabaseHealthCheck_ReturnsHealthyStatus()
    {
        // Act
        var response = await _client.GetAsync("/health/db");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        // The /health/db endpoint returns plain text "Healthy" not JSON
        var content = await response.Content.ReadAsStringAsync();
        Assert.Equal("Healthy", content);
    }

    [Fact]
    public async Task HealthCheck_IncludesDurationInformation()
    {
        // Act
        var response = await _client.GetAsync("/health");
        var json = await response.Content.ReadFromJsonAsync<HealthCheckResponse>();

        // Assert
        Assert.NotNull(json);
        Assert.All(json.Checks, check =>
        {
            Assert.NotNull(check.Duration);
            Assert.True(check.Duration > 0);
        });
    }

    // Helper classes for JSON deserialization
    private class HealthCheckResponse
    {
        public string Status { get; set; } = string.Empty;
        public List<HealthCheckEntry> Checks { get; set; } = new();
    }

    private class HealthCheckEntry
    {
        public string Name { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public double? Duration { get; set; }
        public string? Description { get; set; }
    }
}
