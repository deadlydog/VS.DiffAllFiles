# Contributing code

## Project structure

The VS.DiffAllFiles solution is organized into a number of different projects.

- VS.DiffAllFiles - This project doesn't actually produce any used binaries, but contains all of the code that is shared with the version-specific VS20** projects.
The code from this project is added to the other projects _as links_. This allows us to only have to make code changes in a single place and have it apply to all of the other projects.
- VS.DiffAllFiles.VS2012 - This project only contains the TFVC shared code, because Visual Studio 2012 does not support Git.
This project is used to build the VSIX installer for Visual Studio 2012.
- VS.DiffAllFiles.VS20** - These projects are used to build the VSIX installer for the various versions of Visual Studio.

### Why is there a project for each version of Visual Studio

Because the extension references the `Microsoft.TeamFoundation.*` assemblies, we cannot have a single project that targets all versions of Visual Studio; when we try it results in runtime errors saying that the extension is not able to load the required version of the assemblies.

To work around this issue, we have to include the Visual Studio version-specific assemblies in each project.
These assemblies get installed with the extension to ensure that the version the extension expects to find exists at runtime.

## Building the code

The individual projects all reference their relative Visual Studio version's references.
This means that when building in one version of Visual Studio, you will often receive warnings and errors from the other Visual Studio version projects, as it is unable to find the required version of their references.
To avoid this issue, simply unload all of the other projects except for the one that you are attempting to build and run.

## Other helpful information

If you are trying to contribute code, you may find these other pages useful:

- [How to debug VSIX projects][HowToDebugVsixProjectsPage]
- [Process for updating the LibGit2Sharp version][ProcessForUpdatingLibGit2SharpPage]
- [Adding support for new versions of Visual Studio][SupportingNewVisualStudioVersionsPage]

[HowToDebugVsixProjectsPage]: Contributing/HowToDebugVsixProjects.md
[ProcessForUpdatingLibGit2SharpPage]: Contributing/ProcessForUpdatingLibGit2Sharp.md
[SupportingNewVisualStudioVersionsPage]: Contributing/SupportingNewVisualStudioVersions.md