using UnityEngine;
using UnityEngine.UI;

using System.Collections;
using System.Collections.Generic;

namespace YukiOno.SkillTest
{
    public class ColorLerp : MonoBehaviour
    {
        public float timeInterval = 0.75f;

        public Color colorA = Color.white;
        public Color colorB = Color.white;

        private Image image;

        private float timer = 0f;
        private float progress = 0f;

        private float currentLerp = 1.0f;
        private float targetLerp = 1.0f;

        private void Awake()
        {
            image = GetComponent<Image>();
        }

        private void OnEnable()
        {
            timer = 0f;
            progress = 0f;

            currentLerp = 0f;
            targetLerp = 0f;
        }

        private void Update()
        {
            timer += Time.unscaledDeltaTime;

            if (timer >= timeInterval)
            {
                targetLerp = 1.0f - targetLerp;

                timer -= timeInterval;
            }

            // ===============================

            progress = AnimationCurves.Sinusoidal.InOut(timer / timeInterval);

            if (targetLerp == 0f)
            {
                currentLerp = progress;
            }

            else
            {
                currentLerp = 1.0f - progress;
            }

            // ===============================

            image.color = Color.Lerp(colorA, colorB, currentLerp);
        }
    }
}


