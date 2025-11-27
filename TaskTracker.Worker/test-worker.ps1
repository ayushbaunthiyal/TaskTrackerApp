# TaskTracker Worker Test Script
# Run this after setting up the worker to verify everything works

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "TaskTracker Worker Test Script" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Step 1: Check PostgreSQL
Write-Host "[1/6] Checking PostgreSQL..." -ForegroundColor Yellow
$postgres = docker ps | Select-String "tasktracker-postgres"
if ($postgres) {
    Write-Host "✓ PostgreSQL is running" -ForegroundColor Green
} else {
    Write-Host "✗ PostgreSQL not running. Starting..." -ForegroundColor Red
    docker-compose up -d postgres
    Start-Sleep -Seconds 10
    Write-Host "✓ PostgreSQL started" -ForegroundColor Green
}

# Step 2: Check if worker project exists
Write-Host "[2/6] Checking worker project..." -ForegroundColor Yellow
if (Test-Path "TaskTracker.Worker\TaskTracker.Worker.csproj") {
    Write-Host "✓ Worker project found" -ForegroundColor Green
} else {
    Write-Host "✗ Worker project not found in current directory" -ForegroundColor Red
    Write-Host "Please run this script from: d:\repos\mygit\TaskTrackerApp" -ForegroundColor Red
    exit 1
}

# Step 3: Restore dependencies
Write-Host "[3/6] Restoring dependencies..." -ForegroundColor Yellow
Set-Location TaskTracker.Worker
dotnet restore --verbosity quiet
if ($LASTEXITCODE -eq 0) {
    Write-Host "✓ Dependencies restored" -ForegroundColor Green
} else {
    Write-Host "✗ Failed to restore dependencies" -ForegroundColor Red
    exit 1
}

# Step 4: Build worker
Write-Host "[4/6] Building worker..." -ForegroundColor Yellow
dotnet build --verbosity quiet
if ($LASTEXITCODE -eq 0) {
    Write-Host "✓ Worker built successfully" -ForegroundColor Green
} else {
    Write-Host "✗ Build failed" -ForegroundColor Red
    exit 1
}

# Step 5: Check configuration
Write-Host "[5/6] Checking configuration..." -ForegroundColor Yellow
$config = Get-Content "appsettings.json" | ConvertFrom-Json
if ($config.MailgunSettings.ApiKey -and $config.MailgunSettings.Domain) {
    Write-Host "✓ Mailgun configured" -ForegroundColor Green
} else {
    Write-Host "⚠ Mailgun not configured - emails will fail" -ForegroundColor Yellow
}

if ($config.ConnectionStrings.DefaultConnection) {
    Write-Host "✓ Database connection configured" -ForegroundColor Green
} else {
    Write-Host "✗ Database connection not configured" -ForegroundColor Red
    exit 1
}

# Step 6: Instructions
Write-Host "[6/6] Setup complete!" -ForegroundColor Yellow
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Next Steps:" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "1. Add your email to Mailgun authorized recipients:" -ForegroundColor White
Write-Host "   https://app.mailgun.com/app/sending/domains/" -ForegroundColor Gray
Write-Host ""
Write-Host "2. Run the worker:" -ForegroundColor White
Write-Host "   dotnet run" -ForegroundColor Gray
Write-Host ""
Write-Host "3. Create test tasks in the UI:" -ForegroundColor White
Write-Host "   - Open http://localhost:3000" -ForegroundColor Gray
Write-Host "   - Login or register" -ForegroundColor Gray
Write-Host "   - Create tasks with due dates in next 24 hours" -ForegroundColor Gray
Write-Host ""
Write-Host "4. Wait for reminder cycle (30 minutes in dev)" -ForegroundColor White
Write-Host "   - Or reduce CheckIntervalMinutes to 1 for testing" -ForegroundColor Gray
Write-Host ""
Write-Host "5. Check your email for reminders!" -ForegroundColor White
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Configuration Summary:" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Check Interval: $($config.WorkerSettings.CheckIntervalMinutes) minutes" -ForegroundColor White
Write-Host "Lookahead Window: $($config.WorkerSettings.DueDateLookaheadHours) hours" -ForegroundColor White
Write-Host "Daily Email Quota: $($config.WorkerSettings.DailyEmailQuota)" -ForegroundColor White
Write-Host "Email Sending: $($config.WorkerSettings.EnableEmailSending)" -ForegroundColor White
Write-Host ""
Write-Host "Ready to run! Press Ctrl+C to exit this script." -ForegroundColor Green
Write-Host "Then run: dotnet run" -ForegroundColor Green
Write-Host ""

Set-Location ..
