using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VISUALNOVEL
{
    public class VisualNovelManager : MonoBehaviour
    {
        public static VisualNovelManager instance { get; private set; }
        private void Awake()
        {
            instance = this;
            VN_Database_LinkSetUp linkSetup = GetComponent<VN_Database_LinkSetUp>();
            linkSetup.SetupExternalLinks();

            VNGameSave.activeFile = new VNGameSave();
        }
        public void LoadFile(string filePath)
        {
            List<string> lines = new List<string>();
            TextAsset file = Resources.Load<TextAsset>(filePath);
            try
            {
                lines = FileManager.ReadTextAsset(file);
            }
            catch
            {
                Debug.Log($"dialogue file at path {file} doesnt exist!");
                return;
            }
            DIALOGUE.DialogueSystem.instance.Say(lines, filePath);
        }
    }
}