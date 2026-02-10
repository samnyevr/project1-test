hi im an empty project README file don't mind me c:

please look at the suggested project structure present currently

below is an idea of project structure:

root/
  README.md
  .gitignore
  .editorconfig

  docs/
    resources.md
    test-strategy.md
    environments.md ??
    running-locally.md ??

  specs/                           # Gherkin-first (no code required)
    features/
      - our work from individual branches goes here
      - we merge to main for approval

  test-automation/                 # created AFTER approval
      - idk what this will look like, something incorporating Reqnroll + Selenium + NUnit
      Api. Tests/
      Unit.Tests/
      Perf.Tests/
