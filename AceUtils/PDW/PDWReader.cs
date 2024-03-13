using AceUtils.PDW.Enum;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Yarhl.IO;

namespace AceUtils.PDW
{
    public static class PDWReader
    {
        /// <summary>
        /// Reads a PDW file.
        /// </summary>
        /// <param name="datastream">The <see cref="DataStream"/>.</param>
        /// <returns>A <see cref="PDW"/> object.</returns>
        private static PDW ReadPDW(DataStream datastream)
        {
            var reader = new DataReader(datastream)
            {
                Endianness = EndiannessMode.LittleEndian,
            };

            PDW pdw = new PDW();

            string magic = reader.ReadString(4);
            if (magic != "PDW.")
                throw new Exception("Invalid magic. Expected PDW.");

            int fileSize = reader.ReadInt32();
            pdw.TextureAmount = reader.ReadInt32();
            reader.ReadInt32();
            int ptrInfoTable = reader.ReadInt32();
            reader.Stream.Seek(ptrInfoTable);

            pdw.PixelDataLength = reader.ReadInt32(); //this may not be correct (?) //ACX - FLY04/FLY04D0.PDW

            pdw.Width = reader.ReadUInt16();
            pdw.Height = reader.ReadUInt16();

            pdw.PixelFormat = (PDWPixelFormat)reader.ReadByte();
            pdw.Flag_0x09 = reader.ReadByte();
            pdw.Unk_0x0A = reader.ReadUInt16();
            pdw.Unk_0x0C = reader.ReadInt32();

            if (pdw.PixelFormat == PDWPixelFormat.RGBA8bpp)
            {
                byte[] swizzledTextureData = reader.ReadBytes(pdw.Width * pdw.Height);
                pdw.PixelData = Unswizzle(swizzledTextureData, pdw.Width, pdw.Height, 8);
                pdw.PaletteData = reader.ReadBytes(0x400);
            }
            else if (pdw.PixelFormat == PDWPixelFormat.RGBA4bpp)
            {
                byte[] swizzledTextureData = reader.ReadBytes((pdw.Width * pdw.Height) / 2);
                byte[] unswizzledTextureData = Unswizzle(swizzledTextureData, pdw.Width, pdw.Height, 4);
                pdw.PixelData = new byte[pdw.Width * pdw.Height];
                for ( int i = 0; i < unswizzledTextureData.Length; i++)
                {
                    byte b = unswizzledTextureData[i];
                    byte firstPixel = (byte)(b & 0x0F);
                    byte secondPixel = (byte)(b >> 4);

                    pdw.PixelData[i*2] = firstPixel;
                    pdw.PixelData[i*2+1] = secondPixel;
                }
                pdw.PaletteData = reader.ReadBytes(0x40);
            }
            else
            {
                throw new Exception($"Unknown PDW pixel format '{pdw.PixelFormat}'.");
            }

            pdw.Bitmap = new Bitmap(pdw.Width, pdw.Height, PixelFormat.Format32bppArgb);

            for (int i = 0; i < pdw.PixelData.Length; i++)
            {
                byte pixel = pdw.PixelData[i];
                byte r = pdw.PaletteData[pixel*4+0];
                byte g = pdw.PaletteData[pixel*4+1];
                byte b = pdw.PaletteData[pixel*4+2];
                byte a = pdw.PaletteData[pixel*4+3];

                int x = i % pdw.Height;
                int y = i / pdw.Width;

                pdw.Bitmap.SetPixel(x, y, Color.FromArgb(a, r, g, b));
            }

            return pdw;
        }


        /// <summary>
        /// Reads a PDW file.
        /// </summary>
        /// <param name="fileBytes">The PDW file as byte array.</param>
        /// <param name="offset">The location in the array to start reading data from.</param>
        /// <param name="length">The number of bytes to read from the array.</param>
        /// <returns>A <see cref="PDW"/> object.</returns>
        public static PDW ReadPDW(byte[] fileBytes, int offset = 0, int length = 0)
        {
            if (length == 0) length = fileBytes.Length;
            using (var datastream = DataStreamFactory.FromArray(fileBytes, offset, length))
            {
                return ReadPDW(datastream);
            }
        }


        /// <summary>
        /// Reads a PDW file.
        /// </summary>
        /// <param name="stream">The PDW file as <see cref="Stream"/>.</param>
        /// <returns>A <see cref="PDW"/> object.</returns>
        public static PDW ReadPDW(Stream stream)
        {
            using (var datastream = DataStreamFactory.FromStream(stream))
            {
                return ReadPDW(datastream);
            }
        }


        /// <summary>
        /// Reads a PDW file.
        /// </summary>
        /// <param name="path">The path to the PDW file.</param>
        /// <returns>A <see cref="PDW"/> object.</returns>
        public static PDW ReadPDW(string path)
        {
            using (var datastream = DataStreamFactory.FromFile(path, FileOpenMode.Read))
            {
                return ReadPDW(datastream);
            }
        }


        // https://github.com/AntonioDePau/psp_xettex_viewer/blob/4ff07318492f6a6ae80fbbdec484b6e3afa4f7d2/ConsoleProject/src/services/SwizzleService.cs#L41
        private static byte[] Unswizzle(byte[] source, int width, int height, int bitsPerPixel)
        {
            if (width < 16 || height < 16)
            {
                return source;
            }

            int destinationOffset = 0;

            width = (width * bitsPerPixel) >> 3;

            byte[] destination = new byte[width * height];

            int rowblocks = (width / 16);

            int magicNumber = 8;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int blockX = x / 16;
                    int blockY = y / magicNumber;

                    int blockIndex = blockX + ((blockY) * rowblocks);
                    int blockAddress = blockIndex * 16 * magicNumber;
                    int offset = blockAddress + (x - blockX * 16) + ((y - blockY * magicNumber) * 16);
                    destination[destinationOffset] = source[offset];
                    destinationOffset++;
                }
            }

            return destination;
        }
    }
}
