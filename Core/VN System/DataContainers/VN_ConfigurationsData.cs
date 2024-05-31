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
    public float musicVolume = 0.5f;
    public float audioVolume = 0.5f;
    public float voiceVolume = 0.5f;
    public bool music_mute = false;
    public bool sfx_mute = false;
    public bool voice_mute = false;


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
        Ui.musicVolume.value = musicVolume;
        Ui.soundVolume.value = audioVolume;
        Ui.voiceVolume.value = voiceVolume;
        Ui.musicMute.sprite = music_mute ? Ui.muted : Ui.umnuted;
        Ui.voiceMute.sprite = voice_mute ? Ui.muted : Ui.umnuted;
        Ui.sfxMute.sprite = sfx_mute ? Ui.muted : Ui.umnuted;
    }

    public void Save()
    {
        FileManager.Save(filePath, JsonUtility.ToJson(this));
        VN_ConfigurationsData.activeConfig = null;
    }

    
}
