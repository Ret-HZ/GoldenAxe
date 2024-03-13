using System;
using System.Collections.Generic;
using System.Text;

namespace LibMES
{
    public class TTL
    {
        public TTL()
        {
            CharacterTable = new Dictionary<uint, char>();
            IdList = new List<uint>();
        }

        public enum Version
        {
            X,
            X2,
            XProto
        }

        public Version version { get; set; }

        public Dictionary<uint, char> CharacterTable { get; set; }

        public List<uint> IdList { get; set; }

    }
}
