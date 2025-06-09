using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;

namespace VibeSomeMauiBro.UITests;

public abstract class BaseTest : IDisposable
{
    protected AndroidDriver Driver { get; private set; } = null!;

    protected void InitializeAndroidDriver()
    {
        var options = new AppiumOptions();
        
        // Basic Android capabilities
        options.AddAdditionalAppiumOption("platformName", "Android");
        options.AutomationName = "UiAutomator2";
        options.AddAdditionalAppiumOption("app", GetAndroidAppPath());
        options.AddAdditionalAppiumOption("appium:newCommandTimeout", 300);
        options.AddAdditionalAppiumOption("appium:connectHardwareKeyboard", true);

        // Create driver with default Appium server URL
        var serverUri = new Uri("http://127.0.0.1:4723");
        Driver = new AndroidDriver(serverUri, options);
    }

    private static string GetAndroidAppPath()
    {
        // Look for the APK in the expected location
        var currentDirectory = Directory.GetCurrentDirectory();
        
        // Look for APK in the artifacts directory (for CI)
        var artifactsPath = Path.Combine(currentDirectory, "artifacts");
        if (Directory.Exists(artifactsPath))
        {
            var apkFiles = Directory.GetFiles(artifactsPath, "*.apk", SearchOption.AllDirectories);
            if (apkFiles.Length > 0)
            {
                return apkFiles[0];
            }
        }

        // Look for APK relative to the test project (for local testing)
        var configurations = new[] { "Debug", "Release" };
        foreach (var config in configurations)
        {
            var relativePath = Path.Combine(currentDirectory, "..", "VibeSomeMauiBro", "bin", config, "net9.0-android");
            if (Directory.Exists(relativePath))
            {
                var apkFiles = Directory.GetFiles(relativePath, "*-Signed.apk", SearchOption.AllDirectories);
                if (apkFiles.Length > 0)
                {
                    return apkFiles[0];
                }
            }
        }

        throw new FileNotFoundException("Could not find the Android APK file. Make sure the app is built and the APK is available.");
    }

    public void Dispose()
    {
        Driver?.Quit();
        GC.SuppressFinalize(this);
    }
}