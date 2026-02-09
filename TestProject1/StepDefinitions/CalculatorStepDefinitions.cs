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
            _scenarioContext.Pending();
        }

        [When(@"I press add")]
        public void WhenIpressadd() {
            _scenarioContext.Pending();
        }

        [Then(@"the result should be (.*) on the screen")]
        public void Thentheresultshouldbeonthescreen(decimal args1) {
            _scenarioContext.Pending();
        }

    }
}