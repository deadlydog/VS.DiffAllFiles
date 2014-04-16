using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using VS_DiffAllFiles.StructuresAndEnums;

namespace VS_DiffAllFiles.Settings
{
	/// <summary>
	/// Interaction logic for DiffAllFilesSettingsPageControl.xaml
	/// </summary>
	public partial class DiffAllFilesSettingsPageControl : UserControl
	{
		/// <summary>
		/// A handle to the Settings instance that this control is bound to.
		/// </summary>
		private DiffAllFilesSettings _settings = null;

		public DiffAllFilesSettingsPageControl(DiffAllFilesSettings settings)
		{
			InitializeComponent();
			_settings = settings;
			this.DataContext = _settings;
		}

		private void btnRestoreDefaultSettings_Click(object sender, RoutedEventArgs e)
		{
			_settings.ResetSettings();
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

		private void UserControl_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
		{
			// Find all TextBoxes in this control force the Text bindings to fire to make sure all changes have been saved.
			// This is required because if the user changes some text, then clicks on the Options Window's OK button, it closes 
			// the window before the TextBox's Text bindings fire, so the new value will not be saved.
			foreach (var textBox in DiffAllFilesHelper.FindVisualChildren<TextBox>(sender as UserControl))
			{
				var bindingExpression = textBox.GetBindingExpression(TextBox.TextProperty);
				if (bindingExpression != null) bindingExpression.UpdateSource();
			}
		}

		private void radioCompareMode_LetUserDecide_Checked(object sender, RoutedEventArgs e)
		{
			_settings.CompareModesAvailable = CompareModes.AllowUserToChoose;
		}

		private void radioCompareMode_Independent_Checked(object sender, RoutedEventArgs e)
		{
			_settings.CompareModesAvailable = CompareModes.IndividualFiles;
		}

		private void radioCompareMode_Combined_Checked(object sender, RoutedEventArgs e)
		{
			_settings.CompareModesAvailable = CompareModes.CombinedIntoSingleFile;
		}
	}
}
