using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;

namespace FidelityInsights.Pages
{
    public class HeaderNavBar
    {
        private readonly IWebDriver _driver;
        private readonly WebDriverWait _wait;

        public HeaderNavBar(IWebDriver driver, int seconds = 10)
        {
            _driver = driver;
            _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(seconds));
        }

        private static readonly By NavRootBy = By.CssSelector("nav.nav-bar, header nav");
        private static By NavLinkBy(string text) =>
            By.XPath($"//nav[contains(@class,'nav-bar')]//a[contains(@class,'nav-link') and normalize-space()='{text}']");

        public void GoToPortfolio() => ClickNav("Portfolio");
        public void GoToTradeStocks() => ClickNav("Trade Stocks");
        public void GoToTrainingSessions() => ClickNav("View Training Sessions");

        private void ClickNav(string label)
        {
            _wait.Until(d => d.FindElements(NavRootBy).Any());
            var link = _wait.Until(ExpectedConditions.ElementToBeClickable(NavLinkBy(label)));
            link.Click();
        }
    }
}
