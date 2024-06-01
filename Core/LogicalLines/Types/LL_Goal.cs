using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DIALOGUE.Logical_Lines;

using static DIALOGUE.Logical_Lines.LogicalLinesUtilities.Encapsulation;
using static DIALOGUE.Logical_Lines.LogicalLinesUtilities.Conditions;
using DIALOGUE;

public class LL_Goal : ILogicalLine
{
    public string KeyWord => "goal";
    private static readonly string[] CONTAINERS = new string[] { "(", ")" };
    private static readonly string AGILE = "-agile";
    private static readonly string MOVES = "-moves";
    private static string target;
    private static bool agile;
    private static int moves;
    private static Dictionary<string, Conversation> ConditionGoalChecks = new Dictionary<string, Conversation>();
    public static int Ending { get; set; }
    public static int Starting { get; set; }

    public static int After { get; set; }

    public static Conversation MainConversation;

    public IEnumerator Execute(DIALOGUE_LINE line)
    {
        DialogueSystem.instance.conversationManager.StopConversation();

        string GoalQuery = ExtractQuery(line.RawData).Trim();

        string[] args = GoalQuery.Split(' ');
        target = args[0];
        if (args.Length != 5) yield break;
        if (args[1] == AGILE) bool.TryParse(args[2], out agile);
        if (args[3] == MOVES) int.TryParse(args[4], out moves);
        LOCATIONS.LocationManager.instance.SetGoal(target, moves, agile);
        int progress = DialogueSystem.instance.conversationManager.convProgress;
        MainConversation = DialogueSystem.instance.conversationManager.conversation;
        EncapsulatedData GoalData = RipEncapsulationData(MainConversation, progress, false, MainConversation.fileStartIndex);
        MainConversation.SetProgress(GoalData.startingIndex);
        Ending = GoalData.endingIndex;
        Starting = GoalData.startingIndex;
        After = GoalData.endingIndex + 1;

        if (!GoalData.isNull && GoalData.lines.Count > 0)
        {
            Conversation newConversation = new Conversation(GoalData.lines);
            List<Conversation> newConversations = RipBigEncapsulatedConversation(newConversation);
            List<string> RawConditions = new List<string>();
            foreach (var c in newConversations)
            {
                RawConditions.Add(ExtractConditionLine(c));
            }
            RawConditions.RemoveAll(l => l.Length == 0);
            int[] insideProgress = new int[newConversations.Count];
            int idx = 0;
            foreach (var c in newConversations)
            {
                while (!c.HasReachedEnd())
                {
                    EncapsulatedData miniCondition = RipEncapsulationData(c, insideProgress[idx], false);
                    insideProgress[idx] += miniCondition.lines.Count;
                    Conversation check = new Conversation(miniCondition.lines);
                    if (miniCondition.lines.Count != 0)
                    {
                        if (!ConditionGoalChecks.ContainsKey(RawConditions[idx]))
                            ConditionGoalChecks.Add(RawConditions[idx], check);
                    }


                    c.IncrementProgress();

                }
                idx++;
            }
        }
        yield return null;
    }

    private static List<Conversation> RipBigEncapsulatedConversation(Conversation conversation)
    {
        string start = "if ($location";
        //string startElse = "else";
        List<Conversation> conversations = new List<Conversation>();
        List<string> lines_tmp = new List<string>();
        int idx = 0;
        foreach (var line in conversation.GetLines())
        {

            lines_tmp = new List<string>();
            if (line.Contains(start))
            {
                lines_tmp.Add(line);
                idx++;
                while (idx < conversation.Count() && (!conversation.GetLines()[idx].Contains(start)))
                {
                    lines_tmp.Add(conversation.GetLines()[idx]);
                    idx++;
                }
                if (idx == conversation.Count() - 1) lines_tmp.Add(conversation.GetLines()[idx]);
                Conversation conv = new Conversation(lines_tmp);

                conversations.Add(conv);
            }
        }
        return conversations;
    }


    private string ExtractQuery(string line)
    {
        int startIndex = line.IndexOf(CONTAINERS[0]) + 1;
        int endIndex = line.IndexOf(CONTAINERS[1]);
        return line.Substring(startIndex, endIndex - startIndex).Trim();
    }

    public bool Matches(DIALOGUE_LINE line)
    {
        return line.RawData.StartsWith(KeyWord);
    }

    public static void LoopProgress()
    {
        if (ConditionGoalChecks.Count != 0)
        {
            if (LOCATIONS.LocationManager.instance.goal != null)
            {
                foreach (var pair in ConditionGoalChecks)
                {
                    pair.Value.SetProgress(0);
                    if (EvaluteCondition(pair.Key))
                    {
                        if (pair.Value.GetLines()[0].Contains("if ($"))
                        {
                            EncapsulatedData IFdata = RipEncapsulationData(pair.Value, 0, false);
                            EncapsulatedData ELSEdata = new EncapsulatedData();
                            string RawCondition = ExtractCondition(pair.Value.GetLines())[0].Trim();
                            bool conditionResult = EvaluteCondition(RawCondition);

                            EncapsulatedData OnlyIFData = new EncapsulatedData();
                            OnlyIFData.lines = new List<string>();
                            if (IFdata.endingIndex + 1 < pair.Value.Count())
                            {
                                if (pair.Value.FindProgress("else") != -1)
                                {
                                    ELSEdata = RipEncapsulationData(pair.Value, IFdata.endingIndex, false);
                                    ELSEdata.startingIndex = 0;
                                    ELSEdata.endingIndex = ELSEdata.lines.Count;
                                    for (int i = 0; i < IFdata.endingIndex - ELSEdata.endingIndex; i++)
                                    {
                                        OnlyIFData.lines.Add(IFdata.lines[i]);
                                    }
                                    OnlyIFData.startingIndex = 0;
                                    OnlyIFData.endingIndex = OnlyIFData.lines.Count;

                                }
                            }

                            EncapsulatedData selectedData = conditionResult ? OnlyIFData : ELSEdata;

                            if (!selectedData.isNull && selectedData.lines.Count > 0)
                            {
                                DialogueSystem.instance.conversationManager.conversation.SetProgress(selectedData.endingIndex);
                                Conversation newConversation = new Conversation(selectedData.lines, fileStartIndex:selectedData.startingIndex, fileEndIndex:selectedData.endingIndex);
                                DialogueSystem.instance.conversationManager.EnqueuePriority(newConversation);

                            }
                        }
                        else
                        {
                            DialogueSystem.instance.conversationManager.EnqueuePriority(pair.Value);
                        }
                        //SetProgressAfterGoalChecks(pair.Value);
                        DialogueSystem.instance.conversationManager.StartConverstaion(DialogueSystem.instance.conversationManager.conversation);
                        DialogueSystem.instance.DialogueContainer.show();
                        DialogueSystem.instance.EnableGlobalReading();
                        LOCATIONS.LocationExpander.instance.Expander.interactable = false;
                        return;
                    }
                    else
                    {
                        GetOurOfGoalLoop();
                    }
                }

                //DialogueSystem.instance.conversationManager.conversation.IncrementProgress();

            }



        }

    }

    public static void SetProgressAfterGoalChecks(Conversation conversation)
    {
        After += conversation.Count();
    }

    public static void GetOurOfGoalLoop()
    {
        MainConversation.SetProgress(Ending);
        DialogueSystem.instance.conversationManager.StartConverstaion(MainConversation);
        DialogueSystem.instance.DialogueContainer.hide();
        DialogueSystem.instance.DisableGlobalReading();
    }

    private static List<string> ExtractCondition(List<string> lines)
    {
        List<string> returnData = new List<string>();
        foreach (var line in lines)
        {
            int startIndex = line.IndexOf(CONTAINERS[0]) + 1;
            int endIndex = line.IndexOf(CONTAINERS[1]);
            if (startIndex == -1 || endIndex == -1)
                continue;
            returnData.Add(line.Substring(startIndex, endIndex - startIndex).Trim());
        }
        return returnData;
    }

    private string ExtractConditionLine(Conversation conversation)
    {
        foreach (var line in conversation.GetLines())
        {

            int startIndex = line.Trim('\t').IndexOf(CONTAINERS[0]) + 1;
            int endIndex = line.Trim('\t').IndexOf(CONTAINERS[1]);
            if (startIndex == -1 || endIndex == -1)
                continue;
            return line.Trim('\t').Substring(startIndex, endIndex - startIndex).Trim();
        }
        return string.Empty;
    }
}