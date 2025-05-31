using System.Numerics;

namespace AceUtils.PMD
{
    /// <summary>
    /// Vertex in a <see cref="PMDMesh"/>.
    /// </summary>
    public class PMDVertex
    {
        public Vector2 UV { get; set; }

        public Vector3 Normal { get; set; }

        public Vector3 Position { get; set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="PMDVertex"/> class.
        /// </summary>
        public PMDVertex()
        {
            UV = new Vector2();
            Normal = new Vector3();
            Position = new Vector3();
        }
    }
}
