using System;
using System.Collections.Generic;

namespace AceUtils.CDI
{
    public class CDIFolder
    {
        public CDIFolder()
        {
            Files = new Dictionary<string, CDIFile>();
        }

        /// <summary>
        /// Folder name.
        /// </summary>
        public string Name { get; internal set; }

        public ushort Unknown1 { get; set; }
        public ushort Unknown2 { get; set; }
        public ushort Unknown3 { get; set; }
        public ushort Unknown4 { get; set; }

        /// <summary>
        /// Allocated space for the folder contents.
        /// </summary>
        internal uint AllocatedSpace { get; set; }

        /// <summary>
        /// Folder contents.
        /// </summary>
        public Dictionary<string, CDIFile> Files { get; internal set; }



        /// <summary>
        /// Gets all files inside the folder.
        /// </summary>
        /// <returns>A <see cref="CDIFile"/> list.</returns>
        public List<CDIFile> GetFiles()
        {
            List<CDIFile> returnList = new List<CDIFile>();
            foreach (KeyValuePair<string, CDIFile> kvp in Files)
            {
                returnList.Add(kvp.Value);
            }

            return returnList;
        }


        /// <summary>
        /// Gets files whose names contain the given substring.
        /// </summary>
        /// <param name="searchStr">The substring to look for.</param>
        /// <returns>A <see cref="CDIFile"/> list.</returns>
        public List<CDIFile> SearchFilesByName(string searchStr)
        {
            List<CDIFile> returnList = new List<CDIFile>();
            foreach (KeyValuePair<string, CDIFile> kvp in Files)
            {
                if (kvp.Value.Name.IndexOf(searchStr, StringComparison.OrdinalIgnoreCase) >= 0)
                    returnList.Add(kvp.Value);
            }

            return returnList;
        }
    }
}
