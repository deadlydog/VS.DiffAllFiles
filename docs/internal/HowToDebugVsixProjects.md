# How To Debug VSIX Projects To Test Changes

The VSIX projects (e.g. VS.DiffAllFiles.VS2017) are actually just regular class library projects, so by default you can't just set them as the `Startup Project` and run them; you will first need to:

1. Go into the project properties and open the `Debug` tab.
1. Change the `Start action` to `Start external program` and point it to the Visual Studio executable.

    e.g. C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\Common7\IDE\devenv.exe

1. In the `Start options` `Command line arguments` add `/rootsuffix Exp`.
1. Hit the `Save All` button to save the changes.

These settings are saved in a temporary file that does not get committed to source control, so you will need to do this whenever cloning or cleaning the git repository.