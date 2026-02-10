using FidelityInsights.Support;
using FidelityInsights.Pages.Components;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System.Globalization;

namespace FidelityInsights.Pages
{
    /// <summary>
    /// Page Object for the Portfolio Overview page.
    ///
    /// This class contains:
    /// - Navigation helpers
    /// - Training session modal flows
    /// - Header date controls (Quick + Range)
    /// - "Portfolio Value Over Time" chart range controls + validation helpers
    ///
    /// Conventions used:
    /// - Locators are grouped by feature area and declared near the top.
    /// - Public methods represent stable, higher-level interactions that tests can call.
    /// - Private helpers encapsulate brittle mechanics (XPath quirks, parsing rules, etc.).
    /// - Explicit waits are favored over Thread.Sleep to reduce flakiness.
    /// </summary>
    public class PortfolioOverviewPage : AbstractPage
    {
        // ---------------------------------------------------------------------
        // Navigation
        // ---------------------------------------------------------------------

        private const string PortfolioOverviewUrl = "https://d2rczu3zvi71ix.cloudfront.net/portfolio/overview";

        public PortfolioOverviewPage(DriverContext ctx) : base(ctx.Driver) { }

        private PrimaryNavigationBar Nav => new PrimaryNavigationBar(Driver);


        public void Open() => Driver.Navigate().GoToUrl(PortfolioOverviewUrl);
        public void Refresh() => Driver.Navigate().Refresh();

        // ---------------------------------------------------------------------
        // Primary navigation (header nav) actions
        // ---------------------------------------------------------------------

        public void NavigateToTradeStocksFromHeader() => Nav.GoToTradeStocks();

        public void NavigateToTrainingSessionsFromHeader() => Nav.GoToTrainingSessions();

        public void NavigateToPortfolioOverviewFromHeader() => Nav.GoToPortfolioOverview();


        // ---------------------------------------------------------------------
        // Locators
        // ---------------------------------------------------------------------

        private static readonly By NoSessionRootBy =
            By.CssSelector("app-no-session");

        /// <summary>
        /// Stable UI signal that a session is active.
        /// The dev-testing theme toggle appears only when a viewable session exists (per current UI).
        /// </summary>
        private static readonly By ThemeToggleBy =
            By.CssSelector("section.dev-testing button.theme-toggle");

        private static readonly By TrainingSessionModalBy =
            By.CssSelector("app-training-session-modal .settings-modal");

        /// <summary>
        /// Used to detect any modal state (intermediate dialog OR settings form).
        /// </summary>
        private static readonly By ModalBackdropOrRootBy =
            By.CssSelector("app-training-session-modal, app-training-session-modal .settings-modal-backdrop, app-training-session-modal .settings-modal");

        /// <summary>
        /// Intermediate modal CTA (some builds show a "Start New Training Session" step before the full form).
        /// Intermediate modal may not include cancel/close controls, so tests must be resilient.
        /// </summary>
        private static readonly By StartNewTrainingSessionBtnBy =
            By.XPath("//app-training-session-modal//div[contains(@class,'settings-modal')]//button[normalize-space(.)='Start New Training Session']");

        /// <summary>
        /// Presence indicates we are on the full settings UI.
        /// </summary>
        private static readonly By SettingsFormBy =
            By.CssSelector("app-training-session-modal form.settings-form, app-training-session-modal .settings-form");

        // Fields inside the settings form
        private static readonly By StartDateTriggerBy =
            By.CssSelector("app-training-session-modal button#start-date");

        private static readonly By StartDateDropdownBy =
            By.CssSelector("app-training-session-modal .date-picker .dropdown-menu");

        private static readonly By SessionLengthSelectBy =
            By.CssSelector("app-training-session-modal select#end-date");

        private static readonly By StartingBalanceInputBy =
            By.CssSelector("app-training-session-modal input#starting-balance");

        private static readonly By StartTrainingSessionBtnBy =
            By.XPath("//app-training-session-modal//button[contains(normalize-space(.),'Start Training Session')]");

        private static readonly By StartTrainingSessionBtnDisabledBy =
            By.XPath("//app-training-session-modal//button[contains(normalize-space(.),'Start Training Session') and (@disabled or contains(@class,'disabled'))]");

        private static readonly By CancelBtnBy =
            By.XPath("//app-training-session-modal//button[normalize-space(.)='Cancel' or normalize-space(.)='Close']");

        private static readonly By FormErrorBy =
            By.CssSelector("app-training-session-modal .form-error");

        private static readonly By AnyModalErrorBy =
            By.CssSelector("app-training-session-modal .form-error, app-training-session-modal .input-error, app-training-session-modal .alert-danger");

        // ---------------------------------------------------------------------
        // Locators - Page-level session context
        // ---------------------------------------------------------------------

        private static readonly By DevTestingSectionBy =
            By.CssSelector("section.dev-testing");

        private static readonly By DatePillStrongBy =
            By.CssSelector("section.date-pill strong");

        // ---------------------------------------------------------------------
        // Locators - Header date controls (Quick + Range)
        // ---------------------------------------------------------------------

        private static readonly By PortfolioDateControlsRootBy =
            By.CssSelector("app-portfolio-date-controls");

        /// <summary>
        /// QUICK dropdown: selects a base date anchor/window. Tests use it to get out of "not set" state.
        /// </summary>
        private static readonly By QuickSelectBy =
            By.XPath("//app-portfolio-date-controls//label[.//span[normalize-space()='Quick' or normalize-space()='QUICK']]//select");

        /// <summary>
        /// RANGE dropdown: selects the window size (1M, 3M, 6M, 1Y, YTD, etc.).
        /// </summary>
        private static readonly By RangeSelectBy =
            By.XPath("//app-portfolio-date-controls//label[.//span[normalize-space()='Range' or normalize-space()='RANGE']]//select");

        /// <summary>
        /// Fallback locator in case the implementation changes to clickable pills.
        /// </summary>
        private static readonly By RangePillBy =
            By.CssSelector("app-portfolio-date-controls .range-pill, app-portfolio-date-controls button.range-pill");

        private const string NotSetToken = "not set";

        // =====================================================================
        // Locators (Portfolio overview display feature)
        // =====================================================================

        // Summary cards grid
        private static readonly By StatsGridBy =
            By.CssSelector("section.stats-grid");

        private static readonly By TotalValueStatCardBy =
            By.XPath("//section[contains(@class,'stats-grid')]//article[contains(@class,'stat-card')][.//p[contains(@class,'stat-label') and normalize-space()='Total Value']]");

        private static readonly By CashAvailableCardBy =
            By.CssSelector("section.stats-grid a.stat-card.link-card[aria-label='Trade stocks with available cash']");

        private static readonly By UnrealizedPnlCardBy =
            By.XPath("//section[contains(@class,'stats-grid')]//article[contains(@class,'stat-card')][.//p[contains(@class,'stat-label') and contains(normalize-space(.),'Unrealized')]]");

        private static readonly By UnrealizedPnlRangePillsBy =
            By.CssSelector("section.stats-grid article.stat-card .range-toggle button.range-pill");

        // Allocation panel (host component renders an article.panel.allocation-panel)
        private static readonly By AllocationPanelBy =
            By.CssSelector("article.panel.allocation-panel");

        private static readonly By AllocationLegendItemsBy =
            By.CssSelector("article.panel.allocation-panel .legend span");

        // Portfolio Value Over Time chart
        private static readonly By ChartPanelBy =
            By.CssSelector("article.panel.chart-panel");

        private static readonly By ChartRangePillsBy =
            By.CssSelector("article.panel.chart-panel .range-toggle button.range-pill");

        private static readonly By ChartActiveRangePillBy =
            By.CssSelector("article.panel.chart-panel .range-toggle button.range-pill.active");

        private static readonly By ChartAxisDatesBy =
            By.CssSelector("article.panel.chart-panel .axis-dates span");

        // Top Positions / Top Movers panels (inside custom components)
        private static readonly By TopPositionsPanelBy =
            By.CssSelector("app-portfolio-top-positions article.panel.bars-panel");

        private static readonly By TopPositionsRowsBy =
            By.CssSelector("app-portfolio-top-positions article.panel.bars-panel .bar-row");

        private static readonly By TopMoversPanelBy =
            By.CssSelector("app-portfolio-top-movers article.panel.movers-panel");

        private static readonly By TopMoversRowsBy =
            By.CssSelector("app-portfolio-top-movers article.panel.movers-panel .mover-row");

        // Holdings panel + controls + actual table rows
        private static readonly By HoldingsPanelBy =
            By.CssSelector("section.panel.holdings");

        private static readonly By HoldingsSearchInputBy =
            By.CssSelector("section.panel.holdings .holdings-controls input[placeholder='Search...']");

        private static readonly By HoldingsSortSelectBy =
            By.CssSelector("section.panel.holdings .holdings-controls select");

        private static readonly By HoldingsTableBy =
            By.CssSelector("section.panel.holdings app-portfolio-holdings-table table");

        private static readonly By HoldingsTableHeaderCellsBy =
            By.CssSelector("section.panel.holdings app-portfolio-holdings-table table thead th");

        private static readonly By HoldingsTableRowsBy =
            By.CssSelector("section.panel.holdings app-portfolio-holdings-table table tbody tr.holding-row");

        // Refresh / "Last updated"
        private static readonly By RefreshPortfolioButtonBy =
            By.CssSelector("button.icon-button[aria-label='Refresh portfolio']");

        private static readonly By LastUpdatedTimeBy =
            By.CssSelector(".update.update-top .update-time");

        private static readonly By TotalValueSummaryBy =
            By.CssSelector("[data-testid='total-value-summary'], app-portfolio-summary-card.total-value, section.summary-cards .total-value");

        private static readonly By CashAvailableSummaryBy =
            By.CssSelector("[data-testid='cash-available-summary'], app-portfolio-summary-card.cash-available, section.summary-cards .cash-available");

        private static readonly By UnrealizedPerformanceSummaryBy =
            By.CssSelector("[data-testid='unrealized-performance-summary'], app-portfolio-summary-card.unrealized, section.summary-cards .unrealized");

        // Asset allocation
        private static readonly By AssetAllocationBy =
            By.CssSelector("[data-testid='asset-allocation'], article.panel.asset-allocation, section.asset-allocation");

        // Top positions / top movers
        private static readonly By TopPositionsBy =
            By.CssSelector("[data-testid='top-positions'], article.panel.top-positions, section.top-positions");

        private static readonly By TopMoversBy =
            By.CssSelector("[data-testid='top-movers'], article.panel.top-movers, section.top-movers");

        // Holdings
        private static readonly By HoldingsSectionBy =
            By.CssSelector("[data-testid='holdings'], article.panel.holdings, section.holdings");

        private static readonly By HoldingsRowsBy =
            By.CssSelector("[data-testid='holdings-table'] tbody tr, section.holdings tbody tr, article.panel.holdings tbody tr, app-holdings tbody tr");

        // Sortable headers:
        // We look for header cells that advertise sorting via role/aria or a clickable button.
        private static readonly By HoldingsSortableHeadersBy =
            By.CssSelector(
                "[data-testid='holdings-table'] thead th[role='columnheader']," +
                " section.holdings thead th[role='columnheader']," +
                " article.panel.holdings thead th[role='columnheader']," +
                " app-holdings thead th[role='columnheader']");

        // Refresh data control for Portfolio. If no explicit refresh button exists, tests can fall back to browser refresh.
        private static readonly By PortfolioRefreshButtonBy =
            By.CssSelector("[data-testid='portfolio-refresh'], button[aria-label*='Refresh' i], button:has(svg[aria-label*='refresh' i])");

        // ---------------------------------------------------------------------
        // Theme
        // ---------------------------------------------------------------------

        // Get current theme state
        public string GetThemeState()
        {
            IJavaScriptExecutor js = (IJavaScriptExecutor)Driver;
            return (string)js.ExecuteScript("return document.documentElement.getAttribute('data-theme');");

            // deprecated
            // var toggle = Driver.FindElement(By.CssSelector("button.theme-toggle, button.toggle-switch[role='switch']"));
            // return toggle.GetAttribute("aria-checked");
        }

        // Toggle the theme
        public void ToggleTheme()
        {
            var toggle = Driver.FindElement(By.CssSelector("button.theme-toggle, button.toggle-switch[role='switch']"));
            toggle.Click();

            // Wait for the DOM to update (small delay to ensure theme change has processed)
            System.Threading.Thread.Sleep(500); // Simple approach
        }

        // ---------------------------------------------------------------------
        // Session state / waits
        // ---------------------------------------------------------------------

        public bool IsTrainingSessionActive() =>
            Driver.FindElements(ThemeToggleBy).Any(e => e.Displayed);

        public void WaitForTrainingSessionToBeActive(int seconds = 10)
        {
            new WebDriverWait(Driver, TimeSpan.FromSeconds(seconds))
                .Until(d => d.FindElements(ThemeToggleBy).Any(e => e.Displayed));
        }

        public void WaitForTrainingSessionToBeInactive(int seconds = 5)
        {
            new WebDriverWait(Driver, TimeSpan.FromSeconds(seconds))
                .Until(d => !d.FindElements(ThemeToggleBy).Any(e => e.Displayed));
        }

        public bool IsTrainingSessionModalOpen() =>
            Driver.FindElements(ModalBackdropOrRootBy).Any(e => e.Displayed);

        /// <summary>
        /// The form's submit button is disabled when client-side validation fails.
        /// We check this before clicking submit to keep negative-path tests deterministic.
        /// </summary>
        public bool IsStartTrainingSessionDisabled() =>
            Driver.FindElements(StartTrainingSessionBtnDisabledBy).Any();

        public string WaitForFormError(int seconds = 3)
        {
            var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(seconds));
            var el = wait.Until(d => d.FindElements(FormErrorBy).FirstOrDefault(e => e.Displayed));
            return el?.Text ?? string.Empty;
        }

        public string GetTrainingSessionErrorMessage() =>
            Driver.FindElements(AnyModalErrorBy).FirstOrDefault(e => e.Displayed)?.Text ?? string.Empty;

        /// <summary>
        /// Lightweight check used by tests to confirm the Portfolio is showing session context.
        /// This deliberately avoids asserting exact IDs/text that may change.
        /// </summary>
        public bool IsSessionContextVisible()
        {
            var dev = Driver.FindElements(DevTestingSectionBy).FirstOrDefault(e => e.Displayed);
            if (dev == null) return false;

            var pill = Driver.FindElements(DatePillStrongBy).FirstOrDefault(e => e.Displayed);
            return pill != null;
        }

        public bool IsNoSessionPromptVisible() =>
            Driver.FindElements(NoSessionRootBy).Any(e => e.Displayed);

        // ---------------------------------------------------------------------
        // Training session: high-level flows
        // ---------------------------------------------------------------------

        /// <summary>
        /// Ensures a session is active for scenarios that need it.
        /// If one is already active, does nothing.
        /// </summary>
        public void EnsureActiveTrainingSession()
        {
            if (!IsTrainingSessionActive())
                StartNewTrainingSession("a valid start", "a valid length", "a valid balance");
        }

        public (bool created, string? startingBalanceUi) EnsureActiveTrainingSessionWithUniqueBalance()
        {
            if (IsTrainingSessionActive())
                return (false, null);

            var cents = DateTime.UtcNow.Ticks % 100;
            var raw = 909090909m + (cents / 100m);

            var ui = raw.ToString("C2", CultureInfo.GetCultureInfo("en-US"));
            StartNewTrainingSession("a valid start", "a valid length", raw.ToString(CultureInfo.InvariantCulture));

            return (true, ui);
        }

        /// <summary>
        /// Opens the modal and ensures the settings form is visible.
        /// The UI may show an intermediate modal first; this handles that transition.
        /// </summary>
        public void OpenTrainingSessionSettingsForm()
        {
            OpenTrainingSessionDialog();
            ClickStartNewTrainingSessionIfPresent();
            WaitForSettingsForm();
        }

        /// <summary>
        /// End-to-end creation attempt.
        /// - For invalid inputs: may return early if submit is disabled or an error appears.
        /// - For valid inputs: waits for modal to close and the "session active" indicator to appear.
        /// </summary>
        public void StartNewTrainingSession(string startDate, string sessionLengthToken, string startingBalanceToken)
        {
            OpenTrainingSessionSettingsForm();

            // Apply inputs only if token maps to a value; "(not provided)" intentionally skips fields.
            if (TryResolveDate(startDate, out var resolvedStartDate))
                SetStartDate(resolvedStartDate);

            var length = ResolveSessionLength(sessionLengthToken);
            if (length != null)
                SetSessionLength(length);

            var resolvedBalance = ResolveBalance(startingBalanceToken);
            if (resolvedBalance != null)
                SetStartingBalance(resolvedBalance);

            // If the UI disables submit, return immediately—this indicates client-side validation failure.
            if (IsStartTrainingSessionDisabled())
                return;

            var submit = Wait.Until(ExpectedConditions.ElementToBeClickable(StartTrainingSessionBtnBy));
            submit.Click();

            // For known invalid tokens we expect a synchronous error message. We don't wait for modal close.
            if (IsNegativeOrZeroToken(startingBalanceToken))
            {
                WaitForFormError(3);
                return;
            }

            // Success path: modal closes and the page shows "session active" indicator.
            Wait.Until(d => d.FindElements(TrainingSessionModalBy).Count == 0);
            WaitForTrainingSessionToBeActive(10);
        }

        /// <summary>
        /// Cancels session creation.
        /// - If settings form is open: click Cancel/Close.
        /// - If stuck in intermediate modal (which may not have Cancel): refresh to escape.
        /// </summary>
        public void CancelTrainingSessionCreation()
        {
            if (Driver.FindElements(SettingsFormBy).Any(e => e.Displayed))
            {
                var cancel = Wait.Until(ExpectedConditions.ElementToBeClickable(CancelBtnBy));
                cancel.Click();
                return;
            }

            Refresh();
        }

        // ---------------------------------------------------------------------
        // Training session: modal mechanics
        // ---------------------------------------------------------------------

        /// <summary>
        /// Opens the training session modal from the "no session" state.
        /// Safe to call repeatedly; if modal is already open, no-op.
        /// </summary>
        public void OpenTrainingSessionDialog()
        {
            if (Driver.FindElements(TrainingSessionModalBy).Any(e => e.Displayed))
                return;

            var noSession = Driver.FindElements(NoSessionRootBy).FirstOrDefault();
            if (noSession == null) return;

            // Wait for the CTA button inside the no-session component to be clickable.
            Wait.Until(d =>
            {
                try
                {
                    if (d.FindElements(TrainingSessionModalBy).Any(m => m.Displayed)) return false;

                    var root = d.FindElements(NoSessionRootBy).FirstOrDefault();
                    if (root == null) return false;

                    var cta = root.FindElement(By.XPath(".//button[contains(normalize-space(.),'Start')]"));
                    return cta.Displayed && cta.Enabled;
                }
                catch (NoSuchElementException) { return false; }
                catch (StaleElementReferenceException) { return false; }
            });

            var ctaNow = Driver.FindElements(NoSessionRootBy).First()
                .FindElement(By.XPath(".//button[contains(normalize-space(.),'Start')]"));

            Wait.Until(ExpectedConditions.ElementToBeClickable(ctaNow)).Click();
            Wait.Until(d => d.FindElements(TrainingSessionModalBy).Any(m => m.Displayed));
        }

        /// <summary>
        /// If the UI is showing an intermediate modal step, click through to the settings form.
        /// If the settings form is already visible, no-op.
        /// </summary>
        private void ClickStartNewTrainingSessionIfPresent()
        {
            if (Driver.FindElements(SettingsFormBy).Any(e => e.Displayed))
                return;

            var btn = Driver.FindElements(StartNewTrainingSessionBtnBy)
                .FirstOrDefault(b => b.Displayed && b.Enabled);

            if (btn == null) return;

            Wait.Until(ExpectedConditions.ElementToBeClickable(btn)).Click();
        }

        /// <summary>
        /// Waits until the settings form and its key fields are present.
        /// This is used as the "form is ready" gate for training session tests.
        /// </summary>
        private void WaitForSettingsForm()
        {
            Wait.Until(d =>
            {
                var form = d.FindElements(SettingsFormBy).FirstOrDefault();
                return (form != null && form.Displayed) ? form : null;
            });

            // Field-level waits reduce flakiness where the form appears before inputs are hydrated.
            Wait.Until(d => d.FindElement(StartDateTriggerBy));
            Wait.Until(d => d.FindElement(SessionLengthSelectBy));
            Wait.Until(d => d.FindElement(StartingBalanceInputBy));
        }

        // ---------------------------------------------------------------------
        // Training session: token mapping helpers
        // ---------------------------------------------------------------------

        /// <summary>
        /// Maps feature tokens to actual UI values for the session length select.
        /// Keeping this centralized avoids repeating mapping logic in step definitions.
        /// </summary>
        private static string? ResolveSessionLength(string token)
        {
            if (IsNotProvided(token)) return null;

            var t = token.Trim().ToLowerInvariant();
            if (t == "a valid length") return "6 months";
            if (t == "a valid end") return "6 months";

            // Allow direct UI values (e.g., "1 year") if feature files use them.
            return token.Trim();
        }

        /// <summary>
        /// Maps feature tokens to actual numeric strings used for the balance input.
        /// </summary>
        private static string? ResolveBalance(string token)
        {
            if (IsNotProvided(token)) return null;

            var t = token.Trim().ToLowerInvariant();
            if (t == "a valid balance") return "10000";
            if (t == "negative") return "-1";
            if (t == "zero") return "0";

            return token.Trim();
        }

        private static bool IsNegativeOrZeroToken(string token)
        {
            var t = token?.Trim().ToLowerInvariant();
            return t == "negative" || t == "zero" || t == "-1" || t == "0";
        }

        // ---------------------------------------------------------------------
        // Training session: field setters
        // ---------------------------------------------------------------------

        private void SetStartingBalance(string startingBalance)
        {
            var input = Wait.Until(d => d.FindElement(StartingBalanceInputBy));
            input.Clear();
            input.SendKeys(startingBalance);
        }

        private void SetSessionLength(string sessionLength)
        {
            var select = new SelectElement(Wait.Until(d => d.FindElement(SessionLengthSelectBy)));
            select.SelectByText(sessionLength);
        }

        private void SetStartDate(DateTime date)
        {
            var trigger = Wait.Until(ExpectedConditions.ElementToBeClickable(StartDateTriggerBy));
            trigger.Click();

            var dropdown = Wait.Until(d =>
            {
                var dd = d.FindElements(StartDateDropdownBy).FirstOrDefault();
                return (dd != null && dd.Displayed) ? dd : null;
            });

            NavigateCalendarToMonth(dropdown, date);
            ClickCalendarDay(dropdown, date.Day);
        }

        // ---------------------------------------------------------------------
        // Training session: date picker interaction (best-effort)
        // ---------------------------------------------------------------------

        /// <summary>
        /// Attempts to navigate the date picker to a target month.
        /// This is intentionally defensive because date picker implementations vary
        /// and can change without notice.
        /// </summary>
        private void NavigateCalendarToMonth(IWebElement dropdown, DateTime target)
        {
            for (int i = 0; i < 48; i++)
            {
                var title = dropdown.FindElements(By.XPath(
                    ".//*[self::div or self::span]" +
                    "[contains(normalize-space(.),'January') or contains(normalize-space(.),'February') or contains(normalize-space(.),'March') or " +
                    " contains(normalize-space(.),'April') or contains(normalize-space(.),'May') or contains(normalize-space(.),'June') or " +
                    " contains(normalize-space(.),'July') or contains(normalize-space(.),'August') or contains(normalize-space(.),'September') or " +
                    " contains(normalize-space(.),'October') or contains(normalize-space(.),'November') or contains(normalize-space(.),'December')]"
                )).FirstOrDefault(e => e.Displayed);

                if (title == null) return;

                if (TryParseMonthYear(title.Text, out var currentMonth))
                {
                    var targetMonth = new DateTime(target.Year, target.Month, 1);
                    if (currentMonth == targetMonth) return;

                    var navButtons = dropdown.FindElements(By.XPath(".//button"))
                        .Where(b => b.Displayed && b.Enabled)
                        .ToList();

                    // Some date pickers use "prev" then "next" buttons; we pick based on direction.
                    if (navButtons.Count >= 2)
                    {
                        if (currentMonth > targetMonth) navButtons.First().Click();
                        else navButtons.Last().Click();
                        continue;
                    }
                }

                return;
            }
        }

        private void ClickCalendarDay(IWebElement dropdown, int day)
        {
            // Prefer a button-based day cell when available.
            var dayBtn = dropdown.FindElements(By.XPath($".//button[normalize-space(.)='{day}']"))
                .FirstOrDefault(b => b.Displayed && b.Enabled);

            if (dayBtn != null)
            {
                dayBtn.Click();
                return;
            }

            // Fallback: click any visible element containing the day number.
            var dayCell = dropdown.FindElements(By.XPath($".//*[normalize-space(.)='{day}']"))
                .FirstOrDefault(e => e.Displayed);

            dayCell?.Click();
        }

        private static bool TryParseMonthYear(string text, out DateTime month)
        {
            if (DateTime.TryParseExact(text.Trim(), "MMMM yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
            {
                month = new DateTime(dt.Year, dt.Month, 1);
                return true;
            }

            month = default;
            return false;
        }

        private static bool IsNotProvided(string s) =>
            string.IsNullOrWhiteSpace(s) || s.Trim().Equals("(not provided)", StringComparison.OrdinalIgnoreCase);

        private static bool TryResolveDate(string token, out DateTime date)
        {
            date = default;

            // "(not provided)" => skip setting
            if (IsNotProvided(token)) return false;

            // Feature token "a valid start" means: do not set a specific date (let UI default).
            if (token.Trim().Equals("a valid start", StringComparison.OrdinalIgnoreCase)) return false;

            if (DateTime.TryParse(token, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var parsed))
            {
                date = parsed.Date;
                return true;
            }

            return false;
        }

        // ---------------------------------------------------------------------
        // Header date controls: helper methods used by range selection tests
        // ---------------------------------------------------------------------

        public string GetSelectedDatesPillText()
        {
            var el = Driver.FindElements(DatePillStrongBy).FirstOrDefault(e => e.Displayed);
            return el?.Text?.Trim() ?? string.Empty;
        }

        /// <summary>
        /// Waits until the "Selected dates" pill shows a real range.
        /// This is the primary signal that QUICK/RANGE selection has been applied.
        /// </summary>
        public void WaitForSelectedDatesPillToBeSet(int seconds = 10)
        {
            var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(seconds));
            wait.Until(_ =>
            {
                var txt = GetSelectedDatesPillText();
                return !string.IsNullOrWhiteSpace(txt)
                       && txt.Contains("→")
                       && !txt.Contains(NotSetToken, StringComparison.OrdinalIgnoreCase);
            });
        }

        /// <summary>
        /// Initializes the header date range by selecting the first "real" value in the QUICK dropdown.
        /// Tests use this because the page loads with "not set" values and RANGE changes are otherwise meaningless.
        /// </summary>
        public void InitializeDateRangeFromQuickFirstOption()
        {
            Wait.Until(d => d.FindElements(PortfolioDateControlsRootBy).Any(e => e.Displayed));

            // If already set, don't reseat the value: reduces flakiness and speeds up tests.
            var pill = GetSelectedDatesPillText();
            if (!string.IsNullOrWhiteSpace(pill) &&
                pill.Contains("→") &&
                !pill.Contains(NotSetToken, StringComparison.OrdinalIgnoreCase))
                return;

            var quickSelectEl = Wait.Until(d => d.FindElement(QuickSelectBy));
            var quick = new SelectElement(quickSelectEl);

            var idx = FirstNonPlaceholderIndex(quick);
            if (idx < 0)
                throw new InvalidOperationException("Quick dropdown did not contain a selectable option.");

            quick.SelectByIndex(idx);

            // Wait until the pill reflects the applied selection.
            WaitForSelectedDatesPillToBeSet();
        }

        /// <summary>
        /// Selects a header RANGE value (e.g., 1M, 3M, 6M, 1Y, YTD) and waits for:
        /// - the selected indicator to update
        /// - the "Selected dates" pill to change
        /// </summary>
        public void SelectRange(string range)
        {
            Wait.Until(d => d.FindElements(PortfolioDateControlsRootBy).Any(e => e.Displayed));

            var pillBefore = GetSelectedDatesPillText();

            // Prefer RANGE <select> when present.
            var rangeSelect = Driver.FindElements(RangeSelectBy).FirstOrDefault();
            if (rangeSelect != null)
            {
                var select = new SelectElement(rangeSelect);
                select.SelectByText(range.Trim());
            }
            else
            {
                // Fallback: click a visible pill/button matching the range text.
                var btn = Driver.FindElements(RangePillBy)
                    .FirstOrDefault(b => b.Displayed && b.Enabled &&
                                         string.Equals(b.Text.Trim(), range.Trim(), StringComparison.OrdinalIgnoreCase));

                if (btn == null)
                    throw new NoSuchElementException($"Could not find RANGE select or range pill for '{range}'.");

                Wait.Until(ExpectedConditions.ElementToBeClickable(btn)).Click();
            }

            WaitForRangeIndicator(range, 10);

            // Confirm the dates pill changed as a result of range selection.
            Wait.Until(_ =>
            {
                var after = GetSelectedDatesPillText();
                return !string.IsNullOrWhiteSpace(after)
                       && after.Contains("→")
                       && !after.Contains(NotSetToken, StringComparison.OrdinalIgnoreCase)
                       && after != pillBefore;
            });
        }

        /// <summary>
        /// Returns the currently selected RANGE value.
        /// This reads the selected <option> when a select exists; otherwise it finds the "active" pill.
        /// </summary>
        public string GetSelectedRangeIndicator()
        {
            var rangeSelect = Driver.FindElements(RangeSelectBy).FirstOrDefault();
            if (rangeSelect != null)
            {
                var select = new SelectElement(rangeSelect);
                return select.SelectedOption?.Text?.Trim() ?? string.Empty;
            }

            var pills = Driver.FindElements(RangePillBy).Where(p => p.Displayed).ToList();
            if (pills.Count == 0) return string.Empty;

            var active = pills.FirstOrDefault(p => (p.GetAttribute("class") ?? string.Empty).Contains("active"));
            return (active ?? pills[0]).Text.Trim();
        }

        private void WaitForRangeIndicator(string expected, int seconds)
        {
            var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(seconds));
            wait.Until(_ => string.Equals(GetSelectedRangeIndicator(), expected.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Returns the index of the first non-placeholder option for a <select>.
        /// This prevents tests from selecting entries like "Pick a date" or other sentinel values.
        /// </summary>
        private static int FirstNonPlaceholderIndex(SelectElement select)
        {
            for (int i = 0; i < select.Options.Count; i++)
            {
                var text = (select.Options[i].Text ?? string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(text)) continue;

                // Common placeholder patterns.
                if (text.Equals("Pick a date", StringComparison.OrdinalIgnoreCase)) continue;
                if (text.Contains("select", StringComparison.OrdinalIgnoreCase)) continue;

                return i;
            }
            return -1;
        }

        // ---------------------------------------------------------------------
        // "Portfolio Value Over Time" chart range toggle
        // ---------------------------------------------------------------------

        /// <summary>
        /// Selects a chart range pill (separate from header RANGE) and waits for:
        /// - active pill to change
        /// - axis labels to update (best-effort)
        ///
        /// Important:
        /// Some selections may not visibly change axis labels if the underlying data collapses to the same ticks.
        /// In that case, the axis-change wait may need to be relaxed.
        /// </summary>
        public void SelectPortfolioValueOverTimeRange(string range)
        {
            Wait.Until(d => d.FindElement(ChartPanelBy));

            var beforeAxis = GetPortfolioValueOverTimeAxisDates();

            var pills = Wait.Until(d =>
            {
                var els = d.FindElements(ChartRangePillsBy).Where(e => e.Displayed && e.Enabled).ToList();
                return els.Count > 0 ? els : null;
            });

            var target = pills.FirstOrDefault(p => p.Text.Trim().Equals(range, StringComparison.OrdinalIgnoreCase));
            if (target == null)
                throw new NoSuchElementException($"Chart range pill '{range}' not found.");

            target.Click();

            Wait.Until(_ => GetSelectedPortfolioValueOverTimeRange()
                .Equals(range, StringComparison.OrdinalIgnoreCase));

            Wait.Until(_ =>
            {
                var afterAxis = GetPortfolioValueOverTimeAxisDates();
                return afterAxis.Count > 0 && !ListEquals(beforeAxis, afterAxis);
            });
        }

        public string GetSelectedPortfolioValueOverTimeRange()
        {
            var active = Driver.FindElements(ChartActiveRangePillBy).FirstOrDefault(e => e.Displayed);
            return active?.Text.Trim() ?? string.Empty;
        }

        public List<string> GetPortfolioValueOverTimeAxisDates()
        {
            return Driver.FindElements(ChartAxisDatesBy)
                .Where(e => e.Displayed)
                .Select(e => e.Text.Trim())
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .ToList();
        }

        private static bool ListEquals(List<string> a, List<string> b)
        {
            if (a.Count != b.Count) return false;
            for (int i = 0; i < a.Count; i++)
            {
                if (!string.Equals(a[i], b[i], StringComparison.Ordinal))
                    return false;
            }
            return true;
        }

        // ---------------------------------------------------------------------
        // Chart date validation helpers
        // ---------------------------------------------------------------------

        /// <summary>
        /// Returns the parseable (month/year) start and end bounds for the chart axis.
        /// We use the leftmost tick as "start" and the rightmost tick as "end".
        /// </summary>
        public (DateTime? start, DateTime? end) GetPortfolioValueOverTimeAxisBounds()
        {
            var dates = GetPortfolioValueOverTimeAxisDates();
            if (dates.Count == 0) return (null, null);

            var start = TryParseAxisMonthYear(dates.First());
            var end = TryParseAxisMonthYear(dates.Last());
            return (start, end);
        }

        /// <summary>
        /// Returns the rightmost axis tick parsed to the first day of the month.
        /// Used by tests to verify the chart end date is stable across range changes.
        /// </summary>
        public DateTime? GetPortfolioValueOverTimeEndDate()
        {
            var dates = GetPortfolioValueOverTimeAxisDates();
            if (dates.Count == 0) return null;

            return TryParseAxisMonthYear(dates.Last());
        }

        /// <summary>
        /// Month difference ignoring day; e.g. Jan -> Jul = 6.
        /// This is a stable way to validate chart span when ticks are month/year only.
        /// </summary>
        public static int MonthDiff(DateTime start, DateTime end)
        {
            return (end.Year - start.Year) * 12 + (end.Month - start.Month);
        }

        /// <summary>
        /// Expected span (in months) for the chart range pills.
        /// Adjust if chart semantics change.
        ///
        /// Note on YTD:
        /// - YTD is inherently variable (depends on the end date month).
        /// - In tests we typically validate YTD with a broader rule (Jan of the same year),
        ///   but for now the step uses tolerance around a nominal value.
        /// </summary>
        public static int ExpectedMonthsForChartRange(string range)
        {
            return range.Trim().ToUpperInvariant() switch
            {
                "1M" => 1,
                "3M" => 3,
                "6M" => 6,
                "1Y" => 12,
                "YTD" => 12,
                _ => 0,
            };
        }

        /// <summary>
        /// Parses an axis label into a DateTime (first day of that month).
        ///
        /// Current UI renders labels like:
        ///   toLocaleString('en-US', { month: 'short', year: '2-digit' })
        /// which produces "MMM yy" (e.g., "Jan 26").
        ///
        /// We apply a deterministic pivot for 2-digit years:
        /// - 00-30 => 2000-2030
        /// - 31-99 => 1931-1999
        ///
        /// If the UI changes to "MMM yyyy", DateTime.TryParse will still likely work,
        /// but this method is where you should explicitly add the new format.
        /// </summary>
        private static DateTime? TryParseAxisMonthYear(string label)
        {
            label = (label ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(label)) return null;

            // Primary expected format: "MMM yy" (e.g., "Jan 26")
            if (DateTime.TryParseExact(label, "MMM yy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dtShort))
            {
                var yy = dtShort.Year % 100;
                var year = (yy <= 30) ? 2000 + yy : 1900 + yy;
                return new DateTime(year, dtShort.Month, 1);
            }

            // Fallback: attempt general parsing (covers "MMM yyyy" if it ever changes).
            if (DateTime.TryParse(label, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dtAny))
                return new DateTime(dtAny.Year, dtAny.Month, 1);

            return null;
        }

        // ---------------------------------------------------------------------
        // NEW: Portfolio overview display helpers (for portfolio-overview-display.feature)
        // ---------------------------------------------------------------------

        /// <summary>
        /// Summary: Total Value visible.
        /// This is a presence check only; value correctness is validated elsewhere (if needed).
        /// </summary>
        public bool IsTotalValueSummaryVisible() => IsVisible(TotalValueStatCardBy);

        public bool IsCashAvailableSummaryVisible() => IsVisible(CashAvailableCardBy);

        public bool IsUnrealizedPerformanceSummaryVisible()
            => IsVisible(UnrealizedPnlCardBy) && Driver.FindElements(UnrealizedPnlRangePillsBy).Any(e => e.Displayed);
        public bool IsAssetAllocationVisible()
            => IsVisible(AllocationPanelBy) && Driver.FindElements(AllocationLegendItemsBy).Any(e => e.Displayed);

        public bool IsPortfolioValueOverTimeVisible()
            => IsVisible(ChartPanelBy) && Driver.FindElements(ChartAxisDatesBy).Any(e => e.Displayed);

        public bool IsTopPositionsVisible()
            => IsVisible(TopPositionsPanelBy);
        // && Driver.FindElements(TopPositionsRowsBy).Any(e => e.Displayed); DONT HAVE THESE IN CURRENT TEST DATA

        public bool IsTopMoversVisible()
            => IsVisible(TopMoversPanelBy);
        // && Driver.FindElements(TopMoversRowsBy).Any(e => e.Displayed); DONT HAVE THESE IN CURRENT TEST DATA

        public bool IsHoldingsVisible()
            => IsVisible(HoldingsPanelBy);
        // && Driver.FindElements(HoldingsTableHeaderCellsBy).Any(e => e.Displayed); DONT HAVE THESE IN CURRENT TEST DATA

        public int GetHoldingsRowCount()
            => Driver.FindElements(HoldingsTableRowsBy).Count(r => r.Displayed);

        public bool HasHoldingsSearch()
            => Driver.FindElements(HoldingsSearchInputBy).Any(e => e.Displayed && e.Enabled);

        public bool HasHoldingsSorting()
            => Driver.FindElements(HoldingsSortSelectBy).Any(e => e.Displayed && e.Enabled);

        public string GetHoldingsSearchText()
        {
            var input = Driver.FindElements(HoldingsSearchInputBy).FirstOrDefault(e => e.Displayed);
            if (input == null) return string.Empty;
            return input.GetAttribute("value") ?? string.Empty;
        }

        public void SetHoldingsSearchText(string text)
        {
            var input = Wait.Until(d => d.FindElements(HoldingsSearchInputBy).FirstOrDefault(e => e.Displayed && e.Enabled));
            if (input == null)
                throw new NoSuchElementException("Holdings search input was not found or not interactable.");

            input.Clear();
            input.SendKeys(text);
        }

        public void ClearHoldingsSearch()
        {
            var input = Driver.FindElements(HoldingsSearchInputBy).FirstOrDefault(e => e.Displayed && e.Enabled);
            if (input == null) return;

            input.Clear();
        }

        /// <summary>
        /// Attempts to click the first visible sortable header and detect a sort state change.
        ///
        /// Supported detection mechanisms:
        /// - aria-sort attribute changes (preferred when present)
        /// - class attribute changes (fallback)
        ///
        /// Returns false when no headers are found or no observable change is detected.
        /// </summary>
        public bool TryToggleFirstSortableHoldingsColumn(int seconds = 5)
        {
            var header = Driver.FindElements(HoldingsSortableHeadersBy).FirstOrDefault(h => h.Displayed);
            if (header == null) return false;

            var ariaBefore = header.GetAttribute("aria-sort") ?? string.Empty;
            var classBefore = header.GetAttribute("class") ?? string.Empty;

            // Some tables place the clickable element inside the <th>. Prefer a button if present.
            var clickable = header.FindElements(By.CssSelector("button, a")).FirstOrDefault(e => e.Displayed && e.Enabled) ?? header;

            Wait.Until(ExpectedConditions.ElementToBeClickable(clickable)).Click();

            var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(seconds));
            return wait.Until(_ =>
            {
                var ariaAfter = header.GetAttribute("aria-sort") ?? string.Empty;
                var classAfter = header.GetAttribute("class") ?? string.Empty;

                // If aria-sort exists, it should change from empty/none->ascending/descending, etc.
                if (!string.Equals(ariaBefore, ariaAfter, StringComparison.Ordinal))
                    return true;

                // Otherwise, detect a class toggle.
                if (!string.Equals(classBefore, classAfter, StringComparison.Ordinal))
                    return true;

                return false;
            });
        }
        public void RefreshPortfolioData()
        {
            var btn = Driver.FindElements(RefreshPortfolioButtonBy).FirstOrDefault(e => e.Displayed && e.Enabled);
            if (btn != null) btn.Click();
            else Refresh();
        }

        /// <summary>
        /// Returns a compact "snapshot" string of the Portfolio page suitable for detecting refresh-induced updates.
        ///
        /// Implementation:
        /// - Prefer the summary card container text when possible (most likely to change).
        /// - Fall back to dev-testing section text if summary cards are not easily selectable.
        ///
        /// This is not intended to be a stable UI contract; it is a pragmatic regression check.
        /// </summary>
        public string GetOverviewSnapshotText()
        {
            // Try summary cards first
            var summary = Driver.FindElements(By.CssSelector("section.summary-cards, [data-testid='portfolio-summary']"))
                .FirstOrDefault(e => e.Displayed);

            if (summary != null)
                return NormalizeSnapshot(summary.Text);

            // Fall back to dev-testing area
            var dev = Driver.FindElements(DevTestingSectionBy).FirstOrDefault(e => e.Displayed);
            return NormalizeSnapshot(dev?.Text ?? string.Empty);
        }

        /// <summary>
        /// Waits until GetOverviewSnapshotText() changes from a prior value.
        /// This is used to validate that a refresh action resulted in some visible re-render.
        /// </summary>
        public bool WaitForOverviewSnapshotToChange(string before, int seconds = 10)
        {
            var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(seconds));
            return wait.Until(_ =>
            {
                var after = GetOverviewSnapshotText();
                return !string.IsNullOrWhiteSpace(after) &&
                       !string.Equals(after, before, StringComparison.Ordinal);
            });
        }

        private static string NormalizeSnapshot(string s)
        {
            // Normalize whitespace so minor layout changes (newlines) don't cause false negatives.
            if (string.IsNullOrWhiteSpace(s)) return string.Empty;
            return string.Join(' ', s.Split(new[] { ' ', '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries)).Trim();
        }

        /// <summary>
        /// Centralized "is visible" check to keep selector-based assertions consistent.
        /// </summary>
        private bool IsVisible(By by)
        {
            return Driver.FindElements(by).Any(e =>
            {
                try { return e.Displayed; }
                catch (StaleElementReferenceException) { return false; }
            });
        }

        public void ClickCashAvailableSummaryCard()
        {
            var card = Wait.Until(ExpectedConditions.ElementToBeClickable(CashAvailableCardBy));
            card.Click();
        }

        public (bool created, string? startingBalanceUi) StartNewTrainingSessionWithUniqueBalanceTracking(
            string startDate,
            string sessionLengthToken)
        {
            // If already active, we did not create a new one in this step
            if (IsTrainingSessionActive())
                return (false, null);

            // Unique balance; keep it huge/unique so the row match is deterministic
            var cents = DateTime.UtcNow.Ticks % 100;
            var raw = 909090909m + (cents / 100m);

            var ui = raw.ToString("C2", CultureInfo.GetCultureInfo("en-US"));

            // Use existing flow, but pass the numeric string so ResolveBalance returns it unchanged
            StartNewTrainingSession(startDate, sessionLengthToken, raw.ToString(CultureInfo.InvariantCulture));

            // If creation succeeded, the active indicator should be present
            return IsTrainingSessionActive() ? (true, ui) : (false, null);
        }
    }
}
