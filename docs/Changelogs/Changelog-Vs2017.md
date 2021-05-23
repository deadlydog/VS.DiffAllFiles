# Visual Studio 2017 Extension Changelog

## v1.1.1 - April 12, 2019

Fixes:

- Remove asynchronous loading on background thread, as it would cause the UI to throw errors if the controls were visible right away at Visual Studio startup.
- Remove unnecessary dependencies preventing installation on some systems.

## v1.1.0 - April 6, 2019

Fixes:

- Load extension asynchronously as per Microsoft's recommendation for all modern extensions.

## v1.0.3 - April 2, 2019

Fixes:

- Fix bug where diff was not working properly at all.
- Fix "The given key was not present in dictionary" error.

## v1.0.2 - March 31, 2019

Fixes:

- Updated version of LibGit2Sharp used, which includes bug fixes and performance enhancements.

## v1.0.1 - June 19, 2017

Fixes:

- Fix bug where external Git diff tool cannot be launched when the path in the .gitconfig file is enclosed in single quotes.

## v1.0.0 - June 12, 2017

Initial release of VS 2017 version of extension.
