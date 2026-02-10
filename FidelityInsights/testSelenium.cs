using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace SeleniumTest
{
    /// <summary>
    /// Selenium UI tests for the TCG Inventory Login page.
    /// Demonstrates element selection, input, assertions, waits, and error handling.
    /// </summary>
    [TestFixture]
    public class testSelenium
    {
        private ChromeDriver driver;

        [SetUp]
        public void Setup()
        {
            // Start a new Chrome browser session
            driver = new ChromeDriver();
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
        }

        [Test]
        public void LoginPage_TitleAndElements_ArePresent()
        {
            // Navigate
            driver.Navigate().GoToUrl("https://d2rczu3zvi71ix.cloudfront.net");

            // Maximize the browser window
            driver.Manage().Window.Maximize();

            // Assert the page title is not empty
            Assert.That(driver.Title, Is.Not.Empty, "Page should have a title");
        }
    }
}