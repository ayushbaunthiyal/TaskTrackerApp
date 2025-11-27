using RestSharp;
using RestSharp.Authenticators;
using TaskTracker.Worker.Configuration;

namespace TaskTracker.Worker.Services;

public interface IEmailService
{
    Task<bool> SendReminderEmailAsync(string toEmail, string userName, string taskTitle, DateTime dueDate, string priority);
}

public class MailgunEmailService : IEmailService
{
    private readonly MailgunSettings _settings;
    private readonly ILogger<MailgunEmailService> _logger;

    public MailgunEmailService(MailgunSettings settings, ILogger<MailgunEmailService> logger)
    {
        _settings = settings;
        _logger = logger;
    }

    public async Task<bool> SendReminderEmailAsync(string toEmail, string userName, string taskTitle, DateTime dueDate, string priority)
    {
        try
        {
            var options = new RestClientOptions(_settings.BaseUrl)
            {
                Authenticator = new HttpBasicAuthenticator("api", _settings.ApiKey)
            };

            var client = new RestClient(options);
            var request = new RestRequest($"{_settings.Domain}/messages", Method.Post);

            request.AddParameter("from", _settings.FromEmail);
            request.AddParameter("to", toEmail);
            request.AddParameter("subject", $"⏰ Task Reminder: {taskTitle}");
            request.AddParameter("html", GenerateEmailHtml(userName, taskTitle, dueDate, priority));
            request.AddParameter("text", GenerateEmailText(userName, taskTitle, dueDate, priority));

            var response = await client.ExecuteAsync(request);

            if (response.IsSuccessful)
            {
                _logger.LogInformation("Email sent successfully to {Email} for task: {Task}", toEmail, taskTitle);
                return true;
            }
            else
            {
                _logger.LogError("Failed to send email to {Email}. Status: {Status}, Error: {Error}",
                    toEmail, response.StatusCode, response.Content);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while sending email to {Email}", toEmail);
            return false;
        }
    }

    private string GenerateEmailHtml(string userName, string taskTitle, DateTime dueDate, string priority)
    {
        var priorityColor = priority switch
        {
            "Critical" => "#dc2626",
            "High" => "#ea580c",
            "Medium" => "#ca8a04",
            "Low" => "#16a34a",
            _ => "#6b7280"
        };

        var timeUntilDue = dueDate - DateTime.UtcNow;
        var urgencyMessage = timeUntilDue.TotalHours < 2
            ? "⚠️ <strong>URGENT:</strong> Due in less than 2 hours!"
            : timeUntilDue.TotalHours < 6
            ? "⏰ Due very soon!"
            : "📅 Upcoming task reminder";

        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
</head>
<body style='margin: 0; padding: 0; font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, ""Helvetica Neue"", Arial, sans-serif; background-color: #f3f4f6;'>
    <table width='100%' cellpadding='0' cellspacing='0' style='background-color: #f3f4f6; padding: 40px 20px;'>
        <tr>
            <td align='center'>
                <table width='600' cellpadding='0' cellspacing='0' style='background-color: #ffffff; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1);'>
                    <!-- Header -->
                    <tr>
                        <td style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 30px; border-radius: 8px 8px 0 0;'>
                            <h1 style='margin: 0; color: #ffffff; font-size: 28px; font-weight: 600;'>
                                📋 TaskTracker Reminder
                            </h1>
                        </td>
                    </tr>
                    
                    <!-- Content -->
                    <tr>
                        <td style='padding: 40px 30px;'>
                            <p style='margin: 0 0 20px; font-size: 16px; color: #374151;'>
                                Hi <strong>{userName}</strong>,
                            </p>
                            
                            <div style='background-color: #fef3c7; border-left: 4px solid #f59e0b; padding: 16px; margin: 20px 0; border-radius: 4px;'>
                                <p style='margin: 0; font-size: 15px; color: #92400e;'>
                                    {urgencyMessage}
                                </p>
                            </div>
                            
                            <div style='background-color: #f9fafb; border-radius: 8px; padding: 24px; margin: 24px 0;'>
                                <h2 style='margin: 0 0 16px; font-size: 20px; color: #111827;'>
                                    {taskTitle}
                                </h2>
                                
                                <table width='100%' cellpadding='8' cellspacing='0'>
                                    <tr>
                                        <td style='color: #6b7280; font-size: 14px; padding: 8px 0;'>
                                            <strong>📅 Due Date:</strong>
                                        </td>
                                        <td style='color: #111827; font-size: 14px; padding: 8px 0; text-align: right;'>
                                            {dueDate:dddd, MMMM dd, yyyy 'at' h:mm tt}
                                        </td>
                                    </tr>
                                    <tr>
                                        <td style='color: #6b7280; font-size: 14px; padding: 8px 0;'>
                                            <strong>⚡ Priority:</strong>
                                        </td>
                                        <td style='text-align: right; padding: 8px 0;'>
                                            <span style='background-color: {priorityColor}; color: white; padding: 4px 12px; border-radius: 12px; font-size: 13px; font-weight: 500;'>
                                                {priority}
                                            </span>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td style='color: #6b7280; font-size: 14px; padding: 8px 0;'>
                                            <strong>⏰ Time Remaining:</strong>
                                        </td>
                                        <td style='color: #111827; font-size: 14px; padding: 8px 0; text-align: right;'>
                                            {FormatTimeRemaining(timeUntilDue)}
                                        </td>
                                    </tr>
                                </table>
                            </div>
                            
                            <p style='margin: 24px 0; font-size: 15px; color: #4b5563; line-height: 1.6;'>
                                Don't forget to complete this task before the deadline. Stay organized and productive! 💪
                            </p>
                            
                            <div style='text-align: center; margin: 30px 0;'>
                                <a href='http://localhost:3000' style='display: inline-block; background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; text-decoration: none; padding: 14px 32px; border-radius: 6px; font-weight: 600; font-size: 15px;'>
                                    View Task in TaskTracker
                                </a>
                            </div>
                        </td>
                    </tr>
                    
                    <!-- Footer -->
                    <tr>
                        <td style='background-color: #f9fafb; padding: 24px 30px; border-radius: 0 0 8px 8px; border-top: 1px solid #e5e7eb;'>
                            <p style='margin: 0; font-size: 13px; color: #6b7280; text-align: center;'>
                                This is an automated reminder from TaskTracker
                            </p>
                            <p style='margin: 8px 0 0; font-size: 13px; color: #9ca3af; text-align: center;'>
                                © 2025 TaskTracker. All rights reserved.
                            </p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
    }

    private string GenerateEmailText(string userName, string taskTitle, DateTime dueDate, string priority)
    {
        var timeUntilDue = dueDate - DateTime.UtcNow;
        return $@"
TaskTracker Reminder
====================

Hi {userName},

You have an upcoming task that needs your attention:

Task: {taskTitle}
Due Date: {dueDate:dddd, MMMM dd, yyyy 'at' h:mm tt}
Priority: {priority}
Time Remaining: {FormatTimeRemaining(timeUntilDue)}

Don't forget to complete this task before the deadline. Stay organized and productive!

View your tasks at: http://localhost:3000

---
This is an automated reminder from TaskTracker
© 2025 TaskTracker. All rights reserved.
";
    }

    private string FormatTimeRemaining(TimeSpan timeSpan)
    {
        if (timeSpan.TotalMinutes < 0)
            return "⚠️ OVERDUE";

        if (timeSpan.TotalHours < 1)
            return $"{(int)timeSpan.TotalMinutes} minutes";

        if (timeSpan.TotalHours < 24)
            return $"{(int)timeSpan.TotalHours} hours, {timeSpan.Minutes} minutes";

        var days = (int)timeSpan.TotalDays;
        var hours = timeSpan.Hours;
        return $"{days} day{(days != 1 ? "s" : "")}, {hours} hour{(hours != 1 ? "s" : "")}";
    }
}
