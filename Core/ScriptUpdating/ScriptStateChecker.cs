using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace VISUALNOVEL
{ 
    public class ScriptStateChecker
    {
        private static string scriptStateFilePath => $"{FilePaths.game_saves}/update_time.json";

        public static void SaveScriptState(List<ScriptState> scriptStates)
        {
            string json = JsonUtility.ToJson(new ScriptStateWrapper { scriptStates = scriptStates });
            File.WriteAllText(scriptStateFilePath, json);
        }

        public static List<ScriptState> LoadScriptState()
        {
            if (File.Exists(scriptStateFilePath))
            {
                string json = File.ReadAllText(scriptStateFilePath);
                ScriptStateWrapper wrapper = JsonUtility.FromJson<ScriptStateWrapper>(json);
                return wrapper.scriptStates;
            }
            return new List<ScriptState>();
        }

    }


    [System.Serializable]
    public class ScriptStateWrapper
    {
        public List<ScriptState> scriptStates;
    }
}
