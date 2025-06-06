# UI Tests for CatSwipe

This document describes how to run UI tests for the CatSwipe .NET MAUI application locally and in CI/CD.

## Overview

The UI tests are built using:
- **Appium WebDriver**: For automating mobile app interactions
- **xUnit**: Test framework for .NET
- **Android Emulator**: Target platform for testing

## Prerequisites

### Local Development

1. **.NET 9 SDK** with MAUI workload installed
   ```bash
   dotnet workload install maui --version 9.0.100
   ```

2. **Android SDK and Emulator**
   - Install Android Studio or use `dotnet android` tools
   - Create an Android emulator (API level 29+)

3. **Node.js and Appium**
   ```bash
   npm install -g appium
   appium driver install uiautomator2
   ```

4. **Java 11+** (required for Android development)

## Running UI Tests Locally

### Step 1: Build the Android App

First, build the Android APK:

```bash
# From the repository root (use Debug or Release as needed)
dotnet build VibeSomeMauiBro/VibeSomeMauiBro.csproj -f net9.0-android --configuration Debug
```

This will create a signed APK in `VibeSomeMauiBro/bin/Debug/net9.0-android/` (or the corresponding Release folder).

### Step 2: Start Android Emulator

Create and start an Android emulator:

```bash
# Using dotnet android tools
dotnet android sdk install --package 'system-images;android-33;default;x86_64'
dotnet android avd create --name UITestEmu --sdk 'system-images;android-33;default;x86_64' --force
dotnet android avd start --name UITestEmu --wait-boot --gpu guest --no-snapshot --no-audio --no-window
```

Or use Android Studio to create and start an emulator.

### Step 3: Install the App

Install the APK on the emulator:

```bash
# Find the APK file (works for both Debug and Release)
APK_PATH=$(find VibeSomeMauiBro/bin -name "*-Signed.apk" | head -1)

# Install using dotnet android
dotnet android device install --package "$APK_PATH"

# Or using adb directly
adb install "$APK_PATH"
```

### Step 4: Start Appium Server

Start the Appium server:

```bash
appium server --log-level info
```

Keep this running in a separate terminal.

### Step 5: Run the UI Tests

In another terminal, run the UI tests:

```bash
# Use the same configuration as used for building (Debug or Release)
dotnet test VibeSomeMauiBro.UITests/VibeSomeMauiBro.UITests.csproj --configuration Debug
```

## Test Structure

### BaseTest.cs

The `BaseTest` class provides common functionality:
- Android driver initialization
- APK path resolution
- Cleanup and disposal

### MainPageTests.cs

Contains the main UI tests:

1. **MainPage_Should_LoadCatImage**
   - Verifies that cat images load successfully on the main page
   - Checks for presence of ImageView elements
   - Ensures the app title is visible

2. **MainPage_Should_NotShowUnknownBreed**
   - Verifies that "Unknown Breed" text is not displayed
   - Ensures actual breed information is shown instead

## CI/CD Integration

The UI tests run automatically in GitHub Actions:

### Build Job
- Builds the Android APK on Windows
- Uploads APK as build artifact

### UI Test Job
- Runs on macOS with Android emulator
- Downloads the APK artifact
- Sets up Android emulator and Appium
- Installs the app and runs tests
- Uploads test results

## Adding New Tests

To add new UI tests:

1. Create new test methods in `MainPageTests.cs` or create new test classes
2. Follow the existing pattern using `BaseTest` for driver setup
3. Use Appium WebDriver API to interact with UI elements
4. Add appropriate assertions to verify expected behavior

### Example Test

```csharp
[Fact]
public void MainPage_Should_ShowLikeButton()
{
    // Arrange
    InitializeAndroidDriver();
    var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(30));

    // Act & Assert
    var likeButton = wait.Until(driver =>
        driver.FindElement(By.XPath("//*[contains(@text, '❤️')]")));
    
    Assert.NotNull(likeButton);
    Assert.True(likeButton.Displayed);
}
```

## Performance Considerations

- UI tests are slower than unit tests
- Run UI tests in parallel when possible
- Use appropriate timeouts to balance speed and reliability
- Consider test data management for consistent results

## Resources

- [Appium Documentation](http://appium.io/docs/)
- [.NET MAUI Testing Documentation](https://learn.microsoft.com/en-us/dotnet/maui/deployment/testing)
- [Android Testing Best Practices](https://developer.android.com/training/testing)