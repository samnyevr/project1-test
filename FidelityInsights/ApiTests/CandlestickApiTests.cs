using static RestAssured.Dsl;
using System.Text.Json;
using NUnit.Framework;
using System.Net.Http;

namespace FidelityInsights.ApiTests
{
    public class CandlestickData
    {
        public string ticker { get; set; } = string.Empty;
        public string tradeDate { get; set; } = string.Empty;
        public double open { get; set; }
        public double high { get; set; }
        public double low { get; set; }
        public double close { get; set; }
        public double adjClose { get; set; }
        public long volume { get; set; }
    }

    public class DateRangeResponse
    {
        public string minDate { get; set; } = string.Empty;
        public string maxDate { get; set; } = string.Empty;
    }

    public class CandlestickApiClient
    {
        private const string BaseUrl = "https://d2rczu3zvi71ix.cloudfront.net";
        private const string Ticker = "AAPL";
        private const string FromDate = "2020-01-01";
        private const string ToDate = "2024-12-31";

        public List<CandlestickData> FetchCandlestickData()
        {
            var response = Given()
                .When()
                .Get($"{BaseUrl}/api/candlestick/{Ticker}?from={FromDate}&to={ToDate}")
                .Then()
                .StatusCode(200)
                .Extract()
                .Response();

            var responseBody = response.Content.ReadAsStringAsync().Result;
            var candlestickData = JsonSerializer.Deserialize<List<CandlestickData>>(responseBody);
            
            return candlestickData ?? new List<CandlestickData>();
        }

        public bool ValidateOHLCLogic(List<CandlestickData> data)
        {
            if (data == null || data.Count == 0)
            {
                return false;
            }

            foreach (var candle in data)
            {
                // Validate: High >= Low
                if (candle.high < candle.low)
                {
                    Console.WriteLine($"OHLC Validation Failed on {candle.tradeDate}: High ({candle.high}) < Low ({candle.low})");
                    return false;
                }

                // Validate: High >= Open
                if (candle.high < candle.open)
                {
                    Console.WriteLine($"OHLC Validation Failed on {candle.tradeDate}: High ({candle.high}) < Open ({candle.open})");
                    return false;
                }

                // Validate: High >= Close
                if (candle.high < candle.close)
                {
                    Console.WriteLine($"OHLC Validation Failed on {candle.tradeDate}: High ({candle.high}) < Close ({candle.close})");
                    return false;
                }

                // Validate: Low <= Open
                if (candle.low > candle.open)
                {
                    Console.WriteLine($"OHLC Validation Failed on {candle.tradeDate}: Low ({candle.low}) > Open ({candle.open})");
                    return false;
                }

                // Validate: Low <= Close
                if (candle.low > candle.close)
                {
                    Console.WriteLine($"OHLC Validation Failed on {candle.tradeDate}: Low ({candle.low}) > Close ({candle.close})");
                    return false;
                }

                // Validate: All values > 0
                if (candle.open <= 0 || candle.high <= 0 || candle.low <= 0 || candle.close <= 0)
                {
                    Console.WriteLine($"OHLC Validation Failed on {candle.tradeDate}: One or more OHLC values are non-positive");
                    return false;
                }
            }

            return true;
        }

        public List<CandlestickData> FetchCandlestickData(string ticker, string fromDate, string toDate)
        {
            var response = Given()
                .When()
                .Get($"{BaseUrl}/api/candlestick/{ticker}?from={fromDate}&to={toDate}")
                .Then()
                .StatusCode(200)
                .Extract()
                .Response();

            var responseBody = response.Content.ReadAsStringAsync().Result;
            var candlestickData = JsonSerializer.Deserialize<List<CandlestickData>>(responseBody);
            
            return candlestickData ?? new List<CandlestickData>();
        }

        public HttpResponseMessage FetchCandlestickDataRaw(string ticker, string fromDate, string toDate)
        {
            var response = Given()
                .When()
                .Get($"{BaseUrl}/api/candlestick/{ticker}?from={fromDate}&to={toDate}")
                .Then()
                .Extract()
                .Response();

            return response;
        }

        public HttpResponseMessage GetTickerDateRange(string ticker)
        {
            var response = Given()
                .When()
                .Get($"{BaseUrl}/api/candlestick/{ticker}/range")
                .Then()
                .Extract()
                .Response();

            return response;
        }

        public HttpResponseMessage GetPriceForDate(string ticker, string date)
        {
            var response = Given()
                .When()
                .Get($"{BaseUrl}/api/candlestick/{ticker}/{date}")
                .Then()
                .Extract()
                .Response();

            return response;
        }

        public HttpResponseMessage GetPriceByAssetId(int assetId, string date)
        {
            var response = Given()
                .When()
                .Get($"{BaseUrl}/api/candlestick/asset/{assetId}/date/{date}")
                .Then()
                .Extract()
                .Response();

            return response;
        }
    }

    /// <summary>
    /// Minimal API test coverage for Candlestick endpoints.
    /// Covers: HTTP status codes, key response fields/schemas, critical error scenarios.
    /// </summary>
    [TestFixture]
    public class CandlestickApiTests
    {
        private const string BaseUrl = "https://d2rczu3zvi71ix.cloudfront.net";
        private CandlestickApiClient _apiClient;

        [SetUp]
        public void Setup()
        {
            _apiClient = new CandlestickApiClient();
        }

        // ====================================
        // ENDPOINT 1: GET /api/candlestick/{ticker}?from={date}&to={date}
        // Minimal Coverage: Happy path + Error case + Schema validation
        // ====================================

        [Test]
        [Category("API")]
        [Category("Candlestick")]
        [Description("Happy Path: Valid ticker and date range returns 200 with valid OHLC data")]
        public void Endpoint1_GetCandlestickRange_ValidRequest_Returns200WithValidData()
        {
            // Arrange & Act
            var data = _apiClient.FetchCandlestickData("AAPL", "2020-01-01", "2020-12-31");
            
            // Assert - Status 200 implicit via successful deserialization
            Assert.That(data, Is.Not.Null, "Response should not be null");
            Assert.That(data.Count, Is.GreaterThan(0), "Should return data for valid ticker and date range");
            
            // Validate schema - first record has all required fields
            var firstCandle = data.First();
            Assert.Multiple(() =>
            {
                Assert.That(firstCandle.ticker, Is.Not.Empty, "Ticker field required");
                Assert.That(firstCandle.tradeDate, Is.Not.Empty, "TradeDate field required");
                Assert.That(firstCandle.open, Is.GreaterThan(0), "Open price should be positive");
                Assert.That(firstCandle.high, Is.GreaterThan(0), "High price should be positive");
                Assert.That(firstCandle.low, Is.GreaterThan(0), "Low price should be positive");
                Assert.That(firstCandle.close, Is.GreaterThan(0), "Close price should be positive");
                Assert.That(firstCandle.volume, Is.GreaterThanOrEqualTo(0), "Volume should be non-negative");
            });
            
            // Validate OHLC business logic
            Assert.That(_apiClient.ValidateOHLCLogic(data), Is.True, 
                "OHLC values should follow candlestick rules (High>=Low, etc.)");
        }

        [Test]
        [Category("API")]
        [Category("Candlestick")]
        [Description("Error Case: Invalid ticker returns 404 or empty result")]
        public void Endpoint1_GetCandlestickRange_InvalidTicker_ReturnsErrorOrEmpty()
        {
            // Arrange & Act
            var response = _apiClient.FetchCandlestickDataRaw("INVALIDTICKER999", "2020-01-01", "2020-12-31");
            
            // Assert - Backend may return 404 or 200 with empty array
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var data = JsonSerializer.Deserialize<List<CandlestickData>>(
                    response.Content.ReadAsStringAsync().Result);
                Assert.That(data.Count, Is.EqualTo(0), 
                    "Invalid ticker should return empty array if status is 200");
            }
            else
            {
                Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.NotFound),
                    "Invalid ticker should return 404");
            }
        }

        [Test]
        [Category("API")]
        [Category("Candlestick")]
        [Description("Error Case: Invalid date range (from > to) returns error or empty")]
        public void Endpoint1_GetCandlestickRange_InvalidDateRange_ReturnsErrorOrEmpty()
        {
            // Arrange & Act - When FROM date is after TO date
            var response = _apiClient.FetchCandlestickDataRaw("AAPL", "2024-12-31", "2020-01-01");
            
            // Assert - Backend should reject with 400 or return empty array
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var data = JsonSerializer.Deserialize<List<CandlestickData>>(
                    response.Content.ReadAsStringAsync().Result);
                Assert.That(data.Count, Is.EqualTo(0), 
                    "Invalid date range should return empty array if status is 200");
            }
            else
            {
                Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.BadRequest),
                    "Invalid date range should return 400");
            }
        }

        // ====================================
        // ENDPOINT 2: GET /api/candlestick/{ticker}/range
        // Returns: {"minDate":"1980-12-12","maxDate":"2020-04-01"}
        // Minimal Coverage: Happy path + Error case
        // ====================================

        [Test]
        [Category("API")]
        [Category("Candlestick")]
        [Description("Happy Path: Valid ticker returns 200 with date range object containing minDate and maxDate")]
        public void Endpoint2_GetTickerDateRange_ValidTicker_Returns200WithDateRange()
        {
            // Arrange & Act
            var response = _apiClient.GetTickerDateRange("AAPL");
            
            // Assert - Status
            Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK),
                "Valid ticker should return 200");
            
            // Assert - Response body
            var responseBody = response.Content.ReadAsStringAsync().Result;
            Assert.That(responseBody, Is.Not.Empty, "Response should contain date range data");
            
            // Assert - Schema validation
            var dateRange = JsonSerializer.Deserialize<DateRangeResponse>(responseBody);
            Assert.That(dateRange, Is.Not.Null, "Should deserialize to DateRangeResponse");
            Assert.That(dateRange.minDate, Is.Not.Empty, "minDate should be present");
            Assert.That(dateRange.maxDate, Is.Not.Empty, "maxDate should be present");
            
            // Assert - Date format validation
            Assert.That(() => DateTime.Parse(dateRange.minDate), Throws.Nothing,
                "minDate should be valid date format");
            Assert.That(() => DateTime.Parse(dateRange.maxDate), Throws.Nothing,
                "maxDate should be valid date format");
                
            // Assert - Logical validation
            var minDate = DateTime.Parse(dateRange.minDate);
            var maxDate = DateTime.Parse(dateRange.maxDate);
            Assert.That(minDate, Is.LessThan(maxDate),
                "minDate should be before maxDate");
        }

        [Test]
        [Category("API")]
        [Category("Candlestick")]
        [Description("Error Case: Invalid ticker returns 404 or error")]
        public void Endpoint2_GetTickerDateRange_InvalidTicker_ReturnsError()
        {
            // Arrange & Act
            var response = _apiClient.GetTickerDateRange("INVALIDTICKER999");
            
            // Assert
            Assert.That(response.StatusCode, 
                Is.EqualTo(System.Net.HttpStatusCode.NotFound).Or.EqualTo(System.Net.HttpStatusCode.BadRequest),
                "Invalid ticker should return 404 or 400");
        }

        // ====================================
        // ENDPOINT 3: GET /api/candlestick/{ticker}/{date}
        // Returns: Just a number (e.g., 300.0) - the closing price for that date
        // Minimal Coverage: Happy path + Error case + Schema validation
        // ====================================

        [Test]
        [Category("API")]
        [Category("Candlestick")]
        [Description("Happy Path: Valid ticker and date returns 200 with closing price as a number")]
        public void Endpoint3_GetPriceForDate_ValidTickerAndDate_Returns200WithPrice()
        {
            // Arrange & Act
            var response = _apiClient.GetPriceForDate("AAPL", "2020-01-02");
            
            // Assert - Status
            Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK),
                "Valid ticker and date should return 200");
            
            // Assert - Response body
            var responseBody = response.Content.ReadAsStringAsync().Result;
            Assert.That(responseBody, Is.Not.Empty, "Response should contain price");
            
            // Assert - Parse as number
            double price;
            bool isValidNumber = double.TryParse(responseBody.Trim(), out price);
            Assert.That(isValidNumber, Is.True, 
                "Response should be a valid number (closing price)");
            Assert.That(price, Is.GreaterThan(0), 
                "Price should be positive");
            
            // For AAPL on 2020-01-02, we know the closing price was 300.0
            Assert.That(price, Is.EqualTo(300.0), 
                "Price should match known historical value for AAPL on 2020-01-02");
        }

        [Test]
        [Category("API")]
        [Category("Candlestick")]
        [Description("Error Case: Invalid ticker returns error")]
        public void Endpoint3_GetPriceForDate_InvalidTicker_ReturnsError()
        {
            // Arrange & Act
            var response = _apiClient.GetPriceForDate("INVALIDTICKER999", "2020-01-02");
            
            // Assert
            Assert.That(response.StatusCode, 
                Is.EqualTo(System.Net.HttpStatusCode.NotFound).Or.EqualTo(System.Net.HttpStatusCode.BadRequest),
                "Invalid ticker should return 404 or 400");
        }

        [Test]
        [Category("API")]
        [Category("Candlestick")]
        [Description("Error Case: Invalid date format returns error")]
        public void Endpoint3_GetPriceForDate_InvalidDate_ReturnsError()
        {
            // Arrange & Act
            var response = _apiClient.GetPriceForDate("AAPL", "invalid-date");
            
            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.BadRequest),
                "Invalid date format should return 400");
        }

        // ====================================
        // ENDPOINT 4: GET /api/candlestick/asset/{assetId}/date/{date}
        // Returns: Just a number (e.g., 86.0) - the closing price for that asset on that date
        // Minimal Coverage: Happy path + Error case + Schema validation
        // ====================================

        [Test]
        [Category("API")]
        [Category("Candlestick")]
        [Description("Happy Path: Valid asset ID and date returns 200 with closing price as a number")]
        public void Endpoint4_GetPriceByAssetId_ValidAssetAndDate_Returns200WithPrice()
        {
            // Arrange & Act - assetId 1 exists
            var response = _apiClient.GetPriceByAssetId(1, "2020-01-02");
            
            // Assert - Status
            Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK),
                "Valid asset ID and date should return 200");
            
            // Assert - Response body
            var responseBody = response.Content.ReadAsStringAsync().Result;
            Assert.That(responseBody, Is.Not.Empty, "Response should contain price");
            
            // Assert - Parse as number
            double price;
            bool isValidNumber = double.TryParse(responseBody.Trim(), out price);
            Assert.That(isValidNumber, Is.True, 
                "Response should be a valid number (closing price)");
            Assert.That(price, Is.GreaterThan(0), 
                "Price should be positive");
            
            // For assetId 1 on 2020-01-02, we know the price was 86.0
            Assert.That(price, Is.EqualTo(86.0), 
                "Price should match known historical value for asset 1 on 2020-01-02");
        }

        [Test]
        [Category("API")]
        [Category("Candlestick")]
        [Description("Error Case: Invalid asset ID returns error")]
        public void Endpoint4_GetPriceByAssetId_InvalidAssetId_ReturnsError()
        {
            // Arrange & Act - Assuming asset ID 999999 doesn't exist
            var response = _apiClient.GetPriceByAssetId(999999, "2020-01-02");
            
            // Assert
            Assert.That(response.StatusCode, 
                Is.EqualTo(System.Net.HttpStatusCode.NotFound).Or.EqualTo(System.Net.HttpStatusCode.BadRequest),
                "Invalid asset ID should return 404 or 400");
        }

        [Test]
        [Category("API")]
        [Category("Candlestick")]
        [Description("Error Case: Invalid date format returns error")]
        public void Endpoint4_GetPriceByAssetId_InvalidDate_ReturnsError()
        {
            // Arrange & Act
            var response = _apiClient.GetPriceByAssetId(1, "invalid-date");
            
            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.BadRequest),
                "Invalid date format should return 400");
        }

        // ====================================
        // CROSS-CUTTING: Data Integrity Spot Check
        // Optional: Quick validation that data across endpoints is consistent
        // ====================================

        [Test]
        [Category("API")]
        [Category("Candlestick")]
        [Category("Integration")]
        [Description("Integration: Multiple endpoints return consistent price data for same ticker/date")]
        public void CrossCutting_MultipleEndpoints_ReturnConsistentPrices()
        {
            // Arrange
            string ticker = "AAPL";
            string date = "2020-01-02";
            
            // Act - Get data from range endpoint (returns full OHLC data)
            var rangeData = _apiClient.FetchCandlestickData(ticker, date, date);
            
            // Act - Get price from specific date endpoint (returns just closing price)
            var dateResponse = _apiClient.GetPriceForDate(ticker, date);
            
            // Assert - Closing prices should match across endpoints
            if (dateResponse.StatusCode == System.Net.HttpStatusCode.OK && rangeData.Count > 0)
            {
                var priceFromDateEndpoint = double.Parse(dateResponse.Content.ReadAsStringAsync().Result.Trim());
                var rangeRecord = rangeData.First();
                
                Assert.That(priceFromDateEndpoint, Is.EqualTo(rangeRecord.close),
                    "Price from /ticker/date endpoint should match closing price from range endpoint");
            }
            else
            {
                Assert.Inconclusive("Cannot validate consistency - one or both endpoints did not return data");
            }
        }
    }
}
