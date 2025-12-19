# Supporting New Visual Studio Versions

Below are the steps taken to add Visual Studio 2026 (v18) support to Diff All Files.
Use these as an example of how to add support for future versions of Visual Studio.

1. Make sure there are no pending changes, and do a `git clean -xfd` to remove all temp files.
1. Copy the `VS.DiffAllFiles.VS2022` project directory, and name it `VS.DiffAllFiles.VS2026`.
1. Change all file names with `2022` to `2026`.
1. Grep the new `VS.DiffAllFiles.VS2026` project directory and rename all instances of `2022` to `2026`.
1. Open the solution in the new version of Visual Studio (e.g. 2026).
1. Add the new 2026 project to the solution in Visual Studio, and let VS upgrade it if necessary.
1. Add a new const string to the `Guids.cs` file for the new version, and generate a new unique Guid for it.
   - The new string name should match the name used in the `VS.DiffAllFiles.VS2026Package.cs` file's `Guid` attribute.
1. In the new 2026 project, upgrade the `Microsoft.VSSDK.BuildTools` and `Microsoft.VisualStudio.Sdk` NuGet packages to the newest stable versions.
1. Replace the .dll files in the 2026 project's `VersionSpecificReferences` directory with the Visual Studio 2026 version of the assemblies.
   - The assemblies were all found in `C:\Program Files (x86)\Microsoft Visual Studio\18\Enterprise\Common7\IDE\CommonExtensions\Microsoft\TeamFoundation\Team Explorer`.
1. Remove the `VersionSpecificReferences` references from the 2026 project (this may also delete the files from disk) and re-add them in Visual Studio so that the .csproj file is updated appropriately.
1. Update the 2026 project's .vsixmanifest file as necessary.
   - Reset the version number to `1.0.0`.
   - Change the `Product ID` Guid to match the new Guid added to the `Guids.cs` file earlier above.
   - Change any other basic information (Product name, description, tags, website URLs, etc.)
   - Change the Install Targets to target the new version of Visual Studio.
   e.g. change `[17.0, 18.0)` to `[18.0, 19.0)`.]
   - Change the Prerequisites to target the new version of Visual Studio.
   - Change any other information needed to support the new .vsixmanifest requirements.
1. Starting with Visual Studio 2017, the only other reference for Visual Studio libraries should be the nuget package `Microsoft.VisualStudio.Sdk`, which we updated above to the correct version for the new Visual Studio.
1. If there are breaking Visual Studio library changes, you will need to update the shared code appropriately.
   Each head VSIX project has a preprocessor variable defined for the Visual Studio version.
   e.g. `VS2026` for Visual Studio 2026.
   VS2013-2019 additionally have defined `SUPPORTS_GIT_CONTROLS_EXTENSIBILITY` which enables Git support for the Team Explorer window.
   This was added in VS2013, deprecated in a VS2019 patch release, and fully removed in VS2022 (in favor of `Git Changes` window).

That should be it (hopefully).
Now just [test that everything still works](./ThingsToTestAfterMakingChanges.md).
