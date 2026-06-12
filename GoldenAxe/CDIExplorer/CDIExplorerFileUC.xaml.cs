using AceUtils.CDI;
using AceUtils.PDW;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GoldenAxe.CDIExplorer
{
    /// <summary>
    /// Interaction logic for CDIExplorerFileUC.xaml
    /// </summary>
    public partial class CDIExplorerFileUC : UserControl
    {
        private CDIExplorerDirectoryUC Parent;
        internal CDIFile File;
        public bool IsSelected;

        public CDIExplorerFileUC(CDIExplorerDirectoryUC parent, CDIFile file)
        {
            InitializeComponent();
            Parent = parent;
            File = file;
            txt_FileName.Text = File.Name;
            UpdateThumbnail();
        }


        private void UpdateThumbnail()
        {
            if (!File.IsDummy)
            {
                if (File.GetExtension() == "PDW")
                {
                    img_Thumbnail.Source = Util.BitmapToImageSource(PDWReader.ReadPDW(File.GetContent()).Textures[0].GetBitmap());
                }
            }
        }


        public void Select()
        {
            IsSelected = true;
            txt_FileName.Background = Brushes.DarkOrange;
            rectangle_SelectionImage.Fill = Brushes.DarkOrange;
        }


        public void Deselect()
        {
            IsSelected = false;
            txt_FileName.Background = Brushes.Transparent;
            rectangle_SelectionImage.Fill = Brushes.Transparent;
        }


        private void Grid_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (IsSelected)
            {
                try
                {
                    FileEditorHandler.OpenFileEditor(File);
                }
                catch (Exception ex)
                {
                    Util.ShowMessageBox($"An error has occurred when attempting to open the file.\n\n{ex.Message}", "Error");
                }
            }
            else
            {
                Parent.NewSelection(this);
            }
        }


        private void Grid_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (!IsSelected)
            {
                Parent.NewSelection(this);
            }
        }


        private void mi_FileExtract_Click(object sender, RoutedEventArgs e)
        {
            string extension = File.GetExtension();
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = File.GetNameWithoutExtension();
            dlg.DefaultExt = extension;
            dlg.Filter = $"{extension} (*.{extension})|*.{extension}|All types (*.*)|*.*";

            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                System.IO.File.WriteAllBytes(dlg.FileName, File.GetContent());
            }
        }


        private void mi_FileImport_Click(object sender, RoutedEventArgs e)
        {
            string extension = File.GetExtension();
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = $"{extension} (*.{extension})|*.{extension}|All types (*.*)|*.*";

            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                byte[] content = System.IO.File.ReadAllBytes(dlg.FileName);
                File.SetContent(content);
            }

            UpdateThumbnail();
        }


        private void mi_FileCompression_Click(object sender, RoutedEventArgs e)
        {
            File.SetCompression(!File.IsCompressed);
            Parent.NewSelection(this);
        }
    }
}
