# Rate Limiting Quick Start Guide

## Step 1: Restart the API

Rate limiting has been added to the API. Restart it to activate the new middleware:

```powershell
# Stop the current API if running (Ctrl+C)
# Then restart:
cd TaskTracker.API
dotnet run
```

The API should start on `http://localhost:5128` with rate limiting enabled.

## Step 2: Verify Configuration

Check the logs when the API starts. You should see the rate limiter being configured.

Verify `TaskTracker.API/appsettings.json` contains:

```json
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
```

## Step 3: Run the Rate Limit Tester

In a **new terminal**, run the test application:

```powershell
cd TaskTracker.RateLimitTester
dotnet run
```

You'll see a colorful menu with test options:

```
  ____       _         _     _           _ _     _____         _            
 |  _ \ __ _| |_ ___  | |   (_)_ __ ___ (_) |_  |_   _|__  ___| |_ ___ _ __ 
 | |_) / _` | __/ _ \ | |   | | '_ ` _ \| | __|   | |/ _ \/ __| __/ _ \ '__|
 |  _ < (_| | ||  __/ | |___| | | | | | | | |_    | |  __/\__ \ ||  __/ |   
 |_| \_\__,_|\__\___| |_____|_|_| |_| |_|_|\__|   |_|\___||___/\__\___|_|   

API Base URL: http://localhost:5128

Select a test to run:
❯ Test Per-User Rate Limiting (100 req/min)
  Test Per-IP Auth Rate Limiting (20 req/15min)
  Test Per-IP Strict Rate Limiting (10 req/min)
  Run All Tests
  Exit
```

## Step 4: Test Each Policy

### Test 1: Per-User Rate Limiting

Select **"Test Per-User Rate Limiting (100 req/min)"**

This test will:
1. Login as john.doe@example.com
2. Send 105 GET requests to `/api/Tasks`
3. Track when the rate limit kicks in

**Expected Result:**
- First 100 requests: ✅ 200 OK
- Requests 101-105: ❌ 429 Too Many Requests
- Rate limit hits around request #101

### Test 2: Per-IP Auth Rate Limiting

Select **"Test Per-IP Auth Rate Limiting (20 req/15min)"**

This test will:
1. Send 25 login attempts with invalid credentials
2. Track when the rate limit kicks in

**Expected Result:**
- First 20 requests: ✅ 401 Unauthorized (authentication failed, but not rate limited)
- Requests 21-25: ❌ 429 Too Many Requests
- Rate limit hits around request #21

### Test 3: Per-IP Strict Rate Limiting

Select **"Test Per-IP Strict Rate Limiting (10 req/min)"**

This test will:
1. Login as john.doe@example.com
2. Send 15 file upload requests to `/api/Attachments/task/{taskId}`
3. Track when the rate limit kicks in

**Expected Result:**
- First 10 requests: ✅ 201 Created
- Requests 11-15: ❌ 429 Too Many Requests
- Rate limit hits around request #11

**Note:** This test requires at least one task to exist in the database.

## Step 5: Interpret Results

Each test displays a summary table:

```
╭──────────────────────┬─────────────────╮
│ Metric               │ Value           │
├──────────────────────┼─────────────────┤
│ Total Requests       │ 105             │
│ Successful (200)     │ 100             │
│ Rate Limited (429)   │ 5               │
│ Rate Limit Hit At    │ Request #101    │
│ Retry After          │ 45 seconds      │
│ Avg Response Time    │ 12ms            │
╰──────────────────────┴─────────────────╯

✓ Rate limiting working as expected!
```

**Success Indicators:**
- ✅ Green checkmark = Rate limiting working correctly
- ⚠️ Yellow warning = Rate limit not hit (might need adjustment)
- ❌ Red X = Rate limiting not working as expected

## Step 6: Check API Logs

While tests are running, check the API console logs for rate limit warnings:

```
[Warning] Rate limit exceeded for PerUserPolicy on /api/Tasks. User: john.doe@example.com, IP: ::1
```

These logs help you monitor abuse and troubleshoot issues.

## Common Issues

### "Login failed!"
- Ensure the API is running
- Verify test credentials exist: john.doe@example.com / Password123!
- Check the API logs for authentication errors

### "No tasks found to test with"
- The strict rate limit test needs at least one task
- Login to the React UI and create a task
- Or use the API directly to create a task

### Rate limit not being hit
- Ensure `EnableRateLimiting: true` in appsettings.json
- Restart the API after configuration changes
- Check that the rate limiter middleware is in Program.cs
- Verify controller attributes are present

### Too many requests getting through
- Check the `PermitLimit` values in appsettings.json
- Ensure the correct policy is applied to the endpoint
- Verify IP address detection is working (check logs)

## Adjusting Limits

To make testing easier, you can temporarily lower the limits:

Edit `TaskTracker.API/appsettings.json`:

```json
"PerUser": {
  "PermitLimit": 10,  // Lowered from 100 for easier testing
  "WindowInSeconds": 60,
  "QueueLimit": 5
}
```

Restart the API and re-run the tests to see the lower limit in action.

## Next Steps

After verifying rate limiting works:

1. ✅ Restore rate limits to production values
2. ✅ Add monitoring/alerting for rate limit violations
3. ✅ Document rate limits in API documentation
4. ✅ Consider implementing rate limit headers (X-RateLimit-*)
5. ✅ Test with multiple clients to verify per-IP limiting

## Documentation

See `RATE_LIMITING.md` for complete documentation including:
- Detailed policy descriptions
- Configuration options
- Monitoring and troubleshooting
- Production best practices
- Future enhancement ideas
