using System;
using System.IO;
using System.Linq;
using UnityEngine;
public class FilePaths
{
    public static System.Random r = new System.Random();
    public static readonly string root = $"{Application.dataPath}/gameData/";

    //runtime

    public static string runtimePath
    {
        get
        {
            #if UNITY_EDITOR
                return "Assets/appdata/";
            #else
                return Application.persistentDataPath + "/appdata/";
            #endif
        }
    }

    public static readonly string game_saves = $"{runtimePath}SaveFiles/";

    //resources
    public static readonly string resources_directory = "Assets/_MAIN_/";
    public static readonly string main_path = "_MAIN_/Resources/";
    public static readonly string resources_graphics = "Graphics/";
    public static readonly string resources_graphics_bg_photo = "Graphics/Backgrounds/Photo/";
    public static readonly string resources_graphics_bg_video = "Graphics/Backgrounds/Video/";
    public static readonly string resources_graphics_transition = "Graphics/TransitionEffects/";

    public static readonly string resources_audio_locations = "Audio/Music/LocationTracks/";

    public static readonly string resources_audio = "Audio/";
    public static readonly string resources_audio_sounds = resources_audio + "Sounds/";
    public static readonly string resources_audio_music = resources_audio + "Music/";
    public static readonly string resources_audio_ambience = resources_audio + "Ambience/";
    private const string HOME_DIRECTORY_SYMBOL = "~/";

    public static readonly string dialogue_path = $"Dialogue Files/";

    public static readonly string LocationStatus = "GUI/LocationMap/Status/";

    private static string resources_cats = "GUI/Menu/Cats/";

    public static string resources_menu_gui_path = "GUI/Menu/";

    public static readonly string SearchedObjectsPaths = "GUI/SearchedObject/";
    public static string GetPath(string p, string n)
    {
        if (n.StartsWith(HOME_DIRECTORY_SYMBOL)) return n.Substring(HOME_DIRECTORY_SYMBOL.Length);
        return p + n;
    }

    public static string GetCatPath()
    {
        var path = $"{resources_cats}{r.Next(1, AmmountOFFiles($"{resources_directory}Resources/{resources_cats}") - 1)}";
       // Debug.Log(path);
        return path;
    }

    public static string character_voice(string characterName, string emotion)
    {
        System.Random r = new System.Random();
        string subpath = $"{resources_directory}Resources/Characters/{characterName.Replace(characterName[0], characterName[0].ToString().ToUpper()[0])}/Voices/{emotion.Replace(emotion[0], emotion[0].ToString().ToUpper()[0])}/";
        int v = r.Next(1, AmmountOFFiles(subpath));
        int c = subpath.IndexOf("Characters");
        //Debug.Log(subpath.Remove(0, c) + v.ToString());

        return subpath.Remove(0, c)+ v.ToString();
    }

    private static int AmmountOFFiles(string dir) => Directory.GetFiles(dir, "*", SearchOption.TopDirectoryOnly).Length/2;

    public static string GetRandomTransitionEffectPath()
    {
        var files = Directory.EnumerateFiles(resources_directory + "Resources/" + resources_graphics_transition, "*.*", SearchOption.TopDirectoryOnly)
            .Where(s => s.EndsWith(".png") || s.EndsWith(".jpg"));
        if (files.ToList().Count == 0) Debug.LogWarning("path is empty");
        return files.ToList()[r.Next(0, files.ToList().Count - 1)];
    }

}
