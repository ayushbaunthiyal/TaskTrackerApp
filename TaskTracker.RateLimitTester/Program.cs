using Spectre.Console;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using TaskTracker.RateLimitTester.Models;

namespace TaskTracker.RateLimitTester;

class Program
{
    private static readonly string ApiBaseUrl = "http://localhost:5128";
    private static readonly HttpClient HttpClient = new();

    static async Task Main(string[] args)
    {
        AnsiConsole.Clear();
        
        AnsiConsole.Write(
            new FigletText("Rate Limit Tester")
                .LeftJustified()
                .Color(Color.Blue));

        AnsiConsole.MarkupLine($"[grey]API Base URL: {ApiBaseUrl}[/]\n");

        while (true)
        {
            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[yellow]Select a test to run:[/]")
                    .AddChoices(new[]
                    {
                        "Test Per-User Rate Limiting (100 req/min)",
                        "Test Per-IP Auth Rate Limiting (20 req/15min)",
                        "Test Per-IP Strict Rate Limiting (10 req/min)",
                        "Run All Tests",
                        "Exit"
                    }));

            AnsiConsole.WriteLine();

            switch (choice)
            {
                case "Test Per-User Rate Limiting (100 req/min)":
                    await TestPerUserRateLimit();
                    break;
                case "Test Per-IP Auth Rate Limiting (20 req/15min)":
                    await TestPerIpAuthRateLimit();
                    break;
                case "Test Per-IP Strict Rate Limiting (10 req/min)":
                    await TestPerIpStrictRateLimit();
                    break;
                case "Run All Tests":
                    await TestPerUserRateLimit();
                    AnsiConsole.WriteLine();
                    await TestPerIpAuthRateLimit();
                    AnsiConsole.WriteLine();
                    await TestPerIpStrictRateLimit();
                    break;
                case "Exit":
                    return;
            }

            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[grey]Press any key to continue...[/]");
            Console.ReadKey();
            AnsiConsole.Clear();
        }
    }

    static async Task TestPerUserRateLimit()
    {
        AnsiConsole.Write(new Rule("[blue]Testing Per-User Rate Limiting[/]"));
        AnsiConsole.WriteLine();

        // Login first to get token
        AnsiConsole.Status()
            .Start("Logging in as test user...", ctx =>
            {
                ctx.Spinner(Spinner.Known.Dots);
            });

        var loginResponse = await HttpClient.PostAsJsonAsync(
            $"{ApiBaseUrl}/api/Auth/login",
            new { email = "john.doe@example.com", password = "Password123!" });

        if (!loginResponse.IsSuccessStatusCode)
        {
            AnsiConsole.MarkupLine("[red]✗ Login failed![/]");
            return;
        }

        var authResult = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();
        var token = authResult?.AccessToken;

        AnsiConsole.MarkupLine($"[green]✓ Login successful![/]\n");

        // Test rate limit
        const int totalRequests = 105;
        var successCount = 0;
        var rateLimitedCount = 0;
        var rateLimitHitAt = 0;
        var responseTimes = new List<long>();
        string? retryAfter = null;

        await AnsiConsole.Progress()
            .Columns(new ProgressColumn[]
            {
                new TaskDescriptionColumn(),
                new ProgressBarColumn(),
                new PercentageColumn(),
                new SpinnerColumn(),
            })
            .StartAsync(async ctx =>
            {
                var task = ctx.AddTask($"[blue]Sending {totalRequests} requests to /api/Tasks...[/]", maxValue: totalRequests);

                HttpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                for (int i = 1; i <= totalRequests; i++)
                {
                    var sw = Stopwatch.StartNew();
                    var response = await HttpClient.GetAsync($"{ApiBaseUrl}/api/Tasks?pageSize=5");
                    sw.Stop();

                    responseTimes.Add(sw.ElapsedMilliseconds);

                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        successCount++;
                    }
                    else if (response.StatusCode == HttpStatusCode.TooManyRequests)
                    {
                        rateLimitedCount++;
                        if (rateLimitHitAt == 0)
                        {
                            rateLimitHitAt = i;
                        }

                        var errorContent = await response.Content.ReadFromJsonAsync<ErrorResponse>();
                        retryAfter = errorContent?.RetryAfter;
                    }

                    task.Increment(1);
                    await Task.Delay(10); // Small delay to avoid overwhelming the API
                }
            });

        AnsiConsole.WriteLine();

        // Display results
        var table = new Table();
        table.Border(TableBorder.Rounded);
        table.AddColumn("Metric");
        table.AddColumn("Value");

        table.AddRow("Total Requests", totalRequests.ToString());
        table.AddRow("Successful (200)", $"[green]{successCount}[/]");
        table.AddRow("Rate Limited (429)", rateLimitedCount > 0 ? $"[red]{rateLimitedCount}[/]" : "0");
        table.AddRow("Rate Limit Hit At", rateLimitHitAt > 0 ? $"[yellow]Request #{rateLimitHitAt}[/]" : "N/A");
        table.AddRow("Retry After", retryAfter ?? "N/A");
        table.AddRow("Avg Response Time", $"{(int)responseTimes.Average()}ms");

        AnsiConsole.Write(table);

        if (rateLimitHitAt > 95 && rateLimitHitAt <= 105)
        {
            AnsiConsole.MarkupLine("\n[green]✓ Rate limiting working as expected![/]");
        }
        else
        {
            AnsiConsole.MarkupLine("\n[red]✗ Rate limiting not working as expected![/]");
        }
    }

    static async Task TestPerIpAuthRateLimit()
    {
        AnsiConsole.Write(new Rule("[blue]Testing Per-IP Auth Rate Limiting[/]"));
        AnsiConsole.WriteLine();

        const int totalRequests = 25;
        var successCount = 0;
        var rateLimitedCount = 0;
        var rateLimitHitAt = 0;
        var responseTimes = new List<long>();
        string? retryAfter = null;

        await AnsiConsole.Progress()
            .Columns(new ProgressColumn[]
            {
                new TaskDescriptionColumn(),
                new ProgressBarColumn(),
                new PercentageColumn(),
                new SpinnerColumn(),
            })
            .StartAsync(async ctx =>
            {
                var task = ctx.AddTask($"[blue]Sending {totalRequests} login requests...[/]", maxValue: totalRequests);

                for (int i = 1; i <= totalRequests; i++)
                {
                    var sw = Stopwatch.StartNew();
                    var response = await HttpClient.PostAsJsonAsync(
                        $"{ApiBaseUrl}/api/Auth/login",
                        new { email = "invalid@example.com", password = "wrongpassword" });
                    sw.Stop();

                    responseTimes.Add(sw.ElapsedMilliseconds);

                    if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.BadRequest)
                    {
                        successCount++; // Successful request (not rate limited)
                    }
                    else if (response.StatusCode == HttpStatusCode.TooManyRequests)
                    {
                        rateLimitedCount++;
                        if (rateLimitHitAt == 0)
                        {
                            rateLimitHitAt = i;
                        }

                        var errorContent = await response.Content.ReadFromJsonAsync<ErrorResponse>();
                        retryAfter = errorContent?.RetryAfter;
                    }

                    task.Increment(1);
                    await Task.Delay(100); // Small delay
                }
            });

        AnsiConsole.WriteLine();

        // Display results
        var table = new Table();
        table.Border(TableBorder.Rounded);
        table.AddColumn("Metric");
        table.AddColumn("Value");

        table.AddRow("Total Requests", totalRequests.ToString());
        table.AddRow("Successful (401/400)", $"[green]{successCount}[/]");
        table.AddRow("Rate Limited (429)", rateLimitedCount > 0 ? $"[red]{rateLimitedCount}[/]" : "0");
        table.AddRow("Rate Limit Hit At", rateLimitHitAt > 0 ? $"[yellow]Request #{rateLimitHitAt}[/]" : "N/A");
        table.AddRow("Retry After", retryAfter ?? "N/A");
        table.AddRow("Avg Response Time", $"{(int)responseTimes.Average()}ms");

        AnsiConsole.Write(table);

        if (rateLimitHitAt > 19 && rateLimitHitAt <= 25)
        {
            AnsiConsole.MarkupLine("\n[green]✓ Rate limiting working as expected![/]");
        }
        else if (rateLimitHitAt == 0)
        {
            AnsiConsole.MarkupLine("\n[yellow]⚠ Rate limit not hit (might need more requests or limit changed)[/]");
        }
        else
        {
            AnsiConsole.MarkupLine("\n[red]✗ Rate limiting not working as expected![/]");
        }
    }

    static async Task TestPerIpStrictRateLimit()
    {
        AnsiConsole.Write(new Rule("[blue]Testing Per-IP Strict Rate Limiting[/]"));
        AnsiConsole.WriteLine();

        AnsiConsole.MarkupLine("[grey]Note: This test simulates file uploads (strict rate limit)[/]\n");

        // Login first
        var loginResponse = await HttpClient.PostAsJsonAsync(
            $"{ApiBaseUrl}/api/Auth/login",
            new { email = "john.doe@example.com", password = "Password123!" });

        var authResult = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();
        var token = authResult?.AccessToken;

        HttpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Get a task ID
        var tasksResponse = await HttpClient.GetAsync($"{ApiBaseUrl}/api/Tasks?pageSize=1");
        var tasksResult = await tasksResponse.Content.ReadFromJsonAsync<TasksResponse>();
        var taskId = tasksResult?.Items?.FirstOrDefault()?.Id;

        if (taskId == null)
        {
            AnsiConsole.MarkupLine("[red]✗ No tasks found to test with[/]");
            return;
        }

        const int totalRequests = 15;
        var successCount = 0;
        var rateLimitedCount = 0;
        var rateLimitHitAt = 0;
        var responseTimes = new List<long>();
        string? retryAfter = null;

        await AnsiConsole.Progress()
            .Columns(new ProgressColumn[]
            {
                new TaskDescriptionColumn(),
                new ProgressBarColumn(),
                new PercentageColumn(),
                new SpinnerColumn(),
            })
            .StartAsync(async ctx =>
            {
                var task = ctx.AddTask($"[blue]Simulating {totalRequests} file upload requests...[/]", maxValue: totalRequests);

                for (int i = 1; i <= totalRequests; i++)
                {
                    using var content = new MultipartFormDataContent();
                    var fileContent = new ByteArrayContent(new byte[] { 1, 2, 3, 4, 5 });
                    content.Add(fileContent, "file", "test.txt");

                    var sw = Stopwatch.StartNew();
                    var response = await HttpClient.PostAsync(
                        $"{ApiBaseUrl}/api/Attachments/task/{taskId}",
                        content);
                    sw.Stop();

                    responseTimes.Add(sw.ElapsedMilliseconds);

                    if (response.StatusCode == HttpStatusCode.Created || response.StatusCode == HttpStatusCode.OK)
                    {
                        successCount++;
                    }
                    else if (response.StatusCode == HttpStatusCode.TooManyRequests)
                    {
                        rateLimitedCount++;
                        if (rateLimitHitAt == 0)
                        {
                            rateLimitHitAt = i;
                        }

                        var errorContent = await response.Content.ReadFromJsonAsync<ErrorResponse>();
                        retryAfter = errorContent?.RetryAfter;
                    }

                    task.Increment(1);
                    await Task.Delay(50); // Small delay
                }
            });

        AnsiConsole.WriteLine();

        // Display results
        var table = new Table();
        table.Border(TableBorder.Rounded);
        table.AddColumn("Metric");
        table.AddColumn("Value");

        table.AddRow("Total Requests", totalRequests.ToString());
        table.AddRow("Successful (201)", $"[green]{successCount}[/]");
        table.AddRow("Rate Limited (429)", rateLimitedCount > 0 ? $"[red]{rateLimitedCount}[/]" : "0");
        table.AddRow("Rate Limit Hit At", rateLimitHitAt > 0 ? $"[yellow]Request #{rateLimitHitAt}[/]" : "N/A");
        table.AddRow("Retry After", retryAfter ?? "N/A");
        table.AddRow("Avg Response Time", $"{(int)responseTimes.Average()}ms");

        AnsiConsole.Write(table);

        if (rateLimitHitAt > 9 && rateLimitHitAt <= 15)
        {
            AnsiConsole.MarkupLine("\n[green]✓ Rate limiting working as expected![/]");
        }
        else if (rateLimitHitAt == 0)
        {
            AnsiConsole.MarkupLine("\n[yellow]⚠ Rate limit not hit (might need more requests or limit changed)[/]");
        }
        else
        {
            AnsiConsole.MarkupLine("\n[red]✗ Rate limiting not working as expected![/]");
        }
    }
}
