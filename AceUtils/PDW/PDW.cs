using System.Collections.Generic;

namespace AceUtils.PDW
{
    public class PDW
    {
        internal PDW()
        {
            Textures = new List<PDWTexture>();
        }

        /// <summary>
        /// Amount of textures in the PDW.
        /// </summary>
        public int TextureAmount
        {
            get
            {
                return Textures.Count;
            }
        }

        /// <summary>
        /// Textures in the PDW.
        /// </summary>
        /// <remarks>The file format supports a maximum of 12 textures.</remarks>
        public List<PDWTexture> Textures;
    }
}
