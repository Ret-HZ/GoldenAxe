using AceUtils.PDW.Enum;
using System;
using System.Collections.Generic;
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

            string magic = reader.ReadString(4);
            if (magic != "PDW.")
                throw new Exception("Invalid magic. Expected PDW.");

            PDW pdw = new PDW();

            int fileSize = reader.ReadInt32();
            int textureCount = reader.ReadInt32();
            reader.ReadInt32(); // Padding

            // Texture offset table
            List<int> textureOffsets = new List<int>();
            for (int i = 0; i < textureCount; i++)
            {
                textureOffsets.Add(reader.ReadInt32());
            }

            // Read textures
            foreach (int offset in textureOffsets)
            {
                reader.Stream.Seek(offset);
                pdw.Textures.Add(ReadTexture(reader));
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


        /// <summary>
        /// Reads a <see cref="PDWTexture"/> from the <see cref="DataStream"/>.
        /// </summary>
        /// <param name="reader">The <see cref="DataReader"/>.</param>
        /// <returns>A <see cref="PDWTexture"/>.</returns>
        /// <exception cref="Exception">The pixel format version is not implemented.</exception>
        private static PDWTexture ReadTexture(DataReader reader)
        {
            PDWTexture texture = new PDWTexture();

            texture.PixelDataLength = reader.ReadInt32();

            texture.Width = reader.ReadUInt16();
            texture.Height = reader.ReadUInt16();

            texture.PixelFormat = (PDWPixelFormat)reader.ReadByte();
            texture.Flag_0x09 = reader.ReadByte();
            texture.Unk_0x0A = reader.ReadUInt16();
            texture.Unk_0x0C = reader.ReadInt32();

            if (texture.PixelFormat == PDWPixelFormat.RGBA8bpp)
            {
                byte[] swizzledTextureData = reader.ReadBytes(texture.Width * texture.Height);
                texture.PixelData = Unswizzle(swizzledTextureData, texture.Width, texture.Height, 8);
                texture.PaletteData = reader.ReadBytes(0x400);
            }
            else if (texture.PixelFormat == PDWPixelFormat.RGBA4bpp)
            {
                byte[] swizzledTextureData = reader.ReadBytes((texture.Width * texture.Height) / 2);
                byte[] unswizzledTextureData = Unswizzle(swizzledTextureData, texture.Width, texture.Height, 4);
                texture.PixelData = new byte[texture.Width * texture.Height];
                for (int i = 0; i < unswizzledTextureData.Length; i++)
                {
                    byte b = unswizzledTextureData[i];
                    byte firstPixel = (byte)(b & 0x0F);
                    byte secondPixel = (byte)(b >> 4);

                    texture.PixelData[i * 2] = firstPixel;
                    texture.PixelData[i * 2 + 1] = secondPixel;
                }
                texture.PaletteData = reader.ReadBytes(0x40);
            }
            else
            {
                throw new Exception($"Unknown PDWTexture pixel format '{texture.PixelFormat}'.");
            }

            texture.Bitmap = new Bitmap(texture.Width, texture.Height, PixelFormat.Format32bppArgb);

            for (int i = 0; i < texture.PixelData.Length; i++)
            {
                byte pixel = texture.PixelData[i];
                byte r = texture.PaletteData[pixel * 4 + 0];
                byte g = texture.PaletteData[pixel * 4 + 1];
                byte b = texture.PaletteData[pixel * 4 + 2];
                byte a = texture.PaletteData[pixel * 4 + 3];

                int x = i % texture.Width;
                int y = i / texture.Width;

                texture.Bitmap.SetPixel(x, y, Color.FromArgb(a, r, g, b));
            }

            return texture;
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
