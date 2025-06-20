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

4. **Android Emulator Setup for Screenshots and UI Testing**
   ```bash
   # Install Android SDK tools
   dotnet tool install -g androidsdk.tool
   
   # Install system images for emulator
   android sdk install --package 'system-images;android-33;default;x86_64'
   
   # Create Android Virtual Device (AVD)
   android avd create --name UITestEmu --sdk 'system-images;android-33;default;x86_64' --force
   
   # Start emulator (requires hardware acceleration/KVM on Linux)
   # Important: If exit code is 0, treat as successful - ignore stdout/stderr messages unless failing exit code
   dotnet android avd start --name UITestEmu --wait-boot --gpu guest --no-snapshot --no-audio --no-boot-anim --no-window
   
   # List running devices to verify emulator is running
   dotnet android device list
   ```
   
   **Note**: Android emulator requires hardware acceleration (KVM) on Linux. In environments without KVM support, consider alternative approaches for screenshot generation or use cloud-based device testing services.

### Project Structure

```
CatSwipe/
├── App.xaml & App.xaml.cs          # Application entry point
├── AppShell.xaml & AppShell.xaml.cs # Shell navigation structure
├── MainPage.xaml & MainPage.xaml.cs # Main app page
├── MauiProgram.cs                   # MAUI app configuration
├── CatSwipe.csproj          # Project file with dependencies
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
   dotnet restore CatSwipe/CatSwipe.csproj
   ```

2. **Format Code** (always run before pushing changes)
   ```bash
   dotnet format cat-swipe.sln
   ```

3. **Build the Project**
   ```bash
   dotnet build CatSwipe/CatSwipe.csproj --configuration Release
   ```

4. **Run on Android Emulator** (if available)
   ```bash
   dotnet build CatSwipe/CatSwipe.csproj -t:Run -f net9.0-android
   ```

### Key Development Notes

- The project uses **implicit usings** and **nullable reference types**
- Main application logic should be added to `MainPage.xaml` and `MainPage.xaml.cs`
- Platform-specific code goes in the respective `Platforms/` subdirectories
- Resources like images and fonts are managed through the `Resources/` directory
- The app targets **Android API 21+** and **Windows 10.0.17763.0+**

### Continuous Integration

The project includes a GitHub Actions workflow (`.github/workflows/build.yml`) that:
- Runs on Windows and macOS with .NET 9
- Installs MAUI workloads for multiple platforms
- Builds the project in Release configuration with MSBuild binary logging
- Collects and uploads build artifacts including:
  - **MSBuild Binary Logs** (`.binlog` files) for build diagnostics
  - **Android APK/AAB** files for distribution
  - **Windows unpackaged apps** for sideloading
  - **iOS IPA** files (when building on macOS)
  - **macOS .app** bundles (when building on macOS)
- Artifacts are saved even on build failure for debugging
- Artifacts are retained for 30 days

**Build Artifacts**: The workflow captures all relevant build outputs as GitHub Actions artifacts, making it easy to download and test builds from any commit or pull request.
- Installs MAUI Android workloads
- Uses conditional build configuration:
  - **Pull Requests**: Build in Debug mode (faster CI feedback)
  - **Main branch pushes, release tags**: Build in Release mode (optimized for deployment)

### Future Development

**Note**: As development progresses, append additional setup instructions, coding patterns, architecture decisions, and development best practices to this document to help future contributors and GitHub Copilot understand the project context better.

### Current Implementation Status

**CatSwipe App Architecture (as of current version):**

### Testing

**Test Projects:**
- **CatSwipe.Tests**: Unit tests for services and models using xUnit
- **CatSwipe.UITests**: UI/integration tests using Appium for Android emulator testing

**Testing Commands:**
```bash
# Run unit tests
dotnet test CatSwipe.Tests/CatSwipe.Tests.csproj

# Run UI tests (requires Android emulator and Appium server)
dotnet test CatSwipe.UITests/CatSwipe.UITests.csproj
```

**UI Test Requirements:**
- Android emulator running (API level 29+)
- Appium server running on port 4723
- UiAutomator2 driver installed
- APK built and available for testing

See [docs/UITests.md](../docs/UITests.md) for detailed UI testing setup and usage instructions.

### Coding Standards

**Code Style Guidelines:**
- **Code Formatting**: Always run `dotnet format cat-swipe.sln` before committing changes to maintain consistent code style across the codebase
- **String Literals**: Always use `""` instead of `string.Empty` for empty string initialization
- **Primary Constructors**: Use C# primary constructors for dependency injection where appropriate (C# 12+ feature)
- **Minimal Diffs**: Keep code changes as small as possible - avoid whitespace-only modifications to reduce review overhead. **Always make the smallest possible diff** - change only the specific lines needed to fix the issue, never modify unrelated files or make formatting-only changes
- **Property Change Notifications**: Use `BindableObject.OnPropertyChanged()` for MAUI pages/views - BindableObject already implements `INotifyPropertyChanged`
- **Collections Performance**: Use `HashSet<T>` instead of `List<T>` for membership testing and duplicate prevention - convert to `List<T>` only when needed for serialization
- **JSON Serialization**: Use System.Text.Json source generators for better performance - see [Microsoft docs](https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/source-generation)
- **File I/O Efficiency**: Use `File.OpenRead()` and `File.Create()` instead of manually creating `FileStream` objects for better readability and performance
- **Stream-based JSON**: Always use `JsonSerializer.DeserializeAsync()` and `JsonSerializer.SerializeAsync()` with file streams - never load entire file content into memory as strings
- **Error Logging**: Use `ILogger` for proper error logging in real applications - log warnings for recoverable errors and errors for critical failures
- **XAML Compiled Bindings**: Always use compiled bindings with `x:DataType` to improve runtime performance and enable compile-time binding validation - see [Microsoft docs](https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/data-binding/compiled-bindings?view=net-maui-9.0)
- **Warnings as Errors**: Project configured to treat all warnings as errors via `Directory.Build.props` to ensure code quality

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
- ✅ **Collection Storage**: JSON file-based persistent storage of liked cats with cross-session persistence
- ✅ **Navigation**: Tab-based navigation between swipe and collection screens
- ✅ **Responsive UI**: Works on Android (primary target platform)
- ✅ **Fallback Data**: Local cat images when API is unavailable
- ✅ **UI Testing**: Automated tests using Appium for Android emulator

#### Technical Patterns Used:
- **Dependency Injection**: HttpClient and CatService registered in MauiProgram with ILogger support
- **Data Binding**: Basic data binding for collection view
- **Gesture Recognition**: PanGestureRecognizer for swipe detection
- **Async/Await**: Proper async patterns for API calls and file I/O operations
- **Error Handling**: Try-catch with fallback data for offline scenarios and graceful file I/O error handling
- **File Persistence**: JSON file storage using System.Text.Json source generators with cross-platform file paths

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