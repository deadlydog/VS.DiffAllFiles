# Publishing a new version

The steps to publish a new version are:

1. Download a backup of the VSIX file from the marketplace.
We want to do this in case there's an undiscovered issue with the new version and we need to rollback to the previous version.
The marketplace does not provide an easy way to rollback to a previous version, and the complicated build setup of the project can make it hard to reproduce an old VSIX file, even when we are able to rollback to its commit.
1. Update the appropriate project's `source.extension.vsixmanifest` file to have a new `Version` number; follow [sematic versioning guidelines](https://semver.org/).
1. Build the project in `Release` mode to generate the new .vsix file in the project's `bin\Release` directory.
1. Manually uninstall the old version and install the new version using the .vsix file.
1. [Test all of the functionality](ThingsToTestAfterMakingChanges.md) to ensure nothing is broken.
1. If the new version is working as expected, publish the new version [to the Visual Studio extensions gallery](https://marketplace.visualstudio.com/manage/publishers/deadlydog).
