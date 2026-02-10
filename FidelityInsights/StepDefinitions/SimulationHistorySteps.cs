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
            Assert.That(_driverContext.Driver.PageSource.Contains(tableName));
        }

        [Then(@"the ""(.*)"" session should have a ""(.*)"" status pill")]
        public void ThenTheSessionShouldHaveStatus(string sessionDate, string expectedStatus)
        {
            string actualStatus = _page.GetStatusPillText(sessionDate);
            Assert.That(actualStatus, Is.EqualTo(expectedStatus.ToUpper()));
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
            _page.ClickColumnHeader("Paused", columnName);
            Thread.Sleep(500); // Wait for sort
        }

        [When(@"I click the ""(.*)"" column header again")]
        public void WhenIClickTheColumnHeaderAgain(string columnName)
        {
            _page.ClickColumnHeader("Paused", columnName);
            Thread.Sleep(500);
        }

        [Then(@"the rows should be sorted by Balance in ""(.*)"" order")]
        public void ThenRowsSorted(string order)
        {
          
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
                "Could not find a visible positive amount styled with the expected loss class.");
        }
    }
}