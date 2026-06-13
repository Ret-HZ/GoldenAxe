using System.Collections.Generic;
using System.IO;
using System.Text;
using Yarhl.IO;

namespace AceUtils.CDI
{
    public static class CDIWriter
    {
        /// <summary>
        /// Writes a <see cref="CDI"/> to a <see cref="DataStream"/>.
        /// </summary>
        /// <param name="regfile">The <see cref="CDI"/> to write.</param>
        /// <param name="datastream">The <see cref="DataStream"/> to write to.</param>
        private static void WriteCDI(CDI regfile, DataStream datastream)
        {
            var writer = new DataWriter(datastream)
            {
                Endianness = EndiannessMode.LittleEndian,
                DefaultEncoding = Encoding.UTF8,
            };

            writer.Write("RGF.", false);
            writer.Write(0); //Placeholder file size
            writer.Write(0); //Placeholder audio section pointer
            writer.Write(regfile.Folders.Count);
            writer.Write(1);
            writer.WriteTimes(0x00, 0xC);

            //Folders
            Dictionary<string, int> fileSectionPointers = new Dictionary<string, int>();
            foreach (CDIFolder folder in regfile.GetFolders())
            {
                writer.Write(folder.Name, 0xA, false, null);

                if (regfile.Version == Enum.CDIVersion.X)
                {
                    writer.Write(folder.Unknown1);
                    writer.Write(folder.Unknown2);
                    writer.Write(folder.Unknown3);
                    writer.Write(1);
                    writer.WriteTimes(0x00, 0x10);
                    writer.Write(folder.Unknown4);
                }

                writer.Write((ushort)folder.Files.Count);
                fileSectionPointers.Add(folder.Name, (int)writer.Stream.Position);
                writer.Write(0);

                //Update allocated space
                if (folder.Files.Count > 0)
                {
                    int totalFileSize = 0;
                    foreach (CDIFile file in folder.GetFiles())
                    {
                        totalFileSize += file.GetContentSizeRaw() + 0x14;
                    }
                    totalFileSize += 0x20; //STED folder end
                    totalFileSize += (2048 - (totalFileSize % 2048)); //Pad to 2048 bytes
                    folder.AllocatedSpace = (uint)totalFileSize;
                }
                else
                {
                    folder.AllocatedSpace = 0;
                }

                writer.Write(folder.AllocatedSpace);
            }

            if (regfile.Version == Enum.CDIVersion.X2)
            {
                writer.WriteTimes(0x00, 0x28000 - writer.Stream.Position);
            }
            else
            {
                writer.WriteTimes(0x00, 0x20000 - writer.Stream.Position);
            }

            //Files
            foreach (CDIFolder folder in regfile.GetFolders())
            {
                int ptrSection = (int)writer.Stream.Position;
                writer.Stream.PushToPosition(fileSectionPointers[folder.Name]);
                writer.Write(ptrSection);
                writer.Stream.PopPosition();

                foreach (CDIFile file in folder.GetFiles())
                {
                    writer.Write(file.Name, 0xC, false, null);
                    writer.Write(file.Unknown1);
                    writer.Write(file.Unknown2);

                    if (!file.IsDummy)
                    {
                        int ptrFileSize = (int)writer.Stream.Position;
                        writer.Write(0); //Placeholder file size
                        writer.Write(file.GetContentRaw());
                        writer.WritePadding(0x00, 0x4);
                        int fileSize = (int)writer.Stream.Position - (ptrFileSize);
                        writer.Stream.PushToPosition(ptrFileSize);
                        writer.Write(fileSize);
                        writer.Stream.PopPosition();
                    }
                }

                if (folder.Files.Count > 0)
                {
                    writer.Write("STED", false);
                    int usedSpace = (int)(writer.Stream.Position - ptrSection);
                    writer.WriteTimes(0x00, (int)(folder.AllocatedSpace - usedSpace));
                }
            }

            //Audio section
            int ptrAudioSection = (int)writer.Stream.Position;
            writer.Write(regfile.AudioData);
            writer.Stream.Seek(0x8);
            writer.Write(ptrAudioSection);

            //REGFILE size
            writer.Stream.Seek(0, SeekMode.End);
            int regfileSize = (int)writer.Stream.Position;
            writer.Stream.Seek(0x4);
            writer.Write(regfileSize);
        }


        /// <summary>
        /// Writes a <see cref="CDI"/> to a file.
        /// </summary>
        /// <param name="regfile">The <see cref="CDI"/> to write.</param>
        /// <param name="path">The destination file path.</param>
        public static void WriteCDIToFile(CDI regfile, string path)
        {
            using (var datastream = DataStreamFactory.FromFile(path, FileOpenMode.Write))
            {
                WriteCDI(regfile, datastream);
            }
        }


        /// <summary>
        /// Writes a <see cref="CDI"/> to a <see cref="Stream"/>.
        /// </summary>
        /// <param name="regfile">The <see cref="CDI"/> to write.</param>
        public static Stream WriteCDIToStream(CDI regfile)
        {
            MemoryStream stream = new MemoryStream();
            DataStream tempds = DataStreamFactory.FromMemory();
            WriteCDI(regfile, tempds);
            tempds.WriteTo(stream);
            return stream;
        }


        /// <summary>
        /// Writes a <see cref="CDI"/> to a byte array.
        /// </summary>
        /// <param name="regfile">The <see cref="CDI"/> to write.</param>
        public static byte[] WriteCDIToArray(CDI regfile)
        {
            DataStream tempds = DataStreamFactory.FromMemory();
            WriteCDI(regfile, tempds);
            return tempds.ToArray();
        }
    }
}
