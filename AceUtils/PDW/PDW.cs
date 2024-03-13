using AceUtils.PDW.Enum;
using System.Drawing;

namespace AceUtils.PDW
{
    public class PDW
    {
        internal PDW()
        {

        }

        /// <summary>
        /// File name.
        /// </summary>
        public string Name;

        /// <summary>
        /// Amount of textures in the PDW.
        /// </summary>
        public int TextureAmount;

        /// <summary>
        /// Total amount of pixels.
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
        /// Gets the texture as a <see cref="System.Drawing.Bitmap"./>
        /// </summary>
        /// <returns>A <see cref="System.Drawing.Bitmap"/>.</returns>
        public Bitmap GetBitmap()
        {
            return new Bitmap(Bitmap);
        }
    }
}
