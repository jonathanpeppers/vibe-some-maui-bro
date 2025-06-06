# GitHub Copilot Development Environment Instructions

This document provides setup instructions for developing the **CatSwipe** .NET MAUI application with GitHub Copilot.

## Project Overview

CatSwipe is a .NET MAUI cross-platform mobile application that allows users to swipe through cat photos in a Tinder-like interface. The app is built with:

- **.NET 9**: Target framework
- **C# with nullable enabled**: Primary language
- **XAML**: UI markup
- **MAUI Controls**: Cross-platform UI framework
- **Target Platforms**: Android (primary), Windows (optional)

## Development Environment Setup

### Prerequisites

1. **Install .NET 9 SDK**
   - Follow the official guide: https://learn.microsoft.com/en-us/dotnet/core/install/linux
   - For Linux: Use the Microsoft package manager or download directly
   - Verify installation: `dotnet --version` (should show 9.0.x)

2. **Install .NET MAUI Workloads**
   ```bash
   dotnet workload install maui-android
   # Optional for Windows development:
   # dotnet workload install maui-windows
   ```

3. **Android Development Setup**
   - Follow MAUI Android setup: https://learn.microsoft.com/en-us/dotnet/maui/get-started/first-app?view=net-maui-9.0&tabs=vswin&pivots=devices-android
   - Install Android SDK and emulator if needed for testing

### Project Structure

```
VibeSomeMauiBro/
├── App.xaml & App.xaml.cs          # Application entry point
├── AppShell.xaml & AppShell.xaml.cs # Shell navigation structure
├── MainPage.xaml & MainPage.xaml.cs # Main app page
├── MauiProgram.cs                   # MAUI app configuration
├── VibeSomeMauiBro.csproj          # Project file with dependencies
├── Platforms/                       # Platform-specific code
│   ├── Android/                     # Android platform files
│   ├── iOS/                         # iOS platform files (if enabled)
│   ├── MacCatalyst/                 # macOS platform files
│   ├── Tizen/                       # Tizen platform files
│   └── Windows/                     # Windows platform files
└── Resources/                       # Application resources
    ├── AppIcon/                     # App icons
    ├── Fonts/                       # Custom fonts
    ├── Images/                      # Images and graphics
    ├── Raw/                         # Raw assets
    └── Splash/                      # Splash screen assets
```

### Building and Running

1. **Restore Dependencies**
   ```bash
   dotnet restore VibeSomeMauiBro/VibeSomeMauiBro.csproj
   ```

2. **Build the Project**
   ```bash
   dotnet build VibeSomeMauiBro/VibeSomeMauiBro.csproj --configuration Release
   ```

3. **Run on Android Emulator** (if available)
   ```bash
   dotnet build VibeSomeMauiBro/VibeSomeMauiBro.csproj -t:Run -f net9.0-android
   ```

### Key Development Notes

- The project uses **implicit usings** and **nullable reference types**
- Main application logic should be added to `MainPage.xaml` and `MainPage.xaml.cs`
- Platform-specific code goes in the respective `Platforms/` subdirectories
- Resources like images and fonts are managed through the `Resources/` directory
- The app targets **Android API 21+** and **Windows 10.0.17763.0+**

### Continuous Integration

The project includes a GitHub Actions workflow (`.github/workflows/build.yml`) that:
- Runs on Ubuntu with .NET 9
- Installs MAUI Android workloads
- Builds the project in Release configuration

### Future Development

**Note**: As development progresses, append additional setup instructions, coding patterns, architecture decisions, and development best practices to this document to help future contributors and GitHub Copilot understand the project context better.

## Useful Resources

- [GitHub Copilot Customization Guide](https://docs.github.com/en/copilot/customizing-copilot/customizing-the-development-environment-for-copilot-coding-agent)
- [.NET Core Installation](https://learn.microsoft.com/en-us/dotnet/core/install/linux)
- [.NET MAUI First App Tutorial](https://learn.microsoft.com/en-us/dotnet/maui/get-started/first-app?view=net-maui-9.0&tabs=vswin&pivots=devices-android)
- [.NET MAUI Documentation](https://learn.microsoft.com/en-us/dotnet/maui/)