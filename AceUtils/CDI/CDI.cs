using AceUtils.CDI.Enum;
using System.Collections.Generic;

namespace AceUtils.CDI
{
    public class CDI
    {
        public CDI()
        {
            Folders = new Dictionary<string, CDIFolder>();
        }

        public CDI(CDIVersion version) : this()
        {
            Version = version;
        }

        public CDIVersion Version { get; set; }

        /// <summary>
        /// CDI Contents.
        /// </summary>
        public Dictionary<string, CDIFolder> Folders { get; internal set; }

        /// <summary>
        /// Raw audio data section.
        /// </summary>
        public byte[] AudioData { get; internal set; }



        public List<CDIFolder> GetFolders()
        {
            List<CDIFolder> returnList = new List<CDIFolder>();
            foreach (KeyValuePair<string, CDIFolder> kvp in Folders)
            {
                returnList.Add(kvp.Value);
            }

            return returnList;
        }
    }
}
