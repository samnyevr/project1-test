Feature: Training Session Actions
    As a User
    I want to resume or delete simulations
    So that I can manage my active workspace
    
#Background:
#    Given I am on the "Training Session History" page

    @Ignore
Scenario: Resume a Paused Session
    # This tests the 'Set Active' button inside the meatball menu
    Given I have a paused session with ID "101"
    When I click the "Menu" button for session "101"
    And I click the "Set Active" option
    Then I should be redirected to the "Portfolio" page
    # Or, if it stays on page:
    # Then the status pill for session "101" should become "ACTIVE"

    @Ignore
Scenario: Delete a Session
    # Tests deleteSimulation() in simulation-history.ts
    Given I have a session named "Mistake Run"
    When I click the "Menu" button for "Mistake Run"
    And I click "Delete"
    Then the session "Mistake Run" should be removed from the table
    And I should see a toast message or confirmation

    @Ignore
Scenario: View Portfolio of a Completed Session
    # Tests viewPortfolio() - Requirement 4.2 (Read Only)
    Given I have a completed session "2012 Run"
    When I click the "Menu" button for "2012 Run"
    And I click "View Portfolio"
    Then I should be navigated to the Portfolio View
    But I should not be able to execute new trades