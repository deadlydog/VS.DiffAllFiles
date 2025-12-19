# How To Debug VSIX Projects To Test Changes

The VSIX projects (e.g. VS.DiffAllFiles.VS2026) are actually just regular class library projects, so by default you can't just set them as the `Startup Project` and run them; you will first need to:

1. Go into the project properties and open the `Debug` tab.
1. Change the `Start action` to `Start external program` and point it to the Visual Studio executable.

    e.g. C:\Program Files (x86)\Microsoft Visual Studio\18\Enterprise\Common7\IDE\devenv.exe

1. In the `Start options` `Command line arguments` add `/rootsuffix Exp`.
1. Hit the `Save All` button to save the changes.

These settings are saved in a temporary file that does not get committed to source control, so you may need to do this whenever cloning or cleaning the git repository.

Once you've made the above changes, you can set the desired VSIX project as the `Startup Project` and hit F5 to launch a new instance of Visual Studio with the extension installed in the experimental instance.

If you haven't already, you will need to unload all other `VS.DiffAllFiles.VS20**` projects except for the one you are trying to debug, otherwise Visual Studio will try to build all of them and fail because it can't find the version-specific references for the other versions of Visual Studio.

## Potential errors

You may get an error like the following when launching the experimental instance of Visual Studio:

> The 'VS_DiffAllFiles_VS2015Package' package did not load correctly...

It should include a file path to the ActivityLog.xml file that contains more details about the error.
If the root error is something like:

> CreateInstance failed for package [VS_DiffAllFiles_VS2015Package]Source: &apos;mscorlib&apos; Description: Could not load file or assembly &apos;Microsoft.VisualStudio.Shell.14.0, Version=14.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a&apos; or one of its dependencies. The system cannot find the file specified.

You can likely ignore it.
Just ensure that when you later build the extension in release mode and apply it to your main instance of Visual Studio, that it works as expected.
