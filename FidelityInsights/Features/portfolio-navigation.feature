@ui @portfolio @navigation
Feature: Navigate from Portfolio to other areas
  Users can navigate to related pages from the Portfolio page.

  Background:
    Given the user is on the Portfolio page
    And an active training session exists
    @Ignore
  @smoke
  Scenario: Navigate to Trade Stocks from Cash Available
    When the user selects the Cash Available summary card
    Then the user should be navigated to the Trade Stocks page
    @Ignore
  @regression
  Scenario: Navigate to Trade Stocks using primary navigation
    When the user navigates to the Trade Stocks page from the header navigation
    Then the user should be on the Trade Stocks page
    @Ignore
  @regression
  Scenario: Returning to Portfolio from another page preserves the active session
    When the user navigates to the Trade Stocks page from the header navigation
    And the user returns to the Portfolio page
    Then a training session should still be active
    And the Portfolio page should reflect the active training session
