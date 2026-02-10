Feature: Portfolio Smoke Test
  A minimal test to verify navigation and refresh actions on the Portfolio page.

  Scenario: Open and refresh the Portfolio page
    Given the user is on the Portfolio page
    When the user refreshes the Portfolio page

  Scenario: Create session and teardown deletes it
    Given the user is on the Portfolio page
    And an active training session exists
    When the user refreshes the Portfolio page