using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace DIALOGUE
{
    public class DL_DIALOGUE_DATA
    {
        public string RawData { get; private set; } = string.Empty;
        public List<DIALOGUE_SEGMENT> segments;
        private const string segmentIdentifierPattern = @"\{[ca]\}|\{w[ca]\s\d*\.?\d*\}";
        public DL_DIALOGUE_DATA(string rawDialogue)
        {
            this.RawData = rawDialogue;
            segments = RipSegments(rawDialogue);
        }

        private List<DIALOGUE_SEGMENT> RipSegments(string rawDialogue)
        {
            List<DIALOGUE_SEGMENT> segments = new List<DIALOGUE_SEGMENT>();
            MatchCollection matches = Regex.Matches(rawDialogue, segmentIdentifierPattern);
            int lastIndex = 0;
            DIALOGUE_SEGMENT segment = new DIALOGUE_SEGMENT();
            if (matches.Count == 0)
            {
                segment.dialogue = rawDialogue;
            }
            else
            {
                segment.dialogue = rawDialogue.Substring(0, matches[0].Index);
            }
            segment.Signal = DIALOGUE_SEGMENT.StartSignal.NONE;
            segment.signalDelay = 0;
            segments.Add(segment);

            if (matches.Count == 0) return segments;
            else
            {
                lastIndex = matches[0].Index;
            }
            for (int i = 0; i < matches.Count; i++)
            {
                Match match = matches[i];
                segment = new DIALOGUE_SEGMENT();
                string signalMatch = match.Value;
                signalMatch = signalMatch.Substring(1, match.Length - 2);
                string[] signalSplit = signalMatch.Split(' ');

                segment.Signal = (DIALOGUE_SEGMENT.StartSignal)Enum.Parse(typeof(DIALOGUE_SEGMENT.StartSignal), signalSplit[0].ToUpper());
                if (signalSplit.Length > 1)
                {
                    float.TryParse(signalSplit[1], out segment.signalDelay);
                }
                int nextIndex;
                if (i + 1 < matches.Count) nextIndex = matches[i + 1].Index;
                else nextIndex = rawDialogue.Length;
                segment.dialogue = rawDialogue.Substring(lastIndex + match.Length, nextIndex - (lastIndex + match.Length));
                lastIndex = nextIndex;
                segments.Add(segment);
            }
            return segments;

        }

        public struct DIALOGUE_SEGMENT
        {
            public string dialogue;
            public StartSignal Signal;
            public float signalDelay;
            public enum StartSignal
            {
                NONE, C, A, WC, WA
            }
            public bool AppendText => Signal == StartSignal.A || Signal == StartSignal.WA;
        }
    }
}