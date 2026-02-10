using FidelityInsights.Pages;
using NUnit.Framework;
using Reqnroll;
using System;
using System.Diagnostics;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

namespace FidelityInsights.StepDefinitions
{
    [Binding]
    public class PortfolioNavigationSteps
    {

        private readonly PortfolioOverviewPage _portfolioPage;
        private readonly TradeStocksPage _tradePage;
        private readonly TrainingSessionsPage _sessionsPage;

        public PortfolioNavigationSteps(
            PortfolioOverviewPage portfolio,
            TradeStocksPage trade,
            TrainingSessionsPage sessions)
        {
            _portfolioPage = portfolio;
            _tradePage = trade;
            _sessionsPage = sessions;
        }

        [Given("the user is on the Portfolio page")]
        public void GivenTheUserIsOnThePortfolioPage()
        {
            _portfolioPage.Open();
        }

        [When("the user returns to the Portfolio page")]
        public void WhenTheUserReturnsToThePortfolioPage()
        {
            _portfolioPage.Open();
        }

        [When("the user refreshes the Portfolio page")]
        public void WhenTheUserRefreshesThePortfolioPage()
        {
            _portfolioPage.Refresh();
        }

        // ---------------------------------------------------------------------
        // When steps
        // ---------------------------------------------------------------------

        [When("the user selects the Cash Available summary card")]
        public void WhenTheUserSelectsTheCashAvailableSummaryCard()
        {
            _portfolioPage.ClickCashAvailableSummaryCard();
        }

        [When("the user navigates to the Trade Stocks page from the header navigation")]
        public void WhenTheUserNavigatesToTradeStocksFromHeaderNavigation()
        {
            _portfolioPage.NavigateToTradeStocksFromHeader();
        }

        // ---------------------------------------------------------------------
        // Then steps
        // ---------------------------------------------------------------------

        [Then("the user should be navigated to the Trade Stocks page")]
        [Then("the user should be on the Trade Stocks page")]
        public void ThenTheUserShouldBeOnTheTradeStocksPage()
        {
            _tradePage.WaitForLoaded();
            Assert.That(_tradePage.IsAt(), Is.True, "Expected to be on the Trade Stocks page.");
        }
    }
}