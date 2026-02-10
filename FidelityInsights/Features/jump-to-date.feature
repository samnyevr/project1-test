@trading-session
Feature: Jump to Date
  As a trader
  I want to be able to jump to a specific date in the trading session

    @Ignore
    Scenario: Jump to a specific date
        Given the trader is on the Training Session page under the Trade Stock section
        When the trader clicks on the Jump To Date button
        And the trader selects the date "2010-12-31" from the date picker
        Then the trader can see the trading session date has changed to "2010-12-31"
        And the trader can see the Since 14 days ago popup
        And the trader can see Portfolio value change from 14 days ago
        And the trader can see number of Holdings
        And the trader can see the date has advanced to "2010-12-31"