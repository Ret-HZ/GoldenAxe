using AceUtils.PDW.Enum;
using ImageMagick;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace AceUtils.PDW
{
    public class PDWTexture
    {
        internal PDWTexture()
        {

        }


        /// <summary>
        /// Total amount of bytes taken by the pixel section.
        /// </summary>
        internal int PixelDataLength;

        /// <summary>
        /// Texture height.
        /// </summary>
        public ushort Height;

        /// <summary>
        /// Texture width.
        /// </summary>
        public ushort Width;

        /// <summary>
        /// Specifies the format of the color data for each pixel in the image.
        /// </summary>
        public PDWPixelFormat PixelFormat;

        /// <summary>
        /// Unknown.
        /// </summary>
        public byte Flag_0x09;

        /// <summary>
        /// Unknown.
        /// </summary>
        public ushort Unk_0x0A;

        /// <summary>
        /// Unknown.
        /// </summary>
        public int Unk_0x0C;

        /// <summary>
        /// Pixel data as a byte array.
        /// </summary>
        /// <remarks>Each byte corresponds to 1 pixel.</remarks>
        internal byte[] PixelData;

        /// <summary>
        /// Palette data as a byte array.
        /// </summary>
        internal byte[] PaletteData;

        /// <summary>
        /// Bitmap of the PDW.
        /// </summary>
        internal Bitmap Bitmap;



        /// <summary>
        /// Updates PDW data from the currently assigned bitmap.
        /// </summary>
        private void UpdateData()
        {
            if (Bitmap == null) return;
            Height = (ushort)Bitmap.Height;
            Width = (ushort)Bitmap.Width;
        }


        /// <summary>
        /// Gets the texture as a <see cref="System.Drawing.Bitmap"./>
        /// </summary>
        /// <param name="alphaEnabled">Is the alpha channel enabled? Set by default to <see langword="true"/>.</param>
        /// <returns>A <see cref="System.Drawing.Bitmap"/>.</returns>
        public Bitmap GetBitmap(bool alphaEnabled = true)
        {
            if (alphaEnabled)
            {
                return new Bitmap(Bitmap);
            }
            else
            {
                Bitmap copy = new Bitmap(Bitmap);
                for (int h = 0; h < copy.Height; h++)
                {
                    for (int w = 0; w < copy.Width; w++)
                    {
                        Color currentColor = copy.GetPixel(w, h);
                        copy.SetPixel(w, h, Color.FromArgb(255, currentColor));
                    }
                }
                return copy;
            }
        }


        /// <summary>
        /// Sets the texture from a <see cref="System.Drawing.Bitmap"./>
        /// </summary>
        /// <param name="bitmap">The <see cref="System.Drawing.Bitmap"/>.</param>
        public void SetBitmap(Bitmap bitmap)
        {
            //Check the whole palette and convert to 8bpp if the color amount goes above 256
            List<int> colors = new List<int>();
            for (int h = 0; h < bitmap.Height; h++)
            {
                for (int w = 0; w < bitmap.Width; w++)
                {
                    int col = bitmap.GetPixel(w, h).ToArgb();
                    if (!colors.Contains(col))
                        colors.Add(col);
                }
            }

            if (colors.Count > 16) PixelFormat = PDWPixelFormat.RGBA8bpp;

            if (colors.Count <= 256)
            {
                this.Bitmap = bitmap;
                UpdateData();
                return;
            }

            // Quantize the image to 256 colors
            using (var ms = new MemoryStream())
            {
                bitmap.Save(ms, ImageFormat.Png);
                ms.Position = 0;

                using (var image = new MagickImage(ms))
                {
                    image.ColorType = ColorType.TrueColorAlpha;

                    var settings = new QuantizeSettings()
                    {
                        Colors = 256,
                        DitherMethod = DitherMethod.FloydSteinberg,
                        ColorSpace = ColorSpace.RGB,
                    };
                    image.Quantize(settings);
                    image.ColorType = ColorType.PaletteAlpha;

                    using (var outStream = new MemoryStream())
                    {
                        image.Format = MagickFormat.Png;
                        image.Write(outStream);
                        outStream.Position = 0;

                        this.Bitmap = new Bitmap(outStream);
                    }
                }
            }

            UpdateData();
            //TODO: Make this return some enum as a result code (ie: too many colors, success, etc)
        }
    }
}
