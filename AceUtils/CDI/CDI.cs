using AceUtils.CDI.Enum;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AceUtils.CDI
{
    public class CDI
    {
        internal CDI()
        {
            Folders = new Dictionary<string, CDIFolder>();
            PathCache = new List<string>();
        }

        internal CDI(CDIVersion version) : this()
        {
            Version = version;
        }

        /// <summary>
        /// Format version.
        /// </summary>
        public CDIVersion Version { get; set; }

        /// <summary>
        /// CDI Contents.
        /// </summary>
        public Dictionary<string, CDIFolder> Folders { get; internal set; }

        /// <summary>
        /// Raw audio data section.
        /// </summary>
        public byte[] AudioData { get; internal set; }

        /// <summary>
        /// Cache of folder/file paths for searches.
        /// </summary>
        internal List<string> PathCache { get; set;}



        /// <summary>
        /// Gets all folders in the <see cref="CDI"/>.
        /// </summary>
        /// <returns>A <see cref="CDIFolder"/> list.</returns>
        public List<CDIFolder> GetFolders()
        {
            List<CDIFolder> returnList = new List<CDIFolder>();
            foreach (KeyValuePair<string, CDIFolder> kvp in Folders)
            {
                returnList.Add(kvp.Value);
            }

            return returnList;
        }


        /// <summary>
        /// Gets folders whose names contain the given substring.
        /// </summary>
        /// <param name="searchStr">The substring to look for.</param>
        /// <returns>A <see cref="CDIFolder"/> list.</returns>
        public List<CDIFolder> SearchFoldersByName(string searchStr)
        {
            List<CDIFolder> returnList = new List<CDIFolder>();
            foreach (KeyValuePair<string, CDIFolder> kvp in Folders)
            {
                if (kvp.Value.Name.IndexOf(searchStr, StringComparison.OrdinalIgnoreCase) >= 0)
                    returnList.Add(kvp.Value);
            }

            return returnList;
        }


        /// <summary>
        /// Gets folders whose names or children file names contain the given substring.
        /// </summary>
        /// <param name="searchStr">The substring to look for.</param>
        /// <returns>A <see cref="CDIFolder"/> list.</returns>
        public List<CDIFolder> SearchFoldersByNameAndContentName(string searchStr)
        {
            List<CDIFolder> returnList = new List<CDIFolder>();
            List<string> matchingPaths = PathCache.Where(s => s.IndexOf(searchStr, StringComparison.OrdinalIgnoreCase) >= 0).ToList();

            foreach (string path in matchingPaths)
            {
                string folderName = path.Split('/')[0];
                CDIFolder folder;
                Folders.TryGetValue(folderName, out folder);
                if (folder == null) continue;
                if (!returnList.Contains(folder)) returnList.Add(folder);
            }

            return returnList;
        }
    }
}
