# Clean All Build Outputs Script
Write-Host "Cleaning all build outputs..." -ForegroundColor Green
Write-Host ""

try {
    # Clean solution
    Write-Host "Cleaning solution..." -ForegroundColor Cyan
    dotnet clean --verbosity quiet
    
    # Remove bin and obj directories
    Write-Host "Removing bin and obj directories..." -ForegroundColor Cyan
    Get-ChildItem -Path . -Recurse -Directory -Name "bin","obj" | ForEach-Object {
        $fullPath = $_.FullName
        if (Test-Path $fullPath) {
            Remove-Item $fullPath -Recurse -Force
            Write-Host "  Removed: $fullPath" -ForegroundColor Gray
        }
    }
    
    # Remove dist directory if it exists
    if (Test-Path "dist") {
        Write-Host "Removing dist directory..." -ForegroundColor Cyan
        Remove-Item "dist" -Recurse -Force
        Write-Host "  Removed: dist" -ForegroundColor Gray
    }
    
    Write-Host ""
    Write-Host "All build outputs cleaned!" -ForegroundColor Green
    Write-Host "Next run will build fresh from your latest code." -ForegroundColor Green
    
} catch {
    Write-Host "Error during cleanup: $_" -ForegroundColor Red
}

Write-Host ""
Write-Host "Press any key to exit..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")