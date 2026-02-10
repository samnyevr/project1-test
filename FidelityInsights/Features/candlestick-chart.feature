Feature: Candlestick chart display
  As a user
  I want to view historical stock data
  So that I can analyze price trends

  Scenario: Default chart loads on market data page
    Given I open the application
    Then I see a candlestick chart for the default ticker
    And the date range is set to the ~4 year range from 2020 to 2024

  Scenario: Switch to a different ticker
    Given I am viewing the default candlestick chart
    When I select "AAPL" from the ticker dropdown
    Then the chart updates to show "AAPL" historical data

  Scenario: Change date range using calendar pickers
    Given I am viewing a candlestick chart
    When I set the "From" date to "2025-01-01"
    And I set the "To" date to "2025-02-01"
    And I click the "Load Data" button
    Then the chart updates to display data from "2025-01-01" to "2025-02-01"
    And the displayed OHLC (Open, High, Low, Close) values correspond to the selected date range

  Scenario: Invalid date range (From > To)
    Given I am viewing a candlestick chart
    When I set the "From" date to "2025-02-01"
    And I set the "To" date to "2025-01-01"
    And I click the "Load Data" button
    Then I see an error message indicating "Invalid date range"
    And the chart does not update

  Scenario: Date range outside available data
    Given I am viewing a candlestick chart
    When I set the "From" date to "1900-01-01"
    And I set the "To" date to "1910-12-31"
    And I click the "Load Data" button
    Then I see a message "No data available for selected range"
    And the chart remains unchanged

  Scenario: Save candlestick chart as a PNG image
    Given I am viewing a candlestick chart for "AAPL"
    When I click the "Save as Image" button
    Then a file named "Ticker AAPL.png" should be downloaded to my computer
    And the file should be in "PNG" format


