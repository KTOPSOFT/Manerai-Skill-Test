using UnityEngine;

using System.Collections;
using System.Collections.Generic;

namespace YukiOno.SkillTest
{
    public class AudioList : MonoBehaviour
    {
        public float volumeScale = 1.0f;

        public List<AudioClip> audioClips = new List<AudioClip>();

        private int lastPlayed = -1;

        // =========================================================

        private float[] previousTime;

        private float[] previousVolume;
        
        // =========================================================
        //    Standard Methods
        // =========================================================

        private void Start()
        {
            lastPlayed = -1;

            // =========================================================

            // first index for the active player; the rest for distance ranges [0, 5], [5, 10], [10, 15] and [15, 20]

            previousTime = new float[5];

            previousVolume = new float[5];
        }

        public int GetLastPlayed() // called by AudioPlayer.cs
        {
            return lastPlayed;
        }

        public void SetLastPlayed(int value) // called by AudioPlayer.cs
        {
            lastPlayed = value;
        }

        public float GetPreviousTime(int index) // called by AudioPlayer.cs
        {
            return previousTime[index];
        }

        public float GetPreviousVolume(int index) // called by AudioPlayer.cs
        {
            return previousVolume[index];
        }

        public void SetPreviousTime(int index, float time) // called by AudioPlayer.cs
        {
            previousTime[index] = time;
        }

        public void SetPreviousVolume(int index, float volume) // called by AudioPlayer.cs
        {
            previousVolume[index] = volume;
        }
    }
}








