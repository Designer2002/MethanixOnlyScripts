using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace DIALOGUE
{
    public class DL_SPEAKER_DATA
    {
        public string RawData { get; private set; } = string.Empty;
        public string name, castName;
        public string DisplayName => castName != string.Empty ? castName : name;

        public Vector2 castPosition;

        public bool isCastringName => castName != string.Empty;
        public bool isCastingPosition = false;
        public bool isCastingExpression => CastExpressions.Count > 0;

        public const string NAME_CAST_ID = " as ";
        public const string POS_CAST_ID = " at ";
        public const string EXPRESSION_CAST_ID = " [";
        private const char AXIS_DELIMITTER = ':';
        private const char EXPRESSION_LAYER_JOINER = ',';
        private const char EXPRESSION_LAYER_DELIMITTER = ':';
        private const string ENTER_KEYWORD = "enter ";
        public List<(int layer, string expression)> CastExpressions { get; set; }

        public bool MakeCharacterEnter = false;

        private string ProcessKeyWord(string rawSpeaker)
        {
            if(rawSpeaker.StartsWith(ENTER_KEYWORD))
            {
                rawSpeaker = rawSpeaker.Substring(ENTER_KEYWORD.Length);
                MakeCharacterEnter = true;
            }
            return rawSpeaker;
        }
        public DL_SPEAKER_DATA(string rawSpeaker)
        {
            RawData = rawSpeaker;
            rawSpeaker = ProcessKeyWord(rawSpeaker);
            string pattern = @$"{NAME_CAST_ID}|{POS_CAST_ID}|{EXPRESSION_CAST_ID.Insert(EXPRESSION_CAST_ID.Length - 1, @"\")}";
            castName = "";
            castPosition = Vector2.zero;
            CastExpressions = new List<(int layer, string expression)>();
            MatchCollection matches = Regex.Matches(rawSpeaker, pattern);
            if (matches.Count == 0)
            {
                name = rawSpeaker;
                return;
            }
            int index = matches[0].Index;
            name = rawSpeaker.Substring(0, index);
            for (int i = 0; i < matches.Count; i++)
            {
                Match match = matches[i];
                int startIndex = 0;
                int endIndex = 0;
                if (match.Value == NAME_CAST_ID)
                {
                    startIndex = match.Index + NAME_CAST_ID.Length;
                    if (i < matches.Count - 1)
                    {
                        endIndex = matches[i + 1].Index;
                    }
                    else
                    {
                        endIndex = rawSpeaker.Length;
                    }
                    castName = rawSpeaker.Substring(startIndex, endIndex - startIndex);
                }
                else if (match.Value == POS_CAST_ID)
                {
                    isCastingPosition = true;
                    startIndex = match.Index + POS_CAST_ID.Length;
                    if (i < matches.Count - 1)
                    {
                        endIndex = matches[i + 1].Index;
                    }
                    else
                    {
                        endIndex = rawSpeaker.Length;
                    }
                    string castPos = rawSpeaker.Substring(startIndex, endIndex - startIndex);
                    string[] axis = castPos.Split(AXIS_DELIMITTER, (char)System.StringSplitOptions.RemoveEmptyEntries);
                    float.TryParse(axis[0], out castPosition.x);
                    if (axis.Length > 1) float.TryParse(axis[1], out castPosition.y);
                    Debug.Log("x = " + axis[0]);
                }
                else if (match.Value == EXPRESSION_CAST_ID)
                {
                    startIndex = match.Index + EXPRESSION_CAST_ID.Length;
                    if (i < matches.Count - 1)
                    {
                        endIndex = matches[i + 1].Index;
                    }
                    else
                    {
                        endIndex = rawSpeaker.Length;
                    }
                    string castExp = rawSpeaker.Substring(startIndex, endIndex - (startIndex + 1));
                    CastExpressions = castExp.Split(EXPRESSION_LAYER_JOINER)
                    .Select(x =>
                    {
                        var parts = x.Trim().Split(EXPRESSION_LAYER_DELIMITTER);
                        if (parts.Length == 2) return (int.Parse(parts[0]), parts[1]);
                        else return (1, parts[0]);
                    }).ToList();

                }

            }
        }
    }
}