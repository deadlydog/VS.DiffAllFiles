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
using DansKingdom.VS_DiffAllFiles.Code.Base;

namespace DansKingdom.VS_DiffAllFiles.Code
{
	/// <summary>
	/// Interaction logic for PendingChangesDiffAllControl.xaml
	/// </summary>
	public partial class PendingChangesDiffAllControl : UserControl
	{
		public PendingChangesDiffAllControl()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Parent section.
		/// </summary>
		public PendingChangesDiffAllSection ParentSection
		{
			get { return (PendingChangesDiffAllSection)GetValue(ParentSectionProperty); }
			set { SetValue(ParentSectionProperty, value); }
		}
		public static readonly DependencyProperty ParentSectionProperty =
			DependencyProperty.Register("ParentSection", typeof(PendingChangesDiffAllSection), typeof(PendingChangesDiffAllControl));

		/// <summary>
		/// Handles the Click event of the btnDiffAllFiles control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
		private void btnDiffAllFiles_Click(object sender, RoutedEventArgs e)
		{

		}

		/// <summary>
		/// Handles the Click event of the btnDiffSelectedFiles control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
		private void btnDiffSelectedFiles_Click(object sender, RoutedEventArgs e)
		{

		}

		/// <summary>
		/// Handles the Click event of the btnDiffIncludedFiles control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
		private async void btnDiffIncludedFiles_Click(object sender, RoutedEventArgs e)
		{
			await ParentSection.CompareIncludedPendingChanges();
		}

		/// <summary>
		/// Handles the Click event of the btnDiffExcludedFiles control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
		private void btnDiffExcludedFiles_Click(object sender, RoutedEventArgs e)
		{

		}
	}
}
