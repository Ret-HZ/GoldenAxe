using AceUtils.CDI;
using System.Collections.Generic;
using System.Windows.Controls;

namespace Golden_Axe.CDIExplorer
{
    /// <summary>
    /// Interaction logic for CDIExplorerDirectoryUC.xaml
    /// </summary>
    public partial class CDIExplorerDirectoryUC : UserControl
    {
        CDIExplorerUC Parent;
        CDIExplorerFolderUC SelectedFolderUC;
        CDIExplorerFileUC SelectedFileUC;
        string CurrentDirectory;
        string rootDirectoryName = "CDI_ROOT";


        public CDIExplorerDirectoryUC(CDIExplorerUC parent)
        {
            InitializeComponent();
            Parent = parent;
        }


        private void UpdateFooterElementAmount(int amount)
        {
            lbl_FooterElementAmount.Content = $"{amount} {(amount == 1 ? "element" : "elements")}";
        }


        private void UpdateFooterSelectionInfo(CDIFolder folder)
        {
            lbl_FooterSelectionInfo.Content = $"{folder.Files.Count} {(folder.Files.Count == 1 ? "file" : "files")} in selected folder";
        }


        private void UpdateFooterSelectionInfo(CDIFile file)
        {
            lbl_FooterSelectionInfo.Content = $"Selected file is {Util.FormatBytes(file.GetContentSize())}";
        }


        private void ClearFooterSelectionInfo()
        {
            lbl_FooterSelectionInfo.Content = "";
        }


        private void UpdatePathDisplay(string directoryName)
        {
            txt_NavPath.Text = $"{rootDirectoryName}/{directoryName}";
        }


        public void DisplayDirectory(List<CDIFolder> folders, string directoryName = "")
        {
            ClearSelections();
            ClearFooterSelectionInfo();
            wrappanel_ExplorerContent.Children.Clear();
            foreach (CDIFolder folder in folders)
            {
                CDIExplorerFolderUC folderIcon = new CDIExplorerFolderUC(this, folder);
                wrappanel_ExplorerContent.Children.Add(folderIcon);
            }
            scrollviewer_ExplorerContent.ScrollToTop();
            UpdateFooterElementAmount(folders.Count);
            UpdatePathDisplay(directoryName);
        }


        public void DisplayDirectory(List<CDIFile> files, string directoryName = "")
        {
            ClearSelections();
            ClearFooterSelectionInfo();
            wrappanel_ExplorerContent.Children.Clear();
            foreach (CDIFile file in files)
            {
                CDIExplorerFileUC fileIcon = new CDIExplorerFileUC(this, file);
                wrappanel_ExplorerContent.Children.Add(fileIcon);
            }
            scrollviewer_ExplorerContent.ScrollToTop();
            UpdateFooterElementAmount(files.Count);
            UpdatePathDisplay(directoryName);
        }


        private void ClearSelections()
        {
            if (SelectedFolderUC != null)
            {
                SelectedFolderUC.Deselect();
                SelectedFolderUC = null;
            }
            if (SelectedFileUC != null)
            {
                SelectedFileUC.Deselect();
                SelectedFileUC = null;
            }
        }


        public void NewSelection(CDIExplorerFolderUC folderUC)
        {
            ClearSelections();
            SelectedFolderUC = folderUC;
            SelectedFolderUC.Select();

            UpdateFooterSelectionInfo(folderUC.Folder);
        }


        public void NewSelection(CDIExplorerFileUC fileUC)
        {
            ClearSelections();
            SelectedFileUC = fileUC;
            SelectedFileUC.Select();

            UpdateFooterSelectionInfo(fileUC.File);
        }


        private void btn_NavDirectoryBack_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            DisplayDirectory(Parent.REGFILE.GetFolders());
        }


        private void btn_NavDirectoryForward_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (SelectedFolderUC != null)
            {
                DisplayDirectory(SelectedFolderUC.Folder.GetFiles(), SelectedFolderUC.Folder.Name);
            }
        }
    }
}
