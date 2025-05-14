using UnityEngine;

using System.Collections;
using System.Collections.Generic;

namespace YukiOno.SkillTest
{
    [RequireComponent(typeof(AudioSource))]
    public class AudioPlayer : MonoBehaviour
    {
        private AudioSource m_audioSource;

        public AudioSource audioSource { get { return m_audioSource; } }

        private Transform audioListener;

        private AudioList localList;

        private bool localListAvailable;

        private void Awake()
        {
            Transform elements = GameObject.FindWithTag("GameplayElements").transform;

            audioListener = Camera.main.transform;

            // =====================================================================

            m_audioSource = GetComponent<AudioSource>();
        }

        private static int RandomClip(int listCount, int previousClip)
        {
            int randomClip = (int) Random.Range(0, listCount);

            if (randomClip == previousClip)
                randomClip = (randomClip + 1) % listCount;

            return randomClip;
        }

        // =========================================================
        //    Local List Methods
        // =========================================================

        public void PlayFromLocalList()
        {
            PlayFromLocalList(1.0f, false);
        }

        public void PlayFromLocalList(float volume, bool playOneShot)
        {
            CheckForLocalList();

            if (localListAvailable)
            {
                int listCount = localList.audioClips.Count;

                int index = RandomClip(listCount, localList.GetLastPlayed());

                PlayFromAudioList(localList, index, volume, playOneShot);
            }
        }

        private void CheckForLocalList()
        {
            if (!localListAvailable)
            {
                localList = GetComponent<AudioList>();

                localListAvailable = (localList != null);
            }
        }

        // =========================================================
        //    Audio List Group Methods
        // =========================================================

        public void PlayFromAudioListGroup(AudioListGroup audioListGroup, PlayerController controller, float volumeScale, bool playOneShot)
        {
            List<AudioList> audioLists = audioListGroup.audioLists;

            int listCount = audioLists.Count;

            for (int i = 0; i < listCount; i ++)
            {
                AudioList audioList = audioLists[i];

                PlayFromAudioList(audioList, controller, volumeScale, playOneShot);
            }
        }

        // =========================================================
        //    Player Controller Methods
        // =========================================================

        public void PlayFromAudioList(AudioList audioList, PlayerController controller, bool playOneShot)
        {
            PlayFromAudioList(audioList, controller, 1.0f, playOneShot);
        }

        public void PlayFromAudioList(AudioList audioList, PlayerController controller, float volumeScale, bool playOneShot)
        {
            if (audioList != null)
            {
                int listCount = audioList.audioClips.Count;

                if (listCount > 0)
                {
                    int index = RandomClip(listCount, audioList.GetLastPlayed());

                    float volume = controller.GetVolumeModifier() * volumeScale;

                    bool activePlayer = controller.GetActivePlayer();

                    RequestPlayerAudio(audioList, index, volume, activePlayer, playOneShot);
                }
            }
        }

        public void PlayFromAudioList(AudioList audioList, int index, PlayerController controller, bool playOneShot)
        {
            PlayFromAudioList(audioList, index, controller, 1.0f, playOneShot);
        }

        public void PlayFromAudioList(AudioList audioList, int index, PlayerController controller, float volumeScale, bool playOneShot)
        {
            if (audioList != null)
            {
                int listCount = audioList.audioClips.Count;

                if (index >= 0 && index < listCount)
                {
                    float volume = controller.GetVolumeModifier() * volumeScale;

                    bool activePlayer = controller.GetActivePlayer();

                    RequestPlayerAudio(audioList, index, volume, activePlayer, playOneShot);
                }
            }
        }

        // =========================================================
        //    Audio List Methods
        // =========================================================

        public void PlayFromAudioList(AudioList audioList, int index, bool playOneShot)
        {
            if (audioList != null)
            {
                int listCount = audioList.audioClips.Count;

                if (index >= 0 && index < listCount)
                {
                    PlayFromAudioList(audioList, index, 1.0f, playOneShot);
                }
            }
        }

        private void PlayFromAudioList(AudioList audioList, int index, float volume, bool playOneShot)
        {
            AudioClip audioClip = audioList.audioClips[index];

            if (audioClip != null)
            {
                // m_audioSource.pitch = audioList.pitchOverride;

                float finalVolume = volume * audioList.volumeScale;

                PlayClip(audioClip, finalVolume, playOneShot);

                audioList.SetLastPlayed(index);
            }
        }

        private void PlayClip(AudioClip audioClip, float volume, bool playOneShot)
        {
            m_audioSource.clip = audioClip;

            if (playOneShot)
            {
                m_audioSource.volume = 1.0f;

                m_audioSource.PlayOneShot(audioClip, volume);
            }

            else
            {
                m_audioSource.volume = volume;

                m_audioSource.Play();
            }
        }

        // =========================================================
        //    Request Methods
        // =========================================================

        private void RequestPlayerAudio(AudioList audioList, int index, float volume, bool activePlayer, bool playOneShot)
        {       
            if (activePlayer)
            {
                RequestPlayback(0, audioList, index, volume, playOneShot);
            }

            else
            {
                Vector3 offset = transform.position - audioListener.position;

                float sqrDistance = offset.sqrMagnitude;

                if (sqrDistance < 25.0f)
                {    
                    RequestPlayback(1, audioList, index, volume, playOneShot);
                }

                else if (sqrDistance < 100.0f)
                {    
                    RequestPlayback(2, audioList, index, volume, playOneShot);
                }

                else if (sqrDistance < 225.0f)
                {    
                    RequestPlayback(3, audioList, index, volume, playOneShot);
                }

                else if (sqrDistance < 400.0f)
                {    
                    RequestPlayback(4, audioList, index, volume, playOneShot);
                }
            }
        }

        private void RequestPlayback(int index, AudioList audioList, int clipIndex, float volume, bool playOneShot)
        {
            float currentTime = Time.unscaledTime;

            bool playAudio = volume > audioList.GetPreviousVolume(index);

            if (!playAudio)
            {
                float previousTime = audioList.GetPreviousTime(index);

                if (index == 1) // index 1 is closest to player range, so we should also try to cull sounds from this distance
                {
                    float activePlayerTime = audioList.GetPreviousTime(0);

                    bool validTime = currentTime >= previousTime + 0.10f && currentTime >= activePlayerTime + 0.10f;

                    playAudio = validTime;
                }

                else if (currentTime >= previousTime + 0.10f)
                {
                    playAudio = true;
                }
            }

            // =============================================================

            if (playAudio)
            {
                PlayFromAudioList(audioList, clipIndex, volume, playOneShot);

                audioList.SetPreviousTime(index, currentTime);

                audioList.SetPreviousVolume(index, volume);
            }
        }

        // =========================================================
        //    Pitch Methods
        // =========================================================

        public void ChangePitch(int semitones)
        {
            m_audioSource.pitch = Mathf.Pow(1.05946f, semitones);
        }

        public void SetPitch(float value)
        {
            m_audioSource.pitch = value;
        }
    }
}





