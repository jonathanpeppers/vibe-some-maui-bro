using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;
using System.Diagnostics;

namespace VibeSomeMauiBro.UITests;

public abstract class BaseTest : IDisposable
{
    protected AndroidDriver Driver { get; private set; } = null!;
    
    public string PackageName { get; } = "com.companyname.vibesomemauibro";
    public string ActivityName { get; } = "com.companyname.vibesomemauibro.MainActivity";
    
    private static readonly string ArtifactsPath = Path.Combine(Environment.CurrentDirectory, "test-artifacts");
    
    static BaseTest()
    {
        // Ensure artifacts directory exists
        Directory.CreateDirectory(ArtifactsPath);
    }

    protected void InitializeAndroidDriver()
    {
        var options = new AppiumOptions();
        
        // Basic Android capabilities
        options.AddAdditionalAppiumOption("platformName", "Android");
        options.AutomationName = "UiAutomator2";
        options.AddAdditionalAppiumOption("appPackage", PackageName);
        options.AddAdditionalAppiumOption("appActivity", ActivityName);
        options.AddAdditionalAppiumOption("appium:newCommandTimeout", 300);
        options.AddAdditionalAppiumOption("appium:connectHardwareKeyboard", true);

        // Create driver with default Appium server URL
        var serverUri = new Uri("http://127.0.0.1:4723");
        Driver = new AndroidDriver(serverUri, options);
    }

    protected void CaptureTestFailureDiagnostics(string testName)
    {
        var timestamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
        var testArtifactDir = Path.Combine(ArtifactsPath, $"{testName}-{timestamp}");
        Directory.CreateDirectory(testArtifactDir);
        
        try
        {
            // Capture screenshot
            CaptureScreenshot(testArtifactDir, testName);
            
            // Capture logcat output
            CaptureLogcat(testArtifactDir, testName);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to capture diagnostics for test {testName}: {ex.Message}");
        }
    }
    
    private void CaptureScreenshot(string artifactDir, string testName)
    {
        try
        {
            if (Driver?.SessionId != null)
            {
                var screenshot = Driver.GetScreenshot();
                var screenshotPath = Path.Combine(artifactDir, $"{testName}-screenshot.png");
                screenshot.SaveAsFile(screenshotPath);
                Console.WriteLine($"Screenshot saved: {screenshotPath}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to capture screenshot: {ex.Message}");
        }
    }
    
    private void CaptureLogcat(string artifactDir, string testName)
    {
        try
        {
            var logcatPath = Path.Combine(artifactDir, $"{testName}-logcat.txt");
            
            // Run adb logcat command to capture recent logs
            var processStartInfo = new ProcessStartInfo
            {
                FileName = "adb",
                Arguments = "logcat -d -v time",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            
            using var process = Process.Start(processStartInfo);
            if (process != null)
            {
                var output = process.StandardOutput.ReadToEnd();
                var error = process.StandardError.ReadToEnd();
                process.WaitForExit();
                
                if (process.ExitCode == 0)
                {
                    File.WriteAllText(logcatPath, output);
                    Console.WriteLine($"Logcat saved: {logcatPath}");
                }
                else
                {
                    Console.WriteLine($"adb logcat failed with exit code {process.ExitCode}: {error}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to capture logcat: {ex.Message}");
        }
    }
    
    public void Dispose()
    {
        Driver?.Quit();
    }
}