using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace FidelityInsights.Pages
{
    /// <summary>
    /// Base class for all page objects in the test framework.
    /// Provides access to the Selenium WebDriver and a default WebDriverWait.
    /// </summary>
    public abstract class AbstractPage
    {
        /// <summary>
        /// The Selenium WebDriver instance for interacting with the browser.
        /// </summary>
        protected readonly IWebDriver Driver;

        /// <summary>
        /// A WebDriverWait instance with a default timeout of 10 seconds.
        /// Use this for explicit waits in page objects.
        /// </summary>
        protected readonly WebDriverWait Wait;

        /// <summary>
        /// Initializes the page object with the provided WebDriver.
        /// </summary>
        /// <param name="driver">The Selenium WebDriver instance.</param>
        protected AbstractPage(IWebDriver driver)
        {
            Driver = driver;
            Wait = new WebDriverWait(driver, TimeSpan.FromSeconds(25));
        }
    }
}
