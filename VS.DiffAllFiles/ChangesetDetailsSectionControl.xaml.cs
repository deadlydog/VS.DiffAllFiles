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
	/// Interaction logic for ChangesetDetailsSectionControl.xaml
	/// </summary>
	public partial class ChangesetDetailsSectionControl : UserControl
	{
		private ChangesetDetailsSection _viewModel = null;

		public ChangesetDetailsSectionControl(ChangesetDetailsSection parentSection)
		{
			InitializeComponent();

			// Get a handle to the View Model to use and set it up as the default binding data context.
			_viewModel = parentSection;
			this.DataContext = _viewModel;
		}

		/// <summary>
		/// Handles the Click event of the btnDiffAllFiles control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
		private async void btnDiffAllFiles_Click(object sender, RoutedEventArgs e)
		{
			await _viewModel.ComparePendingChanges(ItemStatusTypesToCompare.All);
		}

		/// <summary>
		/// Handles the Click event of the btnDiffSelectedFiles control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
		private async void btnDiffSelectedFiles_Click(object sender, RoutedEventArgs e)
		{
			await _viewModel.ComparePendingChanges(ItemStatusTypesToCompare.Selected);
		}

		/// <summary>
		/// Handles the Click event of the btnNextSetOfFiles control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
		private void btnNextSetOfFiles_Click(object sender, RoutedEventArgs e)
		{
			_viewModel.CompareNextSetOfFiles();
		}

		/// <summary>
		/// Handles the Click event of the btnCancelComparingFiles control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
		private void btnCancelComparingFiles_Click(object sender, RoutedEventArgs e)
		{
			_viewModel.Cancel();
		}

		/// <summary>
		/// Handles the Click event of the btnCloseAllOpenDiffTools control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
		private void btnCloseAllOpenDiffTools_Click(object sender, RoutedEventArgs e)
		{
			_viewModel.CloseAllOpenCompareWindows();
		}

		/// <summary>
		/// Handles the Click event of the btnCloseAllOpenDiffToolsInThisSet control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
		private void btnCloseAllOpenDiffToolsInThisSet_Click(object sender, RoutedEventArgs e)
		{
			_viewModel.CloseAllOpenCompareWindowsInThisSet();
		}
	}
}
