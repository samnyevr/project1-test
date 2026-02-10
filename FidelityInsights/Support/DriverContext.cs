using System;
using System.Collections.Generic;
using System.Text;
using OpenQA.Selenium;

namespace FidelityInsights.Support;

/// <summary>
/// Provides a shared context for the WebDriver instance.
/// This allows step definitions and page objects to access the same browser session within a scenario.
/// </summary>
public class DriverContext
{
    /// <summary>
    /// The Selenium WebDriver instance used for browser automation.
    /// This is set by the WebDriverHooks at the start of each scenario.
    /// </summary>
    public IWebDriver Driver { get; set; } = default!;
}

