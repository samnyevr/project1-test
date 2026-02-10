@trading-session
Feature: Trading - Buy Stock
    As a trader
    I want to be able to buy during a trading session

      Background:
        Given a new funded trading session is started

    Scenario: Buy a stock successfully
        Given the trader select a stock with ticker "AAL"
        When the trader input a Quantity of "100" shares
        And the trader click on the Buy button
        Then the "100" "AAL" shares should be added to Your Holdings section
        And the available balance should be reduced accordingly
        And the trader is shown a confirmation message "Successfully bought 100 shares of AAL at $5.00"

    Scenario: Buy a stock successfully with Maximum Quantity
        Given the trader select a stock with ticker "AAL"
        When the trader click on the Max button
        And the trader click on the Buy button
        Then the maximum available "AAL" shares should be added to Your Holdings section
        And the available balance should be reduced accordingly
        And the trader is shown a confirmation message "Successfully bought 181818181 shares of AAL at $5.00"

    Scenario: Buy a stock without available balance
        Given the trader has an available balance of "$6.00"
        And the trader select a stock with ticker "AAL" in max
        When the trader input a Quantity of "10" shares
        Then the Buy button should be disabled
        And the trader is shown an error message "Insufficient funds for this purchase."

    Scenario: Buy a stock with quick buy functionality
        Given the trader click on "AAL" in Your Holdings section
        When the trader click on the Buy Toggle button
        Then the trader can input the desired Quantity of shares
        And the trader can click on the Buy button