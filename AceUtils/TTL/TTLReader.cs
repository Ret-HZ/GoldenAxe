using System;
using System.IO;
using System.Text;
using Yarhl.IO;

namespace AceUtils.TTL
{
    public static class TTLReader
    {
        /// <summary>
        /// Reads a message TTL file.
        /// </summary>
        /// <param name="datastream">The <see cref="DataStream"/>.</param>
        /// <returns>A <see cref="TTL"/> object.</returns>
        private static TTL ReadTTL(DataStream datastream)
        {
            var reader = new DataReader(datastream)
            {
                Endianness = EndiannessMode.LittleEndian,
                DefaultEncoding = Encoding.Unicode,
            };

            TTL ttl = new TTL();

            int magic = reader.ReadInt32();
            if (magic == 0x2E54544C) ttl.version = TTL.Version.X;
            else if (magic == 0x2E74746C) ttl.version = TTL.Version.X2;

            if (ttl.version == TTL.Version.X)
            {
                reader.Stream.PushToPosition(0xC);
            }
            else if (ttl.version == TTL.Version.X2)
            {
                reader.Stream.PushToPosition(0x10);
            }

            while (true)
            {
                uint characterNumeric = reader.ReadUInt16();
                if (characterNumeric == 0) break;
                char character = Convert.ToChar(characterNumeric);
                if (!ttl.CharacterTable.ContainsKey(characterNumeric))
                {
                    ttl.CharacterTable.Add(characterNumeric, character);
                    ttl.CharacterTableReverse.Add(character, characterNumeric);
                }
                ttl.IdList.Add(characterNumeric);
            }

            return ttl;
        }


        /// <summary>
        /// Reads a message TTL file.
        /// </summary>
        /// <param name="fileBytes">The TTL file as byte array.</param>
        /// <param name="offset">The location in the array to start reading data from.</param>
        /// <param name="length">The number of bytes to read from the array.</param>
        /// <returns>A <see cref="TTL"/> object.</returns>
        public static TTL ReadTTL(byte[] fileBytes, int offset = 0, int length = 0)
        {
            if (length == 0) length = fileBytes.Length;
            using (var datastream = DataStreamFactory.FromArray(fileBytes, offset, length))
            {
                return ReadTTL(datastream);
            }
        }


        /// <summary>
        /// Reads a message TTL file.
        /// </summary>
        /// <param name="stream">The TTL file as <see cref="Stream"/>.</param>
        /// <returns>A <see cref="TTL"/> object.</returns>
        public static TTL ReadTTL(Stream stream)
        {
            using (var datastream = DataStreamFactory.FromStream(stream))
            {
                return ReadTTL(datastream);
            }
        }


        /// <summary>
        /// Reads a message TTL file.
        /// </summary>
        /// <param name="path">The path to the TTL file.</param>
        /// <returns>A <see cref="TTL"/> object.</returns>
        public static TTL ReadTTL(string path)
        {
            using (var datastream = DataStreamFactory.FromFile(path, FileOpenMode.Read))
            {
                return ReadTTL(datastream);
            }
        }
    }
}
