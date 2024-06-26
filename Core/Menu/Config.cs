using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using VISUALNOVEL;

public class Config : Page
{
    public static Config instance;
    [SerializeField]
    private GameObject[] panels;
    [SerializeField]
    public UIItems UI;
    private GameObject activePanel;
    
    private VN_ConfigurationsData config => VN_ConfigurationsData.activeConfig;
    // Start is called before the first frame update
    void Start()
    {
        LoadConfig();
        for (int i = 0; i < panels.Length; i++)
        {
            panels[i].SetActive(i == 0);
        }
        activePanel = panels[0];
        SetAvailibleResolution();
    }
    private void Awake()
    {
        instance = this;
    }
    private void OnApplicationQuit()
    {
        if(VN_ConfigurationsData.activeConfig != null) VN_ConfigurationsData.activeConfig.Save();
        VN_ConfigurationsData.activeConfig = null;
    }
    public void LoadConfig()
    {
        if(File.Exists(VN_ConfigurationsData.filePath))
        {
            VN_ConfigurationsData.activeConfig = FileManager.Load<VN_ConfigurationsData>(VN_ConfigurationsData.filePath);
        }
        else
        {
            VN_ConfigurationsData.activeConfig = new VN_ConfigurationsData();
        }
        VN_ConfigurationsData.activeConfig.Load();
    }
    public void OpenPanel(string panelName)
    {
        GameObject panel = panels.First(p => p.name == panelName);
        if(panel == null)
        {
            Debug.LogWarning("panel not found");
        }
        
        if (activePanel != null && activePanel != panel) activePanel.SetActive(false);
        panel.SetActive(true);
        activePanel = panel;
        
    }

    public override void Close(bool closeAllMenus = false)
    {
        VN_ConfigurationsData.activeConfig.Save();
        VN_ConfigurationsData.activeConfig = null;
        base.Close(closeAllMenus);
    }

    public void SetAvailibleResolution()
    {
        Resolution[] resolutions = Screen.resolutions;
        List<string> options = new List<string>();
        for(int i = resolutions.Length - 1; i >= 0; i--)
        {
            options.Add($"{resolutions[i].width}x{resolutions[i].height}");
        }
        UI.resolution.ClearOptions();
        UI.resolution.AddOptions(options);
    }


    public void SetDisplayOnScreen(bool fullScreen)
    {
        Screen.fullScreen = fullScreen;
        UI.SetButtonColors(UI.fullscreen, UI.windowed, fullScreen);
    }

    public void SetDisplayResouliton()
    {
        string resolution = UI.resolution.captionText.text;
        string[] values = resolution.Split('x');
        if (int.TryParse(values[0], out int width) && int.TryParse(values[1], out int height))
        {
            Screen.SetResolution(width, height, Screen.fullScreen);
            config.display_resoultion = resolution;
        }
    }
    public void SetContinueSkippingAfterChoice(bool continueSkipping)
    {
        config.continueSkippingAfterChoice = continueSkipping;
        UI.SetButtonColors(UI.continueSkipping, UI.stopSkipping, continueSkipping);
    }
    public void SetTextArchictectSpeed()
    {
        config.dialogueTestSpeed = UI.archictectSpeed.value;
        if (DIALOGUE.DialogueSystem.instance != null) DIALOGUE.DialogueSystem.instance.architect.speed = config.dialogueTestSpeed;
    }

    public void SetMusicVolume()
    {
        config.musicVolume = UI.musicVolume.value;
        AUDIO.AudioManager.instance.SetMusicVolume(config.musicVolume, config.music_mute);
        UI.musicFill.color = config.music_mute ? UI.MusicOffColor : UI.MusicOnColor;
    }
    public void SetSoundVolume()
    {
        config.audioVolume = UI.soundVolume.value;
        AUDIO.AudioManager.instance.SetSFXVolume(config.audioVolume, config.sfx_mute);
        UI.SFXfill.color = config.sfx_mute ? UI.MusicOffColor : UI.MusicOnColor;
    }
    public void SetVoiceVolume()
    {
        config.voiceVolume = UI.voiceVolume.value;
        AUDIO.AudioManager.instance.SetVoiceVolume(config.voiceVolume, config.voice_mute);
        UI.voiceFill.color = config.voice_mute ? UI.MusicOffColor : UI.MusicOnColor;
    }

    public void SetMusicMute()
    {
        config.music_mute = !config.music_mute;
        UI.musicFill.color = config.music_mute ? UI.MusicOffColor : UI.MusicOnColor;
        UI.musicMute.sprite = config.music_mute ? UI.muted : UI.umnuted;

        AUDIO.AudioManager.instance.SetMusicVolume(config.musicVolume, config.music_mute);

    }
    public void SetSFXMute()
    {
        config.sfx_mute = !config.sfx_mute;
        UI.SFXfill.color = config.sfx_mute ? UI.MusicOffColor : UI.MusicOnColor;
        UI.sfxMute.sprite = config.sfx_mute ? UI.muted : UI.umnuted;

        AUDIO.AudioManager.instance.SetSFXVolume(config.audioVolume, config.sfx_mute);
    }
    public void SetVoiceMute()
    {
        config.voice_mute = !config.voice_mute;
        UI.voiceFill.color = config.voice_mute ? UI.MusicOffColor : UI.MusicOnColor;
        UI.voiceMute.sprite = config.voice_mute ? UI.muted : UI.umnuted;

        AUDIO.AudioManager.instance.SetVoiceVolume(config.voiceVolume, config.voice_mute);
    }
    public void SetAutoReaderSpeed()
    {
        config.autoReadTextSpeed = UI.autoReaderSpeed.value;
        if (DIALOGUE.DialogueSystem.instance != null)
        {
            DIALOGUE.AutoReader reader = DIALOGUE.DialogueSystem.instance.reader;

            if (reader != null)
            {
                reader.speed = config.autoReadTextSpeed;
            }
        }
    }
}

[System.Serializable] public class UIItems
{
    private static Color buttonSelectedColor = new Color(0.2f, 0.35f, 0.2f, 1);
    private static Color32 buttonUnSelectedColor = new Color32(155, 107, 255, 255);
    [Header("General")]
    public Button fullscreen, windowed;
    [Header("General")]
    public TMP_Dropdown resolution;
    [Header("General")]
    public Button continueSkipping;
    [Header("General")]
    public Button stopSkipping;
    [Header("General")]
    public Slider archictectSpeed, autoReaderSpeed;

    [Header("Audio")]
    public Slider musicVolume, soundVolume, voiceVolume;
    public Image musicFill, SFXfill, voiceFill;

    public Color32 MusicOnColor = new Color32(45, 70, 255, 255);
    public Color32 MusicOffColor = new Color32(140, 140, 140, 255);
    public Sprite umnuted;
    public Sprite muted;

    public Image musicMute;
    public Image voiceMute;
    public Image sfxMute;

    public void SetButtonColors(Button a, Button b, bool selectedA)
    {
        if (selectedA)
        {
            a.GetComponent<Image>().color = buttonSelectedColor;
            b.GetComponent<Image>().color = buttonUnSelectedColor;
        }

        else
        {
            a.GetComponent<Image>().color = buttonUnSelectedColor;
            b.GetComponent<Image>().color = buttonSelectedColor;
        }

    }
}
