using UnityEngine;
using UnityEngine.Audio;

using System.Collections;
using System.Collections.Generic;

namespace YukiOno.SkillTest
{
    public class AudioFade : MonoBehaviour
    {
        public AudioSource audioSourceA;
        public AudioSource audioSourceB;

        public string parameterA = "soundtrackVolumeA";
        public string parameterB = "soundtrackVolumeB";

        public AudioMixer audioMixer;

        public float fadeDuration = 2.0f;

        private float currentFadeDuration;

        private int activeSource;

        private float currentValue;

        private AudioSource[] audioSources;

        private AudioClip nextInQueue;

        private IEnumerator currentCoroutine;

        private void Start() // AudioMixer.SetFloat() doesn't work on Awake()
        {
            InitializeAudioSources();

            // ============================================

            if (audioMixer != null)
            {
                audioMixer.SetFloat(parameterA, 0f);
                audioMixer.SetFloat(parameterB, -80.0f);

                audioSources[0].gameObject.SetActive(true);
                audioSources[1].gameObject.SetActive(false);
            }

            else
            {
                audioSources[0].gameObject.SetActive(false);
                audioSources[1].gameObject.SetActive(false);
            }
        }

        private void InitializeAudioSources()
        {
            audioSources = new AudioSource[2];

            audioSources[0] = audioSourceA;
            audioSources[1] = audioSourceB;

            // ============================================

            SetAudioSourceValues(audioSources[0]);
            SetAudioSourceValues(audioSources[1]);
        }

        private void SetAudioSourceValues(AudioSource audioSource)
        {
            audioSource.playOnAwake = true;
            audioSource.loop = true;

            audioSource.spatialBlend = 0f;
            audioSource.reverbZoneMix = 0f;
        }

        public void OverrideClip(AudioClip newClip)
        {
            if (audioMixer != null)
            {
                AudioClip activeClip = audioSources[activeSource].clip;

                bool fadeInProgress = currentValue != activeSource;

                if (fadeInProgress || newClip != activeClip)
                {
                    if (currentCoroutine != null)
                        StopCoroutine(currentCoroutine);

                    // ============================================

                    audioSources[0].gameObject.SetActive(false);
                    audioSources[1].gameObject.SetActive(false);

                    // ============================================

                    activeSource = 0;
                    currentValue = 0f;

                    currentFadeDuration = fadeDuration;

                    nextInQueue = null;

                    // ============================================

                    audioSources[0].clip = newClip;

                    audioSources[0].gameObject.SetActive(true);
                }
            }
        }

        public void FadeClip(AudioClip newClip)
        {
            if (audioMixer != null)
            {
                AudioClip activeClip = audioSources[activeSource].clip;

                int targetSource = 1 - activeSource;

                bool fadeInProgress = currentValue != activeSource;

                if (fadeInProgress)
                {
                    if (newClip != activeClip)
                    {
                        currentFadeDuration = 1.0f;

                        nextInQueue = newClip;
                    }

                    else
                    {
                        currentFadeDuration = fadeDuration;

                        nextInQueue = null;

                        activeSource = targetSource;

                        FadeClip();
                    }
                }

                else
                {
                    if (newClip != activeClip)
                    {
                        currentFadeDuration = fadeDuration;

                        SetNewClip(targetSource, newClip);

                        FadeClip();
                    }
                }
            }
        }

        public void StopClip()
        {
            FadeClip(null);
        }

        private void FadeClip()
        {
            if (currentCoroutine != null)
            {
                StopCoroutine(currentCoroutine);

                currentCoroutine = null;
            }

            audioSources[activeSource].gameObject.SetActive(true);

            // ============================================

            if (gameObject.activeInHierarchy)
            {
                currentCoroutine = IE_FadeClip();

                StartCoroutine(currentCoroutine);
            }
        }

        private void SetNewClip(int targetSource, AudioClip newClip)
        {
            if (targetSource == 0)
                audioSources[0].clip = newClip;

            else
                audioSources[1].clip = newClip;
        }

        private IEnumerator IE_FadeClip()
        {   
            int targetSource = 1 - activeSource;

            audioSources[targetSource].gameObject.SetActive(true);

            // ============================================

            int sign = 1;

            if (targetSource == 0)
                sign = -1;

            // ============================================

            while (currentValue != targetSource)
            {
                currentValue += Time.unscaledDeltaTime / currentFadeDuration * sign;

                if (sign > 0)
                {
                    if (currentValue > targetSource)
                        currentValue = targetSource;
                }

                else
                {
                    if (currentValue < targetSource)
                        currentValue = targetSource;
                }

                SetVolume(audioSources[0], "soundtrackVolumeA", 1.0f - currentValue);
                SetVolume(audioSources[1], "soundtrackVolumeB", currentValue);

                yield return null;
            }

            activeSource = targetSource;

            // ============================================

            if (nextInQueue != null)
            {
                AudioClip activeClip = audioSources[activeSource].clip;

                if (nextInQueue != activeClip)
                {
                    SetNewClip(1 - activeSource, nextInQueue);

                    currentFadeDuration = fadeDuration;

                    nextInQueue = null;

                    FadeClip();
                }

                else
                {
                    currentFadeDuration = fadeDuration;

                    nextInQueue = null;
                }
            }
        }

        private void SetVolume(AudioSource audioSource, string audioMixerGroup, float value)
        {
            if (value == 0f)
            {
                audioMixer.SetFloat(audioMixerGroup, -80.0f);

                audioSource.gameObject.SetActive(false);
            }

            else
            {
                audioMixer.SetFloat(audioMixerGroup, Mathf.Log10(value) * 20.0f);
            }
        }
    }
}




