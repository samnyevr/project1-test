using FidelityInsights.Support;
using OpenQA.Selenium;

namespace FidelityInsights.Pages
{
    /// <summary>
    /// Minimal page object for Training Sessions page.
    /// Update the URL token + locator once the actual page markup is known.
    /// </summary>
    public sealed class TrainingSessionsPage : AbstractPage
    {
        // Update if your real route differs.
        private const string SessionsUrlToken = "/sessions";

        private static readonly By PageRootBy =
            By.CssSelector("main, app-training-sessions, app-sessions");

        public TrainingSessionsPage(DriverContext ctx) : base(ctx.Driver) { }

        public bool IsAt()
        {
            var urlOk = (Driver.Url ?? string.Empty).Contains(SessionsUrlToken, StringComparison.OrdinalIgnoreCase);
            var rootOk = Driver.FindElements(PageRootBy).Any(e => e.Displayed);
            return urlOk && rootOk;
        }

        public void WaitForLoaded(int seconds = 25)
        {
            Wait.Timeout = TimeSpan.FromSeconds(seconds);
            Wait.Until(_ => IsAt());
        }
    }
}
