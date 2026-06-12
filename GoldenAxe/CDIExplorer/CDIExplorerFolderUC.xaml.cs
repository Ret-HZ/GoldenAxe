using AceUtils.CDI;
using Microsoft.Win32;
using System.Collections.Generic;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media;

namespace GoldenAxe.CDIExplorer
{
    /// <summary>
    /// Interaction logic for CDIExplorerFolderUC.xaml
    /// </summary>
    public partial class CDIExplorerFolderUC : UserControl
    {
        private CDIExplorerDirectoryUC Parent;
        internal CDIFolder Folder;
        public bool IsSelected;

        public CDIExplorerFolderUC(CDIExplorerDirectoryUC parent, CDIFolder folder)
        {
            InitializeComponent();
            Parent = parent;
            Folder = folder;
            txt_FolderName.Text = Folder.Name;
        }


        public void Select()
        {
            IsSelected = true;
            txt_FolderName.Background = Brushes.DarkOrange;
            rectangle_SelectionImage.Fill = Brushes.DarkOrange;
            
        }


        public void Deselect()
        {
            IsSelected = false;
            txt_FolderName.Background = Brushes.Transparent;
            rectangle_SelectionImage.Fill = Brushes.Transparent;
        }


        private void Grid_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (IsSelected)
            {
                Parent.DisplayDirectory(Folder.GetFiles(), Folder.Name);
            }
            else
            {
                Parent.NewSelection(this);
            }
        }


        private void Grid_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (!IsSelected) Parent.NewSelection(this);
        }


        private void Grid_ContextMenuClosing(object sender, ContextMenuEventArgs e)
        {
            if (IsSelected) Parent.ClearCurrentSelection();
        }


        private void mi_grid_FolderOpen_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Parent.DisplayDirectory(Folder.GetFiles(), Folder.Name);
        }


        private void mi_grid_FolderExtract_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            List<CDIFile> files = Folder.GetFiles();
            if (files.Count > 0)
            {
                var folderDialog = new OpenFolderDialog
                {
                    Title = "Select a destination directory for the contents",
                };

                if (folderDialog.ShowDialog() == true)
                {
                    string outPath = Path.Combine(folderDialog.FolderName, Folder.Name);
                    Directory.CreateDirectory(outPath);
                    foreach (CDIFile file in files)
                    {
                        string fileOutPath = Path.Combine(outPath, file.Name);
                        File.WriteAllBytes(fileOutPath, file.GetContent());
                    }
                    Util.ShowMessageBox($"{files.Count} files extracted.", "Success");
                }
            }
            else
            {
                Util.ShowMessageBox($"No files inside the {Folder.Name} folder.", "Error");
            }
        }
    }
}
