using NUnit.Framework;
using Reqnroll;

namespace MyNamespace {
    [Binding]
    public class CalculatorStepDefinitions {
        private readonly ScenarioContext _scenarioContext;

        private decimal _firstNumber;
        private decimal _secondNumber;
        private decimal _result;

        public CalculatorStepDefinitions(ScenarioContext scenarioContext) {
            _scenarioContext = scenarioContext;
        }

        [Given(@"I have entered (.*) into the calculator")]
        public void GivenIHaveEnteredIntoTheCalculator(decimal number) {
            if (_firstNumber == 0)
                _firstNumber = number;
            else
                _secondNumber = number;
        }

        [When(@"I press add")]
        public void WhenIPressAdd() {
            _result = _firstNumber + _secondNumber;
        }

        [Then(@"the result should be (.*) on the screen")]
        public void ThenTheResultShouldBeOnTheScreen(decimal expected) {
            Assert.That(_result, Is.EqualTo(expected));
        }
    }
}
