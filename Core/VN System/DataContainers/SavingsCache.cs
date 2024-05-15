using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VISUALNOVEL
{
    public class SavingsCache
    {
        public static Dictionary<string, (object asset, int stateIndex)> loadedAssets = new Dictionary<string, (object asset, int stateIndex)>();

        public static T TryLoadObject<T>(string key)
        {
            object resource = null;

            if (loadedAssets.ContainsKey(key))
                resource = loadedAssets[key].asset;
            else
            {
                resource = Resources.Load(key);
                if (resource != null) loadedAssets[key] = (resource, 0);
            }
            if (resource != null)
            {
                if (resource is T)
                    return (T)resource;
                else Debug.Log("wrong format!");
            }
            return default;
        }

        public static AudioClip LoadAudio(string key) => TryLoadObject<AudioClip>(key);
        public static Texture2D LoadImage(string key) => TryLoadObject<Texture2D>(key);
        public static UnityEngine.Video.VideoClip LoadVideo(string key) => TryLoadObject<UnityEngine.Video.VideoClip>(key);

    }
}