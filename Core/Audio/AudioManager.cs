using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace AUDIO
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager instance { get; private set; }

        public const float TRACK_TRANSITION_SPEED = 1;

        public AudioMixerGroup musicMixer;
        public AudioMixerGroup sfxMixer;
        public AudioMixerGroup voiceMixer;
        [SerializeField]
        public AnimationCurve audioFalloffCurve;

        public const string MUSIC_VOLUME_PARAMETER_NAME = "Music";
        public const string SFX_VOLUME_PARAMETER_NAME = "SFX";
        public const string VOICE_VOLUME_PARAMETER_NAME = "Voice";

        public const float MUTE_VOLUME_PARAMETER_NAME = -80.0f;

        public static Dictionary<int, AudioChannel> channels = new Dictionary<int, AudioChannel>();


        private const string SoundName = "SFX";
        private const string NameFormat = "SFX - [{0}]";
        private Transform SoundRoot;
        // Start is called before the first frame update
        private void Awake()
        {
            if (instance == null)
            {
                transform.SetParent(null);
                DontDestroyOnLoad(gameObject);
                instance = this;
            }
            else
            {
                DestroyImmediate(gameObject);
                return;
            }
            SoundRoot = new GameObject(SoundName).transform;
            SoundRoot.SetParent(transform);

        }
        public AudioSource PlaySoundEffect(string filePath, AudioMixerGroup mixer = null, float volume = 1, float pitch = 1, bool loop = false)
        {
            AudioClip clip = Resources.Load<AudioClip>(filePath);
            if (clip == null)
            {
                Debug.Log("no audio");
                return null;
            }
            return PlaySoundEffect(clip, mixer, volume, pitch, loop);
        }

        public AudioSource PlaySoundEffect(AudioClip clip, AudioMixerGroup mixer = null, float volume = 1, float pitch = 1, bool loop = false)
        {
            AudioSource effect = new GameObject(string.Format(NameFormat, clip.name)).AddComponent<AudioSource>();
            effect.transform.SetParent(SoundRoot);
            effect.transform.position = SoundRoot.position;
            effect.clip = clip;

            if (mixer == null) mixer = sfxMixer;
            effect.outputAudioMixerGroup = mixer;
            effect.volume = volume;
            effect.spatialBlend = 0;
            effect.pitch = pitch;
            effect.loop = loop;

            effect.Play();

            if (!loop) Destroy(effect.gameObject, clip.length/pitch + 1);
            return effect;
        }

        public AudioSource PlayVoice(AudioClip clip, float volume = 1, float pitch = 1, bool loop = false) => PlaySoundEffect(clip, voiceMixer, volume, pitch, loop);

        public AudioSource PlayVoice(string path, float volume = 1, float pitch = 1, bool loop = false) => PlaySoundEffect(path, voiceMixer, volume, pitch, loop);

        public void StopSound(AudioClip clip) => StopSoundEffect(clip.name);

        public void StopAllSounds()
        {
            AudioSource[] sources = SoundRoot.GetComponentsInChildren<AudioSource>();
            foreach (var s in sources)
            {
                Destroy(s.gameObject);
            }
        }

        public void StopSoundEffect(string name)
        {
            AudioSource[] sources = SoundRoot.GetComponentsInChildren<AudioSource>();
            foreach(var s in sources)
            {
                if(s.clip.name.ToLower() == name.ToLower())
                {
                    Destroy(s.gameObject);
                    return;
                }
            }
        }

        public AudioTrack PlayTrack(string filePath, int channel = 0, bool loop = true, float startingVolume = 0f, float volumeCap = 1, float pitch = 1f)
        {
            AudioClip clip = Resources.Load<AudioClip>(filePath);
            if (clip == null) return null;
            return PlayTrack(clip, channel, loop, startingVolume, volumeCap, pitch, filePath);
        }
        public AudioTrack PlayTrack(AudioClip clip, int channel = 0, bool loop = true, float startingVolume = 0f, float volumeCap = 1, float pitch = 1f, string filePath = "")
        {
            AudioChannel audioChannel = TryGetChannel(channel, true);
            AudioTrack track = audioChannel.PlayTrack(clip, loop, startingVolume, volumeCap, pitch, filePath);
            return track;
        }

        public void StopTrack(int channel)
        {
            AudioChannel c = TryGetChannel(channel, CreateIfNotExist: false);

            if (c == null)
            {
                Debug.LogWarning("can't stop NULL");
                return;
            }
            c.StopTrack();
        }

        public AudioChannel TryGetChannel(int channelNum, bool CreateIfNotExist = false)
        {
            AudioChannel channel = null;
            if (channels.TryGetValue(channelNum, out channel)) return channel;
            else if (CreateIfNotExist)
            {
                channel = new AudioChannel(channelNum);
                channels.Add(channelNum, channel);
                return channel;
            }
            return null;
        }

        public void SetMusicVolume(float volume, bool muted)
        {
            volume = muted ? MUTE_VOLUME_PARAMETER_NAME : audioFalloffCurve.Evaluate(volume);
            musicMixer.audioMixer.SetFloat(AUDIO.AudioManager.MUSIC_VOLUME_PARAMETER_NAME, volume);
        }

        public void SetSFXVolume(float volume, bool muted)
        {
            volume = muted ? MUTE_VOLUME_PARAMETER_NAME : audioFalloffCurve.Evaluate(volume);
            sfxMixer.audioMixer.SetFloat(AUDIO.AudioManager.SFX_VOLUME_PARAMETER_NAME, volume);
        }

        public void SetVoiceVolume(float volume, bool muted)
        {
            volume = muted ? MUTE_VOLUME_PARAMETER_NAME : audioFalloffCurve.Evaluate(volume);
            voiceMixer.audioMixer.SetFloat(AUDIO.AudioManager.VOICE_VOLUME_PARAMETER_NAME, volume);
        }
    }
}