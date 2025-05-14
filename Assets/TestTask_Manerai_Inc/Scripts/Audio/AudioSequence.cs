using UnityEngine;

using System;

using System.Collections;
using System.Collections.Generic;

namespace YukiOno.SkillTest
{
    public class AudioSequence : MonoBehaviour
    {
        public float volumeScale = 1.0f;

        public List<AudioTrack> audioTracks = new List<AudioTrack>();

        [Serializable]
        public class AudioTrack
        {
            public AudioClip audioClip;

            public float volume = 1.0f;
            public float delayTime = 0f;
        }

        public void PlayTracks(AudioSource audioSource, PlayerController controller)
        {
            PlayTracks(audioSource, controller.GetVolumeModifier());
        }
        
        public void PlayTracks(AudioSource audioSource, PlayerController controller, float volume)
        {
            float volumeModifier = controller.GetVolumeModifier() * volume;
            
            PlayTracks(audioSource, volumeModifier);
        }

        public void PlayTracks(AudioSource audioSource, float volumeModifier)
        {
            int listCount = audioTracks.Count;

            for (int i = 0; i < listCount; i ++)
            {
                AudioTrack currentTrack = audioTracks[i];

                AudioClip audioClip = currentTrack.audioClip;

                if (audioClip != null)
                {
                    float volume = currentTrack.volume * volumeScale * volumeModifier;

                    float delayTime = currentTrack.delayTime;

                    audioSource.volume = 1.0f;

                    if (delayTime <= 0f)
                    {
                        audioSource.PlayOneShot(audioClip, volume);
                    }

                    else
                    {
                        StartCoroutine(PlayTrack(audioSource, audioClip, volume, delayTime));
                    }
                }
            }
        }

        private IEnumerator PlayTrack(AudioSource audioSource, AudioClip audioClip, float volume, float delayTime)
        {
            yield return new WaitForSeconds(delayTime);

            audioSource.PlayOneShot(audioClip, volume);
        }

        private void Reset()
        {
            audioTracks.Clear();

            AudioTrack newTrack = new AudioTrack();

            audioTracks.Add(newTrack);
        }
    }
}





