using System;
using FidelityInsights.Pages;
using FidelityInsights.Support;

namespace FidelityInsights.Hooks
{
    [Binding]
    public class TrainingSessionTeardownHooks
    {
        private readonly ScenarioContext _scenario;
        private readonly DriverContext _ctx;

        public TrainingSessionTeardownHooks(ScenarioContext scenario, DriverContext ctx)
        {
            _scenario = scenario;
            _ctx = ctx;
        }

        [AfterScenario(Order = 1000)]
        public void DeleteCreatedTrainingSession()
        {
            // Only delete sessions created by THIS scenario
            if (!_scenario.TryGetValue("CreatedSession", out bool created) || !created)
                return;

            if (!_scenario.TryGetValue("CreatedStartingBalanceUi", out string startingBalanceUi) ||
                string.IsNullOrWhiteSpace(startingBalanceUi))
                return;

            var historyPage = new TrainingSessionHistoryPage(_ctx);
            historyPage.Open();
            historyPage.DeletePausedSessionByStartingBalance(startingBalanceUi);
        }
    }
}
