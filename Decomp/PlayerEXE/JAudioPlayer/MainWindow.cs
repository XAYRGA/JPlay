using JAudio.Sequence;
using Microsoft.Win32;
using SharpDX;
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Markup;

namespace JAudioPlayer
{
	public class MainWindow : Window, IComponentConnector
	{
		private Playback playback;

		private bool _contentLoaded;

		public MainWindow()
		{
			InitializeComponent();
		}

		private void Help_About_Click(object sender, RoutedEventArgs e)
		{
			About about = new About();
			about.Owner = this;
			about.ShowDialog();
		}

		private void File_Exit_Click(object sender, RoutedEventArgs e)
		{
			System.Windows.Application.Current.Shutdown();
		}

		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			try
			{
				string text = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\JAudio Player";
				string path = text + "\\Z2Sound.dat";
				string soundPath = string.Empty;
				if (!Directory.Exists(text))
				{
					Directory.CreateDirectory(text);
				}
				if (File.Exists(path))
				{
					StreamReader streamReader = new StreamReader(new FileStream(path, FileMode.Open));
					soundPath = streamReader.ReadLine();
					streamReader.Dispose();
				}
				else
				{
					FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
					folderBrowserDialog.Description = "Select the path that contains 'Z2Sound.baa' and the subdirectory 'Waves'.";
					folderBrowserDialog.ShowNewFolderButton = false;
					if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
					{
						StreamWriter streamWriter = new StreamWriter(new FileStream(path, FileMode.Create));
						streamWriter.WriteLine(folderBrowserDialog.SelectedPath);
						streamWriter.Dispose();
						soundPath = folderBrowserDialog.SelectedPath;
					}
					else
					{
						System.Windows.Application.Current.Shutdown();
					}
				}
				playback = new Playback(soundPath);
				if (Environment.GetCommandLineArgs().Length > 1)
				{
					base.Title = Path.GetFileNameWithoutExtension(Environment.GetCommandLineArgs()[1]) + " - JAudio Player";
					Bms sequence = new Bms(File.OpenRead(Environment.GetCommandLineArgs()[1]));
					playback.Sequence = sequence;
					Playback_Start(null, null);
				}
			}
			catch (SharpDXException)
			{
				System.Windows.MessageBox.Show("Could not initialize the audio interface.", "Load sequence", MessageBoxButton.OK, MessageBoxImage.Hand);
			}
			catch (Exception ex2)
			{
				System.Windows.MessageBox.Show(ex2.Message, "Load sequence", MessageBoxButton.OK, MessageBoxImage.Hand);
			}
		}

		private void Playback_Start(object sender, RoutedEventArgs e)
		{
			try
			{
				playback.Start();
			}
			catch (Exception ex)
			{
				System.Windows.MessageBox.Show(ex.Message, "Playback", MessageBoxButton.OK, MessageBoxImage.Hand);
			}
		}

		private void Playback_Stop(object sender, RoutedEventArgs e)
		{
			playback.Stop();
		}

		private void OpenFile(object sender, RoutedEventArgs e)
		{
			Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
			openFileDialog.CheckFileExists = true;
			openFileDialog.CheckPathExists = true;
			openFileDialog.Filter = "BMS sequences|*.bms";
			openFileDialog.FilterIndex = 0;
			openFileDialog.Multiselect = false;
			openFileDialog.Title = "Open sequence";
			if (openFileDialog.ShowDialog(this) == true)
			{
				base.Title = Path.GetFileNameWithoutExtension(openFileDialog.FileName) + " - JAudio Player";
				if (playback.IsPlaying)
				{
					playback.Stop();
				}
				playback.Sequence = new Bms(File.OpenRead(openFileDialog.FileName));
				playback.Start();
			}
		}

		[DebuggerNonUserCode]
		[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
		public void InitializeComponent()
		{
			if (!_contentLoaded)
			{
				_contentLoaded = true;
				Uri resourceLocator = new Uri("/Player_Win8;component/mainwindow.xaml", UriKind.Relative);
				System.Windows.Application.LoadComponent(this, resourceLocator);
			}
		}

		[DebuggerNonUserCode]
		[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		void IComponentConnector.Connect(int connectionId, object target)
		{
			switch (connectionId)
			{
			case 1:
				((MainWindow)target).Loaded += OnLoaded;
				break;
			case 2:
				((System.Windows.Controls.MenuItem)target).Click += OpenFile;
				break;
			case 3:
				((System.Windows.Controls.MenuItem)target).Click += File_Exit_Click;
				break;
			case 4:
				((System.Windows.Controls.MenuItem)target).Click += Playback_Start;
				break;
			case 5:
				((System.Windows.Controls.MenuItem)target).Click += Playback_Stop;
				break;
			case 6:
				((System.Windows.Controls.MenuItem)target).Click += Help_About_Click;
				break;
			default:
				_contentLoaded = true;
				break;
			}
		}
	}
}
