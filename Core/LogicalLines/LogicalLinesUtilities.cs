using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text.RegularExpressions;
using System;

namespace DIALOGUE.Logical_Lines
{
    public static class LogicalLinesUtilities
    {
        public static class Encapsulation
        {
            private const char ENCAPSULATION_START = '{';
            private const char ENCAPSULATION_END = '}';

            public struct EncapsulatedData
            {
                public List<string> lines;
                public bool isNull => lines == null;
                public int startingIndex;
                public int endingIndex;
            }
            public static bool isEncapsulationStart(string line) => line.Trim().StartsWith(ENCAPSULATION_START.ToString());
            public static bool isEncapsulationEnd(string line) => line.Trim().StartsWith(ENCAPSULATION_END.ToString());

            public static EncapsulatedData RipEncapsulationData(Conversation conversation, int startingIndex, bool ripSpeakerandEncapsualtor = false, int parentStartingIndex = 0)
            {

                int encapsulationLevel = 0;
                EncapsulatedData data = new EncapsulatedData { lines = new List<string>(), startingIndex = (startingIndex + parentStartingIndex), endingIndex = 0 };

                for (int i = startingIndex; i < conversation.Count(); i++)
                {
                    string line = conversation.GetLines()[i];
                    if (ripSpeakerandEncapsualtor || encapsulationLevel > 0 && !isEncapsulationEnd(line) || (isEncapsulationEnd(line) && encapsulationLevel > 0 && LOCATIONS.LocationManager.instance.goal == null))
                    {
                        data.lines.Add(line);
                    }

                    if (isEncapsulationStart(line))
                    {
                        encapsulationLevel++;
                        continue;
                    }
                    if (isEncapsulationEnd(line))
                    {
                        encapsulationLevel--;
                        if (encapsulationLevel == 0)
                        {
                            data.endingIndex = i + parentStartingIndex;
                            break;
                        }
                    }
                    if (line.Trim('\t') == "else") data.endingIndex = i + parentStartingIndex;
                }
                return data;
            }

            public static EncapsulatedData RipEncapsulationDataExceptElse(Conversation conversation, int startingIndex, bool ripSpeakerandEncapsualtor = false, int parentStartingIndex = 0)
            {

                int encapsulationLevel = 0;
                EncapsulatedData data = new EncapsulatedData { lines = new List<string>(), startingIndex = (startingIndex + parentStartingIndex), endingIndex = 0 };

                for (int i = startingIndex; i < conversation.Count(); i++)
                {
                    string line = conversation.GetLines()[i];
                    if (ripSpeakerandEncapsualtor || encapsulationLevel > 0 && !isEncapsulationEnd(line))
                    {
                        data.lines.Add(line);
                    }

                    if (isEncapsulationStart(line))
                    {
                        encapsulationLevel++;
                        continue;
                    }
                    if (isEncapsulationEnd(line))
                    {
                        encapsulationLevel--;
                        if (encapsulationLevel == 0)
                        {
                            data.endingIndex = i + parentStartingIndex;
                            break;
                        }
                    }
                }
                return data;
            }
            

        private static bool SearchForIfElseAndSucceed(Conversation conversation)
        {
            bool _if = false;
            bool _else = false;
            foreach (var line in conversation.GetLines())
            {
                if (line.Contains("if"))
                {
                    _if = true;
                    continue;
                }
                if (line.Contains("else"))
                {
                    _else = true;
                    continue;
                }
            }
            return _if && _else;
        }
    }
        public static class Expressions
        {
            public static HashSet<string> OPERATORS = new HashSet<string>() { "-", "-=", "+", "+=", "*", "*=", "/","/=" };
            public static readonly string REGEX_ARITHMATIC = @"([-+*/=]=?)";
            public static readonly string REGEX_OPERATOR_LINE = @"^\$\w+\s*(=|\+=|-=|\*=|/=|)\s*";
            

            public static object CalculateValue(string[] expressionParts)
            {
                List<string> operandStrings = new List<string>();
                List<string> operatorStrings = new List<string>();
                List<object> operands = new List<object>();

                for (int i = 0; i < expressionParts.Length; i++)
                {
                    string part = expressionParts[i].Trim();
                    if (part == string.Empty) continue;
                    if (OPERATORS.Contains(part))
                        operatorStrings.Add(part);
                    else operandStrings.Add(part);
                }
                
                foreach(string operandString in operandStrings)
                {
                    operands.Add(ExtractValue(operandString));
                }
                CalculateValue_DivisionAndMultiplication(operatorStrings, operands);
                CalculateValue_AdditionAndSubstraction(operatorStrings, operands);

                return operands[0];
            }

            private static void CalculateValue_DivisionAndMultiplication(List<string> operatorStrings, List<object> operands)
            {
                for(int i = 0; i < operatorStrings.Count; i++)
                {
                    string operatorString = operatorStrings[i];
                    if(operatorString == "*" || operatorString == "/")
                    {
                        double leftOperand = Convert.ToDouble(operands[i]);
                        double rightOperand = Convert.ToDouble(operands[i + 1]);

                        if (operatorString == "*") operands[i] = leftOperand * rightOperand;
                        else
                        {

                            if (rightOperand == 0)
                            {
                                Debug.LogError("zero division");
                                return;
                                
                            }
                            operands[i] = leftOperand / rightOperand;
                        }
                        operatorStrings.RemoveAt(i + 1);
                        operatorStrings.RemoveAt(i);
                        i--;
                    }
                }
            }

            private static void CalculateValue_AdditionAndSubstraction(List<string> operatorStrings, List<object> operands)
            {
                for (int i = 0; i < operatorStrings.Count; i++)
                {
                    string operatorString = operatorStrings[i];
                    if (operatorString == "+" || operatorString == "-")
                    {
                        double leftOperand = Convert.ToDouble(operands[i]);
                        double rightOperand = Convert.ToDouble(operands[i + 1]);

                        if (operatorString == "+") operands[i] = leftOperand + rightOperand;
                        else operands[i] = leftOperand - rightOperand;
                        operatorStrings.RemoveAt(i + 1);
                        operatorStrings.RemoveAt(i);
                        i--;
                    }
                }
            }

            private static object ExtractValue(string value)
            {
                bool negate = false;
                if(value.StartsWith("!"))
                {
                    negate = true;
                    value = value.Substring(1);
                }
                if(value.StartsWith(VariableStore.VARIABLE_ID.ToString()))
                {
                    string variableName = value.TrimStart(VariableStore.VARIABLE_ID);
                    if(!VariableStore.HasVariable(variableName))
                    {
                        Debug.Log($"{variableName} doesn't exist");
                        return null;
                    }
                    VariableStore.TryGetValue(variableName, out object val);
                    if (val is bool boolVal && negate) return !boolVal;
                    return val;
                }
                else if(value.StartsWith("\"") && value.EndsWith("\""))
                {
                    value = TagManager.Inject(value, injectTags: true);
                    return value.Trim('"');
                }
                else
                {
                    if(int.TryParse(value, out int intValue))
                    {
                        return intValue;
                    }
                    else if (float.TryParse(value, out float floatValue))
                    {
                        return floatValue;
                    }
                    if (bool.TryParse(value, out bool boolValue))
                    {
                        return negate ? !boolValue : boolValue;
                    }
                    else
                    {
                        value = TagManager.Inject(value, injectTags: true);
                        return value;
                    }
                }
            }
        }

        public static class Conditions
        {
            public static readonly string REGEX_CONDITION_OPERATORS = @"(==|!=|<=|>=|<|>|&&|\|\|)";
            public static bool EvaluteCondition(string condition)
            {
                condition = TagManager.Inject(condition, injectTags: true, injectVariables: true);
                string[] parts = Regex.Split(condition, REGEX_CONDITION_OPERATORS)
                    .Select(p => p.Trim()).ToArray();

                for (int i = 0; i < parts.Length; i++)
                {
                    if(parts[i].StartsWith("\"") && parts[i].EndsWith("\""))
                    {
                        parts[i] = parts[i].Substring(1, parts[i].Length - 2);
                    }
                }

                if(parts.Length == 1)
                {
                    if(bool.TryParse(parts[0], out bool result))
                    {
                        return result;
                    }
                    else
                    {
                        if (parts[0] == "else")
                            return !result;
                        Debug.LogError($"couldnt parse condition, {parts[0]}");
                        return false;
                    }
                }
                else if(parts.Length == 3)
                {
                    VariableStore.AddEvent(parts[0], parts[2]);
                    return EvaluateExpression(parts[0], parts[1], parts[2]);
                }
                else
                {
                    Debug.LogError("unsupported");
                    return false;
                }
            }

            private delegate bool OperatorFunc<T>(T left, T right);

            private static Dictionary<string, OperatorFunc<bool>> boolOperators = new Dictionary<string, OperatorFunc<bool>>()
            {
                { "&&", (left, right) => left && right },
                { "||", (left, right) => left || right },
                { "==", (left, right) => left == right },
                { "!=", (left, right) => left != right },
            };

            private static Dictionary<string, OperatorFunc<float>> floatOperators = new Dictionary<string, OperatorFunc<float>>()
            {
                { ">=", (left, right) => left >= right },
                { ">", (left, right) => left > right },
                { "<=", (left, right) => left <= right },
                { "<", (left, right) => left < right },
                { "==", (left, right) => left == right },
                { "!=", (left, right) => left != right },
            };

            private static Dictionary<string, OperatorFunc<int>> intOperators = new Dictionary<string, OperatorFunc<int>>()
            {
                { ">=", (left, right) => left >= right },
                { ">", (left, right) => left > right },
                { "<=", (left, right) => left <= right },
                { "<", (left, right) => left < right },
                { "==", (left, right) => left == right },
                { "!=", (left, right) => left != right },
            };

            private static bool EvaluateExpression(string left, string op, string right)
            {
                if(bool.TryParse(left, out bool leftBool) && bool.TryParse(right, out bool rightBool))
                    if(boolOperators.ContainsKey(op)) return boolOperators[op](leftBool, rightBool);
                if (float.TryParse(left, out float leftFloat) && float.TryParse(right, out float rightFloat))
                    return floatOperators[op](leftFloat, rightFloat);
                if (int.TryParse(left, out int leftInt) && int.TryParse(right, out int rightInt))
                    return intOperators[op](leftInt, rightInt);

                switch(op)
                {
                    case "==":
                        return left == right;
                    case "!=":
                        return left != right;
                    default: throw new InvalidOperationException("wrong operator");
                }
            }
        }
    }
}