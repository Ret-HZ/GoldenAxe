using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Yarhl.IO;

namespace AceUtils.MES
{
    public static class MESWriter
    {
        /// <summary>
        /// Writes a <see cref="MES"/> to a <see cref="DataStream"/>.
        /// </summary>
        /// <param name="mes">The <see cref="MES"/> to write.</param>
        /// <param name="datastream">The <see cref="DataStream"/> to write to.</param>
        private static void WriteMES(MES mes, TTL.TTL ttl, DataStream datastream)
        {
            var writer = new DataWriter(datastream)
            {
                Endianness = EndiannessMode.LittleEndian,
                DefaultEncoding = Encoding.UTF8,
            };

            writer.Write(".MES", false);
            writer.Write(1);
            writer.Write(8600);

            writer.WriteTimes(0x00, 8600 * 4);

            foreach (KeyValuePair<uint, Message> kvp in mes.Messages)
            {
                int ptrMessage = (int)writer.Stream.Position;
                Message msg = kvp.Value;
                string[] lines = msg.Text.Split('\n');

                writer.Write(msg.SpeakerID);
                writer.Write((ushort)lines.Length);
                writer.Write(msg.Unk0x4);
                writer.Write(msg.Duration);

                int lineIndex = 1;
                foreach(string line in lines)
                {
                    writer.Write((ushort)line.Length);

                    bool inKeyword = false;
                    string currentKeyword = "";
                    foreach (char c in line)
                    {
                        //Check control characters
                        if (c == '<')
                        {
                            inKeyword = true;
                            currentKeyword = "";
                        }
                        else if (c == '>')
                        {
                            inKeyword = false;

                            //Color
                            if (currentKeyword.Contains("color"))
                            {
                                string rgbHex = currentKeyword.Split('_')[1];
                                var colors = Enumerable.Range(0, rgbHex.Length / 2).Select(i => rgbHex.Substring(i * 2, 2));
                                writer.Write((ushort)65530); //Control character value
                                writer.Write(Convert.ToUInt16(colors.ElementAt(0), 16)); //R
                                writer.Write(Convert.ToUInt16(colors.ElementAt(1), 16)); //G
                                writer.Write(Convert.ToUInt16(colors.ElementAt(2), 16)); //B
                            }

                            //Symbol
                            else if (currentKeyword.Contains("symbol"))
                            {
                                string buttonStr = currentKeyword.Split('_')[1];
                                writer.Write((ushort)65524); //Control character value
                                writer.Write(ushort.Parse(buttonStr)); //TODO: Replace this with actual button names or an enum
                            }

                            //Delay
                            else if (currentKeyword.Contains("delay"))
                            {
                                string unkValuesStr = currentKeyword.Split('_')[1];
                                string duration = unkValuesStr.Split(',')[0];
                                string unkValue2 = unkValuesStr.Split(',')[1];
                                writer.Write((ushort)65522); //Control character value
                                writer.Write(ushort.Parse(duration));
                                writer.Write(ushort.Parse(unkValue2));
                            }

                            currentKeyword = "";
                        }
                        else if (inKeyword)
                        {
                            currentKeyword += c;
                        }
                        else
                        {
                            ushort charIndex = (ushort)ttl.IdList.IndexOf(ttl.CharacterTableReverse[c]);
                            writer.Write(charIndex);
                        }
                    }
                    if (lineIndex < lines.Length)
                        writer.Write((ushort)65534); //End of line
                    
                    lineIndex++;
                }
                writer.Write((ushort)65535); //End of message

                writer.Stream.PushToPosition(0xC + kvp.Key * 4);
                writer.Write(ptrMessage);
                writer.Stream.PopPosition();
            }
        }


        /// <summary>
        /// Writes a <see cref="MES"/> to a file.
        /// </summary>
        /// <param name="mes">The <see cref="MES"/> to write.</param>
        /// <param name="path">The destination file path.</param>
        public static void WriteMESToFile(MES mes, TTL.TTL ttl, string path)
        {
            using (var datastream = DataStreamFactory.FromFile(path, FileOpenMode.Write))
            {
                WriteMES(mes, ttl, datastream);
            }
        }


        /// <summary>
        /// Writes a <see cref="MES"/> to a <see cref="Stream"/>.
        /// </summary>
        /// <param name="mes">The <see cref="MES"/> to write.</param>
        public static Stream WriteMESToStream(MES mes, TTL.TTL ttl)
        {
            MemoryStream stream = new MemoryStream();
            DataStream tempds = DataStreamFactory.FromMemory();
            WriteMES(mes, ttl, tempds);
            tempds.WriteTo(stream);
            return stream;
        }


        /// <summary>
        /// Writes a <see cref="MES"/> to a byte array.
        /// </summary>
        /// <param name="mes">The <see cref="MES"/> to write.</param>
        public static byte[] WriteMESToArray(MES mes, TTL.TTL ttl)
        {
            DataStream tempds = DataStreamFactory.FromMemory();
            WriteMES(mes, ttl, tempds);
            return tempds.ToArray();
        }
    }
}
