# Laser Focus - PowerShell Deployment Script
# Enhanced deployment script with error handling and validation

param(
    [switch]$SkipTests = $false,
    [switch]$SkipPackaging = $false,
    [string]$OutputPath = "dist\LaserFocus-win-x64",
    [string]$Configuration = "Release"
)

# Color output functions
function Write-Success { param($Message) Write-Host $Message -ForegroundColor Green }
function Write-Warning { param($Message) Write-Host $Message -ForegroundColor Yellow }
function Write-Error { param($Message) Write-Host $Message -ForegroundColor Red }
function Write-Info { param($Message) Write-Host $Message -ForegroundColor Cyan }

# Header
Write-Host "========================================" -ForegroundColor Magenta
Write-Host "    Laser Focus Deployment Script" -ForegroundColor Magenta
Write-Host "========================================" -ForegroundColor Magenta
Write-Host ""

# Validate prerequisites
Write-Info "Checking prerequisites..."

# Check if .NET SDK is installed
try {
    $dotnetVersion = dotnet --version
    Write-Success "✓ .NET SDK found: $dotnetVersion"
} catch {
    Write-Error "✗ .NET SDK not found. Please install .NET 9.0 SDK or later."
    exit 1
}

# Check if solution file exists
if (-not (Test-Path "LaserFocus.sln")) {
    Write-Error "✗ LaserFocus.sln not found. Please run this script from the project root directory."
    exit 1
}
Write-Success "✓ Solution file found"

# Clean previous builds
Write-Info "Cleaning previous builds..."
try {
    dotnet clean --configuration $Configuration --verbosity quiet
    Write-Success "✓ Clean completed"
} catch {
    Write-Warning "⚠ Clean operation had warnings, continuing..."
}

# Restore packages
Write-Info "Restoring NuGet packages..."
try {
    dotnet restore --verbosity quiet
    Write-Success "✓ Package restore completed"
} catch {
    Write-Error "✗ Package restore failed"
    exit 1
}

# Build solution
Write-Info "Building solution in $Configuration configuration..."
try {
    dotnet build --configuration $Configuration --no-restore --verbosity quiet
    Write-Success "✓ Build completed successfully"
} catch {
    Write-Error "✗ Build failed"
    exit 1
}

# Run tests (optional)
if (-not $SkipTests) {
    Write-Info "Running tests..."
    try {
        $testResult = dotnet test --configuration $Configuration --no-build --verbosity quiet --logger "console;verbosity=minimal"
        Write-Success "✓ Tests completed"
    } catch {
        Write-Warning "⚠ Some tests failed, but continuing with deployment..."
    }
} else {
    Write-Warning "⚠ Skipping tests as requested"
}

# Create output directory
Write-Info "Preparing output directory..."
if (Test-Path $OutputPath) {
    Remove-Item $OutputPath -Recurse -Force
}
New-Item -ItemType Directory -Path $OutputPath -Force | Out-Null
Write-Success "✓ Output directory prepared: $OutputPath"

# Publish application
Write-Info "Publishing self-contained application..."
try {
    dotnet publish "src\LaserFocus\LaserFocus.csproj" `
        --configuration $Configuration `
        --runtime win-x64 `
        --self-contained true `
        --output $OutputPath `
        --verbosity minimal `
        --no-build
    Write-Success "✓ Application published successfully"
} catch {
    Write-Error "✗ Publish failed"
    exit 1
}

# Verify executable exists
$exePath = Join-Path $OutputPath "LaserFocus.exe"
if (-not (Test-Path $exePath)) {
    Write-Error "✗ LaserFocus.exe not found in output directory"
    exit 1
}
Write-Success "✓ Executable verified: LaserFocus.exe"

# Get file size information
$exeSize = [math]::Round((Get-Item $exePath).Length / 1MB, 2)
Write-Info "Executable size: $exeSize MB"

# Create deployment package (optional)
if (-not $SkipPackaging) {
    Write-Info "Creating deployment package..."
    $packagePath = "dist\LaserFocus-Release.zip"
    
    if (Test-Path $packagePath) {
        Remove-Item $packagePath -Force
    }
    
    try {
        Compress-Archive -Path "$OutputPath\*" -DestinationPath $packagePath -CompressionLevel Optimal
        $packageSize = [math]::Round((Get-Item $packagePath).Length / 1MB, 2)
        Write-Success "✓ Package created: $packagePath ($packageSize MB)"
    } catch {
        Write-Warning "⚠ Failed to create deployment package, but executable is ready"
    }
} else {
    Write-Warning "⚠ Skipping packaging as requested"
}

# Display summary
Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "         DEPLOYMENT COMPLETED" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Success "✓ Build Configuration: $Configuration"
Write-Success "✓ Output Directory: $OutputPath"
Write-Success "✓ Executable: LaserFocus.exe ($exeSize MB)"
if (-not $SkipPackaging -and (Test-Path "dist\LaserFocus-Release.zip")) {
    Write-Success "✓ Package: LaserFocus-Release.zip"
}
Write-Host ""

# Usage instructions
Write-Info "USAGE INSTRUCTIONS:"
Write-Host "1. Navigate to: $OutputPath" -ForegroundColor White
Write-Host "2. Right-click LaserFocus.exe" -ForegroundColor White
Write-Host "3. Select 'Run as administrator'" -ForegroundColor White
Write-Host ""
Write-Warning "⚠ Administrator privileges are required for full functionality"
Write-Host ""

# Performance information
Write-Info "PERFORMANCE FEATURES:"
Write-Host "• ReadyToRun compilation for faster startup" -ForegroundColor White
Write-Host "• Tiered compilation and PGO optimizations" -ForegroundColor White
Write-Host "• Single-file deployment" -ForegroundColor White
Write-Host "• Optimized garbage collection settings" -ForegroundColor White
Write-Host ""

Write-Success "Deployment script completed successfully!"

# Optional: Open output directory
$openDir = Read-Host "Open output directory? (y/N)"
if ($openDir -eq 'y' -or $openDir -eq 'Y') {
    Start-Process explorer.exe -ArgumentList $OutputPath
}