# Visual Studio 2019 Extension Changelog

## v1.0.4 - May 26, 2019

Fixes:

- Re-enable asynchronous loading on background thread, as VS 2019 v16.1+ now disables all synchronous extensions by default.
  This re-introduces [the issue where the control shows an error instead of loading properly if it's visible right away at Visual Studio startup](https://github.com/deadlydog/VS.DiffAllFiles/issues/27), but at least the extension will be usable again, and that error has an easy work around of just going to the Home tab in Team Explorer and then back to the Changes tab, until it can be fixed properly.

## v1.0.3 - May 24, 2019

Fixes:

- Allow extension to be installed on all versions of Visual Studio 2019 (e.g. v16.x), not just v16.0.

## v1.0.2 - April 12, 2019

Fixes:

- Remove asynchronous loading on background thread, as it would cause the UI to throw errors if the controls were visible right away at Visual Studio startup.

## v1.0.1 - April 7, 2019

Fixes:

- Remove unnecessary dependencies preventing installation on some systems.

## v1.0.0 - April 7, 2019

Initial release of VS 2019 version of extension.
