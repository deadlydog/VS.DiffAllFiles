# Supporting New Visual Studio Versions

Here are the steps taken to add Visual Studio 2019 support to Diff All Files:

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
   - Change any other information needed to support the new .vsixmanifest requirements.
1. Remove Microsoft.VisualStudio.Shell.14.0 NuGet package from VS 2019 project and install 15.0 NuGet package, to avoid runtime error when loading Diff All Files Settings page.
1. Remove Microsoft.VisualStudio.Shell.Immutable.14.0 NuGet package from VS 2019 project to avoid type conflicts.
1. If there are breaking Visual Studio library changes, you will need to update the shared code appropriately.

That should be it (hopefully). Now just test that everything still works:

1. Make sure the button in the Settings window to open the TF Diff Tool Configuration window still works (i.e. that the location of where we expect to find TF.exe hasn't changed).
1. Test diffing both TFVC and Git changes to make sure they work as expected, and fix any bugs that may have arisen.