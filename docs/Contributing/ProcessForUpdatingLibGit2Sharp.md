# LibGit2Sharp version update process

To update LibGit2Sharp to a new version, follow these steps:

1. Update the LibGit2Sharp NuGet package to obtain the new version of the LibGit2Sharp.dll, and it's corresponding LibGit2Sharp.NativeBinaries NuGet package's git2-*.dll.
Use the Package Manager Console if the UI is giving you trouble.
1. Run the `StronglySignLibGit2SharpDll.cmd` batch script against the LibGit2Sharp.dll to create a signed version of the dll file with a new strong name (i.e. PublicKeyToken).
The new file will overwrite the existing LibGit2Sharp.dll file.
   - e.g. "packages\LibGit2Sharp.0.26.0\lib\netstandard2.0\LibGit2Sharp.dll"
We do this to prevent runtime conflicts with other Visual Studio extensions that may also be using a different version of this assembly.
The script paths may need to be updated.
1. Replace the file at "VS.DiffAllFiles\_LibGit2Sharp\LibGit2Sharp.dll" with the new signed version.
1. Remove the LibGit2Sharp reference from all of the VS projects, and re-add it, pointing it to the new signed version.
This will ensure the .csproj files have their hint path set properly.
1. Delete the VS.DiffAllFiles\_LibGit2Sharp\git2-*.dll file from the project and hard drive.
1. Copy the x86 version of the git2-*.dll file from the new LigGit2Sharp.NativeBinaries NuGet package to the VS.DiffAllFiles\_LibGit2Sharp directory.
   - e.g. "packages\LibGit2Sharp.NativeBinaries.2.0.267\runtimes\win-x86\native\git2-572e4d8.dll" to "VS.DiffAllFiles\_LibGit2Sharp\git2-572e4d8.dll"
1. Add the "VS.DiffAllFiles\_LibGit2Sharp\git2-*.dll" file to the VS.DiffAllFiles project, and update the _LibGit2Sharp shortcut files in the other VS projects to point to this new file.
1. In the VSIX projects, change the Visual Studio properties of the _LibGit2Sharp\git2-*.dll file to have `Include In VSIX` set to `True`, and set the `VSIX Sub Path` to `\`.
This will ensure that the git2-*.dll file required by LibGit2Sharp.dll is included in the root directory of the extension when it is installed.