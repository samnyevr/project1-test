@ui @portfolio @trainingSession
Feature: Manage training sessions from Portfolio
  Users must be able to start a training session from the Portfolio page before viewing performance insights.
  When a session is created, the Portfolio page should reflect that an active session exists.

  Background:
    Given the user is on the Portfolio page
    And no active training session exists

  @smoke
  Scenario: Create a new training session when none exists
    When the user creates a new training session with:
      | start date     | session length | starting balance |
      | a valid start  | a valid length | a valid balance  |
    Then a training session should be active
    And the Portfolio page should reflect the active training session

  @regression
  Scenario: Canceling training session creation does not start a session
    When the user starts the training session creation process
    And the user cancels the training session creation
    Then no training session should be active
    And the Portfolio page should indicate that no training session is active

  @validation @regression
  Scenario Outline: Training session cannot be created with invalid inputs
    When the user attempts to create a new training session with:
      | start date    | session length  | starting balance |
      | a valid start | a valid length  | <balance>        |
    Then the training session should not be created
    And the user should be informed of the input problem

    Examples:
      | balance  |
      | negative |
      | zero     |

  @regression
  Scenario: Active training session persists after refresh
    Given an active training session exists
    When the user refreshes the Portfolio page
    Then a training session should still be active
    And the Portfolio page should reflect the active training session
