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
        public void StartNewSessionWithStartingBalance(decimal startingBalance)
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

        private By TickerSearchBox =>
            By.CssSelector("input[matinput]");

        private By TickerSearchBoxSell =>
            By.CssSelector("input[matinput]");

        private By TickerResult(string ticker) =>
            By.XPath($"//span[contains(@class,'mdc-list-item') and contains(., '{ticker}')]");

        private By QuantityInput =>
            By.CssSelector("input#quantity");

        private By BuyButton =>
            By.XPath("//button[contains(., 'BUY')]");

        private By BuyToggleButton =>
            By.XPath("//button[contains(@class, 'action-btn') and contains(., 'Buy')]");
        private By MaxButton =>
            By.XPath("//button[contains(., 'Max')]");

        private By HoldingsRow(string ticker) =>
            By.XPath($"//span[contains(@class, 'holding-shares')]");

        private By AvailableBalanceLabel =>
            By.CssSelector(".cash-value");

        private By ConfirmationMessage =>
            By.CssSelector(".alert-success.alert");

        private By ErrorMessage =>
            By.CssSelector(".validation-msg");

        private By SellButton =>
            By.XPath("//button[contains(., 'SELL')]");

        private By SellToggleButton =>
            By.XPath("//button[contains(@class, 'action-btn') and contains(., 'Sell')]");

        // -------- Page Actions --------

        public void SelectTicker(string ticker) {

            Wait.Until(ExpectedConditions.ElementToBeClickable(TickerSearchBox)).Click();
            Wait.Until(ExpectedConditions.ElementToBeClickable(TickerSearchBox)).Clear();
            Wait.Until(ExpectedConditions.ElementToBeClickable(TickerSearchBox))
                .SendKeys(ticker);
            Wait.Until(ExpectedConditions.ElementToBeClickable(TickerSearchBox)).SendKeys(Keys.Enter);

            Wait.Until(d => d.FindElement(TickerResult(ticker))).Click();
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

        public void ClickBuy() {
            Wait.Until(ExpectedConditions.ElementToBeClickable(BuyButton)).Click();
        }

        public void ClickBuyToggle() {
            Wait.Until(ExpectedConditions.ElementToBeClickable(BuyToggleButton)).Click();
        }

        public void ClickSellToggle() {

            // Small wait for nav bar to settle
            Wait.Until(ExpectedConditions.ElementToBeClickable(SellToggleButton)).Click();
        }

        public bool DoesMaxButtonExist() {
            return Driver.FindElements(MaxButton).Count > 0;
        }

        public void ClickMax() {
            System.Threading.Thread.Sleep(1000);

            Driver.FindElement(MaxButton).Click();
        }

        public bool IsBuyButtonEnabled() {
            return Driver.FindElement(BuyButton).Enabled;
        }

        public bool IsBuyToggleButtonEnabled() {
            return Driver.FindElement(BuyToggleButton).Enabled;
        }

        public string GetAvailableBalance() {
            return Wait.Until(d => d.FindElement(AvailableBalanceLabel)).Text;
        }

        public bool IsTickerInHoldings(string ticker) {
            return Wait.Until(d => d.FindElement(HoldingsRow(ticker))).Displayed;
        }

        public string GetConfirmationMessage() {
            return Wait.Until(d => d.FindElement(ConfirmationMessage)).Text;
        }

        public string GetErrorMessage() {
            return Wait.Until(d => d.FindElement(ErrorMessage)).Text;
        }

        public void ClickTickerInHoldings(string ticker) {
            Wait.Until(d => d.FindElement(HoldingsRow(ticker))).Click();
        }

        public void ClickSell() {
            Wait.Until(ExpectedConditions.ElementToBeClickable(SellButton)).Click();
        }

        public bool IsSellButtonEnabled() {
            return Driver.FindElement(SellButton).Enabled;
        }

        public int GetHoldingQuantity(string ticker) {
            var row = Wait.Until(d => d.FindElement(HoldingsRow(ticker)));
            var qtyText = row.Text;

            var parts = qtyText.Split(' ');
            return int.Parse(parts[0]);
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