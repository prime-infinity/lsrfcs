# Product Overview

## Laser Focus (lsrfcs) Productivity App

A Windows desktop application designed to help developers maintain focus by providing system-wide website blocking and automatic application management.

### Core Features

- **System-wide website blocking** - Blocks distracting websites across all browsers (including incognito mode) by modifying the Windows hosts file
- **Application monitoring** - Automatically closes non-essential applications while allowing development tools (VS Code, Chrome, Kiro) to run
- **Simple on/off control** - Single toggle to enable/disable all monitoring features
- **Real-time process monitoring** - Live view of running processes with allow/block status indicators
- **Clean, minimal interface** - Focused UI design to minimize distractions

### Target Users

Primary users are developers who need to maintain focus during work sessions by eliminating digital distractions.

### Key Requirements

- Requires administrator privileges for hosts file modification and process termination
- Windows desktop application (WPF-based)
- Real-time monitoring with 2-second refresh intervals
- Persistent configuration storage for blocked websites and allowed applications
