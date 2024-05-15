using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace COMMANDS
{
    public class CMD_Database_Extension_Audio : CMD_DatabseExtensions
    {
        private static string FILE_PATH = "-path";
        private static string[] VOLUME = new[] { "-v", "-volume", "-vol" };
        private static string[] PITCH = new[] { "-p", "--pitch" };
        private static string[] LOOP = new[] { "-l", "-loop" };
        private static string[] MUSIC = new[] { "-m", "-music" };
        private static string[] SOUND = new[] { "-s", "-sound" };
        private static string[] AMBIENCE = new[] { "-a", "-ambience" };

        new public static void Extend(CommandDatabase database)
        {
            database.AddCommand("play", new Action<string[]>(Play));
            database.AddCommand("stop", new Action<string>(Stop));
        }

        private static void PlaySFX(string[] data)
        {
            var newdata = data.ToList();
            newdata.Insert(1, FILE_PATH);
            string soundName;
            float volume, pitch;
            bool loop;


            var parameters = ConvertDataToParameters(newdata.ToArray());
            parameters.TryGetValue(FILE_PATH, out soundName);
            AudioClip sound = Resources.Load<AudioClip>(FilePaths.resources_audio_sounds + soundName);
            Debug.Log(FilePaths.resources_audio_sounds + soundName);
            if (sound == null) return;
            parameters.TryGetValue(LOOP, out loop, defaultValue: false);
            parameters.TryGetValue(PITCH, out pitch, defaultValue: 1f);
            parameters.TryGetValue(VOLUME, out volume, defaultValue: 1f);
            AUDIO.AudioManager.instance.PlaySoundEffect(sound, volume: volume, pitch: pitch, loop: loop);
        }

        private static void PlayMusicOrAmbience(string[] data, int channel)
        {
            var newdata = data.ToList();
            newdata.Insert(1, FILE_PATH);
            string soundName;
            float volume, pitch;
            bool loop;


            var parameters = ConvertDataToParameters(newdata.ToArray());
            parameters.TryGetValue(FILE_PATH, out soundName);
            AudioClip sound = channel == 0 ? Resources.Load<AudioClip>(FilePaths.resources_audio_music + soundName) : Resources.Load<AudioClip>(FilePaths.resources_audio_ambience + soundName);
            //Debug.Log(FilePaths.resources_audio_sounds + soundName);
            if (sound == null) return;
            parameters.TryGetValue(LOOP, out loop, defaultValue: true);
            parameters.TryGetValue(PITCH, out pitch, defaultValue: 1f);
            parameters.TryGetValue(VOLUME, out volume, defaultValue: 1f);
            AUDIO.AudioManager.instance.PlayTrack(sound, channel: channel, startingVolume: volume, pitch: pitch, loop: loop);
        }

        private static void StopSFX()
        {
            AUDIO.AudioManager.instance.StopAllSounds();
        }

        private static void StopMusic()
        {
            AUDIO.AudioManager.instance.StopTrack(0);
        }

        private static void StopAmbience()
        {
            AUDIO.AudioManager.instance.StopTrack(1);
            
        }

        private static void Play(string[] data)
        {
            string mixerGroup = data[0];
            switch(mixerGroup)
            {
                case "-s":
                case "-sound":
                    PlaySFX(data);
                    break;
                case "-a":
                case "-ambience":
                    PlayMusicOrAmbience(data, 1);
                    break;
                case "-m":
                case "-music":
                    PlayMusicOrAmbience(data, 0);
                    break;
            }
        }

        private static void Stop(string data)
        {
            string mixerGroup = data;
            switch (mixerGroup)
            {
                case "-s":
                case "-sound":
                    StopSFX();
                    break;
                case "-a":
                case "-ambience":
                    StopAmbience();
                    break;
                case "-m":
                case "-music":
                    StopMusic();
                    break;
            }
        }
    }
}