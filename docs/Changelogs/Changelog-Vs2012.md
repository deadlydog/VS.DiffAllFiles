# Visual Studio 2012 Extension Changelog

## v2.3 - August 17, 2015

Enhancements:

- Temporary files created for the purpose of diffing (combined files, files from a previous checkin, etc.) are now marked read-only, since changes to them would not be reflected back anywhere meaningful.

Fixes:

- Fixed bug where file name's extension's casing could affect if Diff All Files was able to close an external diff viewer window or not.

## v2.2.1 - May 24, 2014

Enhancements:

- Added more default binary file types to ignore.

Fixes:

- Fixed bug where files were not compared properly when using Combined mode.

## v2.2 - May 23, 2014

Enhancements:

- Added more default binary file types to ignore.
- Sped up how fast files get stitched together in Combined mode.

Fixes:

- Fixed UI style to match the current Visual Studio theme (e.g. Dark vs. Light)
- Filtered out directories so they don't get compared, as it would throw an error before.

## v2.1 - April 17, 2014

Enhancements:

- Added Combined compare mode to view all file changes in a single diff window.
- Cancellation support for when building Combined files and launching diff windows.

Fixes:

- Fixed labels shown on the VS diff tool compare tabs.

BREAKING CHANGES:

- This update will reset the "Maximum number of files to compare at a time" in the Settings window back to its default value.

## v2.0 - April 12, 2014

Enhancements:

- Updated to support TFS Changeset Details and Shelveset Details windows.
- Implemented new strategy for getting files to compare that is more accurate and flexible.
- UI updates.

## v1.0 - March 13, 2014

- Initial release.
