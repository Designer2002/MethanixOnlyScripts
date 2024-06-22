using System.Collections.Generic;
using UnityEngine;

namespace VISUALNOVEL
{
    [System.Serializable]
    public class AudioData
    {
        public int channel = 0;
        public string trackName;
        public string trackPath;
        public float trackVolume;
        public float trackPitch;
        public bool loop;

        public AudioData(AUDIO.AudioChannel channel)
        {
            this.channel = channel.channelIdx;
            if (channel.activeTrack == null) return;
            var track = channel.activeTrack;
            trackName = track.name;
            trackPath = channel.channelIdx == 0 ? $"{FilePaths.resources_audio_locations}{trackName}" : $"{FilePaths.resources_audio_ambience}{trackName}";
            trackVolume = track.volume;
            trackPitch = track.pitch;
            loop = track.loop;
        }

        public static List<AudioData> Capture()
        {
            List<AudioData> audioChanels = new List<AudioData>();
            foreach (var channel in AUDIO.AudioManager.channels)
            {
                if (channel.Value.activeTrack == null) continue;
                AudioData data = new AudioData(channel.Value);
                audioChanels.Add(data);
            }
            return audioChanels;
        }

        public static void Apply(List<AudioData> data)
        {
            List<int> cache = new List<int>();
            foreach (var channelData in data)
            {
                AUDIO.AudioChannel channel = AUDIO.AudioManager.instance.TryGetChannel(channelData.channel, CreateIfNotExist: true);
                if (channel.activeTrack == null || channel.activeTrack.name != channelData.trackName)
                {
                    AudioClip clip = SavingsCache.LoadAudio(channelData.trackPath);
                    if (clip != null)
                    {
                        if(channel.activeTrack != null) channel.StopTrack(immediate: true);
                        channel.PlayTrack(clip, channelData.loop, channelData.trackVolume, channelData.trackVolume, channelData.trackPitch, channelData.trackPath);
                    }
                }
                cache.Add(channelData.channel);
            }

            foreach(var channel in AUDIO.AudioManager.channels)
            {
                if (!cache.Contains(channel.Key))
                    channel.Value.StopTrack(true);
            }
        }
    }
}