using System.Collections.Generic;

namespace AceUtils.PMD
{
    /// <summary>
    /// 3D model format for the Ace Combat X / Joint Assault game engine.
    /// </summary>
    public class PMD
    {
        /// <summary>
        /// Bones in this model.
        /// </summary>
        public List<PMDBone> Bones { get; set; }

        /// <summary>
        /// Meshes in this model.
        /// </summary>
        public List<PMDMesh> Meshes { get; set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="PMD"/> class.
        /// </summary>
        public PMD()
        {
            Bones = new List<PMDBone>();
            Meshes = new List<PMDMesh>();
        }
    }
}
