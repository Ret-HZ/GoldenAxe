using LibCDI;
using MahApps.Metro.Controls;
using Microsoft.Win32;
using System.Windows;
using System;
using System.Windows.Controls;

namespace Golden_Axe
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            this.Title = Util.GetAppTitle();
        }


        /// <summary>
        /// Opens an OpenFileDialog with the specified filters.
        /// </summary>
        /// <param name="filters">The filters to use in the file dialog.</param>
        private string? OpenFileDialogGeneric(params string[] filters)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            string dialogFilter = "";
            foreach (string filter in filters)
            {
                dialogFilter += filter;
                if (filter != filters[filters.Length - 1]) dialogFilter += "|";
            }
            openFileDialog.Filter = dialogFilter;
            Nullable<bool> result = openFileDialog.ShowDialog();
            if (result == true)
            {
                string filePath = openFileDialog.FileName;
                return filePath;
            }
            else
            {
                return null;
            }
        }


        private void OpenRegfileX(object sender, RoutedEventArgs e)
        {
            string? path = OpenFileDialogGeneric(
                "REGFILE Archives (*.CDI)|*.CDI",
                "All files (*.*)|*.*"
            );

            if (path != null)
            {
                OpenRegfile(CDIVersion.X, path);
            }
        }


        private void OpenRegfileX2(object sender, RoutedEventArgs e)
        {
            string? path = OpenFileDialogGeneric(
                "REGFILE Archives (*.CDI)|*.CDI",
                "All files (*.*)|*.*"
            );

            if (path != null)
            {
                OpenRegfile(CDIVersion.X2, path);
            }
        }


        private void OpenRegfileX_PROTOTYPE_20060816(object sender, RoutedEventArgs e)
        {
            string? path = OpenFileDialogGeneric(
                "REGFILE Archives (*.CDI)|*.CDI",
                "All files (*.*)|*.*"
            );

            if (path != null)
            {
                OpenRegfile(CDIVersion.X_PROTOTYPE_20060816, path);
            }
        }


        private void OpenRegfileX_PROTOTYPE_20060801(object sender, RoutedEventArgs e)
        {
            string? path = OpenFileDialogGeneric(
                "REGFILE Archives (*.CDI)|*.CDI",
                "All files (*.*)|*.*"
            );

            if (path != null)
            {
                OpenRegfile(CDIVersion.X_PROTOTYPE_20060801, path);
            }
        }


        private void OpenRegfile(CDIVersion version, string path)
        {
            CDI regfile = CDIReader.ReadCDI(version, path);
            CDIExplorer.CDIExplorerUC explorer = new CDIExplorer.CDIExplorerUC(regfile);
            Grid.SetRow(explorer, 0);
            Grid.SetColumn(explorer, 0);
            grid_Explorer.Children.Add(explorer);
        }
    }
}
