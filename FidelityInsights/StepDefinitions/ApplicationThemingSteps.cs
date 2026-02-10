using FidelityInsights.Pages;
using Reqnroll;
using System;
using OpenQA.Selenium;
using NUnit.Framework;

namespace FidelityInsights.StepDefinitions
{
    [Binding]
    public class ApplicationThemingSteps
    {

        private readonly SettingsPage _settingsPage;
        private readonly PortfolioOverviewPage _portfolioOverviewPage;
        private readonly MarketDataPage _marketDataPage;
        private readonly ScenarioContext _scenario;
        private string _initialThemeState;
        private dynamic _currentPage;
        private string _selectedTheme;
        private List<dynamic> _navigationPages;

        public ApplicationThemingSteps(SettingsPage settingsPage, PortfolioOverviewPage portfolioOverviewPage, MarketDataPage marketDataPage, ScenarioContext scenario)
        {
            _settingsPage = settingsPage;
            _portfolioOverviewPage = portfolioOverviewPage;
            _marketDataPage = marketDataPage;
            _scenario = scenario;
        }
        private void TrackCreatedSession((bool created, string? startingBalanceUi) result)
        {
            _scenario["CreatedSession"] = result.created;

            if (result.created && !string.IsNullOrWhiteSpace(result.startingBalanceUi))
                _scenario["CreatedStartingBalanceUi"] = result.startingBalanceUi;
        }


        // Scenario 1: Theme toggle from Settings
        [Given("the application is loaded, on Settings page")]
        public void OpenSettingsPage()
        {
            _settingsPage.Open();
            _currentPage = _settingsPage;
        }

        // Scenario 2: Theme toggle from Portfolio Overview
        [Given("the application is loaded and a training session is loaded, on Portfolio Overview page")]
        public void OpenPortfolioOverviewPage()
        {
            _portfolioOverviewPage.Open();
            var result = _portfolioOverviewPage.EnsureActiveTrainingSessionWithUniqueBalance();
            TrackCreatedSession(result);
            _currentPage = _portfolioOverviewPage;
        }

        // When and Thens merged due to identical method names
        [When("I toggle the theme setting")]
        public void ToggleThemeSetting()
        {
            _initialThemeState = _currentPage.GetThemeState();
            _currentPage.ToggleTheme();
        }
        [Then("the application switches between light and dark mode")]
        public void VerifyThemeSwitch()
        {
            var currentThemeState = _currentPage.GetThemeState();
            Assert.That(_initialThemeState, Is.Not.EqualTo(currentThemeState), "The theme did not switch as expected.");
        }

        [Given("I have selected a theme")]
        public void SelectTheme() 
        {
            _settingsPage.Open();
            _initialThemeState = _settingsPage.GetThemeState();
            _settingsPage.ToggleTheme();

            if (_selectedTheme != "dark")
            {
                _settingsPage.ToggleTheme();
                _selectedTheme = "dark";
            }

        }

        [When("I navigate between pages")]
        public void NavigateBetweenPages()
        {
            // Initialize the pages to navigate through
            _navigationPages = new List<dynamic>
            {
                _portfolioOverviewPage,
                _settingsPage,
                _marketDataPage
            };
            
            // Navigate through each page
            foreach (var page in _navigationPages)
            {
                page.Open();
                // Allow page to fully load
                System.Threading.Thread.Sleep(500); 
            }
        }

        [Then("the selected theme remains active")]
        public void VerifyThemePersistence()
        {
            // Check each page to ensure theme persists
            foreach (var page in _navigationPages)
            {
                page.Open();
                var currentTheme = page.GetThemeState();
                Assert.That(currentTheme, Is.EqualTo(_selectedTheme), 
                    $"Theme did not persist on {page.GetType().Name}. Expected: {_selectedTheme}, Got: {currentTheme}");
            }
        }
    }
}