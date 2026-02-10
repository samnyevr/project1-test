Feature: Application theming
  As a user
  I want to switch between light and dark mode
  So that the application matches my visual preference

  Scenario: Toggle light and dark mode on Settings
    Given the application is loaded, on Settings page
    When I toggle the theme setting
    Then the application switches between light and dark mode

  Scenario: Toggle light and dark mode on Portfolio Overview
    Given the application is loaded and a training session is loaded, on Portfolio Overview page
    When I toggle the theme setting
    Then the application switches between light and dark mode

  Scenario: Theme persists during session
    Given I have selected a theme
    When I navigate between pages
    Then the selected theme remains active

  # scenario for primary / sell / buy / volume chart colors, may be unimplemented

