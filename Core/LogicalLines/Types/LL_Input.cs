using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DIALOGUE.Logical_Lines
{
    public class LL_Input : ILogicalLine
    {
        public string KeyWord => "input";

        public IEnumerator Execute(DIALOGUE.DIALOGUE_LINE line)
        {
            string title = line.dialogueData.RawData;
            InputPanel panel = InputPanel.instance;
            panel.Show(title);
            while (panel.isWaitingOnUserInput) yield return null;
        }

        public bool Matches(DIALOGUE_LINE line)
        {
            return line.hasSpeaker && line.speakerData.name.ToLower() == KeyWord;
        }
    }
}