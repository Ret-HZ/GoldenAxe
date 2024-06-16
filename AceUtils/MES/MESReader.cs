using System.IO;
using Yarhl.IO;

namespace AceUtils.MES
{
    public static class MESReader
    {
        /// <summary>
        /// Reads a message MES file.
        /// </summary>
        /// <param name="datastream">The <see cref="DataStream"/>.</param>
        /// <returns>A <see cref="MES"/> object.</returns>
        private static MES ReadMES(DataStream datastream, TTL.TTL ttl)
        {
            var reader = new DataReader(datastream)
            {
                Endianness = EndiannessMode.LittleEndian,
            };

            MES mes = new MES();

            int magic = reader.ReadInt32();
            int headerUnknown0x4 = reader.ReadInt32(); //Version?
            int stringTableLength = reader.ReadInt32(); //Usually 8600

            for (uint i = 0; i < stringTableLength; i++)
            {
                int ptrString = reader.ReadInt32();
                if (ptrString == 0) continue;
                reader.Stream.PushToPosition(ptrString);
                Message msg = ReadMessage(reader, ttl);
                reader.Stream.PopPosition();
                System.Diagnostics.Trace.WriteLine($"[{i}] - {msg.Text}"); //debug
                mes.Messages.Add(i, msg);
            }

            return mes;
        }


        /// <summary>
        /// Reads a message MES file.
        /// </summary>
        /// <param name="fileBytes">The MES file as byte array.</param>
        /// <param name="offset">The location in the array to start reading data from.</param>
        /// <param name="length">The number of bytes to read from the array.</param>
        /// <returns>A <see cref="MES"/> object.</returns>
        public static MES ReadMES(byte[] fileBytes, TTL.TTL ttl, int offset = 0, int length = 0)
        {
            if (length == 0) length = fileBytes.Length;
            using (var datastream = DataStreamFactory.FromArray(fileBytes, offset, length))
            {
                return ReadMES(datastream, ttl);
            }
        }


        /// <summary>
        /// Reads a message MES file.
        /// </summary>
        /// <param name="stream">The MES file as <see cref="Stream"/>.</param>
        /// <returns>A <see cref="MES"/> object.</returns>
        public static MES ReadMES(Stream stream, TTL.TTL ttl)
        {
            using (var datastream = DataStreamFactory.FromStream(stream))
            {
                return ReadMES(datastream, ttl);
            }
        }


        /// <summary>
        /// Reads a message MES file.
        /// </summary>
        /// <param name="path">The path to the MES file.</param>
        /// <returns>A <see cref="MES"/> object.</returns>
        public static MES ReadMES(string path, TTL.TTL ttl)
        {
            using (var datastream = DataStreamFactory.FromFile(path, FileOpenMode.Read))
            {
                return ReadMES(datastream, ttl);
            }
        }


        private static Message ReadMessage(DataReader reader, TTL.TTL ttl)
        {
            Message msg = new Message();

            msg.SpeakerID = reader.ReadUInt16();
            ushort lineCount = reader.ReadUInt16();
            msg.Unk0x4 = reader.ReadUInt16();
            msg.Duration = reader.ReadUInt16();


            string text = "";

            for (int i = 0; i < lineCount; i++)
            {
                ushort characterCount = reader.ReadUInt16();
                while(true)
                {
                    uint character = reader.ReadUInt16();

                    switch (character)
                    {
                        //Check control characters

                        //End
                        case 65535: break;

                        //New line
                        case 65534:
                            {
                                text += "\n";
                                break;
                            }

                        //Text color. Followed by RGB values
                        case 65530:
                            {
                                byte r = (byte)reader.ReadUInt16();
                                byte g = (byte)reader.ReadUInt16();
                                byte b = (byte)reader.ReadUInt16();
                                text += $"<color_{r.ToString("X2")}{g.ToString("X2")}{b.ToString("X2")}>";
                                continue;
                            }

                        //Symbols. Used to display PS buttons. Followed by the ID of the symbol itself
                        case 65524:
                            {
                                uint psButtonID = reader.ReadUInt16();
                                text += $"<symbol_{psButtonID}>";
                                continue;
                            }

                        //Delay. Used to introduce wait time between lines. The next line wont appear until the current message duration is over.
                        //The line will use the duration established by this special character. Speaker ID and other attributes will be the same as the message.
                        case 65522:
                            {
                                ushort duration = reader.ReadUInt16();
                                ushort unk2 = reader.ReadUInt16();
                                text += $"<delay_{duration},{unk2}>";
                                continue;
                            }

                        default:
                            {
                                char chara = ttl.CharacterTable[ttl.IdList[(int)character]];
                                text += chara;
                                continue;
                            }
                    }

                    break;
                }
            }

            msg.Text = text;
            return msg;
        }
    }
}
