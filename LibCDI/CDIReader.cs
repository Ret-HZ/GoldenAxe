using System;
using System.Collections.Generic;
using System.IO;
using Yarhl.IO;

namespace LibCDI
{
    public static class CDIReader
    {
        /// <summary>
        /// Reads a CDI file.
        /// </summary>
        /// <param name="datastream">The <see cref="DataStream"/>.</param>
        /// <returns>A <see cref="CDI"/> object.</returns>
        private static CDI ReadCDI(CDIVersion version, DataStream datastream)
        {
            var reader = new DataReader(datastream)
            {
                Endianness = EndiannessMode.LittleEndian,
            };

            CDI regfile = new CDI();

            string magic = reader.ReadString(4);
            if (magic != "RGF.")
                throw new Exception("Invalid magic. Expected RGF.");

            int regfileSize = reader.ReadInt32();
            int ptrAudioSection = reader.ReadInt32();
            int folderAmount = reader.ReadInt32();

            reader.Stream.Position += 0x10;

            List<string> DummyFiles = new List<string>();
            if (version == CDIVersion.X)
                DummyFiles = CDIDummyFiles.X;
            else if (version == CDIVersion.X2)
                DummyFiles = CDIDummyFiles.X2;
            else if (version == CDIVersion.X_PROTOTYPE_20060816)
                DummyFiles = CDIDummyFiles.X_PROTOTYPE_20060816;
            else if (version == CDIVersion.X_PROTOTYPE_20060801)
                DummyFiles = CDIDummyFiles.X_PROTOTYPE_20060801;

            //Read folders
            for (int i = 0; i < folderAmount; i++)
            {
                CDIFolder folder = new CDIFolder();
                folder.Name = reader.ReadString(0xA).Trim('\0');
                if (version == CDIVersion.X || version == CDIVersion.X_PROTOTYPE_20060816 || version == CDIVersion.X_PROTOTYPE_20060801)
                {
                    folder.Unknown1 = reader.ReadUInt16();
                    folder.Unknown2 = reader.ReadUInt16();
                    folder.Unknown3 = reader.ReadUInt16();
                    reader.Stream.Position += 0x14; //Skip 1 and padding
                    folder.Unknown4 = reader.ReadUInt16();
                }

                ushort fileAmount = reader.ReadUInt16();
                uint ptrFileSection = reader.ReadUInt32();
                folder.AllocatedSpace = reader.ReadUInt32();

                //Read files
                if (fileAmount > 0)
                {
                    reader.Stream.PushToPosition(ptrFileSection);

                    for (int j = 0; j < fileAmount; j++)
                    {
                        CDIFile file = new CDIFile();
                        file.Name = reader.ReadString(0xC).Trim('\0');
                        file.Unknown1 = reader.ReadInt16();
                        file.Unknown2 = reader.ReadUInt16();

                        if (DummyFiles.Contains($"{folder.Name}/{file.Name}"))
                        {
                            file.IsDummy = true;
                        }
                        else
                        {
                            int fileSize = reader.ReadInt32() - 0x4;
                            file.Content = reader.ReadBytes(fileSize);
                        }

                        folder.Files.Add(file.Name, file);
                    }

                    reader.Stream.PopPosition();
                }

                regfile.Folders.Add(folder.Name, folder);
            }

            return regfile;
        }


        /// <summary>
        /// Reads a CDI file.
        /// </summary>
        /// <param name="fileBytes">The CDI file as byte array.</param>
        /// <param name="offset">The location in the array to start reading data from.</param>
        /// <param name="length">The number of bytes to read from the array.</param>
        /// <returns>A <see cref="CDI"/> object.</returns>
        public static CDI ReadCDI(CDIVersion version, byte[] fileBytes, int offset = 0, int length = 0)
        {
            if (length == 0) length = fileBytes.Length;
            using (var datastream = DataStreamFactory.FromArray(fileBytes, offset, length))
            {
                return ReadCDI(version, datastream);
            }
        }


        /// <summary>
        /// Reads a CDI file.
        /// </summary>
        /// <param name="stream">The CDI file as a <see cref="Stream"/>.</param>
        /// <returns>A <see cref="CDI"/> object.</returns>
        public static CDI ReadCDI(CDIVersion version, Stream stream)
        {
            using (var datastream = DataStreamFactory.FromStream(stream))
            {
                return ReadCDI(version, datastream);
            }
        }


        /// <summary>
        /// Reads a CDI file.
        /// </summary>
        /// <param name="path">The path to the CDI file.</param>
        /// <returns>A <see cref="CDI"/> object.</returns>
        public static CDI ReadCDI(CDIVersion version, string path)
        {
            using (var datastream = DataStreamFactory.FromFile(path, FileOpenMode.Read))
            {
                return ReadCDI(version, datastream);
            }
        }
    }
}
