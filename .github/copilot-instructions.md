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

### Current Implementation Status

**CatSwipe App Architecture (as of current version):**

### Coding Standards

**Code Style Guidelines:**
- **String Literals**: Always use `""` instead of `string.Empty` for empty string initialization
- **Primary Constructors**: Use C# primary constructors for dependency injection where appropriate (C# 12+ feature)
- **Minimal Diffs**: Keep code changes as small as possible - avoid whitespace-only modifications to reduce review overhead
- **Property Change Notifications**: Implement `INotifyPropertyChanged` for data-bound properties that can change at runtime

#### Core Components Implemented:
- **Models/Cat.cs**: Data models for cat information and API responses
- **Services/CatService.cs**: HTTP service for fetching cat photos from The Cat API (https://thecatapi.com)
- **Views/CollectionPage.xaml(.cs)**: Grid view showing user's liked cats
- **MainPage.xaml(.cs)**: Main swipe interface with card stack

#### Key Features Working:
- ✅ **Cat Photo Fetching**: Integration with The Cat API for real cat photos
- ✅ **Swipe Interface**: Card-based UI with swipe animations
- ✅ **Touch Gestures**: Pan gesture recognizer for natural swipe interactions
- ✅ **Like/Dislike Actions**: Button and gesture-based cat selection
- ✅ **Collection Storage**: In-memory storage of liked cats
- ✅ **Navigation**: Tab-based navigation between swipe and collection screens
- ✅ **Responsive UI**: Works on Android (primary target platform)
- ✅ **Fallback Data**: Local cat images when API is unavailable

#### Technical Patterns Used:
- **Dependency Injection**: HttpClient and CatService registered in MauiProgram
- **Data Binding**: Basic data binding for collection view
- **Gesture Recognition**: PanGestureRecognizer for swipe detection
- **Async/Await**: Proper async patterns for API calls
- **Error Handling**: Try-catch with fallback data for offline scenarios

#### API Integration:
- Uses **The Cat API** (https://api.thecatapi.com/v1/images/search) - see [docs/CatApi.md](../docs/CatApi.md)
- Free tier: 10 requests per minute
- Includes breed information when available
- Automatic fallback to local images if API fails

#### Next Development Priorities:
1. **Touch Improvements**: Better swipe thresholds and feedback
2. **Persistent Storage**: SQLite integration for liked cats
3. **Enhanced UI**: Better animations, loading states, and visual polish
4. **Network Management**: Offline mode and better error handling
5. **Performance**: Image caching and smooth scrolling optimizations

## Useful Resources

- [GitHub Copilot Customization Guide](https://docs.github.com/en/copilot/customizing-copilot/customizing-the-development-environment-for-copilot-coding-agent)
- [.NET Core Installation](https://learn.microsoft.com/en-us/dotnet/core/install/linux)
- [.NET MAUI First App Tutorial](https://learn.microsoft.com/en-us/dotnet/maui/get-started/first-app?view=net-maui-9.0&tabs=vswin&pivots=devices-android)
- [.NET MAUI Documentation](https://learn.microsoft.com/en-us/dotnet/maui/)