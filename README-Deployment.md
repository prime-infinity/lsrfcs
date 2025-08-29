# Laser Focus - Deployment Guide

## Building the Application

### Prerequisites

- .NET 9.0 SDK or later
- Windows 10/11 (for deployment target)
- Administrator privileges (for full functionality)

### Quick Build

#### Using Batch Script (Windows)

```cmd
deploy-release.bat
```

#### Using PowerShell Script

```powershell
.\deploy-release.ps1
```

#### Manual Build Commands

```cmd
# Clean and restore
dotnet clean --configuration Release
dotnet restore

# Build
dotnet build --configuration Release

# Publish self-contained
dotnet publish src\LaserFocus\LaserFocus.csproj ^
    --configuration Release ^
    --runtime win-x64 ^
    --self-contained true ^
    --output "dist\LaserFocus-win-x64"
```

## Deployment Options

### Option 1: Self-Contained Executable

- **Location**: `dist/LaserFocus-win-x64/LaserFocus.exe`
- **Size**: ~60-80 MB (includes .NET runtime)
- **Requirements**: None (fully self-contained)
- **Advantages**: No .NET installation required on target machine

### Option 2: Framework-Dependent

```cmd
dotnet publish src\LaserFocus\LaserFocus.csproj ^
    --configuration Release ^
    --runtime win-x64 ^
    --self-contained false ^
    --output "dist\LaserFocus-framework-dependent"
```

- **Size**: ~5-10 MB
- **Requirements**: .NET 9.0 Runtime on target machine
- **Advantages**: Smaller file size, automatic runtime updates

## Installation Instructions

### For End Users

1. **Download** the `LaserFocus-Release.zip` package
2. **Extract** to desired installation directory (e.g., `C:\Program Files\LaserFocus\`)
3. **Right-click** on `LaserFocus.exe` and select **"Run as administrator"**
4. **Create shortcut** (optional):
   - Right-click `LaserFocus.exe` → "Create shortcut"
   - Move shortcut to Desktop or Start Menu
   - Right-click shortcut → Properties → Advanced → "Run as administrator"

### System Requirements

- **OS**: Windows 10 version 1809 or later, Windows 11
- **Architecture**: x64 (64-bit)
- **Memory**: 100 MB RAM minimum
- **Disk Space**: 100 MB free space
- **Privileges**: Administrator rights required for full functionality

## Configuration

### Default Configuration Files

The application creates configuration files in:

```
config/
├── blocked-websites.json      # Persistent blocked websites
├── allowed-applications.json  # Allowed applications list
└── app-settings.json         # Application preferences
```

### First Run Setup

1. Application will request administrator elevation
2. Default allowed applications: `chrome`, `code`, `kiro`, `devenv`, `notepad`
3. No websites blocked by default

## Performance Optimizations

### Build Optimizations Applied

- **ReadyToRun**: Pre-compiled for faster startup
- **Tiered Compilation**: Optimized runtime performance
- **Profile-Guided Optimization**: Enhanced performance based on usage patterns
- **Single File**: Reduced deployment complexity

### Runtime Optimizations

- **Process Caching**: Reduces CPU usage during monitoring
- **Differential UI Updates**: Efficient ObservableCollection updates
- **Memory Management**: Automatic cleanup and garbage collection
- **Smart Scanning**: Full process scans every 10 seconds, incremental updates every 2 seconds

## Troubleshooting

### Common Issues

#### "Access Denied" Errors

- **Cause**: Application not running with administrator privileges
- **Solution**: Right-click executable and select "Run as administrator"

#### Website Blocking Not Working

- **Cause**: Insufficient privileges or antivirus interference
- **Solutions**:
  1. Ensure administrator privileges
  2. Add application to antivirus exclusions
  3. Check Windows Defender real-time protection settings

#### Process Termination Failures

- **Cause**: Protected processes or insufficient privileges
- **Solutions**:
  1. Verify administrator privileges
  2. Check if processes are system-critical
  3. Review process action log for details

### Performance Issues

#### High CPU Usage

- **Check**: Process monitoring frequency (default: 2 seconds)
- **Solution**: Increase monitoring interval in future versions

#### High Memory Usage

- **Check**: Number of cached processes and log entries
- **Solution**: Application automatically manages cache size (max 100 log entries)

### Logs and Diagnostics

#### Log Files Location

```
%TEMP%\LaserFocus\Logs\
├── LaserFocus-YYYY-MM-DD.log
└── [older log files]
```

#### Viewing Logs

- Use any text editor to view log files
- Logs include timestamps, severity levels, and detailed error information
- Automatic cleanup of logs older than 7 days

## Uninstallation

### Manual Uninstallation

1. **Stop** the application if running
2. **Delete** the installation directory
3. **Remove** configuration files from `config/` directory (optional)
4. **Delete** log files from `%TEMP%\LaserFocus\` (optional)
5. **Restore** hosts file backup if needed:
   ```cmd
   copy "C:\Windows\System32\drivers\etc\hosts.laserfocus.backup" "C:\Windows\System32\drivers\etc\hosts"
   ```

### Clean Hosts File

If you want to remove all LaserFocus entries from hosts file:

1. Open `C:\Windows\System32\drivers\etc\hosts` as administrator
2. Remove lines between `# LaserFocus blocked websites` and `# End LaserFocus blocked websites`
3. Save the file
4. Run `ipconfig /flushdns` in command prompt

## Security Considerations

### Administrator Privileges

- Required for hosts file modification and process termination
- Application validates privileges before performing sensitive operations
- Graceful degradation when privileges are insufficient

### Hosts File Safety

- Automatic backup creation before modifications
- Restore functionality available
- Only modifies LaserFocus-specific sections

### Process Safety

- Whitelist-based approach (only terminates non-allowed processes)
- Protection against terminating critical system processes
- Comprehensive error handling and logging

## Support

### Getting Help

- Check log files for detailed error information
- Review this deployment guide for common solutions
- Ensure administrator privileges are granted

### Reporting Issues

Include the following information:

- Windows version and architecture
- Application version
- Relevant log file entries
- Steps to reproduce the issue
- Administrator privilege status
