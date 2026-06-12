using AceUtils.CDI;
using System.Collections.Generic;
using System.Windows.Controls;

namespace GoldenAxe.CDIExplorer
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

        bool IsRootFolderContentSet = false;


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
            int fileCount = folder.GetFiles().Count;
            lbl_FooterSelectionInfo.Content = $"{fileCount} {(fileCount == 1 ? "file" : "files")} in selected folder";
        }


        private void UpdateFooterSelectionInfo(CDIFile file)
        {
            lbl_FooterSelectionInfo.Content = $"Selected file is {Util.FormatBytes(file.GetContentSize())} | Compressed: {(file.IsCompressed ? "Yes" : "No")}";
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
            scrollviewer_ExplorerContentFiles.Visibility = System.Windows.Visibility.Collapsed;
            scrollviewer_ExplorerContentFolders.Visibility = System.Windows.Visibility.Visible;

            if (!IsRootFolderContentSet)
            {
                ClearSelections();
                ClearFooterSelectionInfo();
                wrappanel_ExplorerContentFolders.Children.Clear();
                foreach (CDIFolder folder in folders)
                {
                    CDIExplorerFolderUC folderIcon = new CDIExplorerFolderUC(this, folder);
                    wrappanel_ExplorerContentFolders.Children.Add(folderIcon);
                }
                scrollviewer_ExplorerContentFolders.ScrollToTop();
                IsRootFolderContentSet = true;
            }
            UpdateFooterElementAmount(folders.Count);
            UpdatePathDisplay(directoryName);
        }


        public void DisplayDirectory(List<CDIFile> files, string directoryName = "")
        {
            scrollviewer_ExplorerContentFiles.Visibility = System.Windows.Visibility.Visible;
            scrollviewer_ExplorerContentFolders.Visibility = System.Windows.Visibility.Collapsed;

            ClearSelections();
            ClearFooterSelectionInfo();
            wrappanel_ExplorerContentFiles.Children.Clear();
            foreach (CDIFile file in files)
            {
                CDIExplorerFileUC fileIcon = new CDIExplorerFileUC(this, file);
                wrappanel_ExplorerContentFiles.Children.Add(fileIcon);
            }
            scrollviewer_ExplorerContentFiles.ScrollToTop();
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


        public void ClearCurrentSelection()
        {
            ClearSelections();
            ClearFooterSelectionInfo();
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
