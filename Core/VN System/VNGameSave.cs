using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VISUALNOVEL
{
    [System.Serializable]
    public class VNGameSave
    {
        public static VNGameSave activeFile = null;
        public const string FILE_TYPE = ".amogus";
        public const string SCREENSHOT_FILE_TYPE = ".jpg";
        private const float DOWNSCALE = 1;
        public const bool encryptFiles = true;
        public bool newGame = true;
        public VN_VaribleData[] variables;

        public SavingsData activeState;
        public static bool reload = false;
        public string timestamp;

        private const string BACK_PANEL = "background";
        private const string BASE_LOCATION_CODEWORD = "base";


        public string filePath => $"{FilePaths.game_saves}{slotNumber}{FILE_TYPE}";
        public string screenshotPath => $"{FilePaths.game_saves}{slotNumber}{SCREENSHOT_FILE_TYPE}";

        public int slotNumber = 1;

        public string[] activeConversations;

        public void Save()
        {
            newGame = false;
            activeState = SavingsData.Capture();
            activeConversations = GetConversationData();
            variables = GetVariableData();
            timestamp = System.DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss");
            ScreenShotMaster.CaptureScreenshot(VNMenuManager.instance.mainCamera, Screen.width, Screen.height, DOWNSCALE, screenshotPath);
            string saveJSON = JsonUtility.ToJson(this);
            FileManager.Save(filePath, saveJSON, encryptFiles);
        }

        public static VNGameSave Load(string filePath, bool activateOnLoad = false)
        {
            VNGameSave save = FileManager.Load<VNGameSave>(filePath, encryptFiles);
            activeFile = save;
            if (activateOnLoad) save.Activate();
            return save;
        }

        public void Activate()
        {
            
            
            CheckAndUpdateConversationData();
            if (activeState != null) activeState.Load();
            GRAPHICS.GraphicPanelManager.instance.StartCoroutine(TryFixGraphics());
            DIALOGUE.DialogueSystem.instance.architect.Clear();
            DIALOGUE.DialogueSystem.instance.conversationManager.architect.tmpro.ForceMeshUpdate();

            SetVariableData();
            SetConversationData();

            DIALOGUE.DialogueSystem.instance.DialogueContinuationPrompt.Hide();

            Debug.Log(DIALOGUE.DialogueSystem.instance.conversationManager.conversation.GetProgress());
        }

        public string[] GetConversationData()
        {
            List<string> returnData = new List<string>();
            var conversations = DIALOGUE.DialogueSystem.instance.conversationManager.GetConversationQueue();

            for (int i = 0; i < conversations.Length; i++)
            {
                var conversation = conversations[i];
                string data = "";

                if (conversation.file != string.Empty)
                {
                    var compressedData = new VNConversationDataCompressed();
                    compressedData.fileName = conversation.file;
                    //compressedData.progress = LOCATIONS.LocationManager.instance.goal != null ? LOCATIONS.LocationManager.instance.RollBackProgress : conversation.GetProgress();
                    compressedData.progress = conversation.GetProgress();
                    compressedData.startIndex = conversation.fileStartIndex;
                    compressedData.endIndex = conversation.fileEndIndex;
                    data = JsonUtility.ToJson(compressedData);
                }
                else
                {
                    var fullData = new VNConversationData();
                    fullData.conversation = conversation.GetLines();
                    //fullData.progress = LOCATIONS.LocationManager.instance.goal != null ? LOCATIONS.LocationManager.instance.RollBackProgress : conversation.GetProgress();
                    fullData.progress = conversation.GetProgress();
                    data = JsonUtility.ToJson(fullData);
                }
                returnData.Add(data);
            }
            return returnData.ToArray();
        }

        public void SetConversationData()
        {

            for (int i = 0; i < activeConversations.Length; i++)
            {
                try
                {
                    string data = activeConversations[i];
                    DIALOGUE.Conversation conversation = null;
                    var fullData = JsonUtility.FromJson<VNConversationData>(data);
                    if (fullData.conversation.Count != 0)
                    {
                        conversation = new DIALOGUE.Conversation(fullData.conversation, fullData.progress);
                    }
                    else
                    {
                        var compressedData = JsonUtility.FromJson<VNConversationDataCompressed>(data);
                        if (compressedData != null && compressedData.fileName != string.Empty)
                        {
                            TextAsset file = Resources.Load<TextAsset>(compressedData.fileName);

                            int count = compressedData.endIndex - compressedData.startIndex;


                            List<string> lines = FileManager.ReadTextAsset(file)
                                .Skip(compressedData.startIndex)
                                .Take(count + 1)
                                .ToList();


                            conversation = new DIALOGUE.Conversation(lines, compressedData.progress, compressedData.fileName, compressedData.startIndex, compressedData.endIndex);
                        }
                        else
                        {
                            Debug.LogWarning("unknow format! Failed to RELOAD!");
                        }
                    }

                    if (conversation != null && conversation.GetLines().Count > 0)
                    {
                        if (i == 0)
                        {
                            DIALOGUE.DialogueSystem.instance.architect.Clear();
                            DIALOGUE.DialogueSystem.instance.conversationManager.StartConverstaion(conversation);
                        }
                        else
                        {
                            DIALOGUE.DialogueSystem.instance.conversationManager.Enqueue(conversation);
                        }
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError(e.Message);
                    continue;
                }
            }

            //if (LOCATIONS.LocationManager.instance.goal != null)
            //{
            //    //Debug.Log("KILLED GOAL");
            //    //LOCATIONS.LocationManager.instance.KillGoal(rollback: true);
            //    //if (LOCATIONS.LocationManager.instance.OldLocation != null) VNMenuManager.instance.StartCoroutine(LOCATIONS.LocationManager.instance.Teleport(LOCATIONS.LocationManager.instance.OldLocation, immediate: true, is_teleporting_by_button: false));
            //}
            LOCATIONS.LocationExpander.instance.Expander.interactable = false;
        }

        public VN_VaribleData[] GetVariableData()
        {
            List<VN_VaribleData> returnData = new List<VN_VaribleData>();
            foreach (var database in VariableStore.databases.Values)
            {
                foreach (var variable in database.variables)
                {
                    VN_VaribleData variableData = new VN_VaribleData();
                    variableData.name = $"{database.name}.{variable.Key}";
                    string val = $"{variable.Value.Get().ToString()}";
                    variableData.value = val;
                    variableData.type = val == string.Empty ? "System.String" : variable.Value.Get().GetType().ToString();
                    returnData.Add(variableData);
                }
            }

            if(returnData.Count != 0)
            {
                if (!returnData.Where(i => i.name == BACK_PANEL).Any())
                {
                    if (VariableStore.TryGetValue(BACK_PANEL, out var back))
                    {
                        returnData.Add(new VN_VaribleData()
                        {
                            name = BACK_PANEL,
                            value = back.ToString(),
                            type = "System.String"
                        });
                    }
                }
                if (!returnData.Where(i => i.name == "location").Any())
                {
                    if (VariableStore.TryGetValue("location", out var l))
                    {
                        returnData.Add(new VN_VaribleData()
                        {
                            name = "location",
                            value = l.ToString(),
                            type = "System.String"
                        });
                    }
                }
            }
            return returnData.ToArray();
        }

        public void SetVariableData()
        {
            foreach (var variable in variables)
            {
                string val = variable.value;
                switch (variable.type)
                {
                    case "System.Boolean":
                        if (bool.TryParse(val, out bool b_v))
                        {
                            VariableStore.TrySetValue(variable.name, b_v);
                            continue;
                        }
                        break;
                    case "System.Int32":
                        if (int.TryParse(val, out int i_v))
                        {
                            VariableStore.TrySetValue(variable.name, i_v, create: true);
                            continue;
                        }
                        break;
                    case "System.Single":
                        if (float.TryParse(val, out float f_v))
                        {
                            VariableStore.TrySetValue(variable.name, f_v);
                            continue;
                        }
                        break;
                    case "System.String":
                        if (variable.name.Contains("location") && val == BASE_LOCATION_CODEWORD) continue;
                       // if (variable.name == "backgroundPanel" && val == string.Empty) continue;
                        VariableStore.TrySetValue(variable.name, val, change:false);
                        continue;
                }
                
                Debug.LogError($"couldn't not interpret variable type {variable.name} = {variable.type}");
            }
            VariableStore.TryGetValue("location", out object location);
            VNMenuManager.instance.StartCoroutine(LOCATIONS.LocationManager.instance.Teleport(location.ToString(), immediate: true, is_teleporting_by_button: false));
        }

        public void CheckAndUpdateConversationData()
        {
            if (reload)
            {
                List<string> returnData = new List<string>();
                var conversations = DIALOGUE.DialogueSystem.instance.conversationManager.GetConversationQueue();
                for (int i = 0; i < conversations.Length; i++)
                {
                    var savedData = JsonUtility.FromJson<VNConversationData>(activeConversations[i]);
                    var conversation = conversations[i];
                    string data = "";

                    if (conversation.file != string.Empty)
                    {
                        var compressedData = new VNConversationDataCompressed();
                        compressedData.fileName = conversation.file;
                        compressedData.progress = savedData.progress;
                        compressedData.startIndex = conversation.fileStartIndex;
                        compressedData.endIndex = conversation.fileEndIndex;
                        data = JsonUtility.ToJson(compressedData);
                    }
                    else
                    {
                        var fullData = new VNConversationData();
                        fullData.conversation = conversation.GetLines();
                        fullData.progress = savedData.progress;
                        data = JsonUtility.ToJson(fullData);
                    }
                    returnData.Add(data);
                }
                if(returnData.Count != 0) activeConversations = returnData.ToArray();
                variables = GetVariableData();
                string saveJSON = JsonUtility.ToJson(this);
                FileManager.Save(filePath, saveJSON, encryptFiles);
                reload = false; // Reset the reload flag after saving


                
            }

        }

        public IEnumerator TryFixGraphics()
        {
            VariableStore.TryGetValue("backgroundPanel", out var g);
            string graphicname = g.ToString();
            var panel = GRAPHICS.GraphicPanelManager.instance.GetPanel(BACK_PANEL);
            if (graphicname != panel.layers[panel.layers.Count - 1].currentGraphic.graphicName)
            {
                UnityEngine.Object graphic = null;
                var pathToGaphic = FilePaths.GetPath(FilePaths.resources_graphics_bg_photo, graphicname);
                graphic = Resources.Load<Texture>(pathToGaphic);
                if (graphic == null)
                {
                    pathToGaphic = FilePaths.GetPath(FilePaths.resources_graphics_bg_video, graphicname);
                    graphic = Resources.Load<UnityEngine.Video.VideoClip>(pathToGaphic);
                }
                if (graphic == null)
                {
                    Debug.LogWarning("NULL GRAPHIC");
                    yield break;
                }
                GRAPHICS.GraphicLayer gl = panel.GetLayer(panel.layers.Count - 1);
                if (graphic is Texture) yield return gl.SetTexture(graphic as Texture, path: pathToGaphic, immediate: true);
                if (graphic is UnityEngine.Video.VideoClip) yield return gl.SetVideo(graphic as UnityEngine.Video.VideoClip, path: pathToGaphic,immediate: true);

            }
            else
            {
                yield break;
            }
        }
    }
}