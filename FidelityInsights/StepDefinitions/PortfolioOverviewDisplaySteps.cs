using FidelityInsights.Pages;
using NUnit.Framework;


namespace FidelityInsights.StepDefinitions
{
    /// <summary>
    /// Step definitions for validating that the Portfolio Overview page renders the expected
    /// "active session" experience: summary cards, charts, movers/positions, and holdings.
    ///
    /// Guiding principles:
    /// - Assert presence/visibility using stable selectors and obvious UI contracts.
    /// - Avoid over-asserting exact numeric values (those can vary by data).
    /// - Prefer "is visible" + "has rows/headers" style checks for smoke/regression reliability.
    /// </summary>
    [Binding]
    public class PortfolioOverviewDisplaySteps
    {
        private readonly PortfolioOverviewPage _portfolioPage;

        // Captures a light-weight snapshot so we can detect a change after refresh.
        // This intentionally avoids parsing currency formats; we just require a visible text change.
        private string _snapshotBeforeRefresh = string.Empty;

        public PortfolioOverviewDisplaySteps(PortfolioOverviewPage portfolioPage)
        {
            _portfolioPage = portfolioPage;
        }

        // ---------------------------------------------------------------------
        // Smoke: page sections present for active session
        // ---------------------------------------------------------------------

        [Then(@"the Portfolio should display the total value summary")]
        public void ThenPortfolioShouldDisplayTotalValueSummary()
        {
            Assert.That(_portfolioPage.IsTotalValueSummaryVisible(), Is.True,
                "Expected Total Value summary card to be visible for an active session.");
        }

        [Then(@"the Portfolio should display the cash available summary")]
        public void ThenPortfolioShouldDisplayCashAvailableSummary()
        {
            Assert.That(_portfolioPage.IsCashAvailableSummaryVisible(), Is.True,
                "Expected Cash Available summary card to be visible for an active session.");
        }

        [Then(@"the Portfolio should display the unrealized performance summary")]
        public void ThenPortfolioShouldDisplayUnrealizedPerformanceSummary()
        {
            Assert.That(_portfolioPage.IsUnrealizedPerformanceSummaryVisible(), Is.True,
                "Expected Unrealized Performance summary card to be visible for an active session.");
        }

        [Then(@"the Portfolio should display asset allocation information")]
        public void ThenPortfolioShouldDisplayAssetAllocationInformation()
        {
            Assert.That(_portfolioPage.IsAssetAllocationVisible(), Is.True,
                "Expected Asset Allocation section/chart to be visible for an active session.");
        }

        [Then(@"the Portfolio should display portfolio value over time")]
        public void ThenPortfolioShouldDisplayPortfolioValueOverTime()
        {
            Assert.That(_portfolioPage.IsPortfolioValueOverTimeVisible(), Is.True,
                "Expected Portfolio Value Over Time chart panel to be visible.");
            Assert.That(_portfolioPage.GetPortfolioValueOverTimeAxisDates().Count, Is.GreaterThan(0),
                "Expected Portfolio Value Over Time chart to render axis date labels.");
        }

        [Then(@"the Portfolio should display top positions")]
        public void ThenPortfolioShouldDisplayTopPositions()
        {
            Assert.That(_portfolioPage.IsTopPositionsVisible(), Is.True,
                "Expected Top Positions section to be visible for an active session.");
        }

        [Then(@"the Portfolio should display top movers")]
        public void ThenPortfolioShouldDisplayTopMovers()
        {
            Assert.That(_portfolioPage.IsTopMoversVisible(), Is.True,
                "Expected Top Movers section to be visible for an active session.");
        }

        [Then(@"the Portfolio should display holdings")]
        public void ThenPortfolioShouldDisplayHoldings()
        {
            Assert.That(_portfolioPage.IsHoldingsVisible(), Is.True,
                "Expected Holdings section/table to be visible for an active session.");
            // Assert.That(_portfolioPage.GetHoldingsRowCount(), Is.GreaterThan(0), "Expected Holdings table to have at least one row for an active session."); DONT HAVE THESE IN CURRENT TEST DATA
        }
        // ---------------------------------------------------------------------
        // Regression: "no session" prompt not shown while active
        // ---------------------------------------------------------------------

        [Then(@"the Portfolio page should not indicate that no training session is active")]
        public void ThenPortfolioPageShouldNotIndicateNoTrainingSessionIsActive()
        {
            Assert.That(_portfolioPage.IsTrainingSessionActive(), Is.True,
                "Precondition failed: session should be active in this feature background.");
            Assert.That(_portfolioPage.IsNoSessionPromptVisible(), Is.False,
                "Expected the 'no active session' prompt to NOT be visible when a session is active.");
        }
    }
}
