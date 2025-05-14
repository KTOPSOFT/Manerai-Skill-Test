using UnityEngine;
using UnityEngine.UI;

using System.Collections;
using System.Collections.Generic;

namespace YukiOno.SkillTest
{
    [RequireComponent(typeof(CanvasGroup))]
    public class Meter : MonoBehaviour
    {
        public CustomSlider slider;

        public Image regenBar;

        public bool flashOnFull;

        public bool hideOnFull;

        // =========================================================

        private CanvasGroup canvasGroup;

        private bool fullMeter = true;

        private bool updateSubBar;
        private bool updateAlpha;

        private bool updateTimer;

        private float timer;

        private float currentAlpha;
        private float targetAlpha;

        private float alphaVelocity;

        private float subBarValue;
        private float subBarIncrement;

        private bool flashSubBar;

        private float previousValue;

        public void Awake()
        {
            previousValue = -1.0f;

            slider.progressBars[0].value = 1.0f;

            // =========================================================

            canvasGroup = GetComponent<CanvasGroup>();

            canvasGroup.alpha = 0f;

            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            // =========================================================

            if (regenBar != null)
            {
                regenBar.gameObject.SetActive(false);

                Image image = regenBar.GetComponent<Image>();

                if (image != null)
                {
                    CustomSlider.ValidateImage(image, Image.FillMethod.Radial360, 2, false);
                }
            }
        }

        private void OnEnable()
        {
            previousValue = slider.progressBars[0].value;

            FadeIn();

            if (flashSubBar)
            {
                slider.StartFlashAnimation(1, 0.5f);

                flashSubBar = false;
            }
        }

        private void OnDisable()
        {
            subBarValue = 0f;

            slider.progressBars[1].value = subBarValue;

            updateSubBar = false;

            slider.UpdateValues();
        }

        private void Update()
        {   
            if (updateSubBar)
            {
                UpdateSubBar();
            }

            // =========================================================

            if (updateTimer)
            {
                timer += Time.deltaTime;

                if (timer >= 0.75f)
                {
                    FadeOut();
                }
            }

            // =========================================================

            if (updateAlpha)
            {
                UpdateCanvasGroup();
            }
        }

        private void UpdateSubBar()
        {
            if (updateSubBar)
            {
                var subBar = slider.progressBars[1];

                if (!subBar.inFlashAnimation)
                {
                    float currentValue = slider.progressBars[0].value;

                    if (subBarValue > currentValue)
                    {
                        subBarValue -= Time.deltaTime * subBarIncrement;

                        if (subBarValue <= currentValue)
                        {
                            subBarValue = 0f;

                            updateSubBar = false;
                        }
                    }

                    subBar.value = subBarValue;

                    slider.UpdateValues();
                }
            }
        }

        private void UpdateCanvasGroup()
        {
            currentAlpha = Mathf.SmoothDamp(currentAlpha, targetAlpha, ref alphaVelocity, 0.1f);

            float difference = Mathf.Abs(targetAlpha - currentAlpha);

            if (difference < 0.01f)
            {
                currentAlpha = targetAlpha;

                updateAlpha = false;
            }

            canvasGroup.alpha = currentAlpha;

            // =========================================================

            if (currentAlpha == 0f)
            {
                gameObject.SetActive(false);
            }
        }

        public void SetValue(float value)
        {
            bool active = gameObject.activeSelf;

            if (!active && value != 1.0f)
            {
                gameObject.SetActive(true);

                active = true;
            }

            // =========================================================

            if (active)
            {
                if (value != previousValue)
                {
                    previousValue = value;

                    slider.progressBars[0].value = value;

                    if (regenBar != null)
                    {
                        regenBar.fillAmount = value;
                    }

                    slider.UpdateValues();
                }

                // =========================================================

                if (fullMeter && value != 1.0f)
                {
                    FadeIn();
                }

                else if (!fullMeter && value == 1.0f)
                {
                    StartFadeTimer();
                }
            }
        }

        public void ResetValues()
        {
            slider.progressBars[0].value = 1.0f;
            slider.progressBars[1].value = 0f;

            if (regenBar != null)
            {
                regenBar.fillAmount = 1.0f;
            }

            slider.UpdateValues();
        }

        public void FadeIn()
        {
            timer = 0f;

            updateTimer = false;

            currentAlpha = (currentAlpha < 0.01f) ? 0.01f : currentAlpha;
            targetAlpha = 1.0f;

            updateAlpha = true;

            fullMeter = false;
        }

        public void FadeOut()
        {
            if (gameObject.activeInHierarchy)
            {
                currentAlpha = 0.99f;
                targetAlpha = 0f;

                updateAlpha = true;

                updateTimer = false;
            }
        }

        private void StartFadeTimer()
        {
            timer = 0f;

            if (hideOnFull) { updateTimer = true; }

            ShowRegenBar(false);

            if (flashOnFull) { FlashMainBar(); }

            updateSubBar = false;

            fullMeter = true;
        }

        public void ShowRegenBar(bool value) // called by PlayerController.cs
        {
            if (regenBar != null)
            {
                slider.progressBars[0].image.gameObject.SetActive(!value);

                regenBar.gameObject.SetActive(value);
            }
        }

        public void HideMeter()
        {
            timer = 0f;

            updateTimer = false;

            // =========================================================

            currentAlpha = 0f;
            targetAlpha = 0f;

            updateAlpha = false;

            // =========================================================

            if (canvasGroup != null)
            {
                canvasGroup.alpha = currentAlpha;
            }

            gameObject.SetActive(false);
        }

        private void FlashMainBar()
        {
            slider.StartFlashAnimation(0, 0.66f);
        }

        public void FlashSubBar(float reduction, float maxValue)
        {
            bool active = gameObject.activeSelf;

            if (active)
            {
                gameObject.SetActive(true);
            }

            // =========================================================

            updateSubBar = true;

            subBarValue = slider.progressBars[0].value;

            slider.progressBars[1].value = subBarValue;

            // =========================================================

            if (active)
                slider.StartFlashAnimation(1, 0.5f);

            else
                flashSubBar = true;

            // =========================================================

            float reductionPercent = reduction / maxValue;

            if (reductionPercent > 1.0f)
            {
                reductionPercent = 1.0f;
            }

            // =========================================================

            subBarIncrement = 0.66f;

            if (reductionPercent > 0.25f)
            {
                subBarIncrement *= reductionPercent / 0.25f;
            }
        }
    }
}




