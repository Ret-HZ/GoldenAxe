using ICSharpCode.SharpZipLib.Zip.Compression;
using System.IO;
using System.Linq;
using Yarhl.IO;

namespace AceUtils.CDI
{
    public class CDIFile
    {
        public CDIFile()
        {
            IsDummy = false;
        }

        public CDIFile(string name) : this()
        {
            Name = name;
        }

        /// <summary>
        /// File name.
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// The file is a dummy and has no data.
        /// </summary>
        public bool IsDummy { get; internal set; }

        /// <summary>
        /// The file is compressed with Deflate.
        /// </summary>
        public bool IsCompressed { get; private set; }

        public short Unknown1 { get; set; }

        public ushort Unknown2 { get; set; }

        /// <summary>
        /// File content.
        /// </summary>
        private byte[] Content { get; set; }



        /// <summary>
        /// Get the file name without extension.
        /// </summary>
        /// <returns>A <see cref="string"/>.</returns>
        public string GetNameWithoutExtension()
        {
            return Name.Split(".".ToCharArray(), 2)[0];
        }


        /// <summary>
        /// Gets the file name extension.
        /// </summary>
        /// <returns>A <see cref="string"/>.</returns>
        public string GetExtension()
        {
            return Name.Split(".".ToCharArray(), 2)[1];
        }


        /// <summary>
        /// Check if the file uses compression.
        /// </summary>
        internal void CheckForCompression()
        {
            if (Content.Length > 4)
            {
                byte[] deflateMagic = new byte[] { 0x44, 0x45, 0x46, 0x2E }; //DEF.
                IsCompressed = Content.Take(4).SequenceEqual(deflateMagic);
            }
        }


        /// <summary>
        /// Get the raw file contents.
        /// </summary>
        /// <returns>The raw file contents.</returns>
        internal byte[] GetContentRaw()
        {
            return Content;
        }


        /// <summary>
        /// Set the raw file contents. A compression check will be performed afterwards.
        /// </summary>
        /// <param name="rawContent">The raw file contents.</param>
        internal void SetContentRaw(byte[] rawContent)
        {
            Content = rawContent;
            CheckForCompression();
        }


        /// <summary>
        /// Get the file contents.
        /// </summary>
        /// <returns>The file contents.</returns>
        public byte[] GetContent()
        {
            if (IsDummy) return new byte[] { };
            if (IsCompressed) return Inflate(Content);
            return Content;
        }


        /// <summary>
        /// Sets the file contents.
        /// </summary>
        /// <param name="newContent">The new file contents.</param>
        public void SetContent(byte[] newContent)
        {
            if (IsCompressed) Content = Deflate(newContent);
            else Content = newContent;
        }


        /// <summary>
        /// Returns the content size.
        /// </summary>
        public int GetContentSize()
        {
            return GetContent().Length;
        }


        /// <summary>
        /// Returns the raw content size.
        /// </summary>
        public int GetContentSizeRaw()
        {
            if (IsDummy) return 0;
            return Content.Length;
        }


        /// <summary>
        /// Enables or disables compression for this file.
        /// </summary>
        public void SetCompression(bool useCompression)
        {
            if (useCompression == IsCompressed) return;
            if (useCompression) Content = Deflate(Content);
            else Content = Inflate(Content);
            IsCompressed = useCompression;
        }


        /// <summary>
        /// Compresses the data with Deflate and adds the compression header.
        /// </summary>
        /// <param name="data">The data to compress.</param>
        /// <returns>Compressed data.</returns>
        private byte[] Deflate(byte[] data)
        {
            try
            {
                int decompressedSize = data.Length;
                Deflater deflater = new Deflater(6);
                deflater.SetInput(data);
                byte[] compressedDataNoHeader = new byte[0xFFFFFFFF];
                int compressedSize = deflater.Deflate(compressedDataNoHeader);

                using (MemoryStream ms = new MemoryStream())
                {
                    using (BinaryWriter bw = new BinaryWriter(ms))
                    {
                        bw.Write(new byte[] { 0x44, 0x45, 0x46, 0x2E }); //DEF.
                        bw.Write(compressedSize);
                        bw.Write(decompressedSize);
                        bw.Write(0);
                        bw.Write(compressedDataNoHeader, 0, compressedSize);
                    }
                    return ms.ToArray();
                }
            }
            catch
            {
                return data;
            }
        }


        /// <summary>
        /// Decompresses the data with Inflate and removes the compression header.
        /// </summary>
        /// <param name="data">The data to decompress.</param>
        /// <returns>Decompressed data.</returns>
        private byte[] Inflate(byte[] data)
        {
            try
            {
                using (var ds = DataStreamFactory.FromArray(data, 0, data.Length))
                {
                    var reader = new DataReader(ds)
                    {
                        Endianness = EndiannessMode.LittleEndian,
                    };

                    string magic = reader.ReadString(4);
                    if (magic == "DEF.")
                    {
                        int compressedSizeNoPadding = reader.ReadInt32(); //This size includes the compression header
                        int decompressedSize = reader.ReadInt32();
                        reader.Stream.Seek(0x10);
                        byte[] compressedData = reader.ReadBytes(compressedSizeNoPadding - 0x10);
                        Inflater inf = new Inflater();
                        inf.SetInput(compressedData);
                        byte[] decompressedData = new byte[decompressedSize];
                        int written = inf.Inflate(decompressedData);
                        return decompressedData;
                    }
                    else
                    {
                        return data;
                    }
                }
            }
            catch
            {
                return data;
            }
        }
    }
}
