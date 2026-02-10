using FidelityInsights.Pages;
using FidelityInsights.ApiTests;
using Reqnroll;
using System;
using OpenQA.Selenium;
using NUnit.Framework;



namespace FidelityInsights.StepDefinitions
{
    [Binding]
    public class CandlestickChartSteps
    {

        private readonly MarketDataPage _marketDataPage;
        private readonly CandlestickApiClient _apiClient;

        public CandlestickChartSteps(MarketDataPage marketDataPage, CandlestickApiClient apiClient)
        {
            _marketDataPage = marketDataPage;
            _apiClient = apiClient;
        }

        [Given("I open the application")]
        public void OpenApplication()
        {
            _marketDataPage.Open();
        }

        [Given("I am viewing a candlestick chart for \"(.*)\"")]
        public void GivenIAmViewingACandlestickChartForTicker(string ticker)
        {
            _marketDataPage.Open();
            _marketDataPage.SelectTicker(ticker);
            // Assert.That(_marketDataPage.IsCandlestickChartDisplayed(), Is.True);
        }
        
        // basically a smoke test for the candlestick chart
        [Then("I see a candlestick chart for the default ticker")]
        public void VerifyCandlestickChartIsDisplayed()
        {
            Assert.That(
                _marketDataPage.IsCandlestickChartDisplayed(),
                Is.True,
                "Candlestick chart is not displayed on the Market Data page."
            );
        }

        [Then("the date range is set to the ~4 year range from 2020 to 2024")]
        public void VerifyDefaultDateRange()
        {
            Assert.Multiple(() =>
            {
                Assert.That(_marketDataPage.GetStartDateValue(), Is.EqualTo("2020-01-01"),
                    "Start date was not set to the expected default (2020-01-01)");

                Assert.That(_marketDataPage.GetEndDateValue(), Is.EqualTo("2024-12-31"),
                    "End date was not set to the expected default (2024-12-31)");
            });
        }

        // Scenario 2
        [Given("I am viewing the default candlestick chart")]
        public void GivenIAmViewingTheDefaultCandlestickChart()
        {
            _marketDataPage.Open();
            Assert.That(
                _marketDataPage.IsCandlestickChartDisplayed(),
                Is.True,
                "Candlestick chart is not displayed on the Market Data page."
            );
        }

        [Given("I am viewing a candlestick chart")]
        public void GivenIAmViewingACandlestickChart()
        {
            _marketDataPage.Open();
            Assert.That(
                _marketDataPage.IsCandlestickChartDisplayed(),
                Is.True,
                "Candlestick chart is not displayed on the Market Data page."
            );
        }

        [When("I select \"(.*)\" from the ticker dropdown")]
        public void SelectTickerFromDropdown(string ticker)
        {
            _marketDataPage.SelectTicker(ticker);
        }

        [Then("the chart updates to show \"(.*)\" historical data")]
        public void VerifyChartUpdatesForSelectedTicker(string ticker)
        {
            // Wait for chart to update, probably needs a better wait condition
            System.Threading.Thread.Sleep(2000); 

            // For simplicity, verifies the chart is still displayed after selecting a new ticker
            Assert.That(
                _marketDataPage.IsCandlestickChartDisplayed(),
                Is.True,
                $"Candlestick chart is not displayed after selecting ticker {ticker}."
            );
        }

        // Scenario 3: change date range using text inputs. may need to also test using calendar picker when dev implements the material UI date picker
        [When("I set the \"From\" date to \"(.*)\"")]
        public void SetFromDate(string fromDate)
        {
            _marketDataPage.SetStartDateValue(fromDate);
        }

        [When("I set the \"To\" date to \"(.*)\"")]
        public void SetToDate(string toDate)
        {
            _marketDataPage.SetEndDateValue(toDate);
        }

        [When("I click the \"Load Data\" button")]
        public void ClickLoadData() => _marketDataPage.ClickLoadData();

        [Then("the chart updates to display data from \"(.*)\" to \"(.*)\"")]
        public void VerifyChartDateRange(string expectedStart, string expectedEnd)
        {
            Assert.Multiple(() =>
            {
                Assert.That(_marketDataPage.GetStartDateValue(), Is.EqualTo(expectedStart));
                Assert.That(_marketDataPage.GetEndDateValue(), Is.EqualTo(expectedEnd));
            });
            // deprecated in favor of API testing
            // Assert.That(_marketDataPage.AreOHLCValuesCorrectForDateRange(expectedStart, expectedEnd), Is.True);
        }

        [Then(@"the displayed OHLC \(Open, High, Low, Close\) values correspond to the selected date range")]
        public void ValidateOHLCLogic()
        {

            // for manual validation
            System.Threading.Thread.Sleep(4000); 

            // Fetch candlestick data from API
            var candlestickData = _apiClient.FetchCandlestickData();
            
            // Validate that we received data
            Assert.That(candlestickData, Is.Not.Null, "Failed to fetch candlestick data from API");
            Assert.That(candlestickData.Count, Is.GreaterThan(0), "No candlestick data returned from API");
            
            // Validate OHLC logic rules
            bool isValid = _apiClient.ValidateOHLCLogic(candlestickData);
            Assert.That(isValid, Is.True, "OHLC validation failed - one or more candlestick data points violate OHLC logic rules");
            
            Console.WriteLine($"Successfully validated {candlestickData.Count} candlestick data points for A");
        }

        // Scenario 4: Invalid date range (From > To). Currently unimplemented by dev
        [Then("I see an error message indicating \"(.*)\"")]
        public void VerifyErrorMessageForInvalidDateRange(string expectedErrorMessage)
        {
            string actualErrorMessage = _marketDataPage.GetDateRangeErrorMessage();
            Assert.That(actualErrorMessage, Is.EqualTo(expectedErrorMessage),
                $"Expected error message '{expectedErrorMessage}' but got '{actualErrorMessage}'.");
        }

        [Then("the chart does not update")]
        public void ThenTheChartDoesNotUpdate()
        {
            // Placeholder for verifying the chart data remains unchanged
            Assert.That(
                _marketDataPage.IsCandlestickChartDisplayed(),
                Is.True,
                "Candlestick chart is not displayed after attempting to set an invalid date range."
            );
        }

        // Scenario 5: Date range outside available data. Also currently unimplemented by dev
        [Then("I see a message \"(.*)\"")]
        public void VerifyErrorMessageForOutsideDataRange(string expectedErrorMessage)
        {
            string actualErrorMessage = _marketDataPage.GetDateRangeErrorMessage();
            Assert.That(actualErrorMessage, Is.EqualTo(expectedErrorMessage),
                $"Expected error message '{expectedErrorMessage}' but got '{actualErrorMessage}'.");
        }
        // And data remains unchanged

        // Scenario 6: PNG image download 
        [When("I click the \"Save as Image\" button")]
        public void ClickSaveAsImageButton()
        {
            _marketDataPage.ClickSaveAsImageButton();
        }

        [Then("a file named \"(.*)\" should be downloaded to my computer")]
        public void VerifyFileIsDownloaded(string expectedFileName)
        {
            bool isDownloaded = _marketDataPage.IsFileDownloaded(expectedFileName);
            Assert.That(isDownloaded, Is.True, $"The file '{expectedFileName}' was not downloaded.");
        }
        
        [Then("the file should be in \"(.*)\" format")]
        public void VerifyFileFormat(string expectedFormat)
        {
            var downloadPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "artifacts",
                "downloads"
            );
            
            // Get the most recently downloaded file
            var directory = new DirectoryInfo(downloadPath);
            var latestFile = directory.GetFiles("*.png")
                .OrderByDescending(f => f.LastWriteTime)
                .FirstOrDefault();
            
            Assert.That(latestFile, Is.Not.Null, "No PNG file found in downloads folder");
            
            // Verify file extension matches expected format
            string actualExtension = latestFile.Extension.TrimStart('.').ToUpper();
            string expectedExtensionUpper = expectedFormat.ToUpper();
            
            Assert.That(actualExtension, Is.EqualTo(expectedExtensionUpper),
                $"Expected file format '{expectedFormat}' but got '{actualExtension}'");
            
            // Optional: Verify it's actually a valid PNG by checking file signature
            if (expectedFormat.ToUpper() == "PNG")
            {
                byte[] pngSignature = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
                byte[] fileHeader = new byte[8];
                
                using (var fs = new FileStream(latestFile.FullName, FileMode.Open, FileAccess.Read))
                {
                    fs.Read(fileHeader, 0, 8);
                }
                
                bool isValidPng = pngSignature.SequenceEqual(fileHeader);
                Assert.That(isValidPng, Is.True, 
                    "File has .png extension but does not have valid PNG file signature");
            }
            
            Console.WriteLine($"Verified file '{latestFile.Name}' is in {expectedFormat} format");
        }        
    }
}