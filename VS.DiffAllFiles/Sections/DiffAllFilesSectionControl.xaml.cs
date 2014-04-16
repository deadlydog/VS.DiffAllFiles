using System.Linq;
using System.Windows;
using System.Windows.Controls;
using VS_DiffAllFiles.DiffAllFilesBaseClasses;
using VS_DiffAllFiles.StructuresAndEnums;

namespace VS_DiffAllFiles.Sections
{
	/// <summary>
	/// Interaction logic for DiffAllFilesSectionControl.xaml
	/// </summary>
	public partial class DiffAllFilesSectionControl : UserControl
	{
		private DiffAllFilesSectionBase _viewModel = null;

		public DiffAllFilesSectionControl(DiffAllFilesSectionBase parentSection)
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
			await _viewModel.PerformItemDiffs(ItemStatusTypesToCompare.All);
		}

		/// <summary>
		/// Handles the Click event of the btnDiffSelectedFiles control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
		private async void btnDiffSelectedFiles_Click(object sender, RoutedEventArgs e)
		{
			await _viewModel.PerformItemDiffs(ItemStatusTypesToCompare.Selected);
		}

		/// <summary>
		/// Handles the Click event of the btnDiffIncludedFiles control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
		private async void btnDiffIncludedFiles_Click(object sender, RoutedEventArgs e)
		{
			await _viewModel.PerformItemDiffs(ItemStatusTypesToCompare.Included);
		}

		/// <summary>
		/// Handles the Click event of the btnDiffExcludedFiles control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
		private async void btnDiffExcludedFiles_Click(object sender, RoutedEventArgs e)
		{
			await _viewModel.PerformItemDiffs(ItemStatusTypesToCompare.Excluded);
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
