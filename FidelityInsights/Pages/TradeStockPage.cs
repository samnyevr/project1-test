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
        }

        public void StartNewSession() {
            IJavaScriptExecutor jexecutor = (IJavaScriptExecutor)Driver;
            jexecutor.ExecuteScript("console.log('Setting simulation ID in local storage');");
            var currentSimId = jexecutor.ExecuteScript("return window.localStorage.getItem('currentSimulationId');") as string;
            // Driver.Navigate().Refresh();

            // (Placeholder) Start a new training session, probably will be abstracted by another test in the future
            // Step 1: Click "Start New Trading Session"
            if (string.IsNullOrEmpty(currentSimId)) {
                Wait.Until(d => d.FindElement(By.XPath("//button[contains(@class, 'btn-primary') and contains(text(), 'Start New Trading Session')]"))).Click();

                // Step 2: Click "Start New Training Session"
                Wait.Until(d => d.FindElement(By.XPath("//button[contains(@class, 'btn-primary') and contains(text(), 'Start New Training Session')]"))).Click();

                // Step 3: Click "Start Training Session"
                Wait.Until(d => d.FindElement(By.XPath("//button[contains(@class, 'btn-primary') and contains(text(), ' Start Training Session')]"))).Click();
            }
            // Wait until truly clickable
            Wait.Until(ExpectedConditions.ElementToBeClickable(
                By.XPath("//button[contains(., 'Jump to Date')]")
            ));

            // Re-find and click
            Driver.FindElement(By.XPath("//button[contains(., 'Jump to Date')]")).Click();

            // 2) Wait UNTIL the calendar is actually visible
            Wait.Until(d =>
                d.FindElement(By.CssSelector("button.calendar-day")).Displayed
            );

            // 3) NOW click day 18 (re-find it fresh)
            Wait.Until(d =>
                d.FindElement(
                    By.XPath("//button[contains(@class,'calendar-day') and normalize-space()='18']")
                )
            ).Click();
        }

        // Add a method to start a new session with a specific starting balance
        public void StartNewSessionWithStartingBalance(decimal startingBalance, bool forceNew = true)
        {
            IJavaScriptExecutor jexecutor = (IJavaScriptExecutor)Driver;

            jexecutor.ExecuteScript("console.log('Setting simulation ID in local storage');");
            var currentSimId = jexecutor.ExecuteScript("return window.localStorage.getItem('currentSimulationId');") as string;
            if (string.IsNullOrEmpty(currentSimId)) {
                // Step 1: Click "Start New Trading Session"
                Wait.Until(d => d.FindElement(By.XPath("//button[contains(@class, 'btn-primary') and contains(text(), 'Start New Trading Session')]"))).Click();

                // Step 2: Click "Start New Training Session"
                Wait.Until(d => d.FindElement(By.XPath("//button[contains(@class, 'btn-primary') and contains(text(), 'Start New Training Session')]"))).Click();

                // Step 3: Wait for the modal/form to be visible
                var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(10));
                wait.Until(d => d.FindElements(By.CssSelector("app-training-session-modal form.settings-form, app-training-session-modal .settings-form"))
                                 .Any(e => e.Displayed));

                // Step 4: Wait for the input to be present and enabled
                var balanceInput = wait.Until(d =>
                {
                    var el = d.FindElements(By.CssSelector("input#starting-balance, input[name='startingBalance']")).FirstOrDefault(e => e.Displayed && e.Enabled);
                    return el;
                });
                balanceInput.Clear();
                balanceInput.SendKeys(startingBalance.ToString(CultureInfo.InvariantCulture));

                // Step 5: Click Start Training Session
                Wait.Until(d => d.FindElement(By.XPath("//button[contains(@class, 'btn-primary') and contains(text(), 'Start Training Session')]"))).Click();
            }
            // Wait until truly clickable
            Wait.Until(ExpectedConditions.ElementToBeClickable(
                By.XPath("//button[contains(., 'Jump to Date')]")
            ));
            Driver.FindElement(By.XPath("//button[contains(., 'Jump to Date')]")).Click();
            Wait.Until(d => d.FindElement(By.CssSelector("button.calendar-day")).Displayed);
            Wait.Until(d => d.FindElement(
                By.XPath("//button[contains(@class,'calendar-day') and normalize-space()='18']")
            )).Click();
        }

        // -------- Locators (adjust as needed) --------
        private By HoldingsSection =>
            By.CssSelector(".holdings-section");

        private By EmptyHoldings =>
            By.CssSelector(".holdings-section .empty-holdings");

        private By HoldingCard(string ticker) =>
            By.XPath(
                $"//div[contains(@class,'holdings-section')]//div[contains(@class,'holding-card')]" +
                $"[.//span[contains(@class,'holding-ticker') and normalize-space()='{ticker}']]"
            );


        // --- Trade action toggle buttons
        private By BuyToggleButton =>
            By.XPath("//button[contains(@class,'action-btn') and normalize-space()='Buy']");

        private By SellToggleButton =>
            By.XPath("//button[contains(@class,'action-btn') and normalize-space()='Sell']");

        // --- Ticker autocomplete input (inside app-auto-complete)
        private By TickerSearchBox =>
            By.CssSelector("app-auto-complete.ticker-autocomplete input, .ticker-autocomplete input");

        private By BuyButton =>
            By.XPath("//button[starts-with(normalize-space(.),'BUY')]");

        private By SellButton =>
            By.XPath("//button[starts-with(normalize-space(.),'SELL')]");

        // --- Autocomplete options (Angular CDK / generic list)
        private By TickerOption(string ticker) =>
            By.XPath(
                $"//*[@role='option' or self::mat-option or self::li or self::div]" +
                $"[contains(@class,'option') or contains(@class,'mat') or contains(@class,'item') or @role='option']" +
                $"[normalize-space()='{ticker}' or contains(normalize-space(.),'{ticker}')]"
            );

        // --- Quantity input + Max button (new template has these)
        private By QuantityInput => By.CssSelector("input#quantity");
        private By MaxButton => By.CssSelector("button.max-btn");

        // --- Execute button is shared for BUY/SELL
        private By ExecuteButton => By.CssSelector("button.execute-btn");

        // --- Cash
        private By AvailableBalanceLabel => By.CssSelector(".cash-value");

        // --- Messages (new template)
        private By ConfirmationMessage => By.CssSelector(".alert.alert-success");
        private By ErrorMessage => By.CssSelector(".validation-msg, .alert.alert-error");

        // --- Holdings (new template)
        private By HoldingsRow(string ticker) =>
            By.XPath($"//div[contains(@class,'holding-card')]//span[contains(@class,'holding-ticker') and normalize-space()='{ticker}']");

        private By TickerSearchBoxSell =>
            By.CssSelector("input[matinput]");

        private By TickerResult(string ticker) =>
            By.XPath($"//span[contains(@class,'mdc-list-item') and contains(., '{ticker}')]");

        // -------- Page Actions --------

        /* public void SelectTicker(string ticker) {

            Wait.Until(ExpectedConditions.ElementToBeClickable(TickerSearchBox)).Click();
            Wait.Until(ExpectedConditions.ElementToBeClickable(TickerSearchBox)).Clear();
            Wait.Until(ExpectedConditions.ElementToBeClickable(TickerSearchBox))
                .SendKeys(ticker);
            Wait.Until(ExpectedConditions.ElementToBeClickable(TickerSearchBox)).SendKeys(Keys.Enter);

            Wait.Until(d => d.FindElement(TickerResult(ticker))).Click();
        } */

        public void SelectTicker(string ticker)
        {
            var js = (IJavaScriptExecutor)Driver;
            js.ExecuteScript("window.scrollTo(0, 0);");

            // ensure BUY tab (optional but consistent)
            Wait.Until(ExpectedConditions.ElementToBeClickable(BuyToggleButton)).Click();

            var input = Wait.Until(ExpectedConditions.ElementToBeClickable(TickerSearchBox));
            input.Click();
            input.Clear();
            input.SendKeys(ticker);

            // wait for dropdown to render, then pick the ticker
            var option = Wait.Until(ExpectedConditions.ElementToBeClickable(TickerOption(ticker)));
            option.Click();

            // confirm selection propagated (chart overlay text disappears when selectedTicker() is set)
            Wait.Until(d => !d.PageSource.Contains("Select a stock to view its price chart"));

            WaitForPrice();
        }

        private By CurrentPriceValue => By.CssSelector(".price-display .price-value");

        private void WaitForPrice()
        {
            Wait.Until(d => d.FindElements(CurrentPriceValue).Count > 0);
            var txt = Driver.FindElement(CurrentPriceValue).Text; // "$6.00"
            Wait.Until(_ => !string.IsNullOrWhiteSpace(txt) && txt.Contains("$"));
        }


        public void SelectTickerSell(string ticker) {

            Wait.Until(ExpectedConditions.ElementToBeClickable(TickerSearchBox)).Click();
            Wait.Until(ExpectedConditions.ElementToBeClickable(TickerSearchBox)).Clear();
            Wait.Until(ExpectedConditions.ElementToBeClickable(TickerSearchBoxSell))
                .SendKeys(ticker);
            Wait.Until(ExpectedConditions.ElementToBeClickable(TickerSearchBox)).SendKeys(Keys.Enter);

            Wait.Until(d => d.FindElement(TickerResult(ticker))).Click();
        }

        public void EnterQuantity(string quantity) {
            var input = Wait.Until(d => d.FindElement(QuantityInput));
            input.Clear();
            input.SendKeys(quantity);
        }

        public void ClickBuy()
        {
            var btn = Wait.Until(d => d.FindElement(ExecuteButton));
            ((IJavaScriptExecutor)Driver).ExecuteScript("arguments[0].scrollIntoView({block:'center'});", btn);

            // Wait until it becomes enabled (Max + price must be loaded)
            Wait.Until(d => d.FindElement(ExecuteButton).Enabled);

            btn = Driver.FindElement(ExecuteButton);
            btn.Click();

            // Wait for success or error message so the test doesn't race
            Wait.Until(d => d.FindElements(ConfirmationMessage).Count > 0 || d.FindElements(ErrorMessage).Count > 0);
        }

        public void ClickSell()
        {
            Wait.Until(ExpectedConditions.ElementToBeClickable(SellToggleButton)).Click();
            Wait.Until(ExpectedConditions.ElementToBeClickable(ExecuteButton)).Click();
        }

        public void ClickSellToggle()
        {
            var btn = Wait.Until(ExpectedConditions.ElementExists(SellToggleButton));
            ((IJavaScriptExecutor)Driver).ExecuteScript("arguments[0].scrollIntoView({block:'center'});", btn);

            // Scroll so the button is not under the sticky header / overlay
            ScrollIntoViewCenter(btn);

            // Optional: small settle for Angular animations/toolbar
            Wait.Until(d => btn.Displayed && btn.Enabled);

            TryClickWithFallback(btn);
        }

        public void ClickBuyToggle()
        {
            var btn = Wait.Until(ExpectedConditions.ElementExists(BuyToggleButton));
            ((IJavaScriptExecutor)Driver).ExecuteScript("arguments[0].scrollIntoView({block:'center'});", btn);
            ScrollIntoViewCenter(btn);
            Wait.Until(d => btn.Displayed && btn.Enabled);
            TryClickWithFallback(btn);
        }

        private void ScrollIntoViewCenter(IWebElement el)
        {
            var js = (IJavaScriptExecutor)Driver;
            js.ExecuteScript(
                "arguments[0].scrollIntoView({block:'center', inline:'center'});",
                el
            );
        }

        private void TryClickWithFallback(IWebElement el)
        {
            try
            {
                Wait.Until(ExpectedConditions.ElementToBeClickable(el)).Click();
            }
            catch (OpenQA.Selenium.ElementClickInterceptedException)
            {
                // If something still overlays it, click via JS as a fallback
                var js = (IJavaScriptExecutor)Driver;
                js.ExecuteScript("arguments[0].click();", el);
            }
            catch (StaleElementReferenceException)
            {
                // If the DOM re-rendered, re-find and retry once (optional)
                var js = (IJavaScriptExecutor)Driver;
                js.ExecuteScript("arguments[0].click();", el);
            }
        }


        public bool DoesMaxButtonExist() {
            return Driver.FindElements(MaxButton).Count > 0;
        }

        public void ClickMax()
        {
            // Click the Max button
            Wait.Until(ExpectedConditions.ElementToBeClickable(MaxButton)).Click();

            // Wait until quantity input value is > 0
            Wait.Until(d =>
            {
                var val = d.FindElement(QuantityInput).GetAttribute("value");
                return int.TryParse(val, out var n) && n > 0;
            });
        }

        public bool IsBuyButtonEnabled() =>
            Wait.Until(d => d.FindElement(ExecuteButton)).Enabled;

        public bool IsSellButtonEnabled() =>
            Wait.Until(d => d.FindElement(ExecuteButton)).Enabled;

        public bool IsBuyToggleButtonEnabled() {
            return Driver.FindElement(BuyToggleButton).Enabled;
        }

        public string GetAvailableBalance()
        {
            return Wait.Until(d => d.FindElement(AvailableBalanceLabel)).Text;
        }

        public bool IsTickerInHoldings(string ticker)
        {
            // Scroll holdings section into view (important)
            var section = Wait.Until(d => d.FindElement(HoldingsSection));
            ((IJavaScriptExecutor)Driver).ExecuteScript("arguments[0].scrollIntoView({block:'center'});", section);

            // Wait until either:
            // - the holding card exists, OR
            // - the empty-holdings block is gone (meaning list is rendering), then check again
            return Wait.Until(d =>
            {
                if (d.FindElements(HoldingCard(ticker)).Count > 0) return true;

                // If holdings are still empty, keep waiting
                if (d.FindElements(EmptyHoldings).Count > 0) return false;

                // holdings-list may be rendering but not yet containing this ticker
                return d.FindElements(HoldingCard(ticker)).Count > 0;
            });
        }


        public string GetConfirmationMessage() {
            return Wait.Until(d => d.FindElement(ConfirmationMessage)).Text;
        }

        public string GetErrorMessage() {
            return Wait.Until(d => d.FindElement(ErrorMessage)).Text;
        }

        public void ClickTickerInHoldings(string ticker)
        {
            var card = Wait.Until(ExpectedConditions.ElementToBeClickable(HoldingCard(ticker)));
            ((IJavaScriptExecutor)Driver).ExecuteScript("arguments[0].scrollIntoView({block:'center'});", card);
            card.Click();
        }

        public int GetHoldingQuantity(string ticker)
        {
            var card = Wait.Until(d => d.FindElement(HoldingCard(ticker)));
            var sharesText = card.FindElement(By.CssSelector(".holding-shares")).Text; // "100 shares"

            var match = System.Text.RegularExpressions.Regex.Match(sharesText, @"(\d[\d,]*)\s+shares");
            if (!match.Success) throw new Exception($"Could not parse shares text: {sharesText}");

            return int.Parse(match.Groups[1].Value.Replace(",", ""));
        }

        public int GetQuantity() {
            var input = Wait.Until(d => d.FindElement(QuantityInput));
            var qtyText = input.GetAttribute("value");
            return int.Parse(qtyText);
        }

        public bool IsTickerGoneFromHoldings(string ticker) {
            return Driver.FindElements(HoldingsRow(ticker)).Count == 0;
        }

    }
}