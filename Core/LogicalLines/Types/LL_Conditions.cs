using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static DIALOGUE.Logical_Lines.LogicalLinesUtilities.Encapsulation;
using static DIALOGUE.Logical_Lines.LogicalLinesUtilities.Conditions;
using System.Linq;
using System;

namespace DIALOGUE.Logical_Lines
{
    public class LL_Conditions : ILogicalLine
    {
        public string KeyWord => "if";
        private const string ELSE = "else";
        private readonly string[] CONTAINERS = new string[] { "(", ")" };
        public static List<string> Conditions = new List<string>(); // cond
        public static bool CheckIfNested(Conversation conversation)
        {
            Conditions = new List<string>();
            Conditions = ExtractConditionList(conversation.GetLines());
            if (Conditions.Count <= 0) return false;
            //else if (Conditions.Count == 1 && !Conditions[0].Contains("else")) return true;
            else return true;
        }

        public IEnumerator Execute(DIALOGUE_LINE line)
        {
            ParseCondition(line);
            yield return null;
        }

        public bool EvaluateCondition(string condition)
        {
            // Base case: if the condition does not contain any nested conditions, evaluate it directly
            if (!condition.Contains("{"))
            {
                return EvaluteCondition(condition);
            }

            // Find the innermost nested condition
            int startIndex = condition.LastIndexOf("{");
            int endIndex = condition.IndexOf("}", startIndex);

            // Extract the innermost nested condition
            string innerCondition = condition.Substring(startIndex + 1, endIndex - startIndex - 1);

            // Evaluate the innermost nested condition recursively
            bool innerResult = EvaluateCondition(innerCondition);

            // Replace the innermost nested condition with its result in the original condition string
            condition = condition.Remove(startIndex, endIndex - startIndex + 1).Insert(startIndex, innerResult.ToString());

            // Recursively evaluate the modified condition
            return EvaluateCondition(condition);
        }

        //public void ParseCondition(DIALOGUE_LINE line)
        //{
        //    Conversation currentConversation = DialogueSystem.instance.conversationManager.conversation;
        //    int progress = currentConversation.GetProgress();
        //    EncapsulatedData ifData = RipEncapsulationData(currentConversation, progress, false, currentConversation.fileStartIndex);
        //    EncapsulatedData elseData = new EncapsulatedData();

        //    string RawCondition = ExtractConditionLine(currentConversation, progress).Trim();
        //    if (RawCondition == string.Empty) return;
        //    bool conditionResult = EvaluteCondition(RawCondition);
        //    //Debug.LogWarning(RawCondition);
        //    if (ifData.endingIndex + 1 < currentConversation.Count())
        //    {
        //        string nextLine = currentConversation.GetLines()[ifData.endingIndex + 1].Trim();
        //        if (nextLine == ELSE)
        //        {
        //            elseData = RipEncapsulationData(currentConversation, ifData.endingIndex + 1, false, currentConversation.fileStartIndex);
        //            ifData.endingIndex = elseData.endingIndex;
        //            //Debug.LogWarning($"{ ifData.endingIndex} - if");
        //            //Debug.LogWarning($"{ elseData.endingIndex} - else");
        //        }
        //    }
        //    //Debug.LogWarning($"overmove - {VariableStore.TryGetValue("overmove", out object over).ToString()}");
        //    EncapsulatedData selectedData = conditionResult ? ifData : elseData;

        //    int p = 0;
        //    if (Conditions.Contains(RawCondition))
        //    {
        //        if (elseData.isNull) p = ifData.endingIndex + 1;
        //        else p = elseData.endingIndex + 1;
        //    }
        //    else
        //    {
        //        if (elseData.isNull) p = ifData.endingIndex;
        //        else p = elseData.endingIndex;
        //    }
        //    DialogueSystem.instance.conversationManager.conversation.SetProgress(p);
        //    selectedData.startingIndex += 2;
        //    selectedData.endingIndex -= 1;
        //    if (!selectedData.isNull && selectedData.lines.Count > 0)
        //    {
        //        Conversation newConversation = new Conversation(selectedData.lines, file: currentConversation.file, fileStartIndex: selectedData.startingIndex, fileEndIndex: selectedData.endingIndex);
        //        if (CheckIfNested(currentConversation) && (Conditions.Count > 1 && RawCondition == Conditions[Conditions.Count - 2] || RawCondition == Conditions[Conditions.Count - 1]))
        //        {
        //            List<string> dataTrimBracket = new List<string>();
        //            foreach (var d in selectedData.lines)
        //                if (!d.Contains("}")) dataTrimBracket.Add(d);
        //            selectedData.lines = dataTrimBracket;
        //            newConversation = new Conversation(dataTrimBracket, file: currentConversation.file, fileStartIndex: selectedData.startingIndex, fileEndIndex: selectedData.endingIndex);
        //        }
        //        DialogueSystem.instance.conversationManager.EnqueuePriority(newConversation);
        //        if (CheckIfNested(newConversation)) ParseCondition(line);
        //    }

        //}

        //public void ParseCondition(DIALOGUE_LINE line, int level = 0)
        //{
        //    Conversation currentConversation = DialogueSystem.instance.conversationManager.conversation;
        //    int progress = currentConversation.GetProgress();
        //    EncapsulatedData ifData = RipEncapsulationData(currentConversation, progress, false, currentConversation.fileStartIndex);
        //    EncapsulatedData elseData = new EncapsulatedData();

        //    string RawCondition = ExtractConditionLine(currentConversation, progress).Trim();
        //    if (RawCondition == string.Empty) return;
        //    bool conditionResult = EvaluateCondition(RawCondition); // Using EvaluateCondition here

        //    if (ifData.endingIndex + 1 < currentConversation.Count())
        //    {
        //        string nextLine = currentConversation.GetLines()[ifData.endingIndex + 1].Trim();
        //        if (nextLine == ELSE)
        //        {
        //            elseData = RipEncapsulationData(currentConversation, ifData.endingIndex + 1, false, currentConversation.fileStartIndex);
        //            ifData.endingIndex = elseData.endingIndex;
        //        }
        //    }

        //    EncapsulatedData selectedData = conditionResult ? ifData : elseData;

        //    int p = elseData.isNull ? ifData.endingIndex : elseData.endingIndex;

        //    if (!selectedData.isNull)
        //    {
        //        // Track the nesting level
        //        int nestingLevel = level;
        //        List<string> outerLines = new List<string>();
        //        for (int i = progress; i <= selectedData.lines.Count - 1; i++)
        //        {
        //            string lineText = selectedData.lines[i];
        //            if (lineText.Contains("if ($") || lineText.Contains("else"))
        //            {
        //                if (nestingLevel == 0)
        //                {
        //                    break;
        //                }
        //                else
        //                {
        //                    continue;
        //                }
        //            }
        //            else if (lineText.Contains("{"))
        //            {
        //                nestingLevel++;
        //            }
        //            else if (lineText.Contains("}"))
        //            {
        //                nestingLevel--;
        //            }
        //            else if (nestingLevel == level + 1)
        //            {
        //                outerLines.Add(lineText);
        //            }
        //            else if (nestingLevel == level)
        //            {
        //                break;
        //            }
        //        }
        //        Debug.Log(outerLines.Count);
        //        DialogueSystem.instance.conversationManager.conversation.SetProgress(p);

        //        Conversation newConversation = new Conversation(selectedData.lines, file: currentConversation.file, fileStartIndex: selectedData.startingIndex, fileEndIndex: selectedData.endingIndex);

        //        if (outerLines.Count > 0) DialogueSystem.instance.conversationManager.EnqueuePriority(new Conversation(outerLines, 0));
        //        DialogueSystem.instance.conversationManager.EnqueuePriority(newConversation);
        //        if (CheckIfNested(newConversation)) ParseCondition(line, level + 1);
        //    }
        //}

        //public void ParseCondition(DIALOGUE_LINE line, int level = 0)
        //{
        //    Conversation currentConversation = DialogueSystem.instance.conversationManager.conversation;
        //    int progress = currentConversation.GetProgress();
        //    EncapsulatedData ifData = RipEncapsulationData(currentConversation, progress, false, currentConversation.fileStartIndex);
        //    EncapsulatedData elseData = new EncapsulatedData();

        //    string RawCondition = ExtractConditionLine(currentConversation, progress).Trim();
        //    if (RawCondition == string.Empty) return;
        //    bool conditionResult = EvaluteCondition(RawCondition);

        //    if (ifData.endingIndex + 1 < currentConversation.Count())
        //    {
        //        string nextLine = currentConversation.GetLines()[ifData.endingIndex + 1].Trim();
        //        if (nextLine == ELSE)
        //        {
        //            elseData = RipEncapsulationData(currentConversation, ifData.endingIndex + 1, false, currentConversation.fileStartIndex);
        //            ifData.endingIndex = elseData.endingIndex;
        //        }
        //    }

        //    EncapsulatedData selectedData = conditionResult ? ifData : elseData;

        //    int p = elseData.isNull ? ifData.endingIndex + 1 : elseData.endingIndex + 1;

        //    int nestingLevel = level;
        //    List<string> outerLines = new List<string>();

        //    if (!selectedData.isNull)
        //    {
        //        for (int i = 0; i < selectedData.lines.Count; i++)
        //        {
        //            string lineText = selectedData.lines[i].Trim();

        //            if (lineText.Contains("if ($") || lineText.Contains("else"))
        //            {
        //                continue;
        //            }
        //            else if (lineText.Contains("{"))
        //            {
        //                nestingLevel++;
        //            }
        //            else if (lineText.Contains("}"))
        //            {
        //                nestingLevel--;
        //            }
        //            else if (nestingLevel == level + 1)
        //            {
        //                outerLines.Add(lineText);
        //            }
        //            else if (nestingLevel == level)
        //            {
        //                break;
        //            }
        //        }
        //    }
        //    Debug.Log(outerLines);
        //    DialogueSystem.instance.conversationManager.conversation.SetProgress(p);
        //    selectedData.startingIndex += 2;
        //    selectedData.endingIndex -= 1;
        //    if (!selectedData.isNull && selectedData.lines.Count > 0)
        //    {
        //        Conversation newConversation = new Conversation(outerLines, file: currentConversation.file, fileStartIndex: selectedData.startingIndex, fileEndIndex: selectedData.endingIndex);
        //        if (CheckIfNested(currentConversation) && (Conditions.Count > 1 && RawCondition == Conditions[Conditions.Count - 2] || RawCondition == Conditions[Conditions.Count - 1]))
        //        {
        //            List<string> dataTrimBracket = new List<string>();
        //            foreach (var d in selectedData.lines)
        //                if (!d.Contains("}")) dataTrimBracket.Add(d);
        //            selectedData.lines = dataTrimBracket;
        //            newConversation = new Conversation(dataTrimBracket, file: currentConversation.file, fileStartIndex: selectedData.startingIndex, fileEndIndex: selectedData.endingIndex);
        //        }
        //        DialogueSystem.instance.conversationManager.EnqueuePriority(newConversation);
        //        if (CheckIfNested(newConversation)) ParseCondition(line, level + 1);
        //    }
        //}


        public void ParseCondition(DIALOGUE_LINE line)
        {
            string RawCondition = ExtractCondition(line.RawData.Trim());
            bool conditionResult = EvaluteCondition(RawCondition);

            Conversation currentConversation = DialogueSystem.instance.conversationManager.conversation;
            int currentProgress = DialogueSystem.instance.conversationManager.convProgress;

            EncapsulatedData ifData = RipEncapsulationData(currentConversation, currentProgress, false);
            EncapsulatedData elseData = new EncapsulatedData();

            if(ifData.endingIndex + 1 < currentConversation.Count())
            {
                string nextLine = currentConversation.GetLines()[ifData.endingIndex + 1].Trim();
                if(nextLine == ELSE)
                {
                    elseData = RipEncapsulationData(currentConversation, ifData.endingIndex + 1, false);
                    ifData.endingIndex = elseData.endingIndex;
                }
            }
            EncapsulatedData selectedData = conditionResult ? ifData : elseData;
            if(!selectedData.isNull && selectedData.lines.Count > 0)
            {
                
                Conversation newConversation = new Conversation(selectedData.lines);
                if (!CheckIfNested(newConversation) && LOCATIONS.LocationManager.instance.goal == null)
                {
                    List<string> dataTrimBracket = new List<string>();
                    foreach (var d in selectedData.lines)
                        if (!d.Contains("}")) dataTrimBracket.Add(d);
                    selectedData.lines = dataTrimBracket;
                    newConversation = new Conversation(dataTrimBracket, file: currentConversation.file, fileStartIndex: selectedData.startingIndex, fileEndIndex: selectedData.endingIndex);

                }
                if (!CheckIfNested(newConversation) && LOCATIONS.LocationManager.instance.goal == null)
                {
                    DialogueSystem.instance.conversationManager.conversation.SetProgress(selectedData.endingIndex + 1);
                   // Debug.Log("i");
                }
                else if (CheckIfNested(newConversation))
                {
                    DialogueSystem.instance.conversationManager.conversation.SetProgress(selectedData.endingIndex);
                    //Debug.Log("i2");
                }
                //DialogueSystem.instance.conversationManager.conversation.SetProgress(selectedData.endingIndex);
                DialogueSystem.instance.conversationManager.EnqueuePriority(newConversation);

                if (CheckIfNested(newConversation))
                {
                    var e = ExtractConditionList(newConversation.GetLines())[0];
                    ParseCondition(new DIALOGUE_LINE(e));
                }
            }
            
        }



        //public void ParseCondition(DIALOGUE_LINE line, int level = 0)
        //{
        //    Conversation currentConversation = DialogueSystem.instance.conversationManager.conversation;
        //    int progress = currentConversation.GetProgress();

        //    var dict = FormattedNested(currentConversation, progress);
        //    int max_nested = dict.Max(k => k.Key);
        //    var ripped_conversations = RipNestedConversationDict(dict);

        //    var currentRipped = ripped_conversations[level];

        //    string RawCondition = ExtractConditionLine(currentConversation, progress).Trim();
        //    if (RawCondition == string.Empty) return;
        //    bool conditionResult = EvaluteCondition(RawCondition);
        //}

        //public void ParseCondition(DIALOGUE_LINE line, int level = 0)
        //{
        //    Conversation currentConversation = DialogueSystem.instance.conversationManager.conversation;
        //    int progress = currentConversation.GetProgress();
        //    string RawCondition = ExtractConditionLine(currentConversation, progress).Trim(); // ѕолучение строки услови€ из диалога
        //    if (RawCondition == string.Empty) return;

        //    // ѕроверка на наличие вложенных условий
        //    if (RawCondition.Contains("{"))
        //    {
        //        // ќбнаружены вложенные услови€, извлекаем строку вложенного услови€
        //        string nestedCondition = ExtractNestedCondition(RawCondition);

        //        // –екурсивный вызов дл€ обработки вложенного услови€ с увеличением уровн€ вложенности
        //        ParseCondition(new DIALOGUE_LINE(nestedCondition), level + 1);

        //        // ѕродолжаем обработку остальной части услови€
        //        string remainingCondition = GetRemainingCondition(RawCondition);
        //        bool conditionResult = EvaluteCondition(remainingCondition);
        //    }
        //    else
        //    {
        //        // ќбработка обычного (не вложенного) услови€
        //        bool conditionResult = EvaluteCondition(RawCondition);
        //    }


        //}

        //public void ParseCondition(DIALOGUE_LINE line, int level = 0)
        //{
        //    Conversation currentConversation = DialogueSystem.instance.conversationManager.conversation;
        //    int progress = currentConversation.GetProgress();

        //    var dict = FormattedNested(currentConversation, progress);
        //    int maxNestedLevel = dict.Max(k => k.Key);

        //    if (level > maxNestedLevel)
        //    {
        //        return; // Ѕольше вложенных уровней нет, выход из рекурсии
        //    }

        //    var rippedConversationsIfElse = RipNestedConversationDictIfElse(dict, level);
        //    var currentRipped = rippedConversationsIfElse[level];

        //    string RawCondition = ExtractConditionLine(currentRipped.Item2, progress).Trim();

        //    if (RawCondition == string.Empty)
        //    {
        //        ParseCondition(line, level + 1); // ѕродолжаем рекурсию дл€ следующего уровн€ вложенности
        //        return;
        //    }

        //    bool conditionResult = EvaluteCondition(RawCondition);

        //    var selectedData = conditionResult ? rippedConversationsIfElse[level].Item1 : rippedConversationsIfElse[level].Item2;
        //    DialogueSystem.instance.conversationManager.conversation.SetProgress(progress+selectedData.Count());
        //    DialogueSystem.instance.conversationManager.EnqueuePriority(selectedData);
        //    if (CheckIfNested(selectedData))
        //    {
        //        // ¬ызов рекурсивно дл€ обработки вложенных условий на текущем уровне
        //        ParseCondition(line, level + 1);
        //    }


        //    //ParseCondition(line, level + 1); // ¬ызов рекурсивно дл€ следующего уровн€ вложенности
        //}

        private string GetRemainingCondition(string rawCondition)
        {
            int nestedStart = rawCondition.IndexOf("{");
            if (nestedStart == -1)
            {
                // ≈сли не обнаружено вложенных условий, возвращаем оригинальную строку
                return rawCondition;
            }

            // »щем индекс закрывающей фигурной скобки дл€ вложенного услови€
            int nestedLevel = 1;
            int nestedEnd = nestedStart;
            for (int i = nestedStart + 1; i < rawCondition.Length; i++)
            {
                if (rawCondition[i] == '{')
                {
                    nestedLevel++;
                }
                else if (rawCondition[i] == '}')
                {
                    nestedLevel--;
                    if (nestedLevel == 0)
                    {
                        nestedEnd = i;
                        break;
                    }
                }
            }

            // ќставша€с€ часть после вложенного услови€
            string remainingCondition = rawCondition.Substring(nestedEnd + 1).Trim();
            return remainingCondition;
        }

        private string ExtractNestedCondition(string rawCondition)
        {
            int nestedStart = rawCondition.IndexOf("{");
            if (nestedStart == -1)
            {
                // ¬ строке нет открывающей фигурной скобки дл€ вложенного услови€
                return string.Empty;
            }

            int nestedLevel = 1;
            int nestedEnd = nestedStart;

            for (int i = nestedStart + 1; i < rawCondition.Length; i++)
            {
                if (rawCondition[i] == '{')
                {
                    nestedLevel++; // Ќайдена нова€ открывающа€ скобка, увеличиваем уровень
                }
                else if (rawCondition[i] == '}')
                {
                    nestedLevel--; // Ќайдена закрывающа€ скобка, уменьшаем уровень

                    if (nestedLevel == 0)
                    {
                        nestedEnd = i;
                        break;
                    }
                }
            }

            if (nestedEnd > nestedStart)
            {
                // »звлекаем строку вложенного услови€ с открывающей и закрывающей фигурными скобками
                return rawCondition.Substring(nestedStart, nestedEnd - nestedStart + 1);
            }

            return string.Empty; // ¬ложенное условие не найдено
        }

        private bool DoesConversationContains(Conversation current, EncapsulatedData data)
        {
            for (int i = 0; i < data.lines.Count; i++)
            {
                if (data.lines[i].Contains("if $(") || data.lines[i].Contains("else") || data.lines[i].Contains('}') || data.lines[i].Contains('{')) continue;
                if (current.GetLines().Any(l => l == data.lines[i]))
                {
                    Debug.Log(data.lines[i]);
                    return true;
                }
            }
            return false;
        }

        private Dictionary<int, Tuple<int, Conversation>> RipNestedConversationDict(List<KeyValuePair<int, Tuple<int, string>>> dict)
        {
            return dict
            .GroupBy(kvp => kvp.Key)
            .ToDictionary(
                kvp => kvp.Key,
                kvp => new Tuple<int, Conversation>(kvp.Select(k => k.Value.Item1).First(), new Conversation((kvp.Select(k => k.Value.Item2).ToList()))));
        }

        private Dictionary<int, Tuple<Conversation, Conversation>> RipNestedConversationDictIfElse(List<KeyValuePair<int, Tuple<int, string>>> dict, int level)
        {
            Dictionary<int, Tuple<Conversation, Conversation>> result = new Dictionary<int, Tuple<Conversation, Conversation>>();
            List<string> ifs = new List<string>();
            List<string> elses = new List<string>();
            var range = dict.Where(l => l.Key == level).Count();
            var multipliedFormat = String.Concat(Enumerable.Repeat("\t", level + 1));
            for (int i = 0; i < range; i++)
            {
                if(dict[i].Value.Item2.Contains("if ($"))
                {
                   
                    var start = dict.Where(l => l.Value.Item1 > dict[i].Value.Item1 + 1).First().Value.Item1;
                    for (int j = start; j < dict.Max(n => n.Value.Item1); j++)
                    {
                        if (!dict.Where(k => k.Value.Item1 == j).First().Value.Item2.Contains("}") && !dict.Where(k => k.Value.Item1 == j).First().Value.Item2.Contains($"{multipliedFormat}else"))
                            ifs.Add(dict.Where(k => k.Value.Item1 == j).First().Value.Item2);
                        else
                        {
                            var d = dict.Where(k => k.Value.Item1 == j).First().Value.Item2;
                            Debug.Log(d);
                            break;
                        }
                    }
                }
                else if (dict[i].Value.Item2.Contains("else"))
                {
                   
                    var start = dict.Where(l => l.Value.Item1 > dict[i].Value.Item1 + 1).First().Value.Item1;
                    for (int j = start; j < dict.Max(n => n.Value.Item1); j++)
                    {
                        if (!dict.Where(k => k.Value.Item1 == j).First().Value.Item2.Contains("}") && !dict.Where(k => k.Value.Item1 == j).First().Value.Item2.Contains($"{multipliedFormat}if"))
                            elses.Add(dict.Where(k => k.Value.Item1 == j).First().Value.Item2);
                        else
                        {
                            var d = dict.Where(k => k.Value.Item1 == j).First().Value.Item2;
                            Debug.Log(d);
                            break;
                        }
                    }
                }
            }

            var pair = new Tuple<Conversation, Conversation>(new Conversation(ifs), new Conversation(elses));
            result.Add(level, pair);
            return result;
        }

        public List<string> RemoveNested(Conversation conversation, int progress, int level, Conversation modified)
        {
            var copy = new Conversation(conversation.GetLines(), progress);
            List<string> conversationPart = new List<string>();
            for (int i = progress; i < copy.Count(); i++)
            {
                conversationPart.Add(copy.GetLines()[i]);
            }
            string formattedStart = "\t";
            var conversationList = new List<string>(conversationPart);
            var removed = new List<string>();
            var copyList = new List<string>(conversationList);
            foreach(var line in conversationList)
            {
                if (line.StartsWith(System.String.Concat(Enumerable.Repeat(formattedStart, level+1))))
                {
                    //Debug.LogWarning(line);
                    removed.Add(line);
                    copyList.Remove(line);
                }
            }
            modified = new Conversation(copyList);

            return removed;
        }

        private List<KeyValuePair<int, Tuple<int, string>>> FormattedNested(Conversation conversation, int progress)
        {
            var copy = new Conversation(conversation.GetLines(), progress);
            List<KeyValuePair<int, Tuple<int, string>>> formatted = new List<KeyValuePair<int, Tuple<int, string>>>();
            List<string> conversationPart = new List<string>();
            for (int i = progress; i < copy.Count(); i++)
            {
                if (!copy.GetLines()[i].Contains("if") && !copy.GetLines()[i].Contains("else") && !copy.GetLines()[i].Contains("{") && !copy.GetLines()[i].Contains("}") && !copy.GetLines()[i].Contains("\t"))
                    break;
                else
                {
                    var a = copy.GetLines()[i];
                    conversationPart.Add(a);
                }
                
            }
            char formattedStart = '\t';
            int idx = progress;
            
            foreach (var line in conversationPart)
            {
                var d = new Tuple<int, string>(idx, line);

                var matchQuery = from word in line
                                 where word.Equals(formattedStart)
                                 select word;
                formatted.Add(new KeyValuePair<int, Tuple<int, string>>(matchQuery.Count(), d)); 
                idx++;
            }
            return formatted;
        }

        public bool Matches(DIALOGUE_LINE line)
        {
            return line.RawData.StartsWith(KeyWord);
        }

        private string ExtractCondition(string line)
        {
            int startIndex = line.IndexOf(CONTAINERS[0]) + 1;
            int endIndex = line.IndexOf(CONTAINERS[1]);
            if (startIndex == -1 || endIndex == -1) return line;
            return line.Substring(startIndex, endIndex - startIndex).Trim();
        }

        private string ExtractConditionLine(Conversation conversation, int progress)
        {
            for(int i = 0; i < conversation.GetLines().Count; i++)
            {
                var line = conversation.GetLines()[i];
                int startIndex = line.Trim('\t').IndexOf(CONTAINERS[0]) + 1;
                int endIndex = line.Trim('\t').IndexOf(CONTAINERS[1]);
                //if (startIndex == -1 || endIndex == -1)
                //    continue;
                if (line.Contains("else"))
                    return line;
                else if (line.Contains("if ($"))
                    return line.Trim('\t').Substring(startIndex, endIndex - startIndex).Trim();
            }
            return string.Empty;
        }

        private static List<string> ExtractConditionList(List<string> lines)
        {
            string _if = "if ($";
            string _else = "else";
            List<string> returnData = new List<string>();
            foreach (var line in lines)
            {
                if (line.Contains(_if) || line.Contains(_else))
                {
                    var newL = line.Trim('(');
                    newL = newL.Trim(')');
                    var idx = newL.IndexOf("if");
                    if (idx != -1) newL = newL.Remove(idx, 2);
                    newL = newL.Trim();
                    if (newL.Contains("(")) newL = newL.Substring(1);
                    returnData.Add(newL);
                }
            }

            return returnData;
        }
    }

}