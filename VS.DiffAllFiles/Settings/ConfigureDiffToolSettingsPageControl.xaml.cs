using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace VS_DiffAllFiles.Settings
{
	/// <summary>
	/// Interaction logic for ConfigureDiffToolSettingsPageControl.xaml
	/// </summary>
	public partial class ConfigureDiffToolSettingsPageControl : UserControl
	{
		public ConfigureDiffToolSettingsPageControl()
		{
			InitializeComponent();
		}

		private System.Diagnostics.Process _configureDiffToolProcess = null;
		private void btnConfigureDiffTool_Click(object sender, EventArgs e)
		{
			// If the Configure Diff Tool window is already open, just exit.
			// For some reason the variable is not null until we actually try and check on of it's variables, so the HasExited check is required.
			if (_configureDiffToolProcess != null && !_configureDiffToolProcess.HasExited)
				return;

			// Launch the window to configure the merge tool.
			_configureDiffToolProcess = new System.Diagnostics.Process();
			_configureDiffToolProcess.StartInfo.FileName = DiffAllFilesHelper.TfFilePath;
			_configureDiffToolProcess.StartInfo.Arguments = string.Format("diff /configure");
			_configureDiffToolProcess.StartInfo.CreateNoWindow = true;
			_configureDiffToolProcess.StartInfo.UseShellExecute = false;
			_configureDiffToolProcess.Exited += configureDiffToolProcess_Exited;
			_configureDiffToolProcess.Start();
		}

		void configureDiffToolProcess_Exited(object sender, EventArgs e)
		{
			if (_configureDiffToolProcess != null)
				_configureDiffToolProcess.Exited -= configureDiffToolProcess_Exited;

			// Record that the user closed the Configure Diff Tool window.
			_configureDiffToolProcess = null;
		}

		private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
		{
			// Open the URL in the default browser.
			Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
			e.Handled = true;
		}
	}
}
