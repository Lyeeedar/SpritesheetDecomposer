using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
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

namespace SpritesheetDecomposer
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window, INotifyPropertyChanged
	{
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

			if (dlg.ShowDialog() == true)
			{
				var chosen = dlg.FileName;

				SelectedImage = chosen;
				RaisePropertyChangedEvent("SelectedImage");
			}
		}

		private void ExportButtonClick(object sender, RoutedEventArgs e)
		{
			var dlg = new CommonOpenFileDialog();
			dlg.IsFolderPicker = true;

			if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
			{
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

	public class RowName
	{
		public string Name { get; set; }
	}
}
