using AceUtils.PDW.Enum;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using Yarhl.IO;

namespace AceUtils.PDW
{
    public static class PDWWriter
    {
        /// <summary>
        /// Writes a <see cref="PDW"/> to a <see cref="DataStream"/>.
        /// </summary>
        /// <param name="pdw">The <see cref="PDW"/> to write.</param>
        /// <param name="datastream">The <see cref="DataStream"/> to write to.</param>
        private static void WritePDW(PDW pdw, DataStream datastream)
        {
            var writer = new DataWriter(datastream)
            {
                Endianness = EndiannessMode.LittleEndian,
                DefaultEncoding = Encoding.UTF8,
            };

            writer.Write("PDW.", false);
            writer.Write(0); //Placeholder filesize
            writer.Write(pdw.TextureAmount);
            writer.Write(0);
            int ptrTextureOffsetTable = (int)writer.Stream.Position;
            writer.WriteTimes(0x00, 0x30); //Placeholder texture offset table

            // Texture data
            List<int> textureOffsets = new List<int>();
            foreach (PDWTexture texture in pdw.Textures)
            {
                textureOffsets.Add((int)writer.Stream.Position);
                WriteTexture(writer, texture);
            }

            // Update header
            int fileSize = (int)writer.Stream.Position;
            writer.Stream.Seek(0x4);
            writer.Write(fileSize);

            // Texture offset table
            writer.Stream.Seek(ptrTextureOffsetTable);
            foreach (int offset in textureOffsets)
            {
                writer.Write(offset);
            }
        }


        /// <summary>
        /// Writes a <see cref="PDW"/> to a file.
        /// </summary>
        /// <param name="pdw">The <see cref="PDW"/> to write.</param>
        /// <param name="path">The destination file path.</param>
        public static void WritePDWToFile(PDW pdw, string path)
        {
            using (var datastream = DataStreamFactory.FromFile(path, FileOpenMode.Write))
            {
                WritePDW(pdw, datastream);
            }
        }


        /// <summary>
        /// Writes a <see cref="PDW"/> to a <see cref="Stream"/>.
        /// </summary>
        /// <param name="pdw">The <see cref="PDW"/> to write.</param>
        public static Stream WritePDWToStream(PDW pdw)
        {
            MemoryStream stream = new MemoryStream();
            DataStream tempds = DataStreamFactory.FromMemory();
            WritePDW(pdw, tempds);
            tempds.WriteTo(stream);
            return stream;
        }


        /// <summary>
        /// Writes a <see cref="PDW"/> to a byte array.
        /// </summary>
        /// <param name="pdw">The <see cref="PDW"/> to write.</param>
        public static byte[] WritePDWToArray(PDW pdw)
        {
            DataStream tempds = DataStreamFactory.FromMemory();
            WritePDW(pdw, tempds);
            return tempds.ToArray();
        }


        /// <summary>
        /// Writes a <see cref="PDWTexture"/> to the <see cref="DataStream"/>.
        /// </summary>
        /// <param name="writer">The <see cref="DataWriter"/>.</param>
        /// <param name="texture">The <see cref="PDWTexture"/> to write.</param>
        private static void WriteTexture(DataWriter writer, PDWTexture texture)
        {
            writer.Write(texture.Height * texture.Width); //This should be halved when writing 4bpp
            writer.Write(texture.Width);
            writer.Write(texture.Height);
            //writer.Write((byte)pdw.PixelFormat);
            writer.Write((byte)PDWPixelFormat.RGBA8bpp); //4bpp not supported yet, always write as 8bpp
            writer.Write(texture.Flag_0x09);
            writer.Write(texture.Unk_0x0A);
            writer.Write(texture.Unk_0x0C);

            //Pixel data
            //TODO: 4bpp support
            List<Color> colors = new List<Color>();
            byte[] pixelsUnswizzled = new byte[texture.Width * texture.Height];
            int pixelIndex = 0;
            for (int h = 0; h < texture.Height; h++)
            {
                for (int w = 0; w < texture.Width; w++)
                {
                    Color col = texture.Bitmap.GetPixel(w, h);
                    if (!colors.Contains(col)) colors.Add(col);
                    pixelsUnswizzled[pixelIndex] = (byte)colors.IndexOf(col);
                    pixelIndex++;
                }
            }
            writer.Write(Swizzle(pixelsUnswizzled, texture.Width, texture.Height, 8));

            //Palette
            foreach (Color col in colors)
            {
                writer.Write(col.R);
                writer.Write(col.G);
                writer.Write(col.B);
                writer.Write(col.A);
            }
            //Fill remaining color slots
            for (int i = colors.Count; i < 256; i++)
            {
                writer.Write(0); //R0 G0 B0 A0
            }
        }


        // https://github.com/AntonioDePau/psp_xettex_viewer/blob/4ff07318492f6a6ae80fbbdec484b6e3afa4f7d2/ConsoleProject/src/services/SwizzleService.cs#L9
        public static byte[] Swizzle(byte[] source, int width, int height, int bitsPerPixel)
        {
            if (width < 16 || height < 16)
            {
                return source;
            }

            int offset = 0;

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

                    int destinationOffset = blockAddress + (x - blockX * 16) + ((y - blockY * magicNumber) * 16);

                    destination[destinationOffset] = source[offset];
                    offset++;
                }
            }

            return destination;
        }
    }
}
