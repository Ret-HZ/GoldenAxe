using System.Collections.Generic;

namespace AceUtils.MES
{
    public class MES
    {
        public MES()
        {
            Messages = new Dictionary<uint, Message>();
        }


        public Dictionary<uint, Message> Messages { get; set; }
    }
}
