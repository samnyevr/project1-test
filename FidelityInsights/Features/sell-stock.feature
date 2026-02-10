@trading-session
Feature: Trading - Sell Stock
  As a trader
  I want to be able to sell during a trading session

    Scenario: Sell a stock Successfully
        Given the trader selects "AAL" for selling
        When the trader inputs a sell Quantity of "50" shares
        And the trader click on the Sell button
        Then the "50" "AAL" shares should be removed from Your Holdings section
        And the available balance should be increased accordingly
        And the trader is shown a sold confirmation message "Successfully sold 50 shares of AAL at $6.00"

    Scenario: Sell a stock successfully with Maximum Quantity
        Given the trader selects "AAL" for selling
        When the trader click on the Max button on the Sell Stock page
        And the trader click on the Sell button
        Then all available "AAL" shares should be removed from Your Holdings section
        And the available balance should be increased accordingly
        And the trader is shown a sold confirmation message "Successfully sold 151515151 shares of AAL at $6.00"

    Scenario: Sell a stock without sufficient shares
        Given the trader holds "20" shares of "AAL"
        And after owning only 20 shares the trader selects "AAL" for selling
        When after owning only 20 shares the trader inputs a sell Quantity of "30" shares 
        Then the Sell button should be disabled
        And the trader is shown an sold error message "You don't own enough shares to sell."

    Scenario: Sell a stock with no shares owned
        Given the trader holds no shares of "BCC"
        And the trader selects "BCC" for selling
        When the trader inputs a sell Quantity of "10" shares
        Then the Sell button should be disabled
        And the trader is shown an sold error message "You don't own enough shares to sell."

    Scenario: Sell a stock with quick sell functionality
        Given the trader who is trying to sell click on "AAL" in Your Holdings section
        When all available "AAL" shares should be shown in the Quantity input field
        Then the trader can click on the Sell button