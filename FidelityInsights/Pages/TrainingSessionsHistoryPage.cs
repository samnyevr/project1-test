using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using FidelityInsights.Support;

namespace FidelityInsights.Pages
{
    public class TrainingSessionHistoryPage : AbstractPage
    {
        private const string Url = "https://d2rczu3zvi71ix.cloudfront.net/session-history";

        public TrainingSessionHistoryPage(DriverContext ctx) : base(ctx.Driver) { }

        public void Open() => Driver.Navigate().GoToUrl(Url);

        // Root
        private static readonly By RootBy =
            By.CssSelector("section.simulation-history");

        private static readonly By ActiveSessionRowsBy =
            By.XPath(
                "//section[contains(@class,'simulation-history')]//h2[normalize-space()='Training Sessions']" +
                "/following-sibling::div[contains(@class,'table-shell')][1]" +
                "//table/tbody/tr[.//span[contains(@class,'status-pill') and contains(@class,'active') and normalize-space(.)='ACTIVE']]"
            );


        private static readonly By RowMenuButtonBy =
            By.CssSelector("td.actions button.menu-button[aria-label='Open simulation actions']");

        private static readonly By RowMenuCardBy =
            By.CssSelector("td.actions .menu-card");

        private static readonly By MenuDeleteButtonBy =
            By.XPath(".//button[normalize-space()='Delete']");

        private static readonly By TrainingSessionsTableBy =
            By.XPath("//section[contains(@class,'simulation-history')]//h2[normalize-space()='Training Sessions']/following-sibling::div[contains(@class,'table-shell')][1]//table");

        private static readonly By TrainingSessionsRowsBy =
            By.XPath("//section[contains(@class,'simulation-history')]//h2[normalize-space()='Training Sessions']" +
                     "/following-sibling::div[contains(@class,'table-shell')][1]" +
                     "//table/tbody/tr[td[contains(@class,'actions')]]");

        private static readonly By PausedNextBtnBy =
            By.XPath("//h2[normalize-space()='Training Sessions']/following-sibling::div[contains(@class,'table-shell')][1]//div[contains(@class,'pagination-controls')]//button[contains(@class,'pagination-btn')][normalize-space()='Next >']");

        private static readonly By PausedPrevBtnBy =
            By.XPath("//h2[normalize-space()='Training Sessions']/following-sibling::div[contains(@class,'table-shell')][1]//div[contains(@class,'pagination-controls')]//button[contains(@class,'pagination-btn')][normalize-space()='< Prev']");

        private static readonly By TrainingLoadingRowBy =
    By.XPath("//h2[normalize-space()='Training Sessions']/following-sibling::div[contains(@class,'table-shell')][1]" +
             "//tbody/tr[td[contains(@class,'loading')]]");

        private static readonly By TrainingPageInfoBy =
            By.XPath("//h2[normalize-space()='Training Sessions']/following-sibling::div[contains(@class,'table-shell')][1]" +
                     "//div[contains(@class,'pagination-info')]");

        private static readonly By TrainingTbodyBy =
            By.XPath("//h2[normalize-space()='Training Sessions']/following-sibling::div[contains(@class,'table-shell')][1]//table/tbody");



        public void DeletePausedSessionByStartingBalance(string startingBalanceUi)
        {
            Open();
            Wait.Until(ExpectedConditions.ElementExists(RootBy));
            Wait.Until(d => d.FindElements(TrainingSessionsTableBy).Any());

            GoToFirstTrainingSessionsPage();
            WaitForPageToStabilizeFast();

            const int maxPagesToScanBeforeFallback = 3; // << reduce time

            for (var i = 0; i < maxPagesToScanBeforeFallback; i++)
            {
                // very short wait (or remove entirely)
                WaitForBalanceToAppearOnCurrentPage(startingBalanceUi, TimeSpan.FromMilliseconds(300));

                var row = FindRowByStartingBalanceOnCurrentPage(startingBalanceUi);
                if (row != null)
                {
                    DeleteRowViaMenuByBalance(startingBalanceUi);
                    WaitForRowGoneSoft(startingBalanceUi, seconds: 3);
                    return;
                }

                var next = Driver.FindElements(PausedNextBtnBy).FirstOrDefault();
                if (next == null || !next.Enabled) break;

                next.Click();
                WaitForPageToStabilizeFast();
            }

            // FALLBACK ASAP
            DeleteActiveTrainingSessionFast();
        }

        private void WaitForPageToStabilizeFast(int seconds = 5)
        {
            var w = new WebDriverWait(Driver, TimeSpan.FromSeconds(seconds));

            w.Until(d => !d.FindElements(TrainingLoadingRowBy).Any());

            // tbody exists
            w.Until(d => d.FindElements(TrainingTbodyBy).Any());

            // rows or empty state
            w.Until(d =>
                d.FindElements(TrainingSessionsRowsBy).Any() ||
                d.PageSource.Contains("No training sessions yet."));
        }

        public bool DeleteActiveTrainingSessionFast()
        {
            GoToFirstTrainingSessionsPage();
            WaitForPageToStabilizeFast();

            const int maxPagesToScan = 5;

            for (var i = 0; i < maxPagesToScan; i++)
            {
                var row = Driver.FindElements(ActiveSessionRowsBy).FirstOrDefault(r => r.Displayed);
                if (row != null)
                {
                    DeleteRowViaMenu(row);
                    return true;
                }

                var next = Driver.FindElements(PausedNextBtnBy).FirstOrDefault();
                if (next == null || !next.Enabled) return false;

                next.Click();
                WaitForPageToStabilizeFast();
            }

            return false;
        }

        private void WaitForRowGoneSoft(string startingBalanceUi, int seconds = 3)
        {
            try
            {
                var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(seconds));
                wait.Until(_ => FindRowByStartingBalanceOnCurrentPage(startingBalanceUi) == null);
            }
            catch (WebDriverTimeoutException)
            {
                // soft
            }
        }




        public bool DeleteActiveTrainingSession()
        {
            GoToFirstTrainingSessionsPage();
            WaitForPageToStabilize();

            for (var i = 0; i < 25; i++)
            {
                var row = Driver.FindElements(ActiveSessionRowsBy).FirstOrDefault(r => r.Displayed);
                if (row != null)
                {
                    DeleteRowViaMenu(row);
                    return true;
                }

                var next = Driver.FindElements(PausedNextBtnBy).FirstOrDefault();
                if (next == null || !next.Enabled) return false;

                next.Click();
                WaitForPageToStabilize();
            }

            return false;
        }

        private void DeleteRowViaMenu(IWebElement row)
        {
            var menuBtn = row.FindElements(RowMenuButtonBy).FirstOrDefault(b => b.Displayed && b.Enabled);
            if (menuBtn == null) return;

            Wait.Until(ExpectedConditions.ElementToBeClickable(menuBtn)).Click();

            // Menu card is inside td.actions; re-find from the row to avoid stale
            IWebElement? menuCard = null;
            Wait.Until(_ =>
            {
                try
                {
                    menuCard = row.FindElements(RowMenuCardBy).FirstOrDefault(c => c.Displayed);
                    return menuCard != null;
                }
                catch (StaleElementReferenceException) { return false; }
            });

            if (menuCard == null) return;

            var deleteBtn = menuCard.FindElements(MenuDeleteButtonBy).FirstOrDefault(b => b.Displayed && b.Enabled);
            if (deleteBtn == null) return;

            Wait.Until(ExpectedConditions.ElementToBeClickable(deleteBtn)).Click();
        }



        private void GoToFirstTrainingSessionsPage()
        {
            for (var i = 0; i < 25; i++)
            {
                var prev = Driver.FindElements(PausedPrevBtnBy).FirstOrDefault();
                if (prev == null || !prev.Enabled) return;
                prev.Click();
                WaitForPageToStabilize();
            }
        }

        private IWebElement? FindRowByStartingBalanceOnCurrentPage(string startingBalanceUi)
        {
            var target = ParseCurrency(startingBalanceUi);
            var rows = Driver.FindElements(TrainingSessionsRowsBy);

            foreach (var r in rows)
            {
                try
                {
                    if (!r.Displayed) continue;

                    var cells = r.FindElements(By.CssSelector("td"));
                    if (cells.Count < 8) continue;

                    var cellValue = ParseCurrency(cells[3].Text);

                    // exact cents match
                    if (cellValue == target)
                        return r;

                    // optional tolerance if you ever see rounding drift:
                    // if (Math.Abs(cellValue - target) < 0.01m) return r;
                }
                catch (StaleElementReferenceException) { }
            }

            return null;
        }

        private static decimal ParseCurrency(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return 0m;
            var cleaned = new string(s.Where(c => char.IsDigit(c) || c == '.' || c == '-').ToArray());
            return decimal.TryParse(cleaned, out var v) ? v : 0m;
        }

        private void DeleteRowViaMenuByBalance(string startingBalanceUi)
        {
            var row = FindRowByStartingBalanceOnCurrentPage(startingBalanceUi);
            if (row == null) return;

            var menuBtn = row.FindElements(RowMenuButtonBy).FirstOrDefault();
            if (menuBtn == null) return;

            Wait.Until(ExpectedConditions.ElementToBeClickable(menuBtn)).Click();

            // Re-find row after click (Angular may re-render)
            Wait.Until(_ => FindRowByStartingBalanceOnCurrentPage(startingBalanceUi) != null);
            row = FindRowByStartingBalanceOnCurrentPage(startingBalanceUi)!;

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
            var deleteBtn = menuCard.FindElements(MenuDeleteButtonBy).FirstOrDefault();
            if (deleteBtn == null) return;

            Wait.Until(ExpectedConditions.ElementToBeClickable(deleteBtn)).Click();
        }

        private void WaitForRowGone(string startingBalanceUi)
        {
            var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(10));
            wait.Until(_ => FindRowByStartingBalanceOnCurrentPage(startingBalanceUi) == null);
        }

        private void WaitForPageToStabilize()
        {
            var w = new WebDriverWait(Driver, TimeSpan.FromSeconds(10));

            // wait until loading row is gone
            w.Until(d => !d.FindElements(TrainingLoadingRowBy).Any());

            // wait until there is content (rows or empty state)
            w.Until(d =>
                d.FindElements(TrainingSessionsRowsBy).Any() ||
                d.PageSource.Contains("No training sessions yet."));

            // wait until pagination info stabilizes (helps with page flips)
            string? prev = null;
            w.Until(d =>
            {
                var cur = d.FindElements(TrainingPageInfoBy).FirstOrDefault()?.Text?.Trim() ?? "";
                var ok = !string.IsNullOrEmpty(cur) && cur == prev;
                prev = cur;
                return ok;
            });
        }

        private bool WaitForBalanceToAppearOnCurrentPage(string startingBalanceUi, TimeSpan? timeout = null)
        {
            var w = new WebDriverWait(Driver, timeout ?? TimeSpan.FromSeconds(2));

            try
            {
                return w.Until(d =>
                {
                    // Ensure page is not mid-loading
                    if (d.FindElements(TrainingLoadingRowBy).Any()) return false;

                    foreach (var r in d.FindElements(TrainingSessionsRowsBy))
                    {
                        try
                        {
                            var cells = r.FindElements(By.CssSelector("td"));
                            if (cells.Count < 8) continue;

                            var cellText = cells[3].Text.Trim();
                            if (string.Equals(cellText, startingBalanceUi.Trim(), StringComparison.OrdinalIgnoreCase))
                                return true;
                        }
                        catch (StaleElementReferenceException) { /* retry */ }
                    }
                    return false;
                });
            }
            catch (WebDriverTimeoutException)
            {
                return false; // soft-fail: page didn’t settle / row not found in time
            }
        }

    }
}
