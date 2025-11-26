namespace TaskTracker.RateLimitTester.Models;

public class ErrorResponse
{
    public string? Title { get; set; }
    public int Status { get; set; }
    public string? Detail { get; set; }
    public string? RetryAfter { get; set; }
}
