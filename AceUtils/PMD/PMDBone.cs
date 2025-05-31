using System.Numerics;

namespace AceUtils.PMD
{
    /// <summary>
    /// Bone in a <see cref="PMD"/> model.
    /// </summary>
    public class PMDBone
    {
        /// <summary>
        /// Parent for this bone.
        /// </summary>
        public PMDBone ParentBone { get; set; }

        /// <summary>
        /// Local position matrix.
        /// </summary>
        public Matrix4x4 LocalMatrix { get; set; }

        /// <summary>
        /// World position matrix.
        /// </summary>
        public Matrix4x4 WorldMatrix { get; set; }

        /// <summary>
        /// Position matrix taking into account the position of its parent.
        /// </summary>
        /// <remarks>This is for reference only.</remarks>
        public Matrix4x4 Position { get; set; }

        /// <summary>
        /// Bone name.
        /// </summary>
        /// <remarks>Maximum length of 11 characters.</remarks>
        public string Name { get; set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="PMDBone"/> class.
        /// </summary>
        public PMDBone()
        {
            ParentBone = null;
            LocalMatrix = new Matrix4x4();
            WorldMatrix = new Matrix4x4();
            Position = new Matrix4x4();
            Name = "NEWBONE";
        }
    }
}
