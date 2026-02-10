@ui @portfolio @rangeSelection
Feature: Select performance range on Portfolio
  Users can change the performance range to view portfolio trends over different time windows.

  Background:
    Given the user is on the Portfolio page
    And an active training session exists
    @Ignore
  @regression
  Scenario Outline: Selecting a performance range updates the Portfolio view
    When the user selects the "<range>" performance range
    Then the Portfolio should update to reflect the selected "<range>" range
    And the selected range indicator should show "<range>"

    Examples:
      | range |
      | 1M    |
      | 3M    |
      | 6M    |
      | YTD   |
    @Ignore
@regression
Scenario Outline: Changing chart range updates portfolio value over time visualization window
  When the user selects the "<range>" portfolio value over time range
  Then the portfolio value over time visualization should reflect the selected range

  Examples:
    | range |
    | 1M    |
    | 3M    |
    | 6M    |