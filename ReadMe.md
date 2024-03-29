# Diff All Files Visual Studio Extension Description

A Visual Studio extension to quickly and easily compare all files in TFS (a changeset, shelveset, or files with pending changes) or Git (a commit or staged/unstaged files) to see what was changed.
Instead of clicking on each file individually and choosing to compare it with a different version, just click one button to compare all of the files.

This extension adds a new control into the Team Explorer pane wherever Visual Studio shows what files have changes.

Download this extension for [Visual Studio 2022][DiffAllFilesVs2022MarketplaceUrl], [2019][DiffAllFilesVs2019MarketplaceUrl], [2017][DiffAllFilesVs2017MarketplaceUrl], [2015][DiffAllFilesVs2015MarketplaceUrl], [2013][DiffAllFilesVs2013MarketplaceUrl], or [2012][DiffAllFilesVs2012MarketplaceUrl] from the VS Extension Gallery.

## Features

- Compare files one at a time, many files at a time, or with all files combined in a single file.
- Uses the same diff (i.e. compare) tool that you have configured in your .gitconfig (for Git) and Visual Studio (for TFVC). e.g. KDiff, Beyond Compare, Visual Studio, etc.
- Settings to exclude comparing files with specific extensions, or files that have been added or deleted from source control.
- Button to quickly close all diff tool windows that have been opened.
- Remaining files to be compared will open automatically when current file diff windows are closed.
- Specify the file versions to compare against (i.e. Unmodified, Workspace, Previous, Latest).
- Supports both TFVC and Git<sup>1</sup> source control providers.

### <sup>1</sup>Git Support

This extension only supports Git in Visual Studio 2013, 2015, 2017, and 2019 prior to v16.6.

Visual Studio 2012 did not support Git, and unfortunately in the Visual Studio 2019 v16.6 update the Git experience was moved to a new user interface which does not expose extensibility points.

Please [go up-vote this feature request](https://developercommunity2.visualstudio.com/t/Add-extensibility-points-to-the-new-inte/1274020) to have Microsoft add Git extensibility points to new versions of Visual Studio to allow this extension to work with Git again.

### Settings

You may edit the extension's settings from the `Tools -> Options... -> Diff All Files` settings page.

## Screenshots

Before comparing files (left) and while comparing files (right):

![Diff All Files section before doing a compare][DiffAllFilesBeforeCompareImage] ![Diff All Files section while comparing files][DiffAllFilesComparingImage]

Settings Screen (available in Visual Studio from `Tools -> Options... -> Diff All Files`):

![Diff All Files Settings page][DiffAllFilesSettingsImage]

## Contributing

Pull requests are welcome and appreciated. You may find [the contributing docs][DiffAllFilesContributingPage] helpful to get you up and running.

## Changelogs

See what's changed and when in each version of the extension in [the Changelog](Changelog.md).

## Donate

Buy me a toque for providing this extension open source and for free :)

[![paypal](https://www.paypalobjects.com/en_US/i/btn/btn_donateCC_LG.gif)](https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=9XKSDTCURT24J)

<!-- Links -->
[DiffAllFilesContributingPage]: docs/Contributing.md
[DiffAllFilesVs2012MarketplaceUrl]: https://marketplace.visualstudio.com/items?itemName=deadlydog.DiffAllFilesforVS2012
[DiffAllFilesVs2013MarketplaceUrl]: https://marketplace.visualstudio.com/items?itemName=deadlydog.DiffAllFilesforVS2013
[DiffAllFilesVs2015MarketplaceUrl]: https://marketplace.visualstudio.com/items?itemName=deadlydog.DiffAllFilesforVS2015
[DiffAllFilesVs2017MarketplaceUrl]: https://marketplace.visualstudio.com/items?itemName=deadlydog.DiffAllFilesforVS2017
[DiffAllFilesVs2019MarketplaceUrl]: https://marketplace.visualstudio.com/items?itemName=deadlydog.DiffAllFilesforVS2019
[DiffAllFilesVs2022MarketplaceUrl]: https://marketplace.visualstudio.com/items?itemName=deadlydog.DiffAllFilesforVS2022
[DiffAllFilesBeforeCompareImage]: https://github.com/deadlydog/VS.DiffAllFiles/blob/master/docs/images/Diff%20All%20Files%20Before%20Compare.png
[DiffAllFilesComparingImage]: https://github.com/deadlydog/VS.DiffAllFiles/blob/master/docs/images/Diff%20All%20Files%20Comparing.png
[DiffAllFilesSettingsImage]: https://github.com/deadlydog/VS.DiffAllFiles/blob/master/docs/images/Diff%20All%20Files%20Settings.png
