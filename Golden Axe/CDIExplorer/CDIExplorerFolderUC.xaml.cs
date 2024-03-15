using AceUtils.CDI;
using System.Windows.Controls;
using System.Windows.Media;

namespace Golden_Axe.CDIExplorer
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
    }
}
