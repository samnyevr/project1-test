using FidelityInsights.Support;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace FidelityInsights.Hooks {
    /// <summary>
    /// Hooks for managing the lifecycle of the Chrome WebDriver for each scenario.
    /// Responsible for starting and stopping the browser, and capturing screenshots on failure.
    /// </summary>
    [Binding]
    public class WebDriverHooks {
        private readonly DriverContext _driverContext;
        private readonly ScenarioContext _scenarioContext;

        /// <summary>
        /// Constructor for dependency injection of DriverContext and ScenarioContext.
        /// </summary>
        /// <param name="driverContext">Shared context for the WebDriver instance.</param>
        /// <param name="scenarioContext">Provides information about the current scenario.</param>
        public WebDriverHooks(DriverContext driverContext, ScenarioContext scenarioContext) {
            _driverContext = driverContext;
            _scenarioContext = scenarioContext;
        }

        /// <summary>
        /// Initializes a new Chrome WebDriver before each scenario.
        /// Configures headless mode for CI environments and sets browser options for stability.
        /// </summary>
        [BeforeScenario]
        public void StartBrowser() {
            var options = new ChromeOptions();

            // Enable headless mode in CI environments by setting the HEADLESS environment variable to "true".
            var headless = Environment.GetEnvironmentVariable("HEADLESS")?.Equals("true", StringComparison.OrdinalIgnoreCase) == true;
            if (headless) {
                options.AddArgument("--headless=new");
                options.AddArgument("--window-size=1920,1080");
            }

            // Recommended options for running Chrome in containers or CI
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-dev-shm-usage");
            options.AddArgument("--disable-gpu");
            options.AddArgument("--disable-extensions");
            options.AddArgument("--disable-popup-blocking");
            options.AddArgument("--disable-setuid-sandbox");
            options.AddArgument("--remote-allow-origins=*");   // prevents "Chrome exited"
            options.AddArgument("--disable-blink-features=AutomationControlled");
            options.AddExcludedArgument("enable-automation");

            // Set download directory for file downloads
            var downloadPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "artifacts",
                "downloads"
            );

            Directory.CreateDirectory(downloadPath);

            options.AddUserProfilePreference("download.default_directory", downloadPath);
            options.AddUserProfilePreference("download.prompt_for_download", false);
            options.AddUserProfilePreference("download.directory_upgrade", true);
            options.AddUserProfilePreference("safebrowsing.enabled", true);

            // Instantiate the ChromeDriver with the specified options
            _driverContext.Driver = new ChromeDriver(options);
            // Set implicit wait to 0; prefer explicit waits for reliability
            _driverContext.Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(0);
        }

        /// <summary>
        /// Cleans up the WebDriver after each scenario.
        /// Takes a screenshot if the scenario failed, then quits and disposes the browser.
        /// </summary>
        [AfterScenario]
        public void StopBrowser() {
            try {
                if (_scenarioContext.TestError != null) {
                    // Capture a screenshot on failure for debugging
                    var screenshot = ((ITakesScreenshot)_driverContext.Driver).GetScreenshot();
                    var fileName = $"{Sanitize(_scenarioContext.ScenarioInfo.Title)}.png";
                    Directory.CreateDirectory("artifacts/screenshots");
                    screenshot.SaveAsFile(Path.Combine("artifacts/screenshots", fileName));
                }
            } finally {
                // Always quit and dispose the driver to free resources
                _driverContext.Driver.Quit();
                _driverContext.Driver.Dispose();
            }
        }
        // for use with png download verification
        public void CleanupDownloads() {
            var downloadPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "artifacts",
                "downloads"
            );

            if (Directory.Exists(downloadPath)) {
                foreach (var file in Directory.GetFiles(downloadPath)) {
                    File.Delete(file);
                }
            }
        }



        /// <summary>
        /// Sanitizes a string for use as a filename by replacing invalid characters with underscores.
        /// </summary>
        /// <param name="name">The original string.</param>
        /// <returns>A sanitized string safe for use as a filename.</returns>
        private static string Sanitize(string name)
            => string.Concat(name.Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c));
    }
}
