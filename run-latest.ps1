# LaserFocus - Run Latest Version Script
Write-Host "Starting LaserFocus (Latest Version)..." -ForegroundColor Green
Write-Host ""
Write-Host "Note: This application requires administrator privileges to function properly." -ForegroundColor Yellow
Write-Host "If prompted by Windows UAC, please click 'Yes' to allow the application to run." -ForegroundColor Yellow
Write-Host ""

# Change to script directory
Set-Location $PSScriptRoot

try {
    # Clean and rebuild to ensure we have the latest version
    Write-Host "Cleaning and rebuilding to ensure latest version..." -ForegroundColor Cyan
    dotnet clean --configuration Debug --verbosity quiet
    dotnet build --configuration Debug --verbosity quiet
    
    # Run the application
    Write-Host "Starting application..." -ForegroundColor Cyan
    dotnet run --project src/LaserFocus/LaserFocus.csproj --configuration Debug
    
} catch {
    Write-Host "Error running application: $_" -ForegroundColor Red
    Write-Host ""
    Write-Host "Make sure you have .NET installed and are running as administrator." -ForegroundColor Yellow
}

Write-Host ""
Write-Host "Press any key to exit..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")