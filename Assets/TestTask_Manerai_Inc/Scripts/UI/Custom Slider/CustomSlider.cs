using UnityEngine;
using UnityEngine.UI;

using System;

using System.Collections;
using System.Collections.Generic;

namespace YukiOno.SkillTest
{
    public class CustomSlider : MonoBehaviour
    {
        public ProgressBar background = new ProgressBar();

        public List<ProgressBar> progressBars = new List<ProgressBar>();

        protected int listCount;

        [Serializable]
        public class ProgressBar
        {
            [Range(0, 1)]
            public float value = 1.0f;

            public Image image;

            public Image mask;

            public Color color = Color.white;

            [HideInInspector]
            public bool inFlashAnimation;

            [HideInInspector]
            public IEnumerator flashAnimation;

            public void StopFlashAnimation()
            {
                inFlashAnimation = false;

                flashAnimation = null;
            }
        }

        protected void Reset()
        {
            background.color = Color.black;

            progressBars = new List<ProgressBar>();

            progressBars.Add(new ProgressBar());
        }

        protected void OnDisable()
        {
            ResetProgressBars();
        }

        protected virtual void Awake()
        {   
            listCount = progressBars.Count;
        }

        public virtual void UpdateValues()
        {
            UpdateValues(false);
        }

        public virtual void UpdateValues(bool resetColor)
        {   
            Image image, mask;

            for (int i = 0; i < listCount; i ++)
            {
                ProgressBar progressBar = progressBars[i];

                image = progressBar.image;

                mask = progressBar.mask;

                if (image != null)
                {
                    float value = progressBar.value;

                    image.fillAmount = value;

                    // ===========================================

                    if (mask != null)
                    {
                        mask.fillAmount = 1.0f - value;

                        mask.color = Color.white;
                    }

                    // ===========================================

                    if (resetColor)
                    {
                        image.color = progressBar.color;
                    }
                }
            }

            // ===========================================

            background.value = 1.0f;

            image = background.image;

            if (image != null)
            {
                image.fillAmount = 1.0f;

                if (resetColor)
                {
                    image.color = background.color;
                }
            }
        }

        protected virtual void OnValidate() // overriden by RadialSlider.cs
        {
            listCount = progressBars.Count;

            ValidateImages(Image.FillMethod.Horizontal, 0, 1);

            UpdateValues(true);
        }

        protected void ValidateImages(Image.FillMethod fillMethod, int originA, int originB)
        {
            ValidateImage(background.image, fillMethod, originA, true);

            for (int i = 0; i < listCount; i ++)
            {
                ValidateImage(progressBars[i].image, fillMethod, originA, false);

                ValidateImage(progressBars[i].mask, fillMethod, originB, true);
            }
        }

        public static void ValidateImage(Image image, Image.FillMethod fillMethod, int fillOrigin, bool fillClockwise)
        {
            if (image != null)
            {
                image.type = Image.Type.Filled;

                image.fillMethod = fillMethod;
                image.fillOrigin = fillOrigin;

                image.fillClockwise = fillClockwise;
            }
        }

        public void StartFlashAnimation(int index, float lerpTime)
        {
            var progressBar = progressBars[index];

            IEnumerator animation = progressBar.flashAnimation;

            if (animation != null)
            {
                StopCoroutine(animation);

                progressBar.StopFlashAnimation();
            }

            // =========================================================

            if (gameObject.activeInHierarchy)
            {
                animation = FlashAnimation(progressBar, lerpTime);

                progressBar.flashAnimation = animation;

                StartCoroutine(animation);
            }
        }

        protected IEnumerator FlashAnimation(ProgressBar progressBar, float lerpTime)
        {
            Image image = progressBar.image;

            Color endColor = progressBar.color;

            // =========================================================

            progressBar.inFlashAnimation = true;

            image.color = Color.white;

            float timer = 0f;

            while (timer < lerpTime)
            {
                timer += Time.deltaTime;

                if (timer > lerpTime)
                    timer = lerpTime;

                float progress = AnimationCurves.Sinusoidal.InOut(timer / lerpTime);

                image.color = Color.Lerp(Color.white, endColor, progress);

                yield return null;
            }

            image.color = endColor;

            progressBar.inFlashAnimation = false;

            progressBar.StopFlashAnimation();
        }

        protected void ResetProgressBars()
        {
            int listCount = progressBars.Count;

            for (int i = 0; i < listCount; i ++)
            {
                ProgressBar progressBar = progressBars[i];

                if (progressBar.inFlashAnimation)
                {
                    IEnumerator animation = progressBar.flashAnimation;

                    if (animation != null)
                    {
                        StopCoroutine(animation);

                        progressBar.StopFlashAnimation();
                    }

                    progressBar.StopFlashAnimation();

                    progressBar.inFlashAnimation = false;
                }

                // =========================================================

                Image image = progressBar.image;

                image.color = progressBar.color;
            }
        }

        public static float GetImageWidth(ProgressBar progressBar)
        {
            float imageWidth = 0f;

            Image image = progressBar.image;

            if (image != null)
            {
                RectTransform rectTransform = image.GetComponent<RectTransform>();

                imageWidth = rectTransform.rect.width;
            }

            return imageWidth;
        }
    }
}









