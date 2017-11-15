using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using SpritesheetDecomposer.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
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
using System.Xml.Linq;

namespace SpritesheetDecomposer
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window, INotifyPropertyChanged
	{
		public Settings settings = new Settings();

		public string SelectedImage { get; set; }

		public string RowsString
		{
			get { return m_rowsString; }
			set
			{
				m_rowsString = value;

				int count;
				if (int.TryParse(m_rowsString, out count))
				{
					var rows = new List<RowName>(RowNames);

					while (rows.Count > count)
					{
						rows.RemoveAt(rows.Count - 1);
					}

					while (rows.Count < count)
					{
						rows.Add(new RowName());
					}

					RowNames = rows;
					RaisePropertyChangedEvent("RowNames");
				}
			}
		}
		private string m_rowsString;

		public string ColumnsString { get; set; }

		public List<RowName> RowNames { get; set; } = new List<RowName>();

		public MainWindow()
		{
			DataContext = this;

			InitializeComponent();
		}

		public event PropertyChangedEventHandler PropertyChanged;
		private void RaisePropertyChangedEvent(string propName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
		}

		private void BrowseButtonClick(object sender, RoutedEventArgs e)
		{
			var dlg = new OpenFileDialog();
			dlg.Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png";

			var startPath = settings.BrowsePath;
			if (!string.IsNullOrWhiteSpace(startPath))
			{
				dlg.InitialDirectory = startPath;
			}

			if (dlg.ShowDialog() == true)
			{
				var chosen = dlg.FileName;

				settings.BrowsePath = System.IO.Path.GetDirectoryName(chosen);

				SelectedImage = chosen;
				RaisePropertyChangedEvent("SelectedImage");
			}
		}

		private void ExportButtonClick(object sender, RoutedEventArgs e)
		{
			var dlg = new CommonOpenFileDialog();
			dlg.IsFolderPicker = true;

			var startPath = settings.ExportPath;
			if (!string.IsNullOrWhiteSpace(startPath))
			{
				dlg.InitialDirectory = startPath;
			}

			if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
			{
				settings.ExportPath = dlg.FileName;

				// do the export
				var rows = int.Parse(RowsString);
				var columns = int.Parse(ColumnsString);

				var srcBmp = new Bitmap(SelectedImage);
				var width = srcBmp.Width;
				var height = srcBmp.Height;

				var tileWidth = width / columns;
				var tileHeight = height / rows;

				for (int row = 0; row < rows; row++)
				{
					var rowName = RowNames[row].Name;

					var rowStart = row * tileHeight;

					for (int column = 0; column < columns; column++)
					{
						var columnStart = column * tileWidth;

						var name = rowName + (column + 1);

						var tileRect = new System.Drawing.Rectangle(columnStart, rowStart, tileWidth, tileHeight);

						var tile = srcBmp.Clone(tileRect, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
						tile.Save(System.IO.Path.Combine(dlg.FileName, name + ".png"));
					}
				}
			}
		}
	}

	public class Settings
	{
		public string BrowsePath
		{
			get { return m_browsePath; }
			set
			{
				m_browsePath = value;
				Save();
			}
		}
		private string m_browsePath;

		public string ExportPath
		{
			get { return m_exportPath; }
			set
			{
				m_exportPath = value;
				Save();
			}
		}
		private string m_exportPath;

		public string SettingsFile
		{
			get
			{
				return "DecomposerSettings.xml";
			}
		}

		public Settings()
		{
			if (File.Exists(SettingsFile))
			{
				var doc = XDocument.Load(SettingsFile);
				m_browsePath = doc.Root.Element("BrowsePath").Value;
				m_exportPath = doc.Root.Element("ExportPath").Value;
			}
		}

		public void Save()
		{
			var doc = new XDocument();
			doc.Add(new XElement("Root"));

			doc.Root.Add(new XElement("BrowsePath", BrowsePath));
			doc.Root.Add(new XElement("ExportPath", ExportPath));

			doc.Save(SettingsFile);
		}
	}

	public class RowName
	{
		public string Name { get; set; }
	}
}
