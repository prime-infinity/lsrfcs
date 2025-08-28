# Project Structure

## Recommended Directory Layout

```
lsrfcs-productivity-app/
├── src/
│   ├── LaserFocus/                    # Main WPF application
│   │   ├── MainWindow.xaml            # Primary UI layout
│   │   ├── MainWindow.xaml.cs         # UI code-behind and event handlers
│   │   ├── App.xaml                   # Application configuration
│   │   ├── App.xaml.cs                # Application startup logic
│   │   └── LaserFocus.csproj          # Project file
│   ├── LaserFocus.Core/               # Core business logic
│   │   ├── Models/
│   │   │   ├── ProcessInfo.cs         # Process data model with INotifyPropertyChanged
│   │   │   ├── AppSettings.cs         # Application configuration model
│   │   │   └── BlockedWebsite.cs      # Website blocking data model
│   │   ├── Services/
│   │   │   ├── HostsFileManager.cs    # Website blocking via hosts file
│   │   │   ├── ProcessMonitor.cs      # Application monitoring and termination
│   │   │   └── ConfigurationManager.cs # Persistent storage management
│   │   └── LaserFocus.Core.csproj
│   └── LaserFocus.Tests/              # Unit and integration tests
│       ├── Services/
│       │   ├── HostsFileManagerTests.cs
│       │   ├── ProcessMonitorTests.cs
│       │   └── ConfigurationManagerTests.cs
│       ├── Models/
│       │   └── ProcessInfoTests.cs
│       └── LaserFocus.Tests.csproj
├── config/                            # Configuration files
│   ├── blocked-websites.json          # Persistent blocked websites list
│   ├── allowed-applications.json      # Persistent allowed applications list
│   └── app-settings.json             # Application preferences
├── docs/                             # Documentation
└── LaserFocus.sln                    # Solution file
```

## Component Organization

### UI Layer (`LaserFocus/`)

- **MainWindow.xaml**: Primary application interface with data binding
- **MainWindow.xaml.cs**: Event handlers, timer management, UI logic
- **App.xaml.cs**: Application startup, privilege checking, initialization

### Core Logic (`LaserFocus.Core/`)

- **Models/**: Data classes with proper change notification
- **Services/**: Business logic components for system integration
- Separation of concerns between UI and system operations

### Testing (`LaserFocus.Tests/`)

- Unit tests for each service and model class
- Mocked dependencies for file system and process operations
- Integration tests for end-to-end functionality

## Key Architectural Principles

### Separation of Concerns

- UI layer handles only presentation and user interaction
- Core services handle system integration (hosts file, processes)
- Models contain data and change notification logic

### Data Binding Pattern

- ObservableCollection properties in MainWindow for real-time updates
- INotifyPropertyChanged implementation in all data models
- XAML binding expressions for automatic UI synchronization

### Configuration Management

- JSON files in dedicated config directory
- Centralized ConfigurationManager for all persistence operations
- Default configuration initialization on first run

### Error Handling Strategy

- Try-catch blocks around all file and process operations
- User-friendly error messages in UI
- Graceful degradation when administrator privileges unavailable
- Comprehensive logging for debugging

## File Naming Conventions

- **Classes**: PascalCase (e.g., `HostsFileManager.cs`)
- **Methods**: PascalCase (e.g., `BlockWebsite()`)
- **Properties**: PascalCase (e.g., `ProcessName`)
- **Fields**: camelCase with underscore prefix (e.g., `_allowedProcesses`)
- **Configuration Files**: kebab-case (e.g., `blocked-websites.json`)

## Dependencies and References

- Core project references only system libraries
- UI project references Core project
- Test project references both Core and UI projects
- Minimal external dependencies to reduce complexity
