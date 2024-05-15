using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text.RegularExpressions;

using static DIALOGUE.Logical_Lines.LogicalLinesUtilities.Expressions;

namespace DIALOGUE.Logical_Lines
{
    public class LL_Operator : ILogicalLine
    {
        public string KeyWord => throw new System.NotImplementedException();

        public IEnumerator Execute(DIALOGUE_LINE line)
        {
            string trimmedLine = line.RawData.Trim();
            string[] parts = Regex.Split(trimmedLine, REGEX_ARITHMATIC);
            if(parts.Length < 3)
            {
                Debug.Log("Invalid command");
                yield break;
            }
            string variable = parts[0].Trim().TrimStart(VariableStore.VARIABLE_ID);
            string op = parts[1].Trim();
            string[] remainingParts = new string[parts.Length - 2];
            Array.Copy(parts, 2, remainingParts, 0, parts.Length - 2);

            object value = CalculateValue(remainingParts);
            if (value == null) yield break;
            ProcessOperator(variable, op, value);
        }

        private void ProcessOperator(string variable, string op, object value)
        {
            if(VariableStore.TryGetValue(variable, out object currentValue))
            {
                ProcessOperatorOnVariable(variable, op, value, currentValue);
            }
            else if(op == "=")
            {
                VariableStore.CreateVariable(variable, value);
            }
        }

        private void ProcessOperatorOnVariable(string variable, string op, object value, object currentValue)
        {
            switch(op)
            {
                case "=":
                    VariableStore.TrySetValue(variable, value);
                    break;
                case "+=":
                    VariableStore.TrySetValue(variable, ConcatOrAdd(currentValue,value));
                    break;
                case "-=":
                    VariableStore.TrySetValue(variable, Convert.ToDouble(currentValue) - Convert.ToDouble(value));
                    break;
                case "*=":
                    VariableStore.TrySetValue(variable, Convert.ToDouble(currentValue) * Convert.ToDouble(value));
                    break;
                case "/=":
                    VariableStore.TrySetValue(variable, Convert.ToDouble(currentValue) / Convert.ToDouble(value));
                    break;
                default: 
                    Debug.LogError($"invalid operator: {op}");
                    break;
            }
        }

        private object ConcatOrAdd(object value, object currentValue)
        {
            if (value is string) return currentValue.ToString() + value;
            return Convert.ToDouble(currentValue) + Convert.ToDouble(value);
        }

        public bool Matches(DIALOGUE_LINE line)
        {
            Match match = Regex.Match(line.RawData.Trim(), REGEX_OPERATOR_LINE);
            return match.Success;
        }
    }
}