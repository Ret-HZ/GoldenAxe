using System.Collections.Generic;

namespace AceUtils.PMD
{
    /// <summary>
    /// Mesh in a <see cref="PMD"/> model.
    /// </summary>
    public class PMDMesh
    {
        /// <summary>
        /// Mesh name.
        /// </summary>
        /// <remarks>This name is only for reference. It does not exist in the PMD format.</remarks>
        public string Name { get; set; }

        /// <summary>
        /// Mesh vertices. Not separated.
        /// </summary>
        public List<PMDVertex> Vertices { get; set; }

        /// <summary>
        /// Mesh part vertices. Separated by vertices that form a strip together.
        /// </summary>
        public List<List<PMDVertex>> Parts { get; set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="PMDMesh"/> class.
        /// </summary>
        public PMDMesh()
        {
            Vertices = new List<PMDVertex>();
            Parts = new List<List<PMDVertex>>();
        }
    }
}
