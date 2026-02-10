using FidelityInsights.Pages;
using NUnit.Framework;
using Reqnroll;

namespace FidelityInsights.StepDefinitions {
    [Binding]
    public class BuyStockSteps {
        private readonly TradeStockPage _buyStockPage;

        public BuyStockSteps(TradeStockPage buyStockPage) {
            _buyStockPage = buyStockPage;
        }

        // -------- Scenario 1 --------

        [Given("the trader select a stock with ticker \"(.*)\"")]
        public void GivenTraderSelectsTicker(string ticker) {
            _buyStockPage.Open();
            _buyStockPage.StartNewSession();
            _buyStockPage.SelectTicker(ticker);
        }

        [When("the trader input a Quantity of \"(.*)\" shares")]
        public void WhenTraderInputsQuantity(string qty) {
            _buyStockPage.EnterQuantity(qty);
        }

        [When("the trader click on the Buy button")]
        public void WhenTraderClicksBuy() {
            _buyStockPage.ClickBuy();
        }

        [Then("the \"(.*)\" \"(.*)\" shares should be added to Your Holdings section")]
        public void ThenSharesAppearInHoldings(string qty, string ticker) {
            Assert.That(
                _buyStockPage.IsTickerInHoldings(ticker),
                Is.True,
                $"Expected ticker {ticker} to appear in Your Holdings"
            );
        }

        [Then("the available balance should be reduced accordingly")]
        public void ThenBalanceReduced() {
            var balance = _buyStockPage.GetAvailableBalance();

            Assert.That(balance, Is.Not.Null.And.Not.Empty,
                "Available balance should be visible after purchase");
        }

        [Then("the trader is shown a confirmation message \"(.*)\"")]
        public void ThenConfirmationMessageShown(string expectedMessage) {
            var actualMessage = _buyStockPage.GetConfirmationMessage();

            Assert.That(actualMessage, Is.EqualTo(expectedMessage));
        }

        // -------- Scenario 2: Max Buy --------

        [Given("the trader select a stock with ticker \"(.*)\" in buy step")]
        public void GivenTraderSelectsTickerInBuyStep(string ticker) {
            _buyStockPage.Open();
            _buyStockPage.SelectTicker(ticker);
        }

        [When("the trader click on the Max button")]
        public void WhenTraderClicksMax() {
            Assert.That(_buyStockPage.DoesMaxButtonExist(), Is.True);
            _buyStockPage.ClickMax();
        }

        [When("the trader click on the Buy button in buy step")]
        public void WhenTraderClicksBuyInBuyStep() {
            _buyStockPage.ClickBuy();
        }

        [Then("the maximum available \"(.*)\" shares should be added to Your Holdings section")]
        public void ThenMaxSharesAdded(string ticker) {
            Assert.That(
                _buyStockPage.IsTickerInHoldings(ticker),
                Is.True,
                $"Expected ticker {ticker} to be in holdings after max buy"
            );
        }

        [Then("the available balance should be reduced accordingly in buy step")]
        public void ThenBalanceReducedInBuyStep() {
            var balance = _buyStockPage.GetAvailableBalance();

            Assert.That(balance, Is.Not.Null.And.Not.Empty);
        }

        [Then("the trader is shown a confirmation message of \"(.*)\"")]
        public void ThenMaxConfirmationMessage(string message) {
            var actual = _buyStockPage.GetConfirmationMessage();

            Assert.That(actual, Is.EqualTo(message));
        }

        // -------- Scenario 3: Insufficient Funds --------

        [Given("the trader has an available balance of \"(.*)\"")]
        public void GivenTraderHasZeroBalance(string balance) {
            _buyStockPage.Open();
            GivenTraderSelectsTicker("AAL");
            _buyStockPage.ClickMax();
            _buyStockPage.ClickBuy();

            System.Threading.Thread.Sleep(1000);

            decimal expectedBalance =
                decimal.Parse(balance.Replace("$", ""));

            decimal actualBalance =
                decimal.Parse(
                    _buyStockPage.GetAvailableBalance()
                        .Replace("$", "")
                );

            Assert.That(actualBalance, Is.LessThan(expectedBalance));
        }

        [Given("the trader select a stock with ticker \"(.*)\" in max")]
        public void GivenTraderSelectsTickerInMax(string ticker) {
            _buyStockPage.SelectTicker(ticker);
        }

        [When("the trader input a Quantity of \"(.*)\" shares in buy step")]
        public void WhenTraderInputsQuantityInBuyStep(string qty) {

            System.Threading.Thread.Sleep(1000);
            _buyStockPage.EnterQuantity(qty);
        }

        [Then("the Buy button should be disabled")]
        public void ThenBuyButtonDisabled() {
            Assert.That(
                _buyStockPage.IsBuyButtonEnabled(),
                Is.False,
                "Buy button should be disabled when balance is insufficient"
            );
        }

        [Then("the trader is shown an error message \"(.*)\"")]
        public void ThenErrorMessageShown(string expectedError) {
            var actualError = _buyStockPage.GetErrorMessage();

            Assert.That(actualError, Is.EqualTo(expectedError));
        }

        // -------- Scenario 4: Quick Buy --------

        [Given("the trader who is trying to sell click on \"(.*)\" in Your Holdings section")]
        public void GivenTraderSellClicksTickerInHoldings(string ticker) {
            _buyStockPage.Open();
            _buyStockPage.StartNewSession();

            _buyStockPage.SelectTicker(ticker);
            _buyStockPage.EnterQuantity("10");
            _buyStockPage.ClickBuy();

            System.Threading.Thread.Sleep(1000);

            _buyStockPage.ClickTickerInHoldings(ticker);
        }

        [Then("the trader can input the desired Quantity of shares")]
        public void ThenTraderCanInputQuantity() {
            Assert.That(
                _buyStockPage.IsBuyToggleButtonEnabled(),
                Is.True,
                "Buy panel should be available for input"
            );
        }

        [When("the trader click on the Buy Toggle button")]
        public void WhenTraderClicksBuyToggle() {
            _buyStockPage.ClickBuyToggle();
        }

        [Then("the trader can click on the Buy button")]
        public void ThenTraderCanClickBuy() {
            Assert.That(
                _buyStockPage.IsBuyButtonEnabled(),
                Is.True,
                "Buy button should be clickable in quick buy flow"
            );
        }
    }
}