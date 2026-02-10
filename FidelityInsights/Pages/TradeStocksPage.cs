using FidelityInsights.Pages.Components;
using FidelityInsights.Support;
using OpenQA.Selenium;

namespace FidelityInsights.Pages
{
    /// <summary>
    /// Minimal page object for Trade Stocks.
    /// Only contains what navigation tests need:
    /// - "IsAt" check (URL + a light locator)
    /// - "WaitForLoaded" gate
    /// - ability to navigate back to Portfolio via the shared nav component
    /// </summary>
    public sealed class TradeStocksPage : AbstractPage
    {
        // Update if your real route differs.
        private const string TradeUrlToken = "/trade";

        // Choose a stable element from your trade page (replace if needed).
        private static readonly By PageRootBy =
            By.CssSelector("main, app-trade, app-trade-stocks");

        private PrimaryNavigationBar Nav => new PrimaryNavigationBar(Driver);

        public TradeStocksPage(DriverContext ctx) : base(ctx.Driver) { }

        public bool IsAt()
        {
            var urlOk = (Driver.Url ?? string.Empty).Contains(TradeUrlToken, StringComparison.OrdinalIgnoreCase);
            var rootOk = Driver.FindElements(PageRootBy).Any(e => e.Displayed);
            return urlOk && rootOk;
        }

        public void WaitForLoaded(int seconds = 10)
        {
            Wait.Timeout = TimeSpan.FromSeconds(seconds);
            Wait.Until(_ => IsAt());
        }

        public void NavigateToPortfolioOverviewFromHeader() => Nav.GoToPortfolioOverview();
    }
}
