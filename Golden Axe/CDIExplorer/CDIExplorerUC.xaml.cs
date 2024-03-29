using AceUtils.CDI;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
            InitDirectoryTreeview();
            InitDirectoryExplorer();
        }


        private void InitDirectoryTreeview()
        {
            List<CDIFolderItemTreeView> folders = new List<CDIFolderItemTreeView>();

            foreach (CDIFolder folder in REGFILE.GetFolders())
            {
                CDIFolderItemTreeView tvfolder = new CDIFolderItemTreeView()
                {
                    Name = folder.Name,
                    Folder = folder,
                };
                foreach(CDIFile file in folder.GetFiles())
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
