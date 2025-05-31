using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using Yarhl.IO;

namespace AceUtils.PMD
{
    public static class PMDReader
    {
        /// <summary>
        /// Reads a PMD file.
        /// </summary>
        /// <param name="datastream">The <see cref="DataStream"/>.</param>
        /// <returns>A <see cref="PMD"/> object.</returns>
        private static PMD ReadPMD(DataStream datastream)
        {
            var reader = new DataReader(datastream)
            {
                Endianness = EndiannessMode.LittleEndian,
            };

            string magic = reader.ReadString(4);
            if (magic != "PMD.")
                throw new Exception("Invalid magic. Expected PMD.");

            PMD pmd = new PMD();

            // TODO: Figure out what the other header elements are
            reader.Stream.Seek(0x9);
            sbyte boneCount = reader.ReadSByte();

            // Data Offset Table
            reader.Stream.Seek(0x20);
            int boneNamesOffset = reader.ReadInt32();
            int boneMatricesOffset = reader.ReadInt32();
            int unknownOffset0x08 = reader.ReadInt32();
            int meshBlockOffset = reader.ReadInt32();
            int unknownOffset0x10 = reader.ReadInt32();
            int unknownOffset0x14 = reader.ReadInt32();
            int unknownOffset0x18 = reader.ReadInt32();
            int unknownOffset0x1C = reader.ReadInt32();

            // Bones
            reader.Stream.Seek(boneNamesOffset);
            for (sbyte i = 0; i < boneCount; i++)
            {
                PMDBone bone = new PMDBone();
                sbyte parentBoneIndex = reader.ReadSByte();
                bone.Name = reader.ReadString(0xB);
                bone.ParentBone = (parentBoneIndex != -1 && pmd.Bones.Count > parentBoneIndex) ? pmd.Bones[parentBoneIndex] : null;
                pmd.Bones.Add(bone);
            }

            reader.Stream.Seek(boneMatricesOffset);
            for (sbyte i = 0; i < boneCount; i++)
            {
                PMDBone bone = pmd.Bones[i];
                bone.LocalMatrix = new Matrix4x4(
                    reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(),
                    reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(),
                    reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(),
                    reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()
                );

                bone.WorldMatrix = new Matrix4x4(
                    reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(),
                    reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(),
                    reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(),
                    reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()
                );

                // Calculate bone position relative to its parent
                if (bone.ParentBone != null)
                {
                    bone.Position = bone.LocalMatrix * bone.ParentBone.Position;
                }
                else
                {
                    bone.Position = bone.LocalMatrix;
                }
            }

            // Meshes
            reader.Stream.Seek(meshBlockOffset);
            bool isMeshBlock = true;
            while (isMeshBlock)
            {
                long blockStartOffset = reader.Stream.Position;

                int blockMagic = reader.ReadInt32();
                if (blockMagic != 931 && blockMagic != 17315 && blockMagic != 50083 && blockMagic != 33699 && blockMagic != 54179)
                {
                    System.Diagnostics.Debug.WriteLine($"BAD MESH BLOCK MAGIC!!!!!!! Magic: {blockMagic} at position: {reader.Stream.Position}");
                    isMeshBlock = false;
                    break;
                }

                int blockHeaderSize = reader.ReadInt32();
                int blockSize = reader.ReadInt32();
                short unknown0xC = reader.ReadInt16();
                short meshPartCount = reader.ReadInt16();
                int unknown0x10 = reader.ReadInt32();
                int unknown0x14 = reader.ReadInt32(); // -1
                int unknown0x18 = reader.ReadInt24(); // -1
                byte stride = reader.ReadByte();
                reader.ReadInt32(); //Padding

                List<(short vertices, short amount)> meshParts = new List<(short vertices, short amount)>();
                for (int i = 0; i < meshPartCount; i++)
                {
                    meshParts.Add((reader.ReadInt16(), reader.ReadInt16()));
                }

                PMDMesh mesh = new PMDMesh();

                foreach (var meshPart in meshParts)
                {
                    for (int i = 0; i < meshPart.amount; i++)
                    {
                        List<PMDVertex> stripVerts = new List<PMDVertex>();
                        for (int j = 0; j < meshPart.vertices; j++)
                        {
                            PMDVertex vertex = new PMDVertex();
                            reader.ReadInt32(); // Unknown 0x80000000
                            vertex.UV = new Vector2(reader.ReadSingle(), reader.ReadSingle());

                            vertex.Normal = new Vector3(reader.ReadSByte() / 127.0f, reader.ReadSByte() / 127.0f, reader.ReadSByte() / 127.0f);
                            reader.ReadByte(); // Padding

                            vertex.Position = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());

                            mesh.Vertices.Add(vertex);
                            stripVerts.Add(vertex);
                        }
                        mesh.Parts.Add(stripVerts);
                    }
                }

                mesh.Name = $"mesh_{blockStartOffset}";
                pmd.Meshes.Add(mesh);


                reader.Stream.Seek(blockStartOffset + blockSize);
            }
            System.Diagnostics.Debug.WriteLine($"THERE ARE {pmd.Meshes.Count} MESHES IN THIS PMD");

            return pmd;
        }


        /// <summary>
        /// Reads a PMD file.
        /// </summary>
        /// <param name="fileBytes">The PMD file as byte array.</param>
        /// <param name="offset">The location in the array to start reading data from.</param>
        /// <param name="length">The number of bytes to read from the array.</param>
        /// <returns>A <see cref="PMD"/> object.</returns>
        public static PMD ReadPMD(byte[] fileBytes, int offset = 0, int length = 0)
        {
            if (length == 0) length = fileBytes.Length;
            using (var datastream = DataStreamFactory.FromArray(fileBytes, offset, length))
            {
                return ReadPMD(datastream);
            }
        }


        /// <summary>
        /// Reads a PMD file.
        /// </summary>
        /// <param name="stream">The PMD file as <see cref="Stream"/>.</param>
        /// <returns>A <see cref="PMD"/> object.</returns>
        public static PMD ReadPMD(Stream stream)
        {
            using (var datastream = DataStreamFactory.FromStream(stream))
            {
                return ReadPMD(datastream);
            }
        }


        /// <summary>
        /// Reads a PMD file.
        /// </summary>
        /// <param name="path">The path to the PMD file.</param>
        /// <returns>A <see cref="PMD"/> object.</returns>
        public static PMD ReadPMD(string path)
        {
            using (var datastream = DataStreamFactory.FromFile(path, FileOpenMode.Read))
            {
                return ReadPMD(datastream);
            }
        }
    }
}
