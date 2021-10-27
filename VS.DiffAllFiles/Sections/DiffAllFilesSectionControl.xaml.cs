using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.VisualStudio.Shell;
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
		private void btnDiffAllFiles_Click(object sender, RoutedEventArgs e)
		{
			_viewModel.PerformItemDiffsAsync(ItemStatusTypesToCompare.All).FireAndForget();
		}

		/// <summary>
		/// Handles the Click event of the btnDiffSelectedFiles control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
		private void btnDiffSelectedFiles_Click(object sender, RoutedEventArgs e)
		{
			_viewModel.PerformItemDiffsAsync(ItemStatusTypesToCompare.Selected).FireAndForget();
		}

		/// <summary>
		/// Handles the Click event of the btnDiffIncludedFiles control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
		private void btnDiffIncludedFiles_Click(object sender, RoutedEventArgs e)
		{
			_viewModel.PerformItemDiffsAsync(ItemStatusTypesToCompare.Included).FireAndForget();
		}

		/// <summary>
		/// Handles the Click event of the btnDiffExcludedFiles control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
		private void btnDiffExcludedFiles_Click(object sender, RoutedEventArgs e)
		{
			_viewModel.PerformItemDiffsAsync(ItemStatusTypesToCompare.Excluded).FireAndForget();
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
#if !VS2012
            ThreadHelper.ThrowIfNotOnUIThread();
#endif
			_viewModel.CloseAllOpenCompareWindows();
		}

		/// <summary>
		/// Handles the Click event of the btnCloseAllOpenDiffToolsInThisSet control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
		private void btnCloseAllOpenDiffToolsInThisSet_Click(object sender, RoutedEventArgs e)
		{
#if !VS2012
            ThreadHelper.ThrowIfNotOnUIThread();
#endif
			_viewModel.CloseAllOpenCompareWindowsInThisSet();
		}
	}
}
