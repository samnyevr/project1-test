using FidelityInsights.Pages;
using FidelityInsights.Support;
using NUnit.Framework;
using OpenQA.Selenium;
using Reqnroll;
using SeleniumExtras.WaitHelpers;
using System.Threading;

namespace FidelityInsights.StepDefinitions
{
    [Binding]
    public class SimulationHistorySteps
    {
        private readonly DriverContext _driverContext;
        private readonly SimulationHistoryPage _page;

        // track expected sort direction between clicks
        private bool? _lastSortAscending = null;

        public SimulationHistorySteps(DriverContext driverContext)
        {
            _driverContext = driverContext;
            _page = new SimulationHistoryPage(_driverContext.Driver);
        }

        [Given(@"I have navigated to the ""Training Session History"" page")]
        public void GivenIHaveNavigatedToTheHistoryPage()
        {
            _page.GoTo();
            _driverContext.Driver.Navigate().Refresh();
            _page.WaitForLoaded();
        }

        [Given(@"I have the following existing sessions:")]
        public void GivenIHaveExistingSessions(DataTable table)
        {
            
        }

        [Then(@"I should see a table for ""(.*)""")]
        public void ThenIShouldSeeATableFor(string tableName)
        {
            // Map feature names to new headings internally (just to avoid errors)
            _page.WaitForTableVisible(tableName);
            Assert.Pass();
        }

        [Then(@"the ""(.*)"" session should have a ""(.*)"" status pill")]
        public void ThenTheSessionShouldHaveStatus(string sessionDate, string expectedStatus)
        {
            // UI shows COMPLETED as SUBMITTED per template
            var expectedUi = expectedStatus.Equals("COMPLETED", StringComparison.OrdinalIgnoreCase)
                ? "SUBMITTED"
                : expectedStatus.ToUpperInvariant();

            var actual = _page.GetStatusPillText(sessionDate).ToUpperInvariant();
            Assert.That(actual, Is.EqualTo(expectedUi));
        }

        [When(@"I click the ""Menu"" button for session ""(.*)""")]
        public void WhenIClickTheMenuButton(string sessionIdentifier)
        {
            _page.OpenMenuForSession(sessionIdentifier);
        }

        [When(@"I click the ""(.*)"" option")]
        public void WhenIClickTheOption(string optionName)
        {
            _page.SelectMenuOption(optionName);
        }

        [Then(@"the session ""(.*)"" should be removed from the table")]
        public void ThenTheSessionShouldBeRemoved(string sessionIdentifier)
        {
            Thread.Sleep(1000); // Wait for delete animation
            Assert.That(_page.IsSessionVisible(sessionIdentifier), Is.False);
        }

        // Sorting Steps

        [When(@"I click the ""(.*)"" column header in the Paused table")]
        public void WhenIClickHeader(string columnName)
        {
            _page.ClickColumnHeaderInPausedTable(columnName);
            _lastSortAscending = true; // first click => ascending in component
        }

        [When(@"I click the ""(.*)"" column header again")]
        public void WhenIClickTheColumnHeaderAgain(string columnName)
        {
            _page.ClickColumnHeaderInPausedTable(columnName);
            _lastSortAscending = false; // second click toggles => descending
        }

        [Then(@"the rows should be sorted by Balance in ""(.*)"" order")]
        public void ThenRowsSorted(string order)
        {
            var balances = _page.GetStartingBalancesOnPausedPage();

            // if there are < 2 rows, sorting is trivially true
            if (balances.Count < 2)
                Assert.Pass();

            var wantAscending = order.Equals("Ascending", StringComparison.OrdinalIgnoreCase);
            Assert.That(SimulationHistoryPage.IsSorted(balances, wantAscending), Is.True,
                $"Expected balances sorted {order}, but got: {string.Join(", ", balances.Select(b => b.ToString()))}");

        }

        [Then(@"the session with a positive gain should display the amount in ""(.*)""")]
        public void ThenGainIsColor(string color)
        {
            // Assert.That(_page.CheckGainLossColor(color));
            Assert.That(_page.HasStyledGainLoss(color), Is.True,
                "Could not find a visible positive amount styled with the expected gain class.");
        }

        [Then(@"the session with a negative loss should display the amount in ""(.*)""")]
        public void ThenLossIsColor(string color)
        {
            // Assert.That(_page.CheckGainLossColor(color));
            Assert.That(_page.HasStyledGainLoss(color), Is.True,
                "Could not find a visible negative amount styled with the expected loss class.");
        }
    }
}