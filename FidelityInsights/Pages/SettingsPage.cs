using FidelityInsights.Support;
using System;
using System.Collections.Generic;
using System.Text;
using OpenQA.Selenium;
using NUnit.Framework;

namespace FidelityInsights.Pages
{
    public class SettingsPage : AbstractPage
    {
        private const string SettingsUrl = "https://d2rczu3zvi71ix.cloudfront.net/settings";

        public SettingsPage(DriverContext ctx) : base(ctx.Driver) { }
        public void Open()
        {
            Driver.Navigate().GoToUrl(SettingsUrl);
            // TODO: add a wait for a stable element that indicates page loaded
        }

        public void Refresh() => Driver.Navigate().Refresh();

        // Get current theme state
        public string GetThemeState()
        {
            IJavaScriptExecutor js = (IJavaScriptExecutor)Driver;
            return (string)js.ExecuteScript("return document.documentElement.getAttribute('data-theme');");
        }

        // Toggle the theme
        public void ToggleTheme()
        {
            var toggle = Driver.FindElement(By.CssSelector("button.theme-toggle, button.toggle-switch[role='switch']"));
            toggle.Click();

            // Wait for the DOM to update (small delay to ensure theme change has processed)
            System.Threading.Thread.Sleep(500); // Simple approach
        }
    }
}