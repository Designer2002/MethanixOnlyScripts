using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace COMMANDS
{
    public class CommandParameters
    {
        private const char IDENTIFIER = '-';
        private const char arrayBracketOpen = '(';
        private const char arrayBracketClose = ')';
        private Dictionary<string, string> parameters = new Dictionary<string, string>();
        public CommandParameters(string[] parameterArrray, int startIndex = 0)
        {
            for(int i = startIndex; i < parameterArrray.Length; i++)
            {
                if (parameterArrray[i].StartsWith(IDENTIFIER.ToString()))
                {
                    string pName = parameterArrray[i];
                    string pValue = "";

                    if((i + 1 < parameterArrray.Length && !parameterArrray[i + 1].StartsWith(IDENTIFIER.ToString())) || (i + 1 < parameterArrray.Length && parameterArrray[i + 1].Contains(".")) || (i + 1 < parameterArrray.Length && float.TryParse(parameterArrray[i + 1], out float t)))
                    {
                        pValue = parameterArrray[i + 1];
                        i++;
                    }
                    parameters.Add(pName, pValue);

                }
            }
        }
        public bool TryGetValue<T>(string parameter, out T value, T defaultValue = default(T)) => TryGetValue(new string[] { parameter }, out value, defaultValue);
        public bool TryGetValue<T>(string[] parametersNames, out T value, T defaultValue = default(T))
        {
            foreach(string pname in parametersNames)
            {
                if(parameters.TryGetValue(pname, out string pvalue))
                {
                    if (TryCastParameter(pvalue, out value))
                    {
                        return true;
                    }
                }
            }
            value = defaultValue;
            return false;
        }
        private bool TryCastParameter<T>(string parameterValue, out T value)
        {
            if(typeof(T) == typeof(string[]) && parameterValue.StartsWith(arrayBracketOpen.ToString()) && parameterValue.EndsWith(arrayBracketClose.ToString()))
            {
                string[] valuesplit = parameterValue.Split(' ');
                value = (T)(object)valuesplit;
                return true;
            }
            if(typeof(T) == typeof(bool))
            {
                if(bool.TryParse(parameterValue, out bool boolValue))
                {
                    value = (T)(object)boolValue;
                    return true;
                }
            }
            else if (typeof(T) == typeof(int))
            {
                if (int.TryParse(parameterValue, out int intValue))
                {
                    value = (T)(object)intValue;
                    return true;
                }
            }
            else if (typeof(T) == typeof(float))
            {
                if(parameterValue.Contains(".") && parameterValue[0] != '.')
                {
                    int i = 0;
                    char dot = '\n';
                    while (dot != '.')
                    {
                        i++;
                        dot = parameterValue[i];
                    }
                    var all = parameterValue.Remove(i, 1);
                    var floating = parameterValue.Substring(i+1);

                    var divider = "10";
                    for(int j = 1; j < floating.Length; j++)
                    {
                        divider += "0";
                    }
                    if (float.TryParse(all, out float nodivider))
                    {
                        value = (T)(object)(float)(nodivider / int.Parse(divider));
                        return true;
                    }

                }
                if (float.TryParse(parameterValue, out float floatValue))
                {
                    value = (T)(object)floatValue;
                    return true;
                }
            }
            else if(typeof(T) == typeof(string))
            {
                value = (T)(object)parameterValue;
                return true;
            }
            value = default(T);
            return false;
        }
    }
}