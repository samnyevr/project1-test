using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System;
using System.Linq;
using System.Globalization;
using System.Threading;

namespace FidelityInsights.Pages {
    public class SimulationHistoryPage {
        private readonly IWebDriver _driver;
        private readonly WebDriverWait _wait;

        public SimulationHistoryPage(IWebDriver driver, int seconds = 25) {
            _driver = driver;
            _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(seconds));
        }

        private const string Url = "https://d2rczu3zvi71ix.cloudfront.net/session-history";

        private static readonly By PageTitleBy =
            By.XPath("//h1[normalize-space()='View Training Sessions']");

        // Shells for each section
        private static readonly By RootBy =
            By.CssSelector("section.simulation-history");

        private static readonly By TrainingShellBy =
            By.XPath("//section[contains(@class,'simulation-history')]//h2[normalize-space()='Training Sessions']/following-sibling::div[contains(@class,'table-shell')][1]");

        private static readonly By SubmittedShellBy =
            By.XPath("//section[contains(@class,'simulation-history')]//h2[normalize-space()='Submitted Training Sessions']/following-sibling::div[contains(@class,'table-shell')][1]");

        private static readonly By TableBy =
            By.CssSelector("table");

        private static readonly By RowMenuButtonBy =
            By.CssSelector("button, .icon-button");

        private static readonly By RowKebabBy =
            By.CssSelector("button[aria-label*='menu' i], button:has(svg), button:has(span), button");

        private static readonly By DeleteMenuItemBy =
            By.XPath("//button[normalize-space()='Delete' or .//*[normalize-space()='Delete']]");

        // Optional confirm dialog support (if your UI has it)
        private static readonly By ConfirmDeleteBy =
            By.XPath("//button[normalize-space()='Confirm' or normalize-space()='Delete']");

        // Rows that are actual data rows (exclude loading/empty/padding)
        private static readonly By DataRowsRelativeBy =
            By.XPath(".//tbody/tr[td[contains(@class,'actions')]]");

        private static readonly By StatusPillRelativeBy =
            By.CssSelector("span.status-pill");

        private static readonly By GainLossCellRelativeBy =
            By.XPath("./td[6]"); // gain/loss is 6th col in template

        private static readonly By StartingBalanceCellRelativeBy =
            By.XPath("./td[4]"); // starting balance is 4th col in template

        // Pagination controls for each section
        private static readonly By TrainingNextRelativeBy =
            By.XPath(".//div[contains(@class,'pagination-controls')]//button[contains(@class,'pagination-btn')][normalize-space()='Next >']");

        private static readonly By TrainingPrevRelativeBy =
            By.XPath(".//div[contains(@class,'pagination-controls')]//button[contains(@class,'pagination-btn')][normalize-space()='< Prev']");

        private static readonly By SubmittedNextRelativeBy =
            By.XPath(".//div[contains(@class,'pagination-controls')]//button[contains(@class,'pagination-btn')][normalize-space()='Next >']");

        private static readonly By SubmittedPrevRelativeBy =
            By.XPath(".//div[contains(@class,'pagination-controls')]//button[contains(@class,'pagination-btn')][normalize-space()='< Prev']");

        public void GoTo() => _driver.Navigate().GoToUrl(Url);

        // ---------- Section mapping for your feature text ----------
        // "Paused Training Sessions" => Training Sessions
        // "Completed Training Sessions" => Submitted Training Sessions
        // In case a place is missed with such a reference
        private By ResolveShellBy(string tableName) {
            var key = (tableName ?? "").Trim();

            if (key.Equals("Paused Training Sessions", StringComparison.OrdinalIgnoreCase))
                return TrainingShellBy;

            if (key.Equals("Completed Training Sessions", StringComparison.OrdinalIgnoreCase))
                return SubmittedShellBy;

            // fall back to actual headings if someone passes them directly
            if (key.Equals("Training Sessions", StringComparison.OrdinalIgnoreCase))
                return TrainingShellBy;

            if (key.Equals("Submitted Training Sessions", StringComparison.OrdinalIgnoreCase))
                return SubmittedShellBy;

            throw new ArgumentException($"Unknown table name '{tableName}'. Expected 'Paused Training Sessions' or 'Completed Training Sessions'.");
        }

        public void WaitForLoaded() {
            _wait.Until(ExpectedConditions.ElementExists(RootBy));
            _wait.Until(ExpectedConditions.ElementIsVisible(PageTitleBy));
            // wait until at least the Training section shell exists
            _wait.Until(ExpectedConditions.ElementExists(TrainingShellBy));
        }

        public void WaitForTableVisible(string tableName) {
            var shellBy = ResolveShellBy(tableName);
            var shell = _wait.Until(ExpectedConditions.ElementIsVisible(shellBy));

            // Ensure the table is present in that shell
            _ = shell.FindElement(By.XPath(".//table"));
        }

        // ---------- Status pill ----------
        private IWebElement FindRowByStartDateOnCurrentPage(IWebElement shell, string startDate) {
            var rowBy = By.XPath($".//tbody/tr[td[contains(@class,'actions')] and normalize-space(td[1])='{startDate}']");
            return shell.FindElement(rowBy);
        }

        public string GetStatusPillText(string sessionStartDate) {
            // Search both sections (because your scenario checks a COMPLETED date that will appear under Submitted)
            var trainingShell = _wait.Until(ExpectedConditions.ElementIsVisible(TrainingShellBy));
            var submittedShell = _wait.Until(ExpectedConditions.ElementIsVisible(SubmittedShellBy));

            IWebElement row = null;

            row = trainingShell.FindElements(By.XPath($".//tbody/tr[td[contains(@class,'actions')] and normalize-space(td[1])='{sessionStartDate}']")).FirstOrDefault(r => r.Displayed)
               ?? submittedShell.FindElements(By.XPath($".//tbody/tr[td[contains(@class,'actions')] and normalize-space(td[1])='{sessionStartDate}']")).FirstOrDefault(r => r.Displayed);

            if (row == null)
                throw new NoSuchElementException($"Could not find a row with start date '{sessionStartDate}' in either table.");

            var pill = row.FindElement(StatusPillRelativeBy);
            return (pill.Text ?? "").Trim();
        }

        // ---------- Sorting ----------
        public void ClickColumnHeaderInPausedTable(string columnName) {
            // Your feature says "Paused table" but that's now the Training Sessions table.
            var shell = _wait.Until(ExpectedConditions.ElementIsVisible(TrainingShellBy));
            var headerBtnBy = By.XPath($".//thead//th//button[normalize-space()='{columnName}']");
            _wait.Until(ExpectedConditions.ElementToBeClickable(shell.FindElement(headerBtnBy))).Click();

            WaitForSectionPageToStabilize(shell);
        }

        public List<decimal> GetStartingBalancesOnPausedPage() {
            var shell = _wait.Until(ExpectedConditions.ElementIsVisible(TrainingShellBy));
            WaitForSectionPageToStabilize(shell);

            var rows = shell.FindElements(DataRowsRelativeBy).Where(r => r.Displayed).ToList();
            return rows.Select(r => ParseCurrency(r.FindElement(StartingBalanceCellRelativeBy).Text)).ToList();
        }

        // ---------- Gain/Loss styling (scoped to Training Sessions / visible rows) ----------
        public bool HasStyledGainLoss(string expectedColor, int seconds = 25) {
            var wantGain = expectedColor.Trim().Equals("Green", StringComparison.OrdinalIgnoreCase);
            var expectedClass = wantGain ? "gain" : "loss";

            bool Matches(IWebElement cell) {
                var cls = (cell.GetAttribute("class") ?? "");
                var classes = cls.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                if (!classes.Contains(expectedClass, StringComparer.OrdinalIgnoreCase))
                    return false;

                var txt = (cell.Text ?? "").Trim();
                if (string.IsNullOrWhiteSpace(txt)) return false;

                var isNegative = txt.StartsWith("-", StringComparison.Ordinal);
                return wantGain ? !isNegative : isNegative;
            }

            bool ScanSection(By shellBy, By nextBtnRelBy, int maxPages) {
                var shell = _wait.Until(ExpectedConditions.ElementIsVisible(shellBy));
                WaitForSectionPageToStabilize(shell);

                // best-effort: go to first page (if Prev exists)
                for (int i = 0; i < 50; i++) {
                    var prev = shell.FindElements(TrainingPrevRelativeBy).FirstOrDefault(e => e.Displayed);
                    if (prev == null || !prev.Enabled) break;

                    var firstRow = shell.FindElements(DataRowsRelativeBy).FirstOrDefault();
                    prev.Click();
                    if (firstRow != null) _wait.Until(ExpectedConditions.StalenessOf(firstRow));
                    WaitForSectionPageToStabilize(shell);
                }

                for (int page = 0; page < maxPages; page++) {
                    WaitForSectionPageToStabilize(shell);

                    // td.gain / td.loss are on the gain/loss column
                    var cells = shell.FindElements(By.CssSelector("td.gain, td.loss"))
                                     .Where(e => e.Displayed)
                                     .ToList();

                    if (cells.Any(Matches))
                        return true;

                    var next = shell.FindElements(nextBtnRelBy).FirstOrDefault(e => e.Displayed);
                    if (next == null || !next.Enabled) return false;

                    var oldFirstRow = shell.FindElements(DataRowsRelativeBy).FirstOrDefault();
                    next.Click();
                    if (oldFirstRow != null) _wait.Until(ExpectedConditions.StalenessOf(oldFirstRow));
                }

                return false;
            }

            // Scan Training Sessions, then Submitted Training Sessions (which has your green rows)
            var end = DateTime.UtcNow.AddSeconds(seconds);
            while (DateTime.UtcNow < end) {
                if (ScanSection(TrainingShellBy, TrainingNextRelativeBy, maxPages: 5)) return true;
                if (ScanSection(SubmittedShellBy, SubmittedNextRelativeBy, maxPages: 10)) return true;

                Thread.Sleep(150);
            }

            return false;
        }

        // ---------- Helpers ----------
        public static bool IsSorted(IReadOnlyList<decimal> values, bool ascending) {
            for (int i = 1; i < values.Count; i++) {
                if (ascending) {
                    if (values[i] < values[i - 1]) return false;
                } else {
                    if (values[i] > values[i - 1]) return false;
                }
            }
            return true;
        }

        private static decimal ParseCurrency(string s) {
            if (string.IsNullOrWhiteSpace(s)) return 0m;

            // keep digits, dot, minus
            var cleaned = new string(s.Where(c => char.IsDigit(c) || c == '.' || c == '-').ToArray());

            return decimal.TryParse(cleaned, NumberStyles.Number | NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
                CultureInfo.InvariantCulture, out var v)
                ? v
                : 0m;
        }

        public bool TryDeleteSessionByStartingBalance(string startingBalanceUi) {
            WaitForLoaded();

            // Find the row containing the exact balance text (e.g. "$12,345,678.90")
            var rowBy = By.XPath($"//table//tr[.//td[normalize-space()='{startingBalanceUi}']]");
            var row = _driver.FindElements(rowBy).FirstOrDefault(r => r.Displayed);
            if (row == null) return false;

            // Click kebab/menu button in that row (last column)
            var buttons = row.FindElements(By.CssSelector("button")).Where(b => b.Displayed && b.Enabled).ToList();
            if (buttons.Count == 0) return false;

            buttons.Last().Click();

            // Click Delete in the menu
            var delete = _wait.Until(ExpectedConditions.ElementToBeClickable(DeleteMenuItemBy));
            delete.Click();

            // If a confirm exists, click it; otherwise ignore
            var confirm = _driver.FindElements(ConfirmDeleteBy).FirstOrDefault(e => e.Displayed && e.Enabled);
            confirm?.Click();

            // Wait for row to disappear
            _wait.Until(_ => !_driver.FindElements(rowBy).Any());
            return true;
        }

        // --- Selectors ---

        private IWebElement GetMenuButton(string sessionIdentifier) {
            // Dynamic XPath to find the "Three Dots" button for a specific session (by Date or Name)
            var xpath = $"//tr[contains(., '{sessionIdentifier}')]//button[contains(@class, 'menu-button') or contains(@class, 'icon-button') or .//svg]";
            return _wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath(xpath)));
        }

        private IWebElement GetMenuOption(string optionName) {
            // Broadened to find button in any menu container (card, popover, or div)
            var xpath = $"//div[contains(@class, 'menu')]//button[contains(text(), '{optionName}')]";
            return _wait.Until(ExpectedConditions.ElementIsVisible(By.XPath(xpath)));
        }

        public void ClickColumnHeader(string tableType, string columnName) {
            // FIX: Map the Test language ("Paused") to the actual UI Header ("Training Sessions")
            string headerText = tableType;
            if (tableType.Equals("Paused", StringComparison.OrdinalIgnoreCase)) {
                headerText = "Training Sessions";
            }

            var xpath = $"//h2[contains(normalize-space(), '{headerText}')]/following::table[1]//th[contains(., '{columnName}')]";

            var headerCell = _wait.Until(ExpectedConditions.ElementIsVisible(By.XPath(xpath)));

            var sortButton = headerCell.FindElements(By.TagName("button")).FirstOrDefault();
            if (sortButton != null && sortButton.Displayed) {
                sortButton.Click();
            } else {
                headerCell.Click();
            }
        }

        // --- Actions ---

        public void OpenMenuForSession(string sessionIdentifier) {
            var btn = GetMenuButton(sessionIdentifier);
            btn.Click();
            Thread.Sleep(500); // Allow animation to complete
        }

        public void SelectMenuOption(string optionName) {
            var btn = GetMenuOption(optionName);
            btn.Click();
        }

        public bool IsSessionVisible(string sessionIdentifier) {
            try {
                _driver.FindElement(By.XPath($"//tr[contains(., '{sessionIdentifier}')]"));
                return true;
            } catch (NoSuchElementException) {
                return false;
            }
        }

        public bool CheckGainLossColor(string color) {
            string cssClass = color.ToLower() == "green" ? "gain" : "loss";
            return _driver.FindElements(By.ClassName(cssClass)).Count > 0;
        }

        // --- Settings Menu Actions ---

        public void ClickSettingsGear() {
            bool isMenuOpen = false;
            try {
                var menuItems = _driver.FindElements(By.XPath("//*[contains(normalize-space(), 'Start New Training Session')] | //*[contains(normalize-space(), 'Reset Training Session')]"));

                if (menuItems.Count > 0 && menuItems.Any(e => e.Displayed)) {
                    isMenuOpen = true;
                }
            } catch (Exception) { /* Ignore check errors */ }

            if (isMenuOpen) {
                return;
            }

            var gearBtn = _wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("button.settings-button, button.gear-icon, button[aria-label='Settings']")));
            gearBtn.Click();
        }

        public void SelectSettingsOption(string optionText) {
            IWebElement element;
            try {
                var btnXpath = $"//button[contains(normalize-space(), '{optionText}')]";
                element = _wait.Until(ExpectedConditions.ElementIsVisible(By.XPath(btnXpath)));
            } catch (WebDriverTimeoutException) {
                var anyXpath = $"//*[contains(normalize-space(), '{optionText}')]";
                element = _wait.Until(ExpectedConditions.ElementIsVisible(By.XPath(anyXpath)));
            }

            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView(true);", element);
            Thread.Sleep(500);
            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", element);
        }

        // --- Modals & Popups ---

        public bool IsSimulationSummaryVisible() {
            try {
                var summary = _wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//h1[contains(text(), 'Summary')] | //div[contains(@class, 'modal-title')][contains(., 'Summary')]")));
                return summary.Displayed;
            } catch (WebDriverTimeoutException) {
                return false;
            }
        }

        public void ConfirmEndSession() {
            var xpath = "//*[normalize-space()='Yes']";
            var yesButton = _wait.Until(ExpectedConditions.ElementIsVisible(By.XPath(xpath)));

            Thread.Sleep(500);
            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", yesButton);

            try { _wait.Until(ExpectedConditions.StalenessOf(yesButton)); } catch (WebDriverTimeoutException) { }
        }

        public void ConfirmResetSession() {
            var xpath = "//*[normalize-space()='Yes']";
            var yesButton = _wait.Until(ExpectedConditions.ElementIsVisible(By.XPath(xpath)));

            Thread.Sleep(1000);
            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", yesButton);

            try { _wait.Until(ExpectedConditions.StalenessOf(yesButton)); } catch (WebDriverTimeoutException) { }
        }

        // --- Logic for Ensuring Session Exists ---

        public void EnsureActiveSessionExists() {
            bool isMenuOpen = false;
            try {
                // Check if "Start Training Session" button is visible (Form is open)
                isMenuOpen = _driver.FindElements(By.XPath("//button[contains(normalize-space(), 'Start Training Session')]")).Count > 0;

                if (!isMenuOpen) {
                    // Check if "Reset" option is visible (Menu list is open)
                    if (_driver.FindElements(By.XPath("//*[contains(normalize-space(), 'Reset Training Session')]")).Count > 0) {
                        _driver.FindElement(By.TagName("body")).SendKeys(Keys.Escape);
                        Thread.Sleep(500);
                        return;
                    }
                }
            } catch (Exception) { }

            if (!isMenuOpen) {
                try {
                    ClickSettingsGear();
                    Thread.Sleep(500);
                } catch (ElementClickInterceptedException) { }
            }

            // Check for "Reset" again
            bool resetExists = _driver.FindElements(By.XPath("//*[contains(normalize-space(), 'Reset Training Session')]")).Count > 0;
            if (resetExists) {
                _driver.FindElement(By.TagName("body")).SendKeys(Keys.Escape);
                Thread.Sleep(500);
                return;
            }

            // Start New Session if missing
            if (_driver.FindElements(By.XPath("//button[contains(normalize-space(), 'Start Training Session')]")).Count == 0) {
                SelectSettingsOption("Start New Training Session");
            }

            // Fill the Form
            var startBtn = _wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//button[contains(normalize-space(), 'Start Training Session')]")));

            try {
                var durationDropdown = _driver.FindElement(By.XPath("//*[contains(text(), 'Select a session length')]"));
                durationDropdown.Click();
                Thread.Sleep(500);
                var option = _driver.FindElement(By.XPath("//*[contains(normalize-space(), '1 year')]"));
                option.Click();
            } catch (NoSuchElementException) { }

            var balanceInput = _driver.FindElement(By.XPath("//input[@type='number']"));
            balanceInput.Clear();
            balanceInput.SendKeys("10000");

            startBtn.Click();

            _wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//*[contains(normalize-space(), 'End Day')]")));
        }

        // --- Verification Helpers ---

        public void GoToPortfolio() {
            var portfolioLink = _wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//a[contains(normalize-space(), 'Portfolio')]")));
            portfolioLink.Click();
            _wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//*[contains(normalize-space(), 'Holdings') or contains(normalize-space(), 'Overview')]")));
        }

        public string GetCurrentBalance() {
            try {
                // Primary check: Portfolio Page Header
                var balance = _driver.FindElement(By.CssSelector("h1.page-title"));
                return balance.Text;
            } catch (NoSuchElementException) { }

            try {
                // Secondary check: Label search
                var portfolioXpath = "//*[contains(normalize-space(), 'TOTAL VALUE')]/following::*[contains(text(), '$')][1]";
                return _driver.FindElement(By.XPath(portfolioXpath)).Text;
            } catch (NoSuchElementException) { }

            try {
                // Fallback: Dashboard Header
                var balance = _driver.FindElement(By.CssSelector(".balance-display, .header-balance, #current-balance"));
                return balance.Text;
            } catch (NoSuchElementException) { }

            // Final Fallback: History Table Row
            return _driver.FindElement(By.XPath("//table//tbody//tr[1]//td[5]")).Text;
        }

        public bool AreHoldingsEmpty() {
            try {
                var noHoldingsMsg = _driver.FindElements(By.XPath("//div[contains(text(), 'No holdings')]"));
                if (noHoldingsMsg.Count > 0) return true;
                var rows = _driver.FindElements(By.CssSelector(".holdings-table tbody tr"));
                return rows.Count == 0;
            } catch (NoSuchElementException) {
                return true;
            }
        }

        // --- Start Session Validation ---

        public bool IsNewSessionModalVisible() {
            try {
                var xpath = "//*[contains(normalize-space(), 'Training Session Settings')]";
                var modalTitle = _wait.Until(ExpectedConditions.ElementIsVisible(By.XPath(xpath)));
                return modalTitle.Displayed;
            } catch (WebDriverTimeoutException) {
                return false;
            }
        }

        public bool AreSessionSettingsInputsVisible() {
            try {
                var balanceInput = _driver.FindElement(By.CssSelector("input[type='number']"));
                return balanceInput.Enabled;
            } catch (NoSuchElementException) { return false; }
        }

        private void WaitForSectionPageToStabilize(IWebElement shell) {
            // Wait until either:
            // - there is at least one data row
            // - or an empty message is shown in the shell
            _wait.Until(_ => {
                try {
                    var dataRows = shell.FindElements(DataRowsRelativeBy);
                    if (dataRows.Any(r => r.Displayed)) return true;

                    var text = shell.Text ?? "";
                    return text.Contains("No training sessions yet.", StringComparison.OrdinalIgnoreCase)
                        || text.Contains("No completed training sessions yet.", StringComparison.OrdinalIgnoreCase)
                        || text.Contains("Loading", StringComparison.OrdinalIgnoreCase) == false;
                } catch (StaleElementReferenceException) {
                    return false;
                }
            });
        }
    }
}