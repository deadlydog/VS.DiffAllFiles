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
