using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;

namespace TestProject1;

public class Tests {
    private IWebDriver driver = null!;   // <- removes nullable warning

    [SetUp]
    public void Setup() {
        var options = new ChromeOptions();
        options.AddArgument("--headless=new");   // IMPORTANT for GitHub Actions
        options.AddArgument("--disable-gpu");
        options.AddArgument("--window-size=1920,1080");
        options.AddArgument("--no-sandbox");

        driver = new ChromeDriver(options);
    }

    [Test]
    public void Test1() {
        driver.Navigate().GoToUrl("http://localhost:5173/");

        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

        var card = wait.Until(ExpectedConditions
            .ElementToBeClickable(By.ClassName("cardDesign")));

        card.Click();
    }

    [TearDown]
    public void TearDown() {
        driver.Quit();   // better than Dispose()
        driver.Dispose();
    }
}
