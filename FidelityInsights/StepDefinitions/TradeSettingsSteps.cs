using FidelityInsights.Support;
using FidelityInsights.Pages;
using NUnit.Framework;
using Reqnroll;

namespace FidelityInsights.StepDefinitions
{
    [Binding]
    public class TradeSettingsSteps
    {
        private readonly SimulationHistoryPage _page;

        public TradeSettingsSteps(DriverContext driverContext)
        {
            _page = new SimulationHistoryPage(driverContext.Driver);
        }

        [Given(@"the trader is on the Training Session page under the Trade Stock section")]
        public void GivenTheTraderIsOnTheTrainingSessionPage()
        {
            _page.GoTo();
            _page.EnsureActiveSessionExists();
        }

        [When(@"the trader clicks on the Gear Icon to open Settings")]
        public void WhenTheTraderClicksOnTheGearIcon()
        {
            _page.ClickSettingsGear();
        }

        [When(@"the trader selects the End Training Session option")]
        public void WhenTheTraderSelectsTheEndTrainingSessionOption()
        {
            _page.SelectSettingsOption("End Training Session");
            _page.ConfirmEndSession();
        }

        [Then(@"the trader should be shown a Simulation Summary")]
        public void ThenTheTraderShouldBeShownASimulationSummary()
        {
            Assert.That(_page.IsSimulationSummaryVisible(), Is.True, "Simulation Summary modal did not appear.");
        }

        [Then(@"a summary of the training session should be displayed")]
        public void ThenASummaryOfTheTrainingSessionShouldBeDisplayed()
        {
            Assert.That(_page.IsSimulationSummaryVisible(), Is.True);
        }

        [When(@"the trader selects the Reset Training Session option")]
        public void WhenTheTraderSelectsTheResetTrainingSessionOption()
        {
            _page.SelectSettingsOption("Reset Training Session");
            _page.ConfirmResetSession();
        }

        [Then(@"the trader's available balance should reset to the initial amount")]
        public void ThenTheTradersAvailableBalanceShouldResetToTheInitialAmount()
        {
            // Navigate to Portfolio to verify the live balance rather than the cached history table
            _page.GoToPortfolio();

            string actualBalance = _page.GetCurrentBalance();

            // Check for "10,000" or "500,000" to account for different session seed data
            bool isReset = actualBalance.Contains("10,000") || actualBalance.Contains("500,000");

            Assert.That(isReset, Is.True, $"Expected balance to be 10,000 or 500,000, but found {actualBalance}");
        }

        [Then(@"the trader's holdings should be cleared")]
        public void ThenTheTradersHoldingsShouldBeCleared()
        {
            Assert.That(_page.AreHoldingsEmpty(), Is.True, "Holdings table should be empty after reset.");
        }

        [Given(@"the trader is on the Trade Stock section")]
        public void GivenTheTraderIsOnTheTradeStockSection()
        {
            _page.GoTo();
        }

        [When(@"the trader selects the Start New Trading Session option")]
        public void WhenTheTraderSelectsTheStartNewTradingSessionOption()
        {
            _page.SelectSettingsOption("Start New Training Session");
        }

        [Then(@"a popup with Trading Session Settings should appear")]
        public void ThenAPopupWithTradingSessionSettingsShouldAppear()
        {
            Assert.That(_page.IsNewSessionModalVisible(), Is.True, "The 'Start New Training Session' modal did not appear.");
        }

        [Then(@"the trader should be able to configure settings for the new trading session")]
        public void ThenTheTraderShouldBeAbleToConfigureSettingsForTheNewTradingSession()
        {
            Assert.That(_page.AreSessionSettingsInputsVisible(), Is.True, "Configuration inputs (Balance/Date) were not found or enabled.");
        }
    }
}