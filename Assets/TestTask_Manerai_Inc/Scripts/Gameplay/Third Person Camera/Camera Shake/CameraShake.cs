using UnityEngine;

using System.Collections;
using System.Collections.Generic;

namespace YukiOno.SkillTest
{
    public class CameraShake : MonoBehaviour
    {
        protected static List<CameraShake> cameras = new List<CameraShake>();

        public float maxDistance = 10.0f;

        private float maxDistanceSquared;

        private float magnitude;

        // =================================================

        private int cycles;

        private int cyclesCompleted;

        private float cycleLength = 0.05f;

        // =================================================

        private bool timerRunning;

        private float timeStamp;

        // =============================================
        
        private void OnEnable()
        {
            cameras.Add(this);

            maxDistanceSquared = maxDistance * maxDistance;
        }

        private void OnDisable()
        {
            cameras.Remove(this);
        }

        private void LateUpdate()
        {
            if (timerRunning)
            {
                float currentTime = Time.time;

                if (currentTime >= timeStamp + cycleLength / 2.0f)
                {
                    if (magnitude < 0f)
                    {
                        magnitude *= -0.5f;

                        cyclesCompleted ++;
                    }

                    else
                    {
                        magnitude *= -1.0f;
                    }

                    // =================================================

                    if (cyclesCompleted == cycles)
                    {
                        timerRunning = false;

                        magnitude = 0f;
                    }

                    timeStamp = Time.time;

                    transform.localPosition = Vector3.up * magnitude;
                }
            }
        }

        private void StartShake(float shakeScale, int cycles, Vector3 sourcePosition)
        {
            Vector3 offset = transform.position - sourcePosition;

            float distanceSquared = offset.sqrMagnitude;

            float newMagnitude = 0f;

            if (distanceSquared < maxDistanceSquared)
            {
                newMagnitude = 0.015f * shakeScale * (1.0f - (distanceSquared / maxDistanceSquared));
            }

            // =================================================

            if (newMagnitude >= Mathf.Abs(magnitude))
            {
                this.cycles = cycles;

                cyclesCompleted = 0;

                magnitude = newMagnitude;

                // =================================================

                transform.localPosition = Vector3.up * magnitude;

                timeStamp = Time.time;

                timerRunning = true;
            }
        }

        public static void Shake(float shakeScale, int cycles, bool isLocal, bool activePlayer, Vector3 sourcePosition)
        {
            if (shakeScale > 0 && cycles > 0)
            {
                bool validEvent = !isLocal || (isLocal && activePlayer);

                if (validEvent)
                {
                    int listCount = cameras.Count;

                    for (int i = 0; i < listCount; i ++)
                    {
                        cameras[i].StartShake(shakeScale, cycles, sourcePosition);
                    }
                }
            }
        }
    }
}














