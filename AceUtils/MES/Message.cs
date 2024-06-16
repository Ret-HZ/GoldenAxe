namespace AceUtils.MES
{
    public class Message
    {
        internal Message()
        {

        }


        internal Message(ushort speakerID, ushort duration, string text)
        {
            SpeakerID = speakerID;
            Duration = duration;
            Text = text;
        }


        /// <summary>
        /// Character this message is assigned to.
        /// </summary>
        public ushort SpeakerID { get; set; }

        /// <summary>
        /// Unknown.
        /// </summary>
        public ushort Unk0x4 { get; set; }
        
        /// <summary>
        /// Amount of frames this message stays on screen.
        /// </summary>
        public ushort Duration { get; set; }

        /// <summary>
        /// Message text.
        /// </summary>
        public string Text { get; set; }

    }
}
