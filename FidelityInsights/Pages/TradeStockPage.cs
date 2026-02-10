using FidelityInsights.Support;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System;
using System.Globalization;

namespace FidelityInsights.Pages {
    public class TradeStockPage : AbstractPage {

        private const string TradeStockUrl = "https://d2rczu3zvi71ix.cloudfront.net/trade";
        public TradeStockPage(DriverContext ctx) : base(ctx.Driver) { }

        public void Open() {
            Driver.Navigate().GoToUrl(TradeStockUrl);
            Driver.Manage().Window.Size = new System.Drawing.Size(1920, 1080);
            var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(30));
            wait.Until(d =>
                ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").ToString() == "complete"
                &&
                ((IJavaScriptExecutor)d).ExecuteScript("return (window.ng && window.ng.getComponent) || true;")
                    .ToString() != "false"
            );
        }

        // ----------------- Robust Helpers -----------------

        public void ClickWithRetry(By locator, int retries = 3, int waitSeconds = 10) {
            for (int attempt = 0; attempt < retries; attempt++) {
                try {
                    var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(waitSeconds));
                    var el = wait.Until(ExpectedConditions.ElementToBeClickable(locator));
                    ((IJavaScriptExecutor)Driver).ExecuteScript(
                        "arguments[0].scrollIntoView({block:'center', inline:'center'});", el);
                    el.Click();
                    return;
                } catch (StaleElementReferenceException) {
                    Thread.Sleep(300);
                } catch (ElementClickInterceptedException) {
                    var el = Driver.FindElement(locator);
                    ((IJavaScriptExecutor)Driver).ExecuteScript("arguments[0].click();", el);
                    return;
                } catch (WebDriverTimeoutException) when (attempt < retries - 1) {
                    Thread.Sleep(300);
                }
            }
            throw new Exception($"Failed to click element after {retries} attempts: {locator}");
        }

        // FIX: return the element on success, throw on failure instead of returning null.
        // Returning null causes WebDriverWait to keep retrying until hard timeout with no useful error.
        public IWebElement WaitForVisible(By locator, int waitSeconds = 10) {
            var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(waitSeconds));
            wait.Message = $"Element never became visible: {locator}";
            return wait.Until(ExpectedConditions.ElementIsVisible(locator));
        }

        public IWebElement WaitForEnabled(By locator, int waitSeconds = 10) {
            var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(waitSeconds));
            wait.Message = $"Element never became enabled: {locator}";
            return wait.Until(d => {
                var el = d.FindElement(locator);
                if (el.Displayed && el.Enabled) return el;
                throw new WebDriverException($"Element not yet enabled: {locator}");
            });
        }

        // ----------------- Session Setup -----------------

        /// <summary>
        /// Starts a new session (or resumes existing) and navigates to day 18.
        /// Pass startingBalance to set a custom balance; null uses the app default.
        /// </summary>
        public void StartNewSession(decimal? startingBalance = null) {
            var jexecutor = (IJavaScriptExecutor)Driver;
            var currentSimId = jexecutor.ExecuteScript(
                "return window.localStorage.getItem('currentSimulationId');") as string;

            if (string.IsNullOrEmpty(currentSimId)) {
                Wait.Until(ExpectedConditions.ElementToBeClickable(
                    By.XPath("//button[contains(@class,'btn-primary') and contains(text(),'Start New Trading Session')]")
                )).Click();

                Wait.Until(ExpectedConditions.ElementToBeClickable(
                    By.XPath("//button[contains(@class,'btn-primary') and contains(text(),'Start New Training Session')]")
                )).Click();

                if (startingBalance.HasValue) {
                    // Wait for the settings form
                    Wait.Until(ExpectedConditions.ElementIsVisible(
                        By.CssSelector("app-training-session-modal .settings-form")));

                    var balanceInput = Wait.Until(d =>
                        d.FindElements(By.CssSelector("input#starting-balance, input[name='startingBalance']"))
                         .FirstOrDefault(e => e.Displayed && e.Enabled));
                    balanceInput.Clear();
                    balanceInput.SendKeys(startingBalance.Value.ToString(CultureInfo.InvariantCulture));
                }

                Wait.Until(ExpectedConditions.ElementToBeClickable(
                    By.XPath("//button[contains(@class,'btn-primary') and contains(text(),'Start Training Session')]")
                )).Click();
            }

            // Jump to date 18
            Wait.Until(ExpectedConditions.ElementToBeClickable(
                By.XPath("//button[contains(.,'Jump to Date')]")
            )).Click();

            Wait.Until(ExpectedConditions.ElementIsVisible(
                By.CssSelector("button.calendar-day")));

            Wait.Until(ExpectedConditions.ElementToBeClickable(
                By.XPath("//button[contains(@class,'calendar-day') and normalize-space()='18']")
            )).Click();
        }

        // Keep named overload for backward compatibility
        public void StartNewSessionWithStartingBalance(decimal startingBalance) =>
            StartNewSession(startingBalance);

        // -------- Locators --------

        private By HoldingsSection => By.CssSelector(".holdings-section");
        private By EmptyHoldings => By.CssSelector(".holdings-section .empty-holdings");

        private By HoldingCard(string ticker) =>
            By.XPath(
                $"//div[contains(@class,'holdings-section')]" +
                $"//div[contains(@class,'holding-card')]" +
                $"[.//span[contains(@class,'holding-ticker') and normalize-space()='{ticker}']]"
            );

        private By BuyToggleButton => By.CssSelector(".action-toggle .action-btn.buy");
        private By SellToggleButton => By.CssSelector(".action-toggle .action-btn:not(.buy)");

        private By TickerSearchBox =>
            By.CssSelector(".ticker-autocomplete input[matinput]");

        private By TickerOption(string ticker) =>
            By.CssSelector(".mat-mdc-option.mdc-list-item");

        private By QuantityInput => By.CssSelector("input#quantity");
        private By MaxButton => By.CssSelector("button.max-btn");
        private By ExecuteButton => By.CssSelector("button.execute-btn");
        private By AvailableBalanceLabel => By.CssSelector(".cash-value");
        private By ConfirmationMessage => By.CssSelector(".alert.alert-success");
        private By ErrorMessage => By.CssSelector(".validation-msg, .alert.alert-error");
        private By CurrentPriceValue => By.CssSelector(".price-display .price-value");

        private By HoldingsRow(string ticker) =>
            By.XPath(
                $"//div[contains(@class,'holding-card')]" +
                $"//span[contains(@class,'holding-ticker') and normalize-space()='{ticker}']"
            );

        // TickerSearchBoxSell intentionally reuses TickerSearchBox — same input element

        private By TickerResult(string ticker) =>
            By.XPath($"//span[contains(@class,'mdc-list-item') and contains(.,'{ticker}')]");

        // -------- Page Actions --------

        public void SelectTicker(string ticker) {
            var js = (IJavaScriptExecutor)Driver;
            js.ExecuteScript("window.scrollTo(0, 0);");

            ClickWithRetry(BuyToggleButton);

            var input = Wait.Until(ExpectedConditions.ElementToBeClickable(TickerSearchBox));
            input.Click();
            input.Clear();
            input.SendKeys(ticker);

            var option = Wait.Until(ExpectedConditions.ElementToBeClickable(TickerOption(ticker)));
            option.Click();

            Wait.Until(d => !d.PageSource.Contains("Select a stock to view its price chart"));
            WaitForPrice();
        }

        // FIX: was capturing txt once before the wait loop — always checked a stale value.
        // Now reads the element text fresh on every poll iteration.
        private void WaitForPrice() {
            Wait.Until(d => {
                var els = d.FindElements(CurrentPriceValue);
                if (els.Count == 0) return false;
                var txt = els[0].Text;
                return !string.IsNullOrWhiteSpace(txt) && txt.Contains("$");
            });
        }

        public void SelectTickerSell(string ticker) {
            Wait.Until(ExpectedConditions.ElementToBeClickable(TickerSearchBox)).Click();
            Wait.Until(ExpectedConditions.ElementToBeClickable(TickerSearchBox)).Clear();
            Wait.Until(ExpectedConditions.ElementToBeClickable(TickerSearchBox)).SendKeys(ticker);
            Wait.Until(ExpectedConditions.ElementToBeClickable(TickerSearchBox)).SendKeys(Keys.Enter);
            ClickWithRetry(TickerResult(ticker));
        }

        public void EnterQuantity(string quantity) {
            var input = Wait.Until(ExpectedConditions.ElementIsVisible(QuantityInput));
            input.Clear();
            input.SendKeys(quantity);
        }

        public void ClickBuy() {
            // FIX: was finding element twice without re-waiting between calls — second find could be stale.
            // Now waits for enabled state, then re-finds once cleanly before clicking.
            var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(10));
            var btn = wait.Until(d => {
                var el = d.FindElement(ExecuteButton);
                ((IJavaScriptExecutor)Driver).ExecuteScript(
                    "arguments[0].scrollIntoView({block:'center'});", el);
                return el.Enabled ? el : null;
            });
            btn.Click();

            // Wait for success or error so the test doesn't race ahead
            Wait.Until(d =>
                d.FindElements(ConfirmationMessage).Count > 0 ||
                d.FindElements(ErrorMessage).Count > 0
            );
        }

        public void ClickSell() {
            Wait.Until(ExpectedConditions.ElementToBeClickable(SellToggleButton)).Click();
            Wait.Until(ExpectedConditions.ElementToBeClickable(ExecuteButton)).Click();
        }

        public void ClickSellToggle() {
            var btn = Wait.Until(ExpectedConditions.ElementExists(SellToggleButton));
            ScrollIntoViewCenter(btn);
            Wait.Until(d => btn.Displayed && btn.Enabled);
            TryClickWithFallback(btn);
        }

        public void ClickBuyToggle() {
            var btn = Wait.Until(ExpectedConditions.ElementExists(BuyToggleButton));
            ScrollIntoViewCenter(btn);
            Wait.Until(d => btn.Displayed && btn.Enabled);
            TryClickWithFallback(btn);
        }

        private void ScrollIntoViewCenter(IWebElement el) =>
            ((IJavaScriptExecutor)Driver).ExecuteScript(
                "arguments[0].scrollIntoView({block:'center', inline:'center'});", el);

        private void TryClickWithFallback(IWebElement el) {
            try {
                Wait.Until(ExpectedConditions.ElementToBeClickable(el)).Click();
            } catch (ElementClickInterceptedException) {
                ((IJavaScriptExecutor)Driver).ExecuteScript("arguments[0].click();", el);
            } catch (StaleElementReferenceException) {
                ((IJavaScriptExecutor)Driver).ExecuteScript("arguments[0].click();", el);
            }
        }

        public bool DoesMaxButtonExist() =>
            Driver.FindElements(MaxButton).Count > 0;

        public void ClickMax() {
            Wait.Until(ExpectedConditions.ElementToBeClickable(MaxButton)).Click();
            Wait.Until(d => {
                var val = d.FindElement(QuantityInput).GetAttribute("value");
                return int.TryParse(val, out var n) && n > 0;
            });
        }

        public bool IsBuyButtonEnabled() =>
            Wait.Until(ExpectedConditions.ElementExists(ExecuteButton)).Enabled;

        public bool IsSellButtonEnabled() =>
            Wait.Until(ExpectedConditions.ElementExists(ExecuteButton)).Enabled;

        public bool IsBuyToggleButtonEnabled() =>
            Driver.FindElement(BuyToggleButton).Enabled;

        public string GetAvailableBalance() =>
            Wait.Until(ExpectedConditions.ElementIsVisible(AvailableBalanceLabel)).Text;

        /// <summary>
        /// Waits until the displayed balance drops below the given threshold.
        /// Use instead of Thread.Sleep after a purchase that drains funds.
        /// </summary>
        public void WaitForBalanceBelow(decimal threshold, int waitSeconds = 10) {
            var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(waitSeconds));
            wait.Message = $"Balance never dropped below {threshold}";
            wait.Until(_ => {
                var text = GetAvailableBalance().Replace("$", "").Replace(",", "");
                return decimal.TryParse(text, out var actual) && actual < threshold;
            });
        }

        public bool IsTickerInHoldings(string ticker) {
            var section = Wait.Until(d => d.FindElement(HoldingsSection));
            ((IJavaScriptExecutor)Driver).ExecuteScript(
                "arguments[0].scrollIntoView({block:'center'});", section);

            return Wait.Until(d => {
                if (d.FindElements(HoldingCard(ticker)).Count > 0) return true;
                if (d.FindElements(EmptyHoldings).Count > 0) return false;
                return d.FindElements(HoldingCard(ticker)).Count > 0;
            });
        }

        public string GetConfirmationMessage() =>
            Wait.Until(ExpectedConditions.ElementIsVisible(ConfirmationMessage)).Text;

        public string GetErrorMessage() =>
            Wait.Until(ExpectedConditions.ElementIsVisible(ErrorMessage)).Text;

        public void ClickTickerInHoldings(string ticker) {
            var card = Wait.Until(ExpectedConditions.ElementToBeClickable(HoldingCard(ticker)));
            ((IJavaScriptExecutor)Driver).ExecuteScript(
                "arguments[0].scrollIntoView({block:'center'});", card);
            card.Click();
        }

        public int GetHoldingQuantity(string ticker) {
            var card = Wait.Until(d => d.FindElement(HoldingCard(ticker)));
            var sharesText = card.FindElement(By.CssSelector(".holding-shares")).Text;
            var match = System.Text.RegularExpressions.Regex.Match(sharesText, @"(\d[\d,]*)\s+shares");
            if (!match.Success) throw new Exception($"Could not parse shares text: '{sharesText}'");
            return int.Parse(match.Groups[1].Value.Replace(",", ""));
        }

        public int GetQuantity() {
            var input = Wait.Until(ExpectedConditions.ElementIsVisible(QuantityInput));
            return int.Parse(input.GetAttribute("value"));
        }

        // FIX: now uses HoldingCard (same locator as everywhere else) instead of HoldingsRow.
        // Using a different locator for the same element was causing inconsistent results.
        public bool IsTickerGoneFromHoldings(string ticker) =>
            Driver.FindElements(HoldingCard(ticker)).Count == 0;
    }
}