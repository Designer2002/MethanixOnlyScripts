using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VISUALNOVEL
{
    public class SaveLoadSlot : MonoBehaviour
    {
        public GameObject root;
        public RawImage previewImage;
        public TextMeshProUGUI titleText;
        public Button deleteButton;
        public Button saveButton;
        public Button loadButton;

        [HideInInspector]
        public int fileNumber = 0;

        [HideInInspector]
        public string filePath;

        public void PopulateDetails(SaveAndLoad.MenuFunction function)
        {
            if(File.Exists(filePath))
            {
                VNGameSave file = VNGameSave.Load(filePath);
                PopulateDetailsFromFile(function, file);
            }
            else
            {
                PopulateDetailsFromFile(function, null);
            }
        }

        private void PopulateDetailsFromFile(SaveAndLoad.MenuFunction function, VNGameSave file)
        {
            if (file == null)
            {
                titleText.text = $"{fileNumber} - Ïףסעמי פאיכ";
                deleteButton.gameObject.SetActive(false);
                loadButton.gameObject.SetActive(false);

                saveButton.gameObject.SetActive(function == SaveAndLoad.MenuFunction.SAVE);

                previewImage.texture = SaveAndLoad.instance.emptyFileImage;
            }
            else
            {
                titleText.text = $"{fileNumber}.{file.timestamp}";
                deleteButton.gameObject.SetActive(true);
                loadButton.gameObject.SetActive(function == SaveAndLoad.MenuFunction.LOAD);

                saveButton.gameObject.SetActive(function == SaveAndLoad.MenuFunction.SAVE);
                byte[] imageData = System.IO.File.ReadAllBytes(file.screenshotPath);
                Texture2D screenshotPreview = new Texture2D(1, 1);
                ImageConversion.LoadImage(screenshotPreview, imageData);
                previewImage.texture = screenshotPreview;
            }
        }

        public void Delete()
        {
            File.Delete(filePath);
            PopulateDetails(SaveAndLoad.instance.menuFunction);
        }

        public void Load()
        {
            VNGameSave saveFile = VNGameSave.Load(filePath, false);
            SaveAndLoad.instance.Close(closeAllMenus: true);
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == MainMenu.MAIN_MENU_SCENE_NAME)
            {
                MainMenu.instance.LoadGame(saveFile);
            }
            else
            {
                saveFile.Activate();  
            }
        }

        public void Save()
        {
            var activeSave = VNGameSave.activeFile;
            activeSave.slotNumber = fileNumber;
            activeSave.Save();
            PopulateDetailsFromFile(SaveAndLoad.instance.menuFunction, activeSave);
        }
    }
}