using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class VN_ConfigurationsData
{
    public static VN_ConfigurationsData activeConfig;
    public static string filePath => $"{FilePaths.root}vnconfig.cfg";


    //general
    public bool display_full_screen = true;
    public string display_resoultion = "1920x1280";
    public bool continueSkippingAfterChoice = false;
    public float dialogueTestSpeed = 1f;
    public float autoReadTextSpeed = 1f;

    //audio
    public float musicVolume = 1f;
    public float audioVolume = 1f;
    public float voiceVolume = 1f;
    public bool music_mute = false;

    public void Load()
    {
        var Ui = Config.instance.UI;
        Screen.fullScreen = display_full_screen;
        Config.instance.SetDisplayOnScreen(display_full_screen);
        Ui.SetButtonColors(Ui.fullscreen, Ui.windowed, display_full_screen);

        int resIdx = 0;
        for (int i = 0; i < Ui.resolution.options.Count; i++)
        {
            string resolution = Ui.resolution.options[i].text;
            if(resolution == display_resoultion)
            {
                resIdx = i;
                break;
            }
        }
        Ui.resolution.SetValueWithoutNotify(resIdx);
        Ui.resolution.value = resIdx;
        Ui.autoReaderSpeed.value = autoReadTextSpeed;
        Ui.SetButtonColors(Ui.continueSkipping, Ui.stopSkipping, continueSkippingAfterChoice);
        Ui.archictectSpeed.value = dialogueTestSpeed;
    }

    public void Save()
    {
        FileManager.Save(filePath, JsonUtility.ToJson(this));
        VN_ConfigurationsData.activeConfig = null;
    }

    
}
