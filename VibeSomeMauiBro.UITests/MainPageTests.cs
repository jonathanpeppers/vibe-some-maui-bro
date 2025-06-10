using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace VibeSomeMauiBro.UITests;

public class MainPageTests : BaseTest
{
    private const int WaitForContentLoadMs = 2000;
    [Fact]
    public void App_Should_LaunchWithoutCrashing()
    {
        // Arrange & Act
        InitializeAndroidDriver();
        
        // Give the app a moment to fully initialize
        Thread.Sleep(3000);
        
        // Assert - If we get here without an exception, the app launched successfully
        Assert.NotNull(Driver);
        Assert.True(Driver.SessionId != null, "Driver should have a valid session");
        
        // Verify the driver is responsive by getting the current activity
        var currentActivity = Driver.CurrentActivity;
        Assert.NotNull(currentActivity);
    }

    [Fact]
    public void MainPage_Should_NotShowUnknownBreed()
    {
        // Arrange
        InitializeAndroidDriver();
        var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(30));

        // Wait for the loading to complete
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

        // Wait a bit more for the cat card content to load
        Thread.Sleep(WaitForContentLoadMs);

        // Act & Assert - Check that "Unknown Breed" text is not displayed
        try
        {
            var unknownBreedElements = Driver.FindElements(By.XPath("//*[contains(@text, 'Unknown Breed')]"));
            
            // Assert that no "Unknown Breed" text is found, or if found, it's not displayed
            foreach (var element in unknownBreedElements)
            {
                Assert.False(element.Displayed, "The text 'Unknown Breed' should not be visible on the main page");
            }
        }
        catch (NoSuchElementException)
        {
            // This is actually good - it means "Unknown Breed" text was not found at all
            // Test passes
        }

        // Additional verification: ensure some breed text is actually present
        // Look for text elements that could contain breed information
        var textElements = Driver.FindElements(By.ClassName("android.widget.TextView"));
        var hasBreedInfo = textElements.Any(element => 
        {
            var text = element.Text;
            return !string.IsNullOrEmpty(text) && 
                   !text.Contains("Loading") && 
                   !text.Contains("CatSwipe") &&
                   !text.Contains("Unknown Breed") &&
                   text.Length > 2; // Some actual breed name or description
        });

        Assert.True(hasBreedInfo, "Expected to find some breed information displayed instead of 'Unknown Breed'");
    }
}