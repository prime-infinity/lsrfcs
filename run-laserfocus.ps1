# LaserFocus Productivity App Launcher
Write-Host "Starting LaserFocus Productivity App..." -ForegroundColor Green
Write-Host ""
Write-Host "Note: This application requires administrator privileges to function properly." -ForegroundColor Yellow
Write-Host "If prompted by Windows UAC, please click 'Yes' to allow the application to run." -ForegroundColor Yellow
Write-Host ""

# Change to script directory
Set-Location $PSScriptRoot

# Clean and rebuild to ensure we have the latest version
try {
    Write-Host "Cleaning and rebuilding to ensure latest version..." -ForegroundColor Cyan
    dotnet clean --configuration Debug --verbosity quiet
    dotnet build --configuration Debug --verbosity quiet
    
    # Check if executable exists
    $exePath = "src\LaserFocus\bin\Debug\net9.0-windows\LaserFocus.exe"
    if (-not (Test-Path $exePath)) {
        Write-Host "Error: LaserFocus.exe not found. Build may have failed." -ForegroundColor Red
        Write-Host "Please check the build output above for errors." -ForegroundColor Yellow
        throw "Executable not found at: $exePath"
    }
    
    # Run the application
    Write-Host "Starting application..." -ForegroundColor Cyan
    & $exePath
} catch {
    Write-Host "Error running application: $_" -ForegroundColor Red
    Write-Host ""
    Write-Host "Make sure you have .NET installed and are running as administrator." -ForegroundColor Yellow
}

Write-Host ""
Write-Host "Press any key to exit..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")