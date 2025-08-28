# Technology Stack

## Framework & Platform

- **Platform**: Windows Desktop Application
- **Framework**: WPF (Windows Presentation Foundation)
- **Language**: C#
- **Target Framework**: .NET (modern version recommended)

## Architecture Pattern

- **UI Pattern**: Code-behind with data binding
- **Data Binding**: ObservableCollection for real-time UI updates
- **Threading**: DispatcherTimer for periodic operations
- **Configuration**: JSON serialization for persistent storage

## Core Technologies

### System Integration

- **Hosts File Management**: Direct Windows hosts file manipulation (`C:\Windows\System32\drivers\etc\hosts`)
- **Process Management**: Windows Process API for enumeration and termination
- **Privilege Management**: UAC (User Account Control) integration for administrator elevation

### Data Management

- **Configuration Storage**: JSON files for blocked websites and allowed applications
- **Real-time Updates**: INotifyPropertyChanged interface for UI binding
- **Collections**: ObservableCollection for dynamic list management

## Common Commands

### Development

```cmd
# Build the application
dotnet build

# Run the application
dotnet run

# Run with administrator privileges (required for full functionality)
# Right-click executable -> "Run as administrator"
```

### Testing

```cmd
# Run unit tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

### Deployment

```cmd
# Publish for Windows
dotnet publish -c Release -r win-x64 --self-contained

# Create installer package (if using WiX or similar)
# Specific commands depend on chosen installer technology
```

## Key Dependencies

- System.Collections.ObjectModel (ObservableCollection)
- System.ComponentModel (INotifyPropertyChanged)
- System.Diagnostics (Process management)
- System.IO (File operations)
- System.Text.Json (Configuration serialization)
- Windows.UI.Xaml (WPF UI components)

## Performance Considerations

- **Timer Interval**: 2-second refresh for process monitoring
- **Memory Management**: Proper disposal of Process objects
- **UI Thread**: Use Dispatcher for cross-thread UI updates
- **File I/O**: Batch operations on hosts file to minimize system impact
