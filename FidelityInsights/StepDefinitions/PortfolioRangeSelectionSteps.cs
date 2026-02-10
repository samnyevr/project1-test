using FidelityInsights.Pages;
using NUnit.Framework;

namespace FidelityInsights.StepDefinitions
{
    /// <summary>
    /// Step definitions for validating Portfolio date/range controls.
    ///
    /// This file intentionally focuses on:
    ///   1) The header date controls (Quick + Range) and the "Selected dates" pill at the top of the page.
    ///   2) The "Portfolio Value Over Time" chart range pills (a separate set of controls) and their axis behavior.
    ///
    /// Notes:
    /// - We avoid over-asserting exact start/end values for header range tests since those can be affected by
    ///   server data, timezone, and dropdown contents. Instead we assert the UI changes in the expected ways.
    /// - For the chart range tests, we do validate approximate time span (in months) using the month/year ticks.
    /// </summary>
    [Binding]
    public class PortfolioRangeSelectionSteps
    {
        private readonly PortfolioOverviewPage _portfolioPage;

        // Header "Selected dates" pill text captured before changing range.
        // Used to confirm the pill updates when the user changes a range value.
        private string _headerDatePillBefore = string.Empty;

        // Chart validation state:
        // We cache the chart's "end" tick once per scenario and validate it stays constant while ranges change.
        private DateTime? _chartEndBaseline;
        private string _selectedChartRange = string.Empty;

        public PortfolioRangeSelectionSteps(PortfolioOverviewPage portfolioPage)
        {
            _portfolioPage = portfolioPage;
        }

        // ---------------------------------------------------------------------
        // Header date controls (Quick + Range)
        // ---------------------------------------------------------------------

        [When(@"the user selects the ""(.*)"" performance range")]
        public void WhenTheUserSelectsThePerformanceRange(string range)
        {
            // The page loads with "Selected dates: Start not set → End not set".
            // To make the Range dropdown meaningful, first pick a "Quick" option that sets an initial date window.
            _portfolioPage.InitializeDateRangeFromQuickFirstOption();

            // Capture the pill before changing Range. This lets us verify the page actually reacted to the change.
            _headerDatePillBefore = _portfolioPage.GetSelectedDatesPillText();

            // Now apply the requested range from the Range dropdown.
            _portfolioPage.SelectRange(range);
        }

        [Then(@"the Portfolio should update to reflect the selected ""(.*)"" range")]
        public void ThenThePortfolioShouldUpdateToReflectTheSelectedRange(string range)
        {
            // We keep this assertion UI-driven and stable:
            // - The pill should no longer have "not set".
            // - The pill should keep the UI delimiter arrow.
            // - The pill should change from the previously captured value.
            _portfolioPage.WaitForSelectedDatesPillToBeSet();

            var after = _portfolioPage.GetSelectedDatesPillText();
            Assert.That(after, Does.Not.Contain("not set").IgnoreCase);
            Assert.That(after, Does.Contain("→"));

            Assert.That(
                after,
                Is.Not.EqualTo(_headerDatePillBefore),
                "Expected selected dates pill to update after range change.");
        }

        [Then(@"the selected range indicator should show ""(.*)""")]
        public void ThenTheSelectedRangeIndicatorShouldShow(string range)
        {
            // The indicator is the selected option in the Range <select> (when present),
            // or an 'active' pill if the UI ever switches to pill buttons.
            var selected = _portfolioPage.GetSelectedRangeIndicator();
            Assert.That(selected, Is.EqualTo(range));
        }

        // ---------------------------------------------------------------------
        // Session context (used by other scenarios that share this step class)
        // ---------------------------------------------------------------------

        [Then(@"the Portfolio page should display session context information")]
        public void ThenThePortfolioPageShouldDisplaySessionContextInformation()
        {
            // This checks for stable UI that only shows when the Portfolio is in a "session" context.
            // We prefer this over fragile assertions like exact IDs/values.
            Assert.That(
                _portfolioPage.IsSessionContextVisible(),
                Is.True,
                "Expected session context UI to be visible.");
        }

        // ---------------------------------------------------------------------
        // Portfolio Value Over Time chart range (separate from header controls)
        // ---------------------------------------------------------------------

        [When(@"the user selects the ""(.*)"" portfolio value over time range")]
        public void WhenTheUserSelectsThePortfolioValueOverTimeRange(string range)
        {
            // The chart's intended behavior:
            // - The end date (rightmost axis tick) stays the same.
            // - The start date shifts based on the selected range.
            //
            // We capture the end tick once per scenario and use it as a baseline.
            if (_chartEndBaseline == null)
            {
                _chartEndBaseline = _portfolioPage.GetPortfolioValueOverTimeEndDate();
                Assert.That(
                    _chartEndBaseline.HasValue,
                    Is.True,
                    "Expected chart axis to have a parseable end date.");
            }

            _selectedChartRange = range;

            // Clicking the chart range pill should update the active pill and (usually) update axis ticks.
            _portfolioPage.SelectPortfolioValueOverTimeRange(range);
        }

        [Then(@"the portfolio value over time visualization should reflect the selected range")]
        public void ThenThePortfolioValueOverTimeVisualizationShouldReflectTheSelectedRange()
        {
            // 1) Active pill matches expected selection.
            Assert.That(
                _portfolioPage.GetSelectedPortfolioValueOverTimeRange(),
                Is.EqualTo(_selectedChartRange));

            // 2) Chart end tick remains constant (rightmost label) across range changes.
            var endAfter = _portfolioPage.GetPortfolioValueOverTimeEndDate();
            Assert.That(
                endAfter.HasValue,
                Is.True,
                "Expected chart axis to have a parseable end date after selection.");

            Assert.That(
                endAfter!.Value,
                Is.EqualTo(_chartEndBaseline!.Value),
                "Expected chart end date (rightmost axis label) to remain constant when changing chart range.");

            // 3) Start-to-end span matches the selected range approximately.
            //
            // The axis ticks are month/year (not a full date), and the chart uses only a few ticks.
            // Therefore we validate within a tolerance window.
            var (start, end) = _portfolioPage.GetPortfolioValueOverTimeAxisBounds();
            Assert.That(
                start.HasValue && end.HasValue,
                Is.True,
                "Expected parseable chart start/end axis bounds.");

            var months = PortfolioOverviewPage.MonthDiff(start!.Value, end!.Value);
            var expected = PortfolioOverviewPage.ExpectedMonthsForChartRange(_selectedChartRange);

            // Tolerance: allow +/- 1 month because ticks are coarse (month/year) and may round to nearest tick.
            Assert.That(
                months,
                Is.InRange(expected - 1, expected + 1),
                $"Expected chart span ~{expected} months for '{_selectedChartRange}', but was {months} months.");
        }
    }
}
