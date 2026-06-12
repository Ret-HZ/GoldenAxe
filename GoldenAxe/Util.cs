using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace GoldenAxe
{
    internal static class Util
    {
        public static string GetAppTitle()
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            System.Diagnostics.FileVersionInfo fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);
            string version = fvi.FileVersion;
            string name = GetAssemblyProductName();
            string title = String.Format("{0} {1} {2}", name, version, GetCommitHash());
            return title;
        }


        public static string GetCommitHash()
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var attr = (AssemblyMetadataAttribute)assembly.GetCustomAttribute(typeof(AssemblyMetadataAttribute));
            return attr.Value;
        }


        /// <summary>
        /// Gets the product name of the application according to the <see cref="AssemblyProductAttribute"/>.
        /// </summary>
        /// <returns>The product name.</returns>
        public static string GetAssemblyProductName()
        {
            return Assembly.GetExecutingAssembly().GetCustomAttributes<AssemblyProductAttribute>().FirstOrDefault().Product;
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
