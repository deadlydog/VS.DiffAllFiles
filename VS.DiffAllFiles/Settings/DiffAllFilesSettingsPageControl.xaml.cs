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
