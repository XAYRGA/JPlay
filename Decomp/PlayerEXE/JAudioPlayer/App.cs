using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows;

namespace JAudioPlayer
{
	public class App : Application
	{
		private void Application_Startup(object sender, StartupEventArgs e)
		{
			OperatingSystem oSVersion = Environment.OSVersion;
			if (oSVersion.Version.Major < 6 || (oSVersion.Version.MajorRevision == 6 && oSVersion.Version.Minor < 2))
			{
				MessageBox.Show("This program can only be run on Wndows 8. Use the other version instead.", "OS version", MessageBoxButton.OK, MessageBoxImage.Hand);
				Shutdown();
			}
			base.MainWindow = new MainWindow();
			base.MainWindow.Show();
		}

		[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
		[DebuggerNonUserCode]
		public void InitializeComponent()
		{
			base.Startup += Application_Startup;
		}

		[STAThread]
		[DebuggerNonUserCode]
		[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
		public static void Main()
		{
			App app = new App();
			app.InitializeComponent();
			app.Run();
		}
	}
}
