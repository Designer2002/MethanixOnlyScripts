using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AUDIO
{
    public class AudioChannel
    {
        private const string TRACK_FORMAT = "Channel - [{0}]";
        public int channelIdx { get; private set; }
        public Transform TrackContainer { get; private set; } = null;
        public AudioTrack activeTrack { get; private set; } = null;
        private List<AudioTrack> tracks = new List<AudioTrack>();

        private Coroutine co_leveling = null;
        public bool isLevelingVolume => co_leveling != null;
        public AudioChannel(int channel)
        {
            channelIdx = channel;
            TrackContainer = new GameObject(string.Format(TRACK_FORMAT, channel)).transform;
            TrackContainer.SetParent(AudioManager.instance.transform);
        }

        public AudioTrack PlayTrack(AudioClip clip, bool loop, float volume, float volumeCap, float pitch, string filePath)
        {
            if (TryGetTrack(clip.name, out AudioTrack existingTrack))
            {
                if (!existingTrack.isPlaying) existingTrack.Play();
                SetAsActiveTrack(existingTrack);
                return existingTrack;
            }
            AudioTrack track = new AudioTrack(clip, loop, volume, volumeCap, pitch, this, AudioManager.instance.musicMixer, filePath);
            track.Play();
            SetAsActiveTrack(track);
            return track;
        }

        public bool TryGetTrack(string trackName, out AudioTrack value)
        {
            foreach(var track in tracks)
            {
                if(track.name.ToLower() == trackName.ToLower())
                {
                    value = track;
                    return true;
                }
            }
            value = null;
            return false;
        }

        private void TryStartVolumeLeveling()
        {
            if (!isLevelingVolume)
                co_leveling = AudioManager.instance.StartCoroutine(VolumeLeveling());
        }
        private IEnumerator VolumeLeveling()
        {
            while ( tracks.Count > 0 || (activeTrack != null && activeTrack.volume != activeTrack.volumeCap))
            {
                for(int i = tracks.Count - 1; i >= 0; i--)
                {
                    AudioTrack track = tracks[i];
                    float targetVol = track == activeTrack ? track.volumeCap : 0;

                    if (track == activeTrack && targetVol == track.volume) continue;
                    
                    track.volume = Mathf.MoveTowards(track.volume, targetVol, AudioManager.TRACK_TRANSITION_SPEED * Time.deltaTime);
                    if (track != activeTrack && track.volume == 0)
                    {
                        DestroyTrack(track);
                    }
                }
                yield return null;
            }
            co_leveling = null;
        }

        private void DestroyTrack(AudioTrack track)
        {
            if (tracks.Contains(track)) tracks.Remove(track);
            Object.Destroy(track.root);
        }

        private void SetAsActiveTrack(AudioTrack track)
        {
            if (!tracks.Contains(track)) tracks.Add(track);
            activeTrack = track;
            TryStartVolumeLeveling();
        }

        public void StopTrack(bool immediate = false)
        {
            if (activeTrack == null)
            {
                Debug.LogWarning("nothing to stop!");
                return;
            }
            activeTrack = null;
            if(!immediate) TryStartVolumeLeveling();
        }
    }
}