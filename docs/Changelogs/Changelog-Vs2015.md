# Visual Studio 2015 Extension Changelog

## v1.2.2 - June 26, 2017

Fixes:

- Fix bug where external Git diff tool cannot be launched when the path in the .gitconfig file is enclosed in single quotes.

## v1.2.1 - June 19, 2017

Enhancements:

- Changed settings screen to separate the Diff Tool configuration from other settings.

Fixes:

- Reference newer version of LibGit2Sharp library to fix file path bug causing extension to not load properly on some Git repositories.

## v1.2.0 - July 23, 2016

Enhancements:

- Added support for Git, so extension now shows in the Git Changes and Git Commit Details windows.

## v1.1.0 - July 7, 2016

Enhancements:

- Changed settings to allow up to 9999 files to be diff'd at a time, instead of just 99.

## v1.0.2 - July 1, 2016

Fixes:

- Attempting to fix bug where the extension will not load on some user's computers due to it being unable to load a specific version of Microsoft.TeamFoundation.*.dll files.

## v1.0.1 - August 16, 2015

Fixes:

- Fixed bug where file name's extension's casing could affect if Diff All Files was able to close an external diff viewer window or not.

## v1.0.0 - August 14, 2015

- Initial release.
- Known bug where the "Close All" button does not close external diff application, but wanted to get the VS 2015 version released asap.
