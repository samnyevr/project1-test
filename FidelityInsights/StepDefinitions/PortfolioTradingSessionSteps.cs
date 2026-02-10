using FidelityInsights.Pages;
using NUnit.Framework;

namespace FidelityInsights.StepDefinitions
{
    /// <summary>
    /// Step definitions for creating/canceling training sessions from the Portfolio page.
    ///
    /// Design goals:
    /// - Keep step logic minimal and delegate page interactions to PortfolioOverviewPage.
    /// - Prefer stable UI indicators and explicit waits in the page object.
    /// - Store the last error message for assertions in negative-path scenarios.
    /// </summary>
    [Binding]
    internal class PortfolioTradingSessionSteps
    {
        private readonly PortfolioOverviewPage _portfolioPage;
        private readonly ScenarioContext _scenario;

        // Captures a validation message after an invalid submit attempt.
        // Stored in the step class so it can be asserted later.
        private string? _lastErrorMessage;

        public PortfolioTradingSessionSteps(PortfolioOverviewPage portfolioPage, ScenarioContext scenario)
        {
            _portfolioPage = portfolioPage;
            _scenario = scenario;
        }

        private void TrackCreatedSession((bool created, string? startingBalanceUi) result)
        {
            _scenario["CreatedSession"] = result.created;

            if (result.created && !string.IsNullOrWhiteSpace(result.startingBalanceUi))
                _scenario["CreatedStartingBalanceUi"] = result.startingBalanceUi;
        }



        // ---------------------------------------------------------------------
        // Given
        // ---------------------------------------------------------------------

        [Given("no active training session exists")]
        public void GivenNoActiveTrainingSessionExists()
        {
            // This Given is intentionally strict: if the precondition isn't met,
            // fail early so the test doesn't accidentally pass for the wrong reason.
            if (_portfolioPage.IsTrainingSessionActive())
                Assert.Fail("A training session is already active.");
        }

        [Given("an active training session exists")]
        public void GivenAnActiveTrainingSessionExists()
        {
            // If no session exists, create one with unique balance and track it
            var result = _portfolioPage.EnsureActiveTrainingSessionWithUniqueBalance();
            TrackCreatedSession(result);
        }

        // ---------------------------------------------------------------------
        // When
        // ---------------------------------------------------------------------

        [When("the user creates a new training session with:")]
        public void WhenUserCreatesNewTrainingSession(Table table)
        {
            // The feature uses a single-row table to supply inputs.
            // We read the values by column name so changes in column order won't break tests.
            var row = table.Rows[0];

            // For teardown reliability, always create with a unique balance when we're actually creating a session.
            // We still honor start date + length from the feature file.
            var result = _portfolioPage.StartNewTrainingSessionWithUniqueBalanceTracking(
                row["start date"],
                row["session length"]
            );

            TrackCreatedSession(result);
        }

        [When("the user starts the training session creation process")]
        public void WhenUserStartsTrainingSessionCreationProcess()
        {
            // Opens the training session modal and navigates to the settings form (if there is an intermediate modal).
            _portfolioPage.OpenTrainingSessionSettingsForm();
        }

        [When("the user cancels the training session creation")]
        public void WhenUserCancelsTrainingSessionCreation()
        {
            _portfolioPage.CancelTrainingSessionCreation();
        }

        [When("the user attempts to create a new training session with:")]
        public void WhenUserAttemptsToCreateNewTrainingSessionWith(Table table)
        {
            var row = table.Rows[0];

            // Attempt the full creation flow. If input is invalid, the page object will not close the modal.
            _portfolioPage.StartNewTrainingSession(row["start date"], row["session length"], row["starting balance"]);

            // Capture the validation message (if any) for assertions.
            _lastErrorMessage = _portfolioPage.WaitForFormError(3);

            // not doing session stuff bc these fail out
        }

        // ---------------------------------------------------------------------
        // Then
        // ---------------------------------------------------------------------

        [Then("a training session should be active")]
        public void ThenTrainingSessionShouldBeActive()
        {
            _portfolioPage.WaitForTrainingSessionToBeActive();
            Assert.That(_portfolioPage.IsTrainingSessionActive(), Is.True);
        }

        [Then("a training session should still be active")]
        public void ThenTrainingSessionShouldStillBeActive()
        {
            _portfolioPage.WaitForTrainingSessionToBeActive();
            Assert.That(_portfolioPage.IsTrainingSessionActive(), Is.True);
        }

        [Then("no training session should be active")]
        public void ThenNoTrainingSessionShouldBeActive()
        {
            _portfolioPage.WaitForTrainingSessionToBeInactive();
            Assert.That(_portfolioPage.IsTrainingSessionActive(), Is.False);
        }

        [Then("the Portfolio page should reflect the active training session")]
        public void ThenPortfolioPageShouldReflectActiveTrainingSession()
        {
            // This is intentionally redundant with "a training session should be active"
            // to match the feature language and keep failures localized to a step.
            _portfolioPage.WaitForTrainingSessionToBeActive();
            Assert.That(_portfolioPage.IsTrainingSessionActive(), Is.True);
        }

        [Then("the Portfolio page should indicate that no training session is active")]
        public void ThenPortfolioPageShouldIndicateNoActiveTrainingSession()
        {
            _portfolioPage.WaitForTrainingSessionToBeInactive();
            Assert.That(_portfolioPage.IsTrainingSessionActive(), Is.False);
        }

        [Then("the training session should not be created")]
        public void ThenTrainingSessionShouldNotBeCreated()
        {
            // Negative-path scenarios should not require a long wait; if creation happened, the test should fail.
            Assert.That(_portfolioPage.IsTrainingSessionActive(), Is.False);
        }

        [Then("the user should be informed of the input problem")]
        public void ThenUserShouldBeInformedOfInputProblem()
        {
            // Keep assertion broad enough for minor copy changes while still validating the gist of the message.
            Assert.That(_lastErrorMessage ?? string.Empty, Does.Contain("greater than 0"));
        }
    }
}
