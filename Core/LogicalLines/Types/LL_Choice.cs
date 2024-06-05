using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using static DIALOGUE.Logical_Lines.LogicalLinesUtilities.Encapsulation;

namespace DIALOGUE.Logical_Lines
{
    public class LL_Choice : ILogicalLine
    {
        public string KeyWord => "choice";
        private const char CHOICE_IDENTIFIER = '-';

        public IEnumerator Execute(DIALOGUE_LINE line)
        {
            var currentConverstion = DialogueSystem.instance.conversationManager.conversation;
            var progress = DialogueSystem.instance.conversationManager.convProgress;

            EncapsulatedData data = RipEncapsulationData(currentConverstion, progress, ripSpeakerandEncapsualtor: true, currentConverstion.fileStartIndex);
            //Debug.LogWarning(data.endingIndex);
            List<Choice> choices = GetChoicesFromData(data);
            string title = line.dialogueData.RawData;
            ChoicePanel panel = ChoicePanel.instance;
            string[] choiceTitles = choices.Select(c => c.title).ToArray();
            panel.Show(title, choiceTitles);
            while (panel.isWaitingOnUserChoice) yield return null;
            Choice selectedChoice = choices[panel.lastDecision.answerIdx];
            Conversation newconv = new Conversation(selectedChoice.resultLines, file: currentConverstion.file, fileStartIndex: selectedChoice.startIndex, fileEndIndex: selectedChoice.endIndex);
            DialogueSystem.instance.conversationManager.conversation.SetProgress(data.endingIndex - currentConverstion.fileStartIndex); //data ending index срет
            Debug.Log(DialogueSystem.instance.conversationManager.conversation.GetProgress());
            DialogueSystem.instance.conversationManager.EnqueuePriority(newconv);

            AutoReader reader = DialogueSystem.instance.reader;
            if (reader != null && reader.skip)
            {
                if (VN_ConfigurationsData.activeConfig != null && !VN_ConfigurationsData.activeConfig.continueSkippingAfterChoice)
                {
                    reader.Disable();
                }
                
            }
            else
            {
                Debug.Log(reader);
                Debug.Log(VN_ConfigurationsData.activeConfig.continueSkippingAfterChoice);
            }
        }


        public bool Matches(DIALOGUE_LINE line)
        {
            return line.hasSpeaker && line.speakerData.name.ToLower() == KeyWord;
        }

       

        private List<Choice> GetChoicesFromData(EncapsulatedData data)
        {
            List<Choice> choices = new List<Choice>();
            int encapsulationLevel = 0;
            bool isFirstChoice = true;
            Choice choice = new Choice
            {
                title = string.Empty,
                resultLines = new List<string>(),
            };
            int choiceIndex = 0, i = 0;
            for(i = 1; i < data.lines.Count; i++)
            //foreach(var line in data.lines.Skip(1))
            {
                var line = data.lines[i];
                if(isChoiceStart(line) && encapsulationLevel == 1)
                {
                    if(!isFirstChoice)
                    {
                        choice.startIndex = data.startingIndex + (choiceIndex + 1);
                        choice.endIndex = data.startingIndex + (i - 1);
                        choices.Add(choice);
                        choice = new Choice
                        {
                            title = string.Empty,
                            resultLines = new List<string>()
                        };
                    }
                    choiceIndex = i;
                    choice.title = line.Trim().Substring(1);
                    isFirstChoice = false;
                    continue;
                }
                AddLineToResults(line, ref choice, ref encapsulationLevel);
            }
            if (!choices.Contains(choice))
            {
                choice.startIndex = data.startingIndex + choiceIndex + 1;
                choice.endIndex = data.startingIndex + (i - 2);
                choices.Add(choice);
            }
            return choices;
        }

        private void AddLineToResults(string line, ref Choice choice, ref int level)
        {
            line.Trim();
            if (isEncapsulationStart(line))
            {
                if(level > 0)
                {
                    choice.resultLines.Add(line);
                }
                level++;
                return;
            }
            if(isEncapsulationEnd(line))
            {
                level--;
                if (level > 0)
                {
                    choice.resultLines.Add(line);
                }
                return;
            }
            choice.resultLines.Add(line);
        }

       
        private bool isChoiceStart(string line) => line.Trim().StartsWith(CHOICE_IDENTIFIER.ToString());

      

        private struct Choice
        {
            public string title;
            public List<string> resultLines;
            public int startIndex;
            public int endIndex;
        }
    }
}