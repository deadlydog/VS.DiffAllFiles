# Publishing a new version

## Things to do before publishing a new version

- Download a backup of the VSIX file from the marketplace.
We want to do this in case there's an undiscovered issue with the new version and we need to rollback to the previous version.
The marketplace does not provide an easy way to rollback to a previous version, and the complicated build setup of the project can make it hard to reproduce an old VSIX file, even when we are able to rollback to its commit.
- Before publishing a new version to the marketplace, manually uninstall the old version and install the new version using the .vsix file, just to be sure it does work as expected.
- [Test all of the functionality][ThingsToTestAfterMakingChanges] to ensure nothing is broken.

[ThingsToTestAfterMakingChanges]: ThingsToTestAfterMakingChanges.md