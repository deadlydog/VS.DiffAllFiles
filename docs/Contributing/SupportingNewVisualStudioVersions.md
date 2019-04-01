# Supporting New Visual Studio Versions

Here are the steps taken to add Visual Studio 2017 support to Diff All Files:

1. Copy the `VS.DiffAllFiles.VS2015` project directory, and name it `VS.DiffAllFiles.VS2017`.
1. Change all file names with `2015` to `2017`.
1. Add the new 2017 project to the solution in Visual Studio, and let VS upgrade it.
1. Remove the v14 Microsoft.VSSDK.BuildTools NuGet package, and then add the v15 NuGet package.
1. Grep the 2017 project directory and change `2015` to `2017` where needed.
1. Add a new Guid to the `Guids.cs` file.
1. Replace the .dll files in the 2017 project's `VersionSpecificReferences` directory with the Visual Studio 2017 version of the assemblies.
1. Remove the `VersionSpecificReferences` references from the 2017 project and re-add them in Visual Studio so that the .csproj file is updated appropriately.
1. Update the 2017 project's .vsixmanifest file as necessary.
   * Reset the version number.
   * Change the Guid to match the new Guid added to the `Guids.cs` file earlier above.
   * Change any other basic information (description, tags, website URLs, etc.)
   * Change the Install Targets to target the new version of Visual Studio.
   * Change any other information needed to support the new .vsixmanifest requirements.
1. Make sure the button in the Settings window to open the TF Diff Tool Configuration window still works (i.e. that the location of where we expect to find TF.exe hasn't changed).
1. Test diffing both TFVC and Git changes to make sure they work as expected, and fix any bugs that may have arisen.