using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VISUALNOVEL;

public class Rules : Page
{
    [SerializeField]
    private RawImage CatImage;
    // Start is called before the first frame update
    void Start()
    {
        CatImage.texture = Resources.Load<Texture>(FilePaths.GetCatPath());
    }

    public void Cat()
    {
        CatImage.texture = Resources.Load<Texture>(FilePaths.GetCatPath());
        AUDIO.AudioManager.instance.PlayVoice(Resources.Load<AudioClip>($"{FilePaths.resources_audio_sounds}mefochka"),volume: 2);
    }
}
