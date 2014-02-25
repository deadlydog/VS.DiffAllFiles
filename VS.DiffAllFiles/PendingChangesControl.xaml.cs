using System;
using System.Collections.Generic;
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
using VS_DiffAllFiles.DiffAllFilesBaseClasses;
using VS_DiffAllFiles.TeamExplorerBaseClasses;

namespace VS_DiffAllFiles
{
	/// <summary>
	/// Interaction logic for PendingChangesDiffAllControl.xaml
	/// </summary>
	public partial class PendingChangesControl : UserControl
	{
		private PendingChangesSection _pendingChangesViewModel = null;

		public PendingChangesControl(PendingChangesSection pendingChangesSection)
		{
			InitializeComponent();

			// Get a handle to the View Model to use and set it up as the default binding data context.
			_pendingChangesViewModel = pendingChangesSection;
			this.DataContext = _pendingChangesViewModel;
		}

		/// <summary>
		/// Handles the Click event of the btnDiffAllFiles control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
		private async void btnDiffAllFiles_Click(object sender, RoutedEventArgs e)
		{
			(sender as Button).IsEnabled = false;
			await _pendingChangesViewModel.ComparePendingChanges(ItemStatusTypesToCompare.All);
			(sender as Button).IsEnabled = true;
		}

		/// <summary>
		/// Handles the Click event of the btnDiffSelectedFiles control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
		private async void btnDiffSelectedFiles_Click(object sender, RoutedEventArgs e)
		{
			(sender as Button).IsEnabled = false;
			await _pendingChangesViewModel.ComparePendingChanges(ItemStatusTypesToCompare.Selected);
			(sender as Button).IsEnabled = true;
		}

		/// <summary>
		/// Handles the Click event of the btnDiffIncludedFiles control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
		private async void btnDiffIncludedFiles_Click(object sender, RoutedEventArgs e)
		{
			(sender as Button).IsEnabled = false;
			await _pendingChangesViewModel.ComparePendingChanges(ItemStatusTypesToCompare.Included);
			(sender as Button).IsEnabled = true;
		}

		/// <summary>
		/// Handles the Click event of the btnDiffExcludedFiles control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
		private async void btnDiffExcludedFiles_Click(object sender, RoutedEventArgs e)
		{
			(sender as Button).IsEnabled = false;
			await _pendingChangesViewModel.ComparePendingChanges(ItemStatusTypesToCompare.Excluded);
			(sender as Button).IsEnabled = true;
		}
	}
}
