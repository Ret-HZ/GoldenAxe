using AceUtils.CDI;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Controls;

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
                };
                foreach(CDIFile file in folder.GetFiles())
                {
                    CDIFileItemTreeView tvfile = new CDIFileItemTreeView()
                    {
                        Name = file.Name,
                    };
                    tvfolder.Files.Add(tvfile);
                }

                folders.Add(tvfolder);
            }

            treeview_Directory.ItemsSource = folders;
        }


        private void InitDirectoryExplorer()
        {
            CDIExplorerDirectoryUC explorer = new CDIExplorerDirectoryUC(this);
            grid_DirectoryExplorer.Children.Add(explorer);
            explorer.DisplayDirectory(REGFILE.GetFolders());
        }
    }


    public class CDIFolderItemTreeView
    {
        public CDIFolderItemTreeView()
        {
            this.Files = new ObservableCollection<CDIFileItemTreeView>();
        }

        public string Name { get; set; }

        public ObservableCollection<CDIFileItemTreeView> Files { get; set; }
    }


    public class CDIFileItemTreeView
    {
        public string Name { get; set; }
    }

}
