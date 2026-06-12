using MahApps.Metro.Controls.Dialogs;
using MahApps.Metro.Controls;
using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Linq;

namespace GoldenAxe
{
    internal static class Util
    {
        public static string GetAppTitle()
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            System.Diagnostics.FileVersionInfo fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);
            string version = fvi.FileVersion;
            string name = System.Reflection.Assembly.GetEntryAssembly().GetName().Name;
            string title = String.Format("{0} {1}", name, version);
            return title;
        }


        public static string FormatBytes(long bytes)
        {
            string[] Suffix = { "B", "KB", "MB", "GB", "TB" };
            int i;
            double dblSByte = bytes;
            for (i = 0; i < Suffix.Length && bytes >= 1024; i++, bytes /= 1024)
            {
                dblSByte = bytes / 1024.0;
            }

            return $"{dblSByte:F2} {Suffix[i]}";
        }


        public static BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();

                return bitmapimage;
            }
        }


        public static Task<MessageDialogResult> ShowMessageBox(string message, string title = "", MessageDialogStyle style = MessageDialogStyle.Affirmative, MetroDialogSettings settings = null)
        {
            var firstMetroWindow = Application.Current.Windows.OfType<MetroWindow>().First();
            return firstMetroWindow.ShowMessageAsync(title, message, style, settings);
        }


        public static Task<MessageDialogResult> ShowMessageBox(DependencyObject dependencyObject, string message, string title = "", MessageDialogStyle style = MessageDialogStyle.Affirmative, MetroDialogSettings settings = null)
        {
            Window window = Window.GetWindow(dependencyObject);
            MetroWindow metroWindow = window as MetroWindow;
            return metroWindow.ShowMessageAsync(title, message, style, settings);
        }
    }
}
