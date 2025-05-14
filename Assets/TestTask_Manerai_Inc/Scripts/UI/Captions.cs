using UnityEngine;

using System.Collections;
using System.Collections.Generic;

namespace YukiOno.SkillTest
{
    [RequireComponent(typeof(CanvasGroup))]
    
    [RequireComponent(typeof(TextGroup))]

    public class Captions : MonoBehaviour
    {
        private CanvasGroup canvasGroup;
        
        private TextGroup textGroup;

        private float fadeProgress;

        private IEnumerator fadeCoroutine;

        private void Awake()
        {
            textGroup = GetComponent<TextGroup>();
            
            canvasGroup = GetComponent<CanvasGroup>();

            canvasGroup.alpha = 0f;
        }

        public void SetText(string text)
        {
            float readTime = text.Length * 0.06f; // assumes reading time of [200 words per minute] and average word length of [5 characters]

            float displayTime = Mathf.Max(readTime, 3.0f);

            SetText(text, displayTime);
        }

        public void SetText(string text, float displayTime)
        {   
            if (gameObject.activeInHierarchy)
            {
                if (fadeCoroutine != null)
                {
                    StopCoroutine(fadeCoroutine);
                }

                fadeCoroutine = FadeText(text, displayTime);

                StartCoroutine(fadeCoroutine);
            }
        }

        private IEnumerator FadeText(string text, float displayTime)
        {
            while (fadeProgress > 0f)
            {
                ShowText(false);

                yield return null;
            }

            canvasGroup.alpha = 0f;

            textGroup.SetText(text);

            // =========================================================

            while (fadeProgress < 1.0f)
            {
                ShowText(true);

                yield return null;
            }

            canvasGroup.alpha = 1.0f;

            // =========================================================

            yield return new WaitForSeconds(displayTime);

            while (fadeProgress > 0f)
            {
                ShowText(false);

                yield return null;
            }

            canvasGroup.alpha = 0f;
        }

        private void ShowText(bool value)
        {
            int direction = value ? 1 : -1;

            fadeProgress += Time.deltaTime * direction / 0.25f;

            // =========================================================

            if (value && fadeProgress > 1.0f)
                fadeProgress = 1.0f;

            else if (fadeProgress < 0f)
                fadeProgress = 0f;

            // =========================================================

            canvasGroup.alpha = AnimationCurves.Sinusoidal.InOut(fadeProgress);
        }
    }
}




