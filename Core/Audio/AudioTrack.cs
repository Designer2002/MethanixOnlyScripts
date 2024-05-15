using UnityEngine;

namespace AUDIO
{
    public class AudioTrack
    {
        public string name { get; private set; }
        public string path { get; private set; }
        private const string TRACK_NAME_FORMAT = "Track - [{0}]";
        private AudioChannel channel;
        private AudioSource source;
        public GameObject root => source.gameObject;
        public bool isPlaying => source.isPlaying;
        public bool loop => source.loop;
        public float volumeCap { get; private set; }

        public float volume { get => source.volume; set { source.volume = value; } }
        public float pitch { get => source.pitch; set { source.pitch = value; } }

        public AudioTrack(AudioClip clip, bool loop, float volume, float volumeCap, float pitch, AudioChannel channel, UnityEngine.Audio.AudioMixerGroup mixer, string filePath)
        {
            name = clip.name;
            this.channel = channel;
            this.volumeCap = volumeCap;

            source = CreateSource();
            source.clip = clip;
            source.loop = loop;
            source.volume = volume;
            source.pitch = pitch;

            source.outputAudioMixerGroup = mixer;
        }

        private AudioSource CreateSource()
        {
            GameObject go = new GameObject(string.Format(TRACK_NAME_FORMAT, name));
            go.transform.SetParent(channel.TrackContainer);
            AudioSource source = go.AddComponent<AudioSource>();
            return source;
        }

        public void Play()
        {
            source.Play();
        }

        public void Stop()
        {
            source.Stop();
        }
    }
}