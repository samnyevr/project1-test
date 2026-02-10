Feature: Simulation History Management
    As a User
    I want to view and manage my past and current trading sessions
    So that I can track my performance and switch between different strategies

Background:

    Given I have navigated to the "Training Session History" page
    And I have the following existing sessions:
        | Start Date | Status    | Balance    |
        | 2010-09-01 | COMPLETED | $9,995,769.00 |
        | 2020-04-01 | PAUSED    | $7,759,540.00 |
    @Ignore
Scenario: View Paused and Completed Sessions
    Then I should see a table for "Paused Training Sessions"
    And I should see a table for "Completed Training Sessions"
    And the "2010-09-01" session should have a "COMPLETED" status pill
    And the "2020-04-01" session should have a "PAUSED" status pill
        @Ignore
Scenario: Sort Paused Sessions by Balance
    # Testing the logic in simulation-history.ts togglePausedSort()
    When I click the "Starting Balance" column header in the Paused table
    Then the rows should be sorted by Balance in "Ascending" order
    When I click the "Starting Balance" column header again
    Then the rows should be sorted by Balance in "Descending" order
    @Ignore
Scenario: Verify Gain and Loss Styling
    # Testing CSS classes .gain and .loss from simulation-history.css
    Then the session with a positive gain should display the amount in "Green"
    And the session with a negative loss should display the amount in "Red"