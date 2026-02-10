using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using FidelityInsights.Support;

namespace FidelityInsights.Pages
{
    public class TrainingSessionHistoryPage : AbstractPage
    {
        private const string Url = "https://d2rczu3zvi71ix.cloudfront.net/simulation-history";

        public TrainingSessionHistoryPage(DriverContext ctx) : base(ctx.Driver) { }

        public void Open() => Driver.Navigate().GoToUrl(Url);

        // Root
        private static readonly By RootBy =
            By.CssSelector("section.simulation-history");

        // Paused table
        private static readonly By PausedTableBy =
            By.CssSelector("section.simulation-history .history-card h2 + .table-shell table");

        private static readonly By PausedRowsBy =
            By.CssSelector("section.simulation-history .history-card h2 + .table-shell table tbody tr");

        // Row menu
        private static readonly By RowMenuButtonBy =
            By.CssSelector("td.actions button.menu-button[aria-label='Open simulation actions']");

        private static readonly By RowMenuCardBy =
            By.CssSelector("td.actions .menu-card");

        private static readonly By MenuDeleteButtonBy =
            By.XPath(".//button[normalize-space(.)='Delete' or .//*[normalize-space(.)='Delete']]");

        public void DeletePausedSessionByStartingBalance(string startingBalanceUi)
        {
            Wait.Until(ExpectedConditions.ElementExists(RootBy));
            Wait.Until(d => d.FindElements(PausedTableBy).Any());

            // Find the paused row whose 4th cell is "Starting Balance"
            var row = FindPausedRowByStartingBalance(startingBalanceUi);
            if (row == null)
                return; // teardown should be non-fatal

            // Open the kebab menu for that row
            var menuBtn = row.FindElements(RowMenuButtonBy).FirstOrDefault();
            if (menuBtn == null)
                throw new NoSuchElementException("Menu button not found for the matching paused simulation row.");

            Wait.Until(ExpectedConditions.ElementToBeClickable(menuBtn)).Click();

            // Ensure menu card is visible within this row
            Wait.Until(_ =>
            {
                try
                {
                    var card = row.FindElements(RowMenuCardBy).FirstOrDefault();
                    return card != null && card.Displayed;
                }
                catch (StaleElementReferenceException) { return false; }
            });

            var menuCard = row.FindElements(RowMenuCardBy).First(e => e.Displayed);

            // Click Delete in the menu card
            var deleteBtn = menuCard.FindElements(MenuDeleteButtonBy).FirstOrDefault();
            if (deleteBtn == null)
                throw new NoSuchElementException("Delete button not found in the row menu card.");

            Wait.Until(ExpectedConditions.ElementToBeClickable(deleteBtn)).Click();

            // Wait for row to disappear (Angular removes it from pausedSimulations)
            var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(10));
            wait.Until(_ => FindPausedRowByStartingBalance(startingBalanceUi) == null);
        }

        private IWebElement? FindPausedRowByStartingBalance(string startingBalanceUi)
        {
            var rows = Driver.FindElements(PausedRowsBy);

            foreach (var r in rows)
            {
                try
                {
                    if (!r.Displayed) continue;

                    var cells = r.FindElements(By.CssSelector("td"));
                    // Skip loading/empty rows (they have colspan)
                    if (cells.Count < 7) continue;

                    // Starting Balance is 4th column in both tables (index 3)
                    var startingBalanceCellText = cells[3].Text.Trim();
                    if (string.Equals(startingBalanceCellText, startingBalanceUi.Trim(), StringComparison.OrdinalIgnoreCase))
                        return r;
                }
                catch (StaleElementReferenceException)
                {
                    // retry by continuing
                }
            }

            return null;
        }
    }
}
