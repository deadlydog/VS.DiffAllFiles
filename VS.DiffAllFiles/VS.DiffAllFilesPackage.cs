using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using VS_DiffAllFiles.Settings;
using Microsoft.VisualStudio.Shell;
using VS_DiffAllFiles.StructuresAndEnums;

namespace VS_DiffAllFiles
{
	/// <summary>
	/// This is the class that implements the package exposed by this assembly.
	///
	/// The minimum requirement for a class to be considered a valid package for Visual Studio
	/// is to implement the IVsPackage interface and register itself with the shell.
	/// This package uses the helper classes defined inside the Managed Package Framework (MPF)
	/// to do it: it derives from the Package class that provides the implementation of the 
	/// IVsPackage interface and uses the registration attributes defined in the framework to 
	/// register itself and its components with the shell.
	/// </summary>
	// This attribute is used to register the information needed to show this package in the Help/About dialog of Visual Studio.
	[InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
	// Add our Diff All Files section to the Tools -> Options menu.
	[ProvideOptionPage(typeof(DiffAllFilesSettings), "Diff All Files", "General", 0, 0, true)]
	[ProvideOptionPage(typeof(ConfigureDiffToolSettingsPage), "Diff All Files", "Diff Tool", 0, 0, true)]
	// Include other assemblies in the same directory as this package that this package depends on (e.g. WPF Extended Toolkit, QuickConverter, etc.).
	[ProvideBindingPath]
// VS 2012 and 2013 don't support the AsyncPackage base class, as that's only supported for VS 2015+.
#if (VS2012 || VS2013)
	// This attribute tells the PkgDef creation utility (CreatePkgDef.exe) that this class is a package.
	[PackageRegistration(UseManagedResourcesOnly = true)]
	// Auto Load our assembly even when no solution is open (by using the Microsoft.VisualStudio.VSConstants.UICONTEXT_NoSolution guid).
	[ProvideAutoLoad("ADFC4E64-0397-11D1-9F4E-00A0C911004F")]
	public abstract class VS_DiffAllFilesPackage : Package
#else
	// Async Package info: https://docs.microsoft.com/en-us/visualstudio/extensibility/how-to-use-asyncpackage-to-load-vspackages-in-the-background?view=vs-2019
	[PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
	[ProvideAutoLoad(Microsoft.VisualStudio.Shell.Interop.UIContextGuids.NoSolution, PackageAutoLoadFlags.BackgroundLoad)]
	public abstract class VS_DiffAllFilesPackage : AsyncPackage
#endif
	{
        /// <summary>
        /// Default constructor of the package.
        /// Inside this method you can place any initialization code that does not require 
        /// any Visual Studio service because at this point the package object is created but 
        /// not sited yet inside Visual Studio environment. The place to do all the other 
        /// initialization is the Initialize method.
        /// </summary>
        protected VS_DiffAllFilesPackage()
        {
            Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", this.ToString()));

			// Setup Quick Converter.
			// Add the System namespace so we can use primitive types (i.e. int, etc.).
			QuickConverter.EquationTokenizer.AddNamespace(typeof(object));
			// Add the System.Windows namespace so we can use Visibility.Collapsed, etc.
			QuickConverter.EquationTokenizer.AddNamespace(typeof(System.Windows.Visibility));
			// Add the Diff All Files Base Classes so we can use our custom enums.
	        QuickConverter.EquationTokenizer.AddNamespace(typeof(CompareModes));
        }

		/// <summary>
		/// Initialization of the package; this method is called right after the package is sited, so this is the place
		/// where you can put all the initialization code that rely on services provided by VisualStudio.
		/// </summary>
// VS 2012 and 2013 don't support the AsyncPackage base class, as that's only supported for VS 2015+.
#if (VS2012 || VS2013)
        protected override void Initialize()
        {
            base.Initialize();
			InitializeSettings();
        }
#else
		protected override async System.Threading.Tasks.Task InitializeAsync(System.Threading.CancellationToken cancellationToken, System.IProgress<ServiceProgressData> progress)
		{
			await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
			InitializeSettings();
		}
#endif
		private void InitializeSettings()
		{
			// Load the current settings for this package and save a handle to them.
			DiffAllFilesSettings.CurrentSettings = GetDialogPage(typeof(DiffAllFilesSettings)) as DiffAllFilesSettings;
		}
	}
}
