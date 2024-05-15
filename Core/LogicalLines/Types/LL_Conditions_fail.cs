//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//using static DIALOGUE.Logical_Lines.LogicalLinesUtilities.Encapsulation;
//using static DIALOGUE.Logical_Lines.LogicalLinesUtilities.Conditions;
//using System.Linq;

//namespace DIALOGUE.Logical_Lines
//{
//    public class LL_Conditions : ILogicalLine
//    {
//        public string KeyWord => "if";
//        private const string ELSE = "else";
//        private Conversation newConversation;
//        private readonly string[] CONTAINERS = new string[] { "(", ")" };
//        public static Dictionary<string, bool> Conditions = new Dictionary<string, bool>(); //executed + cond
//        public static int NestingLayers => Conditions.Count;
//        public static string ConditionKey = "";
//        private static bool LastNestedLine = false;
//        public static DIALOGUE_LINE NestedLine;


//        public static bool FindNestedLine(Conversation conversation, DIALOGUE_LINE line)
//        {
//            if(CheckIfNested(conversation))
//            {
//                NestedLine = new DIALOGUE_LINE(Conditions.Keys.ToList()[GetNum(line.RawData) + 1]);
//                return true;
//            }
//            return false;
//        }

//        private static bool CheckIfNested(Conversation conversation)
//        {
//            Dictionary<string, bool> oldConditions = Conditions;
//            int count = oldConditions.Count;
//            Conditions = new Dictionary<string, bool>();
//            List<string> keys = ExtractConditionList(conversation.GetLines());
//            //if (vals.Count == 0) Debug.LogError("лох");
            
//            foreach (var key in keys) if(!Conditions.ContainsKey(key)) Conditions.Add(key, oldConditions.ContainsKey(key) ? oldConditions[key] : false);
//            LastNestedLine = count == 2 && Conditions.Count == 1;
//            if (Conditions.Count <= 1) return false;
//            else return true;
//        }

//        public IEnumerator Execute(DIALOGUE_LINE line)
//        {
//            ParseCondition(line);
//            yield return null;
//        }

//        private bool IsNestedConversation(Conversation conversation)
//        {
//            int ifes_and_elses = 0;
//            foreach(var line in conversation.GetLines())
//            {
//                if (line.Trim('\t').StartsWith("if") || line.Contains("else"))
//                    ifes_and_elses++;
//            }
//            return ifes_and_elses > 0;
//        }

//        public void ParseCondition(DIALOGUE_LINE line)
//        {
//            Conversation currentConversation = DialogueSystem.instance.conversationManager.conversation;
//            int progress = DialogueSystem.instance.conversationManager.convProgress;

//            EncapsulatedData ifData = RipEncapsulationData(currentConversation, progress, false);
//            EncapsulatedData elseData = new EncapsulatedData();

//            string RawCondition = ExtractCondition(line.RawData).Trim();
//            bool conditionResult = EvaluteCondition(RawCondition);
//            if (ifData.endingIndex + 1 < currentConversation.Count())
//            {
//                string nextLine = currentConversation.GetLines()[ifData.endingIndex + 1].Trim();
//                if (nextLine == ELSE)
//                {
//                    elseData = RipEncapsulationData(currentConversation, ifData.endingIndex + 1, false);
//                    ifData.endingIndex = elseData.endingIndex;

//                }
//            }

//            EncapsulatedData selectedData = conditionResult ? ifData : elseData;
//            DialogueSystem.instance.conversationManager.conversation.SetProgress(selectedData.endingIndex);
//            if (!selectedData.isNull && selectedData.lines.Count > 0)
//            {
//                newConversation = new Conversation(selectedData.lines);
//                DialogueSystem.instance.conversationManager.EnqueuePriority(newConversation);
//            }
//        }

//        public IEnumerator Execute2(DIALOGUE_LINE line)
//        {
//            Conversation currentConversation = new Conversation(new List<string>());
//            bool nested = CheckIfNested(newConversation == null ? DialogueSystem.instance.conversationManager.conversation : newConversation);
//            if(LastNestedLine)
//            {
//                List<string> cleanedList = new List<string>();
//                foreach (var l in newConversation.GetLines())
//                {
//                    if (!l.Contains("{") && !l.Contains("if $(") && !(l.Trim('\t') == "else"))
//                        cleanedList.Add(l);
//                }
//                DialogueSystem.instance.conversationManager.EnqueuePriority(new Conversation(cleanedList));
//                yield break;
//            }
//            if (nested)
//            {
//                if (newConversation == null) currentConversation = DialogueSystem.instance.conversationManager.conversation;
//                else currentConversation = newConversation;
//            }
//            else currentConversation = DialogueSystem.instance.conversationManager.conversation;
//            int progress = nested && newConversation != null ? 0 : DialogueSystem.instance.conversationManager.convProgress;

//            EncapsulatedData ifData = RipEncapsulationData(currentConversation, progress, false);
//            EncapsulatedData elseData = new EncapsulatedData();

//            string RawCondition = nested && newConversation!= null ? ExtractConditionLine(newConversation) : ExtractCondition(line.RawData).Trim();
//            bool conditionResult = EvaluteCondition(RawCondition);
//            ConditionKey = RawCondition;

//            Debug.LogWarning(RawCondition);

//            if(ifData.endingIndex + 1 < currentConversation.Count())
//            {
//                string nextLine = currentConversation.GetLines()[ifData.endingIndex + 1].Trim();
//                if (nextLine == ELSE)
//                {
//                    elseData = RipEncapsulationData(currentConversation, ifData.endingIndex + 1, false);
//                    ifData.endingIndex = elseData.endingIndex;

//                }
//                else if (Conditions.TryGetValue(RawCondition, out bool b) && b)
//                {
//                    (ifData, elseData) = RipNestedLines(currentConversation);
//                }
//            }
            
//            EncapsulatedData selectedData = conditionResult ? ifData : elseData;
//            DialogueSystem.instance.conversationManager.conversation.SetProgress(selectedData.endingIndex);
//            if (!selectedData.isNull && selectedData.lines.Count > 0)
//            {
//                if (Conditions.TryGetValue(RawCondition, out bool b) && b)
//                {
//                    newConversation = new Conversation(selectedData.lines);
//                    DialogueSystem.instance.conversationManager.EnqueuePriority(newConversation);
//                }

//                if (Conditions.Count > 0) Conditions[RawCondition] = true;
//            }

//            yield return null;
//        }

//        public bool Matches(DIALOGUE_LINE line)
//        {
//            return line.RawData.StartsWith(KeyWord);
//        }

//        private static int GetNum(string RawCondition)
//        {
//            int idx = 0;
//            foreach(var k in Conditions.Keys)
//            {
//                if (k == RawCondition)
//                    return idx;
//                idx++;
//            }
//            return -1;
//        }

//        private string ExtractCondition(string line)
//        {
//            int startIndex = line.IndexOf(CONTAINERS[0]) + 1;
//            int endIndex = line.IndexOf(CONTAINERS[1]);
//            if (startIndex == -1 || endIndex == -1) return line;
//            return line.Substring(startIndex, endIndex - startIndex).Trim();
//        }

//        private string ExtractConditionLine(Conversation conversation)
//        {
//            foreach (var line in conversation.GetLines())
//            {

//                int startIndex = line.Trim('\t').IndexOf(CONTAINERS[0]) + 1;
//                int endIndex = line.Trim('\t').IndexOf(CONTAINERS[1]);
//                if (startIndex == -1 || endIndex == -1)
//                    continue;
//                return line.Trim('\t').Substring(startIndex, endIndex - startIndex).Trim();
//            }
//            return string.Empty;
//        }

//        private static List<string> ExtractConditionList(List<string> lines)
//        {
//            string _if = "if ($";
//            string _else = "else";
//            List<string> returnData = new List<string>();
//            foreach (var line in lines)
//            {
//                if (line.Contains(_if) || line.Contains(_else))
//                {
//                    var newL = line.Trim('(');
//                    newL = newL.Trim(')');
//                    var idx = newL.IndexOf("if");
//                    if(idx != -1) newL = newL.Remove(idx, 2);
//                    newL = newL.Trim();
//                    if (newL.Contains("(")) newL = newL.Substring(1);
//                    returnData.Add(newL);
//                }
//            }
            
//            return returnData;
//        }

//        private static (EncapsulatedData, EncapsulatedData) RipNestedLines(Conversation conversation)
//        {
//            string start = "if ($";
//            string startElse = "else";
//            List<string> lines_tmp = new List<string>();
//            List<string> lines_tmp2 = new List<string>();
//            bool flag = false;
//            for (int i = 0; i < conversation.GetLines().Count; i++)
//            {
//                if (!conversation.GetLines()[0].Contains(start)) break;
//                if (conversation.GetLines()[i].Contains(startElse)) flag = true;
//                if (!flag) lines_tmp.Add(conversation.GetLines()[i]);
//                else lines_tmp2.Add(conversation.GetLines()[i]);
//            }
            
//            return (new EncapsulatedData { lines = lines_tmp, startingIndex = 0, endingIndex = lines_tmp.Count + lines_tmp2.Count}, new EncapsulatedData { lines = lines_tmp2, startingIndex = 0, endingIndex = lines_tmp2.Count + lines_tmp.Count});
//        }
//    }

//}