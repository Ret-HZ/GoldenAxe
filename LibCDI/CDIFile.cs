using ICSharpCode.SharpZipLib.Zip.Compression;
using System.IO;
using Yarhl.IO;

namespace LibCDI
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

        public short Unknown1 { get; set; }

        public ushort Unknown2 { get; set; }

        /// <summary>
        /// File content.
        /// </summary>
        internal byte[] Content { get; set; }



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
        /// Get the file contents.
        /// </summary>
        /// <returns>The file contents.</returns>
        public byte[] GetContent()
        {
            if (IsDummy) return new byte[] { };

            try
            {
                using (var ds = DataStreamFactory.FromArray(Content, 0, Content.Length))
                {
                    var reader = new DataReader(ds)
                    {
                        Endianness = EndiannessMode.LittleEndian,
                    };

                    string magic = reader.ReadString(4);
                    if (magic == "DEF.")
                    {
                        //Decompress
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
                        return Content;
                    }
                }
            }
            catch
            {
                return Content;
            }
        }


        /// <summary>
        /// Sets the file contents.
        /// </summary>
        /// <param name="newContent">The new file contents.</param>
        public void SetContent(byte[] newContent)
        {
            try
            {
                using (var ds = DataStreamFactory.FromArray(Content, 0, Content.Length))
                {
                    var reader = new DataReader(ds)
                    {
                        Endianness = EndiannessMode.LittleEndian,
                    };

                    string magic = reader.ReadString(4);
                    if (magic == "DEF.")
                    {
                        //Compress
                        int decompressedSize = newContent.Length;
                        Deflater deflater = new Deflater(6);
                        deflater.SetInput(newContent);
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
                            Content = ms.ToArray();
                        }
                        
                    }
                    else
                    {
                        Content = newContent;
                    }
                }
            }
            catch
            {
                Content = newContent;
            }
        }


        /// <summary>
        /// Returns the content size.
        /// </summary>
        public int GetContentSize()
        {
            return GetContent().Length;
        }
    }
}
