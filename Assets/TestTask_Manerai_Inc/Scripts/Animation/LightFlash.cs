using UnityEngine;

using System.Collections;
using System.Collections.Generic;

namespace YukiOno.SkillTest
{
    public class LightFlash : MonoBehaviour
    {
        public float intensity = 1.0f;

        public float flashTime = 0.15f;

        private float elapsedTime;

        private Light lightSource;

        private bool inProgress;

        private void Awake()
        {
            lightSource = GetComponent<Light>();
        }

        private void OnEnable()
        {
            elapsedTime = 0f;

            lightSource.intensity = intensity;

            inProgress = true;
        }

        private void Update()
        {
            if (inProgress)
            {
                elapsedTime += Time.deltaTime;

                float ratio = elapsedTime / flashTime;

                if (ratio > 1.0f)
                {
                    ratio = 1.0f;

                    inProgress = false;
                }

                float progress = AnimationCurves.Sinusoidal.InOut(ratio);

                lightSource.intensity = intensity * (1.0f - progress);
            }
        }
    }
}




