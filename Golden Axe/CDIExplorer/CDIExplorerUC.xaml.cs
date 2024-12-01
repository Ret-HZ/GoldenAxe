using AceUtils.CDI;
using Microsoft.Win32;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Golden_Axe.CDIExplorer
{
    /// <summary>
    /// Interaction logic for CDIExplorerUC.xaml
    /// </summary>
    public partial class CDIExplorerUC : UserControl
    {
        /// <summary>
        /// The REGFILE.CDI to display.
        /// </summary>
        internal CDI REGFILE;

        CDIExplorerDirectoryUC explorer;

        public CDIExplorerUC(CDI regfile)
        {
            InitializeComponent();
            REGFILE = regfile;
            InitDirectoryTreeview(REGFILE.GetFolders());
            InitDirectoryExplorer();
        }


        private void InitDirectoryTreeview(List<CDIFolder> CDIFolders, string searchStr = "")
        {
            List<CDIFolderItemTreeView> folders = new List<CDIFolderItemTreeView>();

            foreach (CDIFolder folder in CDIFolders)
            {
                CDIFolderItemTreeView tvfolder = new CDIFolderItemTreeView()
                {
                    Name = folder.Name,
                    Folder = folder,
                };
                foreach(CDIFile file in folder.SearchFilesByName(searchStr))
                {
                    CDIFileItemTreeView tvfile = new CDIFileItemTreeView()
                    {
                        Name = file.Name,
                        File = file,
                    };
                    tvfolder.Files.Add(tvfile);
                }

                folders.Add(tvfolder);
            }

            treeview_Directory.ItemsSource = folders;
        }


        private void InitDirectoryExplorer()
        {
            explorer = new CDIExplorerDirectoryUC(this);
            grid_DirectoryExplorer.Children.Add(explorer);
            explorer.DisplayDirectory(REGFILE.GetFolders());
        }


        private void mi_tv_FolderOpen_Click(object sender, RoutedEventArgs e)
        {
            TreeViewItem treeViewItem = GetTreeViewItemFromMenuItem(sender);

            if (treeViewItem != null)
            {
                CDIFolderItemTreeView folderItem = treeViewItem.Header as CDIFolderItemTreeView;
                if (folderItem != null && explorer != null)
                {
                    explorer.DisplayDirectory(folderItem.Folder.GetFiles(), folderItem.Name);
                }
            }
        }


        private void mi_tv_FolderExtract_Click(object sender, RoutedEventArgs e)
        {
            TreeViewItem treeViewItem = GetTreeViewItemFromMenuItem(sender);

            if (treeViewItem != null)
            {
                CDIFolderItemTreeView folderItem = treeViewItem.Header as CDIFolderItemTreeView;
                if (folderItem != null && explorer != null)
                {
                    List<CDIFile> files = folderItem.Folder.GetFiles();
                    if (files.Count > 0)
                    {
                        var folderDialog = new OpenFolderDialog
                        {
                            Title = "Select a destination directory for the contents",
                        };

                        if (folderDialog.ShowDialog() == true)
                        {
                            string outPath = Path.Combine(folderDialog.FolderName, folderItem.Folder.Name);
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
                        Util.ShowMessageBox($"No files inside the {folderItem.Name} folder.", "Error");
                    }
                }
            }
        }


        private void mi_tv_FileOpen_Click(object sender, RoutedEventArgs e)
        {
            TreeViewItem treeViewItem = GetTreeViewItemFromMenuItem(sender);

            if (treeViewItem != null)
            {
                CDIFileItemTreeView fileItem = treeViewItem.Header as CDIFileItemTreeView;
                if (fileItem != null)
                {
                    FileEditorHandler.OpenFileEditor(fileItem.File);
                }
            }
        }


        private void mi_tv_FileExtract_Click(object sender, RoutedEventArgs e)
        {
            TreeViewItem treeViewItem = GetTreeViewItemFromMenuItem(sender);

            if (treeViewItem != null)
            {
                CDIFileItemTreeView fileItem = treeViewItem.Header as CDIFileItemTreeView;
                if (fileItem != null)
                {
                    CDIFile file = fileItem.File;
                    string extension = file.GetExtension();
                    SaveFileDialog dialog = new SaveFileDialog()
                    {
                        FileName = file.GetNameWithoutExtension(),
                        DefaultExt = file.GetExtension(),
                        Filter = $"{extension} (*.{extension})|*.{extension}|All types (*.*)|*.*",
                    };

                    if (dialog.ShowDialog() == true)
                    {
                        File.WriteAllBytes(dialog.FileName, fileItem.File.GetContent());
                    }
                }
            }
        }


        private void txt_Searchbox_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key != System.Windows.Input.Key.Enter) return;
            System.Windows.Input.Keyboard.ClearFocus();
            List<CDIFolder> results = REGFILE.SearchFoldersByNameAndContentName(txt_Searchbox.Text);
            InitDirectoryTreeview(results, txt_Searchbox.Text);
        }


        private void StackPanel_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            TreeViewItem treeViewItem = GetTreeViewItemFromStackPanel(sender);

            if (treeViewItem != null)
            {
                treeViewItem.IsSelected = true;
            }
        }


        private TreeViewItem GetTreeViewItemFromMenuItem(object sender)
        {
            MenuItem menuItem = sender as MenuItem;
            ContextMenu contextMenu = menuItem.Parent as ContextMenu;
            StackPanel stackPanel = contextMenu.PlacementTarget as StackPanel;
            TreeViewItem treeViewItem = FindAncestor<TreeViewItem>(stackPanel);
            return treeViewItem;
        }


        private TreeViewItem GetTreeViewItemFromStackPanel(object sender)
        {
            StackPanel stackPanel = sender as StackPanel;
            TreeViewItem treeViewItem = FindAncestor<TreeViewItem>(stackPanel);
            return treeViewItem;
        }


        private T FindAncestor<T>(DependencyObject current) where T : DependencyObject
        {
            do
            {
                if (current is T)
                {
                    return (T)current;
                }
                current = VisualTreeHelper.GetParent(current);
            } while (current != null);
            return null;
        }
    }


    public class CDIFolderItemTreeView
    {
        public CDIFolderItemTreeView()
        {
            this.Files = new ObservableCollection<CDIFileItemTreeView>();
        }

        public string Name { get; set; }

        public CDIFolder Folder { get; set; }

        public ObservableCollection<CDIFileItemTreeView> Files { get; set; }
    }


    public class CDIFileItemTreeView
    {
        public string Name { get; set; }

        public CDIFile File { get; set; }
    }

}
