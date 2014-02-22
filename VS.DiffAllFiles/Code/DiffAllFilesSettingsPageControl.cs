using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DansKingdom.VS_DiffAllFiles.Code
{
	public partial class DiffAllFilesSettingsPageControl : UserControl
	{
		private DiffAllFilesSettings _settings = null;

		public DiffAllFilesSettingsPageControl()
		{
			InitializeComponent();
			var i = toolTip1.AutoPopDelay;

			// Get the current settings and reflect them on the UI.
			_settings = DiffAllFilesSettings.Settings;
			CopySettingsToUi();
		}

		/// <summary>
		/// Copies all of the current settings to the UI.
		/// </summary>
		public void CopySettingsToUi()
		{
			chkCompareDeletedFiles.Checked = _settings.CompareDeletedFiles;
			chkCompareFilesNotChanged.Checked = _settings.CompareFilesNotChanged;
			chkCompareNewFiles.Checked = _settings.CompareNewFiles;
			chkCompareFilesOneAtATime.Checked = _settings.CompareFilesOneAtATime;
			txtFileExtensionsToIgnore.Text = _settings.FileExtensionsToIgnore;
		}

		private void btnRestoreDefaultSettings_Click(object sender, EventArgs e)
		{
			DiffAllFilesSettings.Settings.ResetSettings();
			CopySettingsToUi();
		}

		private void chkCompareNewFiles_CheckedChanged(object sender, EventArgs e)
		{
			//_settings.CompareNewFiles = chkCompareNewFiles.Checked;
		}

		private void chkCompareDeletedFiles_CheckedChanged(object sender, EventArgs e)
		{
			//_settings.CompareDeletedFiles = chkCompareDeletedFiles.Checked;
		}

		private void chkCompareFilesNotChanged_CheckedChanged(object sender, EventArgs e)
		{
			//_settings.CompareFilesNotChanged = chkCompareFilesNotChanged.Checked;
		}

		private void chkCompareFilesOneAtATime_CheckedChanged(object sender, EventArgs e)
		{
			//_settings.CompareFilesOneAtATime = chkCompareFilesOneAtATime.Checked;
		}

		private void txtFileExtensionsToIgnore_Leave(object sender, EventArgs e)
		{
			_settings.FileExtensionsToIgnore = txtFileExtensionsToIgnore.Text;

			// Update the UI with the value, as the settings model will remove any empty entries.
			txtFileExtensionsToIgnore.Text = _settings.FileExtensionsToIgnore;
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
	}
}
