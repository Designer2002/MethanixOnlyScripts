using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VISUALNOVEL;

public class MainMenu : MonoBehaviour
{
    public const string MAIN_MENU_SCENE_NAME = "Main Menu";
    public AudioClip menuMusic;
    public CanvasGroup mainPanel;
    private DIALOGUE.CanvasGroupController mainCG;
    public static MainMenu instance { get; private set; }
    // Start is called before the first frame update
    void Start()
    {
        mainCG = new DIALOGUE.CanvasGroupController(this, mainPanel);
        AUDIO.AudioManager.instance.PlayTrack(menuMusic, startingVolume: 1);
    }
    private void Awake()
    {
        instance = this;
    }
    public void StartNewGame()
    {
        VNGameSave activeFile = new VNGameSave();
        VNGameSave.activeFile = activeFile;
        
        StartCoroutine(StartingGame());
    }

    private IEnumerator StartingGame()
    {
        mainCG.Hide();
        AUDIO.AudioManager.instance.StopTrack(0);
        while(mainCG.is_Visible)
        {
            yield return null;
        }
        UnityEngine.SceneManagement.SceneManager.LoadScene("Visual Novel");;
    }

    public void LoadGame(VNGameSave file)
    {
        VNGameSave.activeFile = file;
        StartCoroutine(StartingGame());
    }
}
