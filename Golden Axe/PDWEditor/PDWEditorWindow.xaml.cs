using AceUtils.CDI;
using AceUtils.PDW;
using MahApps.Metro.Controls;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Golden_Axe.PDWEditor
{
    /// <summary>
    /// Interaction logic for PDWEditor.xaml
    /// </summary>
    public partial class PDWEditorWindow : MetroWindow
    {
        CDIFile File;
        PDW PDW;
        PDWTexture CurrentTexture;

        public PDWEditorWindow(CDIFile file)
        {
            InitializeComponent();
            File = file;
            PDW = PDWReader.ReadPDW(File.GetContent());
            Title = $"{File.Name}";
            CurrentTexture = PDW.Textures[0];
            SetInfoLabels();
            SetImage();
        }


        /// <summary>
        /// Update the information labels.
        /// </summary>
        private void SetInfoLabels()
        {
            lbl_TextureIndex.Content = PDW.Textures.IndexOf(CurrentTexture) + 1;
            lbl_TextureAmount.Content = $"Textures: {PDW.TextureAmount}";
            lbl_Dimensions.Content = $"Dimensions: {CurrentTexture.Width}x{CurrentTexture.Height}";
            lbl_Flags.Content = $"Pixel Format: {CurrentTexture.PixelFormat}\nFlag__0x09: {CurrentTexture.Flag_0x09}";
        }


        /// <summary>
        /// Set the current texture as the displayed image.
        /// </summary>
        private void SetImage()
        {
            Bitmap bitmap = CurrentTexture.GetBitmap((bool)checkbox_AlphaPreviewEnabled.IsChecked);
            Stream stream = new MemoryStream();
            bitmap.Save(stream, ImageFormat.Png);
            stream.Position = 0;
            BitmapImage result = new BitmapImage();
            result.BeginInit();
            result.CacheOption = BitmapCacheOption.OnLoad;
            result.StreamSource = stream;
            result.EndInit();
            result.Freeze();
            stream.Dispose();
            img_Image.Source = result;
        }


        /// <summary>
        /// Click event for the "Export" button. Will export the current image in the selected format.
        /// </summary>
        private void btn_Export_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = File.GetNameWithoutExtension();
            dlg.DefaultExt = ".png";
            dlg.Filter = "PNG (.png)|*.png";

            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                CurrentTexture.GetBitmap().Save(dlg.FileName);
            }
        }


        /// <summary>
        /// Click event for the "Import" button. Will import the selected image into the current texture.
        /// </summary>
        private void btn_Import_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = "PNG (*.png)|*.png|All types (*.*)|*.*";

            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                Bitmap bitmap = new Bitmap(dlg.FileName);
                CurrentTexture.SetBitmap(bitmap);
                SetImage();
                SetInfoLabels();
                File.SetContent(PDWWriter.WritePDWToArray(PDW));
            }
        }


        /// <summary>
        /// Click event for the "Alpha" checkbox. Will redraw the current texture.
        /// </summary>
        private void checkbox_AlphaPreviewEnabled_Click(object sender, RoutedEventArgs e)
        {
            SetImage();
        }


        /// <summary>
        /// Click event for the "Texture Back" button. Will move to the previous texture if there is any.
        /// </summary>
        private void btn_TextureBack_Click(object sender, RoutedEventArgs e)
        {
            int newIndex = (PDW.Textures.IndexOf(CurrentTexture) - 1 + PDW.Textures.Count) % PDW.Textures.Count;
            CurrentTexture = PDW.Textures[newIndex];
            SetInfoLabels();
            SetImage();
        }


        /// <summary>
        /// Click event for the "Texture Forward" button. Will move to the next texture if there is any.
        /// </summary>
        private void btn_TextureForward_Click(object sender, RoutedEventArgs e)
        {
            int newIndex = (PDW.Textures.IndexOf(CurrentTexture) + 1) % PDW.Textures.Count;
            CurrentTexture = PDW.Textures[newIndex];
            SetInfoLabels();
            SetImage();
        }
    }
}
