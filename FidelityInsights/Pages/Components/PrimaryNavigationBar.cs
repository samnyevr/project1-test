using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;

namespace FidelityInsights.Pages.Components
{
    /// <summary>
    /// Component Page Object for the app's primary navigation (header / global nav).
    ///
    /// Why this exists:
    /// - Navigation is used by multiple features/pages and should not be duplicated in each Page Object.
    /// - Nav markup is often stable while page content changes; isolating it reduces churn.
    ///
    /// Design:
    /// - Exposes intent-based methods (GoToTradeStocks) rather than “click CSS selector”.
    /// - Uses tolerant selectors: first try explicit routerLink targets, then fall back to visible link text.
    /// </summary>
    public sealed class PrimaryNavigationBar
    {
        private readonly IWebDriver _driver;
        private readonly WebDriverWait _wait;

        public PrimaryNavigationBar(IWebDriver driver, TimeSpan? timeout = null)
        {
            _driver = driver;
            _wait = new WebDriverWait(_driver, timeout ?? TimeSpan.FromSeconds(25));
        }

        // ---------------------------------------------------------------------
        // Locators
        // ---------------------------------------------------------------------
        //
        // NOTE: We intentionally keep these flexible. Your UI may render:
        // - <a routerLink="/trade">Trade</a>
        // - <a href="/trade">Trade</a>
        // - a button that behaves like a link
        //
        // The XPath below matches either routerLink or href, and then falls back
        // to a visible text match.
        //

        private static By LinkByRouteOrText(string route, params string[] texts)
        {
            // route match: routerLink or href contains the route
            var routeXpath =
                $"//a[(contains(@routerLink,'{route}') or contains(@href,'{route}'))]";

            // text match: normalize-space() equals one of the provided strings
            var textParts = texts
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .Select(t => $"normalize-space(.)='{t}'");

            var textXpath = textParts.Any()
                ? $"//a[{string.Join(" or ", textParts)}]"
                : string.Empty;

            // Prefer route match; if not found, try text match.
            var combined = string.IsNullOrWhiteSpace(textXpath)
                ? routeXpath
                : $"({routeXpath}) | ({textXpath})";

            return By.XPath(combined);
        }

        // Common destinations used by the feature(s)
        private static readonly By TradeStocksLinkBy =
            LinkByRouteOrText("/trade", "Trade", "Trade Stocks");

        private static readonly By TrainingSessionsLinkBy =
            LinkByRouteOrText("/sessions", "Training Sessions", "Sessions");

        private static readonly By PortfolioOverviewLinkBy =
            LinkByRouteOrText("/portfolio/overview", "Portfolio", "Overview");

        // ---------------------------------------------------------------------
        // Public API
        // ---------------------------------------------------------------------

        public void GoToTradeStocks() => ClickNavLink(TradeStocksLinkBy, "Trade Stocks");
        public void GoToTrainingSessions() => ClickNavLink(TrainingSessionsLinkBy, "Training Sessions");
        public void GoToPortfolioOverview() => ClickNavLink(PortfolioOverviewLinkBy, "Portfolio Overview");

        // ---------------------------------------------------------------------
        // Private helpers
        // ---------------------------------------------------------------------

        private void ClickNavLink(By by, string logicalName)
        {
            var el = _wait.Until(d =>
            {
                var candidate = d.FindElements(by).FirstOrDefault(e => e.Displayed && e.Enabled);
                return candidate;
            });

            if (el == null)
                throw new NoSuchElementException($"Primary navigation link not found for: {logicalName}");

            _wait.Until(ExpectedConditions.ElementToBeClickable(el)).Click();
        }
    }
}
