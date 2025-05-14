using UnityEngine;

using System.Collections;
using System.Collections.Generic;

using TMPro;

namespace YukiOno.SkillTest
{
    [RequireComponent(typeof(CanvasGroup))]

    [RequireComponent(typeof(SmoothTransform))]

    public class Message : MonoBehaviour
    {
        public TextMeshProUGUI text;

        private MessageList messageList;

        private Transform container;

        private CanvasGroup containerCanvas;

        private float timer = 0f;

        private bool runTimer;

        private bool entering;
        private bool exiting;

        // ======================================================

        private float messageDuration;

        private float entryTime;
        private float exitTime;

        private Vector3 entryPosition;
        private Vector3 exitPosition;

        // ======================================================

        private CanvasGroup canvasGroup;

        private SmoothTransform smoothTransform;

        // =========================================================
        //    Standard Methods
        // =========================================================

        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();

            smoothTransform = GetComponent<SmoothTransform>();

            // ======================================================

            container = transform.GetChild(0);

            containerCanvas = container.GetComponent<CanvasGroup>();
        }

        private void OnEnable()
        {
            timer = 0f;

            runTimer = false;

            entering = true;
            exiting = false;

            // ======================================================

            canvasGroup.alpha = 1.0f;

            smoothTransform.targetPosition = Vector3.zero;

            transform.localPosition = Vector3.zero;

            transform.localScale = Vector3.one;

            // ======================================================

            containerCanvas.alpha = 0f;

            container.localPosition = entryPosition;
        }

        private void Update()
        {
            if (runTimer)
            {
                timer += Time.deltaTime;

                // ======================================================

                if (entering)
                {
                    float progress = GetProgress(timer, entryTime);

                    container.localPosition = Vector3.Lerp(entryPosition, Vector3.zero, progress);

                    containerCanvas.alpha = progress;
                }

                // ======================================================

                if (exiting)
                {
                    float progress = GetProgress(timer, exitTime);
                    
                    containerCanvas.alpha = (1.0f - progress);

                    container.localPosition = Vector3.Lerp(Vector3.zero, exitPosition, progress);

                    // ======================================================

                    if (progress >= 1.0f)
                    {
                        if (messageList != null)
                        {
                            messageList.RemoveFromList(this);
                        }

                        gameObject.SetActive(false);
                    }
                }

                else if (timer >= messageDuration)
                {
                    Exit();
                }
            }
        }

        public void SetText(string text)
        {
            if (this.text != null)
            {
                this.text.SetText(text);
            }
        }

        public void SetValues(float[] values, Vector3 entryPosition, Vector3 exitPosition)
        {
            messageDuration = values[0];

            entryTime = values[1];
            exitTime = values[2];

            this.entryPosition = entryPosition;
            this.exitPosition = exitPosition;
        }

        public void StartTimer()
        {   
            runTimer = true;
        }

        public void Exit()
        {
            timer = exiting ? timer : 0f;

            exiting = true;
        }

        public void TranslateTowards(Vector3 targetPosition, float smoothTime)
        {
            smoothTransform.TranslateTowards(targetPosition, smoothTime);
        }

        public void SetMessageList(MessageList messageList)
        {
            this.messageList = messageList;
        }

        private static float GetProgress(float currentTime, float targetTime)
        {
            float ratio = currentTime / targetTime;

            if (ratio >= 1.0f)
            {
                ratio = 1.0f;
            }

            float progress = AnimationCurves.Sinusoidal.InOut(ratio);

            return progress;
        }
    }
}









