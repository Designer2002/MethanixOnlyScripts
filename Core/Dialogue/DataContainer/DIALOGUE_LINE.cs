namespace DIALOGUE
{
    public class DIALOGUE_LINE
    {
        public string RawData { get; private set; } = string.Empty;
        // Start is called before the first frame update
        public DL_SPEAKER_DATA speakerData;
        public DL_DIALOGUE_DATA dialogueData;
        public DL_COMMAND_DATA commandData;

        public bool hasDialogue => dialogueData != null; //dialogue != string.Empty;
        public bool hasCommands => commandData != null;
        public bool hasSpeaker => speakerData != null;//speaker != string.Empty;

        public DIALOGUE_LINE(string rawLine, string s, string d, string c)
        {
            RawData = rawLine;
            this.speakerData = !string.IsNullOrWhiteSpace(s) ? new DL_SPEAKER_DATA(s) : null;
            this.dialogueData = string.IsNullOrWhiteSpace(d) ? null : new DL_DIALOGUE_DATA(d);
            this.commandData = string.IsNullOrWhiteSpace(c) ? null : new DL_COMMAND_DATA(c);
        }

        public DIALOGUE_LINE(string rawLine)
        {
            RawData = rawLine;
        }
    }
}
