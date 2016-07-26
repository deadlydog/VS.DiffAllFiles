using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;
using VS_DiffAllFiles.StructuresAndEnums;

namespace VS_DiffAllFiles.Settings
{
	[ClassInterface(ClassInterfaceType.AutoDual)]
	[Guid("083A2080-F4CB-1287-968B-112896C4C6FA")]
	public class ConfigureDiffToolSettingsPage : UIElementDialogPage, INotifyPropertyChanged
	{
		#region Notify Property Changed
		/// <summary>
		/// Inherited event from INotifyPropertyChanged.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Fires the PropertyChanged event of INotifyPropertyChanged with the given property name.
		/// </summary>
		/// <param name="propertyName">The name of the property to fire the event against</param>
		public void NotifyPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}
		#endregion

		#region Overridden Functions

		/// <summary>
		/// Gets the Windows Presentation Foundation (WPF) child element to be hosted inside the Options dialog page.
		/// </summary>
		/// <returns>The WPF child element.</returns>
		protected override System.Windows.UIElement Child
		{
			get { return new ConfigureDiffToolSettingsPageControl(); }
		}

		#endregion
	}
}
