using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnvDTE;
using EnvDTE80;

namespace DansKingdom.VS_DiffAllFiles.Code
{
	public static class PackageHelper
	{
		/// <summary>
		/// Gets the DTE2 of the package.
		/// </summary>
		public static DTE2 DTE2
		{
			get { return _dte2 ?? (_dte2 = Microsoft.VisualStudio.Shell.Package.GetGlobalService(typeof (DTE)) as DTE2); }
		}
		private static DTE2 _dte2 = null;

		/// <summary>
		/// Get if the specified Command is available to be ran or not.
		/// </summary>
		/// <param name="commandName">Name of the command.</param>
		public static bool IsCommandAvailable(string commandName)
		{
			var commands = DTE2.Commands as EnvDTE80.Commands2;
			if (commands == null)
				return false;

			var command = commands.Item(commandName, 0);
			if (command == null)
				return false;

			return command.IsAvailable;
		}

		/// <summary>
		/// Gets the full path to TF.exe.
		/// </summary>
		public static string VisualStudioExecutablePath
		{
			get { return System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName; }
		}
	}
}
