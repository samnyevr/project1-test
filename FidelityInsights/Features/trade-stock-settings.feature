Feature: Trade Stock Settings
  As a trader
  I want to be able to start a new trading session, reset training session, or end training session


    Scenario: Reset Training Session
        Given the trader is on the Training Session page under the Trade Stock section
        When the trader clicks on the Gear Icon to open Settings
        And the trader selects the Reset Training Session option
        Then the trader's available balance should reset to the initial amount
        And the trader's holdings should be cleared

    Scenario: End Training Session
        Given the trader is on the Training Session page under the Trade Stock section
        When the trader clicks on the Gear Icon to open Settings
        And the trader selects the End Training Session option
        Then the trader should be shown a Simulation Summary
        And a summary of the training session should be displayed
    
    Scenario: Start New Trading Session
        Given the trader is on the Trade Stock section
        When the trader clicks on the Gear Icon to open Settings
        And the trader selects the Start New Trading Session option
        Then a popup with Trading Session Settings should appear
        And the trader should be able to configure settings for the new trading session