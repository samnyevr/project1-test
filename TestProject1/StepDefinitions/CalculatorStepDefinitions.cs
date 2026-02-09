using System;
using Reqnroll;

namespace MyNamespace {
    [Binding]
    public class StepDefinitions {
        private readonly ScenarioContext _scenarioContext;

        public StepDefinitions(ScenarioContext scenarioContext) {
            _scenarioContext = scenarioContext;
        }

        [Given(@"I have entered (.*) into the calculator")]
        public void GivenIhaveenteredintothecalculator(decimal args1) {
            throw new PendingStepException();
        }

        [When(@"I press add")]
        public void WhenIpressadd() {
            throw new PendingStepException();
        }

        [Then(@"the result should be (.*) on the screen")]
        public void Thentheresultshouldbeonthescreen(decimal args1) {
            throw new PendingStepException();
        }
    }
}
