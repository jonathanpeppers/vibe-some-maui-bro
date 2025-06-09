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
        string packageName = "com.companyname.vibesomemauibro";
        options.AddAdditionalAppiumOption("appPackage", packageName);
        options.AddAdditionalAppiumOption("appActivity", $"{packageName}.MainActivity");
        options.AddAdditionalAppiumOption("appium:newCommandTimeout", 300);
        options.AddAdditionalAppiumOption("appium:connectHardwareKeyboard", true);

        // Create driver with default Appium server URL
        var serverUri = new Uri("http://127.0.0.1:4723");
        Driver = new AndroidDriver(serverUri, options);
    }

    public void Dispose()
    {
        Driver?.Quit();
        GC.SuppressFinalize(this);
    }
}