using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using TechTalk.SpecFlow;

namespace LamdaTestSpecflowSelenium.Drivers {
    public class SeleniumDriver {
        private IWebDriver driver;

        private readonly ScenarioContext scenarioContext;

        public SeleniumDriver(ScenarioContext scenarioContext) {
            this.scenarioContext = scenarioContext;
        }

        public IWebDriver setUp() {
            var options = new OpenQA.Selenium.Chrome.ChromeOptions();
            driver = new RemoteWebDriver(new Uri("https://lambdatest.github.io/sample-todo-app/"), options.ToCapabilities(), TimeSpan.FromSeconds(600));
            scenarioContext["WebDriver"] = driver;
            driver.Manage().Window.Maximize();
            return driver;
        }
    }
}

