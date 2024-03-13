using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Yarhl.IO;

namespace LibMES
{
    public static class MESReader
    {
        /// <summary>
        /// Reads a message MES file.
        /// </summary>
        /// <param name="datastream"></param>
        /// <returns>A MES object.</returns>
        private static MES ReadMES(DataStream datastream, TTL ttl)
        {
            var reader = new DataReader(datastream)
            {
                Endianness = EndiannessMode.LittleEndian,
            };

            MES mes = new MES();

            int magic = reader.ReadInt32();
            reader.Stream.Seek(0x9EAE); //debug
            string strr = ReadString(reader, ttl); //debug
            System.Diagnostics.Debug.WriteLine(strr);
            

            return mes;
        }


        /// <summary>
        /// Reads a message MES file.
        /// </summary>
        /// <param name="fileBytes">The MES file as byte array.</param>
        /// <param name="offset">The location in the array to start reading data from.</param>
        /// <param name="length">The number of bytes to read from the array.</param>
        /// <returns>A MES object.</returns>
        public static MES ReadMES(byte[] fileBytes, TTL ttl, int offset = 0, int length = 0)
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
        /// <param name="stream">The MES file as stream.</param>
        /// <returns>A MES object.</returns>
        public static MES ReadMES(Stream stream, TTL ttl)
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
        /// <returns>A MES object.</returns>
        public static MES ReadMES(string path, TTL ttl)
        {
            using (var datastream = DataStreamFactory.FromFile(path, FileOpenMode.Read))
            {
                return ReadMES(datastream, ttl);
            }
        }


        private static string ReadString(DataReader reader, TTL ttl)
        {
            string output = "";

            while (true)
            {
                uint character = reader.ReadUInt16();
                if (character == 65535) break;
                if (character == 65534)
                {
                    output += "\n";
                    continue;
                }
                else
                {
                    char chara = ttl.CharacterTable[ttl.IdList[(int)character]];
                    output += chara;
                }
            }

            return output;
        }
    }
}
