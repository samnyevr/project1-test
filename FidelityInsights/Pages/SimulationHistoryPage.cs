using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System;
using System.Threading;

namespace FidelityInsights.Pages
{
    public class SimulationHistoryPage
    {
        private readonly IWebDriver _driver;
        private readonly WebDriverWait _wait;

        public SimulationHistoryPage(IWebDriver driver, int seconds = 10)
        {
            _driver = driver;
            _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(seconds));
        }

        private static readonly By PageTitleBy =
            By.XPath("//h1[normalize-space()='Training Session History']");

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

        public void WaitForLoaded()
        {
            _wait.Until(ExpectedConditions.ElementIsVisible(PageTitleBy));
            _wait.Until(d => d.FindElements(TableBy).Any());
        }

        public bool TryDeleteSessionByStartingBalance(string startingBalanceUi)
        {
            WaitForLoaded();

            // Find the row containing the exact balance text (e.g. "$12,345,678.90")
            var rowBy = By.XPath($"//table//tr[.//td[normalize-space()='{startingBalanceUi}']]");
            var row = _driver.FindElements(rowBy).FirstOrDefault(r => r.Displayed);
            if (row == null) return false;

            // Click kebab/menu button in that row (last column)
            // Your screenshot shows "..." button; this finds any button in the row and prefers the last one.
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
      
        public void GoTo()
        {
            // Update this URL if your team changed the port
            _driver.Navigate().GoToUrl("https://d2rczu3zvi71ix.cloudfront.net/simulation-history");
        }

        // --- Selectors ---

        private IWebElement GetMenuButton(string sessionIdentifier)
        {
            // Dynamic XPath to find the "Three Dots" button for a specific session (by Date or Name)
            var xpath = $"//tr[contains(., '{sessionIdentifier}')]//button[contains(@class, 'menu-button')]";
            return _wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath(xpath)));
        }

        private IWebElement GetMenuOption(string optionName)
        {
            var xpath = $"//div[@class='menu-card']//button[contains(text(), '{optionName}')]";
            return _wait.Until(ExpectedConditions.ElementIsVisible(By.XPath(xpath)));
        }

        public void ClickColumnHeader(string tableType, string columnName)
        {
            var xpath = $"//h2[contains(text(), '{tableType}')]/following-sibling::div//th[contains(., '{columnName}')]//button";
            var header = _wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath(xpath)));
            header.Click();
        }

        // --- Actions ---

        public void OpenMenuForSession(string sessionIdentifier)
        {
            var btn = GetMenuButton(sessionIdentifier);
            btn.Click();
            Thread.Sleep(500); // Allow animation to complete
        }

        public void SelectMenuOption(string optionName)
        {
            var btn = GetMenuOption(optionName);
            btn.Click();
        }

        public bool IsSessionVisible(string sessionIdentifier)
        {
            try
            {
                _driver.FindElement(By.XPath($"//tr[contains(., '{sessionIdentifier}')]"));
                return true;
            }
            catch (NoSuchElementException)
            {
                return false;
            }
        }

        public string GetStatusPillText(string sessionIdentifier)
        {
            var xpath = $"//td[normalize-space()='{sessionIdentifier}']/..//span[contains(@class, 'status-pill')]";
            var element = _wait.Until(ExpectedConditions.ElementIsVisible(By.XPath(xpath)));
            return element.Text;
        }

        public bool CheckGainLossColor(string color)
        {
            string cssClass = color.ToLower() == "green" ? "gain" : "loss";
            return _driver.FindElements(By.ClassName(cssClass)).Count > 0;
        }

        // --- Settings Menu Actions ---

        public void ClickSettingsGear()
        {
            // Check if the menu is already open to avoid clicking the backdrop
            bool isMenuOpen = false;
            try
            {
                var menuItems = _driver.FindElements(By.XPath("//*[contains(normalize-space(), 'Start New Training Session')] | //*[contains(normalize-space(), 'Reset Training Session')]"));

                if (menuItems.Count > 0 && menuItems.Any(e => e.Displayed))
                {
                    isMenuOpen = true;
                }
            }
            catch (Exception) { /* Ignore check errors */ }

            if (isMenuOpen)
            {
                return;
            }

            var gearBtn = _wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("button.settings-button, button.gear-icon, button[aria-label='Settings']")));
            gearBtn.Click();
        }

        public void SelectSettingsOption(string optionText)
        {
            IWebElement element;
            try
            {
                var btnXpath = $"//button[contains(normalize-space(), '{optionText}')]";
                element = _wait.Until(ExpectedConditions.ElementIsVisible(By.XPath(btnXpath)));
            }
            catch (WebDriverTimeoutException)
            {
                var anyXpath = $"//*[contains(normalize-space(), '{optionText}')]";
                element = _wait.Until(ExpectedConditions.ElementIsVisible(By.XPath(anyXpath)));
            }

            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView(true);", element);
            Thread.Sleep(500);
            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", element);
        }

        // --- Modals & Popups ---

        public bool IsSimulationSummaryVisible()
        {
            try
            {
                var summary = _wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//h1[contains(text(), 'Summary')] | //div[contains(@class, 'modal-title')][contains(., 'Summary')]")));
                return summary.Displayed;
            }
            catch (WebDriverTimeoutException)
            {
                return false;
            }
        }

        public void ConfirmEndSession()
        {
            var xpath = "//*[normalize-space()='Yes']";
            var yesButton = _wait.Until(ExpectedConditions.ElementIsVisible(By.XPath(xpath)));

            Thread.Sleep(500);
            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", yesButton);

            try { _wait.Until(ExpectedConditions.StalenessOf(yesButton)); } catch (WebDriverTimeoutException) { }
        }

        public void ConfirmResetSession()
        {
            var xpath = "//*[normalize-space()='Yes']";
            var yesButton = _wait.Until(ExpectedConditions.ElementIsVisible(By.XPath(xpath)));

            Thread.Sleep(1000);
            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", yesButton);

            try { _wait.Until(ExpectedConditions.StalenessOf(yesButton)); } catch (WebDriverTimeoutException) { }
        }

        // --- Logic for Ensuring Session Exists ---

        public void EnsureActiveSessionExists()
        {
            bool isMenuOpen = false;
            try
            {
                // Check if "Start Training Session" button is visible (Form is open)
                isMenuOpen = _driver.FindElements(By.XPath("//button[contains(normalize-space(), 'Start Training Session')]")).Count > 0;

                if (!isMenuOpen)
                {
                    // Check if "Reset" option is visible (Menu list is open)
                    if (_driver.FindElements(By.XPath("//*[contains(normalize-space(), 'Reset Training Session')]")).Count > 0)
                    {
                        _driver.FindElement(By.TagName("body")).SendKeys(Keys.Escape);
                        Thread.Sleep(500);
                        return;
                    }
                }
            }
            catch (Exception) { }

            if (!isMenuOpen)
            {
                try
                {
                    ClickSettingsGear();
                    Thread.Sleep(500);
                }
                catch (ElementClickInterceptedException) { }
            }

            // Check for "Reset" again
            bool resetExists = _driver.FindElements(By.XPath("//*[contains(normalize-space(), 'Reset Training Session')]")).Count > 0;
            if (resetExists)
            {
                _driver.FindElement(By.TagName("body")).SendKeys(Keys.Escape);
                Thread.Sleep(500);
                return;
            }

            // Start New Session if missing
            if (_driver.FindElements(By.XPath("//button[contains(normalize-space(), 'Start Training Session')]")).Count == 0)
            {
                SelectSettingsOption("Start New Training Session");
            }

            // Fill the Form
            var startBtn = _wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//button[contains(normalize-space(), 'Start Training Session')]")));

            try
            {
                var durationDropdown = _driver.FindElement(By.XPath("//*[contains(text(), 'Select a session length')]"));
                durationDropdown.Click();
                Thread.Sleep(500);
                var option = _driver.FindElement(By.XPath("//*[contains(normalize-space(), '1 year')]"));
                option.Click();
            }
            catch (NoSuchElementException) { }

            var balanceInput = _driver.FindElement(By.XPath("//input[@type='number']"));
            balanceInput.Clear();
            balanceInput.SendKeys("10000");

            startBtn.Click();

            _wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//*[contains(normalize-space(), 'End Day')]")));
        }

        // --- Verification Helpers ---

        public void GoToPortfolio()
        {
            var portfolioLink = _wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//a[contains(normalize-space(), 'Portfolio')]")));
            portfolioLink.Click();
            _wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//*[contains(normalize-space(), 'Holdings') or contains(normalize-space(), 'Overview')]")));
        }

        public string GetCurrentBalance()
        {
            try
            {
                // Primary check: Portfolio Page Header
                var balance = _driver.FindElement(By.CssSelector("h1.page-title"));
                return balance.Text;
            }
            catch (NoSuchElementException) { }

            try
            {
                // Secondary check: Label search
                var portfolioXpath = "//*[contains(normalize-space(), 'TOTAL VALUE')]/following::*[contains(text(), '$')][1]";
                return _driver.FindElement(By.XPath(portfolioXpath)).Text;
            }
            catch (NoSuchElementException) { }

            try
            {
                // Fallback: Dashboard Header
                var balance = _driver.FindElement(By.CssSelector(".balance-display, .header-balance, #current-balance"));
                return balance.Text;
            }
            catch (NoSuchElementException) { }

            // Final Fallback: History Table Row
            return _driver.FindElement(By.XPath("//table//tbody//tr[1]//td[5]")).Text;
        }

        public bool AreHoldingsEmpty()
        {
            try
            {
                var noHoldingsMsg = _driver.FindElements(By.XPath("//div[contains(text(), 'No holdings')]"));
                if (noHoldingsMsg.Count > 0) return true;
                var rows = _driver.FindElements(By.CssSelector(".holdings-table tbody tr"));
                return rows.Count == 0;
            }
            catch (NoSuchElementException)
            {
                return true;
            }
        }

        // --- Start Session Validation ---

        public bool IsNewSessionModalVisible()
        {
            try
            {
                var xpath = "//*[contains(normalize-space(), 'Training Session Settings')]";
                var modalTitle = _wait.Until(ExpectedConditions.ElementIsVisible(By.XPath(xpath)));
                return modalTitle.Displayed;
            }
            catch (WebDriverTimeoutException)
            {
                return false;
            }
        }

        public bool AreSessionSettingsInputsVisible()
        {
            try
            {
                var balanceInput = _driver.FindElement(By.CssSelector("input[type='number']"));
                return balanceInput.Enabled;
            }
            catch (NoSuchElementException) { return false; }
        }

        public bool HasStyledGainLoss(string expectedColor, int seconds = 5)
        {
            WaitForLoaded();

            var wantGain = expectedColor.Trim().Equals("Green", StringComparison.OrdinalIgnoreCase);
            var expectedClass = wantGain ? "gain" : "loss";

            // We validate against the sign to avoid false positives from stale/hidden elements.
            // Gain examples: "+$0.00", "$0.00", "+0.00"
            // Loss examples: "-$710.00", "-710.00"
            Func<IWebElement, bool> matchesSign = el =>
            {
                var txt = (el.Text ?? string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(txt)) return false;

                var isNegative = txt.StartsWith("-", StringComparison.Ordinal);
                return wantGain ? !isNegative : isNegative;
            };

            // Try multiple times with scrolling in case rows are virtualized / not yet rendered.
            var end = DateTime.UtcNow.AddSeconds(seconds);
            while (DateTime.UtcNow < end)
            {
                var candidates = _driver
                    .FindElements(By.CssSelector(".gain, .loss"))
                    .Where(e => e.Displayed)
                    .ToList();

                var match = candidates.FirstOrDefault(e =>
                {
                    var cls = e.GetAttribute("class") ?? string.Empty;
                    return cls.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                              .Contains(expectedClass, StringComparer.OrdinalIgnoreCase)
                           && matchesSign(e);
                });

                if (match != null)
                    return true;

                // Scroll a bit and retry (works for body scroll and most scroll containers).
                _driver.FindElement(By.TagName("body")).SendKeys(Keys.PageDown);
                Thread.Sleep(150);
            }

            return false;
        }

    }
}
