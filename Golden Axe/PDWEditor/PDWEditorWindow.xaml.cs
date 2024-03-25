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

        public PDWEditorWindow(CDIFile file)
        {
            InitializeComponent();
            File = file;
            PDW = PDWReader.ReadPDW(File.GetContent());
            Title = $"{File.Name}";
            SetInfoLabels();
            SetImage();
        }


        private void SetInfoLabels()
        {
            lbl_Dimensions.Content = $"Dimensions: {PDW.Width}x{PDW.Height}";
            lbl_TextureAmount.Content = $"Texture Amount: {PDW.TextureAmount}";
            lbl_Flags.Content = $"Pixel Format: {PDW.PixelFormat}\nFlag__0x09: {PDW.Flag_0x09}";
        }


        private void SetImage()
        {
            Bitmap bitmap = PDW.GetBitmap();
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


        private void btn_Export_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = File.GetNameWithoutExtension();
            dlg.DefaultExt = ".png";
            dlg.Filter = "PNG (.png)|*.png";

            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                PDW.GetBitmap().Save(dlg.FileName);
            }
        }


        private void btn_Import_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = "PNG (*.png)|*.png|All types (*.*)|*.*";

            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                Bitmap bitmap = new Bitmap(dlg.FileName);
                PDW.SetBitmap(bitmap);
                SetImage();
                SetInfoLabels();
                File.SetContent(PDWWriter.WritePDWToArray(PDW));
            }
        }
    }
}
