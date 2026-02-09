using NUnit;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;

namespace TestProject1;

public class Tests {

    IWebDriver driver;
    [SetUp]
    public void Setup() {
        driver = new ChromeDriver();
        driver.Manage().Window.Maximize();
    }

    [Test]
    public void Test1() {
        driver.Navigate().GoToUrl("http://localhost:5173/");
        driver.FindElement(By.ClassName("cardDesign")).Click();
        Thread.Sleep(2000);
    }

    [TearDown]
    public void TearDown() {
        driver.Dispose();
    }
}
