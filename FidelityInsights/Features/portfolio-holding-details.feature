@ui @portfolio @holdingDetails
Feature: View holding details from Portfolio
  Users can open holding details to review position and performance information for a ticker.
  CURRENTLY UNIMPLEMENTED - buying stocks from a new training session bugged at time of implementation and required to have holdings to test this feature.

  Background:
    Given the user is on the Portfolio page
    And an active training session exists

  @Ignore
  @regression
  Scenario: Viewing details for a top mover shows the holding details modal
	When the user selects a top mover from the top movers card
    Then the holding details modal should be displayed for that ticker
    And the holding details modal should show position and performance information

  @Ignore
  @regression
  Scenario: Viewing details for a holding row shows the holding details modal
	When the user selects a holding from the holdings table
    Then the holding details modal should be displayed for that ticker
    And the holding details modal should show position and performance information

  @Ignore
  @regression
  Scenario: Closing the holding details modal returns the user to the Portfolio page
    Given the holding details modal is displayed
    When the user closes the holding details modal
    Then the holding details modal should not be displayed
    And the user should remain on the Portfolio page

  @Ignore
  @regression
  Scenario: Holding details modal displays required fields
    When the user opens holding details for a ticker
    Then the holding details modal should show shares information
    And the holding details modal should show average cost information
    And the holding details modal should show last price information
    And the holding details modal should show market value information
    And the holding details modal should show gain or loss information
    And the holding details modal should show percent return information
