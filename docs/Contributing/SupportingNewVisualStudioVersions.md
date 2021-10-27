# Supporting New Visual Studio Versions

Here are the steps taken to add Visual Studio 2019 support to Diff All Files. For versions newer than
2022, use the 2022 project as a source to copy from:

1. Make sure there are no pending changes, and do a `git clean -xfd` to remove all temp files.
1. Copy the `VS.DiffAllFiles.VS2017` project directory, and name it `VS.DiffAllFiles.VS2019`.
1. Change all file names with `2017` to `2019`.
1. Add the new 2019 project to the solution in Visual Studio, and let VS upgrade it if necessary.
1. Remove the non-existent `VS.DiffAllFiles.VS2017Package.cs` file from the project and add the renamed `VS.DiffAllFiles.VS2019Package.cs` file to the project.
1. In the new 2019 project, upgrade the Microsoft.VSSDK.BuildTools NuGet package from v15 to v16.
1. Clean the solution to remove all temp files (manually delete the `bin` and `obj` directories if necessary).
1. Grep the 2019 project directory and change `2017` to `2019` where needed.
   - When changing properties of the .csproj file, be sure to update both the `Debug` and `Release` configuration properties.
1. Add a new Guid to the `Guids.cs` file.
1. Replace the .dll files in the 2019 project's `VersionSpecificReferences` directory with the Visual Studio 2019 version of the assemblies.
   - The assemblies were all found in `C:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\Common7\IDE\CommonExtensions\Microsoft\TeamFoundation\Team Explorer`.
1. Remove the `VersionSpecificReferences` references from the 2019 project (this may also delete the files from disk) and re-add them in Visual Studio so that the .csproj file is updated appropriately.
1. Update the 2019 project's .vsixmanifest file as necessary.
   - Reset the version number.
   - Change the Guid to match the new Guid added to the `Guids.cs` file earlier above.
   - Change any other basic information (Product name, description, tags, website URLs, etc.)
   - Change the Install Targets to target the new version of Visual Studio.
   - Change the Prerequisites to target the new version of Visual Studio.
   - Change any other information needed to support the new .vsixmanifest requirements.
1. Starting with Visual Studio 2017, the only other reference for Visual Studio libraries should be the nuget package `Microsoft.VisualStudio.Sdk` which will need to be updated to the correct version for the new Visual Studio.
1. If there are breaking Visual Studio library changes, you will need to update the shared code appropriately. Each head VSIX project has a preprocessor variable defined for the Visual Studio version. VS2013-2019 additionally have defined `SUPPORTS_GIT_CONTROLS_EXTENSIBILITY` which enables Git support for the Team Explorer window. This was added in VS2013, deprecated in a VS2019 point release, and fully removed in VS2022 (in favor of `Git Changes` window).

That should be it (hopefully). Now just [test that everything still works][ThingsToTestAfterMakingChanges].

[ThingsToTestAfterMakingChanges]: ThingsToTestAfterMakingChanges.md