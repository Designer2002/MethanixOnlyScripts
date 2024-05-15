using DIALOGUE;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace COMMANDS
{
    public class CMD_Database_Extension_General : CMD_DatabseExtensions
    {
        private static readonly string[] PARAM_FILE_PATH = new[] { "-f", "-filepath" };
        private static readonly string[] ENQUEUE = new[] { "-e", "-enqueue" };
        private static readonly string[] MAX = new[] { "-m", "-max" };
        private static readonly string[] ADD = new[] { "-a", "-add" };
        private static readonly string[] LOCATION = new[] { "-l", "-location" };
        new public static void Extend(CommandDatabase database)
        {
            database.AddCommand("wait", new Func<string, IEnumerator>(Wait));
            database.AddCommand("showbox", new Func<IEnumerator>(ShowBox));
            database.AddCommand("hidebox", new Func<IEnumerator>(HideBox));
            database.AddCommand("showui", new Func<IEnumerator>(Show));
            database.AddCommand("hideui", new Func<IEnumerator>(Hide));
            
            database.AddCommand("load", new Action<string[]>(Load));
            
            database.AddCommand("notify", new Action<string[]>(Notify));

            //searching items
            database.AddCommand("search", new Action<string[]>(Search));

            //missions
            database.AddCommand("newmission", new Action<string[]>(AddMission));
            CommandDatabase baseCommands = CommandManager.instance.CreateDatabse(CommandManager.DATABASE_MISSIONS);
            baseCommands.AddCommand("do", new Action<string[]>(SetMissionProgress));
            baseCommands.AddCommand("fail", new Action<string[]>(FailMission));
            baseCommands.AddCommand("unlock", new Action<string>(UnlockMission));

            //locations
            database.AddCommand("lock", new Action<string>(LockLocation));
            database.AddCommand("unlock", new Action<string>(UnlockLocation));
        }

        private static void Search(string[] data)
        {
            string textureName = data[0];
            string where;
            object found;
            var parameters = ConvertDataToParameters(data, 1);
            parameters.TryGetValue(LOCATION, out where);
            if (VariableStore.TryGetValue("found", out found) && !(bool)found) SearchedObjectManager.instance.Display(textureName, where, LOCATIONS.LocationManager.instance.currentLocation);
        }

        private static void UnlockLocation(string data)
        {
            LOCATIONS.LocationManager.instance.GetLocation(data).Unlock();
        }

        private static void LockLocation(string data)
        {
            LOCATIONS.LocationManager.instance.GetLocation(data).Lock();
        }

        private static void UnlockMission(string data)
        {
            var mission = MISSIONS.MissionManager.instance.GetMission(data);
            mission.Stat = MISSIONS.Mission.MisStat.InProcess;
            MISSIONS.MissionManager.instance.UpdateMissionStatus(data, MISSIONS.Mission.MisStat.InProcess);
            MISSIONS.MissionExpander.instance.UpdateCounter();
            DialogueSystem.instance.ShowNotification($"квест {mission.Name} разблокирован!");
        }

        private static void FailMission(string[] data)
        {
            List<string> newdata = data.ToList();
            newdata.Insert(0, ADD[0]);
            string codeword;
            var parameters = ConvertDataToParameters(newdata.ToArray());
            parameters.TryGetValue(ADD[0], out codeword);
            var mission = MISSIONS.MissionManager.instance.GetMission(codeword);
            mission.Fail();
            DialogueSystem.instance.ShowNotification($"Вы не справились! квест {mission.Name} провален... Это повлияет на будущее.");
        }

        private static void AddMission(string[] data)
        {
            List<string> newdata = data.ToList();
            newdata.Insert(0, ADD[0]);
            string codeword;
            int max;
            var parameters = ConvertDataToParameters(newdata.ToArray());
            parameters.TryGetValue(ADD[0], out codeword);
            parameters.TryGetValue(MAX[1], out max, defaultValue: 1);
            var mission = MISSIONS.MissionManager.instance.CreateMission(codeword, max);
        }

        private static void SetMissionProgress(string[] data)
        {
            List<string> newdata = data.ToList();
            newdata.Insert(0, ADD[0]);
            string codeword;
            var parameters = ConvertDataToParameters(newdata.ToArray());
            parameters.TryGetValue(ADD[0], out codeword);
            var mission = MISSIONS.MissionManager.instance.GetMission(codeword);
            mission.progressCurrent++;
        }

        private static void Notify(string[] data)
        {
            List<string> newdata = data.ToList();
            newdata.Insert(0, "-t");
            string message;
            var parameters = ConvertDataToParameters(newdata.ToArray());
            parameters.TryGetValue("-t", out message);
            message =  message.Trim('\"');
            DialogueSystem.instance.ShowNotification(message);
        }

        private static void Load(string[] data)
        {
            string filePath = string.Empty;
            bool enqueue = false;
            var parameters = ConvertDataToParameters(data);
            parameters.TryGetValue(PARAM_FILE_PATH, out filePath);
            parameters.TryGetValue(ENQUEUE, out enqueue);
            string path = FilePaths.GetPath(FilePaths.dialogue_path, filePath);
            TextAsset file = Resources.Load<TextAsset>(path);
            if (file == null)
            {
                Debug.Log($"path {path} is invalid!!!!!!!!!!!!!!!!!");
                return;
            }
            List<string> lines = FileManager.ReadTextAsset(file, includeBlankLines: true);
            Conversation newconv = new Conversation(lines);
            if (enqueue) DialogueSystem.instance.conversationManager.Enqueue(newconv);
            else DialogueSystem.instance.conversationManager.StartConverstaion(newconv);


        }

        private static IEnumerator Hide()
        {
            yield return DIALOGUE.DialogueSystem.instance.hide();
        }

        private static IEnumerator Show()
        {
            yield return DIALOGUE.DialogueSystem.instance.show();
        }

        private static IEnumerator HideBox()
        {
            yield return DIALOGUE.DialogueSystem.instance.DialogueContainer.hide();
        }

        private static IEnumerator ShowBox()
        {
            yield return DIALOGUE.DialogueSystem.instance.DialogueContainer.show();
        }

        private static IEnumerator Wait(string data)
        {
            if (float.TryParse(data, out float time))
                yield return new WaitForSeconds(time);
        }
    }
}