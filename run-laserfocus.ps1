# LaserFocus Productivity App Launcher
Write-Host "Starting LaserFocus Productivity App..." -ForegroundColor Green
Write-Host ""
Write-Host "Note: This application requires administrator privileges to function properly." -ForegroundColor Yellow
Write-Host "If prompted by Windows UAC, please click 'Yes' to allow the application to run." -ForegroundColor Yellow
Write-Host ""

# Change to script directory
Set-Location $PSScriptRoot

# Run the application
try {
    dotnet run --project src/LaserFocus/LaserFocus.csproj
} catch {
    Write-Host "Error running application: $_" -ForegroundColor Red
    Write-Host ""
    Write-Host "Make sure you have .NET installed and are running as administrator." -ForegroundColor Yellow
}

Write-Host ""
Write-Host "Press any key to exit..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")