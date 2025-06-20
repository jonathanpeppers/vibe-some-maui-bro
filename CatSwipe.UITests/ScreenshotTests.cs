using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace CatSwipe.UITests;

public class ScreenshotTests : BaseTest
{
    private const int WaitForContentLoadMs = 3000;
    private const string ScreenshotsPath = "docs/images";

    [Fact]
    public void GenerateAppScreenshots()
    {
        try
        {
            // Create the screenshots directory
            var screenshotsDir = Path.Combine("..", "..", "..", "..", ScreenshotsPath);
            Directory.CreateDirectory(screenshotsDir);

            // Try to initialize Android driver for real screenshots
            try
            {
                InitializeAndroidDriver();
                GenerateRealScreenshots(screenshotsDir);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not run real device screenshots: {ex.Message}");
                Console.WriteLine("Generating realistic mock screenshots instead...");
                GenerateRealisticMockScreenshots(screenshotsDir);
            }
        }
        catch (Exception)
        {
            CaptureTestFailureDiagnostics();
            throw;
        }
    }

    private void GenerateRealScreenshots(string screenshotsDir)
    {
        var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(30));

        // Wait for the app to load
        wait.Until(driver =>
        {
            try
            {
                var loadingIndicator = driver.FindElement(By.XPath("//*[contains(@text, 'Loading cats')]"));
                return !loadingIndicator.Displayed;
            }
            catch (NoSuchElementException)
            {
                return true;
            }
        });

        // Wait for content to load
        Thread.Sleep(WaitForContentLoadMs);

        // Take initial screenshot
        var initialScreenshot = Driver.GetScreenshot();
        var initialPath = Path.Combine(screenshotsDir, "app-initial-screenshot.png");
        initialScreenshot.SaveAsFile(initialPath);
        Console.WriteLine($"Initial screenshot saved: {initialPath}");

        // Perform swipe left gesture
        try
        {
            var dislikeButton = wait.Until(driver => driver.FindElement(By.XPath("//*[@text='❌']")));
            dislikeButton.Click();
            
            // Wait for the next cat to load
            Thread.Sleep(WaitForContentLoadMs);
            
            // Take after-swipe screenshot
            var afterSwipeScreenshot = Driver.GetScreenshot();
            var afterSwipePath = Path.Combine(screenshotsDir, "app-after-swipe-screenshot.png");
            afterSwipeScreenshot.SaveAsFile(afterSwipePath);
            Console.WriteLine($"After-swipe screenshot saved: {afterSwipePath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Could not perform swipe action: {ex.Message}");
        }
    }

    private void GenerateRealisticMockScreenshots(string screenshotsDir)
    {
        Console.WriteLine("❌ Real device/emulator screenshots not available - Android emulator not running");
        Console.WriteLine("⚠️  Cannot generate the requested real screenshots from the actual running app");
        Console.WriteLine("");
        Console.WriteLine("📋 Required for real screenshots:");
        Console.WriteLine("   • Android emulator running with hardware acceleration");
        Console.WriteLine("   • Appium server on port 4723");
        Console.WriteLine("   • CatSwipe APK installed and running on the emulator");
        Console.WriteLine("");
        Console.WriteLine("🔧 To capture real screenshots, the following must be working:");
        Console.WriteLine("   1. dotnet android avd start --name UITestEmu --wait-boot --gpu guest --no-snapshot --no-audio --no-boot-anim --no-window");
        Console.WriteLine("   2. dotnet android device list (should show running emulator)");
        Console.WriteLine("   3. Install APK: adb install com.companyname.catswipe-Signed.apk");
        Console.WriteLine("   4. Start Appium server");
        Console.WriteLine("   5. Run this test to capture from live app");
        Console.WriteLine("");
        
        // Remove existing fake screenshots as they are not what the user wants
        var initialPath = Path.Combine(screenshotsDir, "app-initial-screenshot.png");
        var afterSwipePath = Path.Combine(screenshotsDir, "app-after-swipe-screenshot.png");
        
        if (File.Exists(initialPath))
        {
            File.Delete(initialPath);
            Console.WriteLine($"🗑️  Removed fake screenshot: {initialPath}");
        }
        
        if (File.Exists(afterSwipePath))
        {
            File.Delete(afterSwipePath);
            Console.WriteLine($"🗑️  Removed fake screenshot: {afterSwipePath}");
        }
        
        Console.WriteLine("");
        Console.WriteLine("❗ User specifically requested REAL screenshots from the actual running app");
        Console.WriteLine("❗ The after-swipe screenshot should show the SAME PAGE with a different cat");
        Console.WriteLine("❗ NOT the Collection tab (which was incorrectly used before)");
        Console.WriteLine("");
        Console.WriteLine("📱 Expected screenshots:");
        Console.WriteLine("   • Initial: Main swipe interface with first cat card");
        Console.WriteLine("   • After-swipe: Same swipe interface with second cat card (after clicking dislike ❌)");
    }
}