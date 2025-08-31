# LaserFocus Productivity App

A Windows desktop application designed to help developers maintain focus by providing system-wide website blocking and automatic application management.

## Features

- **System-wide website blocking** - Blocks distracting websites across all browsers by modifying the Windows hosts file
- **Application monitoring** - Automatically closes non-essential applications while allowing development tools to run
- **Simple on/off control** - Single toggle to enable/disable all monitoring features
- **Real-time process monitoring** - Live view of running processes with allow/block status indicators
- **Clean, minimal interface** - Focused UI design to minimize distractions

## Requirements

- Windows 10/11
- .NET 9.0 or later
- Administrator privileges (required for hosts file modification and process termination)

## How to Run

### Option 1: Using the Latest Version Script (Recommended)

**For the most up-to-date version with all your latest changes:**

1. Right-click on `run-latest.bat`
2. Select "Run as administrator"
3. Click "Yes" when prompted by Windows UAC

This script automatically cleans and rebuilds to ensure you're running the latest code.

### Option 2: Using the Standard Batch File

1. Right-click on `run-laserfocus.bat`
2. Select "Run as administrator"
3. Click "Yes" when prompted by Windows UAC

### Option 3: Using PowerShell

1. Right-click on PowerShell and select "Run as administrator"
2. Navigate to the project directory
3. Run: `.\run-latest.ps1` (for latest version) or `.\run-laserfocus.ps1`

### Option 4: Using .NET CLI

1. Open Command Prompt or PowerShell as administrator
2. Navigate to the project directory
3. Run: `dotnet run --project src/LaserFocus/LaserFocus.csproj --configuration Debug`

### Option 5: Running the Executable Directly

**Note:** Direct executable runs may not reflect your latest code changes. Use the scripts above for development.

1. Navigate to `src\LaserFocus\bin\Debug\net9.0-windows\` (for latest changes)
2. Right-click on `LaserFocus.exe`
3. Select "Run as administrator"

## Development Workflow

### Ensuring Latest Version

If you're experiencing issues with old versions running:

1. Run `clean-all-builds.bat` to remove all build outputs
2. Use `run-latest.bat` to ensure you're running the most recent code

### Build Management

- **Development**: Use Debug configuration for active development
- **Release**: Use `deploy-release.bat` or `deploy-release.ps1` for final builds
- **Clean**: Use `clean-all-builds.bat` to remove all build artifacts

## Usage

### Website Blocking

1. Enter a website URL in the "Block Website" text box (e.g., `facebook.com`, `youtube.com`)
2. Click "Block Website" to add it to the blocked list
3. The website will be blocked system-wide across all browsers
4. To unblock, click "Remove" next to the website in the blocked list

### Process Monitoring

1. Click "Start Monitoring" to begin automatic process management
2. The application will scan for running processes every 2 seconds
3. Non-allowed applications will be automatically terminated
4. Allowed applications include: Chrome, VS Code, Kiro, Visual Studio, Notepad
5. Click "Stop Monitoring" to disable automatic process termination

## Configuration

Configuration files are stored in the `config/` directory:

- `blocked-websites.json` - List of blocked websites
- `allowed-applications.json` - List of allowed applications
- `app-settings.json` - Application preferences

## Building from Source

```cmd
# Build the solution
dotnet build LaserFocus.sln

# Build release version
dotnet build LaserFocus.sln -c Release

# Run tests
dotnet test
```

## Important Notes

- **Administrator privileges are required** for the application to function properly
- Website blocking works by modifying the Windows hosts file (`C:\Windows\System32\drivers\etc\hosts`)
- Process termination requires elevated privileges
- The application will show warnings if running without administrator privileges
- Website blocks persist even after closing the application
- Process monitoring stops when the application is closed

## Troubleshooting

### "The requested operation requires elevation" Error

This means the application needs to run as administrator. Use one of the "Run as administrator" methods above.

### Website Blocking Not Working

- Ensure the application is running as administrator
- Check that the website URL is entered correctly (without http:// or https://)
- Clear your browser cache and restart the browser

### Process Monitoring Not Working

- Ensure the application is running as administrator
- Some system processes cannot be terminated even with admin privileges
- Check the process list to see which processes are detected as "Allowed" or "Blocked"

## License

This project is for educational and personal productivity use.
