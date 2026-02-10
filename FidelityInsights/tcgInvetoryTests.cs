/*
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace TcgInventoryTests
{
    /// <summary>
    /// Selenium UI tests for the TCG Inventory Login page.
    /// Demonstrates element selection, input, assertions, waits, and error handling.
    /// </summary>
    [TestFixture]
    public class LoginPageTests
    {
        private ChromeDriver driver;

        [SetUp]
        public void Setup()
        {
            // Start a new Chrome browser session
            driver = new ChromeDriver();
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
        }

        [Test]
        public void LoginPage_TitleAndElements_ArePresent()
        {
            // Navigate to the login page
            driver.Navigate().GoToUrl("http://localhost:5173");

            // Maximize the browser window
            driver.Manage().Window.Maximize();

            // Assert the page title is not empty
            Assert.That(driver.Title, Is.Not.Empty, "Page should have a title");

            // Find the login form by label text
            IWebElement usernameLabel = driver.FindElement(By.XPath("//label[contains(text(),'Username')]"));
            IWebElement passwordLabel = driver.FindElement(By.XPath("//label[contains(text(),'Password')]"));

            Assert.That(usernameLabel, Is.Not.Null, "Username label should be present");
            Assert.That(passwordLabel, Is.Not.Null, "Password label should be present");

            // Find the username and password input fields by id (from controlId in React-Bootstrap)
            IWebElement usernameInput = driver.FindElement(By.Id("username"));
            IWebElement passwordInput = driver.FindElement(By.Id("password"));

            Assert.That(usernameInput.Displayed, Is.True, "Username input should be visible");
            Assert.That(passwordInput.Displayed, Is.True, "Password input should be visible");

            // Find the login button by type
            IWebElement loginButton = driver.FindElement(By.XPath("//button[@type='submit']"));
            Assert.That(loginButton.Enabled, Is.True, "Login button should be enabled");
        }

        [Test]
        public void LoginPage_CanLoginWithValidCredentials()
        {
            driver.Navigate().GoToUrl("http://localhost:5173");

            // Enter valid credentials
            driver.FindElement(By.Id("username")).SendKeys("partner");
            driver.FindElement(By.Id("password")).SendKeys("password123");

            // Click the login button
            driver.FindElement(By.XPath("//button[@type='submit']")).Click();

            // Explicit wait for dashboard element
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            wait.Until(drv => drv.PageSource.Contains("Dashboard"));

            // Assert that login was successful 
            Assert.That(driver.Url.Contains("login"), Is.False, "Should not be on login page after login");
            // Optionally, check for a dashboard element:
            // Assert.That(driver.PageSource.Contains("Dashboard"), Is.True, "Should be on dashboard page after login");
        }

        [Test]
        public void LoginPage_ShowsErrorWithInvalidCredentials()
        {
            driver.Navigate().GoToUrl("http://localhost:5173");

            // Enter invalid credentials
            driver.FindElement(By.Id("username")).SendKeys("wronguser");
            driver.FindElement(By.Id("password")).SendKeys("wrongpass");

            driver.FindElement(By.XPath("//button[@type='submit']")).Click();

            // Explicit wait for error alert to appear
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
            IWebElement errorAlert = wait.Until(drv =>
            {
                try
                {
                    var alert = drv.FindElement(By.XPath("//div[contains(@class,'alert-danger')]"));
                    return alert.Displayed ? alert : null;
                }
                catch (NoSuchElementException)
                {
                    return null;
                }
            });
            Assert.That(errorAlert.Displayed, Is.True, "Error alert should be visible for invalid login");
            Assert.That(errorAlert.Text, Does.Contain("Invalid username or password").Or.Contain("Invalid"), "Error message should be shown");

            // status code opt
        }

        [Test]
        public void LoginPage_DisablesInputsWhileLoading()
        {
            driver.Navigate().GoToUrl("http://localhost:5173");

            // Enter credentials
            driver.FindElement(By.Id("username")).SendKeys("partner");
            driver.FindElement(By.Id("password")).SendKeys("password123");

            // Click login and immediately check if inputs are disabled
            driver.FindElement(By.XPath("//button[@type='submit']")).Click();

            // Inputs should be disabled while loading (may need to adjust timing)
            IWebElement usernameInput = driver.FindElement(By.Id("username"));
            IWebElement passwordInput = driver.FindElement(By.Id("password"));
            Assert.That(usernameInput.GetAttribute("disabled") == "true" || !usernameInput.Enabled, Is.True, "Username input should be disabled while loading");
            Assert.That(passwordInput.GetAttribute("disabled") == "true" || !passwordInput.Enabled, Is.True, "Password input should be disabled while loading");
        }

        [TearDown]
        public void Teardown()
        {
            // Clean up and close the browser
            if (driver != null)
            {
                driver.Quit();
                driver.Dispose();
            }
        }
    }

    /// <summary>
    /// Selenium UI tests for the TCG Inventory Product Management page.
    /// Demonstrates navigation, table interaction, filtering, sorting, modals, and assertions.
    /// </summary>
    [TestFixture]
    public class ProductManagementTests
    {
        private ChromeDriver driver;

        [SetUp]
        public void Setup()
        {
            driver = new ChromeDriver();
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);

            // Log in as partner before each test (reuse login logic)
            driver.Navigate().GoToUrl("http://localhost:5173");
            driver.FindElement(By.Id("username")).SendKeys("partner");
            driver.FindElement(By.Id("password")).SendKeys("password123");
            driver.FindElement(By.XPath("//button[@type='submit']")).Click();

            // Wait for dashboard or product management to load
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            wait.Until(drv => drv.PageSource.Contains("Product Management"));
        }

        [Test]
        public void ProductPage_TitleAndMainElements_ArePresent()
        {
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
            // Wait for heading
            IWebElement heading = wait.Until(drv => drv.FindElement(By.XPath("//h2[contains(text(),'Product Management')]")));
            Assert.That(heading.Displayed, Is.True, "Product Management heading should be visible");

            // Wait for Add New Product button
            IWebElement addButton = wait.Until(drv => drv.FindElement(By.XPath("//button[contains(text(),'Add New Product')]")));
            Assert.That(addButton.Displayed, Is.True, "Add New Product button should be visible");

            // Wait for products table
            IWebElement table = wait.Until(drv => drv.FindElement(By.XPath("//table[contains(@class,'themed-table')]")));
            Assert.That(table.Displayed, Is.True, "Products table should be visible");
        }

        [Test]
        public void ProductPage_CanSortByProductType()
        {
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
            // Wait for Type column header
            IWebElement typeHeader = wait.Until(drv => drv.FindElement(By.XPath("//th[contains(.,'Type')]")));
            typeHeader.Click();

            // Wait for table to update by waiting for at least one row
            wait.Until(drv => drv.FindElements(By.XPath("//table//tbody//tr/td[2]//div[contains(@class,'fw-semibold')]"))?.Count > 0);
            var typeCells = driver.FindElements(By.XPath("//table//tbody//tr/td[2]//div[contains(@class,'fw-semibold')]"));
            var types = typeCells.Select(cell => cell.Text).ToList();
            var sorted = types.OrderBy(t => t).ToList();
            Assert.That(types, Is.EqualTo(sorted), "Product types should be sorted ascending");
        }

        [Test]
        public void ProductPage_CanFilterByBrand()
        {
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
            // Wait for Brand filter dropdown
            IWebElement brandSelect = wait.Until(drv => drv.FindElement(By.XPath("//label[contains(text(),'Brand')]/following-sibling::select")));
            var selectElement = new SelectElement(brandSelect);
            var options = selectElement.Options.Where(o => !string.IsNullOrWhiteSpace(o.GetAttribute("value"))).ToList();
            if (options.Count == 0)
                Assert.Inconclusive("No brands available to filter.");
            string brandToSelect = options[0].Text;
            selectElement.SelectByText(brandToSelect);

            // Wait for table to update by waiting for at least one row
            wait.Until(drv => drv.FindElements(By.XPath("//table//tbody//tr/td[4]/div")).Count > 0);
            var brandCells = driver.FindElements(By.XPath("//table//tbody//tr/td[4]/div"));
            foreach (var cell in brandCells)
            {
                Assert.That(cell.Text, Is.EqualTo(brandToSelect), $"Product brand should be {brandToSelect}");
            }
        }

        [Test]
        public void ProductPage_CanSearchProducts()
        {
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
            // Wait for search input
            IWebElement searchInput = wait.Until(drv => drv.FindElement(By.XPath("//input[@placeholder='Search products...']")));
            searchInput.Clear();
            searchInput.SendKeys("deck");

            // Wait for table to update by waiting for at least one row
            wait.Until(drv => drv.FindElements(By.XPath("//table//tbody//tr/td[3]//div[contains(@class,'fw-semibold')]"))?.Count > 0);
            var nameCells = driver.FindElements(By.XPath("//table//tbody//tr/td[3]//div[contains(@class,'fw-semibold')]"));
            Assert.That(nameCells.Count, Is.GreaterThan(0), "At least one product should match the search");
            foreach (var cell in nameCells)
            {
                Assert.That(cell.Text.ToLower(), Does.Contain("deck"), "Product name should contain the search term");
            }
        }

        [Test]
        public void ProductPage_CanExpandProductRow()
        {
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
            // Wait for expand button
            IWebElement expandButton = wait.Until(drv => drv.FindElement(By.XPath("//button[contains(text(),'⯈')]")));
            expandButton.Click();

            // Wait for inventory details to appear
            IWebElement details = wait.Until(drv => drv.FindElement(By.XPath("//h6[contains(text(),'Inventory Across System')]")));
            Assert.That(details.Displayed, Is.True, "Inventory details should be visible after expanding");
        }

        [Test]
        public void ProductPage_CanOpenAndCloseCreateProductModal()
        {
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
            // Wait for Add New Product button
            IWebElement addButton = wait.Until(drv => drv.FindElement(By.XPath("//button[contains(text(),'Add New Product')]")));
            addButton.Click();

            // Wait for modal to appear
            IWebElement modal = wait.Until(drv => drv.FindElement(By.XPath("//div[contains(@class,'modal-content')]")));
            Assert.That(modal.Displayed, Is.True, "Create Product modal should be visible");

            // Close the modal (find the close button)
            IWebElement closeButton = modal.FindElement(By.XPath(".//button[@aria-label='Close']"));
            closeButton.Click();

            // Wait for modal to disappear
            wait.Until(drv =>
            {
                try
                {
                    return !drv.FindElement(By.XPath("//div[contains(@class,'modal-content')]"))?.Displayed;
                }
                catch (NoSuchElementException)
                {
                    return true;
                }
            });
            Assert.That(() =>
            {
                try
                {
                    return !modal.Displayed;
                }
                catch (StaleElementReferenceException)
                {
                    return true;
                }
            }, Is.True, "Create Product modal should be closed");
        }

        [TearDown]
        public void Teardown()
        {
            if (driver != null)
            {
                driver.Quit();
                driver.Dispose();
            }
        }
    }
}
*/