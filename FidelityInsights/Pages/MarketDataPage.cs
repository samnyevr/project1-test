using FidelityInsights.Support;
using System;
using System.Collections.Generic;
using System.Text;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium.Interactions;
using SeleniumExtras.WaitHelpers;

namespace FidelityInsights.Pages {
    public class MarketDataPage : AbstractPage {
        private const string MarketDataUrl = "https://d2rczu3zvi71ix.cloudfront.net/market-data";

        public MarketDataPage(DriverContext ctx) : base(ctx.Driver) { }
        public void Open() {
            Driver.Navigate().GoToUrl(MarketDataUrl);
            // TODO: add a wait for a stable element that indicates page loaded
        }

        public void Refresh() => Driver.Navigate().Refresh();

        // Get current theme state, for use with Application Theming Steps
        public string GetThemeState() {
            IJavaScriptExecutor js = (IJavaScriptExecutor)Driver;
            return (string)js.ExecuteScript("return document.documentElement.getAttribute('data-theme');");
        }

        // Returns true if the candlestick chart canvas is displayed
        public bool IsCandlestickChartDisplayed() {
            IWebElement chartCanvas = Driver.FindElement(By.CssSelector("canvas[data-zr-dom-id='zr_0']"));
            return chartCanvas.Displayed;
        }

        public string GetStartDateValue() {
            IWebElement startDateInput = Driver.FindElement(By.Id("startDate"));
            return startDateInput.GetAttribute("value");
        }
        public string GetEndDateValue() {
            IWebElement endDateInput = Driver.FindElement(By.Id("endDate"));
            return endDateInput.GetAttribute("value");
        }

        public string SetStartDateValue(string date) {
            IWebElement startDateInput = Driver.FindElement(By.Id("startDate"));

            IJavaScriptExecutor js = (IJavaScriptExecutor)Driver;
            // Set the value directly
            js.ExecuteScript("arguments[0].value = arguments[1]; arguments[0].dispatchEvent(new Event('input'));",
                            startDateInput, date);

            return startDateInput.GetAttribute("value");
        }

        public string SetEndDateValue(string date) {
            IWebElement endDateInput = Driver.FindElement(By.Id("endDate"));

            IJavaScriptExecutor js = (IJavaScriptExecutor)Driver;
            js.ExecuteScript("arguments[0].value = arguments[1]; arguments[0].dispatchEvent(new Event('input'));",
                            endDateInput, date);

            return endDateInput.GetAttribute("value");
        }


        public void SelectTicker(string tickerSymbol) {
            // Wait for dropdown to exist
            var dropdown = new WebDriverWait(Driver, TimeSpan.FromSeconds(25))
                .Until(ExpectedConditions.ElementToBeClickable(By.Id("ticker")));
            dropdown.Click();

            // Wait for the option to be clickable
            var option = new WebDriverWait(Driver, TimeSpan.FromSeconds(25))
                .Until(ExpectedConditions.ElementToBeClickable(By.CssSelector($"option[value='{tickerSymbol}']")));
            option.Click();

            // Close the dropdown by clicking elsewhere or pressing Escape
            new OpenQA.Selenium.Interactions.Actions(Driver)
                .SendKeys(Keys.Escape)
                .Perform();

            // Alternative: Wait for dropdown to close naturally
            var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(5));
            wait.Until(d => {
                var dropdowns = d.FindElements(By.CssSelector(".MuiAutocomplete-popper, .dropdown-menu, select[id='ticker']"));
                return dropdowns.All(e => !e.Displayed || e.TagName.ToLower() == "select");
            });
        }

        public void ClickLoadData() {
            // Wait for button to be clickable
            var loadDataButton = new WebDriverWait(Driver, TimeSpan.FromSeconds(25))
                .Until(ExpectedConditions.ElementToBeClickable(By.XPath("//button[text()=' Load Data ']")));
            loadDataButton.Click();
        }

        public string GetDateRangeErrorMessage() {
            var error = new WebDriverWait(Driver, TimeSpan.FromSeconds(5))
                .Until(ExpectedConditions.ElementIsVisible(
                    By.CssSelector(".error-message"))); // adjust selector

            return error.Text.Trim();
        }

        // The button is rendered on the canvas at coordinates (x=895, y=22). Button is drawn on the canvas itself
        // Currently flaky but there seems to be no other available option without dev 
        public void ClickSaveAsImageButton() {

            // Wait for any open dropdowns or menus to disappear
            var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(5));
            wait.Until(d => {
                var dropdowns = d.FindElements(By.CssSelector(".MuiAutocomplete-popper, .dropdown-menu"));
                return dropdowns.All(e => !e.Displayed);
            });

            Thread.Sleep(1000);

            IWebElement canvas = Driver.FindElement(By.CssSelector("#mainChart canvas"));
            ((IJavaScriptExecutor)Driver).ExecuteScript("arguments[0].scrollIntoView({block:'center'});", canvas);
            Thread.Sleep(500);

            var size = canvas.Size;
            Console.WriteLine($"Canvas size: Width={size.Width}, Height={size.Height}");

            // The button should be in the top-right area
            // Let's try a position that's actually inside the canvas
            // Top-right corner with some padding: 95% width, 5% height
            int absoluteX = (int)(size.Width * 0.98);  // e.g., 835 for 879px width
            int absoluteY = (int)(size.Height * 0.05); // e.g., 20 for 400px height

            Console.WriteLine($"Target absolute position: X={absoluteX}, Y={absoluteY}");

            // Convert to center-based offset for Selenium
            int xFromCenter = absoluteX - (size.Width / 2);
            int yFromCenter = absoluteY - (size.Height / 2);

            Console.WriteLine($"Offset from center: X={xFromCenter}, Y={yFromCenter}");

            // Use JavaScript to highlight where we're about to click (for debugging)
            IJavaScriptExecutor js = (IJavaScriptExecutor)Driver;
            js.ExecuteScript(@"
                var canvas = arguments[0];
                var ctx = canvas.getContext('2d');
                var x = arguments[1];
                var y = arguments[2];
                
                // Draw a red circle at the click position for debugging
                ctx.fillStyle = 'red';
                ctx.beginPath();
                ctx.arc(x, y, 10, 0, 2 * Math.PI);
                ctx.fill();
                
                console.log('Marked click position at:', x, y);
            ", canvas, absoluteX, absoluteY);

            Thread.Sleep(1000); // Give you time to see the red dot

            try {
                new OpenQA.Selenium.Interactions.Actions(Driver)
                    .MoveToElement(canvas, xFromCenter, yFromCenter)
                    .Pause(TimeSpan.FromMilliseconds(500))
                    .Click()
                    .Perform();

                Console.WriteLine("Click action performed successfully");
            } catch (Exception ex) {
                Console.WriteLine($"Selenium click failed: {ex.Message}");
                Console.WriteLine("Trying JavaScript click as fallback...");

                // Dispatch actual click events that ECharts should respond to
                js.ExecuteScript(@"
                    var canvas = arguments[0];
                    var rect = canvas.getBoundingClientRect();
                    var x = arguments[1];
                    var y = arguments[2];
                    
                    var events = ['mousedown', 'mouseup', 'click'];
                    events.forEach(function(eventType) {
                        var evt = new MouseEvent(eventType, {
                            view: window,
                            bubbles: true,
                            cancelable: true,
                            clientX: rect.left + x,
                            clientY: rect.top + y,
                            button: 0
                        });
                        canvas.dispatchEvent(evt);
                        console.log('Dispatched', eventType, 'at', x, y);
                    });
                ", canvas, absoluteX, absoluteY);

                Console.WriteLine("JavaScript click events dispatched");
            }
            Thread.Sleep(2000); // Wait for download to initiate
        }

        public bool IsFileDownloaded(string expectedFileName, int timeoutSeconds = 10) {
            var downloadPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "artifacts",
                "downloads"
            );
            var expectedFilePath = Path.Combine(downloadPath, expectedFileName);
            var endTime = DateTime.Now.AddSeconds(timeoutSeconds);
            while (DateTime.Now < endTime) {
                // File exists and Chrome finished writing it
                if (File.Exists(expectedFilePath) &&
                    !File.Exists(expectedFilePath + ".crdownload")) {
                    return true;
                }

                Thread.Sleep(500);
            }
            return false;
        }
    }
}