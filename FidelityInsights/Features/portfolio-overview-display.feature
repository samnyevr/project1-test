@ui @portfolio
Feature: Portfolio overview displays key information for an active session
  When a training session is active, the Portfolio page shows summary cards, charts, and holdings information.

  Background:
    Given the user is on the Portfolio page
    And an active training session exists
    @Ignore
  @smoke
  Scenario: Portfolio overview is displayed for an active session
    Then the Portfolio should display the total value summary
    And the Portfolio should display the cash available summary
    And the Portfolio should display the unrealized performance summary
    And the Portfolio should display asset allocation information
    And the Portfolio should display portfolio value over time
    And the Portfolio should display top positions
    And the Portfolio should display top movers
    And the Portfolio should display holdings
    @Ignore
  @regression
  Scenario: Portfolio does not show the "no active session" prompt when a session is active
    Then the Portfolio page should not indicate that no training session is active
