using AceUtils.PDW.Enum;
using System.Collections.Generic;
using System.Drawing;

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

            //Convert the bitmap to 8bpp
            Bitmap copy = new Bitmap(bitmap); //Copy to fix Clone()
            Bitmap converted8bppNoAlpha = copy.Clone(new Rectangle(0, 0, copy.Width, copy.Height), System.Drawing.Imaging.PixelFormat.Format8bppIndexed); //Converted palette to 8bpp but alpha is lost
            Bitmap convertedWithAlpha = converted8bppNoAlpha.Clone(new Rectangle(0, 0, converted8bppNoAlpha.Width, converted8bppNoAlpha.Height), System.Drawing.Imaging.PixelFormat.Format32bppArgb); //Post 8bpp conversion for editing since indexed is locked

            //Overwrite the alpha values for each pixel with the ones in the original bitmap
            for (int h = 0; h < bitmap.Height; h++)
            {
                for (int w = 0; w < bitmap.Width; w++)
                {
                    byte originalAlpha = bitmap.GetPixel(w, h).A;
                    Color currentColor = convertedWithAlpha.GetPixel(w, h);
                    convertedWithAlpha.SetPixel(w, h, Color.FromArgb(originalAlpha, currentColor));
                }
            }

            this.Bitmap = convertedWithAlpha;
            UpdateData();
            //TODO: Make this return some enum as a result code (ie: too many colors, success, etc)
        }
    }
}
