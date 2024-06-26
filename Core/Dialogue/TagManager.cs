using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

public class TagManager
{
    private static readonly Dictionary<string, Func<string>> tags = new Dictionary<string, Func<string>>()
    {
        { "<mainCharacter>", () => "Avira" },
        { "<time>", () => DateTime.Now.ToString("hh:mm tt") },
        { "<playerLevel>", () => "15" },
        {"<tempVal1>", () => "42" },
        {"<input>", () => InputPanel.instance.lastInput }
    };
    private static readonly Regex tagRegex = new Regex("<\\w+>");
    public static string Inject(string text, bool injectTags = true, bool injectVariables = true)
    {
        if (injectTags) text = InjectTags(text);
        if (injectVariables) text = InjectVariables(text);
        return text;
    }

    private static string InjectTags(string value)
    {
        if (tagRegex.IsMatch(value))
        {
            foreach (Match match in tagRegex.Matches(value))
            {
                if (tags.TryGetValue(match.Value, out var tagValueRequest))
                {
                    value = value.Replace(match.Value, tagValueRequest());
                }
            }
        }
        return value;
    }

    private static string InjectVariables(string value)
    {
        var matches = Regex.Matches(value, VariableStore.REGEX_VARIABLE_IDS);
        var matchesList = matches.Cast<Match>().ToList();
        for (int i = matches.Count - 1; i >= 0; i--)
        {
            var match = matchesList[i];
            string variableName = match.Value.TrimStart(VariableStore.VARIABLE_ID, '!');

            bool negate = (match.Value.StartsWith("!"));
            bool endsInIllegalCharacter = variableName.EndsWith(VariableStore.DTABASE_VARIABLE_SPLITTER.ToString());
            if(endsInIllegalCharacter)
            {
                variableName = variableName.Substring(0, variableName.Length - 1);
            }
            if(!VariableStore.TryGetValue(variableName, out object variableValue))
            {
                UnityEngine.Debug.LogError($"variable name NOT FOUND");
                continue;
            }

            if (negate && variableValue is bool)
                variableValue = !(bool)variableValue;

            int lengthToBeRamoved = match.Index + match.Length > value.Length ? value.Length - match.Index : match.Length;
            if (endsInIllegalCharacter) lengthToBeRamoved -= 1;
            
            value = value.Remove(match.Index, lengthToBeRamoved);
            value = value.Insert(match.Index, variableValue.ToString());

        }
        return value;
    }
}
