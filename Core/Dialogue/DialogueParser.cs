using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace DIALOGUE
{
    public class DialogParser
    {
        private const string commandRegexPattern = @"[\w\[\]]*[^\s]\(";
        // Start is called before the first frame update
        public static DIALOGUE_LINE Parse(string RawLine)
        {
            //Debug.Log($"parsing line  - '{RawLine}'");
            (string speaker, string dialogue, string command) = RipContent(RawLine);
            //Debug.Log($"speaker = {speaker}, dialogue = {dialogue}, command = {command}");

            return new DIALOGUE_LINE(RawLine, speaker, dialogue, command);
        }
        private static (string, string, string ) RipContent(string RawLine)
        {
            string speaker = "", dialogue = "", commands = "";
            int dialogueStart = -1, dialogueEnd = -1;
            bool isEscaped  = false;
            for(int i = 0; i < RawLine.Length; i++)
            {
                char current = RawLine[i];
                if (current == '\\') isEscaped = !isEscaped;
                else if (current == '"' && !isEscaped)
                {
                    if (dialogueStart == -1) dialogueStart = i;
                    else if (dialogueEnd == -1) dialogueEnd = i;
                }
                else
                {
                    isEscaped = false;
                }
                
                
            }
            //Debug.Log(RawLine.Substring(dialogueStart + 1, (dialogueEnd - dialogueStart-1)));
            Regex commandRegex = new Regex(commandRegexPattern);
            MatchCollection matches = commandRegex.Matches(RawLine);
            int commandStart = -1;
            foreach (Match match in matches)
            {
                if (match.Index < dialogueStart || match.Index > dialogueEnd)
                {
                    commandStart = match.Index;
                }
                if (dialogueStart == -1 && dialogueEnd == -1) return ("", "", RawLine.Trim());
            }
            if(commandStart != -1 && (dialogueStart == -1 && dialogueEnd == -1)) return ("", "", RawLine.Trim());
            if (dialogueStart != -1 && dialogueEnd != -1 && (commandStart == -1 || commandStart > dialogueEnd))
            {
                speaker = RawLine.Substring(0, dialogueStart).Trim();
                dialogue = RawLine.Substring(dialogueStart + 1, dialogueEnd - dialogueStart - 1).Replace("\\\"", "\"");
                if (commandStart != -1)
                {
                    commands = RawLine.Substring(commandStart).Trim();
                }
            }
            else if (commandStart != -1 && dialogueStart > commandStart)
            {
                commands = RawLine;
            }
            else dialogue = RawLine;
            return (speaker, dialogue, commands);
        }
    }
}