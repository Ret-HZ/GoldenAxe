using AceUtils.CDI;
using AceUtils.CDI.Enum;
using MahApps.Metro.Controls;
using Microsoft.Win32;
using System.Windows;
using System.IO;
using System;
using System.Windows.Controls;
using MahApps.Metro.Controls.Dialogs;
using System.Threading.Tasks;

namespace Golden_Axe
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        CDIExplorer.CDIExplorerUC explorer;

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


        private async void OpenRegfile(CDIVersion version, string path)
        {
            var controller = await this.ShowProgressAsync("Opening REGFILE", "Please wait...");
            controller.SetIndeterminate();

            CDI regfile = await Task.Factory.StartNew(() =>
            {
                return CDIReader.ReadCDI(version, path);
            });

            grid_Explorer.Children.Clear();
            explorer = new CDIExplorer.CDIExplorerUC(regfile);
            Grid.SetRow(explorer, 0);
            Grid.SetColumn(explorer, 0);
            grid_Explorer.Children.Add(explorer);
            mi_SaveRegfile.IsEnabled = true;
            mi_Tools_ExtractAll.IsEnabled = true;

            await controller.CloseAsync();
        }


        private void SaveRegfile(string path)
        {
            try
            {
                CDIWriter.WriteCDIToFile(explorer.REGFILE, path);
            } catch (IOException ioexception)
            {
                Util.ShowMessageBox($"{ioexception.Message}", "Error");
            }
        }


        private void mi_SaveRegfile_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.FileName = "REGFILE";
            dlg.DefaultExt = ".CDI";
            dlg.Filter = "REGFILE Archives (*.CDI)|*.CDI|All files (*.*)|*.*";

            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                SaveRegfile(dlg.FileName);
            }
        }


        private async void mi_Tools_ExtractAll_Click(object sender, RoutedEventArgs e)
        {
            var openFolderDialog = new OpenFolderDialog
            {
                Title = "Choose where to extract the contents",
            };

            if (openFolderDialog.ShowDialog() == true)
            {
                var controller = await this.ShowProgressAsync("Extracting REGFILE contents...", "", true);
                await Task.Run(() => {
                    try
                    {
                        int totalFiles = 0;
                        foreach (var folder in explorer.REGFILE.GetFolders())
                        {
                            totalFiles += folder.Files.Count;
                        }
                        controller.Minimum = 1;
                        controller.Maximum = totalFiles;

                        int progress = 1;
                        foreach (var folder in explorer.REGFILE.GetFolders())
                        {
                            Directory.CreateDirectory(Path.Combine(openFolderDialog.FolderName, folder.Name));
                            var files = folder.GetFiles();
                            foreach (var file in files)
                            {
                                if (controller.IsCanceled) return;
                                string filePath = Path.Combine(folder.Name, file.Name);
                                controller.SetMessage(filePath);
                                controller.SetProgress(progress++);
                                if (!file.IsDummy)
                                {
                                    File.WriteAllBytes(Path.Combine(openFolderDialog.FolderName, filePath), file.GetContent());
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        this.Dispatcher.Invoke(() => {
                            Util.ShowMessageBox($"An error has occurred during extraction.\nException: {ex.Message}", "Error");
                        });
                    }
                });

                await controller.CloseAsync();
            }
        }
    }
}
