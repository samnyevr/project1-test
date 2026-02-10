@trading-session
Feature: End Day Functionality
    As a trader
    I want to be able to view the next day after the market closes

    @Ignore
    Scenario: View next day after market close
        Given the trader is on the Training Session page under the Trade Stock section
        When the trader clicks on the End Day button
        Then the trader can see Since Yeterday popup
        And the trader can see Porfolio value change from previous day
        And the trader can see number of Holdings
        And the trader can see the the date has advanced by one day