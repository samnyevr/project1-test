using FidelityInsights.Pages;
using NUnit.Framework;

using System.Globalization;

namespace FidelityInsights.StepDefinitions {
    [Binding]
    public class SellStockSteps {
        private readonly TradeStockPage _tradePage;
        private readonly ScenarioContext _scenario;

        public SellStockSteps(TradeStockPage tradePage, ScenarioContext scenario) {
            _tradePage = tradePage;
            _scenario = scenario;
        }

        // --------- Shared setup: always buy first ----------
        private void BuyAALFirst(string quantity = "100") {
            _tradePage.Open();
            // Generate a unique starting balance for this scenario
            var uniqueBalance = GenerateUniqueStartingBalance();
            _tradePage.StartNewSessionWithStartingBalance(uniqueBalance);
            _scenario["CreatedSession"] = true;
            _scenario["CreatedStartingBalanceUi"] = uniqueBalance.ToString("C2", CultureInfo.GetCultureInfo("en-US"));
            _tradePage.SelectTicker("AAL");
            if (quantity == "MAX") {
                _tradePage.ClickMax();
            } else {

                _tradePage.EnterQuantity(quantity);
            }
            _tradePage.ClickBuy();
        }

        private decimal GenerateUniqueStartingBalance() {
            // Use a large base value plus ticks to ensure uniqueness
            var cents = DateTime.UtcNow.Ticks % 100;
            return 909090909m + (cents / 100m);
        }

        // --------- GIVEN: unique to SELL ------------------

        [Given("the trader selects \"(.*)\" for selling")]
        public void GivenTraderSelectsForSelling(string ticker) {
            BuyAALFirst("150");   // make sure we own shares before selling
            _tradePage.ClickSellToggle();
            try {
                System.Threading.Thread.Sleep(500); // wait for toggle to take effect 
                _tradePage.SelectTicker(ticker);
            } catch (Exception) {

                _tradePage.SelectTickerSell(ticker);

            }
        }

        [Given("after owning only 20 shares the trader selects \"(.*)\" for selling")]
        public void GivenAfterOwningTraderSelectsForSelling(string ticker) {
            _tradePage.ClickSellToggle();
            try {
                System.Threading.Thread.Sleep(500); // wait for toggle to take effect 
                _tradePage.SelectTicker(ticker);
            } catch (Exception) {

                _tradePage.SelectTickerSell(ticker);

            }

        }

        [When("after owning only 20 shares the trader inputs a sell Quantity of \"(.*)\" shares")]
        public void GivenTraderInputsSellQuantityWithInsufficientShares(string qty) {
            System.Threading.Thread.Sleep(500); // wait for toggle to take effect

            _tradePage.EnterQuantity(qty);
        }

        // --------- WHEN steps (SELL-specific wording) -----

        [When("the trader inputs a sell Quantity of \"(.*)\" shares")]
        public void WhenTraderInputsSellQuantity(string qty) {
            _tradePage.EnterQuantity(qty);
        }

        [When("the trader click on the Sell button")]
        public void WhenTraderClicksSell() {
            _tradePage.ClickSell();
            System.Threading.Thread.Sleep(500); // wait for confirmation message to appear
        }

        [When("the trader click on the Max button on the Sell Stock page")]
        public void WhenTraderClicksMaxOnSellStockPage() {
            BuyAALFirst("MAX");   // ensure we have plenty to sell
            _tradePage.ClickSellToggle();
            System.Threading.Thread.Sleep(500); // wait for toggle to take effect

            _tradePage.SelectTicker("AAL");
            _tradePage.ClickMax();
        }

        // --------- THEN steps -----------------------------

        [Then("the \"(.*)\" \"(.*)\" shares should be removed from Your Holdings section")]
        public void ThenSharesRemoved(string qty, string ticker) {
            Assert.That(_tradePage.GetHoldingQuantity(ticker), Is.EqualTo(100));
        }

        [Then("all available \"(.*)\" shares should be removed from Your Holdings section")]
        public void ThenAllSharesRemoved(string ticker) {
            Assert.That(_tradePage.IsTickerGoneFromHoldings(ticker), Is.True);
        }

        [When("all available {string} shares should be shown in the Quantity input field")]
        public void ThenAllAvailableSharesShouldBeShownInTheQuantityInputField(string ticker) {
            // Check that the Quantity input field shows the number of shares owned for this ticker
            int quantity = _tradePage.GetHoldingQuantity(ticker);
            Assert.That(_tradePage.GetQuantity(), Is.EqualTo(quantity));

            System.Threading.Thread.Sleep(2000); // wait for button state to update
        }

        [Then("the available balance should be increased accordingly")]
        public void ThenBalanceIncreased() {
            Assert.That(_tradePage.GetAvailableBalance(), Is.Not.Null.And.Not.Empty);
        }

        [Then("the trader is shown a sold confirmation message \"(.*)\"")]
        public void ThenConfirmationMessageShown(string expected) {
            Assert.That(_tradePage.GetConfirmationMessage(), Is.EqualTo(expected));
        }

        [Then("the Sell button should be disabled")]
        public void ThenSellButtonDisabled() {
            System.Threading.Thread.Sleep(5000); // wait for button state to update
            Assert.That(_tradePage.IsSellButtonEnabled(), Is.False);
        }

        [Then("the trader is shown an sold error message \"(.*)\"")]
        public void ThenErrorMessageShown(string expected) {
            Assert.That(_tradePage.GetErrorMessage(), Is.EqualTo(expected));
        }

        // --------- HOLDINGS SCENARIOS ---------------------

        [Given("the trader holds \"(.*)\" shares of \"(.*)\"")]
        public void GivenTraderHoldsShares(string qty, string ticker) {
            BuyAALFirst(qty);
            Assert.That(_tradePage.GetHoldingQuantity(ticker), Is.EqualTo(int.Parse(qty)));
        }

        [Given("the trader holds no shares of \"(.*)\"")]
        public void GivenTraderHoldsNoShares(string ticker) {
            _tradePage.Open();
            _tradePage.StartNewSession();
        }

        [Given("the trader click on \"(.*)\" in Your Holdings section")]
        public void GivenTraderClicksTickerInHoldings(string ticker) {
            BuyAALFirst("120");   // ensure we have something to quick-sell
            _tradePage.ClickTickerInHoldings(ticker);
        }

        [Then("the trader can click on the Sell button")]
        public void ThenTraderCanClickSell() {
            Assert.That(_tradePage.IsSellButtonEnabled(), Is.True);
        }
    }
}
