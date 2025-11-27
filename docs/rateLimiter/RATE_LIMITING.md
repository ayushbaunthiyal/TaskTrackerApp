# Rate Limiting

This document describes the rate limiting implementation in the TaskTracker API.

## Overview

The TaskTracker API implements three-tier rate limiting to protect against abuse and ensure fair usage:

1. **Per-User Rate Limiting** - Limits authenticated users based on their identity
2. **Per-IP Auth Rate Limiting** - Protects authentication endpoints from brute-force attacks
3. **Per-IP Strict Rate Limiting** - Prevents abuse of resource-intensive endpoints like file uploads

## Rate Limiting Policies

### 1. Per-User Policy
- **Limit**: 100 requests per 60 seconds (per authenticated user)
- **Applied to**:
  - All `/api/Tasks` endpoints
  - All `/api/Attachments` endpoints (except uploads)
- **Partition Key**: JWT "sub" claim (user ID)
- **Queue**: Up to 5 requests can queue when limit is reached

### 2. Per-IP Auth Policy
- **Limit**: 20 requests per 900 seconds (15 minutes per IP address)
- **Applied to**:
  - `POST /api/Auth/register`
  - `POST /api/Auth/login`
- **Partition Key**: Client IP address
- **Queue**: No queueing (immediate rejection)
- **Purpose**: Prevent brute-force attacks on authentication endpoints

### 3. Per-IP Strict Policy
- **Limit**: 10 requests per 60 seconds (per IP address)
- **Applied to**:
  - `POST /api/Attachments/task/{taskId}` (file uploads)
- **Partition Key**: Client IP address
- **Queue**: No queueing (immediate rejection)
- **Purpose**: Prevent abuse of resource-intensive file upload operations

## Configuration

Rate limiting is configured in `appsettings.json`:

```json
{
  "RateLimiting": {
    "EnableRateLimiting": true,
    "PerUser": {
      "PermitLimit": 100,
      "WindowInSeconds": 60,
      "QueueLimit": 5
    },
    "PerIpAuth": {
      "PermitLimit": 20,
      "WindowInSeconds": 900
    },
    "PerIpStrict": {
      "PermitLimit": 10,
      "WindowInSeconds": 60
    }
  }
}
```

### Configuration Options

- **EnableRateLimiting**: Set to `false` to disable all rate limiting
- **PermitLimit**: Maximum number of requests allowed in the time window
- **WindowInSeconds**: Time window duration in seconds
- **QueueLimit**: Number of requests to queue when limit is exceeded (0 = no queueing)

## Error Response

When a rate limit is exceeded, the API returns a `429 Too Many Requests` response:

```json
{
  "title": "Too Many Requests",
  "status": 429,
  "detail": "Rate limit exceeded. Please try again later.",
  "retryAfter": "45 seconds"
}
```

The `retryAfter` field indicates how long the client should wait before retrying.

## Logging

Rate limit violations are logged with Serilog at the Warning level:

```
Rate limit exceeded for PerUserPolicy on /api/Tasks. User: user@example.com, IP: 192.168.1.100
```

This allows monitoring and alerting on potential abuse.

## Testing Rate Limiting

A console test application is provided to verify rate limiting works correctly.

### Prerequisites

1. Ensure the API is running on `http://localhost:5128`
2. Ensure you have a test user account (default: john.doe@example.com / Password123!)
3. Ensure you have at least one task in the database

### Running the Tests

1. Navigate to the `TaskTracker.RateLimitTester` folder
2. Run the application:
   ```powershell
   dotnet run
   ```
3. Select a test from the menu:
   - **Test Per-User Rate Limiting** - Sends 105 requests to /api/Tasks (expects rate limit after ~100)
   - **Test Per-IP Auth Rate Limiting** - Sends 25 login requests (expects rate limit after ~20)
   - **Test Per-IP Strict Rate Limiting** - Sends 15 file upload requests (expects rate limit after ~10)
   - **Run All Tests** - Runs all three tests sequentially

### Expected Results

Each test displays:
- Total requests sent
- Successful requests (200/201 status)
- Rate-limited requests (429 status)
- Which request number hit the rate limit
- Retry-after duration
- Average response time

The test will indicate whether rate limiting is working as expected based on when the limit was hit.

## Implementation Details

### Algorithm

The implementation uses the **Fixed Window** algorithm:
- Requests are counted within fixed time windows
- When a window expires, the counter resets
- Simple and predictable for clients

### Middleware Order

Rate limiting middleware is positioned in the pipeline as:
```
CORS → Rate Limiter → Authentication → Authorization
```

This ensures:
1. CORS headers are applied even to rate-limited requests
2. IP addresses are correctly detected
3. User context is available for per-user limiting

### Controller Annotations

Rate limiting is applied using attributes:

```csharp
// Class-level (applies to all endpoints)
[EnableRateLimiting("PerUserPolicy")]
public class TasksController : ControllerBase { }

// Endpoint-level (overrides class-level)
[EnableRateLimiting("PerIpStrictPolicy")]
public async Task<IActionResult> UploadAttachment() { }
```

## Adjusting Rate Limits

To adjust rate limits:

1. Edit `appsettings.json`
2. Modify `PermitLimit` and/or `WindowInSeconds` for the desired policy
3. Restart the API
4. Test with the console application to verify

### Example: Make Auth Limits More Restrictive

```json
"PerIpAuth": {
  "PermitLimit": 10,      // Reduced from 20
  "WindowInSeconds": 1800 // Increased from 900 (now 30 minutes)
}
```

## Disabling Rate Limiting

To disable rate limiting entirely:

```json
"RateLimiting": {
  "EnableRateLimiting": false,
  // ... rest of config
}
```

Or remove the `UseRateLimiter()` call from `Program.cs`.

## Monitoring

Rate limit violations are logged and can be monitored using:

1. **Serilog Console Output** - View logs in real-time
2. **Log Files** - Check `Logs/` directory for persistent logs
3. **APM Tools** - Use Application Performance Monitoring tools to track 429 responses

### Example Log Query

To find all rate limit violations in the last hour:
```
timestamp > now-1h AND level = "Warning" AND message contains "Rate limit exceeded"
```

## Best Practices

1. **Client-Side Handling**:
   - Implement exponential backoff when receiving 429 responses
   - Respect the `retryAfter` value in error responses
   - Cache API responses to reduce request volume

2. **Production Considerations**:
   - Monitor rate limit logs for abuse patterns
   - Adjust limits based on actual usage patterns
   - Consider implementing allow-lists for trusted IPs/users
   - Use distributed rate limiting for multi-instance deployments

3. **Security**:
   - Keep auth endpoint limits strict to prevent brute-force
   - Monitor for distributed attacks across multiple IPs
   - Consider implementing CAPTCHA for repeated violations

## Troubleshooting

### Rate Limiting Not Working

1. Check `appsettings.json`:
   - Ensure `EnableRateLimiting` is `true`
   - Verify limits are configured

2. Check middleware order in `Program.cs`:
   - `UseRateLimiter()` should be after `UseCors()` and before `UseAuthentication()`

3. Check controller attributes:
   - Ensure `[EnableRateLimiting("PolicyName")]` is present

4. Check logs:
   - Look for rate limiter initialization messages
   - Verify IP address detection is working

### False Positives

If legitimate users are being rate-limited too frequently:

1. Increase `PermitLimit` for the affected policy
2. Increase `WindowInSeconds` to spread requests over longer periods
3. Add `QueueLimit` to allow bursts of requests

### Testing Issues

If the console test application isn't working:

1. Verify the API is running on `http://localhost:5128`
2. Ensure test credentials are valid (john.doe@example.com / Password123!)
3. Check that at least one task exists in the database
4. Verify rate limiting is enabled in `appsettings.json`
5. Restart the API after configuration changes

## Future Enhancements

Potential improvements to the rate limiting system:

1. **Distributed Rate Limiting** - Use Redis for shared state across multiple API instances
2. **Rate Limit Headers** - Return `X-RateLimit-Limit`, `X-RateLimit-Remaining`, `X-RateLimit-Reset` headers
3. **Custom Partitioning** - Rate limit by user role, subscription tier, or API key
4. **Sliding Window** - Use sliding window algorithm for more accurate limiting
5. **Dynamic Limits** - Adjust limits based on server load or time of day
6. **Allow Lists** - Exempt specific users or IPs from rate limiting
