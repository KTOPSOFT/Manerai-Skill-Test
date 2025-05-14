using UnityEngine;
using UnityEngine.UI;

using System.Collections;
using System.Collections.Generic;

namespace YukiOno.SkillTest
{
    public class ScreenFade : MonoBehaviour
    {
        public Image m_image;

        public float fadeTime = 0.33f;

        private float currentAlpha;

        private bool fading;

        private IEnumerator fadeCoroutine;

        public bool isFading { get { return fading; } }

        private void Awake()
        {
            currentAlpha = 0f;

            if (m_image != null)
            {
                m_image.raycastTarget = false;

                m_image.color = new Color(0f, 0f, 0f, currentAlpha);
            }
        }

        public void FadeTo(GameEvent gameEvent)
        {
            if (m_image != null)
            {
                fading = true;

                if (fadeCoroutine != null)
                {
                    StopCoroutine(fadeCoroutine);
                }

                fadeCoroutine = FadeAnimation(gameEvent);

                StartCoroutine(fadeCoroutine);
            }

            else
            {
                Debug.Log("Could not start ScreenFade.Fade() as the image property is null.");
            }
        }

        private IEnumerator FadeAnimation(GameEvent gameEvent)
        {
            float imageAlpha = currentAlpha;

            while (currentAlpha < 1.0f)
            {
                currentAlpha += (Time.unscaledDeltaTime / fadeTime);

                if (currentAlpha > 1.0f)
                {
                    currentAlpha = 1.0f;
                }

                imageAlpha = AnimationCurves.Sinusoidal.InOut(currentAlpha);

                m_image.color = new Color(0f, 0f, 0f, imageAlpha);

                yield return null;
            }

            gameEvent.Invoke();

            yield return SceneHandler.WaitForTenthSecond;

            // =========================================================

            while (currentAlpha > 0f)
            {
                currentAlpha -= (Time.unscaledDeltaTime / fadeTime);

                if (currentAlpha < 0f)
                {
                    currentAlpha = 0f;
                }

                imageAlpha = AnimationCurves.Sinusoidal.InOut(currentAlpha);

                m_image.color = new Color(0f, 0f, 0f, imageAlpha);

                yield return null;
            }

            fading = false;
        }
    }   
}




