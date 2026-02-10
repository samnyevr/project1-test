using FidelityInsights.Pages;
using Reqnroll;
using System;

namespace FidelityInsights.StepDefinitions
{
    [Binding]
    public class PortfolioDisplaySteps
    {

        private readonly PortfolioOverviewPage _portfolioPage;

        public PortfolioDisplaySteps(PortfolioOverviewPage portfolioPage)
        {
            _portfolioPage = portfolioPage;
        }

    }
}